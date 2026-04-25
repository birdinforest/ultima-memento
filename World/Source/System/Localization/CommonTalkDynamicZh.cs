using System;
using System.Text.RegularExpressions;

namespace Server.Localization
{
	/// <summary>
	/// Full-sentence zh-Hans for <see cref="Server.Mobiles.TavernPatrons.CommonTalk"/> lines that concatenate
	/// city / dungeon / adventurer / job fragments (no single English catalog key). Runs before
	/// <see cref="QuestCompositeResolver.ResolveComposite"/> on the whole string so captures stay English for lookup.
	/// </summary>
	public static class CommonTalkDynamicZh
	{
		private sealed class Rule
		{
			public Regex Re;
			public int NGroups;
			public string Template;
		}

		private static Rule[] s_Rules;
		private static bool s_Init;

		private static void EnsureRules()
		{
			if ( s_Init )
				return;

			s_Init = true;

			// (pattern, format template, number of capturing groups). Longer / more specific first.
			var raw = new[]
			{
				(@"^some (.+) that had a tinker in (.+) make a golem with a dark core$", "传闻有位「{0}」在「{1}」雇工匠以黑暗核心铸成一尊魔像。", 2),
				(@"^some (.+) that was killed by a cyclops' eye$", "传闻有位「{0}」丧命于独眼巨人之瞳。", 1),
				(@"^some (.+) that was killed by elemental grues$", "传闻有位「{0}」命丧元素格鲁之手。", 1),
				// Job from RandomThings.GetRandomJob() may be multi-word (e.g. "stable master"); do not use [a-z]+ only.
				(@"^some (.+?) solving the mystery of the Skull Gate$", "传闻有位「{0}」正试图揭开颅骨门之谜。", 1),
				(@"^some (.+?) solving the mystery of the Serpent Pillars$", "传闻有位「{0}」正试图揭开巨蛇柱之谜。", 1),
				(@"^someone buried with great treasure in the graveyard in (.+)$", "传闻「{0}」的墓园中，有人携重宝藏于坟冢。", 1),
				(@"^a demilich dwelling below (.+)$", "传闻「{0}」地下盘踞着一名半巫妖。", 1),
				(@"^some (.+?) selling artifacts in (.+)$", "传闻在「{1}」，有位「{0}」在兜售神器。", 2),
				(@"^someone who killed the (.+?) in (.+)$", "传闻在「{1}」，有人杀害了那位「{0}」。", 2),
				(@"^an ancient book of magic buried in (.+)$", "传闻一册古法魔典埋藏于「{0}」。", 1),
				(@"^a wizard that sails the Isles of Dread, selling rare spells$", "传闻有位法师驾舟于恐惧群岛，贩卖珍奇法术。", 0),
				(@"^a blacksmith in (.+) that makes weapons out of mithril$", "传闻「{0}」有位铁匠能以秘银打造兵器。", 1),
				(@"^Zorn living in (.+)$", "传闻佐恩栖身于「{0}」。", 1),
				(@"^a black sword resting in (.+)$", "传闻一把乌黑长剑静置在「{0}」。", 1),
			};

			var list = new System.Collections.Generic.List<Rule>();

			for ( int i = 0; i < raw.Length; ++i )
			{
				var rx = new Regex( raw[i].Item1, RegexOptions.Compiled | RegexOptions.CultureInvariant );
				list.Add( new Rule { Re = rx, NGroups = raw[i].Item3, Template = raw[i].Item2 } );
			}

			s_Rules = list.ToArray();
		}

		private static string MapTombKind( string k )
		{
			switch ( k )
			{
				case "tomb": return "墓冢";
				case "crypt": return "墓穴";
				case "treasure": return "秘藏";
				case "artifact": return "古器";
				case "remains": return "遗骸";
				default: return k;
			}
		}

		private static string MapLeadKind( string k )
		{
			switch ( k )
			{
				case "map": return "地图";
				case "tablet": return "石板";
				case "scroll": return "卷轴";
				case "book": return "典籍";
				case "clue": return "线索";
				default: return k;
			}
		}

