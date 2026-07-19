#!/usr/bin/env python3
"""Generate SomeRandomNote.cs from some-random-note-arg-spec.json."""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[2]
SPEC_PATH = ROOT / "Data/Localization/tools-output/some-random-note-arg-spec.json"
OUT_PATH = (
    ROOT
    / "Source/Scripts/Engines and Systems/Quests/SomeRandomNote.cs"
)

spec = json.loads(SPEC_PATH.read_text(encoding="utf-8"))


def city_expr(key: str) -> str:
    if key in ("lie.31", "lie.32", "lie.40", "lie.45", "lie.46", "truth.45"):
        return "RandomThings.MadeUpCity()"
    return "RandomThings.GetRandomCity()"


def place_expr(key: str) -> str:
    if key == "lie.29":
        return 'QuestCharacters.SomePlace( "tavern" )'
    mapping = {
        "lie.02": 'QuestCharacters.SomePlace( "random" )',
        "lie.05": 'QuestCharacters.SomePlace( "random" )',
        "lie.07": 'QuestCharacters.SomePlace( "random" )',
        "lie.08": 'QuestCharacters.SomePlace( "random" )',
        "lie.10": 'QuestCharacters.SomePlace( "random" )',
        "lie.18": 'UppercaseFirst( QuestCharacters.SomePlace( "parchment" ) )',
        "lie.28": 'QuestCharacters.SomePlace( "tavern" )',
        "lie.44": 'Server.Misc.QuestCharacters.SomePlace( "tablet" )',
        "truth.08": 'QuestCharacters.SomePlace( "random" )',
        "truth.28": 'QuestCharacters.SomePlace( "tavern" )',
        "truth.29": 'QuestCharacters.SomePlace( "tavern" )',
        "truth.30": 'QuestCharacters.SomePlace( "tavern" )',
        "truth.44": 'Server.Misc.QuestCharacters.SomePlace( "tablet" )',
    }
    return mapping[key]


def amount_expr(key: str) -> str:
    return {
        "lie.24": "Utility.RandomMinMax( 5, 200 ).ToString()",
        "lie.41": "( Utility.RandomMinMax( 9, 20 ) * 10 ).ToString()",
        "lie.46": "( Utility.RandomMinMax( 10, 49 ) * 10 ).ToString()",
        "truth.24": "Utility.RandomMinMax( 5, 200 ).ToString()",
    }[key]


LABEL_EXPR = {
    "name": "QuestCharacters.RandomWords()",
    "writer": "QuestCharacters.ParchmentWriter()",
    "giver": "QuestCharacters.QuestGiver()",
    "questItem": "QuestCharacters.QuestItems( true )",
    "relicItem": "GetSpecialItem( relic, 1 )",
    "relicPlace": "GetSpecialItem( relic, 0 )",
    "job": "RandomThings.GetRandomJob()",
    "race": "Server.Misc.RandomThings.GetRandomIntelligentRace()",
    "orkName": 'NameList.RandomName( "ork_male" )',
    "society": "RandomThings.GetRandomSociety()",
    "magicalItem": "RandomThings.RandomMagicalItem()",
    "days": "Utility.RandomMinMax( 2, 8 ).ToString()",
    "years": "Utility.RandomMinMax( 2, 8 ).ToString()",
    "ship": 'RandomThings.GetRandomShipName( "", 0 )',
    "gem": 'RandomThings.GetRandomGemType( "dragyns" )',
    "poison": "PoisonVocabKey( poison )",
    "skull": "SkullVocabKey( skull )",
    "thing": "ThingVocabKey( thing )",
    "researcher": "ResearcherVocabKey( researcher )",
    "dungeon": "RandomThings.MadeUpDungeon()",
}


