using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using System;
using Server.Spells.Seventh;
using System.Linq;

namespace Server.Misc
{
    class GetPlayerInfo
    {
		public static string GetSkillTitle( Mobile m )
		{
			bool isOriental = Server.Misc.GetPlayerInfo.OrientalPlay( m );
			bool isEvil = Server.Misc.GetPlayerInfo.EvilPlay( m );
			int isBarbaric = Server.Misc.GetPlayerInfo.BarbaricPlay( m );

			if ( ((PlayerMobile)m).CharacterSkill == 66 )
			{
				return "Titan of Ether";
			}
			else if ( m.SkillsTotal > 0 )
			{
				Skill highest = GetShowingSkill( m );

				if ( highest != null )
				{
					if ( highest.Value < 0.1 )
					{
						return "Village Idiot";
					}
					else
					{
						string skillLevel = null;
						if ( highest.Value < 29.1 ){ skillLevel = "Aspiring"; }
						else { skillLevel = GetSkillLevel( highest ); }

						string skillTitle = highest.Info.Title;

						skillTitle = Skill.CharacterTitle( skillTitle, m.Female, m.Karma, m.Skills[SkillName.Knightship].Value, m.Skills[SkillName.Seafaring].Value, m.Skills[SkillName.Magery].Base, m.Skills[SkillName.Necromancy].Base, m.Skills[SkillName.Healing].Base, m.Skills[SkillName.Spiritualism].Base, isBarbaric, isOriental, isMonk(m), isSyth(m,false), isJedi(m,false), isJester(m), isEvil );

						return String.Concat( skillLevel, " ", skillTitle );
					}
				}
			}

			return "Village Idiot";
		}

		public static bool isMonk ( Mobile m )
		{
			int points = 0;

			Spellbook book = Spellbook.FindMystic( m );
			if ( book is MysticSpellbook )
			{
				MysticSpellbook tome = (MysticSpellbook)book;
				if ( tome.owner == m )
				{
					points++;
				}
			}

			if ( Server.Spells.Mystic.MysticSpell.MonkNotIllegal( m ) ){ points++; }

			if ( points > 1 )
				return true;

			return false;
		}

		public static bool isFromSpace( Mobile m )
		{
			return MySettings.S_AllowAlienChoice && m is PlayerMobile && ((PlayerMobile)m).CharacterType == CharacterType.Alien;
		}

		public static bool isSyth ( Mobile m, bool checkSword )
		{
			int points = 0;

			Spellbook book = Spellbook.FindSyth( m );
			if ( book is SythSpellbook )
			{
				SythSpellbook tome = (SythSpellbook)book;
				if ( tome.owner == m )
				{
					points++;
				}
			}

			if ( Server.Spells.Syth.SythSpell.SythNotIllegal( m, checkSword ) ){ points++; }

			if ( points > 1 )
				return true;

			return false;
		}

		public static bool isJedi ( Mobile m, bool checkSword )
		{
			int points = 0;

			Spellbook book = Spellbook.FindJedi( m );
			if ( book is JediSpellbook )
			{
				JediSpellbook tome = (JediSpellbook)book;
				if ( tome.owner == m )
				{
					points++;
				}
			}

			if ( Server.Spells.Jedi.JediSpell.JediNotIllegal( m, checkSword ) ){ points++; }

			if ( points > 1 )
				return true;

			return false;
		}

		public static bool isJester ( Mobile from )
		{
			if ( from is PlayerMobile && from != null && from.Backpack != null )
			{
				var bagOfTricks = from.Backpack.FindItemByType( typeof( BagOfTricks ), true );
				if ( bagOfTricks == null ) return false;
				if ( from.Skills[SkillName.Begging].Value < 10 && from.Skills[SkillName.Psychology].Value < 10 ) return false;

				return CheckGraphicEquipped( from, from.FindItemOnLayer( Layer.OuterTorso ), 0x1f9f, 0x1fa0, 0x4C16, 0x4C17, 0x2B6B, 0x3162 )
					|| CheckGraphicEquipped( from, from.FindItemOnLayer( Layer.MiddleTorso ), 0x1f9f, 0x1fa0, 0x4C16, 0x4C17, 0x2B6B, 0x3162 )
					|| CheckGraphicEquipped( from, from.FindItemOnLayer( Layer.Helm ), 0x171C, 0x4C15 )
					|| CheckGraphicEquipped( from, from.FindItemOnLayer( Layer.Shoes ), 0x4C27 );
			}

			return false;
		}

		private static bool CheckGraphicEquipped( Mobile from, Item item, params int[] itemIds )
		{
			return item != null && itemIds.Any( id => item.ItemID == id || (0 < from.RaceID && item.GraphicID == id) );
		}

		private static Skill GetHighestSkill( Mobile m )
		{
			Skills skills = m.Skills;

			if ( !Core.AOS )
				return skills.Highest;

			Skill highest = m.Skills[SkillName.FistFighting];

			for ( int i = 0; i < m.Skills.Length; ++i )
			{
				Skill check = m.Skills[i];

				if ( highest == null || check.Value > highest.Value )
					highest = check;
				else if ( highest != null && highest.Lock != SkillLock.Up && check.Lock == SkillLock.Up && check.Value == highest.Value )
					highest = check;
			}

			return highest;
		}

		private static string[,] m_Levels = new string[,]
			{
				{ "Neophyte",		"Neophyte",		"Neophyte"		},
				{ "Novice",			"Novice",		"Novice"		},
				{ "Apprentice",		"Apprentice",	"Apprentice"	},
				{ "Journeyman",		"Journeyman",	"Journeyman"	},
				{ "Expert",			"Expert",		"Expert"		},
				{ "Adept",			"Adept",		"Adept"			},
				{ "Master",			"Master",		"Master"		},
				{ "Grandmaster",	"Grandmaster",	"Grandmaster"	},
				{ "Elder",			"Tatsujin",		"Shinobi"		},
				{ "Legendary",		"Kengo",		"Ka-ge"			}
			};

		private static string GetSkillLevel( Skill skill )
		{
			return m_Levels[GetTableIndex( skill ), GetTableType( skill )];
		}

		private static int GetTableType( Skill skill )
		{
			switch ( skill.SkillName )
			{
				default: return 0;
				case SkillName.Bushido: return 1;
				case SkillName.Ninjitsu: return 2;
			}
		}

