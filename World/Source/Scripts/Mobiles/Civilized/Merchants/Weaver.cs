using System;
using System.Collections.Generic;
using Server.Targeting;
using Server.Items;
using Server.ContextMenus;
using Server.Misc;

namespace Server.Mobiles
{
	public class Weaver : BaseVendor
	{
		private List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos{ get { return m_SBInfos; } }

		public override string TalkGumpTitle{ get{ return "Altering Cloaks And Robes"; } }
		public override string TalkGumpSubject{ get{ return "Tailor"; } }

		public override NpcGuild NpcGuild{ get{ return NpcGuild.TailorsGuild; } }

		[Constructable]
		public Weaver() : base( "the weaver" )
		{
			SetSkill( SkillName.Tailoring, 65.0, 88.0 );
		}

		public override void InitSBInfo( Mobile m )
		{
			m_Merchant = m;
			m_SBInfos.Add( new MyStock() );
		}

		public class MyStock: SBInfo
		{
			private List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
			private IShopSellInfo m_SellInfo = new InternalSellInfo();

			public MyStock()
			{
			}

			public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
			public override List<GenericBuyInfo> BuyInfo { get { return m_BuyInfo; } }

			public class InternalBuyInfo : List<GenericBuyInfo>
			{
				public InternalBuyInfo()
				{
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Rare,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Resource,	ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
				}
			}

			public class InternalSellInfo : GenericSellInfo
			{
				public InternalSellInfo()
				{
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Rare,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Resource,	ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
				}
			}
		}

		private class FixEntry : ContextMenuEntry
		{
			private Weaver m_Weaver;
			private Mobile m_From;

			public FixEntry( Weaver Weaver, Mobile from ) : base( 6120, 12 )
			{
				m_Weaver = Weaver;
				m_From = from;
				Enabled = Weaver.CheckVendorAccess( from );
			}

			public override void OnClick()
			{
				m_Weaver.BeginServices( m_From );
			}
		}

		public override void AddCustomContextEntries( Mobile from, List<ContextMenuEntry> list )
		{
			if ( CheckChattingAccess( from ) )
				list.Add( new FixEntry( this, from ) );

			base.AddCustomContextEntries( from, list );
		}

        public void BeginServices(Mobile from)
        {
            if ( Deleted || !from.Alive )
                return;

			int nCost = 5;
			int nCostH = 10;

			if ( BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
			{
				nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost ); if ( nCost < 1 ){ nCost = 1; }
				nCostH = nCostH - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCostH ); if ( nCostH < 1 ){ nCostH = 1; }
				SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.since_you_are_begging_do_you_still_want_me_to_tailor_yo", nCost, nCostH));
			}
			else { SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.if_you_want_me_to_tailor_your_robe_or_cloak_to_look_nor", nCost, nCostH)); }

            from.Target = new RepairTarget(this);
        }

        private class RepairTarget : Target
        {
            private Weaver m_Weaver;

            public RepairTarget(Weaver tailor) : base(12, false, TargetFlags.None)
            {
                m_Weaver = tailor;
            }

            protected override void OnTarget(Mobile from, object targeted)
			{
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				if ( targeted is BaseHat && from.Backpack != null )
				{
                    BaseHat ba = targeted as BaseHat;
                    Container pack = from.Backpack;
                    int toConsume = 0;

                    if (ba.HitPoints < ba.MaxHitPoints)
                    {
						int nCost = 10;

						if ( BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
						{
							nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost ); if ( nCost < 1 ){ nCost = 1; }
							toConsume = (ba.MaxHitPoints - ba.HitPoints) * nCost;
						}
						else { toConsume = (ba.MaxHitPoints - ba.HitPoints) * nCost; }
                    }
                    else if (ba.HitPoints >= ba.MaxHitPoints)
                    {
						m_Weaver.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.that_does_not_need_to_be_repaired"));
                    }
					else
					{
						m_Weaver.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.i_cannot_repair_that"));
					}

                    if (toConsume == 0)
                        return;

                    if (pack.ConsumeTotal(typeof(Gold), toConsume))
                    {
						if ( BeggingPose(from) > 0 ){ Titles.AwardKarma( from, -BeggingKarma( from ), true ); } // DO ANY KARMA LOSS
                        m_Weaver.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.here_is_your_hat"));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.you_pay_0_gold", toConsume));
                        Effects.PlaySound(from.Location, from.Map, 0x248);
                        ba.MaxHitPoints -= 1;
                        ba.HitPoints = ba.MaxHitPoints;
                    }
                    else
                    {
                        m_Weaver.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.it_would_cost_you_0_gold_to_have_that_repaired", toConsume));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_do_not_have_enough_gold"));
                    }
                }
				else
					m_Weaver.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.that_does_not_need_my_services"));
            }
        }

		public Weaver( Serial serial ) : base( serial )
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