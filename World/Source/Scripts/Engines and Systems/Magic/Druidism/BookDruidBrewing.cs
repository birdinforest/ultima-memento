using System;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Localization;

namespace Server.Items
{
	public class BookDruidBrewing : Item
	{
		[Constructable]
		public BookDruidBrewing() : base( 0x5688 )
		{
			Weight = 1.0;
			Name = "Druidic Herbalism";
			Hue = 0x85D;
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

				AddImage( 0, 0, 7005, 2936 );
				AddImage( 0, 0, 7006 );
				AddImage( 0, 0, 7024, 2736 );
				AddImage( 77, 98, 7054 );
				AddImage( 368, 98, 7054 );

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
					AddHtml( 107, 46, 186, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080><CENTER>DRUIDIC HERBALISM</CENTER></BASEFONT></BODY>" ), false, false );
					AddHtml( 398, 48, 186, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080><CENTER>DRUIDIC HERBALISM</CENTER></BASEFONT></BODY>" ), false, false );

					AddHtml( 78, 75, 248, 318, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080>Druidic Herbalism is the art of taking natural reagents and creating mixtures that druids can use. You would use your druidism skill to create and use the potions, but some veterinary is needed to help create them and make them more effective in some cases. This book explains the various potions you can make, as well as additional information to manage these mixtures effectively. Unlike other potions, these require jars as the liquid needs a thicker glass to store as it is acidic enough to dissolve bottle glass and even the wood of a keg.</BASEFONT></BODY>" ), false, false );

					AddHtml( 372, 75, 248, 318, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080>You will need a small cauldron to brew these potions. You can also get a belt pouch to store the ingredients, cauldrons, jars, potions, and this book to make them easier to carry. Single click this bag to organize it for easier use of the potions.</BASEFONT></BODY>" ), false, false );
				}
				else
				{
					AddHtml( 107, 46, 186, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080><CENTER>{0}</CENTER></BASEFONT></BODY>", potionInfo( potion, 1 ) ), false, false );

					AddHtml( 73, 72, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080>Druidism:</BASEFONT></BODY>" ), false, false );
					AddHtml( 267, 72, 47, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 4 ) ), false, false );

					AddHtml( 73, 98, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080>Veterinary:</BASEFONT></BODY>" ), false, false );
					AddHtml( 267, 98, 47, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 5 ) ), false, false );

					AddImage( 77, 128, Int32.Parse( potionInfo( potion, 2 ) ) );
					AddHtml( 133, 139, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080>Ingredients</BASEFONT></BODY>" ), false, false );

					AddHtml( 73, 180, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 6 ) ), false, false );
					AddHtml( 73, 206, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 7 ) ), false, false );
					AddHtml( 73, 232, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 8 ) ), false, false );

