using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Server.Localization;

namespace Server.RateConfig
{
	/// <summary>
	/// Generic, JSON-config-driven table of named numeric values (probabilities, weights, chances, ...).
	/// Loads every "*.json" file under "Data/RateConfig/" (recursive) at startup and merges them into a
	/// single flat "dotted.key" -&gt; double map; a GM can hot-reload the same files at runtime via
	/// [ratereload] (see <c>Server.Commands.RateConfigCommands</c>) without restarting the shard.
	///
	/// This class is intentionally domain-agnostic — it knows nothing about dragons, loot, or any other
	/// specific feature. Callers own their own key namespace (e.g. "dragon.breedWeight.xormite",
	/// "gemdragon.scaleWeight.platinum") and use <see cref="GetDouble"/> / <see cref="GetTable"/> to read
	/// it back. Any future probability feature can adopt this engine by picking a new dotted prefix and
	/// dropping a JSON file under Data/RateConfig/ — no engine changes required.
	///
	/// This type lives in System.csproj (compiled directly into the exe), not the runtime-compiled
	/// "Scripts" assembly (<c>Source/Scripts/**</c>). <see cref="ScriptCompiler.Invoke"/> only reflects
	/// over the Scripts assembly, so the <see cref="CallPriorityAttribute"/> on <see cref="Initialize"/>
	/// below is <b>not</b> what causes it to run — <c>Main.cs</c> calls <see cref="Load"/> directly at
	/// startup (same pattern as <see cref="Server.Localization.LocalizationBootstrap"/>). The accessors
	/// below additionally lazy-load on first use as a defensive fallback, so a missing/removed startup
	/// call degrades to "loads a bit later" rather than "silently always returns defaults forever".
	///
	/// JSON format constraint: the codebase's only JSON reader is the flat string-only parser
	/// <see cref="SimpleJsonObject.ParseStringProperties"/> ("{"key":"value"}", no numbers/nested
	/// objects/arrays). Numeric values are therefore stored as quoted strings (e.g. "0.1") and parsed
	/// here with <see cref="double.TryParse"/> using <see cref="CultureInfo.InvariantCulture"/>.
	/// </summary>
	public static class RateConfigEngine
	{
		private static readonly object m_Lock = new object();
		private static Dictionary<string, double> m_Values = new Dictionary<string, double>( StringComparer.Ordinal );
		private static bool m_Loaded;

		public static void Load()
		{
			lock ( m_Lock )
			{
				var values = new Dictionary<string, double>( StringComparer.Ordinal );

				string root = Path.Combine( Core.BaseDirectory, "Data/RateConfig" );
				int fileCount = 0;
				int badEntryCount = 0;

				if ( Directory.Exists( root ) )
				{
					foreach ( string path in Directory.GetFiles( root, "*.json", SearchOption.AllDirectories ) )
					{
						MergeJsonFile( path, values, ref badEntryCount );
						++fileCount;
					}
				}

				m_Values = values;
				m_Loaded = true;

				Console.WriteLine( "RateConfig: loaded {0} keys from {1} files under Data/RateConfig/{2}.", m_Values.Count, fileCount, badEntryCount > 0 ? string.Format( " ({0} malformed entries skipped)", badEntryCount ) : "" );
			}
		}

		public static void Reload()
		{
			Load();
		}

		public static bool IsLoaded { get { return m_Loaded; } }

		/// <summary>
		/// Defensive fallback for the accessors below: normally <c>Main.cs</c> calls <see cref="Load"/>
		/// once at startup (see class remarks), so this is a no-op in the common case. If that ever
		/// stops happening (e.g. a future refactor drops the call), reads still work — just lazily,
		/// from whichever thread reads a key first — instead of forever returning caller defaults.
		/// </summary>
		private static void EnsureLoaded()
		{
			if ( !m_Loaded )
				Load();
		}

		private static void MergeJsonFile( string path, Dictionary<string, double> target, ref int badEntryCount )
		{
			if ( !File.Exists( path ) )
				return;

			try
			{
				string json = File.ReadAllText( path );
				var chunk = new Dictionary<string, string>( StringComparer.Ordinal );
				SimpleJsonObject.ParseStringProperties( json, chunk );

				foreach ( var kv in chunk )
				{
					double value;

					if ( double.TryParse( kv.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value ) )
					{
						target[kv.Key] = value;
					}
					else
					{
						++badEntryCount;
						Console.WriteLine( "RateConfig: malformed value for key {0} in {1}: \"{2}\" is not a number — ignored.", kv.Key, path, kv.Value );
					}
				}
			}
			catch ( Exception ex )
			{
				Console.WriteLine( "RateConfig: failed to load {0}: {1}", path, ex.Message );
			}
		}

		/// <summary>
		/// Returns the configured value for <paramref name="key"/>, or <paramref name="defaultValue"/>
		/// when the key is missing (or the engine failed to load). Never throws.
		/// </summary>
		public static double GetDouble( string key, double defaultValue )
		{
			if ( key == null )
				return defaultValue;

			EnsureLoaded();

			lock ( m_Lock )
			{
				double value;

				if ( m_Values.TryGetValue( key, out value ) )
					return value;
			}

			return defaultValue;
		}

		/// <summary>
		/// Returns every entry whose key starts with "<paramref name="prefix"/>.", keyed by the
		/// remainder after that prefix. E.g. GetTable("gemdragon.scaleWeight") returns
		/// {"red":10, "platinum":2, ...} from keys "gemdragon.scaleWeight.red" / ".platinum".
		/// This is the generic primitive that makes the engine reusable across features.
		/// </summary>
		public static Dictionary<string, double> GetTable( string prefix )
		{
			var result = new Dictionary<string, double>( StringComparer.Ordinal );

			if ( prefix == null )
				return result;

			EnsureLoaded();

			string dottedPrefix = prefix + ".";

			lock ( m_Lock )
			{
				foreach ( var kv in m_Values )
				{
					if ( kv.Key.Length > dottedPrefix.Length && kv.Key.StartsWith( dottedPrefix, StringComparison.Ordinal ) )
						result[kv.Key.Substring( dottedPrefix.Length )] = kv.Value;
				}
			}

			return result;
		}

		/// <summary>
		/// Returns every raw key/value currently loaded (debug / [ratelist] with no prefix filter).
		/// </summary>
		public static Dictionary<string, double> GetAll()
		{
			EnsureLoaded();

			lock ( m_Lock )
			{
				return new Dictionary<string, double>( m_Values, StringComparer.Ordinal );
			}
		}

		/// <summary>
		/// NOT auto-invoked by <see cref="ScriptCompiler.Invoke"/> — see class remarks. Kept as an
		/// explicit, named entry point (called directly from <c>Main.cs</c>) rather than relying on the
		/// reflection-based Scripts assembly scan. The <see cref="CallPriorityAttribute"/> is retained
		/// only for stylistic parity with <see cref="Server.Localization.LocalizationBootstrap"/>.
		/// </summary>
		[CallPriority( -150 )]
		public static void Initialize()
		{
			Load();
		}
	}
}
