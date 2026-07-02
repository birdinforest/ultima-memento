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
	public class apiSmallWaxPot : Item
	{
		public override string DisplayNameLocalizationKey => "item.apiculture.small_wax_pot";
		public static readonly int MaxWax = 255; //the maximuum amount the pot can hold

		private int m_UsesRemaining;
		private int m_RawBeeswax;
		private int m_PureBeeswax;

		[CommandProperty( AccessLevel.GameMaster )]
		public int UsesRemaining
		{
			get { return m_UsesRemaining; }
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int RawBeeswax
		{
			get { return m_RawBeeswax; }
			set { if(value<0)value=0;if(value>MaxWax)value=MaxWax;m_RawBeeswax = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int PureBeeswax
		{
			get { return m_PureBeeswax; }
			set { if(value<0)value=0;if(value>MaxWax)value=MaxWax;m_PureBeeswax = value; InvalidateProperties(); }
		}

		[Constructable]
		public apiSmallWaxPot() : this( 50 )
		{
		}
		
		[Constructable]
		public apiSmallWaxPot( int uses ) : base( 2532 )
		{
			m_UsesRemaining = uses;
			Name = "small wax pot";
			Weight = 3.0;
			m_RawBeeswax = 0;
			m_PureBeeswax = 0;
		}

		public apiSmallWaxPot( Serial serial ) : base( serial )
		{
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.apiculture.uses", m_UsesRemaining.ToString() );
			else
				list.Add( 1060584, "{0}\t{1}", m_UsesRemaining.ToString(), "Uses" );

			if( PureBeeswax < 1 && RawBeeswax < 1 )
			{
				if ( BuildingPropertyListLocale != null )
					AddLocalizedProperty( list, "prop.apiculture.pot.empty" );
				else
					list.Add( 1049644 , "Empty" );
			}
			else if( PureBeeswax > 0 )
			{
				if ( BuildingPropertyListLocale != null )
				{
					list.Add( 1060663, "{0}\t{1}", ResolvePropertyText( "prop.apiculture.wax" ), PureBeeswax.ToString() );
					AddLocalizedProperty( list, "prop.apiculture.wax.rendered" );
				}
				else
				{
					list.Add( 1060663,"{0}\t{1}" ,"Wax", PureBeeswax.ToString() );
					list.Add( 1049644 , "Rendered" );
				}
			}
			else
			{
				if ( BuildingPropertyListLocale != null )
				{
					list.Add( 1060663, "{0}\t{1}", ResolvePropertyText( "prop.apiculture.wax" ), RawBeeswax.ToString() );
					AddLocalizedProperty( list, "prop.apiculture.wax.raw" );
				}
				else
				{
					list.Add( 1060663,"{0}\t{1}" ,"Wax", RawBeeswax.ToString() );
					list.Add( 1049644 , "Raw" );
				}
			}
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
			from.SendGump( new apiBeeHiveSmallPotGump( from, this ) );
		}

		public void BeginAdd( Mobile from )
		{
			if ( m_RawBeeswax < MaxWax )
				from.Target = new AddWaxTarget( this );
			else
				from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.pot_full_raw" ), from.NetState );
		}

		public void EndAdd( Mobile from, object o )
		{
			if ( o is Item && ((Item)o).IsChildOf( from.Backpack ) )
			{
				if( o is RawBeeswax )
				{
					RawBeeswax wax = (RawBeeswax)o;

					if( (wax.Amount + RawBeeswax) > MaxWax )
					{
						wax.Amount -= (MaxWax - RawBeeswax);
						RawBeeswax = MaxWax;
					}
					else
					{
						RawBeeswax += wax.Amount;
						wax.Delete();
					}
					
					from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.put_raw_in_pot" ), from.NetState );
					
					if( from.HasGump( typeof(apiBeeHiveSmallPotGump)) )
						from.CloseGump( typeof(apiBeeHiveSmallPotGump) );
					
					from.SendGump( new apiBeeHiveSmallPotGump( from, this ) ); //resend the gump
					
					if ( m_RawBeeswax < MaxWax )
						BeginAdd( from );
				}
				else
					from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.only_raw_in_pot" ), from.NetState );
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
			writer.Write( (int) m_RawBeeswax );
			writer.Write( (int) m_PureBeeswax );
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
					m_RawBeeswax = reader.ReadInt();
					m_PureBeeswax = reader.ReadInt();
					break;
				}
			}
		}
	}

	public class AddWaxTarget : Target
	{
		private apiSmallWaxPot m_pot;

		public AddWaxTarget( apiSmallWaxPot pot ) : base( 18, false, TargetFlags.None )
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

	public class apiBeeHiveSmallPotGump : Gump
	{
		apiSmallWaxPot m_pot = null;

		public static bool GiveSlumgum = true;  //does rendering produce slumgum? (impurities in wax)

		public apiBeeHiveSmallPotGump( Mobile from, apiSmallWaxPot pot ): base( 20, 20 )
		{
			m_pot = pot;

			Closable=true;
			Disposable=true;
			Dragable=true;
			Resizable=false;

			AddPage(0);

			AddBackground(15, 12, 352, 140, 9270);
			AddAlphaRegion(30, 27, 321, 109);
			AddImage(326, 110, 210);

			//vines
			AddItem(10, 5, 3311);
			AddItem(11, 49, 3311);
			AddItem(328, 50, 3307);
			AddItem(327, 3, 3307);
			
			//pot image
			if( m_pot.PureBeeswax > 0 )
				AddItem(231, 105, 0x142B);
			else
				AddItem(231, 105, 2532);

			//labels
			AddLabel(76 , 71 , 1153, StringCatalog.ResolveByKey( from.Account, "apiculture.gump.pot.render" ) );
			AddLabel(76 , 40 , 1153, StringCatalog.ResolveByKey( from.Account, "apiculture.gump.pot.add_raw" ) );
			AddLabel(76 , 101, 1153, StringCatalog.ResolveByKey( from.Account, "apiculture.gump.pot.empty" ) );
			AddLabel(331, 110, 1153, "?");

			//buttons
			AddButton(42, 39, 4005, 4006, (int)Buttons.cmdAddRaw, GumpButtonType.Reply, 0);
			AddButton(42, 70, 4005, 4006, (int)Buttons.cmdRenderWax, GumpButtonType.Reply, 0);
			AddButton(42, 102, 4005, 4006, (int)Buttons.cmdEmptyPot, GumpButtonType.Reply, 0);
			AddButton(326, 110, 212, 212, (int)Buttons.cmdHelp, GumpButtonType.Reply, 0);

			//wax amounts
			AddLabel(207, 40, 1153, ApicultureLocale.FormatMsg( from.Account, "apiculture.gump.pot.raw_count", m_pot.RawBeeswax ) );
			AddLabel(207, 71, 1153, ApicultureLocale.FormatMsg( from.Account, "apiculture.gump.pot.pure_count", m_pot.PureBeeswax ) );
		}
		
		public enum Buttons
		{
			cmdAddRaw = 1,
			cmdRenderWax,
			cmdEmptyPot,
			cmdHelp
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;

			if ( info.ButtonID == 0 || m_pot.Deleted || !from.InRange( m_pot.GetWorldLocation(), 3 ) )
				return;

			if( !m_pot.IsAccessibleTo( from ) )
			{
				from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.cannot_use_that_alt" ), from.NetState );
				return;
			}

			switch ( info.ButtonID )
			{
				case (int)Buttons.cmdHelp: 
				{
					from.SendGump( new apiBeeHiveSmallPotGump(from,m_pot) );
					from.SendGump( new apiBeeHiveHelpGump(from, 1) );
					break;
				}
				case (int)Buttons.cmdAddRaw: //Add Raw Honey
				{
					from.SendGump( new apiBeeHiveSmallPotGump(from, m_pot) );

					if ( m_pot.PureBeeswax > 0 )
					{
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.cannot_mix_wax" ), from.NetState );
						return;
					}

					from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.choose_raw_wax" ), from.NetState );
					m_pot.BeginAdd( from );
					
					break;
				}
				case (int)Buttons.cmdEmptyPot: //Empty the pot
				{
					if( m_pot.PureBeeswax < 1 && m_pot.RawBeeswax < 1 )
					{
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.no_wax_in_pot" ), from.NetState );
						from.SendGump( new apiBeeHiveSmallPotGump(from,m_pot) );
						return;
					}

					Item wax;

					if( m_pot.PureBeeswax > 0 )
					{
						wax = new Beeswax(m_pot.PureBeeswax);
					}
					else
					{
						wax = new RawBeeswax(m_pot.RawBeeswax);
					}

					BaseContainer.PutStuffInContainer( from, 3, wax );

					m_pot.RawBeeswax = 0;
					m_pot.PureBeeswax = 0;
					
					m_pot.ItemID = 2532; //empty pot

					from.SendGump( new apiBeeHiveSmallPotGump(from, m_pot) );
					from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.place_wax_in_pack" ), from.NetState );
						
					break;
				}
				case (int)Buttons.cmdRenderWax: //render the wax
				{
					if( m_pot.UsesRemaining < 1 )
					{//no uses remaining
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.pot_too_damaged_render" ), from.NetState );
						from.SendGump( new apiBeeHiveSmallPotGump(from, m_pot) );
						return;
					}
					else if( m_pot.PureBeeswax > 1 )
					{//already rendered
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.pot_full_rendered" ), from.NetState );
						from.SendGump( new apiBeeHiveSmallPotGump(from, m_pot) );
						return;
					}
					else if( m_pot.RawBeeswax < 10 )
					{//not enough raw beeswax
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.not_enough_raw" ), from.NetState );
						from.SendGump( new apiBeeHiveSmallPotGump(from, m_pot) );
						return;
					}
					else if( !BeeHiveHelper.Find( from, BeeHiveHelper.m_HeatSources ) )
					{//need a heat source to melt the wax
						from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.need_heat_render" ), from.NetState );
						from.SendGump( new apiBeeHiveSmallPotGump(from, m_pot) );
						return;
					}

					m_pot.ItemID = 0x142b; //pot overflowing with wax

					m_pot.UsesRemaining--;
					if( m_pot.UsesRemaining < 0 )
						m_pot.UsesRemaining = 0;

					int waste = Utility.RandomMinMax( 1, m_pot.RawBeeswax / 5 );
					
					if( GiveSlumgum )
					{//give slumgum
						Item gum = new Slumgum( Math.Max( 1, waste ) );

						BaseContainer.PutStuffInContainer( from, 3, gum );
					}

					from.PlaySound( 0x21 );
					from.PrivateOverheadMessage( 0, 1154, false, ApicultureLocale.Msg( from.Account, "apiculture.msg.rendering_wax" ), from.NetState );
					
					m_pot.PureBeeswax = m_pot.RawBeeswax - waste;
					m_pot.RawBeeswax = 0;

					break;
				}
			}
		}
	}
}