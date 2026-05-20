using System;
using Server;
using Server.Localization;

namespace Server.Items
{
	public class PaintCanvas : Item
	{
		public override string DefaultDescription{ get{ return StringCatalog.ResolveByKey(null, "eng.these_can_be_handed_to_an_artist_c_where_the_will_create_a_painting_of_you_to_hang_in_your_home_dot_"); } }

		[Constructable]
		public PaintCanvas() : base( 0xA6C )
		{
			Name = "painting canvas";
			Hue = 0x47E;
		}

		public PaintCanvas( Serial serial ) : base( serial )
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