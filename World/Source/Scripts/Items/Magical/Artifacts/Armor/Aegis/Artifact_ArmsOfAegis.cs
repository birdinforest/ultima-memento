using System;
using Server;

namespace Server.Items
{
	public class Artifact_ArmsOfAegis : GiftPlateArms
	{
		public override int InitMinHits{ get{ return 80; } }
		public override int InitMaxHits{ get{ return 160; } }

		public override int BasePhysicalResistance{ get{ return 16; } }

		[Constructable]
		public Artifact_ArmsOfAegis()
		{
			Name = "Arms of Aegis";
			Hue = 0x47E;
			ItemID = 0x1410;
			ArmorAttributes.SelfRepair = 5;
			Attributes.ReflectPhysical = 12;
			Attributes.DefendChance = 12;
			Attributes.LowerManaCost = 6;
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 6, "" );
		}

		public Artifact_ArmsOfAegis( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}