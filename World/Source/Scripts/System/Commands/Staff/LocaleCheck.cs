using Server;
using Server.Commands;
using Server.Localization;

namespace Server.Commands
{
	/// <summary>
	/// Staff command: [localecheck — reports the player's active language and does a
	/// live round-trip test against the StringCatalog, confirming localization is wired.
	/// Usage (in-game as GM/Owner): [localecheck
	/// </summary>
	public class LocaleCheck
	{
		public static void Initialize()
		{
			CommandSystem.Register( "localecheck", AccessLevel.GameMaster, new CommandEventHandler( OnCommand ) );
			CommandSystem.Register( "lc", AccessLevel.GameMaster, new CommandEventHandler( OnCommand ) );
		}

		private static void OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			string lang = AccountLang.GetLanguageCode( from.Account );
			bool isChinese = AccountLang.IsChinese( lang );

			from.SendMessage( 0x5A, "=== Locale Diagnostics ===" );
			from.SendMessage( 0x5A, "Account language tag : " + lang );
			from.SendMessage( 0x5A, "IsChinese            : " + isChinese );
			from.SendMessage( 0x5A, "DefaultLanguage      : " + LangConfig.DefaultLanguage );

			// Test a few known catalog strings
			string[] testEN = new string[]
			{
				"a daemon",
				"a lich lord",
				"an orc captain",
				"Dungeon Shame",
				"the Land of Sosaria",
				"Ahhh...",
			};

			from.SendMessage( 0x5A, "--- Catalog round-trip (EN → ZH) ---" );

			foreach ( string en in testEN )
			{
				string zh = StringCatalog.TryResolve( "zh-Hans", en );
				bool ok = zh != null && zh != en;
				from.SendMessage( ok ? 0x5A : 0x25, ( ok ? "✓" : "✗" ) + " \"" + en + "\" → " + ( zh ?? "(not found)" ) );
			}

			from.SendMessage( 0x5A, "==========================" );
		}
	}
}
