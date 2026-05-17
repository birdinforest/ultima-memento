using System;

using Server.Localization;

namespace Server.Items
{
	public class DecoMandrakeRoot2 : Item
	{

		[Constructable]
		public DecoMandrakeRoot2() : base( 0x18DD )
		{
			Movable = true;
			Stackable = false;
		}

		public DecoMandrakeRoot2( Serial serial ) : base( serial )
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
