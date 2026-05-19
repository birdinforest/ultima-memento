using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class DriedToad : BaseReagent
	{
		public override string DisplayNameLocalizationKey => "item.trade.name.reagent.dried.toad";
		public override bool IsContentLocalized => true;

		[Constructable]
		public DriedToad() : this( 1 )
		{
		}

		[Constructable]
		public DriedToad( int amount ) : base( 0x640F, amount )
		{
			Name = "dried toad";
		}

		public DriedToad( Serial serial ) : base( serial )
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