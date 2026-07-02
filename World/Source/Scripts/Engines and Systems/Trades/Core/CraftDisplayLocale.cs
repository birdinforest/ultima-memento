using Server;
using Server.Localization;

namespace Server.Engines.Craft
{
	/// <summary>
	/// Resolves craft menu strings stored as logical keys (dot-separated, no spaces).
	/// Plain English literals pass through unchanged.
	/// </summary>
	public static class CraftDisplayLocale
	{
		public static bool IsLogicalKey( string text )
		{
			if ( text == null || text.Length == 0 )
				return false;

			if ( text.IndexOf( ' ' ) >= 0 || text.StartsWith( "<" ) )
				return false;

			return text.IndexOf( '.' ) >= 0;
		}

		public static string Resolve( Mobile from, string textOrKey )
		{
			if ( from == null || !IsLogicalKey( textOrKey ) )
				return textOrKey;

			return StringCatalog.ResolveByKey( from.Account, textOrKey );
		}
	}
}
