using Server.Mobiles;

namespace Server.Items
{
	public class Artifact_SprintersSandals : GiftSandals
	{
		[Constructable]
		public Artifact_SprintersSandals()
		{
			Name = "Sprinter's Sandals";
			Hue = 1372;
			Attributes.BonusStam = 15;
			Attributes.RegenStam = 5;
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 15, "Sprinting " );
		}

		public override void OnAdded( object from )
		{
			FastPlayer.OnItemAdded(from as PlayerMobile, this);
			base.OnAdded(from);
		}

        public override void OnRemoved( object parent )
		{
			FastPlayer.OnItemRemoved(parent as PlayerMobile, this);
			base.OnRemoved(parent);
		}

		public Artifact_SprintersSandals( Serial serial ) : base( serial )
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
