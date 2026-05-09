using Server;
using Server.Accounting;

namespace Server.Localization
{
	/// <summary>Logical-key strings for <see cref="Server.Mobiles.ShardGreeter"/> / <see cref="Server.Gumps.GypsyTarotGump"/> (see <c>Data/Localization/en/shard-greeter.json</c>).</summary>
	public static class ShardGreeterLocalization
	{
		public static string GreeterKey( Mobile viewer, string logicalKey, string englishIfMissing )
		{
			if ( logicalKey == null || logicalKey.Length == 0 )
				return englishIfMissing ?? "";

			string lang = AccountLang.GetLanguageCode( viewer != null ? viewer.Account : null );
			string s = StringCatalog.TryResolveByKey( lang, logicalKey );

			if ( s != null && s.Length > 0 )
				return s;

			return englishIfMissing ?? logicalKey;
		}

		public static string GreeterKeyFormat( Mobile viewer, string logicalKey, string englishIfMissing, params object[] args )
		{
			string fmt = GreeterKey( viewer, logicalKey, englishIfMissing );

			if ( args == null || args.Length == 0 )
				return fmt;

			try
			{
				return string.Format( fmt, args );
			}
			catch
			{
				return englishIfMissing ?? fmt;
			}
		}

		/// <summary>Polish procedural fragments (race sentences, place names) for zh-Hans after assembling English glue.</summary>
		public static string PolishCompositeIfZh( Mobile viewer, string assembled )
		{
			if ( viewer == null || assembled == null || assembled.Length == 0 )
				return assembled;

			string lang = AccountLang.GetLanguageCode( viewer.Account );

			if ( !AccountLang.IsChinese( lang ) )
				return assembled;

			QuestCompositeResolver.EnsureInitialized();
			string s = QuestCompositeResolver.ResolveComposite( viewer, assembled );
			return NpcSpeechTokenZh.ApplyNpcVocabularyTokensToZh( s );
		}
	}
}
