using System;
using System.IO;

namespace Server.Localization
{
	public static class LangConfig
	{
		private static string m_DefaultLanguage = "en";
		private static string m_FallbackLanguage = "en";

		public static string DefaultLanguage
		{
			get { return m_DefaultLanguage; }
		}

		public static string FallbackLanguage
		{
			get { return m_FallbackLanguage; }
		}

		public static void Configure()
		{
			string path = Path.Combine( Core.BaseDirectory, "Data/System/CFG/localization.cfg" );

			if ( !File.Exists( path ) )
				return;

			foreach ( string line in File.ReadAllLines( path ) )
			{
				string l = line.Trim();

				if ( l.Length == 0 || l.StartsWith( "#" ) )
					continue;

				int eq = l.IndexOf( '=' );

				if ( eq <= 0 )
					continue;

				string key = l.Substring( 0, eq ).Trim();
				string val = l.Substring( eq + 1 ).Trim();

				if ( Insensitive.Equals( key, "DefaultLanguage" ) && val.Length > 0 )
					m_DefaultLanguage = val;
				else if ( Insensitive.Equals( key, "FallbackLanguage" ) && val.Length > 0 )
					m_FallbackLanguage = val;
			}
		}
	}
}