		private static int GetTableIndex( Skill skill )
		{
			int fp = 0; // Math.Min( skill.BaseFixedPoint, 1200 );

			if ( skill.Value >= 120 ){ fp = 9; }
			else if ( skill.Value >= 110 ){ fp = 8; }
			else if ( skill.Value >= 100 ){ fp = 7; }
			else if ( skill.Value >= 90 ){ fp = 6; }
			else if ( skill.Value >= 80 ){ fp = 5; }
			else if ( skill.Value >= 70 ){ fp = 4; }
			else if ( skill.Value >= 60 ){ fp = 3; }
			else if ( skill.Value >= 50 ){ fp = 2; }
			else if ( skill.Value >= 40 ){ fp = 1; }
			else { fp = 0; }

			return fp;

			// return (fp - 300) / 100;
		}

		private static Skill GetShowingSkill( Mobile m )
		{
			Skill skill = GetHighestSkill( m );

			int NskillShow = ((PlayerMobile)m).CharacterSkill;

			if ( NskillShow > 0 )
			{
				if ( NskillShow == 1 ){ skill = m.Skills[SkillName.Alchemy]; }
				else if ( NskillShow == 2 ){ skill = m.Skills[SkillName.Anatomy]; }
				else if ( NskillShow == 3 ){ skill = m.Skills[SkillName.Druidism]; }
				else if ( NskillShow == 4 ){ skill = m.Skills[SkillName.Taming]; }
				else if ( NskillShow == 5 ){ skill = m.Skills[SkillName.Marksmanship]; }
				else if ( NskillShow == 6 ){ skill = m.Skills[SkillName.ArmsLore]; }
				else if ( NskillShow == 7 ){ skill = m.Skills[SkillName.Begging]; }
				else if ( NskillShow == 8 ){ skill = m.Skills[SkillName.Blacksmith]; }
				else if ( NskillShow == 9 ){ skill = m.Skills[SkillName.Bushido]; }
				else if ( NskillShow == 10 ){ skill = m.Skills[SkillName.Camping]; }
				else if ( NskillShow == 11 ){ skill = m.Skills[SkillName.Carpentry]; }
				else if ( NskillShow == 12 ){ skill = m.Skills[SkillName.Cartography]; }
				else if ( NskillShow == 13 ){ skill = m.Skills[SkillName.Knightship]; }
				else if ( NskillShow == 14 ){ skill = m.Skills[SkillName.Cooking]; }
				else if ( NskillShow == 15 ){ skill = m.Skills[SkillName.Searching]; }
				else if ( NskillShow == 16 ){ skill = m.Skills[SkillName.Discordance]; }
				else if ( NskillShow == 17 ){ skill = m.Skills[SkillName.Psychology]; }
				else if ( NskillShow == 18 ){ skill = m.Skills[SkillName.Fencing]; }
				else if ( NskillShow == 19 ){ skill = m.Skills[SkillName.Seafaring]; }
				else if ( NskillShow == 20 ){ skill = m.Skills[SkillName.Bowcraft]; }
				else if ( NskillShow == 21 ){ skill = m.Skills[SkillName.Focus]; }
				else if ( NskillShow == 22 ){ skill = m.Skills[SkillName.Forensics]; }
				else if ( NskillShow == 23 ){ skill = m.Skills[SkillName.Healing]; }
				else if ( NskillShow == 24 ){ skill = m.Skills[SkillName.Herding]; }
				else if ( NskillShow == 25 ){ skill = m.Skills[SkillName.Hiding]; }
				else if ( NskillShow == 26 ){ skill = m.Skills[SkillName.Inscribe]; }
				else if ( NskillShow == 27 ){ skill = m.Skills[SkillName.Mercantile]; }
				else if ( NskillShow == 28 ){ skill = m.Skills[SkillName.Lockpicking]; }
				else if ( NskillShow == 29 ){ skill = m.Skills[SkillName.Lumberjacking]; }
				else if ( NskillShow == 30 ){ skill = m.Skills[SkillName.Bludgeoning]; }
				else if ( NskillShow == 31 ){ skill = m.Skills[SkillName.Magery]; }
				else if ( NskillShow == 32 ){ skill = m.Skills[SkillName.MagicResist]; }
				else if ( NskillShow == 33 ){ skill = m.Skills[SkillName.Meditation]; }
				else if ( NskillShow == 34 ){ skill = m.Skills[SkillName.Mining]; }
				else if ( NskillShow == 35 ){ skill = m.Skills[SkillName.Musicianship]; }
				else if ( NskillShow == 36 ){ skill = m.Skills[SkillName.Necromancy]; }
				else if ( NskillShow == 37 ){ skill = m.Skills[SkillName.Ninjitsu]; }
				else if ( NskillShow == 38 ){ skill = m.Skills[SkillName.Parry]; }
				else if ( NskillShow == 39 ){ skill = m.Skills[SkillName.Peacemaking]; }
				else if ( NskillShow == 40 ){ skill = m.Skills[SkillName.Poisoning]; }
				else if ( NskillShow == 41 ){ skill = m.Skills[SkillName.Provocation]; }
				else if ( NskillShow == 42 ){ skill = m.Skills[SkillName.RemoveTrap]; }
				else if ( NskillShow == 43 ){ skill = m.Skills[SkillName.Snooping]; }
				else if ( NskillShow == 44 ){ skill = m.Skills[SkillName.Spiritualism]; }
				else if ( NskillShow == 45 ){ skill = m.Skills[SkillName.Stealing]; }
				else if ( NskillShow == 46 ){ skill = m.Skills[SkillName.Stealth]; }
				else if ( NskillShow == 47 ){ skill = m.Skills[SkillName.Swords]; }
				else if ( NskillShow == 48 ){ skill = m.Skills[SkillName.Tactics]; }
				else if ( NskillShow == 49 ){ skill = m.Skills[SkillName.Tailoring]; }
				else if ( NskillShow == 50 ){ skill = m.Skills[SkillName.Tasting]; }
				else if ( NskillShow == 51 ){ skill = m.Skills[SkillName.Tinkering]; }
				else if ( NskillShow == 52 ){ skill = m.Skills[SkillName.Tracking]; }
				else if ( NskillShow == 53 ){ skill = m.Skills[SkillName.Veterinary]; }
				else if ( NskillShow == 54 ){ skill = m.Skills[SkillName.FistFighting]; }
				else if ( NskillShow == 55 ){ skill = m.Skills[SkillName.Elementalism]; }
				else { skill = GetHighestSkill( m ); }
			}

			return skill;
		}