def emit_case(side: str, num: int):
    key = f"{side}.{num:02d}"
    args = spec[key]
    lines = []
    needs = set(args)
    if "thing" in needs:
        lines.extend(
            [
                '\t\t\t\t\t\tstring thing = "book";',
                "\t\t\t\t\t\tswitch ( Utility.RandomMinMax( 0, 3 ) )",
                "\t\t\t\t\t\t{",
                '\t\t\t\t\t\t\tcase 0: thing = "scroll"; break;',
                '\t\t\t\t\t\t\tcase 1: thing = "book"; break;',
                '\t\t\t\t\t\t\tcase 2: thing = "parchment"; break;',
                '\t\t\t\t\t\t\tcase 3: thing = "tapestry"; break;',
                "\t\t\t\t\t\t}",
            ]
        )
    if "researcher" in needs:
        lines.extend(
            [
                '\t\t\t\t\t\tstring researcher = "sage";',
                "\t\t\t\t\t\tswitch ( Utility.RandomMinMax( 0, 2 ) )",
                "\t\t\t\t\t\t{",
                '\t\t\t\t\t\t\tcase 0: researcher = "sage"; break;',
                '\t\t\t\t\t\t\tcase 1: researcher = "scribe"; break;',
                '\t\t\t\t\t\t\tcase 2: researcher = "librarian"; break;',
                "\t\t\t\t\t\t}",
            ]
        )

    arg_exprs = []
    for a in args:
        if a == "city":
            arg_exprs.append(city_expr(key))
        elif a == "place":
            arg_exprs.append(place_expr(key))
        elif a == "amount":
            arg_exprs.append(amount_expr(key))
        else:
            arg_exprs.append(LABEL_EXPR[a])

    commit = f"CommitNote( {num}"
    if arg_exprs:
        commit += ", " + ", ".join(arg_exprs)
    commit += " );"
    return lines, commit


def build_switch(side: str) -> str:
    out = []
    for num in range(1, 50):
        if side == "truth" and num == 15:
            out.extend(
                [
                    "\t\t\t\t\tcase 15:",
                    "\t\t\t\t\t\tif ( Utility.RandomMinMax( 1, 2 ) == 1 )",
                    "\t\t\t\t\t\t\tCommitNote( 15, 1, RandomThings.GetRandomJob(), RandomThings.GetRandomCity() );",
                    "\t\t\t\t\t\telse",
                    "\t\t\t\t\t\t\tCommitNote( 15, RandomThings.GetRandomJob(), RandomThings.GetRandomCity() );",
                    "\t\t\t\t\t\tbreak;",
                ]
            )
            continue
        if side == "truth" and num == 42:
            out.extend(
                [
                    "\t\t\t\t\tcase 42:",
                    "\t\t\t\t\t\tif ( Utility.Random( 2 ) == 1 )",
                    "\t\t\t\t\t\t\tCommitNote( 42, 1, QuestCharacters.ParchmentWriter(), RandomThings.GetRandomCity(), QuestCharacters.QuestGiver() );",
                    "\t\t\t\t\t\telse",
                    "\t\t\t\t\t\t\tCommitNote( 42, QuestCharacters.ParchmentWriter(), RandomThings.GetRandomCity(), QuestCharacters.QuestGiver() );",
                    "\t\t\t\t\t\tbreak;",
                ]
            )
            continue

        lines, commit = emit_case(side, num)
        out.append(f"\t\t\t\t\tcase {num}:")
        out.extend(lines)
        out.append("\t\t\t\t\t\t" + commit)
        out.append("\t\t\t\t\t\tbreak;")
    return "\n".join(out)


