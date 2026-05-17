using System;

namespace Server.Items
{
	[Flipable( 0xC12, 0xC13 )]
	public class BrokenArmoireComponent : AddonComponent
	{
		public override int LabelNumber { get { return 1076262; } } // Broken Armoire

		public BrokenArmoireComponent() : base( 0xC12 )
		{
		}

		public BrokenArmoireComponent( Serial serial ) : base( serial )
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

	public class BrokenArmoireAddon : BaseAddon
	{
		public override BaseAddonDeed Deed { get { return new BrokenArmoireDeed(); } }

		[Constructable]
		public BrokenArmoireAddon() : base()
		{
			AddComponent( new BrokenArmoireComponent(), 0, 0, 0 );
		}

		public BrokenArmoireAddon( Serial serial ) : base( serial )
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

	public class BrokenArmoireDeed : BaseAddonDeed
	{
		public override bool IsContentLocalized => true;

		public override BaseAddon Addon { get { return new BrokenArmoireAddon(); } }
		public override int LabelNumber { get { return 1076262; } } // Broken Armoire

		[Constructable]
		public BrokenArmoireDeed() : base()
		{
			ItemID = 0x3F21;
			LootType = LootType.Blessed;
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
			{
				if ( Amount <= 1 )
					AddLocalizedProperty( list, "item.special.deed.broken.armoire" );
				else
					list.Add( 1050039, "{0}\t{1}", Amount, ResolvePropertyText( "item.special.deed.broken.armoire" ) );
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

		public BrokenArmoireDeed( Serial serial ) : base( serial )
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
