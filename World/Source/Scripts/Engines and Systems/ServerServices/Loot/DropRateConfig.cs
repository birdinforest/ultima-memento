using System;
using System.Collections.Generic;

namespace Server.Engines.ServerServices.Loot
{
	public sealed class DropAmount
	{
		public int Base { get; private set; }
		public int[] FameTiers { get; private set; }
		public double LuckyExtraChancePercent { get; private set; }
		public int RandomBonusMax { get; private set; }

		public DropAmount(int baseAmount, int[] fameTiers, double luckyExtraChancePercent, int randomBonusMax)
		{
			Base = baseAmount;
			FameTiers = fameTiers ?? new int[0];
			LuckyExtraChancePercent = luckyExtraChancePercent;
			RandomBonusMax = randomBonusMax;
		}

		public static DropAmount FromJson(Dictionary<string, object> d, DropAmount fallback)
		{
			if (d == null)
				return fallback;

			return new DropAmount(
				JsonHelper.GetInt(d, "base", fallback.Base),
				JsonHelper.GetIntArray(d, "fame_tiers", fallback.FameTiers),
				JsonHelper.GetDouble(d, "lucky_extra_chance_percent", fallback.LuckyExtraChancePercent),
				JsonHelper.GetInt(d, "random_bonus_max", fallback.RandomBonusMax));
		}

		public string Signature
		{
			get { return string.Format("{0}|{1}|{2}|{3}", Base, string.Join(",", Array.ConvertAll(FameTiers, t => t.ToString())), LuckyExtraChancePercent, RandomBonusMax); }
		}
	}

	/// <summary>
	/// One configurable drop roll.
	/// chance = base + floor((fame - min_fame) / fame_per_bonus_percent) [capped] + lucky bonus, times the global multiplier, capped at max_chance_percent.
	/// </summary>
	public sealed class DropRule
	{
		public string Id { get; private set; }
		public bool Enabled { get; private set; }
		public string Season { get; private set; }
		public string Description { get; private set; }
		public double BaseChancePercent { get; private set; }
		public int MinFame { get; private set; }
		public int FamePerBonusPercent { get; private set; }
		public double MaxFameBonusPercent { get; private set; }
		public double LuckyBonusPercent { get; private set; }
		public bool RequiresLuckyKiller { get; private set; }
		public double MaxChancePercent { get; private set; }
		public bool UseGlobalMultiplier { get; private set; }
		public DropAmount Amount { get; private set; }

		public DropRule(string id, string description, double baseChance, string season = null, int minFame = 0, int famePerBonus = 0,
			double maxFameBonus = -1, double luckyBonus = 0, bool requiresLucky = false, DropAmount amount = null)
		{
			Id = id;
			Description = description;
			Enabled = true;
			Season = season;
			BaseChancePercent = baseChance;
			MinFame = minFame;
			FamePerBonusPercent = famePerBonus;
			MaxFameBonusPercent = maxFameBonus;
			LuckyBonusPercent = luckyBonus;
			RequiresLuckyKiller = requiresLucky;
			MaxChancePercent = 100;
			UseGlobalMultiplier = true;
			Amount = amount ?? new DropAmount(1, null, 0, 0);
		}

		private DropRule()
		{
		}

		public static DropRule FromJson(string id, Dictionary<string, object> d, DropRule fallback)
		{
			if (fallback == null)
				fallback = new DropRule(id, null, 0);

			string season = JsonHelper.GetString(d, "season", fallback.Season);

			return new DropRule
			{
				Id = id,
				Enabled = JsonHelper.GetBool(d, "enabled", fallback.Enabled),
				Season = string.IsNullOrEmpty(season) ? null : season,
				Description = JsonHelper.GetString(d, "description", fallback.Description),
				BaseChancePercent = JsonHelper.GetDouble(d, "base_chance_percent", fallback.BaseChancePercent),
				MinFame = JsonHelper.GetInt(d, "min_fame", fallback.MinFame),
				FamePerBonusPercent = JsonHelper.GetInt(d, "fame_per_bonus_percent", fallback.FamePerBonusPercent),
				MaxFameBonusPercent = JsonHelper.GetDouble(d, "max_fame_bonus_percent", fallback.MaxFameBonusPercent),
				LuckyBonusPercent = JsonHelper.GetDouble(d, "lucky_bonus_percent", fallback.LuckyBonusPercent),
				RequiresLuckyKiller = JsonHelper.GetBool(d, "requires_lucky_killer", fallback.RequiresLuckyKiller),
				MaxChancePercent = JsonHelper.GetDouble(d, "max_chance_percent", fallback.MaxChancePercent),
				UseGlobalMultiplier = JsonHelper.GetBool(d, "use_global_multiplier", fallback.UseGlobalMultiplier),
				Amount = DropAmount.FromJson(JsonHelper.GetObject(d, "amount"), fallback.Amount),
			};
		}

