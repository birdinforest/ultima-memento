using System;
using Server;
using Server.Items;
using System.Text;
using Server.Mobiles;
using Server.Gumps;
using Server.Misc;
using Server.Network;
using System.Collections;
using System.Globalization;
using Server.Accounting;
using Server.Localization;

namespace Server.Items
{
	public class LoreBook : DynamicBook
	{
		[Constructable]
		public LoreBook( )
		{
			Weight = 1.0;

			if ( BookTrue > 0 ){} else
			{
				writeBook( Utility.RandomMinMax( 0, 46 ) );
			}
		}

		public void writeBook( int val )
		{
			BookRegion = null;	BookMap = null;		BookWorld = null;	BookItem = null;	BookTrue = 1;	BookPower = 0;

			ItemID = RandomThings.GetRandomBookItemID();
			Hue = Utility.RandomColor(0);
			SetBookCover( 0, this );
			BookTitle = Server.Misc.RandomThings.GetBookTitle();
			Name = BookTitle;
			BookAuthor = Server.Misc.RandomThings.GetRandomAuthor();

			switch( val )
			{
				case 0: BookTitle = "Akalabeth's Tale"; BookAuthor = "Shamino the Anarch"; SetBookCover( 1, this ); break;
				case 1: BookTitle = "The Lost Land"; BookAuthor = "Sentri the Seeker"; SetBookCover( 42, this ); break;
				case 2: BookTitle = "The Balance Vol I of II"; BookAuthor = "Dedric the Knight"; SetBookCover( 80, this ); break;
				case 3: BookTitle = "The Balance Vol II of II"; BookAuthor = "Dedric the Knight"; SetBookCover( 80, this ); break;
				case 4: BookTitle = "The Black Gate Demon"; BookAuthor = "Zalifar the Wizard"; SetBookCover( 66, this ); break;
				case 5: BookTitle = "The Blue Ore"; BookAuthor = "Jarg the Blacksmith"; SetBookCover( 69, this ); break;
				case 6: BookTitle = "Crystal Flasks"; BookAuthor = "Frug the Explorer"; SetBookCover( 32, this ); break;
				case 7: BookTitle = "The Curse of the Island"; BookAuthor = "Sempkin Burg"; SetBookCover( 23, this ); break;
				case 8: BookTitle = "The Dark Age"; BookAuthor = "Nedina the Ghastly"; SetBookCover( 25, this ); break;
				case 9: BookTitle = "The Dark Core"; BookAuthor = "Erethian the Mage"; SetBookCover( 67, this ); break;
				case 10: BookTitle = "Death to Pirates"; BookAuthor = "Granafla the Sailor"; SetBookCover( 65, this ); break;
				case 11: BookTitle = "The Death Knights"; BookAuthor = "Arul Martos"; SetBookCover( 78, this ); break;
				case 12: BookTitle = "The Darkness Within"; BookAuthor = "Cyrus Belmont"; SetBookCover( 79, this ); break;
				case 13: BookTitle = "The Destruction of Exodus"; BookAuthor = "Hafar of the Red Robe"; SetBookCover( 67, this ); break;
				case 14: BookTitle = "The Knight Who Fell"; BookAuthor = "Darun the Priest"; SetBookCover( 78, this ); break;
				case 15: BookTitle = "The Fall of Mondain"; BookAuthor = "Gram the Seventh"; SetBookCover( 55, this ); break;
				case 16: BookTitle = "Forging the Fire"; BookAuthor = "Malek the Smith"; SetBookCover( 62, this ); break;
				case 17: BookTitle = "Forgotten Dungeons"; BookAuthor = "Curan the Fighter"; SetBookCover( 2, this ); break;
				case 18: BookTitle = "The Cruel Game"; BookAuthor = "Killun the Poor"; SetBookCover( 50, this ); break;
				case 19: BookTitle = "The Ice Queen"; BookAuthor = "Suri the Bard"; SetBookCover( 34, this ); break;
				case 20: BookTitle = "Luck of the Rogue"; BookAuthor = "The Gray Mouser"; SetBookCover( 13, this ); break;
				case 21: BookTitle = "A Tattered Journal"; BookAuthor = "Unknown"; SetBookCover( 0, this ); break;
				case 22: BookTitle = "The Curse of Mangar"; BookAuthor = "Lemka the Cloaked"; SetBookCover( 59, this ); break;
				case 23: BookTitle = "The Times of Minax"; BookAuthor = "Halgram the Obscure"; SetBookCover( 56, this ); break;
				case 24: BookTitle = "Rangers of Lodoria"; BookAuthor = "Grimm the Tracker"; SetBookCover( 77, this ); break;
				case 25: BookTitle = "Gem of Immortality"; BookAuthor = "Batlin the Druid"; SetBookCover( 58, this ); break;
				case 26: BookTitle = "The Gods of Men"; BookAuthor = "Perdue the Magician"; SetBookCover( 75, this ); break;
				case 27: BookTitle = "Castles Above"; BookAuthor = "Harkan the Explorer"; SetBookCover( 71, this ); break;
				case 28: BookTitle = "Staff of Five Parts"; BookAuthor = "Zuri the Wizard"; SetBookCover( 24, this ); break;
				case 29: BookTitle = "The Story of Exodus"; BookAuthor = "Dreova of the Isles"; SetBookCover( 67, this ); break;
				case 30: BookTitle = "The Story of Minax"; BookAuthor = "Jaxina the Wise"; SetBookCover( 56, this ); break;
				case 31: BookTitle = "The Story of Mondain"; BookAuthor = "Milydor the Sage"; SetBookCover( 55, this ); break;
				case 32: BookTitle = "The Bard's Tale"; BookAuthor = "Ramzef the Bard"; SetBookCover( 37, this ); break;
				case 33: BookTitle = "Death Dealing"; BookAuthor = "Murgox the Warlock"; SetBookCover( 27, this ); break;
				case 34: BookTitle = "The Orb of the Abyss"; BookAuthor = "Gribs the High Mage"; SetBookCover( 24, this ); break;
				case 35: BookTitle = "The Underworld Gate"; BookAuthor = "Garamon the Wizard"; SetBookCover( 2, this ); break;
				case 36: BookTitle = "The Elemental Titans"; BookAuthor = "Xavier the Theurgist"; SetBookCover( 46, this ); break;
				case 37: BookTitle = "The Dragon's Egg"; BookAuthor = "Druv the Dwarf"; SetBookCover( 9, this ); break;
				case 38: BookTitle = "Magic in the Moon"; BookAuthor = "Selene the Wizard"; SetBookCover( 71, this ); break;
				case 39: BookTitle = "The Maze of Wonder"; BookAuthor = "Risa the Scholar"; SetBookCover( 49, this ); break;
				case 40: BookTitle = "The Pass of the Gods"; BookAuthor = "Mareskon the Elf"; SetBookCover( 64, this ); break;
				case 41: BookTitle = "Valley of Corruption"; BookAuthor = "Willum the Druid"; SetBookCover( 45, this ); break;
				case 42: BookTitle = "The Demon Shard"; BookAuthor = "Vanesa the Sorcereress"; SetBookCover( 67, this ); break;
				case 43: BookTitle = "The Syth Order"; BookAuthor = "Xandru the Jedi"; SetBookCover( 78, this ); break;
				case 44: BookTitle = "The Rule of One"; BookAuthor = "Asajj Ventress the Syth Lord"; SetBookCover( 78, this ); ItemID = 0x4CDF; Light = LightType.Circle225; break;
				case 45: BookTitle = "Antiquities"; BookAuthor = "Daran the Collector"; SetBookCover( 7, this ); break;
				case 46: BookTitle = "The Jedi Order"; BookAuthor = "Zoda the Jedi Master"; SetBookCover( 16, this ); ItemID = 0x543C; Light = LightType.Circle225; break;
			}

			GetText( this );
			Name = BookTitle;
		}

