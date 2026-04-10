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

		public static void Load()
		{
			lock ( m_Lock )
			{
				m_En = LoadFile( Path.Combine( Core.BaseDirectory, "Data/Localization/strings.en.json" ) );
				m_Zh = LoadFile( Path.Combine( Core.BaseDirectory, "Data/Localization/strings.zh-Hans.json" ) );
				m_Loaded = true;
				Console.WriteLine( "Localization: loaded {0} English and {1} Chinese string overrides.", m_En.Count, m_Zh.Count );
			}
		}

		public static void Reload()
		{
			Load();
		}

		private static Dictionary<string, string> LoadFile( string path )
		{
			var dict = new Dictionary<string, string>( StringComparer.Ordinal );

			if ( !File.Exists( path ) )
				return dict;

			try
			{
				string json = File.ReadAllText( path );
				SimpleJsonObject.ParseStringProperties( json, dict );
			}
			catch ( Exception ex )
			{
				Console.WriteLine( "Localization: failed to load {0}: {1}", path, ex.Message );
			}

			return dict;
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
