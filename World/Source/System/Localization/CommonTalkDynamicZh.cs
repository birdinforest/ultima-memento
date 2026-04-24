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
				(@"^some ([a-z]+) solving the mystery of the Skull Gate$", "传闻有位「{0}」正试图揭开颅骨门之谜。", 1),
				(@"^some ([a-z]+) solving the mystery of the Serpent Pillars$", "传闻有位「{0}」正试图揭开巨蛇柱之谜。", 1),
				(@"^someone buried with great treasure in the graveyard in (.+)$", "传闻「{0}」的墓园中，有人携重宝藏于坟冢。", 1),
				(@"^a demilich dwelling below (.+)$", "传闻「{0}」地下盘踞着一名半巫妖。", 1),
				(@"^some ([a-z]+) selling artifacts in (.+)$", "传闻「{1}」有「{0}」在兜售遗物奇器。", 2),
				(@"^someone who killed the ([a-z]+) in (.+)$", "传闻在「{1}」，有人杀害了那位「{0}」。", 2),
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

		private static string TryComplex( Mobile m, string en )
		{
			Match ma;

			// TavernPatrons citizen hooks — full Chinese sentences (avoid English "for" glue + partial fragments).
			ma = Regex.Match( en, @"^(.+?) was imprisoned for stealing from the (.+?) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string who = ma.Groups[1].Value.Trim();
				string jobEn = ma.Groups[2].Value.Trim();
				string jobZh = NpcSpeechTokenZh.TranslateJobZh( jobEn.ToLowerInvariant() );
				if ( jobZh == null || jobZh.Length == 0 || jobZh.Equals( jobEn, StringComparison.OrdinalIgnoreCase ) )
					jobZh = QuestCompositeResolver.ResolveComposite( m, jobEn );
				string cityZh = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value.Trim() );
				return "「" + who + "」因在「" + cityZh + "」从" + jobZh + "处行窃而入狱。";
			}

			ma = Regex.Match( en, @"^We will search for the (.+) tomorrow\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string midZh = QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value.Trim() );
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
				string relicsZh = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value.Trim() );
				return "某个" + nobleZh + "愿付我们" + coins + "枚金币，若能为其寻得" + relicsZh + "。";
			}

			// 165 before 164 (shared prefix "a map that leads to")
			ma = Regex.Match( en, @"^a (map|tablet|scroll|book|clue) that leads to the (gold|treasure|gems|jewels|riches|crystals) of (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string k1 = MapLeadKind( ma.Groups[1].Value );
				string k2 = MapLootKind( ma.Groups[2].Value );
				string name = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value );
				return "传闻一纸" + k1 + "指向「" + name + "」所藏的" + k2 + "。";
			}

			ma = Regex.Match( en, @"^a (map|tablet|scroll|book|clue) that leads to (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string k1 = MapLeadKind( ma.Groups[1].Value );
				string dun = QuestCompositeResolver.ResolveComposite( m, ma.Groups[2].Value );
				return "传闻一纸" + k1 + "指向「" + dun + "」。";
			}

			ma = Regex.Match( en, @"^a (tomb|crypt|treasure|artifact|remains) of (.+?) in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string kind = MapTombKind( ma.Groups[1].Value );
				string name = QuestCompositeResolver.ResolveComposite( m, ma.Groups[2].Value );
				string dun = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value );
				return "传闻在「" + dun + "」藏有「" + name + "」之" + kind + "。";
			}

			ma = Regex.Match( en, @"^an artifact called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value ) + "」的奇物失落于「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^a magic item called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value ) + "」的魔法物品失落于「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^an ancient artifact called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value ) + "」的远古奇物失落于「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^an ancient relic called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value ) + "」的远古遗物失落于「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^aArtefact called (.+) lost in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻名为「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value ) + "」的奇物失落于「" + QuestCompositeResolver.ResolveComposite( m, ma.Groups[2].Value ) + "」。";

			ma = Regex.Match( en, @"^legends of (.+) being (destroyed|ruined|devastated|lost) during (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string city = QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value );
				string vb = MapBeingVerb( ma.Groups[2].Value );
				string dis = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value );
				return "传闻「" + city + "」于「" + dis + "」之际遭" + vb + "。";
			}

			ma = Regex.Match( en, @"^a ([A-Za-z]+) that (joined|left|betrayed|destroyed|started) (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string job = QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value );
				string soc = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value );
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
				string job = QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value );
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

				string dest = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value );
				return "传闻有位" + job + "在前往「" + dest + "」的途中" + fateZh + "。";
			}

			ma = Regex.Match( en, @"^a (hydra|dragon|drake|wyrm) tooth being thrown on the ground to summon a skeleton$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "传闻有人掷下" + MapWyrmKind( ma.Groups[1].Value ) + "之牙于地，可召来一具骷髅。";

			ma = Regex.Match( en, @"^a ([A-Za-z]+) selling a megaldon tooth to a (fisherman|ship builder|pirate|sailor) in (.+) for (\d+) gold$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string job = QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value );
				string role = MapSeaTradeRole( ma.Groups[2].Value );
				string city = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value );
				string gold = ma.Groups[4].Value;
				return "传闻「" + city + "」有位" + job + "以" + gold + "金币将巨齿鲨齿卖给一名" + role + "。";
			}

			ma = Regex.Match( en, @"^a ([A-Za-z]+) that (died|went missing|perished|was slain|was lost) in (.+)$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string job = QuestCompositeResolver.ResolveComposite( m, ma.Groups[1].Value );
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

				string dun = QuestCompositeResolver.ResolveComposite( m, ma.Groups[3].Value );
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
					args[g] = QuestCompositeResolver.ResolveComposite( m, ma.Groups[g + 1].Value );

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
