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
	/// Iteration order follows <c>quest-composite-terms-order.txt</c> (list longer phrases before shorter substrings where needed).
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

			s_OrderedTerms = list.ToArray();
		}

		public static string ResolveComposite( Mobile m, string text )
		{
			if ( m == null || text == null || text.Length == 0 )
				return text;

			string lang = AccountLang.GetLanguageCode( m.Account );

			if ( !AccountLang.IsChinese( lang ) )
				return text;

			EnsureInitialized();

			if ( s_OrderedTerms == null || s_OrderedTerms.Length == 0 )
				return PolishChineseCompositeGlue( text );

			string work = text;

			for ( int i = 0; i < s_OrderedTerms.Length; ++i )
			{
				string en = s_OrderedTerms[i];

				if ( en == null || en.Length == 0 || !work.Contains( en ) )
					continue;

				string zh = null;

				if ( s_FragmentZh != null && s_FragmentZh.TryGetValue( en, out zh ) && zh != null && zh.Length > 0 && zh != en )
				{
					work = work.Replace( en, zh );
					continue;
				}

				zh = StringCatalog.TryResolve( lang, en );

				if ( zh != null && zh.Length > 0 && zh != en )
					work = work.Replace( en, zh );
			}

			return PolishChineseCompositeGlue( work );
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
