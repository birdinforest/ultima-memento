using System;
using Server;
using Server.Engines.Craft;

namespace Server.Items
{
	public class WaxingPot : BaseTool
	{
		public override string DisplayNameLocalizationKey => "item.apiculture.wax_crafting_pot";
		public override CraftSystem CraftSystem{ get{ return DefWaxingPot.CraftSystem; } }

		[Constructable]
		public WaxingPot() : base( 0x142B )
		{
			Weight = 1.0;
			Name = "wax crafting pot";
			UsesRemaining = 20;
		}

		[Constructable]
		public WaxingPot( int uses ) : base( uses, 0x142B )
		{
			Weight = 20.0;
			Name = "wax crafting pot";
			UsesRemaining = 20;
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.apiculture.skilled_in_cooking" );
			else
				list.Add( 1070722, "For Those Skilled In Cooking");
        } 

		public WaxingPot( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
