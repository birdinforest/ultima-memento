using System;
using Server;
using Server.Items;
using System.Text;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;
using Server.Localization;

namespace Server.Items
{
	public class LearnReagentsBook : Item
	{
		public override bool IsContentLocalized => true;

		[Constructable]
		public LearnReagentsBook( ) : base( 0x02DD )
		{
			Weight = 1.0;
			Name = "Scroll of Various Reagents";
			ItemID = Utility.RandomList( 0x02DD, 0x201A );
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( BuildingPropertyListLocale != null )
				list.Add( StringCatalog.TryResolve( BuildingPropertyListLocale, "A Listing Of Reagents" ) ?? "A Listing Of Reagents" );
			else
				list.Add( "A Listing Of Reagents" );
		}

		public class LearnReagentsGump : Gump
		{
			public LearnReagentsGump( Mobile from ): base( 50, 50 )
			{
				string color = "#ddbc4b";

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 9546, Server.Misc.PlayerSettings.GetGumpHue( from ));

				AddHtml( 15, 15, 600, 20, TradesBookLocalization.Body( from, color, "INFORMATION ON VARIOUS REAGENTS" ), (bool)false, (bool)false);

				AddButton(867, 11, 4017, 4017, 0, GumpButtonType.Reply, 0);

				AddItem(13, 96, 9839);
				AddItem(1, 124, 3963);
				AddItem(5, 157, 3972);
				AddItem(-2, 178, 3973);
				AddItem(1, 213, 3974);
				AddItem(1, 237, 3976);
				AddItem(5, 271, 3981);
				AddItem(-1, 302, 3980);


				AddItem(3, 390, 3960);
				AddItem(4, 420, 3965);
				AddItem(2, 454, 3983);
				AddItem(13, 481, 3982);
				AddItem(4, 509, 3978);


				AddItem(749, 88, 12280);
				AddItem(749, 121, 12243);
				AddItem(746, 148, 12290);
				AddItem(749, 184, 12250);
				AddItem(750, 211, 12251);
				AddItem(749, 238, 12249);
				AddItem(747, 265, 12291);
				AddItem(748, 298, 12257);
				AddItem(750, 330, 12264);
				AddItem(750, 360, 12265);
				AddItem(750, 388, 12279);
				AddItem(751, 420, 12256);


				AddItem(761, 511, 25612);
				AddItem(759, 540, 25613);
				AddItem(758, 568, 25614);
				AddItem(751, 595, 25615);
				AddItem(755, 625, 25616);
				AddItem(759, 664, 25617);
				AddItem(764, 690, 25618);
				AddItem(762, 718, 25619);
				AddItem(752, 739, 25620);

				int i = 45;
				int o = 790;

				AddHtml( 15, 60, 100, 20, TradesBookLocalization.Body( from, color, "COMMON" ), (bool)false, (bool)false);
				AddHtml( i, 90, 100, 20, TradesBookLocalization.Body( from, color, "Black Pearl" ), (bool)false, (bool)false);
				AddHtml( i, 120, 100, 20, TradesBookLocalization.Body( from, color, "Bloodmoss" ), (bool)false, (bool)false);
				AddHtml( i, 150, 100, 20, TradesBookLocalization.Body( from, color, "Garlic" ), (bool)false, (bool)false);
				AddHtml( i, 180, 100, 20, TradesBookLocalization.Body( from, color, "Ginseng" ), (bool)false, (bool)false);
				AddHtml( i, 210, 100, 20, TradesBookLocalization.Body( from, color, "Mandrake Root" ), (bool)false, (bool)false);
				AddHtml( i, 240, 100, 20, TradesBookLocalization.Body( from, color, "Nightshade" ), (bool)false, (bool)false);
				AddHtml( i, 270, 100, 20, TradesBookLocalization.Body( from, color, "Spider Silk" ), (bool)false, (bool)false);
				AddHtml( i, 300, 100, 20, TradesBookLocalization.Body( from, color, "Sulfurous Ash" ), (bool)false, (bool)false);

				AddHtml( 15, 360, 100, 20, TradesBookLocalization.Body( from, color, "NECROMANCY" ), (bool)false, (bool)false);
				AddHtml( i, 390, 100, 20, TradesBookLocalization.Body( from, color, "Bat Wing" ), (bool)false, (bool)false);
				AddHtml( i, 420, 100, 20, TradesBookLocalization.Body( from, color, "Daemon Blood" ), (bool)false, (bool)false);
				AddHtml( i, 450, 100, 20, TradesBookLocalization.Body( from, color, "Grave Dust" ), (bool)false, (bool)false);
				AddHtml( i, 480, 100, 20, TradesBookLocalization.Body( from, color, "Nox Crystal" ), (bool)false, (bool)false);
				AddHtml( i, 510, 100, 20, TradesBookLocalization.Body( from, color, "Pig Iron" ), (bool)false, (bool)false);