		private static string MapLootKind( string k )
		{
			switch ( k )
			{
				case "gold": return "黄金";
				case "treasure": return "宝藏";
				case "gems": return "宝石";
				case "jewels": return "珠宝";
				case "riches": return "财富";
				case "crystals": return "水晶";
				default: return k;
			}
		}

		private static string MapBeingVerb( string k )
		{
			switch ( k )
			{
				case "destroyed": return "毁灭";
				case "ruined": return "沦为废墟";
				case "devastated": return "夷为平地";
				case "lost": return "失落";
				default: return k;
			}
		}

		private static string MapSeaTradeRole( string k )
		{
			switch ( k )
			{
				case "fisherman": return "渔夫";
				case "ship builder": return "造船匠";
				case "pirate": return "海盗";
				case "sailor": return "水手";
				default: return k;
			}
		}

		private static string MapWyrmKind( string k )
		{
			switch ( k )
			{
				case "hydra": return "九头蛇";
				case "dragon": return "巨龙";
				case "drake": return "双足飞龙";
				case "wyrm": return "古龙";
				default: return k;
			}
		}

		/// <summary>Resolve a substring for composite Chinese; when <paramref name="viewer"/> is null, use broadcast zh-Hans (NPC tavern lines).</summary>
		private static string CompositePart( Mobile viewer, string fragment )
		{
			if ( fragment == null )
				return fragment;
			if ( viewer != null )
				return QuestCompositeResolver.ResolveComposite( viewer, fragment );
			return QuestCompositeResolver.ResolveCompositeToZhHans( fragment );
		}

		private static string TranslateTavernLogJobPrefix( Mobile m, string jobPhrase )
		{
			if ( jobPhrase == null )
				return "";
			string p = jobPhrase.Trim();
			Match jm = Regex.Match( p, @"^(An|The|A)\s+(.+)$", RegexOptions.CultureInvariant );
			if ( !jm.Success )
				return CompositePart( m, p );
			string core = jm.Groups[2].Value.Trim();
			string low = core.ToLowerInvariant();
			string zh = NpcSpeechTokenZh.TranslateAdventurerZh( low );
			if ( zh == null || zh.Length == 0 || zh.Equals( low, StringComparison.OrdinalIgnoreCase ) )
				zh = CompositePart( m, core );
			return zh;
		}

		private static string MapWeWillVerb( string v )
		{
			switch ( v )
			{
				case "look for": return "寻找";
				case "search for": return "搜寻";
				case "find": return "找到";
				case "seek out": return "寻访";
				case "try to find": return "设法找到";
				case "ambush": return "伏击";
				case "surprise": return "奇袭";
				case "try to ambush": return "试图伏击";
				case "try to capture": return "试图活捉";
				default: return v;
			}
		}

		private static string AdventurerKindZh( Mobile m, string low )
		{
			string zh = NpcSpeechTokenZh.TranslateAdventurerZh( low );
			if ( zh == null || zh.Length == 0 || zh.Equals( low, StringComparison.OrdinalIgnoreCase ) )
				return low;
			return zh;
		}