		public LoreBook( Serial serial ) : base( serial )
		{
		}

		public override bool IsContentLocalized
		{
			get { return ShotkeyPrefixForTitle( BookTitle ) != null; }
		}

		public override string DisplayNameLocalizationKey
		{
			get
			{
				string prefix = ShotkeyPrefixForTitle( BookTitle );
				return prefix != null ? prefix + ".title" : null;
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.WriteEncodedInt( (int)0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadEncodedInt();

			if ( BookTitle == "Staff of Five" )
			{
				BookTitle = "Staff of Five Parts";
				Name = "Staff of Five Parts";
			}

			GetText( this );
		}

		/// <summary>Logical-key prefix in lore-books.json, or null when the title is not a catalogued LoreBook.</summary>
		public static string ShotkeyPrefixForTitle( string title )
		{
			if ( title == null )
				return null;

			switch ( title )
			{
				case "Akalabeth's Tale": return "lore.book.akalabeths_tale";
				case "The Lost Land": return "lore.book.lost_land";
				case "The Balance Vol I of II": return "lore.book.balance_vol1";
				case "The Balance Vol II of II": return "lore.book.balance_vol2";
				case "The Black Gate Demon": return "lore.book.black_gate_demon";
				case "The Blue Ore": return "lore.book.blue_ore";
				case "Crystal Flasks": return "lore.book.crystal_flasks";
				case "The Curse of the Island": return "lore.book.curse_of_the_island";
				case "The Dark Age": return "lore.book.dark_age";
				case "The Dark Core": return "lore.book.dark_core";
				case "Death to Pirates": return "lore.book.death_to_pirates";
				case "The Death Knights": return "lore.book.death_knights";
				case "The Darkness Within": return "lore.book.darkness_within";
				case "The Destruction of Exodus": return "lore.book.destruction_of_exodus";
				case "The Knight Who Fell": return "lore.book.knight_who_fell";
				case "The Fall of Mondain": return "lore.book.fall_of_mondain";
				case "Forging the Fire": return "lore.book.forging_the_fire";
				case "Forgotten Dungeons": return "lore.book.forgotten_dungeons";
				case "The Cruel Game": return "lore.book.cruel_game";
				case "The Ice Queen": return "lore.book.ice_queen";
				case "Luck of the Rogue": return "lore.book.luck_of_the_rogue";
				case "A Tattered Journal":
				case "Tattered Journal": return "lore.book.tattered_journal";
				case "The Curse of Mangar": return "lore.book.curse_of_mangar";
				case "The Times of Minax": return "lore.book.times_of_minax";
				case "Rangers of Lodoria": return "lore.book.rangers_of_lodoria";
				case "Gem of Immortality": return "lore.book.gem_of_immortality";
				case "The Gods of Men": return "lore.book.gods_of_men";
				case "Castles Above": return "lore.book.castles_above";
				case "Staff of Five Parts": return "lore.book.staff_of_five_parts";
				case "The Story of Exodus": return "lore.book.story_of_exodus";
				case "The Story of Minax": return "lore.book.story_of_minax";
				case "The Story of Mondain": return "lore.book.story_of_mondain";
				case "The Bard's Tale": return "lore.book.bards_tale";
				case "Death Dealing": return "lore.book.death_dealing";
				case "The Orb of the Abyss": return "lore.book.orb_of_the_abyss";
				case "The Underworld Gate": return "lore.book.underworld_gate";
				case "The Elemental Titans": return "lore.book.elemental_titans";
				case "The Dragon's Egg": return "lore.book.dragons_egg";
				case "Magic in the Moon": return "lore.book.magic_in_the_moon";
				case "The Maze of Wonder": return "lore.book.maze_of_wonder";
				case "The Pass of the Gods": return "lore.book.pass_of_the_gods";
				case "Valley of Corruption": return "lore.book.valley_of_corruption";
				case "The Demon Shard": return "lore.book.demon_shard";
				case "The Syth Order": return "lore.book.syth_order";
				case "The Rule of One": return "lore.book.rule_of_one";
				case "Antiquities": return "lore.book.antiquities";
				case "The Jedi Order": return "lore.book.jedi_order";
				default: return null;
			}
		}

		public static int TemplateCityCount( string prefix )
		{
			if ( prefix == "lore.book.orb_of_the_abyss" )
				return 2;
			if ( prefix == "lore.book.underworld_gate" )
				return 1;
			return 0;
		}

		public static string CityShotkey( string englishCity )
		{
			if ( string.IsNullOrEmpty( englishCity ) )
				return null;

			string slug = englishCity;
			if ( slug.StartsWith( "the " ) )
				slug = slug.Substring( 4 );

			slug = slug.Replace( " ", "_" ).Replace( "'", "" ).ToLowerInvariant();
			return "lore.book.city." + slug;
		}

		public static string ResolveCity( IAccount account, string englishCity )
		{
			if ( string.IsNullOrEmpty( englishCity ) )
				return "";

			string key = CityShotkey( englishCity );
			if ( key != null )
			{
				string lang = AccountLang.GetLanguageCode( account );
				string resolved = StringCatalog.TryResolveByKey( lang, key );
				if ( resolved != null )
					return resolved;
			}

			return StringCatalog.Resolve( account, englishCity );
		}

		public static object[] LocalizedCityArgs( IAccount account, DynamicBook book, int count )
		{
			if ( count <= 0 )
				return new object[0];

			object[] args = new object[count];
			args[0] = ResolveCity( account, book != null ? book.BookItem : null );
			if ( count > 1 )
				args[1] = ResolveCity( account, book != null ? book.BookWorld : null );
			return args;
		}

		public static string ResolveLocalizedTitle( Mobile viewer, DynamicBook book )
		{
			if ( book == null || book.BookTitle == null )
				return "";

			string prefix = ShotkeyPrefixForTitle( book.BookTitle );
			string lang = AccountLang.GetLanguageCode( viewer != null ? viewer.Account : null );

			if ( prefix != null )
			{
				string byKey = StringCatalog.TryResolveByKey( lang, prefix + ".title" );
				if ( byKey != null )
					return byKey;
			}

			return StringCatalog.TryResolve( lang, book.BookTitle ) ?? book.BookTitle;
		}

		public static string ResolveLocalizedAuthor( Mobile viewer, DynamicBook book )
		{
			if ( book == null || book.BookAuthor == null )
				return "";

			string prefix = ShotkeyPrefixForTitle( book.BookTitle );
			string lang = AccountLang.GetLanguageCode( viewer != null ? viewer.Account : null );

			if ( prefix != null )
			{
				string byKey = StringCatalog.TryResolveByKey( lang, prefix + ".author" );
				if ( byKey != null )
					return byKey;
			}

			return StringCatalog.TryResolve( lang, book.BookAuthor ) ?? book.BookAuthor;
		}

		public static string ResolveLocalizedBody( Mobile viewer, DynamicBook book )
		{
			if ( book == null )
				return "";

			string prefix = ShotkeyPrefixForTitle( book.BookTitle );
			string lang = AccountLang.GetLanguageCode( viewer != null ? viewer.Account : null );
			IAccount account = viewer != null ? viewer.Account : null;

			if ( prefix != null )
			{
				string tmpl = StringCatalog.TryResolveByKey( lang, prefix + ".body" );
				if ( tmpl != null )
				{
					int n = TemplateCityCount( prefix );
					if ( n <= 0 )
						return tmpl;
					return string.Format( tmpl, LocalizedCityArgs( account, book, n ) );
				}
			}

			if ( book.BookText == null )
				return "";

			return StringCatalog.TryResolve( lang, book.BookText ) ?? book.BookText;
		}

		private static void EnsureTemplateCities( LoreBook book, string prefix )
		{
			int n = TemplateCityCount( prefix );
			if ( n <= 0 )
				return;

			if ( string.IsNullOrEmpty( book.BookItem ) )
				book.BookItem = RandomThings.GetRandomCity();

			if ( n > 1 && string.IsNullOrEmpty( book.BookWorld ) )
				book.BookWorld = RandomThings.GetRandomCity();
		}

		public static void GetText( LoreBook book )
		{
			if ( book == null )
				return;

			string prefix = ShotkeyPrefixForTitle( book.BookTitle );
			if ( prefix == null )
				return;

			EnsureTemplateCities( book, prefix );

			string enBody = StringCatalog.TryResolveByKey( "en", prefix + ".body" );
			if ( enBody == null )
				return;

			int n = TemplateCityCount( prefix );
			if ( n <= 0 )
				book.BookText = enBody;
			else if ( n == 1 )
				book.BookText = string.Format( enBody, book.BookItem ?? "" );
			else
				book.BookText = string.Format( enBody, book.BookItem ?? "", book.BookWorld ?? "" );
		}
	}
}