		public static string GetNPCGuild( Mobile m )
		{
			string GuildTitle = null;

			if ( m is PlayerMobile )
			{
				PlayerMobile pm = (PlayerMobile)m;

				if ( pm.Fugitive == 1 && m.RaceID < 1 ){ GuildTitle = "Fugitive"; }
				else if ( pm.NpcGuild == NpcGuild.MagesGuild ){ GuildTitle = "Wizards Guild"; }
				else if ( pm.NpcGuild == NpcGuild.WarriorsGuild ){ GuildTitle = "Warriors Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ThievesGuild ){ GuildTitle = "Thieves Guild"; }
				else if ( pm.NpcGuild == NpcGuild.RangersGuild ){ GuildTitle = "Rangers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.HealersGuild ){ GuildTitle = "Healers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.MinersGuild ){ GuildTitle = "Miners Guild"; }
				else if ( pm.NpcGuild == NpcGuild.MerchantsGuild ){ GuildTitle = "Merchants Guild"; }
				else if ( pm.NpcGuild == NpcGuild.TinkersGuild ){ GuildTitle = "Tinkers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.TailorsGuild ){ GuildTitle = "Tailors Guild"; }
				else if ( pm.NpcGuild == NpcGuild.FishermensGuild ){ GuildTitle = "Mariners Guild"; }
				else if ( pm.NpcGuild == NpcGuild.BardsGuild ){ GuildTitle = "Bards Guild"; }
				else if ( pm.NpcGuild == NpcGuild.BlacksmithsGuild ){ GuildTitle = "Blacksmiths Guild"; }
				else if ( pm.NpcGuild == NpcGuild.NecromancersGuild ){ GuildTitle = "Black Magic Guild"; }
				else if ( pm.NpcGuild == NpcGuild.AlchemistsGuild ){ GuildTitle = "Alchemists Guild"; }
				else if ( pm.NpcGuild == NpcGuild.DruidsGuild ){ GuildTitle = "Druids Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ArchersGuild ){ GuildTitle = "Archers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CarpentersGuild ){ GuildTitle = "Carpenters Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CartographersGuild ){ GuildTitle = "Cartographers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.LibrariansGuild ){ GuildTitle = "Librarians Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CulinariansGuild ){ GuildTitle = "Culinary Guild"; }
				else if ( pm.NpcGuild == NpcGuild.AssassinsGuild ){ GuildTitle = "Assassins Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ElementalGuild ){ GuildTitle = "Elemental Guild"; }
			}
			else if ( m is BaseVendor )
			{
				BaseVendor pm = (BaseVendor)m;

				if ( pm.NpcGuild == NpcGuild.MagesGuild ){ GuildTitle = "Wizards Guild"; }
				else if ( pm.NpcGuild == NpcGuild.WarriorsGuild ){ GuildTitle = "Warriors Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ThievesGuild ){ GuildTitle = "Thieves Guild"; }
				else if ( pm.NpcGuild == NpcGuild.RangersGuild ){ GuildTitle = "Rangers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.HealersGuild ){ GuildTitle = "Healers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.MinersGuild ){ GuildTitle = "Miners Guild"; }
				else if ( pm.NpcGuild == NpcGuild.MerchantsGuild ){ GuildTitle = "Merchants Guild"; }
				else if ( pm.NpcGuild == NpcGuild.TinkersGuild ){ GuildTitle = "Tinkers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.TailorsGuild ){ GuildTitle = "Tailors Guild"; }
				else if ( pm.NpcGuild == NpcGuild.FishermensGuild ){ GuildTitle = "Mariners Guild"; }
				else if ( pm.NpcGuild == NpcGuild.BardsGuild ){ GuildTitle = "Bards Guild"; }
				else if ( pm.NpcGuild == NpcGuild.BlacksmithsGuild ){ GuildTitle = "Blacksmiths Guild"; }
				else if ( pm.NpcGuild == NpcGuild.NecromancersGuild ){ GuildTitle = "Black Magic Guild"; }
				else if ( pm.NpcGuild == NpcGuild.AlchemistsGuild ){ GuildTitle = "Alchemists Guild"; }
				else if ( pm.NpcGuild == NpcGuild.DruidsGuild ){ GuildTitle = "Druids Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ArchersGuild ){ GuildTitle = "Archers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CarpentersGuild ){ GuildTitle = "Carpenters Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CartographersGuild ){ GuildTitle = "Cartographers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.LibrariansGuild ){ GuildTitle = "Librarians Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CulinariansGuild ){ GuildTitle = "Culinary Guild"; }
				else if ( pm.NpcGuild == NpcGuild.AssassinsGuild ){ GuildTitle = "Assassins Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ElementalGuild ){ GuildTitle = "Elemental Guild"; }
			}
			return GuildTitle;
		}

		public static string GetStatusGuild( Mobile m )
		{
			string GuildTitle = "";

			if ( m is PlayerMobile )
			{
				PlayerMobile pm = (PlayerMobile)m;

				if ( pm.Fugitive == 1 ){ GuildTitle = "The Fugitive"; }
				else if ( pm.NpcGuild == NpcGuild.MagesGuild ){ GuildTitle = "The Wizards Guild"; }
				else if ( pm.NpcGuild == NpcGuild.WarriorsGuild ){ GuildTitle = "The Warriors Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ThievesGuild ){ GuildTitle = "The Thieves Guild"; }
				else if ( pm.NpcGuild == NpcGuild.RangersGuild ){ GuildTitle = "The Rangers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.HealersGuild ){ GuildTitle = "The Healers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.MinersGuild ){ GuildTitle = "The Miners Guild"; }
				else if ( pm.NpcGuild == NpcGuild.MerchantsGuild ){ GuildTitle = "The Merchants Guild"; }
				else if ( pm.NpcGuild == NpcGuild.TinkersGuild ){ GuildTitle = "The Tinkers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.TailorsGuild ){ GuildTitle = "The Tailors Guild"; }
				else if ( pm.NpcGuild == NpcGuild.FishermensGuild ){ GuildTitle = "The Mariners Guild"; }
				else if ( pm.NpcGuild == NpcGuild.BardsGuild ){ GuildTitle = "The Bards Guild"; }
				else if ( pm.NpcGuild == NpcGuild.BlacksmithsGuild ){ GuildTitle = "The Blacksmiths Guild"; }
				else if ( pm.NpcGuild == NpcGuild.NecromancersGuild ){ GuildTitle = "The Black Magic Guild"; }
				else if ( pm.NpcGuild == NpcGuild.AlchemistsGuild ){ GuildTitle = "The Alchemists Guild"; }
				else if ( pm.NpcGuild == NpcGuild.DruidsGuild ){ GuildTitle = "The Druids Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ArchersGuild ){ GuildTitle = "The Archers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CarpentersGuild ){ GuildTitle = "The Carpenters Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CartographersGuild ){ GuildTitle = "The Cartographers Guild"; }
				else if ( pm.NpcGuild == NpcGuild.LibrariansGuild ){ GuildTitle = "The Librarians Guild"; }
				else if ( pm.NpcGuild == NpcGuild.CulinariansGuild ){ GuildTitle = "The Culinary Guild"; }
				else if ( pm.NpcGuild == NpcGuild.AssassinsGuild ){ GuildTitle = "The Assassins Guild"; }
				else if ( pm.NpcGuild == NpcGuild.ElementalGuild ){ GuildTitle = "The Elemental Guild"; }
			}
			return GuildTitle;
		}

		public static int GetPlayerLevel( Mobile m )
		{
			int fame = m.Fame;
				if ( fame > 15000){ fame = 15000; }

			int karma = m.Karma;
				if ( karma < 0 ){ karma = m.Karma * -1; }
				if ( karma > 15000){ karma = 15000; }

			int skills = m.Skills.Total;
			int based = MyServerSettings.SkillBase();

				if ( skills > based ){ skills = based; }

				if ( based >= 20000 )
					skills = (int)( skills / 2 );
				else if ( based >= 19000 )
					skills = (int)( skills / 1.9 );
				else if ( based >= 18000 )
					skills = (int)( skills / 1.8 );
				else if ( based >= 17000 )
					skills = (int)( skills / 1.7 );
				else if ( based >= 16000 )
					skills = (int)( skills / 1.6 );
				else if ( based >= 15000 )
					skills = (int)( skills / 1.5 );
				else if ( based >= 14000 )
					skills = (int)( skills / 1.4 );
				else if ( based >= 13000 )
					skills = (int)( skills / 1.3 );
				else if ( based >= 12000 )
					skills = (int)( skills / 1.2 );
				else if ( based >= 11000 )
					skills = (int)( skills / 1.1 );

				skills = (int)( 1.5 * skills );			// UP TO 15,000

			int stats = m.RawStr + m.RawDex + m.RawInt;
				if ( stats > 250){ stats = 250; }
				stats = 60 * stats;						// UP TO 15,000

			int level = (int)( ( fame + karma + skills + stats ) / 600 );
				level = (int)( ( level - 10 ) * 1.12 );

			if ( level < 1 ){ level = 1; }
			if ( level > 100 ){ level = 100; }

			return level;
		}

		public static int GetPlayerDifficulty( Mobile m )
		{
			int difficulty = 0;
			int level = GetPlayerLevel( m );

			if ( level >=95 ){ difficulty = 4; }
			else if ( level >=75 ){ difficulty = 3; }
			else if ( level >=50 ){ difficulty = 2; }
			else if ( level >=25 ){ difficulty = 1; }

			return difficulty;
		}

		public static int GetResurrectCost( Mobile m, bool force = false )
		{
			if ( !force && ( m.SkillsTotal <= 200 || (m.RawDex + m.RawInt + m.RawStr) <= 90 ) )
				return 0;

			int fame = m.Fame;
				if ( fame > 15000){ fame = 15000; }
			int karma = m.Karma * -1;
				if ( karma > 15000){ karma = 15000; }

			int skills = m.Skills.Total;
				if ( skills > 10000){ skills = 10000; }
				skills = (int)( 1.5 * skills );			// UP TO 15,000

			int stats = m.RawStr + m.RawDex + m.RawInt;
				if ( stats > 250){ stats = 250; }
				stats = 60 * stats;						// UP TO 15,000

			int level = (int)( ( fame + karma + skills + stats ) / 600 );
				level = (int)( ( level - 10 ) * 1.12 );

			if ( level < 1 ){ level = 1; }
			if ( level > 100 ){ level = 100; }

			if ( level < MySettings.S_DeathPayLevel && level < 100 )
				return 0;

			int price = MySettings.S_DeathPayAmount;
				if ( price < 1 )
					price = 1;

			level = ( level * price );

			if ( ((PlayerMobile)m).Fugitive == 1 ){ level = level * 2; }
			else if ( GetPlayerInfo.isFromSpace( m ) ){ level = level * 3; }

			return level;
		}

		public static string GetTodaysDate()
		{
			string sYear = DateTime.Now.Year.ToString();
			string sMonth = DateTime.Now.Month.ToString();
				string sMonthName = "January";
				if ( sMonth == "2" ){ sMonthName = "February"; }
				else if ( sMonth == "3" ){ sMonthName = "March"; }
				else if ( sMonth == "4" ){ sMonthName = "April"; }
				else if ( sMonth == "5" ){ sMonthName = "May"; }
				else if ( sMonth == "6" ){ sMonthName = "June"; }
				else if ( sMonth == "7" ){ sMonthName = "July"; }
				else if ( sMonth == "8" ){ sMonthName = "August"; }
				else if ( sMonth == "9" ){ sMonthName = "September"; }
				else if ( sMonth == "10" ){ sMonthName = "October"; }
				else if ( sMonth == "11" ){ sMonthName = "November"; }
				else if ( sMonth == "12" ){ sMonthName = "December"; }
			string sDay = DateTime.Now.Day.ToString();
			string sHour = DateTime.Now.Hour.ToString();
			string sMinute = DateTime.Now.Minute.ToString();
			string sSecond = DateTime.Now.Second.ToString();

			if ( sHour.Length == 1 ){ sHour = "0" + sHour; }
			if ( sMinute.Length == 1 ){ sMinute = "0" + sMinute; }
			if ( sSecond.Length == 1 ){ sSecond = "0" + sSecond; }

			string sDateString = sMonthName + " " + sDay + ", " + sYear + " at " + sHour + ":" + sMinute;

			return sDateString;
		}

		public static bool CheckLuck( int luck, int freeChanceBonus = 0, int percentOfLuckToUse = 100 )
		{
			if ( luck <= 0 )
				return false;

			const int MAX_LUCK = 2000;
			const double LUCK_TO_PERCENT_CONVERSION = 100f / MAX_LUCK;

			luck = Math.Min( MAX_LUCK, luck ); // Cap luck
			if ( percentOfLuckToUse < 100 ) luck = luck * percentOfLuckToUse / 100; // Reduce luck

			int playerChance = freeChanceBonus + (int)( luck * LUCK_TO_PERCENT_CONVERSION );

			return Utility.RandomMinMax( 1, 100 ) <= playerChance;
		}

		/// <summary>
		/// Returns true up to 80% of the time
		/// </summary>
		/// <param name="luck">Only 80% of the provided Luck is used.</param>
		public static bool LuckyPlayer( int luck, int freePercentBonus = 0 )
		{
			return CheckLuck( luck, freePercentBonus, 80 );
		}

		/// <summary>
		/// Returns true up to 25% of the time
		/// </summary>
		/// <param name="luck">Only 50% of the provided Luck is used.</param>
		public static bool VeryLuckyKiller( int luck, int freePercentBonus = 0 )
		{
			if (!Utility.RandomBool()) return false;

			return LuckyKiller(luck, freePercentBonus);
		}

		/// <summary>
		/// Returns true up to 50% of the time
		/// </summary>
		/// <param name="luck">Only 50% of the provided Luck is used.</param>
		public static bool LuckyKiller( int luck, int freePercentBonus = 0 )
		{
			return CheckLuck( luck, freePercentBonus, 50 );
		}

		public static bool EvilPlayer( Mobile m )
		{
			if ( m is BaseCreature )
				m = ((BaseCreature)m).GetMaster();

			if ( m is PlayerMobile )
			{
				if ( m.AccessLevel > AccessLevel.Player 
					|| m.Skills[SkillName.Necromancy].Base >= 50.0 && m.Karma < 0 // NECROMANCERS
					|| m.Skills[SkillName.Forensics].Base >= 80.0 && m.Karma < 0 // UNDERTAKERS
					|| m.Skills[SkillName.Knightship].Base >= 50.0 && m.Karma <= -5000 // DEATH KNIGHTS
					|| m.Skills[SkillName.Psychology].Base >= 50.0 && m.Skills[SkillName.Swords].Base >= 50.0 && m.Karma <= -5000 && Server.Misc.GetPlayerInfo.isSyth(m,false) // SYTH
					|| Server.Items.BaseRace.IsRavendarkCreature( m ) // EVIL UNDEAD CREATURE PLAYERS
					|| ((PlayerMobile)m).Fugitive == 1) // OUTLAWS
				{
					return true;
				}
			}

			return false;
		}

		public static string GetWantedStatus( Mobile m )
		{
			string warning = "";
			string safe = "";
			bool umbra = true;

			if ( m is PlayerMobile )
			{
				if ( ((PlayerMobile)m).Fugitive == 1 || m.Kills > 0 )
				{
					warning = warning + "You are wanted for your murderous deeds! ";
					umbra = false;
				}
				else if ( m.Criminal )
				{
					warning = warning + "You are being sought as a criminal right now. ";
					umbra = false;
				}
				if ( m is PlayerMobile && ( m.Karma < 2500 || m.Fame < 2500 ) && Server.Items.BaseRace.IsEvil( m ) )
				{
					warning = warning + "You are considered by most to be a vile creature and not welcome in many settlements. ";
				}
				if ( m is PlayerMobile && m.Karma <= -5000 && m.Skills[SkillName.Knightship].Base >= 50 )
				{
					warning = warning + "You are a death knight, which is feared amongst the land. ";
				}
				if ( m is PlayerMobile && m.Karma <= -5000 && m.Skills[SkillName.Psychology].Base >= 50 && Server.Misc.GetPlayerInfo.isSyth(m, false) )
				{
					warning = warning + "You are a syth, and are not welcome in most settlements. ";
				}

				if ( DisguiseTimers.IsDisguised( m ) && warning != "" )
					warning = warning + "You could probably sneak into settlements, however, since you will not be recognized.";
				else if( !m.CanBeginAction( typeof( PolymorphSpell ) ) && warning != "" )
					warning = warning + "You could probably sneak into settlements, however, since you will not be recognized.";

				safe = "<BR><BR>SAFE PLACES:<BR>";
				safe = safe + "<BR>Anchor Rock Port";
				safe = safe + "<BR>Bank";
				safe = safe + "<BR>Black Magic Guild";
				safe = safe + "<BR>Dojo";
				safe = safe + "<BR>Druid Glade";
				safe = safe + "<BR>Forgotten Lighthouse";
				safe = safe + "<BR>Inn";
				safe = safe + "<BR>Kraken Reef Port";
				safe = safe + "<BR>Lankhmar Lighthouse";
				safe = safe + "<BR>Lyceum";
				safe = safe + "<BR>Nightwood Fort";
				safe = safe + "<BR>Port of Shadows";
				safe = safe + "<BR>Ravendark Village";
				safe = safe + "<BR>Savage Sea Port";
				safe = safe + "<BR>Serpent Sail Port";
				safe = safe + "<BR>Stonewall Fort";
				safe = safe + "<BR>Tavern";
				safe = safe + "<BR>Tenebrae Fort";
				safe = safe + "<BR>Thieves Guild";
				if ( umbra ){ safe = safe + "<BR>Umbra Undercity"; }
				safe = safe + "<BR>Wizards Guild";
				safe = safe + "<BR>Xardok's Castle";

				if ( warning != "" ){ warning = warning + safe; }
			}

			if ( warning == "" ){ warning = "You are not wanted for any crimes."; }

			return warning;
		}

		public static bool IsWanted( Mobile m )
		{
			bool wanted = false;

			if ( m is PlayerMobile )
			{
				if ( ((PlayerMobile)m).Fugitive == 1 || m.Kills > 0 )
					wanted = true;
				else if ( m is PlayerMobile && ( m.Karma < 2500 || m.Fame < 2500 ) && Server.Items.BaseRace.IsEvil( m ) )
					wanted = true;
				else if ( m is PlayerMobile && m.Karma <= -5000 && m.Skills[SkillName.Knightship].Base >= 50 )
					wanted = true;
				else if ( m is PlayerMobile && m.Karma <= -5000 && m.Skills[SkillName.Psychology].Base >= 50 && Server.Misc.GetPlayerInfo.isSyth(m, false) )
					wanted = true;
				else if ( m.Criminal )
					wanted = true;
			}
			return wanted;
		}

		public static int LuckyPlayerArtifacts( int luck )
		{
			if ( luck > 2000 )
				luck = 2000;

			int clover = (int)(luck * 0.005); // RETURNS A MAX OF 10

			return clover;
		}

		public static bool OrientalPlay( Mobile m )
		{
			if ( m != null && m is PlayerMobile )
			{
				return ((PlayerMobile)m).Preferences.CharacterOriental;
			}
			else if ( m != null && m is BaseCreature )
			{
				PlayerMobile killer = MobileUtilities.TryGetKillingPlayer( m );

				return killer != null && killer.Preferences.CharacterOriental;
			}

			return false;
		}

		public static int BarbaricPlay( Mobile m )
		{
			if ( m != null && m is PlayerMobile )
			{
				if ( ((PlayerMobile)m).Preferences.CharacterBarbaric > 0 )
					return ((PlayerMobile)m).Preferences.CharacterBarbaric;
			}

			return 0;
		}

		public static bool EvilPlay( Mobile m )
		{
			if ( m != null && m is PlayerMobile )
			{
				if ( ((PlayerMobile)m).Preferences.CharacterEvil )
					return true;
			}
			else if ( m != null && m is BaseCreature )
			{
				Mobile killer = m.LastKiller;
				if (killer is BaseCreature)
				{
					BaseCreature bc_killer = (BaseCreature)killer;
					if(bc_killer.Summoned)
					{
						if(bc_killer.SummonMaster != null)
							killer = bc_killer.SummonMaster;
					}
					else if(bc_killer.Controlled)
					{
						if(bc_killer.ControlMaster != null)
							killer=bc_killer.ControlMaster;
					}
					else if(bc_killer.BardProvoked)
					{
						if(bc_killer.BardMaster != null)
							killer=bc_killer.BardMaster;
					}
				}

				if ( killer != null && killer is PlayerMobile )
				{
					if ( ((PlayerMobile)killer).Preferences.CharacterEvil )
						return true;
				}
				else
				{
					Mobile hitter = m.FindMostRecentDamager(true);
					if (hitter is BaseCreature)
					{
						BaseCreature bc_killer = (BaseCreature)hitter;
						if(bc_killer.Summoned)
						{
							if(bc_killer.SummonMaster != null)
								hitter = bc_killer.SummonMaster;
						}
						else if(bc_killer.Controlled)
						{
							if(bc_killer.ControlMaster != null)
								hitter=bc_killer.ControlMaster;
						}
						else if(bc_killer.BardProvoked)
						{
							if(bc_killer.BardMaster != null)
								hitter=bc_killer.BardMaster;
						}
					}

					if ( hitter != null && hitter is PlayerMobile )
					{
						if ( ((PlayerMobile)hitter).Preferences.CharacterEvil )
							return true;
					}
				}
			}

			return false;
		}

		public static int GetBankedGold( Mobile from )
		{
			int goldCoins = 0;

			Container bank = from.FindBankNoCreate();

			if ( bank != null )
			{
				Item[] gold = bank.FindItemsByType( typeof( Gold ) );

				for ( int i = 0; i < gold.Length; ++i )
					goldCoins += gold[i].Amount;
			}

			return goldCoins;
		}

		public static int GetWealth( Mobile from, int pack )
		{
			int wealth = 0;

			Container bank = from.FindBankNoCreate();
				if ( pack > 0 ){ bank = from.Backpack; }

			if ( bank != null )
			{
				Item[] gold = bank.FindItemsByType( typeof( Gold ) );
				Item[] checks = bank.FindItemsByType( typeof( BankCheck ) );
				Item[] silver = bank.FindItemsByType( typeof( DDSilver ) );
				Item[] copper = bank.FindItemsByType( typeof( DDCopper ) );
				Item[] xormite = bank.FindItemsByType( typeof( DDXormite ) );
				Item[] jewels = bank.FindItemsByType( typeof( DDJewels ) );
				Item[] crystals = bank.FindItemsByType( typeof( Crystals ) );
				Item[] gems = bank.FindItemsByType( typeof( DDGemstones ) );
				Item[] nuggets = bank.FindItemsByType( typeof( DDGoldNuggets ) );

				for ( int i = 0; i < gold.Length; ++i )
					wealth += gold[i].Amount;

				for ( int i = 0; i < checks.Length; ++i )
					wealth += ((BankCheck)checks[i]).Worth;

				for ( int i = 0; i < silver.Length; ++i )
					wealth += (int)Math.Floor((decimal)(silver[i].Amount / 5));

				for ( int i = 0; i < copper.Length; ++i )
					wealth += (int)Math.Floor((decimal)(copper[i].Amount / 10));

				for ( int i = 0; i < xormite.Length; ++i )
					wealth += (xormite[i].Amount)*3;

				for ( int i = 0; i < crystals.Length; ++i )
					wealth += (crystals[i].Amount)*5;

				for ( int i = 0; i < jewels.Length; ++i )
					wealth += (jewels[i].Amount)*2;

				for ( int i = 0; i < gems.Length; ++i )
					wealth += (gems[i].Amount)*2;

				for ( int i = 0; i < nuggets.Length; ++i )
					wealth += (nuggets[i].Amount);
			}

			return wealth;
		}
	}
}

namespace Server.Gumps 
{
    public class StatsGump : Gump
    {
		public int m_Origin;

		public static void Initialize()
		{
            CommandSystem.Register( "status", AccessLevel.Player, new CommandEventHandler( MyStats_OnCommand ) );
		}
		public static void Register( string command, AccessLevel access, CommandEventHandler handler )
		{
            CommandSystem.Register(command, access, handler);
		}

		[Usage( "status" )]
		[Description( "Opens Stats Gump." )]
		public static void MyStats_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			from.CloseGump( typeof( StatsGump ) );
			from.SendGump( new StatsGump( from, from, 0 ) );
        }

        public StatsGump(Mobile m, Mobile from, int origin) : base(50, 50)
        {
            m_Origin = origin;

            if (origin == 0) { from.SendSound(0x4A); }

            int SwingSpeedCap = 100;
            int SDICap = 1000000;
            if (SDICap > MyServerSettings.SpellDamageIncreaseVsMonsters() && MyServerSettings.SpellDamageIncreaseVsMonsters() > 0) { SDICap = MyServerSettings.SpellDamageIncreaseVsMonsters(); }

            int BandageSpeedMilliseconds = BandageContext.HealTimer(m, m);
            TimeSpan SwingSpeed = (from.Weapon as BaseWeapon).GetDelay(from) > TimeSpan.FromSeconds(SwingSpeedCap) ? TimeSpan.FromSeconds(SwingSpeedCap) : (from.Weapon as BaseWeapon).GetDelay(from);

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);

            // 0 - BUTTON // 1 - PLAYERS HANDBOOK // 2 - DIVINATION

            int img = 11420;
            string color = "#E4E377";

            if (m_Origin == 1)
            {
                img = 11417;
                color = "#DCB179";
            }
            else if (m_Origin == 2)
            {
                img = 11419;
                color = "#E59DE2";
            }

            AddImage(1, 1, img, Server.Misc.PlayerSettings.GetGumpHue(m));

            string name = from.Name;
            if (from.Title != "" && from.Title != null) { name = name + " " + from.Title; }
            else { name = name + StatsGumpLocalization.Key(from, "statsgump.name_the", " the ") + GetPlayerInfo.GetSkillTitle(from) + ""; }

            AddHtml(15, 15, 400, 20, @"<BODY><BASEFONT Color=" + color + ">" + name.ToUpper() + "</BASEFONT></BODY>", (bool)false, (bool)false);

            AddButton(667, 12, 4017, 4017, 0, GumpButtonType.Reply, 0);

            AddButton(260, 12, 4011, 4011, 666, GumpButtonType.Reply, 0);
            string warnColor = "#7ab582";
            string warnMsg = StatsGumpLocalization.Key(from, "statsgump.innocent", "Innocent");
            if (Server.Misc.GetPlayerInfo.IsWanted(from))
            {
                warnColor = "#d38a8a";
                warnMsg = StatsGumpLocalization.Key(from, "statsgump.guilty", "Guilty");
            }

            AddHtml(293, 16, 60, 20, @"<BODY><BASEFONT Color=" + warnColor + ">" + warnMsg + "</BASEFONT></BODY>", (bool)false, (bool)false);

            var colAB = new YPosition(45, 35);

            AddStatLine(from, 20, 135, 80, "statsgump.label.level", "Level", string.Format("{0}", GetPlayerInfo.GetPlayerLevel(from)), color, colAB, "statsgump.tooltip.level", "A measure of your skills, stats, and reputation. Max: 100", null);
            AddStatLine(from, 20, 135, 80, "statsgump.label.strength", "Strength", string.Format("{0} + {1}", from.RawStr, from.Str - from.RawStr), color, colAB);
            AddStatLine(from, 20, 135, 80, "statsgump.label.dexterity", "Dexterity", string.Format("{0} + {1}", from.RawDex, from.Dex - from.RawDex), color, colAB);
            AddStatLine(from, 20, 135, 80, "statsgump.label.intelligence", "Intelligence", string.Format("{0} + {1}", from.RawInt, from.Int - from.RawInt), color, colAB);
            AddStatLine(from, 20, 135, 80, "statsgump.label.fame", "Fame", string.Format("{0}", from.Fame), color, colAB, "statsgump.tooltip.max_fame", "Maximum Fame: {0}", new object[] { Titles.MaxFame });
            AddStatLine(from, 20, 135, 80, "statsgump.label.karma", "Karma", string.Format("{0}", from.Karma), color, colAB, "statsgump.tooltip.max_karma", "Maximum Karma: {0}", new object[] { Titles.MaxKarma });
            AddStatLine(from, 20, 135, 80, "statsgump.label.tithe", "Tithe", string.Format("{0}", from.TithingPoints), color, colAB, "statsgump.tooltip.max_tithe", "Maximum Tithe: {0}", new object[] { TithingGump.MaxTithingPoints });
            AddStatLine(from, 20, 135, 80, "statsgump.label.hunger", "Hunger", string.Format("{0}", from.Hunger), color, colAB, "statsgump.tooltip.max_hunger", "Maximum Hunger: 20.", null);
            AddStatLine(from, 20, 135, 80, "statsgump.label.thirst", "Thirst", string.Format("{0}", from.Thirst), color, colAB, "statsgump.tooltip.max_thirst", "Maximum Thirst: 20.", null);
            AddStatLine(from, 20, 135, 80, "statsgump.label.potion_enhance", "Potion Enhance", string.Format("{0}/{1}%", Math.Min(BasePotion.MAX_ENHANCED_POTIONS, AosAttributes.GetValue( m, AosAttribute.EnhancePotions )), BasePotion.MAX_ENHANCED_POTIONS), color, colAB, "statsgump.tooltip.potion_enhance", "Increases effect when consuming potions.", null);
            AddStatLine(from, 20, 135, 80, "statsgump.label.bank_gold", "Bank Gold", Banker.GetBalance(from).ToString(), color, colAB);

            ///////////////////////////////////////////////////////////////////////////////////

            var colCD = new YPosition(45, 35);

            AddStatLine(from, 260, 375, 80, "statsgump.label.hits", "Hits", string.Format("{0} + {1}", from.Hits - AosAttributes.GetValue(from, AosAttribute.BonusHits), AosAttributes.GetValue(from, AosAttribute.BonusHits)), color, colCD);
            AddStatLine(from, 260, 375, 80, "statsgump.label.stamina", "Stamina", string.Format("{0} + {1}", from.Stam - AosAttributes.GetValue(from, AosAttribute.BonusStam), AosAttributes.GetValue(from, AosAttribute.BonusStam)), color, colCD);
            AddStatLine(from, 260, 375, 80, "statsgump.label.mana", "Mana", string.Format("{0} + {1}", from.Mana - AosAttributes.GetValue(from, AosAttribute.BonusMana), AosAttributes.GetValue(from, AosAttribute.BonusMana)), color, colCD);
            AddStatLine(from, 260, 375, 80, "statsgump.label.hits_regen", "Hits Regen", string.Format("{0}", AosAttributes.GetValue(from, AosAttribute.RegenHits)), color, colCD, "statsgump.tooltip.hits_regen", "Regenerates hit points over time", null);
            AddStatLine(from, 260, 375, 80, "statsgump.label.stamina_regen", "Stamina Regen", string.Format("{0}", AosAttributes.GetValue(from, AosAttribute.RegenStam)), color, colCD);
            AddStatLine(from, 260, 375, 80, "statsgump.label.mana_regen", "Mana Regen", string.Format("{0}", AosAttributes.GetValue(from, AosAttribute.RegenMana)), color, colCD);

            if (MyServerSettings.LowerReg() > 0)
                AddStatLine(from, 260, 375, 80, "statsgump.label.low_reagent", "Low Reagent", string.Format("{0}/{1}%", AosAttributes.GetValue(from, AosAttribute.LowerRegCost, false), MyServerSettings.LowerReg()), color, colCD, "statsgump.tooltip.low_reagent", "Increases chance to not use reagents when casting", null);
            if (MyServerSettings.LowerMana() > 0)
                AddStatLine(from, 260, 375, 80, "statsgump.label.low_mana", "Low Mana", string.Format("{0}/{1}%", AosAttributes.GetValue(from, AosAttribute.LowerManaCost, false), MyServerSettings.LowerMana()), color, colCD, "statsgump.tooltip.low_mana", "Reduces mana cost of casting spells and using abilities", null);

            AddStatLine(from, 260, 375, 80, "statsgump.label.spell_damage_plus", "Spell Damage +", string.Format("{0}/{1}%", AosAttributes.GetValue(from, AosAttribute.SpellDamage), SDICap), color, colCD, "statsgump.tooltip.spell_damage", "Increases damage done by spells", null);
            AddStatLine(from, 260, 375, 80, "statsgump.label.resurrect_cost", "Resurrect Cost", string.Format("{0}", GetPlayerInfo.GetResurrectCost(from, true)), color, colCD);
            AddStatLine(from, 260, 375, 80, "statsgump.label.murders", "Murders", string.Format("{0}", from.Kills), color, colCD);

            ///////////////////////////////////////////////////////////////////////////////////
            var colEF = new YPosition(45, 35);

            AddStatLine(from, 500, 615, 80, "statsgump.label.hit_chance", "Hit Chance", string.Format("{0}/{1}%", AosAttributes.GetValue(from, AosAttribute.AttackChance), 45), color, colEF, "statsgump.tooltip.hit_chance", "Increases chance to land weapon attacks", null);
            AddStatLine(from, 500, 615, 80, "statsgump.label.defend_chance", "Defend Chance", string.Format("{0}/{1}%", AosAttributes.GetValue(from, AosAttribute.DefendChance), 45), color, colEF, "statsgump.tooltip.defend_chance", "Increases chance to dodge weapon attacks", null);
            AddStatLine(from, 500, 615, 80, "statsgump.label.swing_speed", "Swing Speed", string.Format("{0}s", new DateTime(SwingSpeed.Ticks).ToString("s.ff")), color, colEF, "statsgump.tooltip.swing_speed", "Duration between weapon attacks", null);
            AddStatLine(from, 500, 615, 80, "statsgump.label.swing_speed_plus", "Swing Speed +", string.Format("{0}/{1}%", AosAttributes.GetValue(from, AosAttribute.WeaponSpeed), 60), color, colEF, "statsgump.tooltip.swing_speed_plus", "Increases attack speed of weapon attacks", null); // Soft cap at 60, Hard cap at 100
            AddStatLine(from, 500, 615, 80, "statsgump.label.bandage_speed", "Bandage Speed", string.Format("{0:0.0}s", new DateTime(TimeSpan.FromMilliseconds(BandageSpeedMilliseconds).Ticks).ToString("s.ff")), color, colEF);
            AddStatLine(from, 500, 615, 80, "statsgump.label.damage_increase", "Damage Increase", string.Format("{0}/{1}%", AosAttributes.GetValue(from, AosAttribute.WeaponDamage), 100), color, colEF, "statsgump.tooltip.damage_increase", "Increases damage of weapon attacks", null);
            AddStatLine(from, 500, 615, 80, "statsgump.label.reflect_damage", "Reflect Damage", string.Format("{0}/{1}%", AosAttributes.GetValue(from, AosAttribute.ReflectPhysical), 100), color, colEF, "statsgump.tooltip.reflect_damage", "Percent of received physical damage that the attacker also receives", null);
            AddStatLine(from, 500, 615, 80, "statsgump.label.fast_cast", "Fast Cast", string.Format("{0}", AosAttributes.GetValue(from, AosAttribute.CastSpeed)), color, colEF, "statsgump.tooltip.fast_cast", "Cast Speed Increase: Pure Knights ({0}) / Otherwise ({1})", new object[] { 4, 2 });
            AddStatLine(from, 500, 615, 80, "statsgump.label.cast_recovery", "Cast Recovery", string.Format("{0}", AosAttributes.GetValue(from, AosAttribute.CastRecovery)), color, colEF);
            AddStatLine(from, 500, 615, 80, "statsgump.label.magic_absorb", "Magic Absorb", string.Format("{0}", from.MagicDamageAbsorb), color, colEF);
            AddStatLine(from, 500, 615, 80, "statsgump.label.melee_absorb", "Melee Absorb", string.Format("{0}", from.MeleeDamageAbsorb), color, colEF);
        }

        private class YPosition
		{
			public int Current { get; set; }
			public int LineHeight { get; private set; }

			public YPosition(int startY, int lineHeight)
			{
				Current = startY;
				LineHeight = lineHeight;
			}

			public int Next()
			{
				int currentY = Current;
				Current += LineHeight;
				return currentY;
			}
		}

		private void AddStatLine( Mobile viewer, int labelX, int valueX, int width, string labelKey, string labelEn, string value, string color, YPosition yPos, string tooltipKey = null, string tooltipEn = null, object[] tooltipArgs = null )
		{
			int currentY = yPos.Next();
			string label = StatsGumpLocalization.Key( viewer, labelKey, labelEn );
			AddHtml( labelX, currentY, 200, 20, @"<BODY><BASEFONT Color=" + color + ">" + label + "</BASEFONT></BODY>", false, false );

			if ( !string.IsNullOrEmpty( tooltipKey ) )
			{
				string tip = ( tooltipArgs != null && tooltipArgs.Length > 0 )
					? StatsGumpLocalization.KeyFormat( viewer, tooltipKey, tooltipEn, tooltipArgs )
					: StatsGumpLocalization.Key( viewer, tooltipKey, tooltipEn );

				AddTooltip( tip );
			}

			AddHtml( valueX, currentY, width, 20, @"<BODY><BASEFONT Color=" + color + "><div align=right>" + value + "</div></BASEFONT></BODY>", false, false );
		}
    
		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			if ( info.ButtonID == 666 )
			{
				from.SendGump( new StatsGump( from, from, m_Origin ) );
				from.SendGump( new Wanted( from ) );
			}
			else if ( m_Origin == 1 ){ from.SendSound( 0x55 ); }
			else if ( m_Origin == 2 ){ from.SendSound( 0x0F9 ); }
			else { from.SendSound( 0x4A ); }
		}
    }

}

namespace Server.Engines.Quests
{
	public class QuestButton
	{
		public static void Initialize()
		{
			EventSink.QuestGumpRequest += new QuestGumpRequestHandler( EventSink_QuestGumpRequest );
		}

		public static void EventSink_QuestGumpRequest( QuestGumpRequestArgs e )
		{
			Mobile from = e.Mobile;

			if ( from is PlayerMobile pm )
				pm.ViewQuestLog();
		}
	}
}