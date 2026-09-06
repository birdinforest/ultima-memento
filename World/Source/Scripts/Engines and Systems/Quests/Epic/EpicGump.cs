using System;
using System.Globalization;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps
{
	public class EpicGump : Gump
	{
		private static string SlugifyNpc( string name )
		{
			if ( string.IsNullOrEmpty( name ) )
				return "";

			return name.Trim().ToLowerInvariant().Replace( ' ', '_' ).Replace( "'", "" ).Replace( ".", "" );
		}

		// Item/dungeon names are the same 59 fixed strings shared with SummonCarriers.cs /
		// SummonPrison.cs (Magical Prison), which already carry curated hash-key translations in
		// scripts-quests.json (item names) and placemap-labels.json (dungeon/region names, already
		// annotated as "中文（English）"). Resolve via the hash-based StringCatalog.TryResolve
		// against the literal -- NOT QuestTome.LocalizedQuestItemName / LocalizedDungeon, which are
		// built for QuestTome's procedurally-*composed* relic/dungeon names and fall back to
		// RandomThings.GetChineseFantasyName() for unrecognized tokens (garbled nonsense for these
		// fixed strings instead of the real, already-reviewed translation). See matching helpers in
		// EpicCharacter.cs.
		private static string FormatItemRequirement( Mobile viewer, string rawItem )
		{
			if ( string.IsNullOrEmpty( rawItem ) || rawItem == "NEW" )
				return rawItem;

			string lang = AccountLang.GetLanguageCode( viewer.Account );

			if ( AccountLang.IsChinese( lang ) )
			{
				string zh = StringCatalog.TryResolve( lang, rawItem );

				if ( !string.IsNullOrEmpty( zh ) && zh != rawItem )
					return zh + "（" + rawItem + "）";

				return rawItem;
			}

			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase( rawItem );
		}

		private static string FormatDungeonRequirement( Mobile viewer, string rawItem )
		{
			string region = EpicTributeChallenge.GetChallengeRegionName( rawItem );

			if ( string.IsNullOrEmpty( region ) )
				return "";

			string lang = AccountLang.GetLanguageCode( viewer.Account );
			string zh = StringCatalog.TryResolve( lang, region );

			// placemap-labels.json's zh values already come pre-annotated as "中文（English）" --
			// do not append the English literal a second time.
			return !string.IsNullOrEmpty( zh ) ? zh : region;
		}

		// "Lord British" -> "不列颠王（Lord British）" for zh accounts, via the existing
		// quest.tome.noun.epic.name.* shotkey table (QuestTome.LocalizedEpicNpc).
		private static string FormatGiverName( Mobile viewer, string giverName )
		{
			if ( string.IsNullOrEmpty( giverName ) )
				return giverName;

			if ( AccountLang.IsChinese( AccountLang.GetLanguageCode( viewer.Account ) ) )
				return QuestTome.LocalizedEpicNpc( "zh-Hans", giverName );

			return giverName;
		}

		private static string ResolveBare( Mobile listener, string alignment, string myName, string thisItem, string dungeon )
		{
			string key = "quest.epic.gump.shared.bare.neutral";

			if ( alignment == "good" )
				key = "quest.epic.gump.shared.bare.good";
			else if ( alignment == "evil" )
				key = "quest.epic.gump.shared.bare.evil";

			return StringCatalog.ResolveFormatByKey( listener.Account, key, myName, thisItem, dungeon );
		}

		public EpicGump( Mobile talker, Mobile listener, bool allowed, string alignment ) : base( 25, 25 )
		{
			string myName = talker.Name;
			string yourName = listener.Name;
			string rawItem = EpicCharacter.GetSpecialItemRequirement( listener );
			string thisItem = FormatItemRequirement( listener, rawItem );
			string dungeon = FormatDungeonRequirement( listener, rawItem );

			string sInfo = StringCatalog.ResolveByKey( listener.Account, "quest.epic.gump.shared.info" );
			string sBare = ResolveBare( listener, alignment, FormatGiverName( listener, myName ), thisItem, dungeon );
			string sLore = StringCatalog.ResolveByKey( listener.Account, "quest.epic.gump.shared.lore" );

			string slug = SlugifyNpc( myName );
			string textKey = allowed ? "quest.epic.gump." + slug + ".text.allowed" : "quest.epic.gump." + slug + ".text.denied";

			string sTitle = StringCatalog.ResolveByKey( listener.Account, "quest.epic.gump." + slug + ".title" );
			string sBody = StringCatalog.ResolveFormatByKey( listener.Account, textKey, yourName );
			string sText = allowed ? sBody + sInfo + sBare + sLore : sBody + sBare + sLore;

			this.Closable = true;
			this.Disposable = true;
			this.Dragable = true;
			this.Resizable = false;

			AddPage( 0 );

			string color = "#d5a496";

			AddImage( 0, 2, 9543, Server.Misc.PlayerSettings.GetGumpHue( listener ) );
			AddHtml( 12, 15, 341, 20, @"<BODY><BASEFONT Color=" + color + ">" + sTitle + "</BASEFONT></BODY>", (bool)false, (bool)false );
			AddHtml( 12, 50, 380, 253, @"<BODY><BASEFONT Color=" + color + ">" + sText + "</BASEFONT></BODY>", (bool)false, (bool)true );
			AddButton( 367, 12, 4017, 4017, 0, GumpButtonType.Reply, 0 );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;
			from.SendSound( 0x4A );
		}
	}
}
