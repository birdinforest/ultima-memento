using System;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Localization;

namespace Server.Items
{
	public class BookWitchBrewing : Item
	{
		[Constructable]
		public BookWitchBrewing() : base( 0x5689 )
		{
			Weight = 1.0;
			Name = "The Witch's Brew";
			Hue = 0x9A2;
		}

		public class BookGump : Gump
		{
			private readonly Mobile m_From;

			public BookGump( Mobile from, int page ) : base( 100, 100 )
			{
				m_From = from;
				from.SendSound( 0x55 );

				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;

				AddPage( 0 );

				AddImage( 0, 0, 7005, 2845 );
				AddImage( 0, 0, 7006 );
				AddImage( 0, 0, 7024, 2736 );
				AddImage( 87, 117, 7053 );
				AddImage( 382, 117, 7053 );

				int prev = page - 1;
				if ( prev < 1 ) { prev = 99; }
				int next = page + 1;

				AddButton( 72, 45, 4014, 4014, prev, GumpButtonType.Reply, 0 );
				AddButton( 590, 48, 4005, 4005, next, GumpButtonType.Reply, 0 );

				int potion = 0;

				if ( page == 2 ) { potion = 2; }
				else if ( page == 3 ) { potion = 4; }
				else if ( page == 4 ) { potion = 6; }
				else if ( page == 5 ) { potion = 8; }
				else if ( page == 6 ) { potion = 10; }
				else if ( page == 7 ) { potion = 11; }
				else if ( page == 8 ) { potion = 12; }
				else if ( page == 9 ) { potion = 14; }
				else if ( page == 10 ) { potion = 16; }

				if ( page == 1 )
				{
					AddHtml( 107, 46, 186, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191><CENTER>THE WITCH'S BREW</CENTER></BASEFONT></BODY>" ), false, false );
					AddHtml( 398, 48, 186, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191><CENTER>THE WITCH'S BREW</CENTER></BASEFONT></BODY>" ), false, false );

					AddHtml( 78, 75, 248, 318, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191>Witchery brewing is the art of taking morbid reagents and creating concoctions that necromancers can use in their dark magics. You would use your forensics skill to create the potions, and your necromancy skill to use them. This book explains the various mixtures you can make, as well as additional information to manage these potions effectively. Unlike other potions, these require jars as the liquid needs a thicker glass to store as it is acidic enough to dissolve bottle glass and even the wood of a keg.</BASEFONT></BODY>" ), false, false );

					AddHtml( 372, 75, 248, 318, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191>You will need a small cauldron to brew these concoctions. You can also get a belt pouch to store the ingredients, cauldrons, jars, potions, and this book to make them easier to carry. Single click this bag to organize it for easier use of the mixtures.</BASEFONT></BODY>" ), false, false );
				}
				else
				{
					AddHtml( 107, 46, 186, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191><CENTER>{0}</CENTER></BASEFONT></BODY>", potionInfo( potion, 1 ) ), false, false );

					AddHtml( 73, 72, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191>Forensics:</BASEFONT></BODY>" ), false, false );
					AddHtml( 267, 72, 47, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 4 ) ), false, false );

					AddHtml( 73, 98, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191>Necromancy:</BASEFONT></BODY>" ), false, false );
					AddHtml( 267, 98, 47, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 5 ) ), false, false );

					AddImage( 77, 128, Int32.Parse( potionInfo( potion, 2 ) ) );
					AddHtml( 133, 139, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191>Ingredients</BASEFONT></BODY>" ), false, false );

					AddHtml( 73, 180, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 6 ) ), false, false );
					AddHtml( 73, 206, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 7 ) ), false, false );
					AddHtml( 73, 232, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 8 ) ), false, false );

					AddHtml( 73, 258, 245, 133, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 3 ) ), false, false );

					potion++;

					AddHtml( 398, 48, 186, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191><CENTER>{0}</CENTER></BASEFONT></BODY>", potionInfo( potion, 1 ) ), false, false );

					AddHtml( 366, 72, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191>Forensics:</BASEFONT></BODY>" ), false, false );
					AddHtml( 560, 72, 47, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 4 ) ), false, false );

