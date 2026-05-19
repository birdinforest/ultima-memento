using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class BlackPearl : BaseReagent
	{
		public override string DisplayNameLocalizationKey => "item.trade.name.reagent.black.pearl";
		public override bool IsContentLocalized => true;

		[Constructable]
		public BlackPearl() : this( 1 )
		{
		}

		[Constructable]
		public BlackPearl( int amount ) : base( 0x266F, amount )
		{
			Name = "black pearl";
		}

		public BlackPearl( Serial serial ) : base( serial )
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

			ItemID = 0x266F;
			Name = "black pearl";
		}
	}
}