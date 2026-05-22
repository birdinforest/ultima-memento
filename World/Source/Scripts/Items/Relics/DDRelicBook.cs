using System;
using Server;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
	public class DDRelicBook : Item, IRelic
	{
		public override void ItemIdentified( bool id )
		{
			m_NotIdentified = id;
			if ( !id )
			{
				ColorHue3 = "FDC844";
			}
		}

		protected override void AddColorText3Property( ObjectPropertyList list, string colorHue3 )
		{
			if ( NotIdentified || CoinPrice <= 0 )
				return;

			string worthText;

			if ( BuildingPropertyListLocale != null )
				worthText = string.Format( ResolvePropertyText( "prop.trade.relic.worth.gold" ), CoinPrice );
			else
				worthText = "Worth " + CoinPrice + " Gold";

			list.Add( 1072173, "{0}\t{1}", colorHue3, worthText );
		}

		[Constructable]
		public DDRelicBook() : base( 0xFBD )
		{
			Weight = 5;
			CoinPrice = Utility.RandomMinMax( 80, 500 );
			NotIdentified = true;
			NotIDSource = Identity.Book;
			NotIDSkill = IDSkill.Mercantile;
			ItemID = RandomThings.GetRandomBookItemID();
			Hue = Utility.RandomColor(0);
			Name = Server.Misc.RandomThings.GetBookTitle();
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) && MySettings.S_IdentifyItemsOnlyInPack && from is PlayerMobile && ((PlayerMobile)from).Preferences.DoubleClickID && NotIdentified ) 
				from.SendMessage( StringCatalog.Resolve( from.Account, "This must be in your backpack to identify." ) );
			else if ( from is PlayerMobile && ((PlayerMobile)from).Preferences.DoubleClickID && NotIdentified )
				IDCommand( from );
		}

		public override void IDCommand( Mobile m )
		{
			if ( this.NotIDSkill == IDSkill.Tasting )
				RelicFunctions.IDItem( m, m, this, SkillName.Tasting );
			else if ( this.NotIDSkill == IDSkill.ArmsLore )
				RelicFunctions.IDItem( m, m, this, SkillName.ArmsLore );
			else
				RelicFunctions.IDItem( m, m, this, SkillName.Mercantile );
		}

		public DDRelicBook(Serial serial) : base(serial)
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
            writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
            int version = reader.ReadInt();

			if ( version < 1 )
				CoinPrice = reader.ReadInt();

			ColorText3 = null;
		}
	}
}