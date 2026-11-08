using System;
using System.Collections.Generic;
using Server.Engines.ServerServices.Time;
using Server.Misc;

namespace Server.Engines.ServerServices.Loot
{
	/// <summary>
	/// Configurable drop chances from Data/System/CFG/Services/drop-rates.json.
	/// With "enabled": false the built-in rules (the original hard-coded rates) are used.
	/// </summary>
	public static class DropRateSystem
	{
		public const string LogSystem = "DropRates";

		private static readonly DropRateConfig m_Defaults = DropRateConfig.Defaults();
		private static readonly JsonConfigFile<DropRateConfig> m_File;
		private static readonly HashSet<string> m_UnknownLogged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static volatile Dictionary<string, object> m_Snapshot = new Dictionary<string, object>();

		static DropRateSystem()
		{
			m_File = ConfigService.Register("drop-rates", "drop-rates.json", DropRateConfig.FromJson, DropRateConfig.Defaults, c => c.Validate());
			ServiceLog.SetLevel(LogSystem, m_File.Current.LogLevel);
			m_File.Changed += OnConfigChanged;
		}

		public static void Initialize()
		{
			WorldTime.SeasonChanged += (season, active) => RefreshSnapshot();
			WarnUnknownSeasons(Effective);
			RefreshSnapshot();

			ServiceLog.Info(LogSystem, "Started: enabled={0}, multiplier={1}, {2} rules.", m_File.Current.Enabled, Effective.GlobalMultiplier, Effective.Rules.Count);
		}

		public static DropRateConfig Effective
		{
			get
			{
				DropRateConfig config = m_File.Current;
				return config.Enabled ? config : m_Defaults;
			}
		}

		/// <summary>Thread-safe view of the rules for the web API, rebuilt on reload and season changes.</summary>
		public static Dictionary<string, object> Snapshot { get { return m_Snapshot; } }

		public static DropRule GetRule(string ruleId)
		{
			DropRule rule;
			if (Effective.Rules.TryGetValue(ruleId, out rule))
				return rule;

			lock (m_UnknownLogged)
			{
				if (m_UnknownLogged.Add(ruleId))
					ServiceLog.Warn(LogSystem, "Unknown drop rule '{0}'; treating as never dropping.", ruleId);
			}

			return null;
		}

		public static bool IsRuleActive(DropRule rule)
		{
			return rule != null && rule.Enabled && (rule.Season == null || WorldTime.IsSeasonActive(rule.Season));
		}

		/// <summary>Chance in percent, or 0 when the rule is disabled or out of season.</summary>
		public static double GetChance(string ruleId, int fame, bool lucky)
		{
			DropRule rule = GetRule(ruleId);
			return IsRuleActive(rule) ? rule.GetChance(fame, lucky, Effective.GlobalMultiplier) : 0;
		}

		public static bool Roll(string ruleId, int fame, int killerLuck)
		{
			int amount;
			return Roll(ruleId, fame, killerLuck, out amount);
		}

		/// <summary>
		/// Rolls a drop. Luck is rolled separately for the chance bonus and the amount bonus,
		/// matching the original per-check GetPlayerInfo.LuckyKiller calls.
		/// </summary>
		public static bool Roll(string ruleId, int fame, int killerLuck, out int amount)
		{
			amount = 0;
			DropRule rule = GetRule(ruleId);

			if (!IsRuleActive(rule))
				return false;

			bool lucky = (rule.LuckyBonusPercent > 0 || rule.RequiresLuckyKiller) && GetPlayerInfo.LuckyKiller(killerLuck);
			double chance = rule.GetChance(fame, lucky, Effective.GlobalMultiplier);

			if (chance <= 0 || Utility.RandomDouble() * 100 >= chance)
				return false;

			amount = RollAmount(rule.Amount, fame, killerLuck);
			return amount > 0;
		}

		private static int RollAmount(DropAmount spec, int fame, int killerLuck)
		{
			int amount = spec.Base;

			foreach (int tier in spec.FameTiers)
			{
				if (fame >= tier)
					amount++;
			}

			if (spec.LuckyExtraChancePercent > 0 && GetPlayerInfo.LuckyKiller(killerLuck) && Utility.RandomDouble() * 100 < spec.LuckyExtraChancePercent)
				amount++;

			if (spec.RandomBonusMax > 0)
				amount += Utility.RandomMinMax(0, spec.RandomBonusMax);

			return amount;
		}

