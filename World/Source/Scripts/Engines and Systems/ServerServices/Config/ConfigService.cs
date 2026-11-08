using System;
using System.Collections.Generic;
using System.IO;
using Server.Commands;

namespace Server.Engines.ServerServices
{
	public sealed class ServicesConfig
	{
		public double PollIntervalSeconds { get; private set; }
		public ServiceLogLevel LogLevel { get; private set; }

		public static ServicesConfig FromJson(Dictionary<string, object> d)
		{
			return new ServicesConfig
			{
				PollIntervalSeconds = JsonHelper.GetDouble(d, "poll_interval_seconds", 2.0),
				LogLevel = ServiceLog.ParseLevel(JsonHelper.GetString(d, "log_level", null), ServiceLogLevel.Info),
			};
		}

		public static ServicesConfig Defaults()
		{
			return FromJson(new Dictionary<string, object>());
		}

		public List<string> Validate()
		{
			var errors = new List<string>();
			if (PollIntervalSeconds < 0.5 || PollIntervalSeconds > 300)
				errors.Add("poll_interval_seconds must be between 0.5 and 300.");
			return errors;
		}
	}

	/// <summary>
	/// Registry of hot-reloadable JSON config files under Data/System/CFG/Services.
	/// Files are polled on the game thread, so change handlers may touch game state directly.
	/// </summary>
	public static class ConfigService
	{
		public const string LogSystem = "Config";
		public const string ConfigDirectory = "Data/System/CFG/Services";

		private static readonly List<IConfigFile> m_Files = new List<IConfigFile>();
		private static readonly JsonConfigFile<ServicesConfig> m_ServicesFile;
		private static Timer m_PollTimer;
		private static volatile List<Dictionary<string, object>> m_StatusSnapshot = new List<Dictionary<string, object>>();

		static ConfigService()
		{
			m_ServicesFile = Register("services", "services.json", ServicesConfig.FromJson, ServicesConfig.Defaults, c => c.Validate());
			ServiceLog.SetLevel(LogSystem, m_ServicesFile.Current.LogLevel);
			m_ServicesFile.Changed += OnServicesChanged;
		}

		public static IEnumerable<IConfigFile> Files
		{
			get { lock (m_Files) return m_Files.ToArray(); }
		}

		/// <summary>Thread-safe status of every registered file, refreshed after each poll.</summary>
		public static List<Dictionary<string, object>> StatusSnapshot
		{
			get { return m_StatusSnapshot; }
		}

		public static string ResolvePath(string fileName)
		{
			return Path.Combine(Path.Combine(Core.BaseDirectory, ConfigDirectory), fileName);
		}

		public static JsonConfigFile<T> Register<T>(string name, string fileName, Func<Dictionary<string, object>, T> parse, Func<T> defaults, Func<T, List<string>> validate) where T : class
		{
			lock (m_Files)
			{
				foreach (IConfigFile existing in m_Files)
				{
					if (string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase))
						throw new InvalidOperationException(string.Format("Config '{0}' is already registered.", name));
				}
			}

			var file = new JsonConfigFile<T>(name, ResolvePath(fileName), parse, defaults, validate);

			if (File.Exists(file.FilePath))
				file.Reload("startup");
			else
				ServiceLog.Warn(LogSystem, "{0}: {1} not found; using code defaults.", name, file.FilePath);

			lock (m_Files)
				m_Files.Add(file);

			RefreshStatusSnapshot();
			return file;
		}

		public static void Initialize()
		{
			StartPolling(m_ServicesFile.Current.PollIntervalSeconds);

			CommandSystem.Register("ConfigReload", AccessLevel.Administrator, new CommandEventHandler(ConfigReload_OnCommand));
			CommandSystem.Register("ConfigStatus", AccessLevel.Administrator, new CommandEventHandler(ConfigStatus_OnCommand));
		}

		private static void OnServicesChanged(ServicesConfig previous, ServicesConfig next)
		{
			ServiceLog.SetLevel(LogSystem, next.LogLevel);

			if (Math.Abs(previous.PollIntervalSeconds - next.PollIntervalSeconds) > 0.001 && m_PollTimer != null)
			{
				ServiceLog.Info(LogSystem, "Poll interval changed {0}s -> {1}s.", previous.PollIntervalSeconds, next.PollIntervalSeconds);
				StartPolling(next.PollIntervalSeconds);
			}
		}

		private static void StartPolling(double seconds)
		{
			if (m_PollTimer != null)
				m_PollTimer.Stop();

			TimeSpan interval = TimeSpan.FromSeconds(seconds);
			m_PollTimer = Timer.DelayCall(interval, interval, new TimerCallback(Poll));
		}

		private static void Poll()
		{
			bool changed = false;

			foreach (IConfigFile file in Files)
			{
				try
				{
					changed |= file.CheckForChanges();
				}
				catch (Exception ex)
				{
					ServiceLog.Error(LogSystem, "{0}: poll failed: {1}", file.Name, ex.Message);
				}
			}

			if (changed)
				RefreshStatusSnapshot();
		}

		private static void RefreshStatusSnapshot()
		{
			var snapshot = new List<Dictionary<string, object>>();

			foreach (IConfigFile file in Files)
			{
				snapshot.Add(new Dictionary<string, object>
				{
					{ "name", file.Name },
					{ "file", Path.GetFileName(file.FilePath) },
					{ "version", file.Version },
					{ "using_defaults", file.UsingDefaults },
					{ "last_loaded", file.LastLoaded == DateTime.MinValue ? null : file.LastLoaded.ToString("o") },
					{ "last_error", file.LastError },
				});
			}

			m_StatusSnapshot = snapshot;
		}

		[Usage("ConfigReload [name]")]
		[Description("Forces a reload of one service config file, or all of them.")]
		private static void ConfigReload_OnCommand(CommandEventArgs e)
		{
			string name = e.Length > 0 ? e.GetString(0) : null;
			int matched = 0;

			foreach (IConfigFile file in Files)
			{
				if (name != null && !string.Equals(file.Name, name, StringComparison.OrdinalIgnoreCase))
					continue;

				matched++;
				bool ok = file.Reload(string.Format("command by {0}", e.Mobile.Name));
				e.Mobile.SendMessage(ok ? 0x40 : 0x22, "{0}: {1}", file.Name, ok ? "reloaded v" + file.Version : "failed - " + file.LastError);
			}

			if (matched == 0)
				e.Mobile.SendMessage(0x22, "No config named '{0}'.", name);

			RefreshStatusSnapshot();
		}

		[Usage("ConfigStatus")]
		[Description("Lists service config files with version, load time and last error.")]
		private static void ConfigStatus_OnCommand(CommandEventArgs e)
		{
			foreach (IConfigFile file in Files)
			{
				e.Mobile.SendMessage("{0} v{1} {2} loaded {3}{4}",
					file.Name,
					file.Version,
					file.UsingDefaults ? "(defaults)" : "",
					file.LastLoaded == DateTime.MinValue ? "never" : file.LastLoaded.ToString("yyyy-MM-dd HH:mm:ss"),
					file.LastError == null ? "" : " | error: " + file.LastError);
			}
		}
	}
}
