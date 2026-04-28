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
					sb.Append( ResolveTextSegment( m, html.Substring( last, len ) ) );

				sb.Append( brMatch.Value );
				last = brMatch.Index + brMatch.Length;
			}

			if ( last < html.Length )
				sb.Append( ResolveTextSegment( m, html.Substring( last ) ) );

			return sb.ToString();
		}

		private static string ResolvePlainText( Mobile m, string s )
		{
			string lang = AccountLang.GetLanguageCode( m.Account );
			string r = StringCatalog.TryResolve( lang, s ) ?? s;

			if ( AccountLang.IsChinese( lang ) )
				r = QuestCompositeResolver.ResolveComposite( m, r );

			return r;
		}

		private static string ResolveTextSegment( Mobile m, string segment )
		{
			if ( segment == null || segment.Length == 0 )
				return segment;

			string original = segment;
			string t = segment.Trim();

			if ( t.Length == 0 )
				return original;

			string lang = AccountLang.GetLanguageCode( m.Account );
			string r = StringCatalog.TryResolve( lang, t ) ?? t;

			if ( AccountLang.IsChinese( lang ) )
				r = QuestCompositeResolver.ResolveComposite( m, r );

			// Unchanged: keep original whitespace if only whitespace / no mapping.
			if ( r == t )
				return original;

			return r;
		}
	}
}
