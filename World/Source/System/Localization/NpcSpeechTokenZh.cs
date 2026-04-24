using System;
using System.Collections.Generic;

namespace Server.Localization
{
	/// <summary>
	/// Post-composite pass: replace common English job / adventurer / noble tokens with zh-Hans.
	/// Lives in the core assembly so <see cref="Server.Mobile"/> overhead and <see cref="CommonTalkDynamicZh"/> can use it (Scripts <c>Citizens</c> is not referenced from core).
	/// </summary>
	public static class NpcSpeechTokenZh
	{
		private static readonly string[] s_JobVocabEn = new string[] {
			"stable master", "guildmaster", "blacksmith", "jeweler", "provisioner", "banker", "minter", "waiter", "guard",
			"herbalist", "alchemist", "healer", "innkeeper", "bartender", "butcher", "shipwright", "scribe", "farmer", "sage", "mage", "tinker", "tailor", "weaver" };

		private static readonly string[] s_AdventurerVocabEn = new string[] {
			"marchioness", "viscountess", "baronetess", "baronet", "princess", "countess", "duchess", "empress", "archbishop", "chancellor", "marquise", "marquess", "marquis", "chevalier", "viscount", "monarch", "duke", "earl", "count", "tsar", "dame",
			"necromancer", "illusionist", "enchantress", "enchanter", "adventurer", "bandit", "barbarian", "bard", "baron", "baroness", "cavalier", "cleric", "conjurer", "defender", "diviner", "explorer", "fighter", "gladiator", "heretic", "hunter", "invoker", "emperor", "king", "knight", "lady", "lord", "mage", "magician", "mercenary", "minstrel", "monk", "mystic", "outlaw", "paladin", "priest", "priestess", "prince", "prophet", "queen", "ranger", "rogue", "sage", "scout", "seeker", "seer", "shaman", "slayer", "sorcerer", "sorceress", "summoner", "templar", "thief", "traveler", "warlock", "warrior", "witch", "wizard" };

		private static string[] s_NpcVocabEnLongestFirst;

		private static void EnsureNpcVocabEnOrder()
		{
			if ( s_NpcVocabEnLongestFirst != null )
				return;
			var set = new HashSet<string>( StringComparer.Ordinal );
			foreach ( string s in s_JobVocabEn )
			{
				if ( s != null && s.Length > 0 )
					set.Add( s );
			}
			foreach ( string s in s_AdventurerVocabEn )
			{
				if ( s != null && s.Length > 0 )
					set.Add( s );
			}
			var list = new List<string>( set );
			list.Sort( ( a, b ) => b.Length - a.Length );
			s_NpcVocabEnLongestFirst = list.ToArray();
		}

		private static bool IsAsciiLetter( char c )
		{
			return ( c >= 'a' && c <= 'z' ) || ( c >= 'A' && c <= 'Z' );
		}

		private static bool IsNpcVocabTokenBoundary( string s, int index, int len )
		{
			if ( index > 0 && IsAsciiLetter( s[index - 1] ) )
				return false;
			int after = index + len;
			if ( after < s.Length && IsAsciiLetter( s[after] ) )
				return false;
			return true;
		}

		private static string TryTranslateVocabToken( string en )
		{
			if ( en == null || en.Length == 0 )
				return en;
			string low = en.ToLowerInvariant();
			string t = TranslateJobZh( low );
			if ( t != low )
				return t;
			return TranslateAdventurerZh( low );
		}

		public static string ApplyNpcVocabularyTokensToZh( string s )
		{
			if ( s == null || s.Length == 0 )
				return s;
			EnsureNpcVocabEnOrder();
			for ( int i = 0; i < s_NpcVocabEnLongestFirst.Length; ++i )
			{
				string en = s_NpcVocabEnLongestFirst[i];
				if ( en == null || en.Length == 0 || s.Length < en.Length )
					continue;
				StringComparison cmp = StringComparison.OrdinalIgnoreCase;
				int idx = 0;
				while ( true )
				{
					idx = s.IndexOf( en, idx, cmp );
					if ( idx < 0 )
						break;
					if ( !IsNpcVocabTokenBoundary( s, idx, en.Length ) )
					{
						++idx;
						continue;
					}
					string zh = TryTranslateVocabToken( en );
					if ( zh != null && zh.Length > 0 && !zh.Equals( en, StringComparison.OrdinalIgnoreCase ) )
					{
						s = s.Remove( idx, en.Length ).Insert( idx, zh );
						idx += zh.Length;
					}
					else
						++idx;
				}
			}
			return s;
		}

