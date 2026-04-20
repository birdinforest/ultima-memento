using System;
using Server;
using Server.Localization;
using Server.Mobiles;
using Server.Accounting;

namespace Server.Mobiles
{
	/// <summary>
	/// Shared localization helpers for Civilized NPC speech.
	/// SayLocalized: resolves to Chinese (or English fallback) and broadcasts via Say().
	/// SayLocalizedComposite: accepts a pre-built zh string (e.g. from parallel zh assembly)
	///   and broadcasts whichever is available.
	/// </summary>
	public static class CitizenLocalization
	{
		/// <summary>
		/// Resolves the English literal to Chinese via StringCatalog and broadcasts to all
		/// nearby players using Say(). Falls back to English if no translation exists.
		/// </summary>
		public static void SayLocalized( Mobile speaker, string english )
		{
			if ( speaker == null || english == null || english.Length == 0 )
				return;

			string zh = Server.Localization.StringCatalog.TryResolve( "zh-Hans", english );
			speaker.Say( zh != null && zh.Length > 0 ? zh : english );
		}

		/// <summary>
		/// Resolves a format-string to Chinese (or English fallback), applies string.Format with
		/// the given args, and broadcasts via Say(). Use for combat shouts that include names.
		/// </summary>
		public static void SayLocalizedFormat( Mobile speaker, string englishFmt, params object[] args )
		{
			if ( speaker == null || englishFmt == null )
				return;

			string fmt = Server.Localization.StringCatalog.TryResolve( "zh-Hans", englishFmt );
			if ( fmt == null || fmt.Length == 0 )
				fmt = englishFmt;

			speaker.Say( string.Format( fmt, args ) );
		}

		/// <summary>
		/// Resolves the English literal to the target player's language and sends via SayTo().
		/// Falls back to English if no translation exists.
		/// </summary>
		public static void SayToLocalized( Mobile speaker, Mobile target, string english )
		{
			if ( speaker == null || target == null || english == null )
				return;

			string resolved = Server.Localization.StringCatalog.Resolve( target.Account, english );
			speaker.SayTo( target, resolved );
		}

		/// <summary>
		/// Resolves a format-string to the target player's language, applies string.Format, and sends via SayTo().
		/// </summary>
		public static void SayToLocalizedFormat( Mobile speaker, Mobile target, string englishFmt, params object[] args )
		{
			if ( speaker == null || target == null || englishFmt == null )
				return;

			string fmt = Server.Localization.StringCatalog.Resolve( target.Account, englishFmt );
			speaker.SayTo( target, string.Format( fmt, args ) );
		}

		/// <summary>
		/// Sends the pre-built zh string to the target if their account is Chinese, otherwise English.
		/// Used for composite SayTo messages (GypsyLady fortune readings, etc.) where the Chinese
		/// version is assembled in parallel code paths.
		/// </summary>
		public static void SayToLocalizedComposite( Mobile speaker, Mobile target, string english, string zh )
		{
			if ( speaker == null || target == null )
				return;

			string lang = AccountLang.GetLanguageCode( target.Account );
			bool isChinese = AccountLang.IsChinese( lang );
			speaker.SayTo( target, isChinese && zh != null && zh.Length > 0 ? zh : english );
		}

		/// <summary>
		/// Broadcasts the pre-built zh string if non-empty, otherwise the English string.
		/// Used for procedurally assembled speech (TavernPatrons, etc.) where the Chinese
		/// version is constructed in parallel code (same random branch, different language).
		/// </summary>
		public static void SayLocalizedComposite( Mobile speaker, string english, string zh )
		{
			if ( speaker == null )
				return;

			if ( zh != null && zh.Length > 0 )
				speaker.Say( zh );
			else if ( english != null && english.Length > 0 )
				speaker.Say( english );
		}
	}
}
