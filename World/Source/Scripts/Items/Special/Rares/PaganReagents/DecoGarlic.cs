using System;

using Server.Localization;

namespace Server.Items
{
	public class DecoGarlic : Item
	{

		[Constructable]
		public DecoGarlic() : base( 0x18E1 )
		{
			Movable = true;
			Stackable = false;
		}

		public DecoGarlic( Serial serial ) : base( serial )
		{
		}

		public override bool OnDragLift( Mobile from )
		{
			from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.special.paganreagent.decorative.msg" ) );
			return base.OnDragLift( from );
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
