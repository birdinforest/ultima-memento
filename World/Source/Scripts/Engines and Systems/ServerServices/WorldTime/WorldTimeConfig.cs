using System;
using System.Collections.Generic;
using System.Globalization;

namespace Server.Engines.ServerServices.Time
{
	public enum CalendarMode
	{
		Real,
		Offset,
		Fixed
	}

	/// <summary>A yearly date window such as 12-01..12-31. Windows may wrap the year end (12-20..01-06).</summary>
	public sealed class SeasonDefinition
	{
		public string Id { get; private set; }
		public string DisplayName { get; private set; }
		public bool Enabled { get; private set; }
		public int StartMonth { get; private set; }
		public int StartDay { get; private set; }
		public int EndMonth { get; private set; }
		public int EndDay { get; private set; }

		public SeasonDefinition(string id, string displayName, bool enabled, string start, string end)
		{
			Id = id;
			DisplayName = string.IsNullOrEmpty(displayName) ? id : displayName;
			Enabled = enabled;

			int m, d;
			ParseMonthDay(id, "start", start, out m, out d);
			StartMonth = m;
			StartDay = d;
			ParseMonthDay(id, "end", end, out m, out d);
			EndMonth = m;
			EndDay = d;
		}

		public string Start { get { return string.Format("{0:00}-{1:00}", StartMonth, StartDay); } }
		public string End { get { return string.Format("{0:00}-{1:00}", EndMonth, EndDay); } }

		public bool Contains(DateTime date)
		{
			int key = date.Month * 100 + date.Day;
			int start = StartMonth * 100 + StartDay;
			int end = EndMonth * 100 + EndDay;

			return start <= end ? (key >= start && key <= end) : (key >= start || key <= end);
		}

		/// <summary>First date on or after <paramref name="from"/> that falls on the season's start day.</summary>
		public DateTime NextStart(DateTime from)
		{
			return NextOccurrence(StartMonth, StartDay, from.Date);
		}

		/// <summary>Last day of the window that is active on, or next begins after, <paramref name="from"/>.</summary>
		public DateTime NextEnd(DateTime from)
		{
			DateTime anchor = Contains(from) ? from.Date : NextStart(from);
			return NextOccurrence(EndMonth, EndDay, anchor);
		}

		public string Signature
		{
			get { return string.Format("{0}|{1}|{2}|{3}", Enabled, Start, End, DisplayName); }
		}

		private static DateTime NextOccurrence(int month, int day, DateTime from)
		{
			for (int year = from.Year; year <= from.Year + 1; year++)
			{
				DateTime candidate = new DateTime(year, month, Math.Min(day, DateTime.DaysInMonth(year, month)));
				if (candidate >= from)
					return candidate;
			}

			return new DateTime(from.Year + 1, month, Math.Min(day, DateTime.DaysInMonth(from.Year + 1, month)));
		}

