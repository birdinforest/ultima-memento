using System;
using Server;

namespace Server.Items
{
	public class Artifact_QuiverOfIce : ElvenQuiver
	{
		[Constructable]
		public Artifact_QuiverOfIce() : base()
		{
			BaseRunicTool.ApplyAttributes( this, 4, 4, 40, 100 );

			Name = "Quiver of Ice";
			Hue = 0x998;
			ItemID = 0x2B02;
			ArtifactLevel = ArtifactLevel.Artifact;
		}

		public Artifact_QuiverOfIce( Serial serial ) : base( serial )
		{
		}

		public override void AlterBowDamage( ref int phys, ref int fire, ref int cold, ref int pois, ref int nrgy, ref int chaos, ref int direct )
		{
			fire = pois = nrgy = chaos = direct = 0;
			phys = cold = 50;
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
