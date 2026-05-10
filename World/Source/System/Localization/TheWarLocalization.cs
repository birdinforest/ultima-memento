using Server.Mobiles;

namespace Server.Localization
{
	/// <summary>Logical keys for <c>WarEffortRecruiter</c> / Supporting the War quest (<c>Data/Localization/*/thewar-quest.json</c>).</summary>
	public static class TheWarLocalization
	{
		public static string RecruiterShout( Mobile viewer, int line, string englishIfMissing )
		{
			string key = "thewar.recruiter.shout." + line.ToString();

			if ( viewer == null )
				return englishIfMissing ?? "";

			string lang = AccountLang.GetLanguageCode( viewer.Account );
			string s = StringCatalog.TryResolveByKey( lang, key );

			if ( s != null && s.Length > 0 )
				return s;

			return englishIfMissing ?? key;
		}
	}
}
