using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Menus;
using Server.Menus.Questions;
using Server.Accounting;
using Server.Multis;
using Server.Mobiles;
using Server.Regions;
using System.Collections;
using System.Collections.Generic;
using Server.Commands;
using Server.Misc;
using Server.Items;
using System.Globalization;
using Server.Engines.MLQuests.Gumps;

namespace Server.Engines.Help
{
	public class ContainedMenu : QuestionMenu
	{
		private Mobile m_From;

		public ContainedMenu( Mobile from ) : base( "You already have an open help request. We will have someone assist you as soon as possible.  What would you like to do?", new string[]{ "Leave my old help request like it is.", "Remove my help request from the queue." } )
		{
			m_From = from;
		}

		public override void OnCancel( NetState state )
		{
			m_From.SendLocalizedMessage( 1005306, "", 0x35 ); // Help request unchanged.
		}

		public override void OnResponse( NetState state, int index )
		{
			m_From.SendSound( 0x4A );
			if ( index == 0 )
			{
				m_From.SendLocalizedMessage( 1005306, "", 0x35 ); // Help request unchanged.
			}
			else if ( index == 1 )
			{
				PageEntry entry = PageQueue.GetEntry( m_From );

				if ( entry != null && entry.Handler == null )
				{
					m_From.SendLocalizedMessage( 1005307, "", 0x35 ); // Removed help request.
					entry.AddResponse( entry.Sender, "[Canceled]" );
					PageQueue.Remove( entry );
				}
				else
				{
					m_From.SendLocalizedMessage( 1005306, "", 0x35 ); // Help request unchanged.
				}
			}
		}
	}

	public class HelpGump : Gump
	{
		private enum PageActionType
		{
			Close = 0,
			Navigate_Main = 1,
			Do_Toggle_AFK = 2,
			Show_Chat = 3,
			Do_CorpseClear = 4,
			Do_CorpseSearch = 5,
			Show_Emote = 6,
			Navigate_MagicToolbars = 7,
			Do_MoongateSearch = 8,
			Show_MOTD = 9,
			Show_Quests = 10,
			Show_QuickBar = 11,

			Show_Settings = 12,
			Navigate_Library = 13,
			Show_Statistics = 14,
			Do_StuckInWorld = 15,
			Show_WeaponAbilities = 16,
			Show_WealthBar = 17,
			Show_Conversations = 18,
			Navigate_Changelog = 19,
			Setting_OrdinaryResources = 49,
			Setting_WeaponAbilityNames = 51,
			Setting_AutoSheath = 52,
			Setting_MusicTone = 53,
			Setting_PrivatePlay = 54,
			Setting_LootOptions = 55,
			Setting_SkillTitle = 56,
			Setting_Playstyle_Normal = 57,
			Setting_Playstyle_Evil = 58,
			Setting_Playstyle_Oriental = 59,
			Setting_MessageColors = 60,
			Setting_AutoAttack = 61,
			Show_RegBar = 62,
			Setting_UseAncientSpellbook = 63,
			Setting_ClassicPoisoning = 64,
			Setting_MusicPlaylist = 65,
			MagicToolbar_BardSongsBarI_Config = 66,
			MagicToolbar_BardSongsBarII_Config = 67,
			MagicToolbar_KnightSpellBarI_Config = 68,
			MagicToolbar_KnightSpellBarII_Config = 69,
			MagicToolbar_DeathKnightSpellBarI_Config = 70,
			MagicToolbar_DeathKnightSpellBarII_Config = 71,
			MagicToolbar_MagerySpellBarI_Config = 72,
			MagicToolbar_MagerySpellBarII_Config = 73,
			MagicToolbar_MagerySpellBarIII_Config = 74,
			MagicToolbar_MagerySpellBarIV_Config = 75,
			MagicToolbar_NecromancerSpellBarI_Config = 76,
			MagicToolbar_NecromancerSpellBarII_Config = 77,
			MagicToolbar_PriestSpellBarI_Config = 78,
			MagicToolbar_PriestSpellBarII_Config = 79,

			Setting_CustomTitle = 80,

			MARKER_SETTING_START = 81,
			Setting_MusicTone_Info = 82,
			Setting_MusicPlaylist_Info = 83,
			Setting_PrivatePlay_Info = 84,
			Setting_LootOptions_Info = 85,
			Setting_ClassicPoisoning_Info = 86,
			Setting_SkillTitle_Info = 87,
			Setting_MessageColors_Info = 88,
			Setting_AttoAttack_Info = 89,
			Setting_Playstyle_Normal_Info = 92,
			Setting_Playstyle_Evil_Info = 93,
			Setting_Playstyle_Oriental_Info = 94,
			ShowHelp_MagicToolbars = 95,
			Setting_MagerySpellColor_Info = 96,
			Setting_CustomTitle_Info = 97,
			Setting_WeaponAbilityNames_Info = 99,
			Setting_AutoSheath_Info = 100,
			Setting_GumpImages_Info = 101,
			Setting_WeaponAbilityBar_Info = 102,
			Setting_CreatureMagicFocus_Info = 103,
			Setting_CreatureType_Info = 104,
			Setting_CreatureSounds_Info = 105,
			Setting_UseAncientSpellbook_Info = 106,
			Setting_SetCraftingContainer_Info = 107,
			Setting_SetHarvestingContainer_Info = 108,
			Setting_SetLootContainer_Info = 109,
			Setting_OrdinaryResources_Info = 110,
			Setting_DoubleClickToIDItems_Info = 111,
			Setting_Playstyle_Barbaric_Info = 198,
			Setting_SkillList_Info = 199,
			MARKER_SETTING_END = 200,

			MARKER_TOOLBAR_START = 200, // Yes, duplicate
			MagicToolbar_Bard_I_Open = 266,
			MagicToolbar_Bard_II_Open = 267,
			MagicToolbar_Knight_I_Open = 268,
			MagicToolbar_Knight_II_Open = 269,
			MagicToolbar_DeathKnight_I_Open = 270,
			MagicToolbar_DeathKnight_II_Open = 271,
			MagicToolbar_Magery_I_Open = 272,
			MagicToolbar_Magery_II_Open = 273,
			MagicToolbar_Magery_III_Open = 274,
			MagicToolbar_Magery_IV_Open = 275,
			MagicToolbar_Necromancer_I_Open = 276,
			MagicToolbar_Necromancer_II_Open = 277,
			MagicToolbar_Priest_I_Open = 278,
			MagicToolbar_Priest_II_Open = 279,
			MagicToolbar_Monk_I_Open = 280,
			MagicToolbar_Monk_II_Open = 281,
			MagicToolbar_Elemental_I_Open = 282,
			MagicToolbar_Elemental_II_Open = 283,
			MagicToolbar_Bard_I_Close = 366,
			MagicToolbar_Bard_II_Close = 367,
			MagicToolbar_Knight_I_Close = 368,
			MagicToolbar_Knight_II_Close = 369,
			MagicToolbar_DeathKnight_I_Close = 370,
			MagicToolbar_DeathKnight_II_Close = 371,
			MagicToolbar_Magery_I_Close = 372,
			MagicToolbar_Magery_II_Close = 373,
			MagicToolbar_Magery_III_Close = 374,
			MagicToolbar_Magery_IV_Close = 375,
			MagicToolbar_Necromancer_I_Close = 376,
			MagicToolbar_Necromancer_II_Close = 377,
			MagicToolbar_Priest_I_Close = 378,
			MagicToolbar_Priest_II_Close = 379,
			MagicToolbar_Monk_I_Close = 380,
			MagicToolbar_Monk_II_Close = 381,
			MagicToolbar_Elemental_I_Close = 382,
			MagicToolbar_Elemental_II_Close = 383,
			MagicToolbar_Ancient_I_Open = 384,
			MagicToolbar_Ancient_I_Close = 385,
			MagicToolbar_Ancient_II_Open = 386,
			MagicToolbar_Ancient_II_Close = 387,
			MagicToolbar_Ancient_III_Open = 388,
			MagicToolbar_Ancient_III_Close = 389,
			MagicToolbar_Ancient_IV_Open = 390,
			MagicToolbar_Ancient_IV_Close = 391,
			MARKER_TOOLBAR_END = 400,

			Setting_MagerySpellColor_White = 500,
			Setting_MagerySpellColor_Black = 501,
			Setting_MagerySpellColor_Blue = 502,
			Setting_MagerySpellColor_Red = 503,
			Setting_MagerySpellColor_Green = 504,
			Setting_MagerySpellColor_Purple = 505,
			Setting_MagerySpellColor_Yellow = 506,
			Setting_MagerySpellColor_Default = 507,
			MagicToolbar_ElementalSpellBarI_Config = 978,
			MagicToolbar_ElementalSpellBarII_Config = 979,
			MagicToolbar_MonkSpellBarI_Config = 980,
			MagicToolbar_MonkSpellBarII_Config = 981,
			Setting_SkillList = 982,
			Show_SkillList = 983,
			Setting_Playstyle_Barbaric = 984,
			Setting_GumpImages = 985,
			Setting_WeaponAbilityBar = 986,
			Setting_CreatureMagicFocus = 989,
			Setting_CreatureType = 990,
			Setting_CreatureSounds = 991,
			MagicToolbar_AncientSpellBarI_Config = 1081,
			MagicToolbar_AncientSpellBarII_Config = 1082,
			MagicToolbar_AncientSpellBarIII_Config = 1083,
			MagicToolbar_AncientSpellBarIV_Config = 1084,
			Setting_SetLootContainer = 5000,
			Setting_SetCraftingContainer = 5001,
			Setting_SetHarvestingContainer = 5002,
			Setting_DoubleClickToIDItems = 5003,
		}

		public static void Initialize()
		{
			EventSink.HelpRequest += new HelpRequestEventHandler( EventSink_HelpRequest );
            CommandSystem.Register("toolbars", AccessLevel.Player, e => OpenHelpGumpPageCommand(e, 7));
		}

        private static void OpenHelpGumpPageCommand(CommandEventArgs e, int pageNumber)
        {
			var player = e.Mobile as PlayerMobile;
			if (player == null) return;

			player.CloseGump(typeof(HelpGump));
			player.SendGump(new HelpGump(player, pageNumber));
        }

        private static void EventSink_HelpRequest( HelpRequestEventArgs e )
		{
			foreach ( Gump g in e.Mobile.NetState.Gumps )
			{
				if ( g is HelpGump )
					return;
			}

			if ( !PageQueue.CheckAllowedToPage( e.Mobile ) )
				return;

			if ( PageQueue.Contains( e.Mobile ) )
				e.Mobile.SendMenu( new ContainedMenu( e.Mobile ) );
			else
				e.Mobile.SendGump( new HelpGump( e.Mobile, 1 ) );
		}

		private static bool IsYoung( Mobile m )
		{
			if ( m is PlayerMobile )
				return ((PlayerMobile)m).Young;

			return false;
		}

		public static bool CheckCombat( Mobile m )
		{
			for ( int i = 0; i < m.Aggressed.Count; ++i )
			{
				AggressorInfo info = m.Aggressed[i];

				if ( DateTime.Now - info.LastCombatTime < TimeSpan.FromSeconds( 30.0 ) )
					return true;
			}

			return false;
		}

		public HelpGump( Mobile from, int page ) : base( 50, 50 )
		{
			string HelpText = MyHelp();
			string color = "#ddbc4b";
			int button = 4005;

			from.SendSound( 0x4A ); 

            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);

			int r = 40;
			int e = 30;

