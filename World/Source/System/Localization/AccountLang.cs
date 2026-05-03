using System;
using Server.Accounting;

namespace Server.Localization
{
	public static class AccountLang
	{
		public const string TagName = "Language";

		public static string GetLanguageCode( IAccount acct )
		{
			if ( acct == null )
				return LangConfig.DefaultLanguage;

			string code = acct.GetTag( TagName );

			if ( string.IsNullOrEmpty( code ) )
				return LangConfig.DefaultLanguage;

			return code.Trim();
		}

		public static void SetLanguageCode( IAccount acct, string code )
		{
			if ( acct == null || string.IsNullOrEmpty( code ) )
				return;

			acct.SetTag( TagName, code.Trim() );
		}

		public static bool IsChinese( string code )
		{
			if ( code == null )
				return false;

			return Insensitive.Equals( code, "zh-Hans" ) || Insensitive.Equals( code, "zh-CN" ) || Insensitive.Equals( code, "zh" );
		}
	}
}
