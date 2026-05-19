using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class BloodRose : BaseReagent
	{
		public override string DisplayNameLocalizationKey => "item.trade.name.reagent.blood.rose";
		public override bool IsContentLocalized => true;

		[Constructable]
		public BloodRose() : this( 1 )
		{
		}

		[Constructable]
		public BloodRose( int amount ) : base( 0x640E, amount )
		{
			Name = "blood rose";
		}

		public BloodRose( Serial serial ) : base( serial )
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