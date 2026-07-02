using System;
using Server;
using Server.Network;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Engines.Apiculture;
using Server.Targeting;
using Server.Localization;

namespace Server.Items
{
	public class apiLargeWaxPot : Item
	{
		public override string DisplayNameLocalizationKey => "item.apiculture.large_wax_pot";
		public static readonly int MaxWax = 999; //the maximum amount the pot can hold

		private int m_UsesRemaining;
		private int m_Beeswax;
		
		[CommandProperty( AccessLevel.GameMaster )]
		public int UsesRemaining
		{
			get { return m_UsesRemaining; }
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MeltedBeeswax
		{
			get { return m_Beeswax; }
			set { if(value<0)value=0;if(value>MaxWax)value=MaxWax;m_Beeswax = value; InvalidateProperties(); }
		}

		[Constructable]
		public apiLargeWaxPot() : this( 50 )
		{
		}
		
		[Constructable]
		public apiLargeWaxPot( int uses ) : base( 2541 )
		{
			m_UsesRemaining = uses;
			Name = "large wax pot";
			Weight = 5.0;
			m_Beeswax = 0;
		}

		public apiLargeWaxPot( Serial serial ) : base( serial )
		{
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.apiculture.uses", m_UsesRemaining.ToString() );
			else
				list.Add( 1060584, "{0}\t{1}", m_UsesRemaining.ToString(), "Uses" );

			if( MeltedBeeswax < 1 )
			{
				if ( BuildingPropertyListLocale != null )
					AddLocalizedProperty( list, "prop.apiculture.pot.empty" );
				else
					list.Add( 1049644 , "Empty" );
			}
			else if ( BuildingPropertyListLocale != null )
				list.Add( 1060663, "{0}\t{1}", ResolvePropertyText( "prop.apiculture.melted_wax" ), MeltedBeeswax.ToString() );
			else
				list.Add( 1060663,"{0}\t{1}" ,"Melted Wax", MeltedBeeswax.ToString() );
		}

		public virtual void DisplayDurabilityTo( Mobile m )
		{
			LabelToAffix( m, 1017323, AffixType.Append, ": " + m_UsesRemaining.ToString() ); // Durability
		}

		public override void OnSingleClick( Mobile from )
		{
			DisplayDurabilityTo( from );

			base.OnSingleClick( from );
		}

		public override void OnDoubleClick(Mobile from)
		{
			if ( !IsChildOf( from.Backpack ) )
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			else
			{
				from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.choose_wax_melt" ), from.NetState );
				BeginAdd( from );
			}
		}

		public void BeginAdd( Mobile from )
		{
			from.Target = new AddPureWaxTarget( this );
		}

		public void EndAdd( Mobile from, object o )
		{
			if ( o is Item && ((Item)o).IsChildOf( from.Backpack ) )
			{
				if( o is Beeswax )
				{
					//error checking
					if ( UsesRemaining < 1 )
					{
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.pot_too_damaged_melt" ), from.NetState );
						return;
					}
					else if ( m_Beeswax >= MaxWax )
					{
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.pot_full_wax" ), from.NetState );
						return;
					}
					else if( !BeeHiveHelper.Find( from, BeeHiveHelper.m_HeatSources ) )
					{
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.need_heat_melt" ), from.NetState );
						return;
					}
	
					Beeswax wax = (Beeswax)o;

					if( (wax.Amount + MeltedBeeswax) > MaxWax )
					{
						wax.Amount -= (MaxWax - MeltedBeeswax);
						MeltedBeeswax = MaxWax;
					}
					else
					{
						MeltedBeeswax += wax.Amount;
						wax.Delete();
					}
					
					from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.melting_wax" ), from.NetState );

					this.ItemID = 5162; //change the graphic					

					from.PlaySound( 43 ); //bellow sound
					//from.PlaySound( 0x21 ); //bubbling sound

					UsesRemaining--;

					if ( MeltedBeeswax < MaxWax )
						BeginAdd( from );
				}
				else if ( o == this )
				{
					//empty the pot
					if( MeltedBeeswax < 1 )
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.no_wax_in_pot" ), from.NetState );
					else
					{
						Item wax = new Beeswax( MeltedBeeswax );

						BaseContainer.PutStuffInContainer( from, 3, wax );

						MeltedBeeswax = 0;
					
						ItemID = 2541; //empty pot

						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.empty_pot_to_pack" ), from.NetState );
					}
				}
				else
					from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.only_pure_in_pot" ), from.NetState );
			}
			else
			{
				from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.wax_in_pack" ), from.NetState );
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
			writer.Write( (int) m_UsesRemaining );
			writer.Write( (int) m_Beeswax );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_UsesRemaining = reader.ReadInt();
					m_Beeswax = reader.ReadInt();
					break;
				}
			}
		}
	}

	public class AddPureWaxTarget : Target
	{
		private apiLargeWaxPot m_pot;

		public AddPureWaxTarget( apiLargeWaxPot pot ) : base( 18, false, TargetFlags.None )
		{
			m_pot = pot;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			if ( m_pot.Deleted || !m_pot.IsChildOf( from.Backpack ) )
				return;

			m_pot.EndAdd( from, targeted );
		}
	}
}