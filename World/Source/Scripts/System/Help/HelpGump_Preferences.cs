using Server.Gumps;
using Server.Mobiles;

namespace Server.Engines.Help
{
	public partial class HelpGump
	{
		private void AddPreferencesPanel(PlayerMobile from, int pageNumber)
		{
			const int PAGE_ICON = 4011;

			const int SECTION_START_X = 225;
			const int SETTING_START_X = SECTION_START_X + 20;
			const int SETTING_SECTION_WIDTH = 725;
			const int ROW_HEIGHT = 30;

			const int MAX_COLUMNS_PER_ROW = 4;
			const int COLUMN_WIDTH = 125;
			const int MAX_WIDTH = COLUMN_WIDTH * MAX_COLUMNS_PER_ROW;

			const int EXCESS_PADDING_PER_COLUMN = (int)( (double)( SETTING_SECTION_WIDTH - MAX_WIDTH ) / MAX_COLUMNS_PER_ROW );

			const int FIRST_COL_X = SETTING_START_X;
			const int SECOND_COL_X = FIRST_COL_X + COLUMN_WIDTH + EXCESS_PADDING_PER_COLUMN;
			const int THIRD_COL_X = SECOND_COL_X + COLUMN_WIDTH + EXCESS_PADDING_PER_COLUMN;
			const int FOURTH_COL_X = THIRD_COL_X + COLUMN_WIDTH + EXCESS_PADDING_PER_COLUMN;

			bool isEvenRow = false;
			int xs = SETTING_START_X;
			int rowY = 40;

			AddGeneralActionRowHeader(SECTION_START_X, rowY, "Personal Preferences");
			rowY += ROW_HEIGHT;

			AddSetting(xs, rowY, from, "Auto Sheath", PageActionType.Setting_AutoSheath, PageActionType.Setting_AutoSheath_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			{
				var enabled = from.RaceID > 0 && (
					(from.Region).Name == "the Tavern" ||
					( from.Map == Map.Sosaria && from.X >= 6982 && from.Y >= 694 && from.X <= 6999 && from.Y <= 713 )
				);
				AddSetting(xs, rowY, from, "Creature Type", PageActionType.Setting_CreatureType, PageActionType.Setting_CreatureType_Info, enabled);
				if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }
			}

			AddSetting(xs, rowY, from, "Double Click Talk", PageActionType.Setting_DoubleClickToTalk, PageActionType.Setting_DoubleClickToTalk_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Gump Images", PageActionType.Setting_GumpImages, PageActionType.Setting_GumpImages_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Message Colors", PageActionType.Setting_MessageColors, PageActionType.Setting_MessageColors_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Modernized Skill Configuration", PageActionType.Show_ModernSkills, PageActionType.Show_ModernSkills_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Music Playlist", PageActionType.Setting_MusicPlaylist, PageActionType.Setting_MusicPlaylist_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Music Tone", PageActionType.Setting_MusicTone, PageActionType.Setting_MusicTone_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Private Play", PageActionType.Setting_PrivatePlay, PageActionType.Setting_PrivatePlay_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Skill Title", PageActionType.Setting_SkillTitle, PageActionType.Setting_SkillTitle_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			string skillLocks = "Skill List (Show Up)"; 
			if ( from.SkillDisplay == 1 ){ skillLocks = "Skill List (Show Up and Locked)"; }
			AddSetting(xs, rowY, from, skillLocks, PageActionType.Setting_SkillList, PageActionType.Setting_SkillList_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Weapon Ability Names", PageActionType.Setting_WeaponAbilityNames, PageActionType.Setting_WeaponAbilityNames_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }


			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; }
			rowY += (int)(0.5 * ROW_HEIGHT);
			AddGeneralActionRowHeader(SECTION_START_X, rowY, "Magery Spell Color");
			AddButton(SECTION_START_X + 130, rowY, PAGE_ICON, PAGE_ICON, (int)PageActionType.Setting_MagerySpellColor_Info, GumpButtonType.Reply, 0);
			rowY += ROW_HEIGHT;

			xs = FIRST_COL_X;
			AddAction(xs, rowY, from, "Default", PageActionType.Setting_MagerySpellColor_Default, COLUMN_WIDTH);
			xs = SECOND_COL_X;
			AddAction(xs, rowY, from, "Black", PageActionType.Setting_MagerySpellColor_Black, COLUMN_WIDTH);
			xs = THIRD_COL_X;
			AddAction(xs, rowY, from, "Blue", PageActionType.Setting_MagerySpellColor_Blue, COLUMN_WIDTH);
			xs = FOURTH_COL_X;
			AddAction(xs, rowY, from, "Green", PageActionType.Setting_MagerySpellColor_Green, COLUMN_WIDTH);
			rowY += ROW_HEIGHT;

			xs = FIRST_COL_X;
			AddAction(xs, rowY, from, "Purple", PageActionType.Setting_MagerySpellColor_Purple, COLUMN_WIDTH);
			xs = SECOND_COL_X;
			AddAction(xs, rowY, from, "Red", PageActionType.Setting_MagerySpellColor_Red, COLUMN_WIDTH);
			xs = THIRD_COL_X;
			AddAction(xs, rowY, from, "White", PageActionType.Setting_MagerySpellColor_White, COLUMN_WIDTH);
			xs = FOURTH_COL_X;
			AddAction(xs, rowY, from, "Yellow", PageActionType.Setting_MagerySpellColor_Yellow, COLUMN_WIDTH);
			rowY += ROW_HEIGHT;
		}
	}
}