		public static string TranslateAdventurerZh( string en )
		{
			switch ( en )
			{
				case "emperor":     return "皇帝";
				case "empress":     return "皇后";
				case "duke":        return "公爵";
				case "duchess":     return "公爵夫人";
				case "marquess":    return "侯爵";
				case "marchioness": return "侯爵夫人";
				case "marquis":     return "侯爵";
				case "marquise":    return "侯爵夫人";
				case "earl":        return "伯爵";
				case "count":       return "伯爵";
				case "countess":    return "伯爵夫人";
				case "viscount":    return "子爵";
				case "viscountess": return "子爵夫人";
				case "baronet":     return "从男爵";
				case "baronetess":  return "女从男爵";
				case "chevalier":   return "勋爵士";
				case "tsar":        return "沙皇";
				case "monarch":     return "君主";
				case "archbishop":  return "大主教";
				case "chancellor":  return "宰相";
				case "dame":        return "女爵士";
				case "adventurer": return "冒险者";
				case "bandit":     return "强盗";
				case "barbarian":  return "蛮族战士";
				case "bard":       return "吟游诗人";
				case "baron":      return "男爵";
				case "baroness":   return "女男爵";
				case "cavalier":   return "骑士";
				case "cleric":     return "神职者";
				case "conjurer":   return "召唤师";
				case "defender":   return "卫士";
				case "diviner":    return "占卜师";
				case "enchanter":  return "附魔师";
				case "enchantress":return "女附魔师";
				case "explorer":   return "探索者";
				case "fighter":    return "战士";
				case "gladiator":  return "角斗士";
				case "heretic":    return "异教徒";
				case "hunter":     return "猎人";
				case "illusionist":return "幻术师";
				case "invoker":    return "咒召师";
				case "king":       return "国王";
				case "knight":     return "骑士";
				case "lady":       return "贵妇";
				case "lord":       return "领主";
				case "mage":       return "法师";
				case "magician":   return "魔法师";
				case "mercenary":  return "佣兵";
				case "minstrel":   return "吟游歌手";
				case "monk":       return "修士";
				case "mystic":     return "神秘者";
				case "necromancer":return "死灵法师";
				case "outlaw":     return "亡命之徒";
				case "paladin":    return "圣骑士";
				case "priest":     return "祭司";
				case "priestess":  return "女祭司";
				case "prince":     return "王子";
				case "princess":   return "公主";
				case "prophet":    return "先知";
				case "queen":      return "女王";
				case "ranger":     return "游侠";
				case "rogue":      return "盗贼";
				case "sage":       return "智者";
				case "scout":      return "斥候";
				case "seeker":     return "寻觅者";
				case "seer":       return "预言者";
				case "shaman":     return "萨满";
				case "slayer":     return "杀手";
				case "sorcerer":   return "术士";
				case "sorceress":  return "女术士";
				case "summoner":   return "召唤师";
				case "templar":    return "圣殿骑士";
				case "thief":      return "盗贼";
				case "traveler":   return "旅人";
				case "warlock":    return "战锁";
				case "warrior":    return "战士";
				case "witch":      return "女巫";
				case "wizard":     return "巫师";
				default:           return en;
			}
		}

		public static string TranslateJobZh( string en )
		{
			switch ( en )
			{
				case "blacksmith":    return "铁匠";
				case "jeweler":       return "珠宝商";
				case "provisioner":   return "杂货商";
				case "banker":        return "银行家";
				case "minter":        return "铸币匠";
				case "waiter":        return "服务生";
				case "guard":         return "卫兵";
				case "sage":          return "智者";
				case "mage":          return "法师";
				case "herbalist":     return "草药师";
				case "alchemist":     return "炼金师";
				case "healer":        return "治疗师";
				case "guildmaster":   return "公会长";
				case "tinker":        return "机关匠";
				case "innkeeper":     return "客栈老板";
				case "bartender":     return "酒保";
				case "butcher":       return "屠夫";
				case "tailor":        return "裁缝";
				case "weaver":        return "织工";
				case "shipwright":    return "造船师";
				case "scribe":        return "书记员";
				case "farmer":        return "农夫";
				case "stable master": return "马厩管理员";
				default:              return en;
			}
		}
	}
}
