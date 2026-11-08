using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Engines.ServerServices
{
	public enum ServiceLogLevel
	{
		Debug = 0,
		Info = 1,
		Warn = 2,
		Error = 3,
		Off = 4
	}

	/// <summary>
	/// Shared log for server services. Writes to the console and to Logs/services/&lt;system&gt;-&lt;yyyyMMdd&gt;.log.
	/// Safe to call from any thread.
	/// </summary>
	public static class ServiceLog
	{
		private static readonly object m_Lock = new object();
		private static readonly Dictionary<string, ServiceLogLevel> m_Levels = new Dictionary<string, ServiceLogLevel>(StringComparer.OrdinalIgnoreCase);

		public static string LogDirectory
		{
			get { return Path.Combine(Path.Combine(Core.BaseDirectory, "Logs"), "services"); }
		}

		public static void SetLevel(string system, ServiceLogLevel level)
		{
			lock (m_Lock)
				m_Levels[system] = level;
		}

		public static ServiceLogLevel GetLevel(string system)
		{
			lock (m_Lock)
			{
				ServiceLogLevel level;
				return m_Levels.TryGetValue(system, out level) ? level : ServiceLogLevel.Info;
			}
		}

		public static ServiceLogLevel ParseLevel(string value, ServiceLogLevel fallback)
		{
			if (string.IsNullOrEmpty(value))
				return fallback;

			ServiceLogLevel level;
			return Enum.TryParse(value.Trim(), true, out level) ? level : fallback;
		}

		public static void Debug(string system, string format, params object[] args) { Write(system, ServiceLogLevel.Debug, format, args); }
		public static void Info(string system, string format, params object[] args) { Write(system, ServiceLogLevel.Info, format, args); }
		public static void Warn(string system, string format, params object[] args) { Write(system, ServiceLogLevel.Warn, format, args); }
		public static void Error(string system, string format, params object[] args) { Write(system, ServiceLogLevel.Error, format, args); }

		public static void Write(string system, ServiceLogLevel level, string format, params object[] args)
		{
			if (level == ServiceLogLevel.Off || level < GetLevel(system))
				return;

			string message = (args == null || args.Length == 0) ? format : string.Format(format, args);
			DateTime now = DateTime.Now;
			string line = string.Format("{0:yyyy-MM-dd HH:mm:ss} [{1}] {2}", now, level.ToString().ToUpperInvariant(), message);

			Console.WriteLine("[{0}] {1}", system, message);

			lock (m_Lock)
			{
				try
				{
					string dir = LogDirectory;
					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					string path = Path.Combine(dir, string.Format("{0}-{1:yyyyMMdd}.log", system, now));
					File.AppendAllText(path, line + Environment.NewLine);
				}
				catch
				{
				}
			}
		}
	}
}