				AddHtml( 760, 60, 100, 20, TradesBookLocalization.Body( from, color, "HERBALIST" ), (bool)false, (bool)false);
				AddHtml( o, 90, 100, 20, TradesBookLocalization.Body( from, color, "Beetle Shell" ), (bool)false, (bool)false);
				AddHtml( o, 120, 100, 20, TradesBookLocalization.Body( from, color, "Brimstone" ), (bool)false, (bool)false);
				AddHtml( o, 150, 100, 20, TradesBookLocalization.Body( from, color, "Butterfly Wings" ), (bool)false, (bool)false);
				AddHtml( o, 180, 100, 20, TradesBookLocalization.Body( from, color, "Eye of Toad" ), (bool)false, (bool)false);
				AddHtml( o, 210, 100, 20, TradesBookLocalization.Body( from, color, "Fairy Egg" ), (bool)false, (bool)false);
				AddHtml( o, 240, 100, 20, TradesBookLocalization.Body( from, color, "Gargoyle Ear" ), (bool)false, (bool)false);
				AddHtml( o, 270, 100, 20, TradesBookLocalization.Body( from, color, "Moon Crystal" ), (bool)false, (bool)false);
				AddHtml( o, 300, 100, 20, TradesBookLocalization.Body( from, color, "Pixie Skull" ), (bool)false, (bool)false);
				AddHtml( o, 330, 100, 20, TradesBookLocalization.Body( from, color, "Red Lotus" ), (bool)false, (bool)false);
				AddHtml( o, 360, 100, 20, TradesBookLocalization.Body( from, color, "Sea Salt" ), (bool)false, (bool)false);
				AddHtml( o, 390, 100, 20, TradesBookLocalization.Body( from, color, "Silver Widow" ), (bool)false, (bool)false);
				AddHtml( o, 420, 100, 20, TradesBookLocalization.Body( from, color, "Swamp Berries" ), (bool)false, (bool)false);

				AddHtml( 760, 480, 100, 20, TradesBookLocalization.Body( from, color, "WITCHERY" ), (bool)false, (bool)false);
				AddHtml( o, 510, 100, 20, TradesBookLocalization.Body( from, color, "Bitter Root" ), (bool)false, (bool)false);
				AddHtml( o, 540, 100, 20, TradesBookLocalization.Body( from, color, "Black Sand" ), (bool)false, (bool)false);
				AddHtml( o, 570, 100, 20, TradesBookLocalization.Body( from, color, "Blood Rose" ), (bool)false, (bool)false);
				AddHtml( o, 600, 100, 20, TradesBookLocalization.Body( from, color, "Dried Toad" ), (bool)false, (bool)false);
				AddHtml( o, 630, 100, 20, TradesBookLocalization.Body( from, color, "Maggot" ), (bool)false, (bool)false);
				AddHtml( o, 660, 100, 20, TradesBookLocalization.Body( from, color, "Mummy Wrap" ), (bool)false, (bool)false);
				AddHtml( o, 690, 100, 20, TradesBookLocalization.Body( from, color, "Violet Fungus" ), (bool)false, (bool)false);
				AddHtml( o, 720, 100, 20, TradesBookLocalization.Body( from, color, "Werewolf Claw" ), (bool)false, (bool)false);
				AddHtml( o, 750, 100, 20, TradesBookLocalization.Body( from, color, "Wolfsbane" ), (bool)false, (bool)false);

				AddHtml( 237, 62, 434, 707, TradesBookLocalization.Body( from, color, "Mages, necromancers, witches, alchemists, and druids all use reagents of some type. This is a listing of the kinds of reagents you may find while traveling the world. This is not a complete list, as legends and rumors tell of other types that may exist. Reagents can be purchased from merchants, picked from some gardens, found on some creatures, or discovered with other treasure. One will commonly find reagents that may be used in their current trade, but if they have no such trade, then they may find any type that they can leave behind or sell.<br><br>Mages use the common reagents in the casting of spells, where necromancers use the necromancy reagents for their magic. Witches and druids use some of these reagents listed here as well.<br><br>You may find many reagents you need to identify. If you have practiced your tasting, you will able to discover what these are. Mages and necromancers simply carry the reagents with them to use as the cast spells. Alchemists use a mortar and pestle to create potions, while witches and druids use cauldrons." ), (bool)false, (bool)false);
			}

			public override void OnResponse( NetState state, RelayInfo info ) 
			{
				Mobile from = state.Mobile;
				from.SendSound( 0x249 );
			}
		}

		public override void OnDoubleClick( Mobile e )
		{
			if ( !IsChildOf( e.Backpack ) && this.Weight != -50.0 ) 
			{
				e.SendMessage( TradesBookLocalization.Resolve( e, "This must be in your backpack to read." ) );
			}
			else
			{
				e.CloseGump( typeof( LearnReagentsGump ) );
				e.SendGump( new LearnReagentsGump( e ) );
				e.PlaySound( 0x249 );
				Server.Gumps.MyLibrary.readBook ( this, e );
			}
		}

		public LearnReagentsBook(Serial serial) : base(serial)
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