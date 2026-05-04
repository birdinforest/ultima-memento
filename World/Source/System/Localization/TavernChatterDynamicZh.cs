using System;
using System.Text.RegularExpressions;
using Server.Mobiles;

namespace Server.Localization
{
	/// <summary>
	/// zh-Hans sentence templates for <see cref="Server.Misc.TavernPatrons.GetChatter"/> (tails after optional rumor prefix, and non-prefix lines).
	/// Invoked from <see cref="CommonTalkDynamicZh.TryComplex"/> before other rules.
	/// </summary>
	public static class TavernChatterDynamicZh
	{
		private static string Cmp( Mobile viewer, string fragment )
		{
			if ( fragment == null )
				return fragment;
			if ( viewer != null )
				return QuestCompositeResolver.ResolveComposite( viewer, fragment );
			return QuestCompositeResolver.ResolveCompositeToZhHans( fragment );
		}

		private static string MapCoin( string c )
		{
			switch ( c.ToLowerInvariant() )
			{
				case "gold": return "金币";
				case "silver": return "银币";
				case "copper": return "铜币";
				case "jewels": return "珠宝";
				case "crystals": return "水晶";
				default: return c;
			}
		}

		private static string MapDebtTail( Mobile m, string debt )
		{
			if ( debt == null || debt.Length == 0 )
				return "";
			string z = Cmp( m, debt.Trim() );
			return z;
		}

		private static string MapWhileIdle( string w )
		{
			switch ( w.ToLowerInvariant() )
			{
				case "sleeping": return "打盹";
				case "drinking": return "喝酒";
				case "eating": return "吃饭";
				case "distracted": return "分心";
				case "searching": return "搜寻";
				case "lost": return "迷路";
				case "gone": return "走开";
				case "exploring": return "探路";
				case "drunk": return "喝醉";
				default: return w;
			}
		}

		private static string MapBountyVerb( string v )
		{
			switch ( v.ToLowerInvariant() )
			{
				case "kill": return "除掉";
				case "find": return "找到";
				case "slay": return "斩杀";
				case "assassinate": return "刺杀";
				case "rescue": return "救出";
				case "kidnap": return "绑走";
				case "free": return "释放";
				case "help": return "帮助";
				case "capture": return "活捉";
				default: return v;
			}
		}

		private static string MapPrize( string p )
		{
			switch ( p.ToLowerInvariant() )
			{
				case "prize": return "赏格";
				case "fee": return "酬金";
				case "reward": return "赏金";
				case "tribute": return "贡礼";
				case "sack": return "一袋赏金";
				case "chest": return "一箱赏金";
				case "coffer": return "一箱赏金";
				case "pile": return "一堆赏金";
				default: return p;
			}
		}

		private static string MapRelicAct( string a )
		{
			switch ( a.ToLowerInvariant() )
			{
				case "destroyed": return "毁掉了";
				case "sold": return "卖掉了";
				case "lost": return "弄丢了";
				case "found": return "找到了";
				case "discovered": return "发现了";
				case "traded": return "换掉了";
				case "stole": return "偷走了";
				default: return a;
			}
		}

		private static string MapThemAct( string a )
		{
			switch ( a.ToLowerInvariant() )
			{
				case "robbed": return "劫了";
				case "assassinated": return "暗杀了";
				case "betrayed": return "背叛了";
				case "captured": return "俘虏了";
				case "fooled": return "愚弄了";
				case "killed": return "杀了";
				case "swindled": return "骗了";
				default: return a;
			}
		}

		private static string MapTradeVerb( string a )
		{
			switch ( a.ToLowerInvariant() )
			{
				case "bought that from": return "向";
				case "stole that from": return "从";
				case "sold that to": return "卖给了";
				case "met with": return "会见过";
				case "kidnapped": return "绑架了";
				case "robbed": return "抢劫了";
				case "works for": return "效力于";
				case "lives with": return "与";
				default:
					if ( a.StartsWith( "owes ", StringComparison.OrdinalIgnoreCase ) )
					{
						Match mx = Regex.Match( a, @"^owes (\d+) gold to$", RegexOptions.IgnoreCase );
						if ( mx.Success )
							return "欠着" + mx.Groups[1].Value + "枚金币，债主是";
					}
					return a;
			}
		}

