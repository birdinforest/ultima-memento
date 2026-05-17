using System;
using Server;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public class Artifact_AcidProofRobe : GiftRobe
	{
		public override string DisplayNameLocalizationKey => "item.magical.artifact.acidproofrobe";
		public DateTime TimeUsed;

		[CommandProperty(AccessLevel.Owner)]
		public DateTime Time_Used { get { return TimeUsed; } set { TimeUsed = value; InvalidateProperties(); } }

		[Constructable]
		public Artifact_AcidProofRobe()
		{
			Name = "Acidic Robe";
			Hue = 1167;
			Resistances.Fire = 20;
			Resistances.Poison = 20;
			ArtifactLevel = ArtifactLevel.StandardArtefact;
			Server.Misc.Arty.ArtySetup( this, 10, "Acid Soaked " );
		}

		public override void OnDoubleClick( Mobile from )
		{
			DateTime TimeNow = DateTime.Now;
			long ticksThen = TimeUsed.Ticks;
			long ticksNow = TimeNow.Ticks;
			int minsThen = (int)TimeSpan.FromTicks(ticksThen).TotalMinutes;
			int minsNow = (int)TimeSpan.FromTicks(ticksNow).TotalMinutes;
			int CanFillBottle = 10 - ( minsNow - minsThen );

			if ( Parent != from )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.artifact.acid.robe.wear" ) );
			}
			else if ( CanFillBottle > 0 )
			{
				TimeSpan t = TimeSpan.FromMinutes( CanFillBottle );
				string wait = string.Format("{0:D1} hours and {1:D2} minutes", 
								t.Hours, 
								t.Minutes);
				from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "prop.magical.artifact.acid.robe.cooldown", wait ) );
			}
			else
			{
				if (!from.Backpack.ConsumeTotal(typeof(Bottle), 1))
				{
					from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.artifact.acid.robe.need.bottle" ) );
				}
				else
				{
					from.PlaySound( 0x240 );
					from.AddToBackpack( new BottleOfAcid() );
					from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.artifact.acid.robe.squeeze" ) );
					TimeUsed = DateTime.Now;
				}
			}
		}

		public override bool OnDragLift( Mobile from )
		{
			if ( from is PlayerMobile )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.magical.artifact.acid.robe.draghint" ) );
			}

			return true;
		}

		public Artifact_AcidProofRobe( Serial serial ) : base( serial )
		{
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 );
            writer.Write( TimeUsed );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			TimeUsed = reader.ReadDateTime();
		}
	}
}
