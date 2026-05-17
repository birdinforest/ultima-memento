using System;

using Server.Localization;

namespace Server.Items
{
	public class DecoBlackmoor : Item
	{

		[Constructable]
		public DecoBlackmoor() : base( 0xF79 )
		{
			Movable = true;
			Stackable = false;
		}

		public DecoBlackmoor( Serial serial ) : base( serial )
		{
		}

		public override bool OnDragLift( Mobile from )
		{
			from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.special.paganreagent.decorative.msg.pagan" ) );
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
