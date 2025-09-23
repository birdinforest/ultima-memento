using System;
using Server.Items;

namespace Server.Items
{
	public class Artifact_Pestilence: BaseQuiver
	{		
		[Constructable]
		public Artifact_Pestilence() : base()
        {
			BaseRunicTool.ApplyAttributes( this, 4, 4, 40, 100 );

			Name = "Pestilence";
			Hue = 1151;
			Attributes.DefendChance = 5;
			Attributes.AttackChance = 5;
			LowerAmmoCost = 5;
			ItemID = 0x2B02;
			WeightReduction = 100;
			ArtifactLevel = ArtifactLevel.Artifact;
		}

		public Artifact_Pestilence( Serial serial ) : base( serial )
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