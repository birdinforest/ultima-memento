using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Localization
{
	public static class StringCatalog
	{
		private static readonly object m_Lock = new object();
		private static Dictionary<string, string> m_En = new Dictionary<string, string>( StringComparer.Ordinal );
		private static Dictionary<string, string> m_Zh = new Dictionary<string, string>( StringComparer.Ordinal );
		private static bool m_Loaded;

		private static readonly string[] m_LegacyEnFiles = new string[]
		{
			"Data/Localization/strings.en.json"
		};

		private static readonly string[] m_LegacyZhFiles = new string[]
		{
			"Data/Localization/strings.zh-Hans.json"
		};

		public static void Load()
		{
			lock ( m_Lock )
			{
				m_En = new Dictionary<string, string>( StringComparer.Ordinal );
				m_Zh = new Dictionary<string, string>( StringComparer.Ordinal );

				string root = Path.Combine( Core.BaseDirectory, "Data/Localization" );
				string enRoot = Path.Combine( root, "en" );
				string zhRoot = Path.Combine( root, "zh-Hans" );

				int enFiles = 0, zhFiles = 0;

				if ( Directory.Exists( enRoot ) )
				{
					foreach ( string path in Directory.GetFiles( enRoot, "*.json", SearchOption.AllDirectories ) )
					{
						MergeJsonFile( path, m_En, "en" );
						++enFiles;
					}
				}

				if ( Directory.Exists( zhRoot ) )
				{
					foreach ( string path in Directory.GetFiles( zhRoot, "*.json", SearchOption.AllDirectories ) )
					{
						MergeJsonFile( path, m_Zh, "zh-Hans" );
						++zhFiles;
					}
				}

				// Legacy single-file layout (deprecated)
				if ( m_En.Count == 0 )
				{
					for ( int i = 0; i < m_LegacyEnFiles.Length; ++i )
					{
						string p = Path.Combine( Core.BaseDirectory, m_LegacyEnFiles[i] );

						if ( File.Exists( p ) )
							MergeJsonFile( p, m_En, "en (legacy)" );
					}
				}

				if ( m_Zh.Count == 0 )
				{
					for ( int i = 0; i < m_LegacyZhFiles.Length; ++i )
					{
						string p = Path.Combine( Core.BaseDirectory, m_LegacyZhFiles[i] );

						if ( File.Exists( p ) )
							MergeJsonFile( p, m_Zh, "zh-Hans (legacy)" );
					}
				}

				m_Loaded = true;
				Console.WriteLine( "Localization: merged {0} English keys from {1} files, {2} Chinese keys from {3} files.", m_En.Count, enFiles > 0 ? enFiles.ToString() : "legacy", m_Zh.Count, zhFiles > 0 ? zhFiles.ToString() : "legacy" );
			}
		}

		public static void Reload()
		{
			Load();
		}

		private static void MergeJsonFile( string path, Dictionary<string, string> target, string labelForLog )
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
					string existing;

					if ( target.TryGetValue( kv.Key, out existing ) && existing != kv.Value )
						Console.WriteLine( "Localization: duplicate key {0} in {1} ({2}) — keeping first value.", kv.Key, path, labelForLog );
					else
						target[kv.Key] = kv.Value;
				}
			}
			catch ( Exception ex )
			{
				Console.WriteLine( "Localization: failed to load {0}: {1}", path, ex.Message );
			}
		}

		public static string TryResolve( string languageCode, string englishLiteral )
		{
			if ( englishLiteral == null || englishLiteral.Length == 0 )
				return null;

			if ( !m_Loaded )
				return null;

			string key = StringKey.ForEnglish( englishLiteral );

			lock ( m_Lock )
			{
				if ( AccountLang.IsChinese( languageCode ) )
				{
					string zh;

					if ( m_Zh.TryGetValue( key, out zh ) && zh != null && zh.Length > 0 )
						return zh;
				}

				string en;

				if ( m_En.TryGetValue( key, out en ) && en != null && en.Length > 0 )
					return en;
			}

			return null;
		}

		/// <summary>
		/// Looks up a stable logical key (e.g. books.dynamic.by_line) in merged locale JSON.
		/// Use for strings that are not suitable for hash-based <see cref="TryResolve"/> (fragments, templates).
		/// </summary>
		public static string TryResolveByKey( string languageCode, string key )
		{
			if ( key == null || key.Length == 0 )
				return null;

			if ( !m_Loaded )
				return null;

			lock ( m_Lock )
			{
				if ( AccountLang.IsChinese( languageCode ) )
				{
					string zh;

					if ( m_Zh.TryGetValue( key, out zh ) && zh != null && zh.Length > 0 )
						return zh;
				}

				string en;

				if ( m_En.TryGetValue( key, out en ) && en != null && en.Length > 0 )
					return en;
			}

			return null;
		}

		/// <summary>
		/// For strings whose <paramref name="text"/> is a stable logical id (prefix <c>quest-</c>),
		/// tries <see cref="TryResolveByKey"/> first, then hash <see cref="TryResolve"/>; otherwise only hash lookup.
		/// Used by ML quest gumps and RPG dialogue so quest copy can live in hand-maintained JSON keys.
		/// </summary>
		public static string TryResolveLogicalOrHash( string languageCode, string text )
		{
			if ( text == null || text.Length == 0 )
				return null;

			if ( !m_Loaded )
				return null;

			if ( text.StartsWith( "quest-", StringComparison.Ordinal ) )
			{
				string byKey = TryResolveByKey( languageCode, text );

				if ( byKey != null && byKey.Length > 0 )
					return byKey;

				return TryResolve( languageCode, text );
			}

			return TryResolve( languageCode, text );
		}

		/// <summary>
		/// Convenience wrapper: resolves <paramref name="english"/> for the given account's language,
		/// falling back to the English literal when no translation is found.
		/// </summary>
		public static string Resolve( Server.Accounting.IAccount account, string english )
		{
			if ( english == null )
				return string.Empty;

			string lang = AccountLang.GetLanguageCode( account );
			return TryResolve( lang, english ) ?? english;
		}

		/// <summary>
		/// Convenience wrapper: resolves a format template for <paramref name="account"/>'s language,
		/// then substitutes <paramref name="args"/> via <see cref="string.Format"/>.
		/// </summary>
		public static string ResolveFormat( Server.Accounting.IAccount account, string englishFormat, params object[] args )
		{
			string resolved = Resolve( account, englishFormat );
			return args == null || args.Length == 0 ? resolved : string.Format( resolved, args );
		}

		public static bool IsAsciiOnly( string s )
		{
			if ( s == null )
				return true;

			for ( int i = 0; i < s.Length; ++i )
			{
				if ( s[i] > 0x7F )
					return false;
			}

			return true;
		}
	}
}
