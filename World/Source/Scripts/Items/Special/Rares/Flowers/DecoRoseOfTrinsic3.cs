using System;

namespace Server.Items
{
	public class DecoRoseOfTrinsic3 : Item
	{
		public override string DisplayNameLocalizationKey => "item.special.rares.flowers.velvet.rose";

		[Constructable]
		public DecoRoseOfTrinsic3() : base( 0x234B )
		{
			Name = "velvet rose";
			Movable = true;
			Stackable = false;
		}

		public DecoRoseOfTrinsic3( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}
