namespace Server.Items
{
	public class Artifact_GrimReapersRobe : GiftRobe
	{
		public override int BasePhysicalResistance { get{ return 3; } }
		public override int BaseFireResistance { get{ return 4; } }
		public override int BaseColdResistance { get{ return 4; } }
		public override int BasePoisonResistance { get{ return 4; } }
		public override int BaseEnergyResistance { get{ return 4; } }

		[Constructable]
		public Artifact_GrimReapersRobe()
		{
			ItemID = 0x1F03;
			Name = "Grim Reaper's Robe";
			Hue = 0xAF0;
			Attributes.ReflectPhysical = 25;
			SkillBonuses.SetValues( 0, SkillName.Necromancy, 15 );
			SkillBonuses.SetValues( 1, SkillName.Spiritualism, 15 );
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 6, "" );
		}

		public Artifact_GrimReapersRobe( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}