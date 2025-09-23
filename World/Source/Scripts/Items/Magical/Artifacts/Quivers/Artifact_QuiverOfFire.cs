using System;
using Server;

namespace Server.Items
{
	public class Artifact_QuiverOfFire : ElvenQuiver
	{
		[Constructable]
		public Artifact_QuiverOfFire() : base()
		{
			BaseRunicTool.ApplyAttributes( this, 4, 4, 40, 100 );
			ArtifactLevel = ArtifactLevel.Artifact;

			Name = "Quiver of Fire";
			Hue = 0xB17;
			ItemID = 0x2B02;
		}

		public Artifact_QuiverOfFire( Serial serial ) : base( serial )
		{
		}

		public override void AlterBowDamage( ref int phys, ref int fire, ref int cold, ref int pois, ref int nrgy, ref int chaos, ref int direct )
		{
			cold = pois = nrgy = chaos = direct = 0;
			phys = fire = 50;
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