					AddHtml( 366, 98, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191>Necromancy:</BASEFONT></BODY>" ), false, false );
					AddHtml( 560, 98, 47, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 5 ) ), false, false );

					AddImage( 366, 128, Int32.Parse( potionInfo( potion, 2 ) ) );
					AddHtml( 422, 139, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#d89191>Ingredients</BASEFONT></BODY>" ), false, false );

					AddHtml( 366, 180, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 6 ) ), false, false );
					AddHtml( 366, 206, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 7 ) ), false, false );
					AddHtml( 366, 232, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 8 ) ), false, false );

					AddHtml( 366, 258, 245, 133, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#d89191>{0}</BASEFONT></BODY>", potionInfo( potion, 3 ) ), false, false );
				}
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state.Mobile;
				int page = info.ButtonID;
				if ( page == 99 ) { page = 9; }
				else if ( page > 9 ) { page = 1; }

				if ( info.ButtonID > 0 )
					from.SendGump( new BookGump( from, page ) );
				else
					from.SendSound( 0x55 );
			}

			private string R( string english ) => StringCatalog.Resolve( m_From.Account, english );

			public static string potionDesc( int potion ) => potionDesc( null, potion );

			public static string potionDesc( Server.Accounting.IAccount account, int potion )
			{
				switch ( potion )
				{
					case 2: return StringCatalog.Resolve( account, "This is created from a witch's brew: Gives one the same sight of the undead, where they are able to see in the dark. To use it, one should have a 10 in Forensics and a 5 in Necromancy." );
					case 3: return StringCatalog.Resolve( account, "This is created from a witch's brew: Summons the spirits to unlock something for you. To use it, one should have a 15 in Forensics and a 10 in Necromancy." );
					case 4: return StringCatalog.Resolve( account, "This is created from a witch's brew: Causes one to suffer from a poisonous disease. To use it, one should have a 20 in Forensics and a 15 in Necromancy." );
					case 5: return StringCatalog.Resolve( account, "This is created from a witch's brew: Summons a spirit to disable a trap. To use it, one should have a 25 in Forensics and a 20 in Necromancy." );
					case 6: return StringCatalog.Resolve( account, "This is created from a witch's brew: Creates a burst of harmful gas. To use it, one should have a 30 in Forensics and a 25 in Necromancy." );
					case 7: return StringCatalog.Resolve( account, "This is created from a witch's brew: Absorbs mana from the target, giving it to you in return. To use it, one should have a 35 in Forensics and a 30 in Necromancy." );
					case 8: return StringCatalog.Resolve( account, "This is created from a witch's brew: Creates a protective wall of spikes. To use it, one should have a 40 in Forensics and a 35 in Necromancy." );
					case 9: return StringCatalog.Resolve( account, "This is created from a witch's brew: Cures one of poisonous diseases. To use it, one should have a 45 in Forensics and a 40 in Necromancy." );
					case 10: return StringCatalog.Resolve( account, "This is created from a witch's brew: Takes some of your life and bestows it upon another. To use it, one should have a 50 in Forensics and a 45 in Necromancy." );
					case 11: return StringCatalog.Resolve( account, "This is created from a witch's brew: Turns the body into an invisible ghostly form that cannot be seen. To use it, one should have a 55 in Forensics and a 50 in Necromancy." );
					case 12: return StringCatalog.Resolve( account, "This is created from a witch's brew: Turns your body into ghostly matter that reappears in a nearby location. To use it, one should have a 60 in Forensics and a 55 in Necromancy." );
					case 13: return StringCatalog.Resolve( account, "This is created from a witch's brew: Ignites a marked rune with power to transport one to that location. To use it, one should have a 65 in Forensics and a 60 in Necromancy." );
					case 14: return StringCatalog.Resolve( account, "This is created from a witch's brew: Creates an illusionary image of you, distracting your foes. To use it, one should have a 70 in Forensics and a 65 in Necromancy." );
					case 15: return StringCatalog.Resolve( account, "This is created from a witch's brew: Marks a rune location with symbols of evil, so you can use recalling magic on it to return to that location. To use it, one should have a 75 in Forensics and a 70 in Necromancy." );
					case 16: return StringCatalog.Resolve( account, "This is created from a witch's brew: Uses a magic rune to create a horrific black gate. Those that enter will appear at the runic location. To use it, one should have a 80 in Forensics and a 75 in Necromancy." );
					case 17: return StringCatalog.Resolve( account, "This is created from a witch's brew: Dumps vampire blood in your pack, that will resurrect you a few moments after losing your life. You can also directly resurrect others. To use it, one should have a 85 in Forensics and a 80 in Necromancy." );
					default: return "";
				}
			}

			/// <param name="val">1 name, 2 icon id, 3 description, 4–5 skill thresholds, 6–8 ingredients (9 = third ingredient when used).</param>
			public string potionInfo( int page, int val )
			{
				string txtName = "";
				string txtIcon = "";
				string txtInfo = "";
				string txtSklA = "";
				string txtSklB = "";
				string txtIngA = "";
				string txtIngB = "";
				string txtIngC = "";

				if ( page == 2 ) { txtName = R( "Eyes of the Dead Mixture" ); txtIcon = "11460"; txtInfo = R( "Gives one the same sight of the undead, where they are able to see in the dark." ); txtSklA = "10"; txtSklB = "5"; txtIngA = R( "Mummy Wrap" ); txtIngB = R( "Eye of Toad" ); txtIngC = ""; }
				else if ( page == 3 ) { txtName = R( "Tomb Raiding Concoction" ); txtIcon = "11468"; txtInfo = R( "Summons the spirits to unlock something for you." ); txtSklA = "15"; txtSklB = "10"; txtIngA = R( "Maggot" ); txtIngB = R( "Beetle Shell" ); txtIngC = ""; }
				else if ( page == 4 ) { txtName = R( "Disease Draught" ); txtIcon = "11458"; txtInfo = R( "Causes one to suffer from a poisonous disease." ); txtSklA = "20"; txtSklB = "15"; txtIngA = R( "Violet Fungus" ); txtIngB = R( "Nox Crystal" ); txtIngC = ""; }
				else if ( page == 5 ) { txtName = R( "Phantasm Elixir" ); txtIcon = "11465"; txtInfo = R( "Summons a spirit to disable a trap." ); txtSklA = "25"; txtSklB = "20"; txtIngA = R( "Dried Toad" ); txtIngB = R( "Gargoyle Ear" ); txtIngC = ""; }
				else if ( page == 6 ) { txtName = R( "Retched Air Elixir" ); txtIcon = "11466"; txtInfo = R( "Creates a burst of harmful gas." ); txtSklA = "30"; txtSklB = "25"; txtIngA = R( "Black Sand" ); txtIngB = R( "Grave Dust" ); txtIngC = ""; }
				else if ( page == 7 ) { txtName = R( "Lich Leech Mixture" ); txtIcon = "11464"; txtInfo = R( "Absorbs mana from the target, giving it to you in return." ); txtSklA = "35"; txtSklB = "30"; txtIngA = R( "Dried Toad" ); txtIngB = R( "Red Lotus" ); txtIngC = ""; }
				else if ( page == 8 ) { txtName = R( "Wall of Spike Draught" ); txtIcon = "11470"; txtInfo = R( "Creates a protective wall of spikes." ); txtSklA = "40"; txtSklB = "35"; txtIngA = R( "Bitter Root" ); txtIngB = R( "Pig Iron" ); txtIngC = ""; }
				else if ( page == 9 ) { txtName = R( "Disease Curing Concoction" ); txtIcon = "11459"; txtInfo = R( "Cures one of poisonous diseases." ); txtSklA = "45"; txtSklB = "40"; txtIngA = R( "Wolfsbane" ); txtIngB = R( "Swamp Berries" ); txtIngC = ""; }
				else if ( page == 10 ) { txtName = R( "Blood Pact Elixir" ); txtIcon = "11456"; txtInfo = R( "Takes some of your life and bestows it upon another." ); txtSklA = "50"; txtSklB = "45"; txtIngA = R( "Blood Rose" ); txtIngB = R( "Daemon Blood" ); txtIngC = ""; }
				else if ( page == 11 ) { txtName = R( "Spectre Shadow Elixir" ); txtIcon = "11467"; txtInfo = R( "Turns the body into an invisible ghostly form that cannot be seen." ); txtSklA = "55"; txtSklB = "50"; txtIngA = R( "Violet Fungus" ); txtIngB = R( "Silver Widow" ); txtIngC = ""; }
				else if ( page == 12 ) { txtName = R( "Ghost Phase Concoction" ); txtIcon = "11461"; txtInfo = R( "Turns your body into ghostly matter that reappears in a nearby location." ); txtSklA = "60"; txtSklB = "55"; txtIngA = R( "Bitter Root" ); txtIngB = R( "Moon Crystal" ); txtIngC = ""; }
				else if ( page == 13 ) { txtName = R( "Demonic Fire Ooze" ); txtIcon = "11457"; txtInfo = R( "Ignites a marked rune with power to transport one to that location." ); txtSklA = "65"; txtSklB = "60"; txtIngA = R( "Maggot" ); txtIngB = R( "Black Pearl" ); txtIngC = ""; }
				else if ( page == 14 ) { txtName = R( "Ghostly Images Draught" ); txtIcon = "11462"; txtInfo = R( "Creates an illusionary image of you, distracting your foes." ); txtSklA = "70"; txtSklB = "65"; txtIngA = R( "Mummy Wrap" ); txtIngB = R( "Bloodmoss" ); txtIngC = ""; }
				else if ( page == 15 ) { txtName = R( "Hellish Branding Ooze" ); txtIcon = "11463"; txtInfo = R( "Marks a rune location with symbols of evil, so you can use recalling magic on it to return to that location." ); txtSklA = "75"; txtSklB = "70"; txtIngA = R( "Werewolf Claw" ); txtIngB = R( "Brimstone" ); txtIngC = ""; }
				else if ( page == 16 ) { txtName = R( "Black Gate Draught" ); txtIcon = "11455"; txtInfo = R( "Uses a magic rune to create a horrific black gate. Those that enter will appear at the runic location." ); txtSklA = "80"; txtSklB = "75"; txtIngA = R( "Black Sand" ); txtIngB = R( "Wolfsbane" ); txtIngC = R( "Pixie Skull" ); }
				else if ( page == 17 ) { txtName = R( "Vampire Blood Draught" ); txtIcon = "11469"; txtInfo = R( "Dumps vampire blood in your pack, that will resurrect you a few moments after losing your life. You can also directly resurrect others." ); txtSklA = "85"; txtSklB = "80"; txtIngA = R( "Werewolf Claw" ); txtIngB = R( "Bat Wing" ); txtIngC = R( "Blood Rose" ); }

				if ( val == 1 )
					return txtName;
				else if ( val == 2 )
					return txtIcon;
				else if ( val == 3 )
					return txtInfo;
				else if ( val == 4 )
					return txtSklA;
				else if ( val == 5 )
					return txtSklB;
				else if ( val == 6 )
					return txtIngA;
				else if ( val == 7 )
					return txtIngB;

				return txtIngC;
			}
		}

		public override void OnDoubleClick( Mobile e )
		{
			if ( !IsChildOf( e.Backpack ) )
			{
				e.SendMessage( StringCatalog.Resolve( e.Account, "This must be in your backpack to read." ) );
			}
			else
			{
				e.CloseGump( typeof( BookGump ) );
				e.SendGump( new BookGump( e, 1 ) );
			}
		}

		public BookWitchBrewing( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			/*int version = */reader.ReadInt();
		}
	}
}
