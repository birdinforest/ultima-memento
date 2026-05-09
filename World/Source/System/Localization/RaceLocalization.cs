using Server;
using Server.Accounting;

namespace Server.Localization
{
	public static class RaceLocalization
	{
		public static string Key( Mobile viewer, string logicalKey, string englishIfMissing )
		{
			if ( logicalKey == null || logicalKey.Length == 0 )
				return englishIfMissing ?? "";

			string lang = AccountLang.GetLanguageCode( viewer != null ? viewer.Account : null );
			string s = StringCatalog.TryResolveByKey( lang, logicalKey );

			if ( s != null && s.Length > 0 )
				return s;

			return englishIfMissing ?? logicalKey;
		}

		public static string KeyDefault( string logicalKey, string englishIfMissing )
		{
			if ( logicalKey == null || logicalKey.Length == 0 )
				return englishIfMissing ?? "";

			string s = StringCatalog.TryResolveByKey( LangConfig.DefaultLanguage, logicalKey );

			if ( s != null && s.Length > 0 )
				return s;

			return englishIfMissing ?? logicalKey;
		}

		public static string KeyFormat( Mobile viewer, string logicalKey, string englishIfMissing, params object[] args )
		{
			string fmt = Key( viewer, logicalKey, englishIfMissing );

			if ( args == null || args.Length == 0 )
				return fmt;

			try
			{
				return string.Format( fmt, args );
			}
			catch
			{
				return englishIfMissing ?? fmt;
			}
		}
	}
}