HEADER = r"""using System;
using Server;
using Server.Misc;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;
using Server.Accounting;
using Server.Localization;

namespace Server.Items
{
	public class SomeRandomNote : Item
	{
		private bool m_Structured;
		private int m_TemplateId;
		private int m_Variant;
		private string[] m_Args;

		private static string ResolveText( Mobile from, string text )
		{
			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		public override Catalogs DefaultCatalog{ get{ return Catalogs.Scroll; } }

		public string ScrollMessage;
		public int ScrollTrue;

		[CommandProperty(AccessLevel.Owner)]
		public string Scroll_Message { get { return ScrollMessage; } set { ScrollMessage = value; m_Structured = false; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.Owner)]
		public int Scroll_True { get { return ScrollTrue; } set { ScrollTrue = value; InvalidateProperties(); } }

		private static string PoisonVocabKey( string poison )
		{
			if ( poison == null ) poison = "lethal";
			return "quest.note.vocab.poison." + poison;
		}

		private static string SkullVocabKey( string skull )
		{
			if ( skull == null ) skull = "lich";
			return "quest.note.vocab.skull." + skull.Replace( " ", "_" );
		}

		private static string ThingVocabKey( string thing )
		{
			if ( thing == null ) thing = "book";
			return "quest.note.vocab.thing." + thing;
		}

		private static string ResearcherVocabKey( string researcher )
		{
			if ( researcher == null ) researcher = "sage";
			return "quest.note.vocab.researcher." + researcher;
		}

		private static string ResolveArg( IAccount acct, string arg )
		{
			if ( arg == null )
				return "";
			if ( arg.StartsWith( "quest.note.vocab." ) )
				return StringCatalog.ResolveByKey( acct, arg );
			return arg;
		}

		private static string NoteKey( int templateId, int variant, int scrollTrue )
		{
			string side = scrollTrue == 1 ? "truth" : "lie";
			string key = "quest.note." + side + "." + templateId.ToString( "D2" );
			if ( variant > 0 )
				key = key + "." + variant.ToString();
			return key;
		}

		private static object[] ResolveArgsForDisplay( Mobile from, IAccount acct, string[] args )
		{
			if ( args == null || args.Length == 0 )
				return new object[0];

			bool zh = from != null && AccountLang.IsChinese( AccountLang.GetLanguageCode( acct ) );
			object[] resolved = new object[args.Length];

			for ( int i = 0; i < args.Length; ++i )
			{
				string a = ResolveArg( acct, args[i] );

				// Translate English place/item fragments in args only.
				// Do not run composite on the finished Chinese template — that rewrites
				// proper-noun annotations like 冥界深渊（Underworld） into doubled Chinese.
				if ( zh && a != null && a.Length > 0 )
					a = QuestCompositeResolver.ResolveComposite( from, a );

				resolved[i] = a;
			}

			return resolved;
		}

		private static string FormatNote( Mobile from, IAccount acct, int templateId, int variant, int scrollTrue, string[] args )
		{
			if ( templateId < 1 )
				return "";

			string key = NoteKey( templateId, variant, scrollTrue );
			object[] resolved = ResolveArgsForDisplay( from, acct, args );
			return StringCatalog.ResolveFormatByKey( acct, key, resolved );
		}

		private void CommitNote( int templateId, params string[] args )
		{
			CommitNote( templateId, 0, args );
		}

		private void CommitNote( int templateId, int variant, params string[] args )
		{
			m_Structured = true;
			m_TemplateId = templateId;
			m_Variant = variant;
			m_Args = args ?? new string[0];
			// English fallback for GM props / tools (no viewer).
			ScrollMessage = FormatNote( null, null, templateId, variant, ScrollTrue, m_Args );
		}

		public string BuildDisplayText( Mobile from )
		{
			if ( m_Structured && m_TemplateId > 0 )
			{
				IAccount acct = from != null ? from.Account : null;
				return FormatNote( from, acct, m_TemplateId, m_Variant, ScrollTrue, m_Args );
			}

			// Legacy saves: keep English body. Fragment composite on English sentences produces broken ZH/EN mash.
			return ScrollMessage ?? "";
		}

		[Constructable]
		public SomeRandomNote( ) : base( 0x4CCA )
		{
			Weight = 1.0;
			Name = "an old parchment";
			ItemID = Utility.RandomList( 0x4CCA, 0x4CCB );

			switch ( Utility.RandomMinMax( 0, 2 ) )
			{
				case 0:	Name = "parchment";	break;
				case 1:	Name = "note";		break;
				case 2:	Name = "scroll";		break;
			}

			switch ( Utility.RandomMinMax( 0, 5 ) )
			{
				case 0:	Name = "an old" + " " + Name;		break;
				case 1:	Name = "an ancient" + " " + Name;	break;
				case 2:	Name = "a worn" + " " + Name;		break;
				case 3:	Name = "a scribbled" + " " + Name;	break;
				case 4:	Name = "an unusual" + " " + Name;	break;
				case 5:	Name = "a strange" + " " + Name;	break;
			}

			string poison = "lethal";
			switch ( Utility.RandomMinMax( 0, 4 ) )
			{
				case 0:	poison = "lesser"; break;
				case 1:	poison = "regular"; break;
				case 2:	poison = "greater"; break;
				case 3:	poison = "deadly"; break;
				case 4:	poison = "lethal"; break;
			}

			string skull = "lich";

			switch ( Utility.RandomMinMax( 0, 6 ) )
			{
				case 0: skull = "lich";				break;
				case 1: skull = "lich lord";		break;
				case 2: skull = "ancient lich";		break;
				case 3: skull = "demilich";			break;
				case 4: skull = "bone magi";		break;
				case 5: skull = "skeletal mage";	break;
				case 6: skull = "skeletal wizard";	break;
			}

			ItemID = Utility.RandomList( 0xE34, 0x14ED, 0x14EE, 0x14EF, 0x14F0 );

			ScrollTrue = 1;
			string written = "truth";
			if ( 1 == Utility.RandomMinMax( 0, 1 ) ){ written = "lies"; ScrollTrue = 0; }

			int amnt = Utility.RandomMinMax( 1, 49 );
			int relic = Utility.RandomMinMax( 1, 59 );

			m_Structured = false;
			m_TemplateId = 0;
			m_Variant = 0;
			m_Args = new string[0];

			if ( written == "lies" )
			{
				switch ( amnt )
				{
"""

