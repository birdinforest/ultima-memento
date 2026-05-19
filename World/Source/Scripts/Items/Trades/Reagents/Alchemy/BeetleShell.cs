using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class BeetleShell : BaseReagent
	{
		public override string DisplayNameLocalizationKey => "item.trade.name.reagent.beetle.shell";
		public override bool IsContentLocalized => true;

		[Constructable]
		public BeetleShell() : this( 1 )
		{
		}

		[Constructable]
		public BeetleShell( int amount ) : base( 0x2FF8, amount )
		{
			Name = "beetle shell";
		}

		public BeetleShell( Serial serial ) : base( serial )
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