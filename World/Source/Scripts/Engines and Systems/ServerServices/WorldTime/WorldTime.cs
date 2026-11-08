using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Engines.ServerServices.WebApi;
using Server.Items;

namespace Server.Engines.ServerServices.Time
{
	/// <summary>
	/// World calendar and in-game clock, configured by Data/System/CFG/Services/world-time.json.
	/// With "enabled": false the server uses real time and the original clock/season rules.
	/// </summary>
	public static class WorldTime
	{
		public const string LogSystem = "WorldTime";

		private static readonly DateTime WorldStart = new DateTime(1997, 9, 1);
		private static readonly WorldTimeConfig m_Defaults = WorldTimeConfig.Defaults();
		private static readonly JsonConfigFile<WorldTimeConfig> m_File;

		private static readonly object m_ClockLock = new object();
		private static DateTime m_AnchorReal;
		private static double m_AnchorMinutes;
		private static double m_SecondsPerMinute;
		private static bool m_Frozen;

		private static HashSet<string> m_ActiveSeasons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static Timer m_SeasonTimer;
		private static Timer m_SnapshotTimer;

		private static volatile Dictionary<string, object> m_TimeSnapshot = new Dictionary<string, object>();
		private static volatile List<Dictionary<string, object>> m_SeasonsSnapshot = new List<Dictionary<string, object>>();

		/// <summary>Raised on the game thread when a season starts (true) or ends (false).</summary>
		public static event Action<string, bool> SeasonChanged;

		static WorldTime()
		{
			m_File = ConfigService.Register("world-time", "world-time.json", WorldTimeConfig.FromJson, WorldTimeConfig.Defaults, c => c.Validate());
			ServiceLog.SetLevel(LogSystem, m_File.Current.LogLevel);
			ResetClockAnchor(Effective);
			m_ActiveSeasons = ComputeActiveSeasons();
			m_File.Changed += OnConfigChanged;
		}

		public static void Initialize()
		{
			WorldTimeConfig config = Effective;
			StartTimers(config);
			RefreshSnapshots();

			CommandSystem.Register("WorldTime", AccessLevel.GameMaster, new CommandEventHandler(WorldTime_OnCommand));

			WebApiHost.Register("GET", WebApiHost.ApiRoot + "/world-time", request => TimeSnapshot,
				"Real time, effective world calendar time, in-game clock and moon phase.");
			WebApiHost.Register("GET", WebApiHost.ApiRoot + "/seasons", request => new Dictionary<string, object> { { "seasons", SeasonsSnapshot } },
				"All seasons with active state and next start/end dates.");

			ServiceLog.Info(LogSystem, "Started: enabled={0}, mode={1}, world date {2:yyyy-MM-dd HH:mm}, active seasons [{3}].",
				m_File.Current.Enabled, config.Mode, Now, string.Join(", ", ActiveSeasons.ToArray()));
		}

		/// <summary>The config in effect: the file when enabled, otherwise built-in defaults.</summary>
		public static WorldTimeConfig Effective
		{
			get
			{
				WorldTimeConfig config = m_File.Current;
				return config.Enabled ? config : m_Defaults;
			}
		}

		public static bool Enabled { get { return m_File.Current.Enabled; } }

		public static DateTime Now
		{
			get
			{
				WorldTimeConfig config = Effective;
				switch (config.Mode)
				{
					case CalendarMode.Offset: return DateTime.Now.AddDays(config.OffsetDays).AddHours(config.OffsetHours);
					case CalendarMode.Fixed: return config.FixedDate ?? DateTime.Now;
					default: return DateTime.Now;
				}
			}
		}

		public static int Month { get { return Now.Month; } }

		public static bool IsSeasonActive(string id)
		{
			SeasonDefinition season = Effective.FindSeason(id);
			return season != null && season.Enabled && season.Contains(Now);
		}

		public static List<string> ActiveSeasons
		{
			get { return new List<string>(ComputeActiveSeasons()); }
		}

		public static Dictionary<string, object> TimeSnapshot { get { return m_TimeSnapshot; } }
		public static List<Dictionary<string, object>> SeasonsSnapshot { get { return m_SeasonsSnapshot; } }

		/// <summary>Total in-game minutes since the world epoch, before per-map offsets.</summary>
		public static double GetGameTotalMinutes()
		{
			lock (m_ClockLock)
			{
				if (m_Frozen)
					return m_AnchorMinutes;

				return m_AnchorMinutes + (DateTime.Now - m_AnchorReal).TotalSeconds / m_SecondsPerMinute;
			}
		}

		public static void ApplyHourOverride(ref int hours, ref int minutes)
		{
			int fixedHour = Effective.FixedHour;
			if (fixedHour >= 0)
			{
				hours = fixedHour;
				minutes = 0;
			}
		}

