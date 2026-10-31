using Server.Mobiles;

namespace Server.Engines.Help
{
	public partial class HelpGump
	{
		private void AddSettingsPanel(PlayerMobile from, int pageNumber)
		{
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

			AddGeneralActionRowHeader(SECTION_START_X, rowY, "Mechanic Altering Settings");
			rowY += ROW_HEIGHT;
			AddSetting(xs, rowY, from, "Auto Attack", PageActionType.Setting_AutoAttack, PageActionType.Setting_AutoAttack_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Classic Poisoning", PageActionType.Setting_ClassicPoisoning, PageActionType.Setting_ClassicPoisoning_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Colorless Fabric Breakdown", PageActionType.Setting_ColorlessFabricBreakdown, PageActionType.Setting_ColorlessFabricBreakdown_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }
			
			{
				string magic = "Default";
				if ( from.RaceMagicSchool == 1 ){ magic = "Magery"; }
				else if ( from.RaceMagicSchool == 2 ){ magic = "Necromancy"; }
				else if ( from.RaceMagicSchool == 3 ){ magic = "Elementalism"; }

				var enabled = from.RaceID > 0 && Server.Items.BaseRace.GetMonsterMage( from.RaceID ) && from.Region.Name == "the Tavern";
				AddSetting(xs, rowY, from, "Creature Magic (" + magic + ")", PageActionType.Setting_CreatureMagicFocus, PageActionType.Setting_CreatureMagicFocus_Info, enabled);
				if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }
			}

			AddSetting(xs, rowY, from, "Double Click to ID Items", PageActionType.Setting_DoubleClickToIDItems, PageActionType.Setting_DoubleClickToIDItems_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Legacy Carve", PageActionType.Setting_LegacyCarve, PageActionType.Setting_LegacyCarve_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Ordinary Resources", PageActionType.Setting_OrdinaryResources, PageActionType.Setting_OrdinaryResources_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Remove Vendor Gold Safeguard", PageActionType.Setting_RemoveVendorGoldSafeguard, PageActionType.Setting_RemoveVendorGoldSafeguard_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Single ID Attempt", PageActionType.Setting_SingleAttemptID, PageActionType.Setting_SingleAttemptID_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Use Ancient Spellbook", PageActionType.Setting_UseAncientSpellbook, PageActionType.Setting_UseAncientSpellbook_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }


			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; }
			rowY += (int)(0.5 * ROW_HEIGHT);
			AddGeneralActionRowHeader(SECTION_START_X, rowY, "MobileUO Settings");
			rowY += ROW_HEIGHT;

			AddSetting(xs, rowY, from, "Suppress Vendor Tooltips", PageActionType.Setting_SuppressVendorTooltips, PageActionType.Setting_SuppressVendorTooltips_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			AddSetting(xs, rowY, from, "Suppress System Messages", PageActionType.Setting_SuppressSystemMessages, PageActionType.Setting_SuppressSystemMessages_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }


			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; }
			rowY += (int)(0.5 * ROW_HEIGHT);
			AddGeneralActionRowHeader(SECTION_START_X, rowY, "Play Style");
			rowY += ROW_HEIGHT;

			xs = FIRST_COL_X;
			AddSetting(xs, rowY, from, "Normal", PageActionType.Setting_Playstyle_Normal, PageActionType.Setting_Playstyle_Normal_Info);
			xs += COLUMN_WIDTH + EXCESS_PADDING_PER_COLUMN;

			xs = SECOND_COL_X;
			AddSetting(xs, rowY, from, "Evil", PageActionType.Setting_Playstyle_Evil, PageActionType.Setting_Playstyle_Evil_Info);
			xs += COLUMN_WIDTH + EXCESS_PADDING_PER_COLUMN;

			xs = THIRD_COL_X;
			AddSetting(xs, rowY, from, "Oriental", PageActionType.Setting_Playstyle_Oriental, PageActionType.Setting_Playstyle_Oriental_Info);
			xs += COLUMN_WIDTH + EXCESS_PADDING_PER_COLUMN;

			xs = FOURTH_COL_X;
			string barbaricStyle = !from.Female ? "Barbaric" : "Barbaric (Amazon)";
			AddSetting(xs, rowY, from, barbaricStyle, PageActionType.Setting_Playstyle_Barbaric, PageActionType.Setting_Playstyle_Barbaric_Info);
			if ( isEvenRow ){ rowY += ROW_HEIGHT; isEvenRow=false; xs=FIRST_COL_X; } else { isEvenRow=true; xs=THIRD_COL_X; }

			rowY += (int)(0.5 * ROW_HEIGHT);
			AddGeneralActionRowHeader(SECTION_START_X, rowY, "Language");
			rowY += ROW_HEIGHT;

			xs = FIRST_COL_X;
			AddAction( xs, rowY, from, "English", PageActionType.Setting_Language_English, 130 );
			xs += COLUMN_WIDTH + EXCESS_PADDING_PER_COLUMN;

			xs = SECOND_COL_X;
			AddAction( xs, rowY, from, "简体中文 (zh-Hans)", PageActionType.Setting_Language_ZhHans, 240 );
		}
	}
}