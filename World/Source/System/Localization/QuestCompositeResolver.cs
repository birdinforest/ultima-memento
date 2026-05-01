using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Mobiles;

namespace Server.Localization
{
	/// <summary>
	/// Replacement of known English fragments inside dynamic quest text (lands, dungeons,
	/// creature labels) using <c>quest-fragment-zh-table.json</c> first, then <see cref="StringCatalog"/>.
	/// Terms from <c>quest-composite-terms-order.txt</c> are applied <b>longest-first</b> (stable tie-break: file order) so
	/// phrases like <c>an ancient wyrm sleeping below Dungeon Hate</c> replace as a whole before bare <c>Dungeon Hate</c>.
	/// </summary>
	public static class QuestCompositeResolver
	{
		private static string[] s_OrderedTerms;
		private static Dictionary<string, string> s_FragmentZh;
		private static bool s_LoadAttempted;

		public static void EnsureInitialized()
		{
			if ( s_LoadAttempted )
				return;

			s_LoadAttempted = true;

			s_FragmentZh = new Dictionary<string, string>( StringComparer.Ordinal );

			string tablePath = Path.Combine( Core.BaseDirectory, "Data/Localization/quest-fragment-zh-table.json" );

			if ( File.Exists( tablePath ) )
			{
				try
				{
					string json = File.ReadAllText( tablePath );
					SimpleJsonObject.ParseStringProperties( json, s_FragmentZh );
				}
				catch
				{
				}
			}

			var list = new List<string>();
			string path = Path.Combine( Core.BaseDirectory, "Data/Localization/quest-composite-terms-order.txt" );

			if ( File.Exists( path ) )
			{
				foreach ( string line in File.ReadAllLines( path ) )
				{
					string t = line.Trim();

					if ( t.Length == 0 || t[0] == '#' )
						continue;

					// Order file uses a line "Some " (trimmed to "Some") before adventurer titles:
					// restore trailing space so replacement targets "Some bard" and not "Someone".
					if ( t == "Some" )
						t = "Some ";

					list.Add( t );
				}
			}

			// Longer multi-word fragments must win over shorter contained keys (e.g. dungeon names).
			if ( list.Count > 1 )
			{
				var indexed = new List<KeyValuePair<int, string>>( list.Count );
				for ( int li = 0; li < list.Count; ++li )
					indexed.Add( new KeyValuePair<int, string>( li, list[li] ) );
				indexed.Sort( delegate( KeyValuePair<int, string> a, KeyValuePair<int, string> b )
				{
					int cmp = b.Value.Length.CompareTo( a.Value.Length );
					if ( cmp != 0 )
						return cmp;
					return a.Key.CompareTo( b.Key );
				} );
				for ( int li = 0; li < indexed.Count; ++li )
					list[li] = indexed[li].Value;
			}

			s_OrderedTerms = list.ToArray();
		}

		private static bool IsAsciiLetter( char c )
		{
			return ( c >= 'a' && c <= 'z' ) || ( c >= 'A' && c <= 'Z' );
		}

		/// <summary>
		/// Single-token ASCII fragments (e.g. <c>ore</c>, <c>demon</c>) must not match inside longer English words
		/// (<c>explorer</c>, <c>demonic</c>). Multi-word phrases use plain replacement.
		/// </summary>
		private static bool NeedsAsciiWordBoundaries( string en )
		{
			if ( en == null || en.Length == 0 )
				return false;
			if ( en.IndexOf( ' ' ) >= 0 )
				return false;
			for ( int i = 0; i < en.Length; ++i )
			{
				char c = en[i];
				if ( c > 127 || !char.IsLetter( c ) )
					return false;
			}
			return true;
		}

		private static string ReplaceFragment( string work, string en, string zh, StringComparison cmp )
		{
			if ( work == null || en == null || en.Length == 0 || work.Length < en.Length )
				return work;
			if ( zh == null )
				zh = "";
			bool wholeWord = NeedsAsciiWordBoundaries( en );
			int from = 0;
			System.Text.StringBuilder sb = new System.Text.StringBuilder( work.Length + 32 );
			while ( from <= work.Length - en.Length )
			{
				int pos = work.IndexOf( en, from, cmp );
				if ( pos < 0 )
				{
					sb.Append( work, from, work.Length - from );
					break;
				}
				sb.Append( work, from, pos - from );
				bool ok = true;
				if ( wholeWord )
				{
					if ( pos > 0 && IsAsciiLetter( work[pos - 1] ) )
						ok = false;
					int after = pos + en.Length;
					if ( after < work.Length && IsAsciiLetter( work[after] ) )
						ok = false;
				}
				if ( ok )
				{
					sb.Append( zh );
					from = pos + en.Length;
				}
				else
				{
					sb.Append( work[pos] );
					from = pos + 1;
				}
			}
			return sb.ToString();
		}

		private static string ApplyCompositeReplacements( string text, string catalogLang )
		{
			if ( text == null || text.Length == 0 )
				return text;

			EnsureInitialized();

			if ( s_OrderedTerms == null || s_OrderedTerms.Length == 0 )
				return PolishChineseCompositeGlue( text );

			string work = text;
			StringComparison cmp = StringComparison.Ordinal;

			for ( int i = 0; i < s_OrderedTerms.Length; ++i )
			{
				string en = s_OrderedTerms[i];

				if ( en == null || en.Length == 0 || !work.Contains( en ) )
					continue;

				string zh = null;

				if ( s_FragmentZh != null && s_FragmentZh.TryGetValue( en, out zh ) && zh != null && zh.Length > 0 && zh != en )
				{
					work = ReplaceFragment( work, en, zh, cmp );
					continue;
				}

				zh = StringCatalog.TryResolve( catalogLang, en );

				if ( zh != null && zh.Length > 0 && zh != en )
					work = ReplaceFragment( work, en, zh, cmp );
			}

			return PolishChineseCompositeGlue( work );
		}

		public static string ResolveComposite( Mobile m, string text )
		{
			if ( m == null || text == null || text.Length == 0 )
				return text;

			string lang = AccountLang.GetLanguageCode( m.Account );

			if ( !AccountLang.IsChinese( lang ) )
				return text;

			return ApplyCompositeReplacements( text, lang );
		}

		/// <summary>
		/// Fragment + StringCatalog zh-Hans pass without a viewer <see cref="Mobile"/> (NPCs have no account).
		/// Used to pre-build parallel Chinese for tavern broadcast lines; English clients still use the English string from <see cref="Server.Mobiles.CitizenLocalization.SayLocalizedComposite"/>.
		/// </summary>
		public static string ResolveCompositeToZhHans( string text )
		{
			if ( text == null || text.Length == 0 )
				return text;

			return ApplyCompositeReplacements( text, "zh-Hans" );
		}

		/// <summary>
		/// After English fragments are swapped to Chinese, normalize leftover English glue in
		/// citizen/tavern lines: cult-item pattern <c>the 'Name'</c> → corner quotes, <c>and</c> → 和.
		/// </summary>
		private static string PolishChineseCompositeGlue( string work )
		{
			if ( work == null || work.Length == 0 )
				return work;

			// Item closing quote + conjunction (must run before bare " and ").
			work = work.Replace( "' and ", "」和 " );
			work = work.Replace( "'和 ", "」和 " );
			work = work.Replace( " and ", " 和 " );
			work = work.Replace( " the '", "「" );
			work = work.Replace( " The '", "「" );

			return work;
		}
	}
}
