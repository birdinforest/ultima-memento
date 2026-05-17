using System;
using Server.Items;

namespace Server.Items
{
	public class RoyalGorget : PlateGorget
	{
		public override string DisplayNameLocalizationKey => "item.equip.armor.royalgorget";
		[Constructable]
		public RoyalGorget()
		{
			ItemID = 0x2B0E;
			Name = "royal gorget";
			Weight = 2.0;
		}

		public RoyalGorget( Serial serial ) : base( serial )
		{
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}