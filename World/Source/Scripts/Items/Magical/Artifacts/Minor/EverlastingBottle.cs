using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Localization;

namespace Server.Items
{
	public class EverlastingBottle : Item
	{
		public override string DisplayNameLocalizationKey => "item.magical.artifact.everlastingbottle";
		public override double DefaultWeight
		{
			get { return 1.0; }
		}

		[Constructable]
		public EverlastingBottle() : base( 0x2827 )
		{
			Hue = 0x849;
			Name = "Everlasting Bottle";
			ArtifactLevel = ArtifactLevel.Artifact;
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.Thirst = 20;
			from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.artifact.bottle.drink" ) );
			from.PlaySound( Utility.RandomList( 0x30, 0x2D6 ) );
		}

		public EverlastingBottle( Serial serial ) : base( serial )
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

			if ( version < 1 )
				ArtifactLevel = ArtifactLevel.Artifact;

			ItemID = 0x2827;
			Hue = 0x849;
		}
	}
}