		private static bool TryClassifyCitizenRareMixPreface( string pref, out int pclass, out string advLow )
		{
			pclass = -1;
			advLow = null;
			if ( pref == "I found" )
			{
				pclass = 0;
				return true;
			}

			if ( pref == "We found" )
			{
				pclass = 1;
				return true;
			}

			if ( pref == "I heard rumours about" || pref == "I heard rumors about" )
			{
				pclass = 2;
				return true;
			}

			if ( pref == "We heard rumours about" || pref == "We heard rumors about" )
			{
				pclass = 3;
				return true;
			}

			if ( pref == "I heard a story about" )
			{
				pclass = 4;
				return true;
			}

			if ( pref == "We heard a story about" )
			{
				pclass = 5;
				return true;
			}

			if ( pref == "I overheard someone tell of" )
			{
				pclass = 6;
				return true;
			}

			if ( pref == "We overheard someone tell of" )
			{
				pclass = 7;
				return true;
			}

			Match sm = Regex.Match( pref, @"^Some ([a-z]+) found$", RegexOptions.CultureInvariant );
			if ( sm.Success )
			{
				pclass = 8;
				advLow = sm.Groups[1].Value;
				return true;
			}

			sm = Regex.Match( pref, @"^Some ([a-z]+) heard rumou?rs about$", RegexOptions.CultureInvariant );
			if ( sm.Success )
			{
				pclass = 9;
				advLow = sm.Groups[1].Value;
				return true;
			}

			sm = Regex.Match( pref, @"^Some ([a-z]+) heard a story about$", RegexOptions.CultureInvariant );
			if ( sm.Success )
			{
				pclass = 10;
				advLow = sm.Groups[1].Value;
				return true;
			}

			sm = Regex.Match( pref, @"^Some ([a-z]+) overheard another tell of$", RegexOptions.CultureInvariant );
			if ( sm.Success )
			{
				pclass = 11;
				advLow = sm.Groups[1].Value;
				return true;
			}

			sm = Regex.Match( pref, @"^Some ([a-z]+) is spreading rumou?rs about$", RegexOptions.CultureInvariant );
			if ( sm.Success )
			{
				pclass = 12;
				advLow = sm.Groups[1].Value;
				return true;
			}

			sm = Regex.Match( pref, @"^Some ([a-z]+) is telling tales about$", RegexOptions.CultureInvariant );
			if ( sm.Success )
			{
				pclass = 13;
				advLow = sm.Groups[1].Value;
				return true;
			}

			return false;
		}

		private static string MapRareMixSentence( Mobile m, string prefRaw, string whatEn, string whereEn, int clauseKind )
		{
			if ( !TryClassifyCitizenRareMixPreface( prefRaw, out int pclass, out string advLow ) )
				return null;

			string w = CompositePart( m, whatEn.Trim() );
			string y = CompositePart( m, whereEn.Trim() );
			string mid;
			switch ( clauseKind )
			{
				case 0:
					mid = "在「" + y + "」或许能找到「" + w + "」";
					break;
				case 1:
					mid = "若要寻得「" + w + "」，须往「" + y + "」";
					break;
				case 2:
					mid = "若在「" + y + "」搜寻，或许能觅得「" + w + "」";
					break;
				default:
					return null;
			}

			string advZh = advLow != null ? AdventurerKindZh( m, advLow ) : null;
			switch ( pclass )
			{
				case 0:
					return "我听说" + mid + "。";
				case 1:
					return "我们听说" + mid + "。";
				case 2:
					return "有传言称" + mid + "。";
				case 3:
					return "我们听人说" + mid + "。";
				case 4:
					return "我听了一个故事，说" + mid + "。";
				case 5:
					return "我们听了一个故事，说" + mid + "。";
				case 6:
					return "我无意中听说" + mid + "。";
				case 7:
					return "我们无意中听说" + mid + "。";
				case 8:
					return "听说有位" + advZh + "称，" + mid + "。";
				case 9:
					return "听说有位" + advZh + "曾议论道，" + mid + "。";
				case 10:
					return "听说有位" + advZh + "说过这样一个故事：" + mid + "。";
				case 11:
					return "听说有位" + advZh + "无意间吐露，" + mid + "。";
				case 12:
					return "听说有位" + advZh + "到处扬言，" + mid + "。";
				case 13:
					return "听说有位" + advZh + "讲述传说时称，" + mid + "。";
				default:
					return null;
			}
		}

