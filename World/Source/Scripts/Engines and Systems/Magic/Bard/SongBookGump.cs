using Server.Items; 
using Server.Localization;
using Server.Network; 
using Server.Spells.Song; 
 

namespace Server.Gumps 
{ 
	public class SongBookGump : Gump 
	{ 
		private readonly SongBook m_Book; 

		public bool HasSpell( Mobile from, int spellID )
		{
			if ( m_Book.RootParentEntity == from )
				return (m_Book.HasSpell(spellID));
			else
				return false;
		}
       
		public SongBookGump( Mobile from, SongBook book, int page ) : base( 100, 100 ) 
		{ 
			m_Book = book;
			from.PlaySound( 0x55 );

			this.Closable=true;
			this.Disposable=false;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);
			AddImage(0, 0, 7005, book.Hue-1);
			AddImage(0, 0, 7006);
			AddImage(0, 0, 7024, 2736);
			AddImage(125, 130, 7047);
			AddImage(436, 130, 7047);

			int PriorPage = page - 1;
				if ( PriorPage < 1 ){ PriorPage = 9; }
			int NextPage = page + 1;
				if ( NextPage > 9 ){ NextPage = 1; }

			AddButton(72, 45, 4014, 4014, PriorPage, GumpButtonType.Reply, 0);
			AddButton(590, 48, 4005, 4005, NextPage, GumpButtonType.Reply, 0);