		private static void ResetClockAnchor(WorldTimeConfig config)
		{
			lock (m_ClockLock)
			{
				m_AnchorReal = DateTime.Now;
				m_SecondsPerMinute = config.SecondsPerUOMinute;
				m_AnchorMinutes = (m_AnchorReal - WorldStart).TotalSeconds / m_SecondsPerMinute;
				m_Frozen = config.Frozen;
			}
		}

		/// <summary>Keeps the current in-game minute and only changes rate / frozen state from now on.</summary>
		private static void ReanchorClock(WorldTimeConfig config)
		{
			double current = GetGameTotalMinutes();
			lock (m_ClockLock)
			{
				m_AnchorReal = DateTime.Now;
				m_AnchorMinutes = current;
				m_SecondsPerMinute = config.SecondsPerUOMinute;
				m_Frozen = config.Frozen;
			}
		}

		private static void OnConfigChanged(WorldTimeConfig previousFile, WorldTimeConfig nextFile)
		{
			ServiceLog.SetLevel(LogSystem, nextFile.LogLevel);

			WorldTimeConfig previous = previousFile.Enabled ? previousFile : m_Defaults;
			WorldTimeConfig next = nextFile.Enabled ? nextFile : m_Defaults;

			if (previousFile.Enabled != nextFile.Enabled)
			{
				ServiceLog.Info(LogSystem, "World time {0}; in-game clock reset to its canonical time.", nextFile.Enabled ? "ENABLED" : "DISABLED (real time, original rules)");
				ResetClockAnchor(next);
			}
			else if (Math.Abs(previous.SecondsPerUOMinute - next.SecondsPerUOMinute) > 0.0001 || previous.Frozen != next.Frozen)
			{
				ServiceLog.Info(LogSystem, "Game clock: seconds_per_uo_minute {0} -> {1}, frozen {2} -> {3} (re-anchored, no time jump).",
					previous.SecondsPerUOMinute, next.SecondsPerUOMinute, previous.Frozen, next.Frozen);
				ReanchorClock(next);
			}

			LogDiff("calendar_mode", previous.Mode, next.Mode);
			LogDiff("offset_days", previous.OffsetDays, next.OffsetDays);
			LogDiff("offset_hours", previous.OffsetHours, next.OffsetHours);
			LogDiff("fixed_date", FormatDate(previous.FixedDate), FormatDate(next.FixedDate));
			LogDiff("game_clock.fixed_hour", previous.FixedHour, next.FixedHour);
			LogSeasonDiff(previous, next);

			if (Math.Abs(previous.SnapshotIntervalSeconds - next.SnapshotIntervalSeconds) > 0.001
				|| Math.Abs(previous.SeasonCheckIntervalSeconds - next.SeasonCheckIntervalSeconds) > 0.001)
			{
				StartTimers(next);
			}

			ServiceLog.Info(LogSystem, "World date is now {0:yyyy-MM-dd HH:mm}.", Now);
			CheckSeasons();
			RefreshSnapshots();
		}

		private static void LogDiff(string field, object before, object after)
		{
			if (!Equals(before, after))
				ServiceLog.Info(LogSystem, "{0}: {1} -> {2}", field, before ?? "(none)", after ?? "(none)");
		}

		private static void LogSeasonDiff(WorldTimeConfig previous, WorldTimeConfig next)
		{
			foreach (SeasonDefinition season in next.Seasons)
			{
				SeasonDefinition old = previous.FindSeason(season.Id);
				if (old == null)
					ServiceLog.Info(LogSystem, "Season added: {0} {1}..{2} enabled={3}", season.Id, season.Start, season.End, season.Enabled);
				else if (old.Signature != season.Signature)
					ServiceLog.Info(LogSystem, "Season changed: {0} {1}..{2} enabled={3} (was {4}..{5} enabled={6})",
						season.Id, season.Start, season.End, season.Enabled, old.Start, old.End, old.Enabled);
			}

			foreach (SeasonDefinition season in previous.Seasons)
			{
				if (next.FindSeason(season.Id) == null)
					ServiceLog.Info(LogSystem, "Season removed: {0}", season.Id);
			}
		}

		private static string FormatDate(DateTime? date)
		{
			return date == null ? null : date.Value.ToString("yyyy-MM-dd HH:mm:ss");
		}

		private static void StartTimers(WorldTimeConfig config)
		{
			if (m_SeasonTimer != null)
				m_SeasonTimer.Stop();
			if (m_SnapshotTimer != null)
				m_SnapshotTimer.Stop();

			TimeSpan seasonInterval = TimeSpan.FromSeconds(config.SeasonCheckIntervalSeconds);
			TimeSpan snapshotInterval = TimeSpan.FromSeconds(config.SnapshotIntervalSeconds);

			m_SeasonTimer = Timer.DelayCall(seasonInterval, seasonInterval, new TimerCallback(CheckSeasons));
			m_SnapshotTimer = Timer.DelayCall(snapshotInterval, snapshotInterval, new TimerCallback(RefreshSnapshots));
		}

