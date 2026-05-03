using System;
using Server;
using Server.Items;
using System.Text;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Misc;
using Server.Localization;
using Server.Accounting;

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

			public BookGump( Mobile from, int page ): base( 100, 100 )
			{
				m_From = from;
				string color = "#80d080";
				from.SendSound( 0x55 );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 7005, 2936);
				AddImage(0, 0, 7006);
				AddImage(0, 0, 7024, 2736);
				AddImage(77, 98, 7054);
				AddImage(368, 98, 7054);

				int prev = page - 1;
					if ( prev < 1 ){ prev = 99; }
				int next = page + 1;

				AddButton(72, 45, 4014, 4014, prev, GumpButtonType.Reply, 0);
				AddButton(590, 48, 4005, 4005, next, GumpButtonType.Reply, 0);

				int potion = 0;

				if ( page == 2 ){ potion = 2; }
				else if ( page == 3 ){ potion = 4; }
				else if ( page == 4 ){ potion = 6; }
				else if ( page == 5 ){ potion = 8; }
				else if ( page == 6 ){ potion = 10; }
				else if ( page == 7 ){ potion = 11; }
				else if ( page == 8 ){ potion = 12; }
				else if ( page == 9 ){ potion = 14; }
				else if ( page == 10 ){ potion = 16; }

				// --------------------------------------------------------------------------------

				if ( page == 1 )
				{
					AddHtml( 107, 46, 186, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + StringCatalog.Resolve( from.Account, "DRUIDIC HERBALISM" ) + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 398, 48, 186, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + StringCatalog.Resolve( from.Account, "DRUIDIC HERBALISM" ) + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 78, 75, 248, 318, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Druidic Herbalism is the art of taking natural reagents and creating mixtures that druids can use. You would use your druidism skill to create and use the potions, but some veterinary is needed to help create them and make them more effective in some cases. This book explains the various potions you can make, as well as additional information to manage these mixtures effectively. Unlike other potions, these require jars as the liquid needs a thicker glass to store as it is acidic enough to dissolve bottle glass and even the wood of a keg." ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 372, 75, 248, 318, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "You will need a small cauldron to brew these potions. You can also get a belt pouch to store the ingredients, cauldrons, jars, potions, and this book to make them easier to carry. Single click this bag to organize it for easier use of the potions." ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
				}
				else
				{
					AddHtml( 107, 46, 186, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + PotionInfo( potion, 1 ) + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 73, 72, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Druidism:" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 267, 72, 47, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 4 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 73, 98, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Veterinary:" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 267, 98, 47, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 5 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddImage(77, 128, Int32.Parse( PotionInfo( potion, 2 ) ) );
					AddHtml( 133, 139, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Ingredients" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 73, 180, 246, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 6 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 73, 206, 246, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 7 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 73, 232, 246, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 8 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 73, 258, 245, 133, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 3 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

					potion++;

					AddHtml( 398, 48, 186, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + PotionInfo( potion, 1 ) + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 366, 72, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Druidism:" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 560, 72, 47, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 4 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 366, 98, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Veterinary:" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 560, 98, 47, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 5 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddImage(366, 128, Int32.Parse( PotionInfo( potion, 2 ) ) );
					AddHtml( 422, 139, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Ingredients" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 366, 180, 246, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 6 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 366, 206, 246, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 7 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 366, 232, 246, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 8 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 366, 258, 245, 133, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 3 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
				}
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state.Mobile;
				int page = info.ButtonID;
					if ( page == 99 ){ page = 9; }
					else if ( page > 9 ){ page = 1; }

				if ( info.ButtonID > 0 )
				{
					from.SendGump( new BookGump( from, page ) );
				}
				else
					from.SendSound( 0x55 );
			}

			public static string potionDesc( int potion ) => potionDesc( null, potion );

			public static string potionDesc( IAccount account, int potion )
			{
				string txt = "";

				txt = StringCatalog.Resolve( account, "This is created from druidic herbalism: " ) + PotionInfo( account, potion, 3 ) + StringCatalog.Resolve( account, " To use it, one should have a " ) + PotionInfo( account, potion, 4 ) + StringCatalog.Resolve( account, " in Druidism and a " ) + PotionInfo( account, potion, 5 ) + StringCatalog.Resolve( account, " in Veterinary." );

				return txt;
			}

			public string PotionInfo( int page, int val )
			{
				return PotionInfo( m_From != null ? m_From.Account : null, page, val );
			}

			public static string PotionInfo( IAccount account, int page, int val )
			{
				string txtName = "";
				string txtIcon = "";
				string txtInfo = "";
				string txtSklA = "";
				string txtSklB = "";
				string txtIngA = "";
				string txtIngB = "";
				string txtIngC = "";

				if ( page == 2 ){ txtName = StringCatalog.Resolve( account, "Stone in a Jar" ); txtIcon = "11446"; txtInfo = StringCatalog.Resolve( account, "Dumps out a magical stone that draws all nearby animals to it." ); txtSklA = "10"; txtSklB = "5"; txtIngA = StringCatalog.Resolve( account, "Moon Crystal" ); txtIngB = StringCatalog.Resolve( account, "Silver Widow" ); txtIngC = ""; }
				else if ( page == 3 ){ txtName = StringCatalog.Resolve( account, "Nature Passage Mixture" ); txtIcon = "11449"; txtInfo = StringCatalog.Resolve( account, "Turns one into flower petals and carries them on the wind to a magic rune location." ); txtSklA = "15"; txtSklB = "10"; txtIngA = StringCatalog.Resolve( account, "Sea Salt" ); txtIngB = StringCatalog.Resolve( account, "Fairy Egg" ); txtIngC = ""; }
				else if ( page == 4 ){ txtName = StringCatalog.Resolve( account, "Shield of Earth Liquid" ); txtIcon = "11450"; txtInfo = StringCatalog.Resolve( account, "Causes a wall of foliage to grow, blocking the way of others." ); txtSklA = "20"; txtSklB = "15"; txtIngA = StringCatalog.Resolve( account, "Ginseng" ); txtIngB = StringCatalog.Resolve( account, "Black Pearl" ); txtIngC = ""; }
				else if ( page == 5 ){ txtName = StringCatalog.Resolve( account, "Woodland Protection Oil" ); txtIcon = "11454"; txtInfo = StringCatalog.Resolve( account, "Increases your protection by making your skin like bark from an ancient tree." ); txtSklA = "25"; txtSklB = "20"; txtIngA = StringCatalog.Resolve( account, "Garlic" ); txtIngB = StringCatalog.Resolve( account, "Swamp Berries" ); txtIngC = ""; }
				else if ( page == 6 ){ txtName = StringCatalog.Resolve( account, "Stone Rising Concoction" ); txtIcon = "11451"; txtInfo = StringCatalog.Resolve( account, "Causes stones to push up from the ground, trapping your foes." ); txtSklA = "30"; txtSklB = "25"; txtIngA = StringCatalog.Resolve( account, "Beetle Shell" ); txtIngB = StringCatalog.Resolve( account, "Sea Salt" ); txtIngC = ""; }
				else if ( page == 7 ){ txtName = StringCatalog.Resolve( account, "Grasping Roots Mixture" ); txtIcon = "11443"; txtInfo = StringCatalog.Resolve( account, "Releases roots from the ground to entangle a foe." ); txtSklA = "35"; txtSklB = "30"; txtIngA = StringCatalog.Resolve( account, "Mandrake Root" ); txtIngB = StringCatalog.Resolve( account, "Ginseng" ); txtIngC = ""; }
				else if ( page == 8 ){ txtName = StringCatalog.Resolve( account, "Druidic Marking Oil" ); txtIcon = "11439"; txtInfo = StringCatalog.Resolve( account, "Marks a magic rune with your location, that you can use recalling magics to transport to later." ); txtSklA = "40"; txtSklB = "35"; txtIngA = StringCatalog.Resolve( account, "Black Pearl" ); txtIngB = StringCatalog.Resolve( account, "Eye of Toad" ); txtIngC = ""; }
				else if ( page == 9 ){ txtName = StringCatalog.Resolve( account, "Herbal Healing Elixir" ); txtIcon = "11444"; txtInfo = StringCatalog.Resolve( account, "Heals the target of all ailments." ); txtSklA = "45"; txtSklB = "40"; txtIngA = StringCatalog.Resolve( account, "Red Lotus" ); txtIngB = StringCatalog.Resolve( account, "Garlic" ); txtIngC = ""; }
				else if ( page == 10 ){ txtName = StringCatalog.Resolve( account, "Forest Blending Oil" ); txtIcon = "11442"; txtInfo = StringCatalog.Resolve( account, "Allows one to blend seamlessly with the forest, causing foes to lose sight of them." ); txtSklA = "50"; txtSklB = "45"; txtIngA = StringCatalog.Resolve( account, "Silver Widow" ); txtIngB = StringCatalog.Resolve( account, "Nightshade" ); txtIngC = ""; }
				else if ( page == 11 ){ txtName = StringCatalog.Resolve( account, "Jar of Fireflies" ); txtIcon = "11445"; txtInfo = StringCatalog.Resolve( account, "Releases fireflies to distract a foe from battle." ); txtSklA = "55"; txtSklB = "50"; txtIngA = StringCatalog.Resolve( account, "Spider Silk" ); txtIngB = StringCatalog.Resolve( account, "Butterfly Wings" ); txtIngC = ""; }
				else if ( page == 12 ){ txtName = StringCatalog.Resolve( account, "Mushroom Gateway Growth" ); txtIcon = "11448"; txtInfo = StringCatalog.Resolve( account, "using a magical rune, this liquid causes magical mushrooms to grow a portal to the runic location." ); txtSklA = "60"; txtSklB = "55"; txtIngA = StringCatalog.Resolve( account, "Bloodmoss" ); txtIngB = StringCatalog.Resolve( account, "Eye of Toad" ); txtIngC = ""; }
				else if ( page == 13 ){ txtName = StringCatalog.Resolve( account, "Jar of Insects" ); txtIcon = "11441"; txtInfo = StringCatalog.Resolve( account, "Releases a swarm of insects from the jar that bite and sting nearby foes." ); txtSklA = "65"; txtSklB = "60"; txtIngA = StringCatalog.Resolve( account, "Butterfly Wings" ); txtIngB = StringCatalog.Resolve( account, "Beetle Shell" ); txtIngC = ""; }
				else if ( page == 14 ){ txtName = StringCatalog.Resolve( account, "Fairy in a Jar" ); txtIcon = "11440"; txtInfo = StringCatalog.Resolve( account, "Releases a fairy from the jar to a help the adventurer on their journey." ); txtSklA = "70"; txtSklB = "65"; txtIngA = StringCatalog.Resolve( account, "Fairy Egg" ); txtIngB = StringCatalog.Resolve( account, "Moon Crystal" ); txtIngC = ""; }
				else if ( page == 15 ){ txtName = StringCatalog.Resolve( account, "Treant Fertilizer" ); txtIcon = "11452"; txtInfo = StringCatalog.Resolve( account, "Causes a living tree to grow and wander along with the you." ); txtSklA = "75"; txtSklB = "70"; txtIngA = StringCatalog.Resolve( account, "Swamp Berries" ); txtIngB = StringCatalog.Resolve( account, "Mandrake Root" ); txtIngC = ""; }
				else if ( page == 16 ){ txtName = StringCatalog.Resolve( account, "Volcanic Fluid" ); txtIcon = "11453"; txtInfo = StringCatalog.Resolve( account, "Causes molten lava to burst from the ground, hitting every foe nearby." ); txtSklA = "80"; txtSklB = "75"; txtIngA = StringCatalog.Resolve( account, "Brimstone" ); txtIngB = StringCatalog.Resolve( account, "Sulfurous Ash" ); txtIngC = ""; }
				else if ( page == 17 ){ txtName = StringCatalog.Resolve( account, "Jar of Magical Mud" ); txtIcon = "11447"; txtInfo = StringCatalog.Resolve( account, "Dumps mystical mud in your pack, that will resurrect you a few moments after losing your life. You can also directly resurrect others." ); txtSklA = "85"; txtSklB = "80"; txtIngA = StringCatalog.Resolve( account, "Nightshade" ); txtIngB = StringCatalog.Resolve( account, "Red Lotus" ); txtIngC = ""; }

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

		public BookDruidBrewing(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
