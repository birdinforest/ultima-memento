using System;

using Server.Localization;

namespace Server.Items
{
	public class DecoGinsengRoot : Item
	{

		[Constructable]
		public DecoGinsengRoot() : base( 0x18EB )
		{
			Movable = true;
			Stackable = false;
		}

		public DecoGinsengRoot( Serial serial ) : base( serial )
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
