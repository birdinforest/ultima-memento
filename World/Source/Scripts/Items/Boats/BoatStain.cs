using Server;
using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Misc;
using Server.Multis;
using Server.Localization;

namespace Server.Items
{
	public class BoatStain : Item
	{
		[Constructable]
		public BoatStain() : base( 0x14E0 )
		{
			Weight = 2.0;
			Name = "boat stain";
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );
			if (BuildingPropertyListLocale != null)
			{
				AddLocalizedProperty(list, "prop.boat.stain");
			}
			else
			{
				list.Add( 1070722, "Stain Boats to the Standard Color" );
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			Target t;

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1060640 ); // The item must be in your backpack to use it.
			}
			else
			{
				from.SendMessage( StringCatalog.Resolve( from.Account, "What docked ship do you wish to stain?" ) );
				t = new DyeTarget( this );
				from.Target = t;
			}
		}

		private class DyeTarget : Target
		{
			private BoatStain m_Dye;

			public DyeTarget( BoatStain tube ) : base( 1, false, TargetFlags.None )
			{
				m_Dye = tube;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Item )
				{
					Item iDye = targeted as Item;

					if ( !iDye.IsChildOf( from.Backpack ) )
					{
						from.SendMessage( StringCatalog.Resolve( from.Account, "You can only dye docked ships in your pack." ) );
					}
					else if ( iDye is BaseBoatDeed || iDye is BaseDockedBoat )
					{
						iDye.Hue = 0x5BE;
						from.RevealingAction();
						from.PlaySound( 0x23E );
					}
					else
					{
						from.SendMessage( StringCatalog.Resolve( from.Account, "You cannot stain that with this." ) );
					}
				}
				else
				{
					from.SendMessage( StringCatalog.Resolve( from.Account, "You cannot stain that with this." ) );
				}
			}
		}

		public BoatStain( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( ( int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}