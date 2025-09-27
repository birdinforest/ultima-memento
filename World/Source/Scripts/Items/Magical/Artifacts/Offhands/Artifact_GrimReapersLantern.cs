namespace Server.Items
{
	public class Artifact_GrimReapersLantern : GiftLantern
	{
		[Constructable]
		public Artifact_GrimReapersLantern()
		{
			Name = "Grim Reaper's Lantern";
			Hue = 0x47E;
			Attributes.CastRecovery = 2;
			Attributes.CastSpeed = 1;
			Attributes.SpellDamage = 10;
			SkillBonuses.SetValues( 0, SkillName.Necromancy, 15 );
			SkillBonuses.SetValues( 1, SkillName.Spiritualism, 15 );
			ArtifactLevel = ArtifactLevel.Artifact;
			Server.Misc.Arty.ArtySetup( this, 8, "" );
		}

		public Artifact_GrimReapersLantern( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 2 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( version < 1 )
				ArtifactLevel = ArtifactLevel.Artifact;
		}
	}
}