					AddHtml( 73, 258, 245, 133, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 3 ) ), false, false );

					potion++;

					AddHtml( 398, 48, 186, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080><CENTER>{0}</CENTER></BASEFONT></BODY>", potionInfo( potion, 1 ) ), false, false );

					AddHtml( 366, 72, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080>Druidism:</BASEFONT></BODY>" ), false, false );
					AddHtml( 560, 72, 47, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 4 ) ), false, false );

					AddHtml( 366, 98, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080>Veterinary:</BASEFONT></BODY>" ), false, false );
					AddHtml( 560, 98, 47, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 5 ) ), false, false );

					AddImage( 366, 128, Int32.Parse( potionInfo( potion, 2 ) ) );
					AddHtml( 422, 139, 187, 20, StringCatalog.Resolve( m_From.Account, @"<BODY><BASEFONT Color=#80d080>Ingredients</BASEFONT></BODY>" ), false, false );

					AddHtml( 366, 180, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 6 ) ), false, false );
					AddHtml( 366, 206, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 7 ) ), false, false );
					AddHtml( 366, 232, 246, 20, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 8 ) ), false, false );

					AddHtml( 366, 258, 245, 133, StringCatalog.ResolveFormat( m_From.Account, @"<BODY><BASEFONT Color=#80d080>{0}</BASEFONT></BODY>", potionInfo( potion, 3 ) ), false, false );
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
					case 2: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Dumps out a magical stone that draws all nearby animals to it. To use it, one should have a 10 in Druidism and a 5 in Veterinary." );
					case 3: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Turns one into flower petals and carries them on the wind to a magic rune location. To use it, one should have a 15 in Druidism and a 10 in Veterinary." );
					case 4: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Causes a wall of foliage to grow, blocking the way of others. To use it, one should have a 20 in Druidism and a 15 in Veterinary." );
					case 5: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Increases your protection by making your skin like bark from an ancient tree. To use it, one should have a 25 in Druidism and a 20 in Veterinary." );
					case 6: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Causes stones to push up from the ground, trapping your foes. To use it, one should have a 30 in Druidism and a 25 in Veterinary." );
					case 7: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Releases roots from the ground to entangle a foe. To use it, one should have a 35 in Druidism and a 30 in Veterinary." );
					case 8: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Marks a magic rune with your location, that you can use recalling magics to transport to later. To use it, one should have a 40 in Druidism and a 35 in Veterinary." );
					case 9: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Heals the target of all ailments. To use it, one should have a 45 in Druidism and a 40 in Veterinary." );
					case 10: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Allows one to blend seamlessly with the forest, causing foes to lose sight of them. To use it, one should have a 50 in Druidism and a 45 in Veterinary." );
					case 11: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Releases fireflies to distract a foe from battle. To use it, one should have a 55 in Druidism and a 50 in Veterinary." );
					case 12: return StringCatalog.Resolve( account, "This is created from druidic herbalism: using a magical rune, this liquid causes magical mushrooms to grow a portal to the runic location. To use it, one should have a 60 in Druidism and a 55 in Veterinary." );
					case 13: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Releases a swarm of insects from the jar that bite and sting nearby foes. To use it, one should have a 65 in Druidism and a 60 in Veterinary." );
					case 14: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Releases a fairy from the jar to a help the adventurer on their journey. To use it, one should have a 70 in Druidism and a 65 in Veterinary." );
					case 15: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Causes a living tree to grow and wander along with the you. To use it, one should have a 75 in Druidism and a 70 in Veterinary." );
					case 16: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Causes molten lava to burst from the ground, hitting every foe nearby. To use it, one should have a 80 in Druidism and a 75 in Veterinary." );
					case 17: return StringCatalog.Resolve( account, "This is created from druidic herbalism: Dumps mystical mud in your pack, that will resurrect you a few moments after losing your life. You can also directly resurrect others. To use it, one should have a 85 in Druidism and a 80 in Veterinary." );
					default: return "";
				}
			}

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

				if ( page == 2 ) { txtName = R( "Stone in a Jar" ); txtIcon = "11446"; txtInfo = R( "Dumps out a magical stone that draws all nearby animals to it." ); txtSklA = "10"; txtSklB = "5"; txtIngA = R( "Moon Crystal" ); txtIngB = R( "Silver Widow" ); txtIngC = ""; }
				else if ( page == 3 ) { txtName = R( "Nature Passage Mixture" ); txtIcon = "11449"; txtInfo = R( "Turns one into flower petals and carries them on the wind to a magic rune location." ); txtSklA = "15"; txtSklB = "10"; txtIngA = R( "Sea Salt" ); txtIngB = R( "Fairy Egg" ); txtIngC = ""; }
				else if ( page == 4 ) { txtName = R( "Shield of Earth Liquid" ); txtIcon = "11450"; txtInfo = R( "Causes a wall of foliage to grow, blocking the way of others." ); txtSklA = "20"; txtSklB = "15"; txtIngA = R( "Ginseng" ); txtIngB = R( "Black Pearl" ); txtIngC = ""; }
				else if ( page == 5 ) { txtName = R( "Woodland Protection Oil" ); txtIcon = "11454"; txtInfo = R( "Increases your protection by making your skin like bark from an ancient tree." ); txtSklA = "25"; txtSklB = "20"; txtIngA = R( "Garlic" ); txtIngB = R( "Swamp Berries" ); txtIngC = ""; }
				else if ( page == 6 ) { txtName = R( "Stone Rising Concoction" ); txtIcon = "11451"; txtInfo = R( "Causes stones to push up from the ground, trapping your foes." ); txtSklA = "30"; txtSklB = "25"; txtIngA = R( "Beetle Shell" ); txtIngB = R( "Sea Salt" ); txtIngC = ""; }
				else if ( page == 7 ) { txtName = R( "Grasping Roots Mixture" ); txtIcon = "11443"; txtInfo = R( "Releases roots from the ground to entangle a foe." ); txtSklA = "35"; txtSklB = "30"; txtIngA = R( "Mandrake Root" ); txtIngB = R( "Ginseng" ); txtIngC = ""; }
				else if ( page == 8 ) { txtName = R( "Druidic Marking Oil" ); txtIcon = "11439"; txtInfo = R( "Marks a magic rune with your location, that you can use recalling magics to transport to later." ); txtSklA = "40"; txtSklB = "35"; txtIngA = R( "Black Pearl" ); txtIngB = R( "Eye of Toad" ); txtIngC = ""; }
				else if ( page == 9 ) { txtName = R( "Herbal Healing Elixir" ); txtIcon = "11444"; txtInfo = R( "Heals the target of all ailments." ); txtSklA = "45"; txtSklB = "40"; txtIngA = R( "Red Lotus" ); txtIngB = R( "Garlic" ); txtIngC = ""; }
				else if ( page == 10 ) { txtName = R( "Forest Blending Oil" ); txtIcon = "11442"; txtInfo = R( "Allows one to blend seamlessly with the forest, causing foes to lose sight of them." ); txtSklA = "50"; txtSklB = "45"; txtIngA = R( "Silver Widow" ); txtIngB = R( "Nightshade" ); txtIngC = ""; }
				else if ( page == 11 ) { txtName = R( "Jar of Fireflies" ); txtIcon = "11445"; txtInfo = R( "Releases fireflies to distract a foe from battle." ); txtSklA = "55"; txtSklB = "50"; txtIngA = R( "Spider Silk" ); txtIngB = R( "Butterfly Wings" ); txtIngC = ""; }
				else if ( page == 12 ) { txtName = R( "Mushroom Gateway Growth" ); txtIcon = "11448"; txtInfo = R( "using a magical rune, this liquid causes magical mushrooms to grow a portal to the runic location." ); txtSklA = "60"; txtSklB = "55"; txtIngA = R( "Bloodmoss" ); txtIngB = R( "Eye of Toad" ); txtIngC = ""; }
				else if ( page == 13 ) { txtName = R( "Jar of Insects" ); txtIcon = "11441"; txtInfo = R( "Releases a swarm of insects from the jar that bite and sting nearby foes." ); txtSklA = "65"; txtSklB = "60"; txtIngA = R( "Butterfly Wings" ); txtIngB = R( "Beetle Shell" ); txtIngC = ""; }
				else if ( page == 14 ) { txtName = R( "Fairy in a Jar" ); txtIcon = "11440"; txtInfo = R( "Releases a fairy from the jar to a help the adventurer on their journey." ); txtSklA = "70"; txtSklB = "65"; txtIngA = R( "Fairy Egg" ); txtIngB = R( "Moon Crystal" ); txtIngC = ""; }
				else if ( page == 15 ) { txtName = R( "Treant Fertilizer" ); txtIcon = "11452"; txtInfo = R( "Causes a living tree to grow and wander along with the you." ); txtSklA = "75"; txtSklB = "70"; txtIngA = R( "Swamp Berries" ); txtIngB = R( "Mandrake Root" ); txtIngC = ""; }
				else if ( page == 16 ) { txtName = R( "Volcanic Fluid" ); txtIcon = "11453"; txtInfo = R( "Causes molten lava to burst from the ground, hitting every foe nearby." ); txtSklA = "80"; txtSklB = "75"; txtIngA = R( "Brimstone" ); txtIngB = R( "Sulfurous Ash" ); txtIngC = ""; }
				else if ( page == 17 ) { txtName = R( "Jar of Magical Mud" ); txtIcon = "11447"; txtInfo = R( "Dumps mystical mud in your pack, that will resurrect you a few moments after losing your life. You can also directly resurrect others." ); txtSklA = "85"; txtSklB = "80"; txtIngA = R( "Nightshade" ); txtIngB = R( "Red Lotus" ); txtIngC = ""; }

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

		public BookDruidBrewing( Serial serial ) : base( serial )
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
