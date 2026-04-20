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
				return text;

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

			return work;
		}
	}
}
