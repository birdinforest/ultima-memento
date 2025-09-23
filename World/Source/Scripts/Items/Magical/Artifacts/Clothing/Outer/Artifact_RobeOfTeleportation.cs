using Server.Spells.Third;

namespace Server.Items
{
	public class Artifact_RobeOfTeleportation : GiftRobe
	{
		public override int BasePhysicalResistance { get{ return 3; } }
		public override int BaseFireResistance { get{ return 4; } }
		public override int BaseColdResistance { get{ return 4; } }
		public override int BasePoisonResistance { get{ return 4; } }
		public override int BaseEnergyResistance { get{ return 4; } }

		[Constructable]
		public Artifact_RobeOfTeleportation()
		{
			Name = "Robe Of Teleportation";
			Hue = Utility.RandomColor( 0 );
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 5, "(Use to Teleport) " );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( Parent != from )
			{
				from.SendMessage( "You must be wearing the robe to teleport." );
			}
			else
			{
				new TeleportSpell( from, this ).Cast();
			}
			return;
		}

		public Artifact_RobeOfTeleportation( Serial serial ) : base( serial )
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
