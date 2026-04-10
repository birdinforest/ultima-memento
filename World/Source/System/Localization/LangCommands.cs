using System;
using Server;
using Server.Accounting;
using Server.Commands;

namespace Server.Localization
{
	public static class LangCommands
	{
		public static void Initialize()
		{
			CommandSystem.Register( "Lang", AccessLevel.Player, new CommandEventHandler( Lang_OnCommand ) );
			CommandSystem.Register( "Language", AccessLevel.Player, new CommandEventHandler( Lang_OnCommand ) );
			CommandSystem.Register( "ReloadLang", AccessLevel.Player, new CommandEventHandler( ReloadLang_OnCommand ) );
		}

		private static void Lang_OnCommand( CommandEventArgs e )
		{
			Mobile m = e.Mobile;
			IAccount acct = m.Account;

			if ( acct == null )
			{
				m.SendMessage( "Your account could not be found." );
				return;
			}

			if ( e.Arguments.Length == 0 )
			{
				string cur = AccountLang.GetLanguageCode( acct );
				m.SendMessage( "Your language is {0}. Use [Lang en] or [Lang zh-Hans] to change.", cur );
				return;
			}

			string code = e.Arguments[0].Trim();

			if ( Insensitive.Equals( code, "en" ) || Insensitive.Equals( code, "english" ) )
				code = "en";
			else if ( Insensitive.Equals( code, "zh" ) || Insensitive.Equals( code, "zh-cn" ) || Insensitive.Equals( code, "cn" ) || Insensitive.Equals( code, "chinese" ) )
				code = "zh-Hans";

			if ( code != "en" && code != "zh-Hans" )
			{
				m.SendMessage( "Unknown language. Use en or zh-Hans." );
				return;
			}

			AccountLang.SetLanguageCode( acct, code );
			m.SendMessage( "Language set to {0}.", code );

			try
			{
				World.Save( false, false );
			}
			catch
			{
			}
		}

		private static void ReloadLang_OnCommand( CommandEventArgs e )
		{
			StringCatalog.Reload();
			e.Mobile.SendMessage( "Localization files reloaded." );
		}
	}
}
