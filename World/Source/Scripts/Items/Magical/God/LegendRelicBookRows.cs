using System;
using Server.Localization;

namespace Server.Items
{
	/// <summary>
	/// Shared row labels for <see cref="LegendsBook"/> / <see cref="ManualOfItems"/> gump lists
	/// (<c>god.legendbook.row.*</c> in <c>zh-Hans/legend-book-rows.json</c>).
	/// </summary>
	public static class LegendRelicBookRows
	{
		public static string LocalizedRowLabel( Mobile viewer, int artifactIndex1Based, string englishLabel )
		{
			if ( viewer == null || string.IsNullOrEmpty( englishLabel ) )
				return englishLabel;
			string lang = AccountLang.GetLanguageCode( viewer.Account );
			if ( !AccountLang.IsChinese( lang ) )
				return englishLabel;
			string key = "god.legendbook.row." + artifactIndex1Based.ToString( "D3" );
			string zh = StringCatalog.TryResolveByKey( lang, key );
			return string.IsNullOrEmpty( zh ) ? englishLabel : zh;
		}
	}
}
