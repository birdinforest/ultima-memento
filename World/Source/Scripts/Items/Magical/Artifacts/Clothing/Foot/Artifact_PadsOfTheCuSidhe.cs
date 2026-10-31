using System;
using Server;
using Server.Localization;
using Server.Mobiles;

namespace Server.Items
{
	public class Artifact_PadsOfTheCuSidhe : GiftFurBoots
	{
		public override string DisplayNameLocalizationKey => "item.magical.artifact.padsofthecusidhe";
		[Constructable]
		public Artifact_PadsOfTheCuSidhe()
		{
			Name = "Pads of the Cu Sidhe";
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 0, "" );
		}

		public override void OnRemoved( object parent )
		{
			PlayerMobile wearer = parent as PlayerMobile;

			if ( wearer != null
				&& wearer.Mounted
				&& wearer.AccessLevel == AccessLevel.Player
				&& wearer.Race != Race.Elf
				&& wearer.Mount is CuSidhe )
			{
				BaseMount.Dismount( wearer );
				wearer.SendMessage( StringCatalog.ResolveByKey( wearer.Account, "mob.other.cu_sidhe.pads_left" ) );
			}

			base.OnRemoved( parent );
		}

		public Artifact_PadsOfTheCuSidhe( Serial serial ) : base( serial )
		{
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