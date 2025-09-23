namespace Server.Items
{
	public class Artifact_HoodedShroudOfShadows : GiftRobe
	{
		[Constructable]
		public Artifact_HoodedShroudOfShadows()
		{
			ItemID = 0x2B69;
			Hue = 0x455;
			Name = "Shroud of Shadows";
			SkillBonuses.SetValues( 0, SkillName.Hiding, 50 );
			SkillBonuses.SetValues( 1, SkillName.Stealth, 50 );
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 10, "" );
		}

		public Artifact_HoodedShroudOfShadows( Serial serial ) : base( serial )
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