		private static void OnConfigChanged(DropRateConfig previousFile, DropRateConfig nextFile)
		{
			ServiceLog.SetLevel(LogSystem, nextFile.LogLevel);

			DropRateConfig previous = previousFile.Enabled ? previousFile : m_Defaults;
			DropRateConfig next = nextFile.Enabled ? nextFile : m_Defaults;

			if (previousFile.Enabled != nextFile.Enabled)
				ServiceLog.Info(LogSystem, "Drop-rate overrides {0}.", nextFile.Enabled ? "ENABLED" : "DISABLED (built-in rates)");

			if (Math.Abs(previous.GlobalMultiplier - next.GlobalMultiplier) > 0.0001)
				ServiceLog.Info(LogSystem, "global_multiplier: {0} -> {1}", previous.GlobalMultiplier, next.GlobalMultiplier);

			foreach (DropRule rule in next.Rules.Values)
			{
				DropRule old;
				if (!previous.Rules.TryGetValue(rule.Id, out old))
					ServiceLog.Info(LogSystem, "Rule added: {0} ({1})", rule.Id, Describe(rule));
				else if (old.Signature != rule.Signature)
					ServiceLog.Info(LogSystem, "Rule changed: {0} ({1}; was {2})", rule.Id, Describe(rule), Describe(old));
			}

			foreach (DropRule rule in previous.Rules.Values)
			{
				if (!next.Rules.ContainsKey(rule.Id))
					ServiceLog.Info(LogSystem, "Rule removed: {0}", rule.Id);
			}

			lock (m_UnknownLogged)
				m_UnknownLogged.Clear();

			WarnUnknownSeasons(next);
			RefreshSnapshot();
		}

		private static string Describe(DropRule rule)
		{
			return string.Format("enabled={0}, base={1}%, season={2}", rule.Enabled, rule.BaseChancePercent, rule.Season ?? "any");
		}

		private static void WarnUnknownSeasons(DropRateConfig config)
		{
			foreach (DropRule rule in config.Rules.Values)
			{
				if (rule.Season != null && WorldTime.Effective.FindSeason(rule.Season) == null)
					ServiceLog.Warn(LogSystem, "Rule {0} uses season '{1}', which is not defined in world-time.json; it will never drop.", rule.Id, rule.Season);
			}
		}

		private static void RefreshSnapshot()
		{
			DropRateConfig config = Effective;
			var rules = new List<Dictionary<string, object>>();

			foreach (DropRule rule in config.Rules.Values)
			{
				bool active = IsRuleActive(rule);
				rules.Add(new Dictionary<string, object>
				{
					{ "id", rule.Id },
					{ "description", rule.Description },
					{ "enabled", rule.Enabled },
					{ "season", rule.Season },
					{ "active", active },
					{ "base_chance_percent", rule.BaseChancePercent },
					{ "min_fame", rule.MinFame },
					{ "fame_per_bonus_percent", rule.FamePerBonusPercent },
					{ "max_fame_bonus_percent", rule.MaxFameBonusPercent },
					{ "lucky_bonus_percent", rule.LuckyBonusPercent },
					{ "requires_lucky_killer", rule.RequiresLuckyKiller },
					{ "max_chance_percent", rule.MaxChancePercent },
					{ "effective_chance_percent", new Dictionary<string, object>
						{
							{ "at_min_fame", active ? rule.GetChance(rule.MinFame, false, config.GlobalMultiplier) : 0 },
							{ "at_min_fame_lucky", active ? rule.GetChance(rule.MinFame, true, config.GlobalMultiplier) : 0 },
							{ "at_fame_20000_lucky", active ? rule.GetChance(Math.Max(rule.MinFame, 20000), true, config.GlobalMultiplier) : 0 },
						}
					},
					{ "amount", new Dictionary<string, object>
						{
							{ "base", rule.Amount.Base },
							{ "fame_tiers", rule.Amount.FameTiers },
							{ "lucky_extra_chance_percent", rule.Amount.LuckyExtraChancePercent },
							{ "random_bonus_max", rule.Amount.RandomBonusMax },
						}
					},
				});
			}

			m_Snapshot = new Dictionary<string, object>
			{
				{ "enabled", m_File.Current.Enabled },
				{ "global_multiplier", config.GlobalMultiplier },
				{ "rules", rules },
				{ "generated_at", DateTime.Now.ToString("o") },
			};
		}
	}
}
