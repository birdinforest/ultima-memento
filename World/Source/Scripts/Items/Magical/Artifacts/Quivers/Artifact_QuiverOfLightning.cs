using System;
using Server;

namespace Server.Items
{
	public class Artifact_QuiverOfLightning : ElvenQuiver
	{
		[Constructable]
		public Artifact_QuiverOfLightning() : base()
		{
			BaseRunicTool.ApplyAttributes( this, 4, 4, 40, 100 );

			Name = "Quiver of Lightning";
			Hue = 0x8D9;
			ItemID = 0x2B02;
			ArtifactLevel = ArtifactLevel.Artifact;
		}

		public Artifact_QuiverOfLightning( Serial serial ) : base( serial )
		{
		}

		public override void AlterBowDamage( ref int phys, ref int fire, ref int cold, ref int pois, ref int nrgy, ref int chaos, ref int direct )
		{
			fire = cold = pois = chaos = direct = 0;
			phys = nrgy = 50;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}
}