			AddImage(0, 0, 9548, Server.Misc.PlayerSettings.GetGumpHue( from ));
			AddHtml( 12, 12, 300, 20, @"<BODY><BASEFONT Color=" + color + ">HELP OPTIONS</BASEFONT></BODY>", (bool)false, (bool)false);
			AddButton(967, 10, 4017, 4017, (int)PageActionType.Close, GumpButtonType.Reply, 0);

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 1 ){ button = 4006; AddHtml( 252, 71, 739, 630, @"<BODY><BASEFONT Color=" + color + ">" + HelpText + "</BASEFONT></BODY>", (bool)false, (bool)true); }
			AddButton(15, r, button, button, (int)PageActionType.Navigate_Main, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Main</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 3609;
			HelpText = "Your 'Away From Keyboard' Settings Are Disabled.";
			if ( Server.Commands.AFK.m_AFK.Contains( from.Serial.Value ) )
			{
				button = 4018;
				HelpText = "Your 'Away From Keyboard' Settings Are Enabled.";
			}
			if ( page == 2 ){ AddHtml( 252, 71, 739, 630, @"<BODY><BASEFONT Color=" + color + ">" + HelpText + "</BASEFONT></BODY>", (bool)false, (bool)true); }
			AddButton(15, r, button, button, (int)PageActionType.Do_Toggle_AFK, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">AFK</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 3 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_Chat, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Chat</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 18 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_Conversations, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Conversations</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 4 ){ button = 4006; AddHtml( 252, 71, 739, 630, @"<BODY><BASEFONT Color=" + color + ">Your empty corpses have been removed.</BASEFONT></BODY>", (bool)false, (bool)true); }
			AddButton(15, r, button, button, (int)PageActionType.Do_CorpseClear, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Corpse Clear</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 5 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Do_CorpseSearch, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Corpse Search</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 6 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_Emote, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Emote</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 13 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Navigate_Library, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Library</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 7 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Navigate_MagicToolbars, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Magic Toolbars</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;
			if ( page == 7 )
			{
				int barS = 40;
				int barM = 30;

				AddButton(904, 10, 3610, 3610, (int)PageActionType.ShowHelp_MagicToolbars, GumpButtonType.Reply, 0);

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_AncientSpellBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Ancient Spell Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Ancient_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Ancient_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_AncientSpellBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Ancient Spell Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Ancient_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Ancient_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_AncientSpellBarIII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Ancient Spell Bar III</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Ancient_III_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Ancient_III_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_AncientSpellBarIV_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Ancient Spell Bar IV</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Ancient_IV_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Ancient_IV_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_BardSongsBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Bard Songs Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Bard_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Bard_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_BardSongsBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Bard Songs Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Bard_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Bard_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_KnightSpellBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Knight Spell Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Knight_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Knight_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_KnightSpellBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Knight Spell Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Knight_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Knight_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_DeathKnightSpellBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Death Knight Spell Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_DeathKnight_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_DeathKnight_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_DeathKnightSpellBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Death Knight Spell Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_DeathKnight_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_DeathKnight_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_ElementalSpellBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Elemental Spell Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Elemental_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Elemental_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_ElementalSpellBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Elemental Spell Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Elemental_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Elemental_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_MagerySpellBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Magery Spell Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Magery_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Magery_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_MagerySpellBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Magery Spell Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Magery_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Magery_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_MagerySpellBarIII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Magery Spell Bar III</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Magery_III_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Magery_III_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_MagerySpellBarIV_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Magery Spell Bar IV</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Magery_IV_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Magery_IV_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_MonkSpellBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Monk Ability Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Monk_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Monk_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_MonkSpellBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Monk Ability Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Monk_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Monk_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_NecromancerSpellBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Necromancer Spell Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Necromancer_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Necromancer_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_NecromancerSpellBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Necromancer Spell Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Necromancer_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Necromancer_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_PriestSpellBarI_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Priest Prayer Bar I</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Priest_I_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Priest_I_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;

				AddButton(245, barS, 4005, 4005, (int)PageActionType.MagicToolbar_PriestSpellBarII_Config, GumpButtonType.Reply, 0);
				AddHtml( 280, barS, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Priest Prayer Bar II</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(605, barS, 4005, 4005, (int)PageActionType.MagicToolbar_Priest_II_Open, GumpButtonType.Reply, 0);
				AddHtml( 640, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Open Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(800, barS, 4020, 4020, (int)PageActionType.MagicToolbar_Priest_II_Close, GumpButtonType.Reply, 0);
				AddHtml( 835, barS, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Close Toolbar</BASEFONT></BODY>", (bool)false, (bool)false);
				barS = barS + barM;
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 8 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Do_MoongateSearch, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Moongate Search</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


			button = 4005; if ( page == 9 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_MOTD, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">MOTD</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 10 )
			{
				button = 4006;
				AddHtml( 252, 71, 739, 630, @"<BODY><BASEFONT Color=" + color + ">Throughout your journey, you may come across particular events that appear in your quest log. They may be a simple achievement of finding a strange land, or they may reference an item you must find. Quests are handled in a 'virtual' manner. What this means is that any achievements are real, but any references to items found are not. If your quest log states that you found an ebony key, you will not have an ebony key in your backpack...but you will 'virtually' have the item. The quest will keep track of this fact for you. Because of this, you will never lose that ebony key and it remains unique to your character's questing. The quest knows you found it and have it. You may be tasked to find an item in a dungeon. When there is an indication you found it, it will be 'virtually' in your possession. You will often hear a sound of victory when a quest event is reached, along with a message about it. You still may miss it, however. So check your quest log from time to time. One way to get quests is to visit taverns or inns. If you see a bulletin board called 'Seeking Brave Adventurers', single click on it to begin your life questing for fame and fortune.<BR><BR>" + MyQuests( from ) + "<BR><BR></BASEFONT></BODY>", (bool)false, (bool)true);
			}
			AddButton(15, r, button, button, (int)PageActionType.Show_Quests, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Quests</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 11 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_QuickBar, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Quick Bar</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			button = 4005; if ( page == 62 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_RegBar, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Reagent Bar</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


			button = 4005; if ( page == 12 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_Settings, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Settings</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;
			if ( page == 12 )
			{
				int g = 70;
				int j = 30;
				int setB = 3609;
				int xm = 245;
				int xo = 700;
				int xr = 0;
				int xs = 245;

				if ( !from.NoAutoAttack ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_AutoAttack, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_AttoAttack_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Auto Attack</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( ((PlayerMobile)from).CharacterSheath == 1 ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_AutoSheath, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_AutoSheath_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Auto Sheath</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( ((PlayerMobile)from).ClassicPoisoning == 1 ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_ClassicPoisoning, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_ClassicPoisoning_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Classic Poisoning</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( from.RaceID > 0 && (from.Region).Name == "the Tavern" && Server.Items.BaseRace.GetMonsterMage( from.RaceID ) )
				{
					string magic = "Default";
					if ( from.RaceMagicSchool == 1 ){ magic = "Magery"; }
					else if ( from.RaceMagicSchool == 2 ){ magic = "Necromancy"; }
					else if ( from.RaceMagicSchool == 3 ){ magic = "Elementalism"; }
					AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_CreatureMagicFocus, GumpButtonType.Reply, 0);
					AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_CreatureMagicFocus_Info, GumpButtonType.Reply, 0);
					AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Creature Magic (" + magic + ")</BASEFONT></BODY>", (bool)false, (bool)false);
					if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }
				}

				if ( from.RaceID > 0 )
				{
					if ( from.RaceMakeSounds ){ setB = 4018; } else { setB = 3609; }
					AddButton(xs, g, setB, setB, (int)PageActionType.Setting_CreatureSounds, GumpButtonType.Reply, 0);
					AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_CreatureSounds_Info, GumpButtonType.Reply, 0);
					AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Creature Sounds</BASEFONT></BODY>", (bool)false, (bool)false);

					if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }
				}

				if ( from.RaceID > 0 
					&& (
					(from.Region).Name == "the Tavern" ||
					( from.Map == Map.Sosaria && from.X >= 6982 && from.Y >= 694 && from.X <= 6999 && from.Y <= 713 )
				))
				{
					AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_CreatureType, GumpButtonType.Reply, 0);
					AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_CreatureType_Info, GumpButtonType.Reply, 0);
					AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Creature Type</BASEFONT></BODY>", (bool)false, (bool)false);

					if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }
				}

				if ( MySettings.S_AllowCustomTitles )
				{
					AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_CustomTitle, GumpButtonType.Reply, 0);
					AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_CustomTitle_Info, GumpButtonType.Reply, 0);
					AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Custom Title</BASEFONT></BODY>", (bool)false, (bool)false);

					if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }
				}

				if ( ((PlayerMobile)from).GumpHue > 0 ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_GumpImages, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_GumpImages_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Gump Images</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_LootOptions, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_LootOptions_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Loot Options</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( from.RainbowMsg ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_MessageColors, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_MessageColors_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Message Colors</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_MusicPlaylist, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_MusicPlaylist_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Music Playlist</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( ((PlayerMobile)from).CharMusical == "Forest" ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_MusicTone, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_MusicTone_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Music Tone</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( ((PlayerMobile)from).PublicInfo == false ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_PrivatePlay, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_PrivatePlay_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Private Play</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_SkillTitle, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_SkillTitle_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Skill Title</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				string skillLocks = "Skill List (Show Up)"; 
				if ( ((PlayerMobile)from).SkillDisplay == 1 ){ setB = 4018; skillLocks = "Skill List (Show Up and Locked)"; } else { setB = 4017; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_SkillList, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_SkillList_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">" + skillLocks + "</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( ((PlayerMobile)from).WeaponBarOpen > 0 ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_WeaponAbilityBar, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_WeaponAbilityBar_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Weapon Ability Bar</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_SetCraftingContainer, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_SetCraftingContainer_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Set Crafting Container</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( ((PlayerMobile)from).CharacterWepAbNames == 1 ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_WeaponAbilityNames, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_WeaponAbilityNames_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Weapon Ability Names</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_SetHarvestingContainer, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_SetHarvestingContainer_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Set Harvesting Container</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( ResearchSettings.BookCaster( from ) ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_UseAncientSpellbook, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_UseAncientSpellbook_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Use Ancient Spellbook</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				AddButton(xs, g, 4005, 4005, (int)PageActionType.Setting_SetLootContainer, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_SetLootContainer_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Set Loot Container</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( ((PlayerMobile)from).DoubleClickID ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_DoubleClickToIDItems, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_DoubleClickToIDItems_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Double Click to ID Items</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( xr == 1 ){ g=g+j; xr=0; xs=xm; } else { xr=1; xs=xo; }

				if ( from.HarvestOrdinary ){ setB = 4018; } else { setB = 3609; }
				AddButton(xs, g, setB, setB, (int)PageActionType.Setting_OrdinaryResources, GumpButtonType.Reply, 0);
				AddButton(xs+40, g, 4011, 4011, (int)PageActionType.Setting_OrdinaryResources_Info, GumpButtonType.Reply, 0);
				AddHtml( xs+80, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Ordinary Resources</BASEFONT></BODY>", (bool)false, (bool)false);

				g=g+j;
				g=g+j;

				AddHtml( 325, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Play Styles</BASEFONT></BODY>", (bool)false, (bool)false);
				g=g+j;

				if ( ((PlayerMobile)from).CharacterEvil == 0 && ((PlayerMobile)from).CharacterOriental == 0 && ((PlayerMobile)from).CharacterBarbaric == 0 ){ setB = 4018; } else { setB = 3609; }
				AddButton(325, g, setB, setB, (int)PageActionType.Setting_Playstyle_Normal, GumpButtonType.Reply, 0);
				AddButton(370, g, 4011, 4011, (int)PageActionType.Setting_Playstyle_Normal_Info, GumpButtonType.Reply, 0);
				AddHtml( 410, g, 65, 20, @"<BODY><BASEFONT Color=" + color + ">Normal</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( ((PlayerMobile)from).CharacterEvil == 1 ){ setB = 4018; } else { setB = 3609; }
				AddButton(535, g, setB, setB, (int)PageActionType.Setting_Playstyle_Evil, GumpButtonType.Reply, 0);
				AddButton(575, g, 4011, 4011, (int)PageActionType.Setting_Playstyle_Evil_Info, GumpButtonType.Reply, 0);
				AddHtml( 620, g, 65, 20, @"<BODY><BASEFONT Color=" + color + ">Evil</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( ((PlayerMobile)from).CharacterOriental == 1 ){ setB = 4018; } else { setB = 3609; }
				AddButton(745, g, setB, setB, (int)PageActionType.Setting_Playstyle_Oriental, GumpButtonType.Reply, 0);
				AddButton(785, g, 4011, 4011, (int)PageActionType.Setting_Playstyle_Oriental_Info, GumpButtonType.Reply, 0);
				AddHtml( 830, g, 65, 20, @"<BODY><BASEFONT Color=" + color + ">Oriental</BASEFONT></BODY>", (bool)false, (bool)false);

				g=g+j;

				string amazon = "";
				if ( ((PlayerMobile)from).CharacterBarbaric == 1 ){ setB = 4018; } 
				else if ( ((PlayerMobile)from).CharacterBarbaric == 2 ){ setB = 4003; amazon = " with Amazon Fighting Titles"; } 
				else { setB = 3609; }
				AddButton(325, g, setB, setB, (int)PageActionType.Setting_Playstyle_Barbaric, GumpButtonType.Reply, 0);
				AddButton(370, g, 4011, 4011, (int)PageActionType.Setting_Playstyle_Barbaric_Info, GumpButtonType.Reply, 0);
				AddHtml( 410, g, 300, 20, @"<BODY><BASEFONT Color=" + color + ">Barbaric" + amazon + "</BASEFONT></BODY>", (bool)false, (bool)false);

				g=g+j;
				g=g+j;

				AddButton(285, g, 4011, 4011, (int)PageActionType.Setting_MagerySpellColor_Info, GumpButtonType.Reply, 0);
				AddHtml( 325, g, 316, 20, @"<BODY><BASEFONT Color=" + color + ">Magery Spell Color</BASEFONT></BODY>", (bool)false, (bool)false);

				if ( ((PlayerMobile)from).MagerySpellHue == 0x47E ){ setB = 4018; } else { setB = 3609; }
				AddHtml( 565, g, 61, 20, @"<BODY><BASEFONT Color=" + color + ">White</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(523, g, setB, setB, (int)PageActionType.Setting_MagerySpellColor_White, GumpButtonType.Reply, 0);

				if ( ((PlayerMobile)from).MagerySpellHue == 0x94E ){ setB = 4018; } else { setB = 3609; }
				AddHtml( 685, g, 61, 20, @"<BODY><BASEFONT Color=" + color + ">Black</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(643, g, setB, setB, (int)PageActionType.Setting_MagerySpellColor_Black, GumpButtonType.Reply, 0);

				if ( ((PlayerMobile)from).MagerySpellHue == 0x48D ){ setB = 4018; } else { setB = 3609; }
				AddHtml( 805, g, 61, 20, @"<BODY><BASEFONT Color=" + color + ">Blue</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(764, g, setB, setB, (int)PageActionType.Setting_MagerySpellColor_Blue, GumpButtonType.Reply, 0);

				if ( ((PlayerMobile)from).MagerySpellHue == 0x48E ){ setB = 4018; } else { setB = 3609; }
				AddHtml( 925, g, 61, 20, @"<BODY><BASEFONT Color=" + color + ">Red</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(883, g, setB, setB, (int)PageActionType.Setting_MagerySpellColor_Red, GumpButtonType.Reply, 0);

				g=g+j;

				if ( ((PlayerMobile)from).MagerySpellHue == 0x48F ){ setB = 4018; } else { setB = 3609; }
				AddHtml( 565, g, 61, 20, @"<BODY><BASEFONT Color=" + color + ">Green</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(523, g, setB, setB, (int)PageActionType.Setting_MagerySpellColor_Green, GumpButtonType.Reply, 0);

				if ( ((PlayerMobile)from).MagerySpellHue == 0x490 ){ setB = 4018; } else { setB = 3609; }
				AddHtml( 685, g, 61, 20, @"<BODY><BASEFONT Color=" + color + ">Purple</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(643, g, setB, setB, (int)PageActionType.Setting_MagerySpellColor_Purple, GumpButtonType.Reply, 0);

				if ( ((PlayerMobile)from).MagerySpellHue == 0x491 ){ setB = 4018; } else { setB = 3609; }
				AddHtml( 805, g, 61, 20, @"<BODY><BASEFONT Color=" + color + ">Yellow</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(764, g, setB, setB, (int)PageActionType.Setting_MagerySpellColor_Yellow, GumpButtonType.Reply, 0);

				if ( ((PlayerMobile)from).MagerySpellHue == 0 ){ setB = 4018; } else { setB = 3609; }
				AddHtml( 925, g, 61, 20, @"<BODY><BASEFONT Color=" + color + ">Default</BASEFONT></BODY>", (bool)false, (bool)false);
				AddButton(883, g, setB, setB, (int)PageActionType.Setting_MagerySpellColor_Default, GumpButtonType.Reply, 0);

				g=g+j;
				g=g+j;
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 983 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_SkillList, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Skill List</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 14 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_Statistics, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Statistics</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			bool house = false;
			if ( from.Region is HouseRegion )
			    if (((HouseRegion)from.Region).House.IsOwner(from))
					house = true;
			if ( from.Region.GetLogoutDelay( from ) != TimeSpan.Zero && house == false && !( from.Region is SkyHomeDwelling ) && !( from.Region is PrisonArea ) && !( from.Region is DungeonHomeRegion ) && !( from.Region is GargoyleRegion ) && !( from.Region is SafeRegion ) )
			{
				button = 4005; if ( page == 15 ){ button = 4006; }
				AddButton(15, r, button, button, (int)PageActionType.Do_StuckInWorld, GumpButtonType.Reply, 0);
				AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Stuck in World</BASEFONT></BODY>", (bool)false, (bool)false);
				r=r+e;
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			HelpText = Server.Misc.ChangeLog.Versions();
			button = 4005; if ( page == 19 ){ button = 4006; AddHtml( 252, 71, 739, 630, @"<BODY><BASEFONT Color=" + color + ">" + HelpText + "</BASEFONT></BODY>", (bool)false, (bool)true); }
			AddButton(15, r, button, button, (int)PageActionType.Navigate_Changelog, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Version</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 17 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_WealthBar, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Wealth Bar</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			button = 4005; if ( page == 16 ){ button = 4006; }
			AddButton(15, r, button, button, (int)PageActionType.Show_WeaponAbilities, GumpButtonType.Reply, 0);
			AddHtml( 50, r, 148, 20, @"<BODY><BASEFONT Color=" + color + ">Weapon Abilities</BASEFONT></BODY>", (bool)false, (bool)false);
			r=r+e;
		}

        public void InvokeCommand( string c, Mobile from )
        {
            CommandSystem.Handle(from, String.Format("{0}{1}", CommandSystem.Prefix, c));
        }

		private void TryConfigureSpellBar(SetupSpellBarGump gump)
		{
			if (gump.ConfigureGump())
			{
				gump.Player.CloseGump(gump.GetType());
				gump.Player.SendGump(gump);
			}
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			int pressed = info.ButtonID;
			PageActionType actionType = (PageActionType)info.ButtonID;

			Mobile from = state.Mobile;

			from.SendSound( 0x4A ); 

			from.CloseGump( typeof(Server.Engines.Help.HelpGump) );

			if ( pressed > (int)PageActionType.MARKER_SETTING_START && pressed < (int)PageActionType.MARKER_SETTING_END )
			{
				// SMALL INFO HELP WINDOWS
				from.CloseGump( typeof( InfoHelpGump ) );
				from.SendGump( new InfoHelpGump( from, info.ButtonID, (int)PageActionType.Show_Settings ) );
			}
			else if ( info.ButtonID >= (int)PageActionType.MARKER_TOOLBAR_START && info.ButtonID <= (int)PageActionType.MARKER_TOOLBAR_END ) // MAGIC BARS OPEN AND CLOSE
			{
				from.SendGump( new Server.Engines.Help.HelpGump( from, 7 ) );

				switch (actionType)
				{
					case PageActionType.MagicToolbar_Bard_I_Open: InvokeCommand( "bardtool1", from ); break;
					case PageActionType.MagicToolbar_Bard_I_Close: InvokeCommand( "bardclose1", from ); break;
					case PageActionType.MagicToolbar_Bard_II_Open: InvokeCommand( "bardtool2", from ); break;
					case PageActionType.MagicToolbar_Bard_II_Close: InvokeCommand( "bardclose2", from ); break;
					case PageActionType.MagicToolbar_Knight_I_Open: InvokeCommand( "knighttool1", from ); break;
					case PageActionType.MagicToolbar_Knight_I_Close: InvokeCommand( "knightclose1", from ); break;
					case PageActionType.MagicToolbar_Knight_II_Open: InvokeCommand( "knighttool2", from ); break;
					case PageActionType.MagicToolbar_Knight_II_Close: InvokeCommand( "knightclose2", from ); break;
					case PageActionType.MagicToolbar_DeathKnight_I_Open: InvokeCommand( "deathtool1", from ); break;
					case PageActionType.MagicToolbar_DeathKnight_I_Close: InvokeCommand( "deathclose1", from ); break;
					case PageActionType.MagicToolbar_DeathKnight_II_Open: InvokeCommand( "deathtool2", from ); break;
					case PageActionType.MagicToolbar_DeathKnight_II_Close: InvokeCommand( "deathclose2", from ); break;
					case PageActionType.MagicToolbar_Magery_I_Open: InvokeCommand( "magetool1", from ); break;
					case PageActionType.MagicToolbar_Magery_I_Close: InvokeCommand( "mageclose1", from ); break;
					case PageActionType.MagicToolbar_Magery_II_Open: InvokeCommand( "magetool2", from ); break;
					case PageActionType.MagicToolbar_Magery_II_Close: InvokeCommand( "mageclose2", from ); break;
					case PageActionType.MagicToolbar_Magery_III_Open: InvokeCommand( "magetool3", from ); break;
					case PageActionType.MagicToolbar_Magery_III_Close: InvokeCommand( "mageclose3", from ); break;
					case PageActionType.MagicToolbar_Magery_IV_Open: InvokeCommand( "magetool4", from ); break;
					case PageActionType.MagicToolbar_Magery_IV_Close: InvokeCommand( "mageclose4", from ); break;
					case PageActionType.MagicToolbar_Necromancer_I_Open: InvokeCommand( "necrotool1", from ); break;
					case PageActionType.MagicToolbar_Necromancer_I_Close: InvokeCommand( "necroclose1", from ); break;
					case PageActionType.MagicToolbar_Necromancer_II_Open: InvokeCommand( "necrotool2", from ); break;
					case PageActionType.MagicToolbar_Necromancer_II_Close: InvokeCommand( "necroclose2", from ); break;
					case PageActionType.MagicToolbar_Priest_I_Open: InvokeCommand( "holytool1", from ); break;
					case PageActionType.MagicToolbar_Priest_I_Close: InvokeCommand( "holyclose1", from ); break;
					case PageActionType.MagicToolbar_Priest_II_Open: InvokeCommand( "holytool2", from ); break;
					case PageActionType.MagicToolbar_Priest_II_Close: InvokeCommand( "holyclose2", from ); break;
					case PageActionType.MagicToolbar_Monk_I_Open: InvokeCommand( "monktool1", from ); break;
					case PageActionType.MagicToolbar_Monk_I_Close: InvokeCommand( "monkclose1", from ); break;
					case PageActionType.MagicToolbar_Monk_II_Open: InvokeCommand( "monktool2", from ); break;
					case PageActionType.MagicToolbar_Monk_II_Close: InvokeCommand( "monkclose2", from ); break;
					case PageActionType.MagicToolbar_Elemental_I_Open: InvokeCommand( "elementtool1", from ); break;
					case PageActionType.MagicToolbar_Elemental_I_Close: InvokeCommand( "elementclose1", from ); break;
					case PageActionType.MagicToolbar_Elemental_II_Open: InvokeCommand( "elementtool2", from ); break;
					case PageActionType.MagicToolbar_Elemental_II_Close: InvokeCommand( "elementclose2", from ); break;
					case PageActionType.MagicToolbar_Ancient_I_Open: InvokeCommand( "archtool1", from ); break;
					case PageActionType.MagicToolbar_Ancient_I_Close: InvokeCommand( "archclose1", from ); break;
					case PageActionType.MagicToolbar_Ancient_II_Open: InvokeCommand( "archtool2", from ); break;
					case PageActionType.MagicToolbar_Ancient_II_Close: InvokeCommand( "archclose2", from ); break;
					case PageActionType.MagicToolbar_Ancient_III_Open: InvokeCommand( "archtool3", from ); break;
					case PageActionType.MagicToolbar_Ancient_III_Close: InvokeCommand( "archclose3", from ); break;
					case PageActionType.MagicToolbar_Ancient_IV_Open: InvokeCommand( "archtool4", from ); break;
					case PageActionType.MagicToolbar_Ancient_IV_Close: InvokeCommand( "archclose4", from ); break;
					default: Console.WriteLine("[HelpGump] Invalid open/close for magic toolbar: {0}", actionType); break;
				}
			}
			else
			{
				switch ( actionType )
				{
					case PageActionType.Close:
					{
						//from.SendLocalizedMessage( 501235, "", 0x35 ); // Help request aborted.
						break;
					}
					case PageActionType.Navigate_Main:
					{
						from.SendGump( new Server.Engines.Help.HelpGump( from, pressed ) );
						break;
					}
					case PageActionType.Do_Toggle_AFK:
					{
						InvokeCommand( "afk", from );
						from.SendGump( new Server.Engines.Help.HelpGump( from, pressed ) );
						break;
					}
					case PageActionType.Show_Chat:
					{
						InvokeCommand( "c", from );
						break;
					}
					case PageActionType.Do_CorpseClear:
					{
						InvokeCommand( "corpseclear", from );
						from.SendGump( new Server.Engines.Help.HelpGump( from, pressed ) );
						break;
					}
					case PageActionType.Do_CorpseSearch:
					{
						InvokeCommand( "corpse", from );
						break;
					}
					case PageActionType.Show_Emote:
					{
						InvokeCommand( "emote", from );
						break;
					}
					case PageActionType.Navigate_MagicToolbars:
					{
						from.SendGump( new Server.Engines.Help.HelpGump( from, 7 ) );
						break;
					}
					case PageActionType.Do_MoongateSearch:
					{
						InvokeCommand( "magicgate", from );
						break;
					}
					case PageActionType.Show_MOTD:
					{
						from.CloseGump( typeof( Joeku.MOTD.MOTD_Gump ) );
						Joeku.MOTD.MOTD_Utility.SendGump( from, false, 0, 1 );
						break;
					}
					case PageActionType.Show_Quests:
					{
						from.SendGump( new Server.Engines.Help.HelpGump( from, pressed ) );
						from.SendGump(new QuestLogGump((PlayerMobile)from));
						break;
					}
					case PageActionType.Show_QuickBar:
					{
						from.CloseGump( typeof( QuickBar ) );
						from.SendGump( new QuickBar( from ) );
						break;
					}
					case PageActionType.Show_RegBar:
					{
						from.CloseGump( typeof( RegBar ) );
						from.SendGump( new RegBar( from ) );
						break;
					}
					case PageActionType.Show_Settings:
					{
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Navigate_Library:
					{
						from.CloseGump( typeof( MyLibrary ) );
						from.SendSound( 0x4A ); 
						from.SendGump( new MyLibrary( from, 1 ) );
						break;
					}
					case PageActionType.Show_Statistics:
					{
						from.CloseGump( typeof( Server.Statistics.StatisticsGump ) );
						from.SendGump( new Server.Statistics.StatisticsGump( from, 1 ) );
						break;
					}
					case PageActionType.Do_StuckInWorld:
					{
						BaseHouse house = BaseHouse.FindHouseAt( from );

						if ( house != null && house.IsAosRules )
						{
							from.Location = house.BanLocation;
						}
						else if ( from.Region.IsPartOf( typeof( Server.Regions.Jail ) ) )
						{
							from.SendLocalizedMessage( 1041530, "", 0x35 ); // You'll need a better jailbreak plan then that!
						}
						else if ( from.CanUseStuckMenu() && from.Region.CanUseStuckMenu( from ) && !CheckCombat( from ) && !from.Frozen && !from.Criminal && (Core.AOS || from.Kills < 5) )
						{
							StuckMenu menu = new StuckMenu( from, from, true );

							menu.BeginClose();

							from.SendGump( menu );
						}

						break;
					}
					case PageActionType.Show_WeaponAbilities:
					{
						InvokeCommand( "sad", from );
						break;
					}
					case PageActionType.Show_WealthBar:
					{
						from.CloseGump( typeof( WealthBar ) );
						from.SendGump( new WealthBar( from ) );
						break;
					}
					case PageActionType.Show_Conversations:
					{
						from.CloseGump( typeof( MyChat ) );
						from.SendSound( 0x4A ); 
						from.SendGump( new MyChat( from, 1 ) );
						break;
					}
					case PageActionType.Navigate_Changelog:
					{
						from.SendGump( new Server.Engines.Help.HelpGump( from, pressed ) );
						break;
					}
					case PageActionType.Setting_OrdinaryResources:
					{
						if ( from.HarvestOrdinary )
							from.HarvestOrdinary = false;
						else
							from.HarvestOrdinary = true;

						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_SetLootContainer:
					case PageActionType.Setting_SetCraftingContainer:
					case PageActionType.Setting_SetHarvestingContainer:
					{
						int box;
						switch(actionType)
						{
							case PageActionType.Setting_SetLootContainer: box = 1; break;
							case PageActionType.Setting_SetCraftingContainer: box = 2; break;
							case PageActionType.Setting_SetHarvestingContainer: box = 3; break;
							default: Console.WriteLine("[HelpGump] Invalid container type: {0}", pressed); return;
						}

						BaseContainer.ContainerSetTarget( from, box );
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_WeaponAbilityNames:
					{
						if ( ((PlayerMobile)from).CharacterWepAbNames != 1 )
						{
							((PlayerMobile)from).CharacterWepAbNames = 1;
						}
						else
						{
							((PlayerMobile)from).CharacterWepAbNames = 0;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_AutoSheath:
					{
						if ( ((PlayerMobile)from).CharacterSheath == 1 )
						{
							((PlayerMobile)from).CharacterSheath = 0;
						}
						else
						{
							((PlayerMobile)from).CharacterSheath = 1;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MusicTone:
					{
						string tunes = ((PlayerMobile)from).CharMusical;

						if ( tunes == "Forest" )
						{
							((PlayerMobile)from).CharMusical = "Dungeon";
						}
						else
						{
							((PlayerMobile)from).CharMusical = "Forest";
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_PrivatePlay:
					{
						PlayerMobile pm = (PlayerMobile)from;

						if ( pm.PublicInfo == false )
						{
							pm.PublicInfo = true;
						}
						else
						{
							pm.PublicInfo = false;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_LootOptions:
					{
						from.CloseGump( typeof( LootChoices ) );
						from.SendGump( new LootChoices( from, 1 ) );
						break;
					}
					case PageActionType.Setting_SkillTitle:
					{
						from.CloseGump( typeof( SkillTitleGump ) );
						from.SendGump( new SkillTitleGump( from ) );
						break;
					}
					case PageActionType.Setting_UseAncientSpellbook:
					{
						if ( !ResearchSettings.BookCaster( from ) )
						{
							((PlayerMobile)from).UsingAncientBook = true;
						}
						else
						{
							((PlayerMobile)from).UsingAncientBook = false;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_SkillList:
					{
						if ( ((PlayerMobile)from).SkillDisplay > 0 ){ ((PlayerMobile)from).SkillDisplay = 0; } else { ((PlayerMobile)from).SkillDisplay = 1; }
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						Server.Gumps.SkillListingGump.RefreshSkillList( from );
						break;
					}
					case PageActionType.Show_SkillList:
					{
						Server.Gumps.SkillListingGump.OpenSkillList( from );
						break;
					}
					case PageActionType.Setting_GumpImages:
					{
						int gump = ((PlayerMobile)from).GumpHue;

						if ( gump > 0 )
						{
							((PlayerMobile)from).GumpHue = 0;
						}
						else
						{
							((PlayerMobile)from).GumpHue = 1;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_WeaponAbilityBar:
					{
						int wep = ((PlayerMobile)from).WeaponBarOpen;

						if ( wep > 0 )
						{
							((PlayerMobile)from).WeaponBarOpen = 0;
						}
						else
						{
							((PlayerMobile)from).WeaponBarOpen = 1;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_CreatureMagicFocus:
					{
						if ( from.RaceMagicSchool == 0 )
								from.RaceMagicSchool = 1;
						else if ( from.RaceMagicSchool == 1 )
								from.RaceMagicSchool = 2;
						else if ( from.RaceMagicSchool == 2 )
								from.RaceMagicSchool = 3;
						else
								from.RaceMagicSchool = 0;

						if ( from.FindItemOnLayer( Layer.Special ) != null && from.RaceID > 0 )
						{
							if ( from.FindItemOnLayer( Layer.Special ) is BaseRace )
								Server.Items.BaseRace.SetMonsterMagic( from, (BaseRace)(from.FindItemOnLayer( Layer.Special )) );
						}

						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_CreatureType:
					{
						from.RaceSection = 1;
						from.SendGump( new Server.Items.RacePotions.RacePotionsGump( from, 1 ) );
						break;
					}
					case PageActionType.Setting_CreatureSounds:
					{
						if ( !from.RaceMakeSounds )
								from.RaceMakeSounds = true;
						else
								from.RaceMakeSounds = false;

						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_Playstyle_Normal:
					{
						((PlayerMobile)from).CharacterEvil = 0;
						((PlayerMobile)from).CharacterOriental = 0;
						((PlayerMobile)from).CharacterBarbaric = 0;
						Server.Items.BarbaricSatchel.GetRidOf( from );
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_Playstyle_Evil:
					{
						((PlayerMobile)from).CharacterEvil = 1;
						((PlayerMobile)from).CharacterOriental = 0;
						((PlayerMobile)from).CharacterBarbaric = 0;
						Server.Items.BarbaricSatchel.GetRidOf( from );
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_Playstyle_Oriental:
					{
						((PlayerMobile)from).CharacterEvil = 0;
						((PlayerMobile)from).CharacterOriental = 1;
						((PlayerMobile)from).CharacterBarbaric = 0;
						Server.Items.BarbaricSatchel.GetRidOf( from );
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MessageColors:
					{
						if ( from.RainbowMsg )
						{
							from.RainbowMsg = false;
						}
						else
						{
							from.RainbowMsg = true;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_AutoAttack:
					{
						if ( from.NoAutoAttack )
						{
							from.NoAutoAttack = false;
						}
						else
						{
							from.NoAutoAttack = true;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_Playstyle_Barbaric:
					{
						if ( ((PlayerMobile)from).CharacterBarbaric == 1 && from.Female )
						{
							((PlayerMobile)from).CharacterBarbaric = 2;
						}
						else if ( ((PlayerMobile)from).CharacterBarbaric > 0 )
						{
							((PlayerMobile)from).CharacterBarbaric = 0;
							Server.Items.BarbaricSatchel.GetRidOf( from );
						}
						else
						{
							((PlayerMobile)from).CharacterEvil = 0;
							((PlayerMobile)from).CharacterOriental = 0;
							((PlayerMobile)from).CharacterBarbaric = 1;
							Server.Items.BarbaricSatchel.GivePack( from );
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_ClassicPoisoning:
					{
						if ( ((PlayerMobile)from).ClassicPoisoning == 1 )
						{
							((PlayerMobile)from).ClassicPoisoning = 0;
						}
						else
						{
							((PlayerMobile)from).ClassicPoisoning = 1;
						}
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MusicPlaylist:
					{
						from.CloseGump( typeof( MusicPlaylist ) );
						from.SendGump( new MusicPlaylist( from, 1 ) );
						break;
					}
					case PageActionType.Setting_DoubleClickToIDItems:
					{
						((PlayerMobile)from).DoubleClickID = !((PlayerMobile)from).DoubleClickID;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}

					case PageActionType.MagicToolbar_BardSongsBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsBard1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_BardSongsBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsBard2( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_KnightSpellBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsKnight1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_KnightSpellBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsKnight2( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_DeathKnightSpellBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsDeath1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_DeathKnightSpellBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsDeath2( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_MagerySpellBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsMage1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_MagerySpellBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsMage2( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_MagerySpellBarIII_Config:
					{
						TryConfigureSpellBar( new SetupBarsMage3( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_MagerySpellBarIV_Config:
					{
						TryConfigureSpellBar( new SetupBarsMage4( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_NecromancerSpellBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsNecro1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_NecromancerSpellBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsNecro2( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_PriestSpellBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsPriest1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_PriestSpellBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsPriest2( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.Setting_CustomTitle:
					{
						from.SendGump( new CustomTitleGump( from ) );
						break;
					}
					case PageActionType.MagicToolbar_AncientSpellBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsArch1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_AncientSpellBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsArch2( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_AncientSpellBarIII_Config:
					{
						TryConfigureSpellBar( new SetupBarsArch3( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_AncientSpellBarIV_Config:
					{
						TryConfigureSpellBar( new SetupBarsArch4( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_MonkSpellBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsMonk1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_MonkSpellBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsMonk2( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_ElementalSpellBarI_Config:
					{
						TryConfigureSpellBar( new SetupBarsElement1( (PlayerMobile)from, 1 ) );
						break;
					}
					case PageActionType.MagicToolbar_ElementalSpellBarII_Config:
					{
						TryConfigureSpellBar( new SetupBarsElement2( (PlayerMobile)from, 1 ) );
						break;
					}

					case PageActionType.Setting_MagerySpellColor_White:
					{
						((PlayerMobile)from).MagerySpellHue = 0x47E;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MagerySpellColor_Black:
					{
						((PlayerMobile)from).MagerySpellHue = 0x94E;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MagerySpellColor_Blue:
					{
						((PlayerMobile)from).MagerySpellHue = 0x48D;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MagerySpellColor_Red:
					{
						((PlayerMobile)from).MagerySpellHue = 0x48E;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MagerySpellColor_Green:
					{
						((PlayerMobile)from).MagerySpellHue = 0x48F;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MagerySpellColor_Purple:
					{
						((PlayerMobile)from).MagerySpellHue = 0x490;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MagerySpellColor_Yellow:
					{
						((PlayerMobile)from).MagerySpellHue = 0x491;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
					case PageActionType.Setting_MagerySpellColor_Default:
					{
						((PlayerMobile)from).MagerySpellHue = 0;
						from.SendGump( new Server.Engines.Help.HelpGump( from, 12 ) );
						break;
					}
				}
			}
		}

		public static string MyQuests( Mobile from )
        {
			PlayerMobile pm = (PlayerMobile)from;

			string sQuests = "Below is a brief list of current quests, along with achievements in specific discoveries. These are owned quests, which are specific to your character. Other quests (like messages in a bottle, treasure maps, or scribbled notes) are not listed here.<br><br>";

			string ContractQuest = PlayerSettings.GetQuestInfo( from, "StandardQuest" );
			if ( PlayerSettings.GetQuestState( from, "StandardQuest" ) ){ string sAdventurer = StandardQuestFunctions.QuestStatus( from ); sQuests = sQuests + "-" + sAdventurer + ".<br><br>"; }

			string ContractKiller = PlayerSettings.GetQuestInfo( from, "AssassinQuest" );
			if ( PlayerSettings.GetQuestState( from, "AssassinQuest" ) ){ string sAssassin = AssassinFunctions.QuestStatus( from ); sQuests = sQuests + "-" + sAssassin + ".<br><br>"; }

			string ContractSailor = PlayerSettings.GetQuestInfo( from, "FishingQuest" );
			if ( PlayerSettings.GetQuestState( from, "FishingQuest" ) ){ string sSailor = FishingQuestFunctions.QuestStatus( from ); sQuests = sQuests + "-" + sSailor + ".<br><br>"; }

			sQuests = sQuests + OtherQuests( from );

			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleMadGodName" ) ){ sQuests = sQuests + "-Learned about the Mad God Tarjan.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleCatacombKey" ) ){ sQuests = sQuests + "-The priest from the Mad God Temple gave me the key to the Catacombs.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleSpectreEye" ) ){ sQuests = sQuests + "-Found a mysterious eye from the Catacombs.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleHarkynKey" ) ){ sQuests = sQuests + "-Found a key with a symbol of a dragon on it.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleDragonKey" ) ){ sQuests = sQuests + "-Found a rusty key from around a gray dragon's neck.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleCrystalSword" ) ){ sQuests = sQuests + "-Found a crystal sword.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleSilverSquare" ) ){ sQuests = sQuests + "-Found a silver square.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleKylearanKey" ) ){ sQuests = sQuests + "-Found a key with a symbol of a unicorn on it.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleBedroomKey" ) ){ sQuests = sQuests + "-Found a key with a symbol of a tree on it.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleSilverTriangle" ) ){ sQuests = sQuests + "-Found a silver triangle.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleCrystalGolem" ) ){ sQuests = sQuests + "-Destroyed the crystal golem and found a golden key.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleEbonyKey" ) ){ sQuests = sQuests + "-Kylearan gave me an ebony key with a demon symbol on it.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleSilverCircle" ) ){ sQuests = sQuests + "-Found a silver circle.<br><br>"; }
			if ( PlayerSettings.GetBardsTaleQuest( from, "BardsTaleWin" ) && ((PlayerMobile)from).Fugitive != 1 ){ sQuests = sQuests + "-Defeated the evil wizard Mangar and escaped Skara Brae.<br><br>"; }

			if ( PlayerSettings.GetKeys( from, "UndermountainKey" ) ){ sQuests = sQuests + "-Found a key made of dwarven steel.<br><br>"; }
			if ( PlayerSettings.GetKeys( from, "BlackKnightKey" ) ){ sQuests = sQuests + "-Found the Black Knight's key.<br><br>"; }
			if ( PlayerSettings.GetKeys( from, "SkullGate" ) ){ sQuests = sQuests + "-Discovered the secret of Skull Gate.<br>   One is in the Undercity of Umbra in Sosaria.<br>   The other is in the Ravendark Woods.<br><br>"; }
			if ( PlayerSettings.GetKeys( from, "SerpentPillars" ) ){ sQuests = sQuests + "-Discovered the secret of the Serpent Pillars.<br>   Sosaria: 86° 41'S, 124° 39'E<br>   Lodoria: 35° 36'S, 65° 2'E<br><br>"; }
			if ( PlayerSettings.GetKeys( from, "RangerOutpost" ) ){ sQuests = sQuests + "-Discovered the Ranger Outpost.<br><br>"; }
			if ( PlayerSettings.GetKeys( from, "DragonRiding" ) ){ sQuests = sQuests + "-Learned the secrets of riding draconic creatures.<br><br>"; }

			if ( PlayerSettings.GetDiscovered( from, Land.Sosaria ) ){ sQuests = sQuests + "-Discovered the World of Sosaria.<br><br>"; }
			if ( PlayerSettings.GetDiscovered( from, Land.UmberVeil ) ){ sQuests = sQuests + "-Discovered Umber Veil.<br><br>"; }
			if ( PlayerSettings.GetDiscovered( from, Land.Ambrosia ) ){ sQuests = sQuests + "-Discovered Ambrosia.<br><br>"; }
			if ( PlayerSettings.GetDiscovered( from, Land.Lodoria ) ){ sQuests = sQuests + "-Discovered the Elven World of Lodoria.<br><br>"; }
			if ( PlayerSettings.GetDiscovered( from, Land.Serpent ) ){ sQuests = sQuests + "-Discovered the Serpent Island.<br><br>"; }
			if ( PlayerSettings.GetDiscovered( from, Land.IslesDread ) ){ sQuests = sQuests + "-Discovered the Isles of Dread.<br><br>"; }
			if ( PlayerSettings.GetDiscovered( from, Land.Savaged ) ){ sQuests = sQuests + "-Discovered the Valley of the Savaged Empire.<br><br>"; }
			if ( PlayerSettings.GetDiscovered( from, Land.Kuldar ) ){ sQuests = sQuests + "-Discovered the Bottle World of Kuldar.<br><br>"; }
			if ( PlayerSettings.GetDiscovered( from, Land.Underworld ) ){ sQuests = sQuests + "-Discovered the Underworld.<br><br>"; }

			return "Quests For " + from.Name + "<br><br>" + sQuests;
        }

		public static string OtherQuests( Mobile from )
        {
			string quests = "";

			ArrayList targets = new ArrayList();
			foreach ( Item item in World.Items.Values )
			{
				if ( item is ThiefNote )
				{
					if ( ((ThiefNote)item).NoteOwner == from )
					{
						if ( Server.Items.ThiefNote.ThiefAllowed( from ) == null )
						{
							quests = quests + "-" + ((ThiefNote)item).NoteStory + "<br><br>";
						}
						else
						{
							quests = quests + "-You have a secret note instructing you to steal something, but you will take a break from thieving and read it in about " + Server.Items.ThiefNote.ThiefAllowed( from ) + " minutes.<br><br>";
						}
					}
				}
				else if ( item is CourierMail )
				{
					if ( ((CourierMail)item).Owner == from )
					{
						quests = quests + "-You need to find " + ((CourierMail)item).SearchItem + " for " + ((CourierMail)item).ForWho + ". They said in their letter that you should search in " + ((CourierMail)item).SearchDungeon + " in " + ((CourierMail)item).SearchWorld + ".<br><br>";
					}
				}
				else if ( item is SearchPage )
				{
					if ( ((SearchPage)item).Owner == from )
					{
						quests = quests + "-You want to find " + ((SearchPage)item).SearchItem + " in " + ((SearchPage)item).SearchDungeon + " in " + ((SearchPage)item).SearchWorld + ".<br><br>";
					}
				}
				else if ( item is SummonPrison )
				{
					if ( ((SummonPrison)item).owner == from )
					{
						quests = quests + "-You currently have " + System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(((SummonPrison)item).Prisoner.ToLower()) + " in a Magical Prison.<br><br>";
					}
				}
				else if ( item is FrankenJournal )
				{
					if ( ((FrankenJournal)item).JournalOwner == from )
					{
						int parts = 0;
						if ( ((FrankenJournal)item).HasArmRight > 0 ){ parts++; }
						if ( ((FrankenJournal)item).HasArmLeft > 0 ){ parts++; }
						if ( ((FrankenJournal)item).HasLegRight > 0 ){ parts++; }
						if ( ((FrankenJournal)item).HasLegLeft > 0 ){ parts++; }
						if ( ((FrankenJournal)item).HasTorso > 0 ){ parts++; }
						if ( ((FrankenJournal)item).HasHead > 0 ){ parts++; }

						quests = quests + "-You currently have " + parts + " out of 6 body parts needed to create a flesh golem.<br><br>";
					}
				}
				else if ( item is RuneBox )
				{
					if ( ((RuneBox)item).RuneBoxOwner == from )
					{
						int runes = 0;
						if ( ((RuneBox)item).HasCompassion > 0 ){ runes++; }
						if ( ((RuneBox)item).HasHonesty > 0 ){ runes++; }
						if ( ((RuneBox)item).HasHonor > 0 ){ runes++; }
						if ( ((RuneBox)item).HasHumility > 0 ){ runes++; }
						if ( ((RuneBox)item).HasJustice > 0 ){ runes++; }
						if ( ((RuneBox)item).HasSacrifice > 0 ){ runes++; }
						if ( ((RuneBox)item).HasSpirituality > 0 ){ runes++; }
						if ( ((RuneBox)item).HasValor > 0 ){ runes++; }

						quests = quests + "-You currently have " + runes + " out of 8 runes of virtue.<br><br>";
					}
				}
				else if ( item is SearchPage )
				{
					if ( ((SearchPage)item).Owner == from )
					{
						quests = quests + "-You are on a quest to obtain the " + ((SearchPage)item).SearchItem + ".<br><br>";
					}
				}
				else if ( item is VortexCube )
				{
					if ( ((VortexCube)item).CubeOwner == from )
					{
						VortexCube cube = (VortexCube)item;
						quests = quests + "-You are searching for the Codex of Ultimate Wisdom.<br>";

						if ( cube.HasConvexLense > 0 ){ quests = quests + "   -You have the Convex Lense.<br>"; }
						if ( cube.HasConcaveLense > 0 ){ quests = quests + "   -You have the Concave Lense.<br>"; }

						if ( cube.HasKeyLaw > 0 ){ quests = quests + "   -You have the Key of Law.<br>"; }
						if ( cube.HasKeyBalance > 0 ){ quests = quests + "   -You have the Key of Balance.<br>"; }
						if ( cube.HasKeyChaos > 0 ){ quests = quests + "   -You have the Key of Chaos.<br>"; }

						if ( cube.HasCrystalRed > 0 ){ quests = quests + "   -You have the Red Void Crystal.<br>"; }
						if ( cube.HasCrystalBlue > 0 ){ quests = quests + "   -You have the Blue Void Crystal.<br>"; }
						if ( cube.HasCrystalGreen > 0 ){ quests = quests + "   -You have the Green Void Crystal.<br>"; }
						if ( cube.HasCrystalYellow > 0 ){ quests = quests + "   -You have the Yellow Void Crystal.<br>"; }
						if ( cube.HasCrystalWhite > 0 ){ quests = quests + "   -You have the White Void Crystal.<br>"; }
						if ( cube.HasCrystalPurple > 0 ){ quests = quests + "   -You have the Purple Void Crystal.<br>"; }

						quests = quests + "<br>";
					}
				}
				else if ( item is ObeliskTip )
				{
					if ( ((ObeliskTip)item).ObeliskOwner == from )
					{
						ObeliskTip obelisk = (ObeliskTip)item;
						quests = quests + "-You are trying to become a Titan of Ether.<br>";
						quests = quests + "   -You have the Obelisk Tip.<br>"; 

						if ( obelisk.WonAir > 0 ){ quests = quests + "   -You have defeated Stratos, the Titan of Air.<br>"; }
						else if ( obelisk.HasAir > 0 ){ quests = quests + "   -You have the Breath of Air.<br>"; }
						if ( obelisk.WonFire > 0 ){ quests = quests + "   -You have defeated Pyros, the Titan of Fire.<br>"; }
						else if ( obelisk.HasFire > 0 ){ quests = quests + "   -You have the Tongue of Flame.<br>"; }
						if ( obelisk.WonEarth > 0 ){ quests = quests + "   -You have defeated Lithos, the Titan of Earth.<br>"; }
						else if ( obelisk.HasEarth > 0 ){ quests = quests + "   -You have the Heart of Earth.<br>"; }
						if ( obelisk.WonWater > 0 ){ quests = quests + "   -You have defeated Hydros, the Titan of Water.<br>"; }
						else if ( obelisk.HasWater > 0 ){ quests = quests + "   -You have the Tear of the Seas.<br>"; }

						quests = quests + "<br>";
					}
				}
				else if ( item is MuseumBook )
				{
					if ( ((MuseumBook)item).ArtOwner == from )
					{
						quests = quests + "-You have found " + MuseumBook.GetTotal( (MuseumBook)item ) + " out of 60 antiques for the museum.<br><br>";
					}
				}
				else if ( item is RuneBox )
				{
					if ( ((RuneBox)item).RuneBoxOwner == from )
					{
						int runes = ((RuneBox)item).HasCompassion + ((RuneBox)item).HasHonesty + ((RuneBox)item).HasHonor + ((RuneBox)item).HasHumility + ((RuneBox)item).HasJustice + ((RuneBox)item).HasSacrifice + ((RuneBox)item).HasSpirituality + ((RuneBox)item).HasValor;
						quests = quests + "-You have found " + runes + " out of 8 runes of virtue.<br><br>";
					}
				}
				else if ( item is QuestTome )
				{
					if ( ((QuestTome)item).QuestTomeOwner == from )
					{
						quests = quests + "-You are on a quest to find " + ((QuestTome)item).GoalItem4 + ".<br><br>";
					}
				}
			}

			if ( 	from.Backpack.FindItemByType( typeof ( ScalesOfEthicality ) ) != null || 
					from.Backpack.FindItemByType( typeof ( OrbOfLogic ) ) != null || 
					from.Backpack.FindItemByType( typeof ( LanternOfDiscipline ) ) != null || 
					from.Backpack.FindItemByType( typeof ( BlackrockSerpentOrder ) ) != null || 
					from.Backpack.FindItemByType( typeof ( BlackrockSerpentChaos ) ) != null || 
					from.Backpack.FindItemByType( typeof ( BlackrockSerpentBalance ) ) != null )
			{
				quests = quests + "-You are on a quest to bring the Serpents back into balance.<br><br>";
			}

			if ( 	from.Backpack.FindItemByType( typeof ( ShardOfFalsehood ) ) != null || 
					from.Backpack.FindItemByType( typeof ( ShardOfCowardice ) ) != null || 
					from.Backpack.FindItemByType( typeof ( ShardOfHatred ) ) != null || 
					from.Backpack.FindItemByType( typeof ( CandleOfLove ) ) != null || 
					from.Backpack.FindItemByType( typeof ( BookOfTruth ) ) != null || 
					from.Backpack.FindItemByType( typeof ( BellOfCourage ) ) != null )
			{
				quests = quests + "-You are on a quest to destroy the Shadowlords and construct a Gem of Immortality.<br><br>";
			}
			else if ( from.Backpack.FindItemByType( typeof ( GemImmortality ) ) != null )
			{
				quests = quests + "-You have constructed a Gem of Immortality.<br><br>";
			}

			if ( PlayerSettings.GetKeys( from, "Museums" ) )
			{
				quests = quests + "-You have found all of the antiques for the Museum.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Gygax" ) )
			{
				quests = quests + "-You have obtained the Statue of Gygax.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Virtues" ) )
			{
				quests = quests + "-You have cleansed all of the Runes of Virtue.<br><br>";
			}
			else if ( PlayerSettings.GetKeys( from, "Corrupt" ) )
			{
				quests = quests + "-You have corrupted all of the Runes of Virtue.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Exodus" ) )
			{
				quests = quests + "-You have destroyed the Core of Exodus.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "BlackGateDemon" ) )
			{
				quests = quests + "-You have defeated the Black Gate Demon and found a portal to the Ethereal Plane.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Jormungandr" ) )
			{
				quests = quests + "-You have defeated the legendary serpent known as Jormungandr.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Dracula" ) )
			{
				quests = quests + "-You have destroyed Dracula, the ruler of all vampires.<br><br>";
			}
			if ( 	from.Backpack.FindItemByType( typeof ( StaffPartVenom ) ) != null || 
					from.Backpack.FindItemByType( typeof ( StaffPartCaddellite ) ) != null || 
					from.Backpack.FindItemByType( typeof ( StaffPartFire ) ) != null || 
					from.Backpack.FindItemByType( typeof ( StaffPartLight ) ) != null || 
					from.Backpack.FindItemByType( typeof ( StaffPartEnergy ) ) != null )
			{
				quests = quests + "-You are seeking to assemble the Staff of Ultimate Power.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Arachnar" ) )
			{
				quests = quests + "-You have defeated Arachnar, the guardian of the staff.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Surtaz" ) )
			{
				quests = quests + "-You have defeated Surtaz, the guardian of the staff.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Vordinax" ) )
			{
				quests = quests + "-You have defeated Vordinax, the guardian of the staff.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Vulcrum" ) )
			{
				quests = quests + "-You have defeated Vulcrum, the guardian of the staff.<br><br>";
			}
			if ( PlayerSettings.GetKeys( from, "Xurtzar" ) )
			{
				quests = quests + "-You have defeated Xurtzar, the guardian of the staff.<br><br>";
			}

			return quests;
		}

		public static string MyHelp()
        {
			string HelpText = "If you are looking for help exploring this world, you can learn about almost anything within the game world you travel. Some merchants sell scrolls or books that will explain how some skills can be performed, resources gathered, and even how elements of the world can be manipulated. A sage often sells many tomes of useful information on skills, weapon abilities, or various types of magics available. If you are totally new to this game, buy yourself a Guide to Adventure book from a sage if you lost the one you started with. This book explains how to navigate and play the game. You will also learn some things about how the world behaves such as merchant interactions, how to use items, and what to do when your character dies. Talk to the townsfolk to learn whatever you can. On this screen there are many options, information, and settings that can assist in your journey. Many of the options here have keyboard commands that are listed below. Make sure to check out the 'Info' section on your character's paperdoll as it has some vital information about your character.<br><br>"
				+ "Common Commands: Below are the commands you can use for various things in the game.<br><br>"
				+ "[abilitynames - Turns on/off the special weapon ability names next to the appropriate icons.<br><br>"
				+ "[afk - Turns on/off the notification to others that you are away from keyboard.<br><br>"
				+ "[ancient - Turns on/off whether you are using magic from the research bag or the ancient spellbook.<br><br>"
				+ "[autoattack - Turns on/off whether you auto attack when attacked.<br><br>"
				+ "[bandother - Bandage other command.<br><br>"
				+ "[bandself - Bandage self command.<br><br>"
				+ "[barbaric - Turns on/off the barbaric flavor the game provides (see end).<br><br>"
				+ "[c - Initiates the chat system.<br><br>"
				+ "[corpse - Helps one find their remains.<br><br>"
				+ "[corpseclear - Removes your corpse from a ship's deck.<br><br>"
				+ "[e - Opens the emote mini window.<br><br>"
				+ "[emote - Opens the emote window.<br><br>"
				+ "[evil - Turns on/off the evil flavor the game provides (see end).<br><br>"
				+ "[feats - Opens the Achievements window.<br><br>"
				+ "[loot - Automatically take certain items from common dungeon chests or corpses and put them in your backpack. The unknown items are those that will need identification, but you may decide to take them anyway. The reagent options have a few categories. Magery and necromancer reagents are those used specifically by those characters, where witches brew reagents mainly fall into the necromancer category. Alchemic reagents are those that fall outside the category of magery and necromancer reagents, and only alchemists use them. Herbalist reagents are useful in druidic herbalism.<br><br>"
				+ "[magicgate - Helps one find the nearest magical gate.<br><br>"
				+ "[motd - Opens the message of the day.<br><br>"
				+ "[oriental - Turns on/off the oriental flavor the game provides (see end).<br><br>"
				+ "[password - Change your account password.<br><br>"
				+ "[poisons - This changes how poisoned weapons work, which can be for either precise control with special weapon infectious strikes (default) or with hits of a one-handed slashing or piercing weapon.<br><br>"
				+ "[private - Turns on/off detailed messages of your journey for the town crier and local citizen chatter.<br><br>"
				+ "[quests - Opens a scroll to show certain quest events.<br><br>"
				+ "[quickbar - Opens a small, vertical bar with common game functions for easier use.<br><br>"
				+ "[sad - Opens the weapon's special abilities.<br><br>"
				+ "[set1 - Sets your weapon's first ability to active.<br>"
				+ "[set2 - Sets your weapon's second ability to active.<br>"
				+ "[set3 - Sets your weapon's third ability to active.<br>"
				+ "[set4 - Sets your weapon's fourth ability to active.<br>"
				+ "[set5 - Sets your weapon's fifth ability to active.<br><br>"
				+ "[sheathe - Turns on/off the feature to sheathe your weapon when not in battle.<br><br>"
				+ "[skill - Shows you what each skill is used for.<br><br>"
				+ "[skilllist - Displays a more condensed list of skills you have set to 'up' and perhaps 'locked'.<br><br>"
				+ "[spellhue ## - This command, following by a color reference hue number, will change all of your magery spell effects to that color. A value of '1' will normally render as '0' so avoid that setting as it will not produce the result you may want.<br><br>"
				+ "[statistics - Shows you some statistics of the server.<br><br>"
				+ "[wealth - Opens a small, horizontal bar showing your gold value for the various forms of currency and gold in your bank and backpack. Currency are items you would have a banker convert to gold for you (silver, copper, xormite, jewels, and crystals). If you put these items in your bank, you can update the values on the wealth bar by right clicking on it.<br><br>"

				+ "<br><br>"

				+ "Area Difficulty Levels: When you enter many dangerous areas, there will be a message to you that you entered a particular area. There may be a level of difficulty shown in parenthesis, that will give you an indication on the difficulty of the area. Below are the descriptions for each level.<br><br>"
				+ " - Easy (Not much of a challenge)<br>"
				+ " - Normal (An average level of challenge)<br>"
				+ " - Difficult (A tad more difficult)<br>"
				+ " - Challenging (You will probably run away alot)<br>"
				+ " - Hard (You will probably die alot)<br>"
				+ " - Deadly (I dare you)<br>"
				// + " - Epic (For Titans of Ether)<br>"

				+ "<br><br>"

				+ "Skill Titles: You can set your default title for your character. Although you may be a Grandmaster Driven, you may want your title to reflect your Apprentice Wizard title instead. This is how you set it...<br><br>"
				+ "Type the '[SkillName' command followed by the name of the skill you want to set as your default. Make sure you surround the skill name in quotes and all lowercase. Example...<br>"
				+ "  [SkillName \"taming\"<br><br>"
				+ "If you want the game to manage your character's title, simply use the same command with a skill name of \"clear\".<br><br>"

				+ "<br><br>"

				+ "Reagent Bars: Below are the commands you can use to watch your reagent quantities as you cast spells or create potions. These are customizable bars that will show the quantities of the reagents you are carrying. These will show updated quantities of reagents whenever you cast a spell or make a potion that uses them. Otherwise you can make a macro to these commands and use them to refresh the amounts manually.<br><br>"
				+ "[regbar - Opens the reagent bar.<br><br>"
				+ "[regclose - Closes the reagent bar.<br><br>"

				+ "<br><br>"

				+ "Magic Toolbars: Below are the commands you can use to manage magic toolbars that might help you play better.<br><br>"
				+ "[archspell1 - Opens the 1st ancient spell bar editor.<br>"
				+ "[archspell2 - Opens the 2nd ancient spell bar editor.<br>"
				+ "[archspell3 - Opens the 3rd ancient spell bar editor.<br>"
				+ "[archspell4 - Opens the 4th ancient spell bar editor.<br>"
				+ "[archtool1 - Opens the 1st ancient spell bar.<br>"
				+ "[archtool2 - Opens the 2nd ancient spell bar.<br>"
				+ "[archtool3 - Opens the 3rd ancient spell bar.<br>"
				+ "[archtool4 - Opens the 4th ancient spell bar.<br>"
				+ "[archclose1 - Closes the 1st ancient spell bar.<br>"
				+ "[archclose2 - Closes the 2nd ancient spell bar.<br>"
				+ "[archclose3 - Closes the 3rd ancient spell bar.<br>"
				+ "[archclose4 - Closes the 4th ancient spell bar.<br>"
				+ "<br>"

				+ "[bardsong1 - Opens the 1st bard song bar editor.<br>"
				+ "[bardsong2 - Opens the 2nd bard song bar editor.<br>"
				+ "[bardtool1 - Opens the 1st bard song bar.<br>"
				+ "[bardtool2 - Opens the 2nd bard song bar.<br>"
				+ "[bardclose1 - Closes the 1st bard song bar.<br>"
				+ "[bardclose2 - Closes the 2nd bard song bar.<br>"
				+ "<br>"

				+ "[knightspell1 - Opens the 1st knight spell bar editor.<br>"
				+ "[knightspell2 - Opens the 2nd knight spell bar editor.<br>"
				+ "[knighttool1 - Opens the 1st knight spell bar.<br>"
				+ "[knighttool2 - Opens the 2nd knight spell bar.<br>"
				+ "[knightclose1 - Closes the 1st knight spell bar.<br>"
				+ "[knightclose2 - Closes the 2nd knight spell bar.<br>"
				+ "<br>"

				+ "[deathspell1 - Opens the 1st death knight spell bar editor.<br>"
				+ "[deathspell2 - Opens the 2nd death knight spell bar editor.<br>"
				+ "[deathtool1 - Opens the 1st death knight spell bar.<br>"
				+ "[deathtool2 - Opens the 2nd death knight spell bar.<br>"
				+ "[deathclose1 - Closes the 1st death knight spell bar.<br>"
				+ "[deathclose2 - Closes the 2nd death knight spell bar.<br>"
				+ "<br>"

				+ "[elementspell1 - Opens the 1st elemental spell bar editor.<br>"
				+ "[elementspell2 - Opens the 2nd elemental spell bar editor.<br>"
				+ "[elementtool1 - Opens the 1st elemental spell bar.<br>"
				+ "[elementtool2 - Opens the 2nd elemental spell bar.<br>"
				+ "[elementclose1 - Closes the 1st elemental spell bar.<br>"
				+ "[elementclose2 - Closes the 2nd elemental spell bar.<br>"
				+ "<br>"

				+ "[holyspell1 - Opens the 1st priest prayer bar editor.<br>"
				+ "[holyspell2 - Opens the 2nd priest prayer bar editor.<br>"
				+ "[holytool1 - Opens the 1st priest prayer bar.<br>"
				+ "[holytool2 - Opens the 2nd priest prayer bar.<br>"
				+ "[holyclose1 - Closes the 1st priest prayer bar.<br>"
				+ "[holyclose2 - Closes the 2nd priest prayer bar.<br>"
				+ "<br>"

				+ "[magespell1 - Opens the 1st mage spell bar editor.<br>"
				+ "[magespell2 - Opens the 2nd mage spell bar editor.<br>"
				+ "[magespell3 - Opens the 3rd mage spell bar editor.<br>"
				+ "[magespell4 - Opens the 4th mage spell bar editor.<br>"
				+ "[magetool1 - Opens the 1st mage spell bar.<br>"
				+ "[magetool2 - Opens the 2nd mage spell bar.<br>"
				+ "[magetool3 - Opens the 3rd mage spell bar.<br>"
				+ "[magetool4 - Opens the 4th mage spell bar.<br>"
				+ "[mageclose1 - Closes the 1st mage spell bar.<br>"
				+ "[mageclose2 - Closes the 2nd mage spell bar.<br>"
				+ "[mageclose3 - Closes the 3rd mage spell bar.<br>"
				+ "[mageclose4 - Closes the 4th mage spell bar.<br>"
				+ "<br>"

				+ "[monkspell1 - Opens the 1st monk ability bar editor.<br>"
				+ "[monkspell2 - Opens the 2nd monk ability bar editor.<br>"
				+ "[monktool1 - Opens the 1st monk ability bar.<br>"
				+ "[monktool2 - Opens the 2nd monk ability bar.<br>"
				+ "[monkclose1 - Closes the 1st monk ability bar.<br>"
				+ "[monkclose2 - Closes the 2nd monk ability bar.<br>"
				+ "<br>"

				+ "[necrospell1 - Opens the 1st necromancer spell bar editor.<br>"
				+ "[necrospell2 - Opens the 2nd necromancer spell bar editor.<br>"
				+ "[necrotool1 - Opens the 1st necromancer spell bar.<br>"
				+ "[necrotool2 - Opens the 2nd necromancer spell bar.<br>"
				+ "[necroclose1 - Closes the 1st necromancer spell bar.<br>"
				+ "[necroclose2 - Closes the 2nd necromancer spell bar.<br>"

				+ "<br><br>"

				+ "Music: There is many different pieces of classic music in the game, and they play depending on areas you visit. Some of the music is from the original game, but there are some pieces from older games. There are also some pieces from computer games in the 1990's, but they really fit the theme when traveling the land. You can choose to listen to them, or change the music you are listening to when exploring the world. Keep in mind that when you change the music, and you enter a new area, the default music for that area will play and you may have to change your music again. Also keep in mind that your game client will want to play the song for a few seconds before allowing a switch of new music. You can use the below command to open a window that allows you to choose a song to play. Almost all of them play in a loop, where there are three that do not and are marked with an asterisk. There are two pages of songs to choose from so use the top arrow to go back and forth to each screen. When your music begins to play, then press the OKAY button to exit the screen. Although an unnecessary function, it does give you some control over the music in the game.<br><br>"
				+ "[music - Opens the music playlist and player.<br><br>"
				+ "The below command will simply toggle your music preference to play a different set of music in the dungeons. When turned on, it will play music you normally hear when traveling the land, instead of the music commonly played in dungeons.<br><br>"
				+ "[musical - Sets the default dungeon music.<br><br>"

				+ "<br><br>"

				+ "Evil Style: There is an evil element to the game that some want to participate in. With classes such as Necromancers, some players may want to travel a world with this flavor added. This particular setting allows you to toggle between regular and evil flavors. When in the evil mode, some of the treasure you will find will often have a name that fits in the evil style. When you stay within negative karma, skill titles will change for you as well, but not all. Look over the book of skill titles (found within the game world) to see which titles will change based on karma. Some of the relics you will find may also have this style, to perhaps decorate a home in this fashion. This option can be turned off and on at any time. You can only have one type of play style active at any one time.<br><br>"
				+ "[evil - Turns on/off the evil flavor the game provides.<br><br>"

				+ "<br><br>"

				+ "Oriental Style: There is an oriental element to the game that most do not want to participate in. With classes such as Ninja and Samurai, some players may want to travel a world with this flavor added. This particular setting allows you to toggle between fantasy and oriental. When in the oriental mode, half of the treasure you will find will be of Chinese or Japanese historical origins. These types of items will most times be named to match the style. Items that once belonged to someone, will often have a name that fits in the oriental style. Some of the skill titles will change for you as well, but not all. Look over the book of skill titles (found within the game world) to see which titles will change based on this play style. Some of the relics and artwork you will find will also have this style, to perhaps decorate a home in this fashion. This option can be turned off and on at any time. You can only have one type of play style active at any one time.<br><br>"
				+ "[oriental - Turns on/off the oriental flavor the game provides.<br><br>"

				+ "<br><br>"

				+ "Barbaric Style: The default game does not lend itself to a sword and sorcery experience. This means that it is not the most optimal play experience to be a loin cloth wearing barbarian that roams the land with a huge axe. Characters generally get as much equipment as they can in order to maximize their rate of survivability. This particular play style can help in this regard. Choosing to play in this style will have a satchel appear in your main pack. You cannot store anything in this satchel, as its purpose is to change certain pieces of equipment you place into it. It will change shields, hats, helms, tunics, sleeves, leggings, boots, gorgets, gloves, necklaces, cloaks, and robes. When these items get changed, they will become something that appears differently but behave in the same way the previous item did. These different items can be equipped but may not appear on your character. Also note that when you wear robes, they cover your character's tunics and sleeves. Wearing a sword and sorcery robe will do the same thing so you will have to remove the robe in order to get to the sleeves and/or tunic. This play style has their own set of skill titles for many skills as well. If you are playing a female character, pressing the button further will convert any 'Barbarian' titles to 'Amazon'. You can open your satchel to learn more about this play style. This option can be turned off and on at any time. You can only have one type of play style active at any one time.<br><br>"
				+ "[barbaric - Turns on/off the barbaric flavor the game provides.<br><br>"

			+ "";

			return HelpText;
		}
	}
}

namespace Server.Gumps
{
    public class InfoHelpGump : Gump
    {
		public int m_Origin;

        public InfoHelpGump( Mobile from, int page, int origin ) : base( 50, 50 )
        {
			m_Origin = origin;

            this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			string title = "";
			string info = "";
			bool scrollbar = true;

			if ( page == 82 )
			{
				scrollbar = false;
				title = "Music Tone";
				info = "This option will simply toggle your music preference to play a different set of music in the dungeons. When turned on, it will play music you normally hear when traveling the land, instead of the music commonly played in dungeons.";
			}
			else if ( page == 83 )
			{
				title = "Music Playlist";
				info = "This gives you a complete list of the in-game music. You can select the music you like and those choices will randomly play as you go from region to region. To listen to a song for review, select the blue gem icon. Note that the client has a delay time when you can start another song so selecting the blue gem may not respond if you started a song too soon before that. Wait for a few seconds and try clicking the blue gem again to see if that song starts to play. Playlists are disabled by default, so if you want your playlist to function, make sure to enable it.";
			}
			else if ( page == 84 )
			{
				scrollbar = false;
				title = "Private Play";
				info = "This option turns on or off the detailed messages of your journey for the town crier and local citizen chatter. This keeps your activities private so others will not see where you are traveling the world.";
			}
			else if ( page == 85 )
			{
				title = "Loot Options";
				info = "This lets you select from a list of categories, where they will automatically take those types of items from common dungeon chests or corpses and put them in your backpack. If you select coins, you will take wealth in the form of currency or gold nuggets. If you take gems and jewels, this will consist of gems, gemstones, jewelry, jewels, and crystals. The unknown items are those that will need identification, but you may decide to take them anyway. The reagent options have a few categories. Magery and necromancer reagents are those used specifically by those characters, where witches brew reagents fall into the necromancer category. Alchemic reagents are those that fall outside the category of magery and necromancer reagents, and only alchemists use them. Herbalist reagents are useful druidic herbalism.";
			}
			else if ( page == 86 )
			{
				title = "Classic Poisoning";
				info = "There are two methods that assassins use to handle poisoned weapons. One is the simple method of soaking the blade and having it poison whenever it strikes their opponent. With this method, known as classic poisoning, there is little control on the dosage given but it is easier to maneuver. When this option is turned off, it has the newer and more tactical method, where only certain weapons can be poisoned and the assassin can control when the poison is administered with the hit. Although the tactical method requires more thought, it does have the potential to allow an assassin to poison certain arrows, for example. The choice of methods can be switched at any time, but only one method can be in use at a given time.";
			}
			else if ( page == 87 )
			{
				title = "Skill Title";
				info = "When you don't set your skill title here, the game will take your highest skill and make that into your character's title. Choosing a skill here will force your title to that profession. So if you always want to be known as a wizard, then select the 'Magery' option (for example). You can let the game manage this at any time by setting it back to 'Auto Title'. Be warned when choosing a skill, if you have zero skill points in it, you will be titled 'the Village Idiot'. If you get at least 0.1, you will at least be 'Aspiring'.";
			}
			else if ( page == 88 )
			{
				scrollbar = false;
				title = "Message Color";
				info = "By default, most of the messages appearing on the lower left of the screen are gray in color. Enabling this option will change those messages to have a random color whenenver a new message appears. This feature can help some more easily see such messages and the varying colors can also help distinguish individual messages that may be scrolling by.";
			}
			else if ( page == 89 )
			{
				scrollbar = false;
				title = "Auto Attack";
				info = "By default, when you are attacked you will automatically attack back. If you want to instead decide when or if you want to attack back, you can turn this option off. This can be helpful if you do not want to kill innocents by accident, or you are trying to tame an angry creature.";
			}
			else if ( page == 92 )
			{
				title = "Play Style - Normal";
				info = "This is the default play style for the " + MySettings.S_ServerName + ". It is designed for a classic fantasy world experience for the players. There are two other play styles available, evil and oriental. Play styles do not change the mechanics of the game playing experience, but it does change the flavor of the treasure you find and the henchman you hire. For example, you can set your play style to an 'evil' style of play. What happens is you will find treasure geared toward that play style. Where you would normally find a blue 'mace of might', the evil style would have you find a black 'mace of ghostly death'. They are simply a way to tweak your character's experience in the game.";
			}
			else if ( page == 93 )
			{
				title = "Play Style - Evil";
				info = "There is an evil element to the game that some want to participate in. With classes such as Necromancers, some players may want to travel a world with this flavor added. This particular setting allows you to toggle between regular and evil flavors. When in the evil mode, some of the treasure you will find will often have a name that fits in the evil style. When you stay within negative karma, skill titles will change for you as well, but not all. Look over the book of skill titles (found within the game world) to see which titles will change based on karma. Some of the relics you will find may also have this style, to perhaps decorate a home in this fashion. This option can be turned off and on at any time. You can only have one type of play style active at any one time.<br><br>"
				+ "[evil - Turns on/off the evil flavor the game provides.";
			}
			else if ( page == 94 )
			{
				title = "Play Style - Oriental";
				info = "There is an oriental element to the game that most do not want to participate in. With classes such as Ninja and Samurai, some players may want to travel a world with this flavor added. This particular setting allows you to toggle between fantasy and oriental. When in the oriental mode, much of the treasure you will find will be of Chinese or Japanese historical origins. These types of items will most times be named to match the style. Items that once belonged to someone, will often have a name that fits in the oriental style. Some of the skill titles will change for you as well, but not all. Look over the book of skill titles (found within the game world) to see which titles will change based on this play style. Some of the relics and artwork you will find will also have this style, to perhaps decorate a home in this fashion. This option can be turned off and on at any time. You can only have one type of play style active at any one time.";
			}
			else if ( page == 95 )
			{
				m_Origin = 7;
				title = "Magic Toolbars";
				info = "These toolbars can be configured for all areas of magical-style spells in the game. Each school of magic has two separate toolbars you can customize, except for magery which has four available. The large number of spells for magery benefit from the extra two toolbars. These toolbars allow you to select spells that you like to cast often, and set whether the bar will appear vertical or horizontal. If you choose to have the toolbar appear vertical, you have the additional option of showing the spell names next to the icons. These toolbars can be moved around and you need only single click the appropriate icon to cast the spell. If you have spells selected for a toolbar, but lack the spell in your spellbook, the icon will not appear when you open the toolbar. These toolbars cannot be closed by normal means, to avoid the chance you close them by accident when in combat. You can either use the command button available in the 'Help' section, or the appropriate typed keyboard command.";
			}
			else if ( page == 96 )
			{
				scrollbar = false;
				title = "Magery Spell Color";
				info = "You can change the color for all of your magery spell effects here. There are a limited amount of choices given here. Once set, your spells will be that color for every effect. If you want to set it back to normal, then select the 'Default' option. You can also use the '[spellhue' command followed by a number of any color you want to set it to.";
			}
			else if ( page == 97 )
			{
				scrollbar = false;
				title = "Custom Title";
				info = "This allows you to enter a custom title for your character, instead of relying on the game to assign you one based on your best skill or the skill you choose to have represent you. To clear out a custom title you may have set with this option, enter the word of 'clear' to remove it.";
			}
			else if ( page == 99 )
			{
				scrollbar = false;
				title = "Weapon Ability Names";
				info = "When you get good enough with tactics and a weapon type, you will get special abilities that they can perform. These usually appear as simple icons you can select to do the action, but this option will turn on or off the special weapon ability names next to the appropriate icons.";
			}
			else if ( page == 100 )
			{
				scrollbar = false;
				title = "Auto Sheath";
				info = "This option turns on or off the feature to sheathe your weapon when not in battle. When you put your character back into war mode, they will draw the weapon.";
			}
			else if ( page == 101 )
			{
				scrollbar = false;
				title = "Gump Images";
				info = "Many window gumps have a faded image in the background. Turning this off will have those windows only be black in color, with no background image.";
			}
			else if ( page == 102 )
			{
				scrollbar = false;
				title = "Weapon Ability Bar";
				info = "This option turns on or off the auto-opening of the weapon ability icon bar, meaning you will have to do it manually if you turn it off.";
			}
			else if ( page == 103 )
			{
				scrollbar = false;
				title = "Creature Magic";
				info = "Some creatures have a natural ability for magic. This setting lets you change which school of magic you want to focus on: magery, necromancy, or elementalism. This allows magery or necromancy creatures to move their focus into elementalism, or to switch between magery and necromancy.";
			}
			else if ( page == 104 )
			{
				scrollbar = false;
				title = "Creature Type";
				info = "Some creature species has more than one option for appearance. This setting lets you change to another of that species if another appearance is available. You can also turn yourself into a human if you choose. If you become human, you will remain that way forever.";
			}
			else if ( page == 105 )
			{
				scrollbar = false;
				title = "Creature Sounds";
				info = "Since you are a creature, you sometimes make sounds when attacking or getting hurt from attacks. You can turn these sounds on or off here.";
			}
			else if ( page == 106 )
			{
				scrollbar = false;
				title = "Ancient Spellbook";
				info = "If you begin researching the 64 ancient spells that were long forgotten, enabling this setting means you will be casting such magic from a book instead of using your research bag. If you have this enabled, you will need reagents to cast spells and the spells being cast must be in your book. Disabling this checks your research bag to see if you have the spell prepared ahead of time.";
			}
			else if ( page == 107 )
			{
				scrollbar = false;
				title = "Set Crafting Container";
				info = "This allows you to set a container, where items will go when you are creating them through crafting. The container must be in your main pack in order to collect the items, and not within another container.";
			}
			else if ( page == 108 )
			{
				scrollbar = false;
				title = "Set Harvesting Container";
				info = "This allows you to set a container, where items will go when you are harvesting for items. These are items you get from activities like mining, lumberjacking, and fishing. The container must be in your main pack in order to collect the items, and not within another container.";
			}
			else if ( page == 109 )
			{
				scrollbar = false;
				title = "Set Loot Container";
				info = "This allows you to set a container, where items will go that you configured in the Loot Options setting. The container must be in your main pack in order to collect the items, and not within another container.";
			}
			else if ( page == 110 )
			{
				scrollbar = false;
				title = "Ordinary Resources";
				info = "Turning this setting on will have your character only harvest or gather ordinary resources like wood, leather, granite, iron and bones. This means you will not be collecting higher resourced items when skinning, mining, or lumberjacking.";
			}
			else if ( page == 111 )
			{
				scrollbar = false;
				title = "Double Click to ID Items";
				info = "Enabling this will allow your character to try and identify items by double clicking them.<BR><BR>NOTE: if you are using any third party software, that tries to open all of your containers, then that third party software will try to identify these items without your consent.";
			}
			else if ( page == 198 )
			{
				title = "Play Style - Barbaric";
				info = "The default game does not lend itself to a sword and sorcery experience. This means that it is not the most optimal play experience to be a loin cloth wearing barbarian that roams the land with a huge axe. Characters generally get as much equipment as they can in order to maximize their rate of survivability. This particular play style can help in this regard. Choosing to play in this style will have a satchel appear in your main pack. You cannot store anything in this satchel, as its purpose is to change certain pieces of equipment you place into it. It will change shields, hats, helms, tunics, sleeves, leggings, boots, gorgets, gloves, necklaces, cloaks, and robes. When these items get changed, they will become something that appears differently but behave in the same way the previous item did. These different items can be equipped but may not appear on your character. Also note that when you wear robes, they cover your character's tunics and sleeves. Wearing a sword and sorcery robe will do the same thing so you will have to remove the robe in order to get to the sleeves and/or tunic. This play style has their own set of skill titles for many skills as well. If you are playing a female character, pressing the button further will convert any 'Barbarian' titles to 'Amazon'. You can open your satchel to learn more about this play style. This option can be turned off and on at any time. You can only have one type of play style active at any one time.";
			}
			else if ( page == 199 )
			{
				title = "Skill Lists";
				info = "Skill lists are an alternative to the normal skill lists you can get from clicking the appropriate button on the paper doll. Although you still need to use that for skill management (up, down, lock), skill lists have a more condensed appearance for when you play the game. In order for skills to appear in this alternate list, they have to either be set to 'up', or they can be set to 'locked'. The 'locked' skills will only display in this list if you change your settings here to reflect that. The list does not refresh in real time, but it will often refresh itself to show your skill status in both real and enhanced values. Any skill that appears in orange indicates a skill that you have locked. You can open this list with the '[skilllist' command, or the appropriate button on the main screen.";
			}
			else if ( page == 1000 )
			{
				title = "Flip Deed";
				info = "This option allows you to flip some deeds that can come in one of two direction facings. So if a deed states that furniture faces east, then you can set the deed on the floor of your house and flip it to face south instead. This can flip almost any deed-like items in this manner, but not all items are called 'deeds' or look like deeds. Some items behave as deeds and those can be flipped in the same manner. Tents or bear rugs, for example, have a facing and you can flip those with this command..";
			}
			//

			AddPage(0);

			string color = "#ddbc4b";

			AddImage(0, 0, 9577, Server.Misc.PlayerSettings.GetGumpHue( from ));
			AddHtml( 12, 12, 239, 20, @"<BODY><BASEFONT Color=" + color + ">" + title + "</BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 12, 43, 278, 212, @"<BODY><BASEFONT Color=" + color + ">" + info + "</BASEFONT></BODY>", (bool)false, (bool)scrollbar);
			AddButton(268, 9, 4017, 4017, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
			from.SendSound( 0x4A );
			from.CloseGump( typeof( Server.Engines.Help.HelpGump ) );
			if ( m_Origin != 999 ){ from.SendGump( new Server.Engines.Help.HelpGump( from, m_Origin ) ); }
        }
    }
}