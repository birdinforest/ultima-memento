using System;
using Server;
using Server.Localization;
using Server.Mobiles;
using Server.Accounting;
using Server.Network;

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

			// Mobile.Say → PublicOverheadMessage applies StringCatalog.TryResolve per viewer language.
			speaker.Say( english );
		}

		/// <summary>
		/// Resolves a format-string to Chinese (or English fallback), applies string.Format with
		/// the given args, and broadcasts via Say(). Use for combat shouts that include names.
		/// </summary>
		public static void SayLocalizedFormat( Mobile speaker, string englishFmt, params object[] args )
		{
			if ( speaker == null || englishFmt == null || speaker.Map == null )
				return;

			IPooledEnumerable eable = speaker.Map.GetClientsInRange( speaker.Location );

			foreach ( NetState state in eable )
			{
				Mobile m = state.Mobile;

				if ( m == null || !m.CanSee( speaker ) )
					continue;

				string fmt = StringCatalog.Resolve( m.Account, englishFmt );
				string msg = args == null || args.Length == 0 ? fmt : string.Format( fmt, args );

				if ( msg != null && msg.Length > 0 )
					speaker.SayTo( m, msg );
			}

			eable.Free();
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
			string msg = isChinese && zh != null && zh.Length > 0 ? QuestCompositeResolver.ResolveComposite( target, zh ) : english;
			if ( isChinese && zh != null && zh.Length > 0 )
				msg = NpcSpeechTokenZh.ApplyNpcVocabularyTokensToZh( msg );
			speaker.SayTo( target, msg );
		}

		/// <summary>
		/// Broadcasts tavern-style composite speech: each nearby client hears Chinese if their
		/// account is zh-Hans and a parallel <paramref name="zh"/> string was supplied; otherwise
		/// English. When <paramref name="zh"/> is null/empty, zh-Hans accounts still get
		/// <see cref="QuestCompositeResolver"/> + NPC vocab token pass on <paramref name="english"/>.
		/// </summary>
		public static void SayLocalizedComposite( Mobile speaker, string english, string zh )
		{
			if ( speaker == null || speaker.Map == null )
				return;

			bool haveZh = zh != null && zh.Length > 0;

			if ( !haveZh && ( english == null || english.Length == 0 ) )
				return;

			IPooledEnumerable eable = speaker.Map.GetClientsInRange( speaker.Location );

			foreach ( NetState state in eable )
			{
				Mobile m = state.Mobile;

				if ( m == null || !m.CanSee( speaker ) )
					continue;

				string lang = AccountLang.GetLanguageCode( m.Account );
				bool isChinese = AccountLang.IsChinese( lang );

				string msg;

				// Pre-built zh may still embed English place names (e.g. dungeon concatenated in TavernPatrons);
				// run the same fragment pass as English so quest-fragment-zh-table.json applies.
				if ( haveZh && isChinese )
				{
					msg = QuestCompositeResolver.ResolveComposite( m, zh );
					msg = NpcSpeechTokenZh.ApplyNpcVocabularyTokensToZh( msg );
				}
				else if ( haveZh && !isChinese )
					msg = english ?? "";
				else if ( isChinese && english != null && english.Length > 0 )
				{
					msg = QuestCompositeResolver.ResolveComposite( m, english );
					msg = NpcSpeechTokenZh.ApplyNpcVocabularyTokensToZh( msg );
				}
				else
					msg = english ?? "";

				if ( msg == null || msg.Length == 0 )
					continue;

				speaker.SayTo( m, msg );
			}

			eable.Free();
		}
	}
}
