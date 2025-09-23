namespace Server.Items
{
	public class Artifact_PolarBearCape : GiftFurCape
	{
		public override int BasePhysicalResistance { get{ return 2; } }
		public override int BaseFireResistance { get{ return 4; } }
		public override int BaseColdResistance { get{ return 3; } }
		public override int BasePoisonResistance { get{ return 3; } }
		public override int BaseEnergyResistance { get{ return 3; } }

		[Constructable]
		public Artifact_PolarBearCape()
		{
			Hue = 0x47E;
			Name = "Polar Bear Cape";
			Resistances.Cold = 30;
			Attributes.WeaponDamage = 10;
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 1, "" );
		}

		public Artifact_PolarBearCape( Serial serial ) : base( serial )
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