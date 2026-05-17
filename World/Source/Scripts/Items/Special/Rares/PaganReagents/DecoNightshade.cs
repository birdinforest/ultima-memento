using System;

using Server.Localization;

namespace Server.Items
{
	public class DecoNightshade : Item
	{

		[Constructable]
		public DecoNightshade() : base( 0x18E7 )
		{
			Movable = true;
			Stackable = false;
		}

		public DecoNightshade( Serial serial ) : base( serial )
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
