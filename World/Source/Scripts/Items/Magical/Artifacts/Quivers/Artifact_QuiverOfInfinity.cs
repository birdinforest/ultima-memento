using System;

namespace Server.Items
{
	public class Artifact_QuiverOfInfinity : BaseQuiver
	{
		[Constructable]
		public Artifact_QuiverOfInfinity() : base()
		{
			BaseRunicTool.ApplyAttributes( this, 4, 4, 40, 100 );

			Name = "Quiver of Infinity";
			ItemID = 0x2B02;
			Hue = 0x99A;
			WeightReduction = 80;
			LowerAmmoCost = 20;
			Attributes.DefendChance = 5;
			ArtifactLevel = ArtifactLevel.Artifact;
		}

		public Artifact_QuiverOfInfinity( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 1 ); //version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();

		}
	}
}
