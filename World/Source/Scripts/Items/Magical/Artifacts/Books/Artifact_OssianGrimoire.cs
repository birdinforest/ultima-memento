using System;

namespace Server.Items
{
	public class Artifact_OssianGrimoire : NecromancerSpellbook
	{
		[Constructable]
		public Artifact_OssianGrimoire() : base()
		{
			Hue = 0xA99;
			Name = "Ossian Grimoire";
			Attributes.RegenMana = 1;
			Attributes.CastSpeed = 1;
			ArtifactLevel = ArtifactLevel.Artifact;

			this.Content = (ulong)( (int)(ulong)0x1FFFF );
			int attributeCount = Utility.RandomMinMax(8,15);
			int min = Utility.RandomMinMax(15,25);
			int max = min + 40;
			BaseRunicTool.ApplyAttributesTo( (Spellbook)this, attributeCount, min, max );

			SkillBonuses.SetValues( 0, SkillName.MagicResist, ( 10.0 + (Utility.RandomMinMax(0,2)*5) ) );
			SkillBonuses.SetValues( 1, SkillName.Spiritualism, ( 10.0 + (Utility.RandomMinMax(0,2)*5) ) );
			SkillBonuses.SetValues( 2, SkillName.Necromancy, ( 10.0 + (Utility.RandomMinMax(0,2)*5) ) );
			SkillBonuses.SetValues( 3, SkillName.Meditation, ( 10.0 + (Utility.RandomMinMax(0,2)*5) ) );
		}

		public Artifact_OssianGrimoire( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.WriteEncodedInt( 0 ); //version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadEncodedInt();
		}
	}
}