		private static string TryPrefixedRareLocationMixTogether( Mobile m, string en )
		{
			Match ma = Regex.Match( en, @"^(.+?)\s+where one can find\s+(.+?)\s+in\s+(.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return MapRareMixSentence( m, ma.Groups[1].Value.Trim(), ma.Groups[2].Value.Trim(), ma.Groups[3].Value.Trim(), 0 );

			ma = Regex.Match( en, @"^(.+?)\s+where one would need to go to\s+(.+?)\s+if they are going to find\s+(.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return MapRareMixSentence( m, ma.Groups[1].Value.Trim(), ma.Groups[3].Value.Trim(), ma.Groups[2].Value.Trim(), 1 );

			ma = Regex.Match( en, @"^(.+?)\s+that someone can probably find\s+(.+?)\s+if they search\s+(.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return MapRareMixSentence( m, ma.Groups[1].Value.Trim(), ma.Groups[2].Value.Trim(), ma.Groups[3].Value.Trim(), 2 );

			return null;
		}

		private static string TryComplex( Mobile m, string en )
		{
			Match ma;

			string tavern = TavernChatterDynamicZh.TryResolve( m, en );
			if ( tavern != null )
				return tavern;

			if ( en == "We need to find a bank and split this loot we have." )
				return "我们得去找家银行，把这批战利品分了。";

			ma = Regex.Match( en, @"^amagic item called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + CompositePart( m, ma.Groups[1].Value ) + "」的魔法物品失落于「" + CompositePart( m, ma.Groups[2].Value ) + "」。";

			// TavernPatrons.GetChatter — log-style rumour frames: "<JobPrefix> <verb> <body>."
			ma = Regex.Match( en, @"^We will (look for|search for|find|seek out|try to find|ambush|surprise|try to ambush|try to capture) (.+?) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string verbZh = MapWeWillVerb( ma.Groups[1].Value );
				string who = CompositePart( m, ma.Groups[2].Value.Trim() );
				string placeRaw = ma.Groups[3].Value.Trim();
				bool tomorrow = false;
				if ( placeRaw.EndsWith( " tomorrow", StringComparison.Ordinal ) )
				{
					tomorrow = true;
					placeRaw = placeRaw.Substring( 0, placeRaw.Length - " tomorrow".Length ).Trim();
				}

				string placeZh = CompositePart( m, placeRaw );
				if ( tomorrow )
					return "我们明日将在「" + placeZh + "」" + verbZh + "「" + who + "」。";
				return "我们将在「" + placeZh + "」" + verbZh + "「" + who + "」。";
			}

			ma = Regex.Match( en, @"^((?:An|The|A)\s+.+?)\s+tells of\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string roleZh = TranslateTavernLogJobPrefix( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				bool q = body.EndsWith( "?" );
				if ( q )
					body = body.Substring( 0, body.Length - 1 );
				else if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				if ( q )
					return "有位" + roleZh + "谈及「" + bzh + "」吗？";
				return "有位" + roleZh + "谈及「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^((?:An|The|A)\s+.+?)\s+tells tales of\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string roleZh = TranslateTavernLogJobPrefix( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				bool q = body.EndsWith( "?" );
				if ( q )
					body = body.Substring( 0, body.Length - 1 );
				else if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				if ( q )
					return "有位" + roleZh + "讲述了「" + bzh + "」的传说吗？";
				return "有位" + roleZh + "讲述了「" + bzh + "」的传说。";
			}

			ma = Regex.Match( en, @"^((?:An|The|A)\s+.+?)\s+is spreading rumou?rs about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string roleZh = TranslateTavernLogJobPrefix( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return "有位" + roleZh + "正在散布关于「" + bzh + "」的传言。";
			}

			ma = Regex.Match( en, @"^((?:An|The|A)\s+.+?)\s+heard rumou?rs about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string roleZh = TranslateTavernLogJobPrefix( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return "有位" + roleZh + "听说了「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^((?:An|The|A)\s+.+?)\s+heard of\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string roleZh = TranslateTavernLogJobPrefix( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				bool q = body.EndsWith( "?" );
				if ( q )
					body = body.Substring( 0, body.Length - 1 );
				else if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				if ( q )
					return "有位" + roleZh + "听说过「" + bzh + "」吗？";
				return "有位" + roleZh + "听说过「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^((?:An|The|A)\s+.+?)\s+mentioned that there was\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string roleZh = TranslateTavernLogJobPrefix( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				bool q = body.EndsWith( "?" );
				if ( q )
					body = body.Substring( 0, body.Length - 1 );
				else if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				if ( q )
					return "有位" + roleZh + "提到曾有「" + bzh + "」吗？";
				return "有位" + roleZh + "提到曾有「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^((?:An|The|A)\s+.+?)\s+mentioned something about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string roleZh = TranslateTavernLogJobPrefix( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				bool q = body.EndsWith( "?" );
				if ( q )
					body = body.Substring( 0, body.Length - 1 );
				else if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				if ( q )
					return "有位" + roleZh + "提到了「" + bzh + "」吗？";
				return "有位" + roleZh + "提到了「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^((?:An|The|A)\s+.+?)\s+found\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string roleZh = TranslateTavernLogJobPrefix( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				bool q = body.EndsWith( "?" );
				if ( q )
					body = body.Substring( 0, body.Length - 1 );
				else if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				if ( q )
					return "有位" + roleZh + "发现了「" + bzh + "」吗？";
				return "有位" + roleZh + "发现了「" + bzh + "」。";
			}

			// Citizens.SetupCitizen — fixed-topic rumors (full English sentence).
			ma = Regex.Match( en, @"^I met with (.+?) and they told me to bring back (.+) from (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我与「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」会面，对方嘱咐我从「" + CompositePart( m, ma.Groups[3].Value.Trim() ) + "」带回「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^I was talking with the local (.+), and they mentioned (.+) and (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我与当地的「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」闲聊，对方提及「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」与「" + CompositePart( m, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^Someone told me that (.+) is where you would look for (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "有人告诉我，若要寻找「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」，该去「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」碰碰运气。";

			ma = Regex.Match( en, @"^Someone from (.+) died in (.+) searching for (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」来的人在「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」搜寻「" + CompositePart( m, ma.Groups[3].Value.Trim() ) + "」时殒命了。";

			ma = Regex.Match( en, @"^I heard many tales of adventurers going to (.+) and seeing (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我常听人说有冒险者前往「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」，并在那儿见到了「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^(.+?) was in the tavern talking about (.+) and (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」曾在酒馆里谈起「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」与「" + CompositePart( m, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^I heard that (.+) can be obtained in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我听说「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」可在「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」得手。";

			ma = Regex.Match( en, @"^I heard that (.+) can be found in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我听说在「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」能找到「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^I heard something about (.+) and (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我隐约听说「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」与「" + CompositePart( m, ma.Groups[2].Value.Trim() ) + "」都有些风声。";

			{
				string rareMix = TryPrefixedRareLocationMixTogether( m, en );
				if ( rareMix != null )
					return rareMix;
			}

			ma = Regex.Match( en, @"^Some ([a-z]+) found\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string advZh = AdventurerKindZh( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				bool q = body.EndsWith( "?" );
				if ( q )
					body = body.Substring( 0, body.Length - 1 );
				else if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				if ( q )
					return "听说有位" + advZh + "发现了「" + bzh + "」吗？";
				return "听说有位" + advZh + "发现了「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^Some ([a-z]+) heard rumou?rs about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string advZh = AdventurerKindZh( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return "听说有位" + advZh + "提起了「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^Some ([a-z]+) heard a story about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string advZh = AdventurerKindZh( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return "听说有位" + advZh + "听了一个关于「" + bzh + "」的故事。";
			}

			ma = Regex.Match( en, @"^Some ([a-z]+) overheard another tell of\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string advZh = AdventurerKindZh( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return "听说有位" + advZh + "无意间听到有人谈起「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^Some ([a-z]+) is spreading rumou?rs about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string advZh = AdventurerKindZh( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return "听说有位" + advZh + "正在散布关于「" + bzh + "」的传言。";
			}

			ma = Regex.Match( en, @"^Some ([a-z]+) is telling tales about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string advZh = AdventurerKindZh( m, ma.Groups[1].Value );
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return "听说有位" + advZh + "讲述了「" + bzh + "」的传说。";
			}

			ma = Regex.Match( en, @"^(I|We) found\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string iw = ma.Groups[1].Value;
				string body = ma.Groups[2].Value.TrimEnd();
				bool q = body.EndsWith( "?" );
				if ( q )
					body = body.Substring( 0, body.Length - 1 );
				else if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				string subj = iw == "We" ? "我们" : "我";
				if ( q )
					return subj + "发现了「" + bzh + "」吗？";
				return subj + "发现了「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^(I|We) heard rumou?rs about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string iw = ma.Groups[1].Value;
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return ( iw == "We" ? "我们" : "我" ) + "听说了「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^(I|We) heard a story about\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string iw = ma.Groups[1].Value;
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return ( iw == "We" ? "我们" : "我" ) + "听了一个关于「" + bzh + "」的故事。";
			}

			ma = Regex.Match( en, @"^(I|We) overheard someone tell of\s+(.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string iw = ma.Groups[1].Value;
				string body = ma.Groups[2].Value.TrimEnd();
				if ( body.EndsWith( "." ) )
					body = body.Substring( 0, body.Length - 1 );
				string bzh = CompositePart( m, body );
				return ( iw == "We" ? "我们" : "我" ) + "无意间听到有人谈起「" + bzh + "」。";
			}

			ma = Regex.Match( en, @"^Were you saying something about\s+(.+)\?$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "你是不是在说「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」？";

			ma = Regex.Match( en, @"^Were you saying that there is\s+(.+)\?$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "你是不是在说有「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」？";

			ma = Regex.Match( en, @"^Where did I hear about\s+(.+)\?$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我曾在哪儿听说「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」？";

			ma = Regex.Match( en, @"^Where did I hear that there is\s+(.+)\?$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我曾在哪儿听说有「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」？";

			ma = Regex.Match( en, @"^Are you telling me that there is\s+(.+)\?$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "你是告诉我有「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」？";

			ma = Regex.Match( en, @"^Do you mean to say that there is\s+(.+)\?$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "你的意思是说有「" + CompositePart( m, ma.Groups[1].Value.Trim() ) + "」？";

			// TavernPatrons citizen hooks — full Chinese sentences (avoid English "for" glue + partial fragments).
			ma = Regex.Match( en, @"^(.+?) was imprisoned for stealing from the (.+?) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string who = ma.Groups[1].Value.Trim();
				string jobEn = ma.Groups[2].Value.Trim();
				string jobZh = NpcSpeechTokenZh.TranslateJobZh( jobEn.ToLowerInvariant() );
				if ( jobZh == null || jobZh.Length == 0 || jobZh.Equals( jobEn, StringComparison.OrdinalIgnoreCase ) )
					jobZh = CompositePart( m, jobEn );
				string cityZh = CompositePart( m, ma.Groups[3].Value.Trim() );
				return "「" + who + "」因在「" + cityZh + "」从" + jobZh + "处行窃而入狱。";
			}

			ma = Regex.Match( en, @"^We will search for the (.+) tomorrow\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string midZh = CompositePart( m, ma.Groups[1].Value.Trim() );
				return "明日我们将寻访「" + midZh + "」。";
			}

			ma = Regex.Match( en, @"^Some (.+?) will pay us (.+?) gold if we find them (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string noble = ma.Groups[1].Value.Trim();
				string nobleZh = NpcSpeechTokenZh.TranslateAdventurerZh( noble.ToLowerInvariant() );
				if ( nobleZh == null || nobleZh.Length == 0 || nobleZh.Equals( noble, StringComparison.OrdinalIgnoreCase ) )
					nobleZh = noble;
				string coins = ma.Groups[2].Value.Trim();
				string relicsZh = CompositePart( m, ma.Groups[3].Value.Trim() );
				return "某个" + nobleZh + "愿付我们" + coins + "枚金币，若能为其寻得" + relicsZh + "。";
			}

			// 165 before 164 (shared prefix "a map that leads to")
			ma = Regex.Match( en, @"^a (map|tablet|scroll|book|clue) that leads to the (gold|treasure|gems|jewels|riches|crystals) of (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string k1 = MapLeadKind( ma.Groups[1].Value );
				string k2 = MapLootKind( ma.Groups[2].Value );
				string name = CompositePart( m, ma.Groups[3].Value );
				return "传闻一纸" + k1 + "指向「" + name + "」所藏的" + k2 + "。";
			}

			ma = Regex.Match( en, @"^a (map|tablet|scroll|book|clue) that leads to (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string k1 = MapLeadKind( ma.Groups[1].Value );
				string dun = CompositePart( m, ma.Groups[2].Value );
				return "传闻一纸" + k1 + "指向「" + dun + "」。";
			}

			ma = Regex.Match( en, @"^a (tomb|crypt|treasure|artifact|remains) of (.+?) in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string kind = MapTombKind( ma.Groups[1].Value );
				string name = CompositePart( m, ma.Groups[2].Value );
				string dun = CompositePart( m, ma.Groups[3].Value );
				return "传闻在「" + dun + "」藏有「" + name + "」之" + kind + "。";
			}

			ma = Regex.Match( en, @"^an artifact called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + CompositePart( m, ma.Groups[1].Value ) + "」的奇物失落于「" + CompositePart( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^a magic item called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + CompositePart( m, ma.Groups[1].Value ) + "」的魔法物品失落于「" + CompositePart( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^an ancient artifact called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + CompositePart( m, ma.Groups[1].Value ) + "」的远古奇物失落于「" + CompositePart( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^an ancient relic called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + CompositePart( m, ma.Groups[1].Value ) + "」的远古遗物失落于「" + CompositePart( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^aArtefact called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + CompositePart( m, ma.Groups[1].Value ) + "」的奇物失落于「" + CompositePart( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^legends of (.+) being (destroyed|ruined|devastated|lost) during (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string city = CompositePart( m, ma.Groups[1].Value );
				string vb = MapBeingVerb( ma.Groups[2].Value );
				string dis = CompositePart( m, ma.Groups[3].Value );
				return "传闻「" + city + "」于「" + dis + "」之际遭" + vb + "。";
			}

			ma = Regex.Match( en, @"^a ([A-Za-z]+) that (joined|left|betrayed|destroyed|started) (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string job = CompositePart( m, ma.Groups[1].Value );
				string soc = CompositePart( m, ma.Groups[3].Value );
				string act = ma.Groups[2].Value;
				string actZh = act;
				switch ( act )
				{
					case "joined": actZh = "加入了"; break;
					case "left": actZh = "离开了"; break;
					case "betrayed": actZh = "背叛了"; break;
					case "destroyed": actZh = "摧毁了"; break;
					case "started": actZh = "发起了"; break;
				}

				return "传闻有位" + job + actZh + soc + "。";
			}

			ma = Regex.Match( en, @"^a ([A-Za-z]+) that was (robbed|killed|lost|slain|arrested|kidnapped) on the way to (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string job = CompositePart( m, ma.Groups[1].Value );
				string fate = ma.Groups[2].Value;
				string fateZh = fate;
				switch ( fate )
				{
					case "robbed": fateZh = "遭劫"; break;
					case "killed": fateZh = "遇害"; break;
					case "lost": fateZh = "失踪"; break;
					case "slain": fateZh = "遇害"; break;
					case "arrested": fateZh = "被捕"; break;
					case "kidnapped": fateZh = "被掳"; break;
				}

				string dest = CompositePart( m, ma.Groups[3].Value );
				return "传闻有位" + job + "在前往「" + dest + "」的途中" + fateZh + "。";
			}

			ma = Regex.Match( en, @"^a (hydra|dragon|drake|wyrm) tooth being thrown on the ground to summon a skeleton$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻有人掷下" + MapWyrmKind( ma.Groups[1].Value ) + "之牙于地，可召来一具骷髅。";

			ma = Regex.Match( en, @"^a ([A-Za-z]+) selling a megaldon tooth to a (fisherman|ship builder|pirate|sailor) in (.+) for (\d+) gold$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string job = CompositePart( m, ma.Groups[1].Value );
				string role = MapSeaTradeRole( ma.Groups[2].Value );
				string city = CompositePart( m, ma.Groups[3].Value );
				string gold = ma.Groups[4].Value;
				return "传闻「" + city + "」有位" + job + "以" + gold + "金币将巨齿鲨齿卖给一名" + role + "。";
			}

			ma = Regex.Match( en, @"^a ([A-Za-z]+) that (died|went missing|perished|was slain|was lost) in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string job = CompositePart( m, ma.Groups[1].Value );
				string fate = ma.Groups[2].Value;
				string fateZh = fate;
				switch ( fate )
				{
					case "died": fateZh = "殒命"; break;
					case "went missing": fateZh = "失踪"; break;
					case "perished": fateZh = "身亡"; break;
					case "was slain": fateZh = "遇害"; break;
					case "was lost": fateZh = "失踪"; break;
				}

				string dun = CompositePart( m, ma.Groups[3].Value );
				return "传闻有位" + job + "在「" + dun + "」" + fateZh + "。";
			}

			return null;
		}

		/// <summary>Returns localized full sentence, or <c>null</c> to fall back to fragment replacement.</summary>
		public static string TryApply( Mobile m, string english )
		{
			if ( m == null || english == null || english.Length == 0 )
				return null;

			string lang = AccountLang.GetLanguageCode( m.Account );

			if ( !AccountLang.IsChinese( lang ) )
				return null;

			QuestCompositeResolver.EnsureInitialized();

			string complex = TryComplex( m, english );

			if ( complex != null )
				return complex;

			EnsureRules();

			if ( s_Rules == null )
				return null;

			for ( int i = 0; i < s_Rules.Length; ++i )
			{
				Rule r = s_Rules[i];
				Match ma = r.Re.Match( english );

				if ( !ma.Success )
					continue;

				if ( r.NGroups == 0 )
					return r.Template;

				if ( ma.Groups.Count - 1 < r.NGroups )
					continue;

				object[] args = new object[r.NGroups];

				for ( int g = 0; g < r.NGroups; ++g )
					args[g] = CompositePart( m, ma.Groups[g + 1].Value );

				try
				{
					return string.Format( r.Template, args );
				}
				catch
				{
					return null;
				}
			}

			return null;
		}

		/// <summary>
		/// Like <see cref="TryApply"/> but does not require a Chinese player account on <paramref name="english"/> source mobile.
		/// Used to pre-build zh-Hans for NPC tavern broadcast strings (viewer-specific polish still runs in <see cref="CitizenLocalization.SayLocalizedComposite"/>).
		/// </summary>
		public static string TryApplyForBroadcast( string english )
		{
			if ( english == null || english.Length == 0 )
				return null;

			QuestCompositeResolver.EnsureInitialized();

			string complex = TryComplex( null, english );

			if ( complex != null )
				return complex;

			EnsureRules();

			if ( s_Rules == null )
				return null;

			for ( int i = 0; i < s_Rules.Length; ++i )
			{
				Rule r = s_Rules[i];
				Match ma = r.Re.Match( english );

				if ( !ma.Success )
					continue;

				if ( r.NGroups == 0 )
					return r.Template;

				if ( ma.Groups.Count - 1 < r.NGroups )
					continue;

				object[] args = new object[r.NGroups];

				for ( int g = 0; g < r.NGroups; ++g )
					args[g] = CompositePart( null, ma.Groups[g + 1].Value );

				try
				{
					return string.Format( r.Template, args );
				}
				catch
				{
					return null;
				}
			}

			return null;
		}
	}
}
