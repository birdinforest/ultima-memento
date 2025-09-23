using System;
using Server;

namespace Server.Items
{
	public class Artifact_QuiverOfElements : BaseQuiver
	{
		[Constructable]
		public Artifact_QuiverOfElements() : base()
		{
			BaseRunicTool.ApplyAttributes( this, 4, 4, 40, 100 );

			Name = "Quiver of the Elements";
			Hue = 0xAFE;
			ItemID = 0x2B02;
			WeightReduction = 100;
			ArtifactLevel = ArtifactLevel.Artifact;
		}

		public Artifact_QuiverOfElements( Serial serial ) : base( serial )
		{
		}

		public override void AlterBowDamage( ref int phys, ref int fire, ref int cold, ref int pois, ref int nrgy, ref int chaos, ref int direct )
		{
			chaos = phys = direct = 0;
			fire = 25;
			cold = 25;
			nrgy = 25;
			pois = 25;
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
