using System;
using Server;

namespace Server.Items
{
	public class PhillipsWoodenSteed : MonsterStatuette
	{
		[Constructable]
		public PhillipsWoodenSteed() : base( MonsterStatuetteType.PhillipsWoodenSteed )
		{
			LootType = LootType.Regular;
			ArtifactLevel = ArtifactLevel.DecorativeArtefact;
		}

		public override bool ForceShowProperties{ get{ return ObjectPropertyList.Enabled; } }

		public PhillipsWoodenSteed( Serial serial ) : base( serial )
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
			if (version < 1)
				ArtifactLevel = ArtifactLevel.DecorativeArtefact;
		}
	}
}