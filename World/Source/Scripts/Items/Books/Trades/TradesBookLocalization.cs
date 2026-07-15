using Server.Localization;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Items
{
	/// <summary>
	/// Shared Resolve helpers for Books/ gumps (trade books, learn books, bulletin boards).
	/// Extractor captures string literals in <c>Resolve( from, "..." )</c> / <c>Body( from, color, "..." )</c>.
	/// </summary>
	internal static class TradesBookLocalization
	{
		public static string Resolve( Mobile from, string text )
		{
			if ( from == null )
				return text;
			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		public static string Resolve( IAccount account, string text )
		{
			string lang = AccountLang.GetLanguageCode( account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		public static string ResolveFormat( Mobile from, string format, params object[] args )
		{
			if ( from == null )
				return string.Format( format, args );
			return StringCatalog.ResolveFormat( from.Account, format, args );
		}

		public static string Body( Mobile from, string color, string text )
		{
			return @"<BODY><BASEFONT Color=" + color + ">" + Resolve( from, text ) + "</BASEFONT></BODY>";
		}

		public static string BodyRaw( string color, string alreadyResolvedText )
		{
			return @"<BODY><BASEFONT Color=" + color + ">" + alreadyResolvedText + "</BASEFONT></BODY>";
		}
	}
}