		private static void ParseMonthDay(string id, string field, string value, out int month, out int day)
		{
			DateTime parsed;
			if (string.IsNullOrEmpty(value) || !DateTime.TryParseExact(value.Trim() + "-2000", "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
				throw new FormatException(string.Format("seasons.{0}.{1} must be MM-dd (got '{2}').", id, field, value));

			month = parsed.Month;
			day = parsed.Day;
		}
	}

	public sealed class WorldTimeConfig
	{
		public const double DefaultSecondsPerUOMinute = 5.0;

		public bool Enabled { get; private set; }
		public ServiceLogLevel LogLevel { get; private set; }
		public CalendarMode Mode { get; private set; }
		public double OffsetDays { get; private set; }
		public double OffsetHours { get; private set; }
		public DateTime? FixedDate { get; private set; }
		public double SecondsPerUOMinute { get; private set; }
		public bool Frozen { get; private set; }
		public int FixedHour { get; private set; }
		public double SnapshotIntervalSeconds { get; private set; }
		public double SeasonCheckIntervalSeconds { get; private set; }
		public List<SeasonDefinition> Seasons { get; private set; }

		public SeasonDefinition FindSeason(string id)
		{
			foreach (SeasonDefinition season in Seasons)
			{
				if (string.Equals(season.Id, id, StringComparison.OrdinalIgnoreCase))
					return season;
			}

			return null;
		}

		public static WorldTimeConfig FromJson(Dictionary<string, object> d)
		{
			var config = new WorldTimeConfig
			{
				Enabled = JsonHelper.GetBool(d, "enabled", false),
				LogLevel = ServiceLog.ParseLevel(JsonHelper.GetString(d, "log_level", null), ServiceLogLevel.Info),
				Mode = ParseMode(JsonHelper.GetString(d, "calendar_mode", "real")),
				OffsetDays = JsonHelper.GetDouble(d, "offset_days", 0),
				OffsetHours = JsonHelper.GetDouble(d, "offset_hours", 0),
				FixedDate = ParseDate(JsonHelper.GetString(d, "fixed_date", null)),
				SnapshotIntervalSeconds = JsonHelper.GetDouble(d, "snapshot_interval_seconds", 5),
				SeasonCheckIntervalSeconds = JsonHelper.GetDouble(d, "season_check_interval_seconds", 30),
				Seasons = new List<SeasonDefinition>(),
			};

			Dictionary<string, object> clock = JsonHelper.GetObject(d, "game_clock");
			config.SecondsPerUOMinute = JsonHelper.GetDouble(clock, "seconds_per_uo_minute", DefaultSecondsPerUOMinute);
			config.Frozen = JsonHelper.GetBool(clock, "frozen", false);
			config.FixedHour = JsonHelper.GetInt(clock, "fixed_hour", -1);

			Dictionary<string, object> seasons = JsonHelper.GetObject(d, "seasons");
			if (seasons == null)
			{
				// Matches the original month gates in ItemSales.
				config.Seasons.Add(new SeasonDefinition("christmas", "Midwinter", true, "12-01", "12-31"));
				config.Seasons.Add(new SeasonDefinition("halloween", "Halloween", true, "10-01", "10-31"));
			}
			else
			{
				foreach (KeyValuePair<string, object> entry in seasons)
				{
					if (entry.Key.StartsWith("_"))
						continue;

					Dictionary<string, object> s = JsonHelper.GetObject(seasons, entry.Key);
					config.Seasons.Add(new SeasonDefinition(
						entry.Key,
						JsonHelper.GetString(s, "display_name", entry.Key),
						JsonHelper.GetBool(s, "enabled", true),
						JsonHelper.GetString(s, "start", null),
						JsonHelper.GetString(s, "end", null)));
				}
			}

			return config;
		}

		public static WorldTimeConfig Defaults()
		{
			return FromJson(new Dictionary<string, object>());
		}

		public List<string> Validate()
		{
			var errors = new List<string>();

			if (Mode == CalendarMode.Fixed && FixedDate == null)
				errors.Add("calendar_mode 'fixed' requires fixed_date.");
			if (Math.Abs(OffsetDays) > 36500)
				errors.Add("offset_days must be within +/-36500.");
			if (SecondsPerUOMinute < 0.1 || SecondsPerUOMinute > 3600)
				errors.Add("game_clock.seconds_per_uo_minute must be between 0.1 and 3600.");
			if (FixedHour < -1 || FixedHour > 23)
				errors.Add("game_clock.fixed_hour must be -1 (off) or 0-23.");
			if (SnapshotIntervalSeconds < 1 || SnapshotIntervalSeconds > 3600)
				errors.Add("snapshot_interval_seconds must be between 1 and 3600.");
			if (SeasonCheckIntervalSeconds < 1 || SeasonCheckIntervalSeconds > 3600)
				errors.Add("season_check_interval_seconds must be between 1 and 3600.");

			return errors;
		}

		private static CalendarMode ParseMode(string value)
		{
			CalendarMode mode;
			if (!Enum.TryParse(value, true, out mode))
				throw new FormatException(string.Format("calendar_mode must be real, offset or fixed (got '{0}').", value));
			return mode;
		}

		private static DateTime? ParseDate(string value)
		{
			if (string.IsNullOrEmpty(value))
				return null;

			DateTime parsed;
			string[] formats = { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss" };
			if (!DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
				throw new FormatException(string.Format("fixed_date must be yyyy-MM-dd or yyyy-MM-ddTHH:mm[:ss] (got '{0}').", value));

			return parsed;
		}
	}
}