		/// <summary>Chance in percent for a given creature fame; <paramref name="lucky"/> adds the lucky bonus.</summary>
		public double GetChance(int fame, bool lucky, double globalMultiplier)
		{
			if (!Enabled || fame < MinFame || (RequiresLuckyKiller && !lucky))
				return 0;

			double fameBonus = 0;
			if (FamePerBonusPercent > 0)
			{
				fameBonus = Math.Floor((double)(fame - MinFame) / FamePerBonusPercent);
				if (MaxFameBonusPercent >= 0)
					fameBonus = Math.Min(fameBonus, MaxFameBonusPercent);
			}

			double chance = BaseChancePercent + fameBonus + (lucky ? LuckyBonusPercent : 0);
			if (UseGlobalMultiplier)
				chance *= globalMultiplier;

			return Math.Max(0, Math.Min(MaxChancePercent, chance));
		}

		public string Signature
		{
			get
			{
				return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}",
					Enabled, Season, BaseChancePercent, MinFame, FamePerBonusPercent, MaxFameBonusPercent,
					LuckyBonusPercent, RequiresLuckyKiller, MaxChancePercent, UseGlobalMultiplier, Amount.Signature);
			}
		}

		public List<string> Validate()
		{
			var errors = new List<string>();
			string p = "rules." + Id + ".";

			if (BaseChancePercent < 0 || BaseChancePercent > 100)
				errors.Add(p + "base_chance_percent must be 0-100.");
			if (MaxChancePercent < 0 || MaxChancePercent > 100)
				errors.Add(p + "max_chance_percent must be 0-100.");
			if (FamePerBonusPercent < 0)
				errors.Add(p + "fame_per_bonus_percent must be >= 0.");
			if (LuckyBonusPercent < 0 || LuckyBonusPercent > 100)
				errors.Add(p + "lucky_bonus_percent must be 0-100.");
			if (Amount.Base < 0 || Amount.Base > 1000)
				errors.Add(p + "amount.base must be 0-1000.");
			if (Amount.RandomBonusMax < 0 || Amount.RandomBonusMax > 1000)
				errors.Add(p + "amount.random_bonus_max must be 0-1000.");
			if (Amount.LuckyExtraChancePercent < 0 || Amount.LuckyExtraChancePercent > 100)
				errors.Add(p + "amount.lucky_extra_chance_percent must be 0-100.");

			return errors;
		}
	}

	public sealed class DropRateConfig
	{
		public bool Enabled { get; private set; }
		public ServiceLogLevel LogLevel { get; private set; }
		public double GlobalMultiplier { get; private set; }
		public Dictionary<string, DropRule> Rules { get; private set; }

		/// <summary>Built-in rules; values reproduce the original hard-coded drop logic.</summary>
		public static Dictionary<string, DropRule> BuiltInRules()
		{
			var rules = new List<DropRule>
			{
				new DropRule("christmas.winter_spirit_mote", "Winter Spirit Mote from any eligible kill during the christmas season.",
					5, season: "christmas", famePerBonus: 2000, maxFameBonus: 15, luckyBonus: 5,
					amount: new DropAmount(1, new[] { 5000, 15000 }, 50, 0)),
				new DropRule("christmas.veil_torn_bonus", "Extra motes on a Veil-Torn Cu Sidhe corpse during the christmas season.",
					50, season: "christmas", amount: new DropAmount(5, null, 0, 7)),
				new DropRule("christmas.veil_torn_pack", "Motes packed on a Veil-Torn Cu Sidhe when it spawns.",
					35, amount: new DropAmount(3, null, 0, 5)),
				new DropRule("champion_skull", "Champion Skull from high-fame kills outside champion spawns.",
					10, minFame: 20000, famePerBonus: 2000),
				new DropRule("lucky_trinket", "Trophy trinket (eye, skull) from certain monsters for a lucky killer.",
					5, requiresLucky: true),
				new DropRule("lucky_mask", "Lich mask from liches for a lucky killer.",
					5, requiresLucky: true),
			};

			var result = new Dictionary<string, DropRule>(StringComparer.OrdinalIgnoreCase);
			foreach (DropRule rule in rules)
				result[rule.Id] = rule;

			return result;
		}

		public static DropRateConfig FromJson(Dictionary<string, object> d)
		{
			var config = new DropRateConfig
			{
				Enabled = JsonHelper.GetBool(d, "enabled", false),
				LogLevel = ServiceLog.ParseLevel(JsonHelper.GetString(d, "log_level", null), ServiceLogLevel.Info),
				GlobalMultiplier = JsonHelper.GetDouble(d, "global_multiplier", 1.0),
				Rules = BuiltInRules(),
			};

			Dictionary<string, object> rules = JsonHelper.GetObject(d, "rules");
			if (rules != null)
			{
				foreach (KeyValuePair<string, object> entry in rules)
				{
					if (entry.Key.StartsWith("_"))
						continue;

					DropRule fallback;
					config.Rules.TryGetValue(entry.Key, out fallback);
					config.Rules[entry.Key] = DropRule.FromJson(entry.Key, JsonHelper.GetObject(rules, entry.Key), fallback);
				}
			}

			return config;
		}

		public static DropRateConfig Defaults()
		{
			return FromJson(new Dictionary<string, object>());
		}

		public List<string> Validate()
		{
			var errors = new List<string>();

			if (GlobalMultiplier < 0 || GlobalMultiplier > 100)
				errors.Add("global_multiplier must be 0-100.");

			foreach (DropRule rule in Rules.Values)
				errors.AddRange(rule.Validate());

			return errors;
		}
	}
}