			if ( page == 1 )
			{
				AddHtml( 107, 46, 186, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382><CENTER>BARDIC SONGS</CENTER></BASEFONT></BODY>" ), false, false);

				int x = 95;
				int y = 100;
				int c = 0;

				if (HasSpell( from, 351) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 351, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Army's Paeon</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 352) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 352, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Enchanting Etude</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 353) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 353, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Energy Carol</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 354) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 354, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Energy Threnody</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 355) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 355, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Fire Carol</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 356) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 356, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Fire Threnody</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 357) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 357, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Foe Requiem</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 358) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 358, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Ice Carol</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 359) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 359, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Ice Threnody</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 360) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 360, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Knight's Minne</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 361) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 361, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Mage's Ballad</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 362) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 362, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Magic Finale</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 363) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 363, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Poison Carol</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 364) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 364, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Poison Threnody</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 365) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 365, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Shepherd's Dance</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
				if (HasSpell( from, 366) ) 
				{
					AddButton(x-5, y-5, 7048, 7048, 366, GumpButtonType.Reply, 0);
					AddHtml( x+25, y, 148, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Sinewy Etude</BASEFONT></BODY>" ), false, false); c++; y = y + 38;
				} if (c == 8){ x = 415; y = 100; }
			}
			else
			{
				AddHtml( 107, 46, 186, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382><CENTER>BARDIC SONGS</CENTER></BASEFONT></BODY>" ), false, false);
				AddHtml( 398, 48, 186, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382><CENTER>BARDIC SONGS</CENTER></BASEFONT></BODY>" ), false, false);

				int s11 = 0;
				string s12 = "";
				string s13 = "";
				string s14 = "";
				string s15 = "";
				string s16 = "";

				int s21 = 0;
				string s22 = "";
				string s23 = "";
				string s24 = "";
				string s25 = "";
				string s26 = "";

				if ( page == 2 )
				{
					s12 = StringCatalog.Resolve( from.Account, "Army's Paeon" );
					s13 = StringCatalog.Resolve( from.Account, "An area of effect that regenerates your party's health slowly." );
					s14 = "55";
					s15 = "15";
					s16 = "ArmysPaeon";
					s11 = 0x404;

					s22 = StringCatalog.Resolve( from.Account, "Enchanting Etude" );
					s23 = StringCatalog.Resolve( from.Account, "An area of effect that raises the intelligence of your party." );
					s24 = "60";
					s25 = "20";
					s26 = "EnchantingEtude";
					s21 = 0x405;
				}
				else if ( page == 3 )
				{
					s12 = StringCatalog.Resolve( from.Account, "Energy Carol" );
					s13 = StringCatalog.Resolve( from.Account, "An area of effect that raises the energy resistance of your party." );
					s14 = "50";
					s15 = "12";
					s16 = "EnergyCarol";
					s11 = 0x406;

					s22 = StringCatalog.Resolve( from.Account, "Energy Threnody" );
					s23 = StringCatalog.Resolve( from.Account, "Lowers the energy resistance of your target." );
					s24 = "70";
					s25 = "25";
					s26 = "EnergyThrenody";
					s21 = 0x407;
				}
				else if ( page == 4 )
				{
					s12 = StringCatalog.Resolve( from.Account, "Fire Carol" );
					s13 = StringCatalog.Resolve( from.Account, "An area of effect that raises the fire resistance of your party." );
					s14 = "50";
					s15 = "12";
					s16 = "FireCarol";
					s11 = 0x408;

					s22 = StringCatalog.Resolve( from.Account, "Fire Threnody" );
					s23 = StringCatalog.Resolve( from.Account, "Lowers the fire resistance of your target." );
					s24 = "70";
					s25 = "25";
					s26 = "FireThrenody";
					s21 = 0x409;
				}
				else if ( page == 5 )
				{
					s12 = StringCatalog.Resolve( from.Account, "Foe Requiem" );
					s13 = StringCatalog.Resolve( from.Account, "Damages your target with a burst of sonic energy." );
					s14 = "50";
					s15 = "30";
					s16 = "FoeRequiem";
					s11 = 0x40A;

					s22 = StringCatalog.Resolve( from.Account, "Ice Carol" );
					s23 = StringCatalog.Resolve( from.Account, "An area of effect that raises the cold resistance of your party." );
					s24 = "50";
					s25 = "12";
					s26 = "IceCarol";
					s21 = 0x40B;
				}
				else if ( page == 6 )
				{
					s12 = StringCatalog.Resolve( from.Account, "Ice Threnody" );
					s13 = StringCatalog.Resolve( from.Account, "Lowers the ice resistance of your target." );
					s14 = "70";
					s15 = "25";
					s16 = "IceThrenody";
					s11 = 0x40C;

					s22 = StringCatalog.Resolve( from.Account, "Knight's Minne" );
					s23 = StringCatalog.Resolve( from.Account, "An area of effect that raises the physical resist of your party." );
					s24 = "50";
					s25 = "12";
					s26 = "KnightsMinne";
					s21 = 0x40D;
				}
				else if ( page == 7 )
				{
					s12 = StringCatalog.Resolve( from.Account, "Mage's Ballad" );
					s13 = StringCatalog.Resolve( from.Account, "An area of effect that regenerates your party's mana slowly." );
					s14 = "55";
					s15 = "15";
					s16 = "MagesBallad";
					s11 = 0x40E;

					s22 = StringCatalog.Resolve( from.Account, "Magic Finale" );
					s23 = StringCatalog.Resolve( from.Account, "An area of effect that dispels all summoned creatures around you." );
					s24 = "90";
					s25 = "35";
					s26 = "MagicFinale";
					s21 = 0x410;
				}
				else if ( page == 8 )
				{
					s12 = StringCatalog.Resolve( from.Account, "Poison Carol" );
					s13 = StringCatalog.Resolve( from.Account, "An area of effect that raises the poison resistance of your party." );
					s14 = "50";
					s15 = "12";
					s16 = "PoisonCarol";
					s11 = 0x411;

					s22 = StringCatalog.Resolve( from.Account, "Poison Threnody" );
					s23 = StringCatalog.Resolve( from.Account, "Lowers the poison resistance of your target." );
					s24 = "70";
					s25 = "25";
					s26 = "PoisonThrenody";
					s21 = 0x412;
				}
				else if ( page == 9 )
				{
					s12 = StringCatalog.Resolve( from.Account, "Shepherd's Dance" );
					s13 = StringCatalog.Resolve( from.Account, "An area of effect that raises the dexterity of your party." );
					s14 = "60";
					s15 = "20";
					s16 = "ShephardsDance";
					s11 = 0x413;

					s22 = StringCatalog.Resolve( from.Account, "Sinewy Etude" );
					s23 = StringCatalog.Resolve( from.Account, "An area of effect that raises the strength of your party." );
					s24 = "60";
					s25 = "20";
					s26 = "SinewyEtude";
					s21 = 0x414;
				}

				AddImage(75, 80, s11);
				AddHtml( 134, 90, 177, 20, @"<BODY><BASEFONT Color=#d6c382>" + s12 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 135, 125, 56, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Skill:</BASEFONT></BODY>" ), false, false);
				AddHtml( 199, 125, 56, 20, @"<BODY><BASEFONT Color=#d6c382>" + s14 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 134, 155, 56, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Mana:</BASEFONT></BODY>" ), false, false);
				AddHtml( 198, 155, 56, 20, @"<BODY><BASEFONT Color=#d6c382>" + s15 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 95, 215, 189, 20, @"<BODY><BASEFONT Color=#d6c382>[" + s16 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 76, 250, 247, 143, @"<BODY><BASEFONT Color=#d6c382>" + s13 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 77, 190, 189, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Keyboard Command:</BASEFONT></BODY>" ), false, false);

				AddImage(370, 80, s21);
				AddHtml( 429, 90, 177, 20, @"<BODY><BASEFONT Color=#d6c382>" + s22 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 430, 125, 56, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Skill:</BASEFONT></BODY>" ), false, false);
				AddHtml( 494, 125, 56, 20, @"<BODY><BASEFONT Color=#d6c382>" + s24 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 429, 155, 56, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Mana:</BASEFONT></BODY>" ), false, false);
				AddHtml( 493, 155, 56, 20, @"<BODY><BASEFONT Color=#d6c382>" + s25 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 390, 215, 189, 20, @"<BODY><BASEFONT Color=#d6c382>[" + s26 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 371, 250, 247, 143, @"<BODY><BASEFONT Color=#d6c382>" + s23 + @"</BASEFONT></BODY>", false, false);
				AddHtml( 372, 190, 189, 20, StringCatalog.Resolve( from.Account, @"<BODY><BASEFONT Color=#d6c382>Keyboard Command:</BASEFONT></BODY>" ), false, false);
			}
		}
       
		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile; 

			if ( info.ButtonID < 300 && info.ButtonID > 0 )
			{
				from.SendSound( 0x55 );
				int page = info.ButtonID;
				if ( page < 1 ){ page = 9; }
				if ( page > 9 ){ page = 1; }
				from.SendGump( new SongBookGump( from, m_Book, page ) );
			}
			else if ( info.ButtonID > 300 )
			{
				if ( info.ButtonID == 351 ){ new ArmysPaeonSong( from, null ).Cast(); }
				else if ( info.ButtonID == 352 ){ new EnchantingEtudeSong( from, null ).Cast(); }
				else if ( info.ButtonID == 353 ){ new EnergyCarolSong( from, null ).Cast(); }
				else if ( info.ButtonID == 354 ){ new EnergyThrenodySong( from, null ).Cast(); }
				else if ( info.ButtonID == 355 ){ new FireCarolSong( from, null ).Cast(); }
				else if ( info.ButtonID == 356 ){ new FireThrenodySong( from, null ).Cast(); }
				else if ( info.ButtonID == 357 ){ new FoeRequiemSong( from, null ).Cast(); }
				else if ( info.ButtonID == 358 ){ new IceCarolSong( from, null ).Cast(); }
				else if ( info.ButtonID == 359 ){ new IceThrenodySong( from, null ).Cast(); }
				else if ( info.ButtonID == 360 ){ new KnightsMinneSong( from, null ).Cast(); }
				else if ( info.ButtonID == 361 ){ new MagesBalladSong( from, null ).Cast(); }
				else if ( info.ButtonID == 362 ){ new MagicFinaleSong( from, null ).Cast(); }
				else if ( info.ButtonID == 363 ){ new PoisonCarolSong( from, null ).Cast(); }
				else if ( info.ButtonID == 364 ){ new PoisonThrenodySong( from, null ).Cast(); }
				else if ( info.ButtonID == 365 ){ new SheepfoeMamboSong( from, null ).Cast(); }
				else if ( info.ButtonID == 366 ){ new SinewyEtudeSong( from, null ).Cast(); }
			}
			else
				from.PlaySound( 0x55 );
		}
	} 
}