		private static string MapWarKindPhrase( string a )
		{
			switch ( a.ToLowerInvariant() )
			{
				case "a war": return "一场战祸";
				case "a battle": return "一场战端";
				case "an alliance": return "一纸盟约";
				case "a pact": return "一道密约";
				case "a trade agreement": return "一项商约";
				case "a tournament": return "一场比武大会";
				case "a standoff": return "一场对峙";
				case "a blockade": return "一道封锁";
				case "a dispute": return "一场纷争";
				default: return a;
			}
		}

		private static string MapSiegeOutcome( string fate )
		{
			switch ( fate.ToLowerInvariant() )
			{
				case "destroyed": return "夷毁";
				case "captured": return "沦陷";
				case "invaded": return "遭侵";
				case "rescued": return "获救";
				case "freed": return "光复";
				case "ruined": return "沦为废墟";
				case "taken": return "易手";
				case "surrounded": return "被围";
				case "settled": return "平定";
				default: return fate;
			}
		}

		private static string MapKillByMonster( string st )
		{
			switch ( st.ToLowerInvariant() )
			{
				case "killed": return "遇害";
				case "slain": return "被斩杀";
				case "defeated": return "被击败";
				case "almost killed": return "险些遇害";
				case "almost slain": return "险些被斩";
				case "almost defeated": return "险些落败";
				default: return st;
			}
		}

		private static string MapSceneAct( string a )
		{
			switch ( a.ToLowerInvariant() )
			{
				case "robbed": return "抢劫了";
				case "assassinated": return "刺杀了";
				case "captured": return "俘虏了";
				case "met": return "会见了";
				case "killed": return "杀害了";
				case "left": return "离开了";
				case "followed": return "跟踪了";
				case "served": return "侍奉过";
				case "arrested": return "逮捕了";
				default: return a;
			}
		}

		private static string MapExecuted( string a )
		{
			switch ( a.ToLowerInvariant() )
			{
				case "executed": return "处决了";
				case "jailed": return "下狱了";
				case "arrested": return "逮捕了";
				case "captured": return "擒获了";
				case "banished": return "放逐了";
				case "rewarded": return "嘉奖了";
				case "celebrated": return "褒扬了";
				case "promoted": return "擢升了";
				case "released": return "释放了";
				default: return a;
			}
		}

		private static string MapCrime( string c )
		{
			switch ( c.ToLowerInvariant() )
			{
				case "murder": return "谋杀";
				case "theft": return "盗窃";
				case "gambling": return "赌博";
				case "witchcraft": return "巫术";
				case "slavery": return "奴役";
				case "attempted murder": return "谋杀未遂";
				case "debauchery": return "放荡";
				case "drunkenness": return "酗酒";
				default: return c;
			}
		}

		private static string MapWantedState( string s )
		{
			switch ( s.ToLowerInvariant() )
			{
				case "wanted": return "被通缉";
				case "on trial": return "在受审";
				case "in jail": return "在牢里";
				case "in prison": return "在狱中";
				case "put to death": return "被判死刑";
				case "sought after": return "被追捕";
				case "put in chains": return "被上了镣铐";
				case "sentenced": return "被判刑";
				case "put in the iron maiden": return "被关进铁处女";
				default: return s;
			}
		}

		private static string MapCityFate( string s )
		{
			switch ( s.ToLowerInvariant() )
			{
				case "destroyed": return "夷为平地";
				case "captured": return "沦陷";
				case "invaded": return "遭入侵";
				case "rescued": return "获解救";
				case "freed": return "光复";
				case "ruined": return "沦为废墟";
				case "taken": return "被攻占";
				case "surrounded": return "被围困";
				case "settled": return "被平定";
				default: return s;
			}
		}

		private static string MapMilUnit( string u )
		{
			switch ( u.ToLowerInvariant() )
			{
				case "army": return "大军";
				case "troops": return "部队";
				case "soldiers": return "士兵";
				case "knights": return "骑士团";
				case "fleet": return "舰队";
				case "forces": return "军力";
				default: return u;
			}
		}

		private static string MapHavingBeenClause( Mobile v, string st )
		{
			if ( st == null )
				return st;
			string t = st.Trim();
			if ( t.StartsWith( "starting ", StringComparison.OrdinalIgnoreCase ) )
				return "忙着筹备" + Cmp( v, t.Substring( "starting ".Length ).Trim() );
			switch ( t.ToLowerInvariant() )
			{
				case "hiding": return "隐姓埋名躲着";
				case "missing": return "下落不明";
				case "living": return "在此定居";
				case "resting": return "在此静养";
				case "laying low": return "暂避风头";
				case "imprisoned":
				case "locked up": return "身陷囹圄";
				case "retired": return "闭门赋闲";
				case "settling": return "在此安家落户";
				default: return Cmp( v, t );
			}
		}

