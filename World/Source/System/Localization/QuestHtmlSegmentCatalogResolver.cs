using System;
using System.Text;
using System.Text.RegularExpressions;
using Server.Mobiles;

namespace Server.Localization
{
	/// <summary>
	/// Many ML quest <see cref="TextDefinition.String"/> values concatenate HTML fragments that each have their own
	/// <c>StringCatalog</c> key (e.g. two paragraphs with <c>&lt;br&gt;&lt;br&gt;</c> between). A hash of the full HTML
	/// does not match any key, so the description stays English. This resolver finds runs of <c>&lt;br&gt;</c> tags,
	/// maps each text segment to the catalog, then rejoins with the original markup.
	/// </summary>
	public static class QuestHtmlSegmentCatalogResolver
	{
		private static readonly Regex s_BrRun = new Regex( @"(<br\s*/?>\s*)+", RegexOptions.IgnoreCase | RegexOptions.Compiled );

		/// <summary>Resolve <paramref name="html"/> using <see cref="StringCatalog"/> and <see cref="QuestCompositeResolver"/> on each inter-<c>br</c> segment.</summary>
		public static string Resolve( Mobile m, string html )
		{
			if ( m == null || string.IsNullOrEmpty( html ) )
				return html;

			if ( html.IndexOf( "<br", StringComparison.OrdinalIgnoreCase ) < 0 )
				return ResolvePlainText( m, html );

			MatchCollection matches = s_BrRun.Matches( html );

			if ( matches == null || matches.Count == 0 )
				return ResolvePlainText( m, html );

			StringBuilder sb = new StringBuilder( html.Length * 2 );
			int last = 0;

			for ( int i = 0; i < matches.Count; i++ )
			{
				Match brMatch = matches[i];
				int len = brMatch.Index - last;

				if ( len > 0 )
				{
					SegmentResolveResult res = ResolveTextSegmentWithFollowingBr( m, html.Substring( last, len ), brMatch.Value );

					sb.Append( res.Text );

					if ( !res.ConsumedFollowingBrRun )
						sb.Append( brMatch.Value );
				}
				else
					sb.Append( brMatch.Value );

				last = brMatch.Index + brMatch.Length;
			}

			if ( last < html.Length )
				sb.Append( ResolveTextSegmentWithFollowingBr( m, html.Substring( last ), null ).Text );

			return sb.ToString();
		}

		private struct SegmentResolveResult
		{
			public string Text;
			/// <summary>True when the catalog match was for <c>trimmedText + followingBrRun</c> (translation already includes that markup).</summary>
			public bool ConsumedFollowingBrRun;
		}

		private static string ResolvePlainText( Mobile m, string s )
		{
			string lang = AccountLang.GetLanguageCode( m.Account );
			string r = StringCatalog.TryResolve( lang, s ) ?? s;

			if ( AccountLang.IsChinese( lang ) )
				r = QuestCompositeResolver.ResolveComposite( m, r );

			return r;
		}

		/// <summary>
		/// Extractor keys often include the trailing <c>&lt;br&gt;</c> run that follows a segment in source (e.g. one Append literal ends with <c>&lt;br&gt;&lt;br&gt;</c>).
		/// Splitting on <c>&lt;br&gt;</c> alone drops that suffix from the hashed English, so we try <paramref name="trimmedText"/> + <paramref name="followingBrRun"/> first.
		/// </summary>
		private static SegmentResolveResult ResolveTextSegmentWithFollowingBr( Mobile m, string segment, string followingBrRun )
		{
			if ( segment == null || segment.Length == 0 )
				return new SegmentResolveResult { Text = segment, ConsumedFollowingBrRun = false };

			string original = segment;
			string t = segment.Trim();

			if ( t.Length == 0 )
				return new SegmentResolveResult { Text = original, ConsumedFollowingBrRun = false };

			string lang = AccountLang.GetLanguageCode( m.Account );

			if ( !string.IsNullOrEmpty( followingBrRun ) )
			{
				string compound = t + followingBrRun;
				string rCompound = StringCatalog.TryResolve( lang, compound );

				if ( rCompound != null )
				{
					if ( AccountLang.IsChinese( lang ) )
						rCompound = QuestCompositeResolver.ResolveComposite( m, rCompound );

					return new SegmentResolveResult { Text = rCompound, ConsumedFollowingBrRun = true };
				}
			}

			string r = StringCatalog.TryResolve( lang, t ) ?? t;

			if ( AccountLang.IsChinese( lang ) )
				r = QuestCompositeResolver.ResolveComposite( m, r );

			if ( r == t )
				return new SegmentResolveResult { Text = original, ConsumedFollowingBrRun = false };

			return new SegmentResolveResult { Text = r, ConsumedFollowingBrRun = false };
		}
	}
}
