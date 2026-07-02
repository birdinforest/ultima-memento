using Server;
using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Localization;
using Server.Engines.Apiculture;

namespace Server.Items
{
	public class JarsOfWaxMetal : Item
	{
		public override Catalogs DefaultCatalog{ get{ return Catalogs.Wax; } }
		public override string DisplayNameLocalizationKey => "item.apiculture.jar_metal_wax";

		[Constructable]
		public JarsOfWaxMetal( ) : base( 0x1007 )
		{
			Stackable = true;
			Weight = 1.0;
			Stackable = false;
			Name = "jar of metal wax";
			Hue = 0x967;
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.apiculture.wax.metal_durability" );
			else
				list.Add( 1070722, "Adds Durability To Metal" );
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
				from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.target_metal" ) );
				t = new WaxTarget( this );
				from.Target = t;
			}
		}

		private class WaxTarget : Target
		{
			private JarsOfWaxMetal m_Wax;

			public WaxTarget( JarsOfWaxMetal tube ) : base( 1, false, TargetFlags.None )
			{
				m_Wax = tube;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Item iWax = targeted as Item;

				if ( iWax is BaseWeapon )
				{
					BaseWeapon xWax = (BaseWeapon)iWax;

					if ( !iWax.IsChildOf( from.Backpack ) )
					{
						from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.wax_items_in_pack" ) );
					}
					else if ( iWax.IsChildOf( from.Backpack ) && CraftResources.GetType( iWax.Resource ) == CraftResourceType.Metal )
					{
						int cBonus = xWax.WeaponAttributes.DurabilityBonus;

						if ( cBonus > 50 ){ from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.metal_good_condition" ) ); }
						else
						{
							xWax.WeaponAttributes.DurabilityBonus = ( cBonus + 10 );
							from.RevealingAction();
							from.PlaySound( 0x242 );
							from.AddToBackpack( new Bottle() );
							m_Wax.Consume();
						}
					}
					else
					{
						from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.cannot_rub_wax" ) );
					}
				}
				else if ( iWax is BaseArmor )
				{
					BaseArmor xWax = (BaseArmor)iWax;

					if ( !iWax.IsChildOf( from.Backpack ) )
					{
						from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.wax_items_in_pack" ) );
					}
					else if ( iWax.IsChildOf( from.Backpack ) && CraftResources.GetType( iWax.Resource ) == CraftResourceType.Metal )
					{
						int cBonus = xWax.ArmorAttributes.DurabilityBonus;

						if ( cBonus > 50 ){ from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.metal_good_condition" ) ); }
						else
						{
							xWax.ArmorAttributes.DurabilityBonus = ( cBonus + 10 );
							from.RevealingAction();
							from.PlaySound( 0x242 );
							from.AddToBackpack( new Bottle() );
							m_Wax.Consume();
						}
					}
					else
					{
						from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.cannot_rub_wax" ) );
					}
				}
				else
				{
					from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.cannot_rub_wax" ) );
				}
			}
		}

		public JarsOfWaxMetal( Serial serial ) : base( serial )
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
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public class JarsOfWaxLeather : Item
	{
		public override Catalogs DefaultCatalog{ get{ return Catalogs.Wax; } }
		public override string DisplayNameLocalizationKey => "item.apiculture.jar_leather_wax";

		[Constructable]
		public JarsOfWaxLeather( ) : base( 0x1007 )
		{
			Stackable = true;
			Weight = 1.0;
			Stackable = false;
			Name = "jar of leather wax";
			Hue = 0x972;
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.apiculture.wax.leather_durability" );
			else
				list.Add( 1070722, "Adds Durability To Leather" );
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
				from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.target_leather" ) );
				t = new WaxTarget( this );
				from.Target = t;
			}
		}

		private class WaxTarget : Target
		{
			private JarsOfWaxLeather m_Wax;

			public WaxTarget( JarsOfWaxLeather tube ) : base( 1, false, TargetFlags.None )
			{
				m_Wax = tube;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Item iWax = targeted as Item;

				if ( iWax is BaseArmor )
				{
					BaseArmor xWax = (BaseArmor)iWax;

					if ( !iWax.IsChildOf( from.Backpack ) )
					{
						from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.wax_items_in_pack" ) );
					}
					else if ( iWax.IsChildOf( from.Backpack ) && CraftResources.GetType( iWax.Resource ) == CraftResourceType.Leather )
                    {
						int cBonus = xWax.ArmorAttributes.DurabilityBonus;

						if ( cBonus > 50 ){ from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.leather_good_condition" ) ); }
						else
						{
							xWax.ArmorAttributes.DurabilityBonus = ( cBonus + 10 );
							from.RevealingAction();
							from.PlaySound( 0x242 );
							from.AddToBackpack( new Bottle() );
							m_Wax.Consume();
						}
					}
					else
					{
						from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.cannot_rub_wax" ) );
					}
				}
				else
				{
					from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.cannot_rub_wax" ) );
				}
			}
		}

		public JarsOfWaxLeather( Serial serial ) : base( serial )
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
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public class JarsOfWaxInstrument : Item
	{
		public override Catalogs DefaultCatalog{ get{ return Catalogs.Wax; } }
		public override string DisplayNameLocalizationKey => "item.apiculture.jar_instrument_wax";

		[Constructable]
		public JarsOfWaxInstrument( ) : base( 0x1007 )
		{
			Stackable = true;
			Weight = 1.0;
			Stackable = false;
			Name = "jar of instrument wax";
			Hue = 0x845;
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.apiculture.wax.restores_instruments" );
			else
				list.Add( 1070722, "Restores Instruments" );
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
				from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.target_instrument" ) );
				t = new WaxTarget( this );
				from.Target = t;
			}
		}

		private class WaxTarget : Target
		{
			private JarsOfWaxInstrument m_Wax;

			public WaxTarget( JarsOfWaxInstrument tube ) : base( 1, false, TargetFlags.None )
			{
				m_Wax = tube;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				Item iWax = targeted as Item;

				if ( iWax is BaseInstrument )
				{
					BaseInstrument xWax = (BaseInstrument)iWax;

					if ( !iWax.IsChildOf( from.Backpack ) )
					{
						from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.wax_items_in_pack" ) );
					}
					else
					{
						int cBonus = xWax.UsesRemaining;

						if ( cBonus > 300 ){ from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.instrument_good_condition" ) ); }
						else
						{
							xWax.UsesRemaining = ( cBonus + 50 );
							from.RevealingAction();
							from.PlaySound( 0x242 );
							from.AddToBackpack( new Bottle() );
							m_Wax.Consume();
						}
					}
				}
				else
				{
					from.SendMessage( ApicultureLocale.Msg( from.Account, "apiculture.msg.cannot_rub_wax" ) );
				}
			}
		}

		public JarsOfWaxInstrument( Serial serial ) : base( serial )
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