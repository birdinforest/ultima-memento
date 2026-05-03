using System;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Misc;
using Server.Localization;
using Server.Accounting;

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

			public BookGump( Mobile from, int page ): base( 100, 100 )
			{
				m_From = from;
				string color = "#d89191";
				from.SendSound( 0x55 );

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 7005, 2845);
				AddImage(0, 0, 7006);
				AddImage(0, 0, 7024, 2736);
				AddImage(87, 117, 7053);
				AddImage(382, 117, 7053);

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
					AddHtml( 107, 46, 186, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + StringCatalog.Resolve( from.Account, "THE WITCH'S BREW" ) + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 398, 48, 186, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + StringCatalog.Resolve( from.Account, "THE WITCH'S BREW" ) + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 78, 75, 248, 318, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Witchery brewing is the art of taking morbid reagents and creating concoctions that necromancers can use in their dark magics. You would use your forensics skill to create the potions, and your necromancy skill to use them. This book explains the various mixtures you can make, as well as additional information to manage these potions effectively. Unlike other potions, these require jars as the liquid needs a thicker glass to store as it is acidic enough to dissolve bottle glass and even the wood of a keg." ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 372, 75, 248, 318, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "You will need a small cauldron to brew these concoctions. You can also get a belt pouch to store the ingredients, cauldrons, jars, potions, and this book to make them easier to carry. Single click this bag to organize it for easier use of the mixtures." ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
				}
				else
				{
					AddHtml( 107, 46, 186, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + PotionInfo( potion, 1 ) + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 73, 72, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Forensics:" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 267, 72, 47, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 4 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 73, 98, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Necromancy:" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
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

					AddHtml( 366, 72, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Forensics:" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
					AddHtml( 560, 72, 47, 20, @"<BODY><BASEFONT Color=" + color + ">" + PotionInfo( potion, 4 ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

					AddHtml( 366, 98, 187, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.Resolve( from.Account, "Necromancy:" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
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

				txt = StringCatalog.Resolve( account, "This is created from a witch's brew: " ) + PotionInfo( account, potion, 3 ) + StringCatalog.Resolve( account, " To use it, one should have a " ) + PotionInfo( account, potion, 4 ) + StringCatalog.Resolve( account, " in Forensics and a " ) + PotionInfo( account, potion, 5 ) + StringCatalog.Resolve( account, " in Necromancy." );

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

				if ( page == 2 ){ txtName = StringCatalog.Resolve( account, "Eyes of the Dead Mixture" ); txtIcon = "11460"; txtInfo = StringCatalog.Resolve( account, "Gives one the same sight of the undead, where they are able to see in the dark." ); txtSklA = "10"; txtSklB = "5"; txtIngA = StringCatalog.Resolve( account, "Mummy Wrap" ); txtIngB = StringCatalog.Resolve( account, "Eye of Toad" ); txtIngC = ""; }
				else if ( page == 3 ){ txtName = StringCatalog.Resolve( account, "Tomb Raiding Concoction" ); txtIcon = "11468"; txtInfo = StringCatalog.Resolve( account, "Summons the spirits to unlock something for you." ); txtSklA = "15"; txtSklB = "10"; txtIngA = StringCatalog.Resolve( account, "Maggot" ); txtIngB = StringCatalog.Resolve( account, "Beetle Shell" ); txtIngC = ""; }
				else if ( page == 4 ){ txtName = StringCatalog.Resolve( account, "Disease Draught" ); txtIcon = "11458"; txtInfo = StringCatalog.Resolve( account, "Causes one to suffer from a poisonous disease." ); txtSklA = "20"; txtSklB = "15"; txtIngA = StringCatalog.Resolve( account, "Violet Fungus" ); txtIngB = StringCatalog.Resolve( account, "Nox Crystal" ); txtIngC = ""; }
				else if ( page == 5 ){ txtName = StringCatalog.Resolve( account, "Phantasm Elixir" ); txtIcon = "11465"; txtInfo = StringCatalog.Resolve( account, "Summons a spirit to disable a trap." ); txtSklA = "25"; txtSklB = "20"; txtIngA = StringCatalog.Resolve( account, "Dried Toad" ); txtIngB = StringCatalog.Resolve( account, "Gargoyle Ear" ); txtIngC = ""; }
				else if ( page == 6 ){ txtName = StringCatalog.Resolve( account, "Retched Air Elixir" ); txtIcon = "11466"; txtInfo = StringCatalog.Resolve( account, "Creates a burst of harmful gas." ); txtSklA = "30"; txtSklB = "25"; txtIngA = StringCatalog.Resolve( account, "Black Sand" ); txtIngB = StringCatalog.Resolve( account, "Grave Dust" ); txtIngC = ""; }
				else if ( page == 7 ){ txtName = StringCatalog.Resolve( account, "Lich Leech Mixture" ); txtIcon = "11464"; txtInfo = StringCatalog.Resolve( account, "Absorbs mana from the target, giving it to you in return." ); txtSklA = "35"; txtSklB = "30"; txtIngA = StringCatalog.Resolve( account, "Dried Toad" ); txtIngB = StringCatalog.Resolve( account, "Red Lotus" ); txtIngC = ""; }
				else if ( page == 8 ){ txtName = StringCatalog.Resolve( account, "Wall of Spike Draught" ); txtIcon = "11470"; txtInfo = StringCatalog.Resolve( account, "Creates a protective wall of spikes." ); txtSklA = "40"; txtSklB = "35"; txtIngA = StringCatalog.Resolve( account, "Bitter Root" ); txtIngB = StringCatalog.Resolve( account, "Pig Iron" ); txtIngC = ""; }
				else if ( page == 9 ){ txtName = StringCatalog.Resolve( account, "Disease Curing Concoction" ); txtIcon = "11459"; txtInfo = StringCatalog.Resolve( account, "Cures one of poisonous diseases." ); txtSklA = "45"; txtSklB = "40"; txtIngA = StringCatalog.Resolve( account, "Wolfsbane" ); txtIngB = StringCatalog.Resolve( account, "Swamp Berries" ); txtIngC = ""; }
				else if ( page == 10 ){ txtName = StringCatalog.Resolve( account, "Blood Pact Elixir" ); txtIcon = "11456"; txtInfo = StringCatalog.Resolve( account, "Takes some of your life and bestows it upon another." ); txtSklA = "50"; txtSklB = "45"; txtIngA = StringCatalog.Resolve( account, "Blood Rose" ); txtIngB = StringCatalog.Resolve( account, "Daemon Blood" ); txtIngC = ""; }
				else if ( page == 11 ){ txtName = StringCatalog.Resolve( account, "Spectre Shadow Elixir" ); txtIcon = "11467"; txtInfo = StringCatalog.Resolve( account, "Turns the body into an invisible ghostly form that cannot be seen." ); txtSklA = "55"; txtSklB = "50"; txtIngA = StringCatalog.Resolve( account, "Violet Fungus" ); txtIngB = StringCatalog.Resolve( account, "Silver Widow" ); txtIngC = ""; }
				else if ( page == 12 ){ txtName = StringCatalog.Resolve( account, "Ghost Phase Concoction" ); txtIcon = "11461"; txtInfo = StringCatalog.Resolve( account, "Turns your body into ghostly matter that reappears in a nearby location." ); txtSklA = "60"; txtSklB = "55"; txtIngA = StringCatalog.Resolve( account, "Bitter Root" ); txtIngB = StringCatalog.Resolve( account, "Moon Crystal" ); txtIngC = ""; }
				else if ( page == 13 ){ txtName = StringCatalog.Resolve( account, "Demonic Fire Ooze" ); txtIcon = "11457"; txtInfo = StringCatalog.Resolve( account, "Ignites a marked rune with power to transport one to that location." ); txtSklA = "65"; txtSklB = "60"; txtIngA = StringCatalog.Resolve( account, "Maggot" ); txtIngB = StringCatalog.Resolve( account, "Black Pearl" ); txtIngC = ""; }
				else if ( page == 14 ){ txtName = StringCatalog.Resolve( account, "Ghostly Images Draught" ); txtIcon = "11462"; txtInfo = StringCatalog.Resolve( account, "Creates an illusionary image of you, distracting your foes." ); txtSklA = "70"; txtSklB = "65"; txtIngA = StringCatalog.Resolve( account, "Mummy Wrap" ); txtIngB = StringCatalog.Resolve( account, "Bloodmoss" ); txtIngC = ""; }
				else if ( page == 15 ){ txtName = StringCatalog.Resolve( account, "Hellish Branding Ooze" ); txtIcon = "11463"; txtInfo = StringCatalog.Resolve( account, "Marks a rune location with symbols of evil, so you can use recalling magic on it to return to that location." ); txtSklA = "75"; txtSklB = "70"; txtIngA = StringCatalog.Resolve( account, "Werewolf Claw" ); txtIngB = StringCatalog.Resolve( account, "Brimstone" ); txtIngC = ""; }
				else if ( page == 16 ){ txtName = StringCatalog.Resolve( account, "Black Gate Draught" ); txtIcon = "11455"; txtInfo = StringCatalog.Resolve( account, "Uses a magic rune to create a horrific black gate. Those that enter will appear at the runic location." ); txtSklA = "80"; txtSklB = "75"; txtIngA = StringCatalog.Resolve( account, "Black Sand" ); txtIngB = StringCatalog.Resolve( account, "Wolfsbane" ); txtIngC = StringCatalog.Resolve( account, "Pixie Skull" ); }
				else if ( page == 17 ){ txtName = StringCatalog.Resolve( account, "Vampire Blood Draught" ); txtIcon = "11469"; txtInfo = StringCatalog.Resolve( account, "Dumps vampire blood in your pack, that will resurrect you a few moments after losing your life. You can also directly resurrect others." ); txtSklA = "85"; txtSklB = "80"; txtIngA = StringCatalog.Resolve( account, "Werewolf Claw" ); txtIngB = StringCatalog.Resolve( account, "Bat Wing" ); txtIngC = StringCatalog.Resolve( account, "Blood Rose" ); }

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

		public BookWitchBrewing(Serial serial) : base(serial)
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
