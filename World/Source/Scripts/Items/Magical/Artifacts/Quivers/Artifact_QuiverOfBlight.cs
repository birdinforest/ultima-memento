using System;
using Server;

namespace Server.Items
{
	public class Artifact_QuiverOfBlight : ElvenQuiver
	{
		[Constructable]
		public Artifact_QuiverOfBlight() : base()
		{
			BaseRunicTool.ApplyAttributes( this, 4, 4, 40, 100 );

			Name = "Quiver of Blight";
			Hue = 0xB79;
			ItemID = 0x2B02;
			ArtifactLevel = ArtifactLevel.Artifact;
		}

		public Artifact_QuiverOfBlight( Serial serial ) : base( serial )
		{
		}

		public override void AlterBowDamage( ref int phys, ref int fire, ref int cold, ref int pois, ref int nrgy, ref int chaos, ref int direct )
		{
			phys = fire = nrgy = chaos = direct = 0;
			cold = pois = 50;
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