		private static string MapPersonLootSentence( Mobile v, string who, string verb, string loot, string locale, string town )
		{
			string lootZ = Cmp( v, loot.Trim() );
			string loc = MapLocalePrep( locale, town, v );
			switch ( verb.ToLowerInvariant() )
			{
				case "hiding": return "据说「" + who + "」" + loc + "藏着「" + lootZ + "」。";
				case "burying": return "据说「" + who + "」" + loc + "埋着「" + lootZ + "」。";
				case "bringing": return "据说「" + who + "」" + loc + "运来了「" + lootZ + "」。";
				case "losing": return "据说「" + who + "」" + loc + "弄丢了「" + lootZ + "」。";
				case "finding": return "据说「" + who + "」" + loc + "找到了「" + lootZ + "」。";
				case "searching for": return "据说「" + who + "」" + loc + "在找「" + lootZ + "」。";
				case "delivering": return "据说「" + who + "」" + loc + "押送着「" + lootZ + "」。";
				case "leaving": return "据说「" + who + "」" + loc + "留下了「" + lootZ + "」。";
				default: return "据说「" + who + "」" + loc + "与「" + lootZ + "」有关。";
			}
		}

		private static string MapLootState( string s )
		{
			switch ( s.ToLowerInvariant() )
			{
				case "hidden": return "藏匿";
				case "buried": return "埋藏";
				case "lost": return "失落";
				case "waiting": return "等候取走";
				default: return s;
			}
		}

		private static string MapLocalePrep( string loc, string town, Mobile m )
		{
			string t = Cmp( m, town );
			string low = loc.ToLowerInvariant();
			if ( low.StartsWith( "somewhere ", StringComparison.Ordinal ) )
				low = low.Substring( "somewhere ".Length ).Trim();
			if ( low == "near" || low == "by" )
				return "在「" + t + "」附近";
			if ( low == "on the outskirts of" || low == "outside" )
				return "在「" + t + "」城外";
			if ( low == "inside" || low == "in" )
				return "在「" + t + "」之内";
			return "在「" + t + "」一带";
		}

		private static string MapMonsterHuntSentence( Mobile v, string who, string verb, string mon, string tomb )
		{
			string m = Cmp( v, mon.Trim() );
			string t = Cmp( v, tomb.Trim() );
			switch ( verb.ToLowerInvariant() )
			{
				case "killing": return "据说「" + who + "」在「" + t + "」猎杀「" + m + "」。";
				case "slaying": return "据说「" + who + "」在「" + t + "」屠戮「" + m + "」。";
				case "being killed by": return "据说「" + who + "」在「" + t + "」遭「" + m + "」毒手。";
				case "being slain by": return "据说「" + who + "」在「" + t + "」被「" + m + "」斩杀。";
				case "fleeing from": return "据说「" + who + "」在「" + t + "」躲避「" + m + "」。";
				case "chasing": return "据说「" + who + "」在「" + t + "」追逐「" + m + "」。";
				case "hunting for": return "据说「" + who + "」在「" + t + "」猎捕「" + m + "」。";
				case "searching for": return "据说「" + who + "」在「" + t + "」搜寻「" + m + "」。";
				case "never finding": return "据说「" + who + "」在「" + t + "」怎么也寻不见「" + m + "」。";
				default: return "据说「" + who + "」在「" + t + "」与「" + m + "」有关。";
			}
		}

