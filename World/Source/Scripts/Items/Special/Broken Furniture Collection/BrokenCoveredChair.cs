using System;

namespace Server.Items
{
	[Flipable( 0xC17, 0xC18 )]
	public class BrokenCoveredChairComponent : AddonComponent
	{
		public override int LabelNumber { get { return 1076257; } } // Broken Covered Chair

		public BrokenCoveredChairComponent() : base( 0xC17 )
		{
		}

		public BrokenCoveredChairComponent( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}

	public class BrokenCoveredChairAddon : BaseAddon
	{
		public override BaseAddonDeed Deed { get { return new BrokenCoveredChairDeed(); } }

		[Constructable]
		public BrokenCoveredChairAddon() : base()
		{
			AddComponent( new BrokenCoveredChairComponent(), 0, 0, 0 );
		}

		public BrokenCoveredChairAddon( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}

	public class BrokenCoveredChairDeed : BaseAddonDeed
	{
		public override bool IsContentLocalized => true;

		public override BaseAddon Addon { get { return new BrokenCoveredChairAddon(); } }
		public override int LabelNumber { get { return 1076257; } } // Broken Covered Chair

		[Constructable]
		public BrokenCoveredChairDeed() : base()
		{
			LootType = LootType.Blessed;
			ItemID = 0x3F26;
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
			{
				if ( Amount <= 1 )
					AddLocalizedProperty( list, "item.special.deed.broken.covered.chair" );
				else
					list.Add( 1050039, "{0}\t{1}", Amount, ResolvePropertyText( "item.special.deed.broken.covered.chair" ) );
				return;
			}
			base.AddNameProperty( list );
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.special.brokenfurniture.place.in.home" );
			else
				list.Add( 1049644, "Double Click To Place In Your Home");
        }

		public BrokenCoveredChairDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}
}