# Fix accidental python comment in C# header
HEADER = HEADER.replace(
    "\t\t\t# Legacy saves: keep English body. Fragment composite on English sentences produces broken ZH/EN mash.",
    "\t\t\t// Legacy saves: keep English body. Fragment composite on English sentences produces broken ZH/EN mash.",
)

FOOTER_MID = """
				}
			}
			else
			{
				switch ( amnt )
				{
"""

FOOTER_END = r"""
				}
			}
		}

		public class ClueGump : Gump
		{
			public ClueGump( Mobile from, Item parchment ): base( 100, 100 )
			{
				SomeRandomNote scroll = (SomeRandomNote)parchment;
				string sText = scroll.BuildDisplayText( from );
				from.PlaySound( 0x249 );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 10901, 2786);
				AddImage(0, 0, 10899, 2117);
				AddHtml( 45, 78, 386, 218, @"<BODY><BASEFONT Color=#d9c781>" + sText + "</BASEFONT></BODY>", (bool)false, (bool)true);
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state.Mobile;
				from.PlaySound( 0x249 );
			}
		}

		static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			return char.ToUpper(s[0]) + s.Substring(1);
		}

		public static string GetSpecialItem( int relic, int part )
		{
			string Part1 = "";
			switch ( relic )
			{
				case 1: Part1 = "Stonegate Castle"; break;
				case 2: Part1 = "the Vault of the Black Knight"; break;
				case 3: Part1 = "the Crypts of Dracula"; break;
				case 4: Part1 = "the Lodoria Catacombs"; break;
				case 5: Part1 = "Dungeon Deceit"; break;
				case 6: Part1 = "Dungeon Despise"; break;
				case 7: Part1 = "Dungeon Destard"; break;
				case 8: Part1 = "the City of Embers"; break;
				case 9: Part1 = "Dungeon Hythloth"; break;
				case 10: Part1 = "the Ice Fiend Lair"; break;
				case 11: Part1 = "Dungeon Shame"; break;
				case 12: Part1 = "Terathan Keep"; break;
				case 13: Part1 = "the Halls of Undermountain"; break;
				case 14: Part1 = "the Volcanic Cave"; break;
				case 15: Part1 = "the Mausoleum"; break;
				case 16: Part1 = "the Tower of Brass"; break;
				case 17: Part1 = "Vordo's Dungeon"; break;
				case 18: Part1 = "the Dragon's Maw"; break;
				case 19: Part1 = "the Ancient Pyramid"; break;
				case 20: Part1 = "Dungeon Exodus"; break;
				case 21: Part1 = "the Caverns of Poseidon"; break;
				case 22: Part1 = "Dungeon Clues"; break;
				case 23: Part1 = "Dardin's Pit"; break;
				case 24: Part1 = "Dungeon Doom"; break;
				case 25: Part1 = "the Fires of Hell"; break;
				case 26: Part1 = "the Mines of Morinia"; break;
				case 27: Part1 = "the Perinian Depths"; break;
				case 28: Part1 = "the Dungeon of Time Awaits"; break;
				case 29: Part1 = "the Ancient Prison"; break;
				case 30: Part1 = "the Cave of Fire"; break;
				case 31: Part1 = "the Cave of Souls"; break;
				case 32: Part1 = "Dungeon Ankh"; break;
				case 33: Part1 = "Dungeon Bane"; break;
				case 34: Part1 = "Dungeon Hate"; break;
				case 35: Part1 = "Dungeon Scorn"; break;
				case 36: Part1 = "Dungeon Torment"; break;
				case 37: Part1 = "Dungeon Vile"; break;
				case 38: Part1 = "Dungeon Wicked"; break;
				case 39: Part1 = "Dungeon Wrath"; break;
				case 40: Part1 = "the Flooded Temple"; break;
				case 41: Part1 = "the Gargoyle Crypts"; break;
				case 42: Part1 = "the Serpent Sanctum"; break;
				case 43: Part1 = "the Tomb of the Fallen Wizard"; break;
				case 44: Part1 = "the Blood Temple"; break;
				case 45: Part1 = "the Dungeon of the Mad Archmage"; break;
				case 46: Part1 = "the Tombs"; break;
				case 47: Part1 = "the Dungeon of the Lich King"; break;
				case 48: Part1 = "the Forgotten Halls"; break;
				case 49: Part1 = "the Ice Queen Fortress"; break;
				case 50: Part1 = "Dungeon Rock"; break;
				case 51: Part1 = "the Scurvy Reef"; break;
				case 52: Part1 = "the Undersea Castle"; break;
				case 53: Part1 = "the Tomb of Kazibal"; break;
				case 54: Part1 = "the Azure Castle"; break;
				case 55: Part1 = "the Catacombs of Azerok"; break;
				case 56: Part1 = "Dungeon Covetous"; break;
				case 57: Part1 = "the Glacial Scar"; break;
				case 58: Part1 = "the Temple of Osirus"; break;
				case 59: Part1 = "the Sanctum of Saltmarsh"; break;
			}

			if ( part > 0 ){ return GetRelicItem( Part1 ); }

			return Part1;
		}

		public static string GetRelicItem( string name )
		{
			switch ( name )
			{
				case "Stonegate Castle": return "heart of ash";
				case "the Vault of the Black Knight": return "mystical wax";
				case "the Crypts of Dracula": return "vampire teeth";
				case "the Lodoria Catacombs": return "face of the ancient king";
				case "Dungeon Deceit": return "wand of Talosh";
				case "Dungeon Despise": return "head of Urg";
				case "Dungeon Destard": return "flame of Dramulox";
				case "the City of Embers": return "crown of Vorgol";
				case "Dungeon Hythloth": return "claw of Saramon";
				case "the Ice Fiend Lair": return "horn of the frozen hells";
				case "Dungeon Shame": return "elemental salt";
				case "Terathan Keep": return "eye of plagues";
				case "the Halls of Undermountain": return "hair of the earth";
				case "the Volcanic Cave": return "skull of Turlox";
				case "the Mausoleum": return "tattered robe of Mezlo";
				case "the Tower of Brass": return "blood of the forest";
				case "Vordo's Dungeon": return "cinders of life";
				case "the Dragon's Maw": return "crystal scales";
				case "the Ancient Pyramid": return "chest of suffering";
				case "Dungeon Exodus": return "whip from below";
				case "the Caverns of Poseidon": return "scale of the sea";
				case "Dungeon Clues": return "braclet of war";
				case "Dardin's Pit": return "stump of the ancients";
				case "Dungeon Doom": return "dark blood";
				case "the Fires of Hell": return "firescale tooth";
				case "the Mines of Morinia": return "ichor of Xthizx";
				case "the Perinian Depths": return "heart of a vampire queen";
				case "the Dungeon of Time Awaits": return "hourglass of ages";
				case "the Ancient Prison": return "shackles of Saramak";
				case "the Cave of Fire": return "mouth of embers";
				case "the Cave of Souls": return "cowl of shadegloom";
				case "Dungeon Ankh": return "wedding dress of virtue";
				case "Dungeon Bane": return "lilly pad of the bog";
				case "Dungeon Hate": return "immortal bones";
				case "Dungeon Scorn": return "staff of scorn";
				case "Dungeon Torment": return "mind of allurement";
				case "Dungeon Vile": return "mask of the ghost";
				case "Dungeon Wicked": return "dead venom flies";
				case "Dungeon Wrath": return "branch of the reaper";
				case "the Flooded Temple": return "ink of the deep";
				case "the Gargoyle Crypts": return "amulet of the stygian abyss";
				case "the Serpent Sanctum": return "skin of the guardian";
				case "the Tomb of the Fallen Wizard": return "orb of the fallen wizard";
				case "the Blood Temple": return "bleeding crystal";
				case "the Dungeon of the Mad Archmage": return "jade idol of Nesfatiti";
				case "the Tombs": return "scroll of Abraxus";
				case "the Dungeon of the Lich King": return "sphere of the dark circle";
				case "the Forgotten Halls": return "urn of Ulmarek's ashes";
				case "the Ice Queen Fortress": return "crystal of everfrost";
				case "Dungeon Rock": return "stone of the night gargoyle";
				case "the Scurvy Reef": return "pearl of Neptune";
				case "the Undersea Castle": return "Black Beard's brandy";
				case "the Tomb of Kazibal": return "lamp of the desert";
				case "the Azure Castle": return "azure dust";
				case "the Catacombs of Azerok": return "skull of Azerok";
				case "Dungeon Covetous": return "egg of the harpy hen";
				case "the Glacial Scar": return "bone of the frost giant";
				case "the Temple of Osirus": return "mind of silver";
				case "the Sanctum of Saltmarsh": return "scale of Scarthis";
			}

			return "";
		}

		public override void OnDoubleClick( Mobile e )
		{
			if ( !IsChildOf( e.Backpack ) )
			{
				e.SendMessage( ResolveText( e, "This must be in your backpack to read." ) );
			}
			else
			{
				e.CloseGump( typeof( ClueGump ) );
				e.SendGump( new ClueGump( e, this ) );
			}
		}

		public SomeRandomNote(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 1);
			writer.Write( ScrollMessage );
			writer.Write( ScrollTrue );
			writer.Write( m_Structured );
			writer.Write( m_TemplateId );
			writer.Write( m_Variant );
			int n = m_Args != null ? m_Args.Length : 0;
			writer.Write( n );
			for ( int i = 0; i < n; ++i )
				writer.Write( m_Args[i] ?? "" );
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			ScrollMessage = reader.ReadString();
			ScrollTrue = reader.ReadInt();

			if ( version >= 1 )
			{
				m_Structured = reader.ReadBool();
				m_TemplateId = reader.ReadInt();
				m_Variant = reader.ReadInt();
				int n = reader.ReadInt();
				if ( n < 0 ) n = 0;
				m_Args = new string[n];
				for ( int i = 0; i < n; ++i )
					m_Args[i] = reader.ReadString();
			}
			else
			{
				m_Structured = false;
				m_TemplateId = 0;
				m_Variant = 0;
				m_Args = new string[0];
			}
		}
	}
}
"""


def main() -> None:
    text = HEADER + build_switch("lie") + FOOTER_MID + build_switch("truth") + FOOTER_END
    OUT_PATH.write_text(text, encoding="utf-8")
    print(f"Wrote {OUT_PATH} ({text.count(chr(10)) + 1} lines)")
    print("CommitNote calls:", text.count("CommitNote("))


if __name__ == "__main__":
    main()