		/// <summary>Returns a full zh-Hans sentence, or <c>null</c> to let other rules handle <paramref name="en"/>.</summary>
		public static string TryResolve( Mobile viewer, string en )
		{
			if ( en == null || en.Length == 0 )
				return null;

			Match ma;

			ma = Regex.Match( en, @"^We are supposed to wait for (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我们本该等「" + ma.Groups[1].Value.Trim() + "」。";

			ma = Regex.Match( en, @"^(.+) lives somewhere near (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」住在「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」附近。";

			ma = Regex.Match( en, @"^We will go find (.+) tomorrow\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我们明日便去寻访「" + ma.Groups[1].Value.Trim() + "」。";

			ma = Regex.Match( en, @"^(.+) still owes me (\d+) (gold|silver|copper|jewels|crystals) (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string who = ma.Groups[1].Value.Trim();
				string coin = MapCoin( ma.Groups[3].Value );
				string debt = MapDebtTail( viewer, ma.Groups[4].Value );
				return "「" + who + "」还欠我" + ma.Groups[2].Value + "枚" + coin + "（缘由：" + debt + "）。";
			}

			ma = Regex.Match( en, @"^I think (.+) stole it while we were (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我看是「" + ma.Groups[1].Value.Trim() + "」趁我们" + MapWhileIdle( ma.Groups[2].Value ) + "时偷走的。";

			ma = Regex.Match( en, @"^(.+) will bring it here when they find it\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "等他们找到了，「" + ma.Groups[1].Value.Trim() + "」自会带到这里来。";

			ma = Regex.Match( en, @"^(Do you know|Where did you meet|Where did you see|When did you meet|When did you see|When have you last heard from|When did you kill|Where did you kill|When will I meet|When will we meet)\s+(.+)\?$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string q = ma.Groups[1].Value;
				string who = ma.Groups[2].Value.Trim();
				string qzh = q + "「" + who + "」？";
				switch ( q )
				{
					case "Do you know": qzh = "你可认得「" + who + "」？"; break;
					case "Where did you meet": qzh = "你在哪儿遇见「" + who + "」的？"; break;
					case "Where did you see": qzh = "你在哪儿见过「" + who + "」？"; break;
					case "When did you meet": qzh = "你何时见过「" + who + "」？"; break;
					case "When did you see": qzh = "你何时见到「" + who + "」？"; break;
					case "When have you last heard from": qzh = "你最近一次听说「" + who + "」是什么时候？"; break;
					case "When did you kill": qzh = "你何时对「" + who + "」下的手？"; break;
					case "Where did you kill": qzh = "你在何处对「" + who + "」下的手？"; break;
					case "When will I meet": qzh = "我何时能见到「" + who + "」？"; break;
					case "When will we meet": qzh = "我们何时能见到「" + who + "」？"; break;
				}
				return qzh;
			}

			ma = Regex.Match( en, @"^(.+) sold (.+) for (\d+) (gold|silver|copper|jewels|crystals)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string item = Cmp( viewer, ma.Groups[2].Value.Trim() );
				return "「" + ma.Groups[1].Value.Trim() + "」以" + ma.Groups[3].Value + "枚" + MapCoin( ma.Groups[4].Value ) + "卖掉了「" + item + "」。";
			}

			ma = Regex.Match( en, @"^I paid (.+) (\d+) (gold|silver|copper|jewels|crystals) for (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string item = Cmp( viewer, ma.Groups[4].Value.Trim() );
				return "我付给「" + ma.Groups[1].Value.Trim() + "」" + ma.Groups[2].Value + "枚" + MapCoin( ma.Groups[3].Value ) + "，买下了「" + item + "」。";
			}

			// Defer "A/An … found …. " (sentence ending with period) to CommonTalkDynamicZh.FormatArticleRoleFoundRumorZh.
			// Otherwise the greedy (.+)(found)(.+) rule below yields 「A queen」… and broken token polish.
			if ( Regex.IsMatch( en, @"^((?:A|An)\s+.+?)\s+found\s+(.+)\.$", RegexOptions.CultureInvariant ) )
				return null;

			ma = Regex.Match( en, @"^(.+) (destroyed|sold|lost|found|discovered|traded|stole) (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」" + MapRelicAct( ma.Groups[2].Value ) + "「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^(.+) (robbed|assassinated|betrayed|captured|fooled|killed|swindled) them, I just know it\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」肯定" + MapThemAct( ma.Groups[2].Value ) + "他们，我心里有数。";

			ma = Regex.Match( en, @"^(.+) (bought that from|stole that from|sold that to|met with|kidnapped|robbed|works for|lives with|owes \d+ gold to) a (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string verb = MapTradeVerb( ma.Groups[2].Value );
				string job = Cmp( viewer, ma.Groups[3].Value.Trim() );
				string city = Cmp( viewer, ma.Groups[4].Value.Trim() );
				if ( ma.Groups[2].Value.IndexOf( "owes", StringComparison.OrdinalIgnoreCase ) >= 0 )
					return "「" + ma.Groups[1].Value.Trim() + "」" + verb + "「" + city + "」的那位" + job + "。";
				if ( ma.Groups[2].Value.Equals( "lives with", StringComparison.OrdinalIgnoreCase ) )
					return "「" + ma.Groups[1].Value.Trim() + "」与「" + city + "」的" + job + "同住。";
				if ( ma.Groups[2].Value.Equals( "works for", StringComparison.OrdinalIgnoreCase ) )
					return "「" + ma.Groups[1].Value.Trim() + "」效力于「" + city + "」的" + job + "。";
				return "「" + ma.Groups[1].Value.Trim() + "」在「" + city + "」" + verb + "那位" + job + "。";
			}

			ma = Regex.Match( en, @"^(.+) (robbed|assassinated|betrayed|captured|met|killed|left|followed|served|arrested) (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」在「" + Cmp( viewer, ma.Groups[4].Value.Trim() ) + "」" + MapSceneAct( ma.Groups[2].Value ) + "了「" + ma.Groups[3].Value.Trim() + "」。";

			ma = Regex.Match( en, @"^(.+) was (executed|jailed|arrested|captured|banished|rewarded|celebrated|promoted|released) for killing that (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string jobEn = ma.Groups[3].Value.Trim();
				string jobZh = NpcSpeechTokenZh.TranslateAdventurerZh( jobEn.ToLowerInvariant() );
				if ( jobZh == null || jobZh.Length == 0 || jobZh.Equals( jobEn, StringComparison.OrdinalIgnoreCase ) )
					jobZh = Cmp( viewer, jobEn );
				return "「" + ma.Groups[1].Value.Trim() + "」因在「" + Cmp( viewer, ma.Groups[4].Value.Trim() ) + "」杀害那位" + jobZh + "而被" + MapExecuted( ma.Groups[2].Value ) + "。";
			}

			ma = Regex.Match( en, @"^I heard (.+) became a (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "听说「" + ma.Groups[1].Value.Trim() + "」在「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」当了" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "。";

			ma = Regex.Match( en, @"^I need to see the (.+) before we travel on\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "上路前我得先见见那位" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "。";

			ma = Regex.Match( en, @"^(.+) retired and became a (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」退隐后在「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」当了" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "。";

			ma = Regex.Match( en, @"^(.+) has been selling body parts to the black magic guild\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」一直在向黑魔法行会兜售尸块。";

			ma = Regex.Match( en, @"^(.+) sold that monster skull to the necromancers for (\d+) gold\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」把那具怪物的颅骨卖给了死灵法师，换了" + ma.Groups[2].Value + "枚金币。";

			ma = Regex.Match( en, @"^The (.+) in (.+) is looking for some help with (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」的那位" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "想请人帮忙对付「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^(.+) sank off the coast of the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」在「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」近海沉没了。";

			ma = Regex.Match( en, @"^I found a map that leads to (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我弄到一纸地图，指向「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^The (.+) are going to (attack|destroy|invade|war with|be defeated by|be attacked by) the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string act = ma.Groups[2].Value.ToLowerInvariant();
				string actZh = act;
				switch ( act )
				{
					case "attack": actZh = "进攻"; break;
					case "destroy": actZh = "摧毁"; break;
					case "invade": actZh = "入侵"; break;
					case "war with": actZh = "与"; break;
					case "be defeated by": actZh = "败于"; break;
					case "be attacked by": actZh = "遭"; break;
				}
				string troops = Cmp( viewer, ma.Groups[1].Value.Trim() );
				string realm = Cmp( viewer, ma.Groups[3].Value.Trim() );
				if ( act == "war with" )
					return "「" + troops + "」将与「" + realm + "」开战。";
				if ( act == "be defeated by" )
					return "「" + realm + "」将败于「" + troops + "」。";
				if ( act == "be attacked by" )
					return "「" + realm + "」将遭「" + troops + "」袭击。";
				return "「" + troops + "」将要" + actZh + "「" + realm + "」。";
			}

			ma = Regex.Match( en, @"^We should build that (tower|castle|mansion|keep|home|cabin) in the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string b = ma.Groups[1].Value.ToLowerInvariant();
				string bzh = b;
				switch ( b )
				{
					case "tower": bzh = "塔楼"; break;
					case "castle": bzh = "城堡"; break;
					case "mansion": bzh = "宅邸"; break;
					case "keep": bzh = "要塞"; break;
					case "home": bzh = "居所"; break;
					case "cabin": bzh = "小屋"; break;
				}
				return "我们该在「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」起一座" + bzh + "。";
			}

			ma = Regex.Match( en, @"^We need to get to (.+) before (.+) does\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我们得赶在「" + ma.Groups[2].Value.Trim() + "」之前抵达「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^The (.+) in (.+) has (.+) for sale\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」的那位" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "手上有「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」待售。";

			ma = Regex.Match( en, @"^The (.+) is offering gold to rid the (.+) of (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "有位" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "悬赏黄金，要清剿「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」境内的「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^I think we got the most treasure out of (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我看咱们在「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」捞到的财宝最多。";

			ma = Regex.Match( en, @"^(.+) (swore allegiance to|was jailed by|was killed by|spied on|robbed|assassinated|met|betrayed|serves|killed) the (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string v = ma.Groups[2].Value.ToLowerInvariant();
				string vz = v;
				switch ( v )
				{
					case "robbed": vz = "抢劫了"; break;
					case "assassinated": vz = "刺杀了"; break;
					case "met": vz = "会见了"; break;
					case "spied on": vz = "监视了"; break;
					case "betrayed": vz = "背叛了"; break;
					case "swore allegiance to": vz = "宣誓效忠于"; break;
					case "serves": vz = "侍奉"; break;
					case "was jailed by": vz = "被下狱于"; break;
					case "was killed by": vz = "被杀害于"; break;
					case "killed": vz = "杀害了"; break;
				}
				return "「" + ma.Groups[1].Value.Trim() + "」在「" + Cmp( viewer, ma.Groups[4].Value.Trim() ) + "」" + vz + "那位" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "。";
			}

			ma = Regex.Match( en, @"^There is a bounty of  (\d+) gold for (.+) the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "悬赏" + ma.Groups[1].Value.Trim() + "枚金币，要拿「" + ma.Groups[2].Value.Trim() + "」这位" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "。";

			ma = Regex.Match( en, @"^The (.+) said for great treasure we need to go to (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "那位" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "说，若要发大财，就得去「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^(.+) (hid|lost|left|hidden|found|discovered|created) (.+) deep in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string vz = ma.Groups[2].Value.ToLowerInvariant();
				string z = vz;
				switch ( vz )
				{
					case "hid": z = "把"; break;
					case "lost": z = "把"; break;
					case "left": z = "把"; break;
					case "hidden": z = "把"; break;
					case "found": z = "在"; break;
					case "discovered": z = "在"; break;
					case "created": z = "在"; break;
				}
				string item = Cmp( viewer, ma.Groups[3].Value.Trim() );
				string dun = Cmp( viewer, ma.Groups[4].Value.Trim() );
				if ( vz == "found" || vz == "discovered" )
					return "「" + ma.Groups[1].Value.Trim() + "」在「" + dun + "」深处" + ( vz == "found" ? "找到了" : "发现了" ) + "「" + item + "」。";
				if ( vz == "created" )
					return "「" + ma.Groups[1].Value.Trim() + "」在「" + dun + "」深处造出了「" + item + "」。";
				return "「" + ma.Groups[1].Value.Trim() + "」把「" + item + "」藏进了「" + dun + "」深处。";
			}

			ma = Regex.Match( en, @"^(.+) found a magic (mirror|portal) that led to (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string p = ma.Groups[2].Value.ToLowerInvariant() == "mirror" ? "魔镜" : "魔门";
				return "「" + ma.Groups[1].Value.Trim() + "」发现一道" + p + "，通往「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";
			}

			ma = Regex.Match( en, @"^(.+) drank from a strange pool in (.+) and (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」在「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」一汪怪泉边饮了水，结果" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "。";

			ma = Regex.Match( en, @"^(.+) died in (.+) from (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + ma.Groups[1].Value.Trim() + "」死于「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」的「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^(.+) was (killed|slain|defeated|almost killed|almost slain|almost defeated) by (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string mob = Cmp( viewer, ma.Groups[3].Value.Trim() );
				string place = Cmp( viewer, ma.Groups[4].Value.Trim() );
				return "「" + ma.Groups[1].Value.Trim() + "」在「" + place + "」遭「" + mob + "」" + MapKillByMonster( ma.Groups[2].Value ) + "。";
			}

			ma = Regex.Match( en, @"^(Let me tell you|Tell me) the (tale|story|fable|legend|myth) of (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string head = ma.Groups[1].Value.StartsWith( "Tell me", StringComparison.Ordinal ) ? "跟我说说" : "且听我说说";
				string kind = ma.Groups[2].Value.ToLowerInvariant();
				string kz = kind == "tale" ? "轶闻" : kind == "story" ? "故事" : kind == "fable" ? "寓言" : kind == "legend" ? "传说" : "神话";
				return head + "那" + kz + "——「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";
			}

			ma = Regex.Match( en, @"^(.+) (died|went missing|has been|almost died|never returned while|vanished|perished) (searching for|looking for|trying to find|trying to locate) (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string who = ma.Groups[1].Value.Trim();
				string fate = ma.Groups[2].Value.ToLowerInvariant();
				string goal = Cmp( viewer, ma.Groups[4].Value.Trim() );
				string cv = ma.Groups[3].Value.ToLowerInvariant();
				string cvz = cv.IndexOf( "search", StringComparison.Ordinal ) >= 0 ? "搜寻" : "寻觅";
				if ( fate == "has been" )
					return "「" + who + "」长久以来都在" + cvz + "「" + goal + "」。";
				switch ( fate )
				{
					case "died":
						return "「" + who + "」死在了" + cvz + "「" + goal + "」的路上。";
					case "went missing":
						return "「" + who + "」在" + cvz + "「" + goal + "」时失踪了。";
					case "almost died":
						return "「" + who + "」在" + cvz + "「" + goal + "」时险些丧命。";
					case "never returned while":
						return "「" + who + "」一去不返，当时正在" + cvz + "「" + goal + "」。";
					case "vanished":
						return "「" + who + "」在" + cvz + "「" + goal + "」时消失了。";
					case "perished":
						return "「" + who + "」殒命于" + cvz + "「" + goal + "」的途中。";
					default:
						return "「" + who + "」" + cvz + "「" + goal + "」。";
				}
			}

			ma = Regex.Match( en, @"^a (prize|fee|reward|tribute|sack|chest|coffer|pile) of (\d+) gold if we (kill|find|slay|assassinate|rescue|kidnap|free|help|capture) (.+) the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string pz = MapPrize( ma.Groups[1].Value );
				string vz = MapBountyVerb( ma.Groups[3].Value );
				string name = ma.Groups[4].Value.Trim();
				string job = Cmp( viewer, ma.Groups[5].Value.Trim() );
				return "悬赏" + pz + "：" + ma.Groups[2].Value + "枚金币，条件是我们要" + vz + "「" + name + "」这位" + job + "。";
			}

			ma = Regex.Match( en, @"^((?:a|an) (?:war|battle|alliance|pact|trade agreement|tournament|standoff|blockade|dispute)) between (.+) and (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "据说在「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」与「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」之间起了" + MapWarKindPhrase( ma.Groups[1].Value.Trim() ) + "。";

			ma = Regex.Match( en, @"^((?:a|an) (?:war|battle|alliance|pact|trade agreement|tournament|standoff|blockade|dispute)) between the (.+) and the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "据说「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」与「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」之间起了" + MapWarKindPhrase( ma.Groups[1].Value.Trim() ) + "。";

			ma = Regex.Match( en, @"^(.+) marrying (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "据说「" + ma.Groups[1].Value.Trim() + "」与「" + ma.Groups[2].Value.Trim() + "」将于「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」成婚。";

			ma = Regex.Match( en, @"^the (.+) of (.+) marrying the (.+) of (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "据说「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」的" + ma.Groups[1].Value.Trim() + "将与「" + Cmp( viewer, ma.Groups[4].Value.Trim() ) + "」的" + ma.Groups[3].Value.Trim() + "联姻。";

			ma = Regex.Match( en, @"^(.+) marrying the (.+) of (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "据说「" + ma.Groups[1].Value.Trim() + "」将嫁给「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」的" + ma.Groups[2].Value.Trim() + "。";

			ma = Regex.Match( en, @"^(.+) marrying the (.+) of the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "据说「" + ma.Groups[1].Value.Trim() + "」将嫁给「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」的" + ma.Groups[2].Value.Trim() + "。";

			ma = Regex.Match( en, @"^(.+) becoming the (.+) of (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "据说「" + ma.Groups[1].Value.Trim() + "」将成为「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」的" + ma.Groups[2].Value.Trim() + "。";

			ma = Regex.Match( en, @"^(.+) becoming the (.+) of the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "据说「" + ma.Groups[1].Value.Trim() + "」将成为「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」的" + ma.Groups[2].Value.Trim() + "。";

			ma = Regex.Match( en, @"^(.+?) having been (.+?) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string who = ma.Groups[1].Value.Trim();
				string st = ma.Groups[2].Value.Trim();
				string place = Cmp( viewer, ma.Groups[3].Value.Trim() );
				return "据说「" + who + "」在「" + place + "」" + MapHavingBeenClause( viewer, st ) + "。";
			}

			ma = Regex.Match( en, @"^(.+?) being (wanted|on trial|in jail|in prison|put to death|sought after|put in chains|sentenced|put in the iron maiden) for (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string who = ma.Groups[1].Value.Trim();
				string st = MapWantedState( ma.Groups[2].Value );
				string crime = MapCrime( ma.Groups[3].Value.Trim() );
				string place = Cmp( viewer, ma.Groups[4].Value.Trim() );
				return "据说「" + who + "」于「" + place + "」因「" + crime + "」" + st + "。";
			}

			ma = Regex.Match( en, @"^(.+?) (hiding|burying|bringing|losing|finding|searching for|delivering|leaving) the (.+?) ((?:somewhere )?(?:near|on the outskirts of|outside|inside|in|by)) (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return MapPersonLootSentence( viewer, ma.Groups[1].Value.Trim(), ma.Groups[2].Value, ma.Groups[3].Value, ma.Groups[4].Value, ma.Groups[5].Value );

			ma = Regex.Match( en, @"^the (.+) being (hidden|buried|lost|waiting) (.+) (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string loot = Cmp( viewer, ma.Groups[1].Value.Trim() );
				string st = MapLootState( ma.Groups[2].Value );
				string loc = ma.Groups[3].Value.Trim();
				string town = Cmp( viewer, ma.Groups[4].Value.Trim() );
				return "据说那批「" + loot + "」正被" + st + "，" + MapLocalePrep( loc, town, viewer ) + "。";
			}

			ma = Regex.Match( en, @"^(.+?) (killing|slaying|being killed by|being slain by|fleeing from|chasing|hunting for|searching for|never finding) (.+) in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return MapMonsterHuntSentence( viewer, ma.Groups[1].Value.Trim(), ma.Groups[2].Value, ma.Groups[3].Value, ma.Groups[4].Value );

			ma = Regex.Match( en, @"^(.+) being (destroyed|captured|invaded|rescued|freed|ruined|taken|surrounded|settled) by the (.+) of (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
			{
				string city = ma.Groups[1].Value.Trim();
				string fate = ma.Groups[2].Value.ToLowerInvariant();
				string unit = ma.Groups[3].Value.Trim();
				string realm = ma.Groups[4].Value.Trim();
				return "据悉「" + Cmp( viewer, city ) + "」遭「" + Cmp( viewer, realm ) + "」麾下" + MapMilUnit( unit ) + "，终至「" + MapSiegeOutcome( fate ) + "」之局。";
			}

			ma = Regex.Match( en, @"^I finally learned how we can get the (.+)\. We need to assemble the others and meet at (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我终于弄清如何取得「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」。我们得召集同伴，在「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」会合。";

			ma = Regex.Match( en, @"^We need to go to (.+) if we are going to obtain the (.+) for (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "若要替「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」取得「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」，我们就得去「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^The (.+) in (.+) told me that we can probably get the (.+) if we search (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」的那位" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "说，只要去「" + Cmp( viewer, ma.Groups[4].Value.Trim() ) + "」搜寻，便有望得到「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^The (.+) in (.+) told me that someone can probably find (.+) if they search (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」的那位" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "说，若去「" + Cmp( viewer, ma.Groups[4].Value.Trim() ) + "」搜寻，或许便能寻得「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^The (.+) in (.+) told me that we can probably find (.+) if we search (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」的那位" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "说，只要去「" + Cmp( viewer, ma.Groups[4].Value.Trim() ) + "」搜寻，便有望找到「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^We need to go to (.+) if we are going to find the (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "若要寻得「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」，我们就得去「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^We need to go to (.+) if we are going to find (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "若要取得「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」，我们就得去「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^I learned where one can find (.+)\. They would need to head to (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我终于弄清「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」可在何处寻得。须往「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」一行。";

			ma = Regex.Match( en, @"^One would need to go to (.+) if they are going to find (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "若要寻得「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」，须往「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^I finally learned where we can find (.+)\. We need to head to (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我终于弄清我们能在何处寻得「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」。我们得赶往「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^Lord British would tell me stories about (.+), and how it was in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "不列颠王曾对我讲起「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」，以及它在「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」时的情形。";

			ma = Regex.Match( en, @"^Someone in the castle went to (.+) and saw (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "堡里有人去了「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」，见到了「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」。";

			ma = Regex.Match( en, @"^I heard (.+) tell Lord British that (.+) was said to be in (.+)\.$", RegexOptions.CultureInvariant );
			if ( ma.Success )
				return "我听见「" + Cmp( viewer, ma.Groups[1].Value.Trim() ) + "」对不列颠王说，「" + Cmp( viewer, ma.Groups[2].Value.Trim() ) + "」似在「" + Cmp( viewer, ma.Groups[3].Value.Trim() ) + "」。";

			return null;
		}
	}
}