		private static HashSet<string> ComputeActiveSeasons()
		{
			var active = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			DateTime now = Now;

			foreach (SeasonDefinition season in Effective.Seasons)
			{
				if (season.Enabled && season.Contains(now))
					active.Add(season.Id);
			}

			return active;
		}

		private static void CheckSeasons()
		{
			HashSet<string> previous = m_ActiveSeasons;
			HashSet<string> current = ComputeActiveSeasons();
			m_ActiveSeasons = current;

			foreach (string id in current)
			{
				if (!previous.Contains(id))
					RaiseSeasonChanged(id, true);
			}

			foreach (string id in previous)
			{
				if (!current.Contains(id))
					RaiseSeasonChanged(id, false);
			}
		}

		private static void RaiseSeasonChanged(string id, bool active)
		{
			ServiceLog.Info(LogSystem, "Season {0} {1} (world date {2:yyyy-MM-dd}).", id, active ? "STARTED" : "ENDED", Now);

			Action<string, bool> handlers = SeasonChanged;
			if (handlers == null)
				return;

			foreach (Action<string, bool> handler in handlers.GetInvocationList())
			{
				try
				{
					handler(id, active);
				}
				catch (Exception ex)
				{
					ServiceLog.Error(LogSystem, "SeasonChanged handler threw: {0}", ex);
				}
			}
		}

		private static void RefreshSnapshots()
		{
			WorldTimeConfig config = Effective;
			DateTime now = Now;

			int hours, minutes, totalMinutes;
			Clock.GetTime(Map.Sosaria, 100, 100, out hours, out minutes, out totalMinutes);

			m_TimeSnapshot = new Dictionary<string, object>
			{
				{ "enabled", m_File.Current.Enabled },
				{ "calendar_mode", config.Mode.ToString().ToLowerInvariant() },
				{ "real_time", DateTime.Now.ToString("o") },
				{ "world_time", now.ToString("yyyy-MM-ddTHH:mm:ss") },
				{ "world_date", now.ToString("yyyy-MM-dd") },
				{ "month", now.Month },
				{ "game_clock", new Dictionary<string, object>
					{
						{ "hour", hours },
						{ "minute", minutes },
						{ "display", string.Format("{0:00}:{1:00}", hours, minutes) },
						{ "total_minutes", totalMinutes },
						{ "moon_phase", Clock.GetMoonPhase(Map.Sosaria, 100, 100).ToString() },
						{ "seconds_per_uo_minute", config.SecondsPerUOMinute },
						{ "frozen", config.Frozen },
						{ "fixed_hour", config.FixedHour },
					}
				},
				{ "active_seasons", new List<string>(ComputeActiveSeasons()) },
				{ "generated_at", DateTime.Now.ToString("o") },
			};

			var seasons = new List<Dictionary<string, object>>();
			foreach (SeasonDefinition season in config.Seasons)
			{
				bool active = season.Enabled && season.Contains(now);
				seasons.Add(new Dictionary<string, object>
				{
					{ "id", season.Id },
					{ "display_name", season.DisplayName },
					{ "enabled", season.Enabled },
					{ "active", active },
					{ "start", season.Start },
					{ "end", season.End },
					{ "starts_on", active ? null : season.NextStart(now).ToString("yyyy-MM-dd") },
					{ "ends_on", season.NextEnd(now).ToString("yyyy-MM-dd") },
				});
			}

			m_SeasonsSnapshot = seasons;
		}

		[Usage("WorldTime")]
		[Description("Shows the world calendar date, in-game clock and active seasons.")]
		private static void WorldTime_OnCommand(CommandEventArgs e)
		{
			WorldTimeConfig config = Effective;
			int hours, minutes, totalMinutes;
			Clock.GetTime(Map.Sosaria, 100, 100, out hours, out minutes, out totalMinutes);

			e.Mobile.SendMessage("World time {0} (mode {1}): {2:yyyy-MM-dd HH:mm}", Enabled ? "enabled" : "disabled", config.Mode, Now);
			e.Mobile.SendMessage("Game clock {0:00}:{1:00}, {2}s per UO minute{3}{4}", hours, minutes, config.SecondsPerUOMinute,
				config.Frozen ? ", frozen" : "", config.FixedHour >= 0 ? ", fixed hour " + config.FixedHour : "");
			e.Mobile.SendMessage("Active seasons: {0}", m_ActiveSeasons.Count == 0 ? "none" : string.Join(", ", new List<string>(m_ActiveSeasons).ToArray()));
		}
	}
}
