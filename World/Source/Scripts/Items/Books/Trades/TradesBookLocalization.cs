using Server.Localization;
using Server.Mobiles;

namespace Server.Items
{
	internal static class TradesBookLocalization
	{
		public static string Resolve( Mobile from, string text )
		{
			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		public static string Body( Mobile from, string color, string text )
		{
			return @"<BODY><BASEFONT Color=" + color + ">" + Resolve( from, text ) + "</BASEFONT></BODY>";
		}
	}
}
