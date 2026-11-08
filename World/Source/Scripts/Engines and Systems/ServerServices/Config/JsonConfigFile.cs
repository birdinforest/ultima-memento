using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Engines.ServerServices
{
	public interface IConfigFile
	{
		string Name { get; }
		string FilePath { get; }
		int Version { get; }
		DateTime LastLoaded { get; }
		string LastError { get; }
		bool UsingDefaults { get; }

		/// <summary>Reloads when the file's modification time changed. Returns true when a new config was applied.</summary>
		bool CheckForChanges();

		bool Reload(string reason);
	}

	/// <summary>
	/// A JSON config file with validate-then-swap reload semantics.
	/// <see cref="Current"/> always holds the last valid config (or code defaults).
	/// </summary>
	public sealed class JsonConfigFile<T> : IConfigFile where T : class
	{
		public const string LogSystem = "Config";

		// Wait for writes to settle so a half-saved file isn't parsed.
		private static readonly TimeSpan SettleDelay = TimeSpan.FromMilliseconds(750);

		private readonly Func<Dictionary<string, object>, T> m_Parse;
		private readonly Func<T> m_Defaults;
		private readonly Func<T, List<string>> m_Validate;

		private volatile T m_Current;
		private DateTime m_SeenWriteTimeUtc = DateTime.MinValue;
		private bool m_MissingLogged;

		public string Name { get; private set; }
		public string FilePath { get; private set; }
		public int Version { get; private set; }
		public DateTime LastLoaded { get; private set; }
		public string LastError { get; private set; }
		public bool UsingDefaults { get; private set; }

		public T Current { get { return m_Current; } }

		/// <summary>Raised on the game thread after a new config is applied: (previous, next).</summary>
		public event Action<T, T> Changed;

		public JsonConfigFile(string name, string filePath, Func<Dictionary<string, object>, T> parse, Func<T> defaults, Func<T, List<string>> validate)
		{
			Name = name;
			FilePath = filePath;
			m_Parse = parse;
			m_Defaults = defaults;
			m_Validate = validate;

			m_Current = defaults();
			UsingDefaults = true;
		}

		public bool CheckForChanges()
		{
			if (!File.Exists(FilePath))
			{
				if (!m_MissingLogged)
				{
					m_MissingLogged = true;
					ServiceLog.Warn(LogSystem, "{0}: file not found at {1}; keeping {2}.", Name, FilePath, UsingDefaults ? "code defaults" : "last good config");
				}
				return false;
			}

			m_MissingLogged = false;

			DateTime writeTime = File.GetLastWriteTimeUtc(FilePath);
			if (writeTime == m_SeenWriteTimeUtc)
				return false;

			if (DateTime.UtcNow - writeTime < SettleDelay)
				return false;

			return Reload("file changed");
		}

		public bool Reload(string reason)
		{
			if (!File.Exists(FilePath))
			{
				LastError = "File not found.";
				ServiceLog.Warn(LogSystem, "{0}: reload ({1}) skipped, file not found at {2}.", Name, reason, FilePath);
				return false;
			}

			m_SeenWriteTimeUtc = File.GetLastWriteTimeUtc(FilePath);

			T next;
			try
			{
				string json;
				using (var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
				using (var reader = new StreamReader(stream, Encoding.UTF8))
					json = reader.ReadToEnd();

				next = m_Parse(JsonHelper.ParseObject(json));
			}
			catch (Exception ex)
			{
				LastError = ex.Message;
				ServiceLog.Error(LogSystem, "{0}: reload ({1}) failed to parse, keeping previous config. {2}", Name, reason, ex.Message);
				return false;
			}

			List<string> errors = m_Validate != null ? m_Validate(next) : null;
			if (errors != null && errors.Count > 0)
			{
				LastError = string.Join("; ", errors.ToArray());
				ServiceLog.Error(LogSystem, "{0}: reload ({1}) failed validation, keeping previous config. {2}", Name, reason, LastError);
				return false;
			}

			T previous = m_Current;
			m_Current = next;
			Version++;
			LastLoaded = DateTime.Now;
			LastError = null;
			UsingDefaults = false;

			ServiceLog.Info(LogSystem, "{0}: loaded v{1} ({2}).", Name, Version, reason);

			Action<T, T> handlers = Changed;
			if (handlers != null)
			{
				foreach (Action<T, T> handler in handlers.GetInvocationList())
				{
					try
					{
						handler(previous, next);
					}
					catch (Exception ex)
					{
						ServiceLog.Error(LogSystem, "{0}: change handler threw: {1}", Name, ex);
					}
				}
			}

			return true;
		}

		public T Defaults()
		{
			return m_Defaults();
		}
	}
}
