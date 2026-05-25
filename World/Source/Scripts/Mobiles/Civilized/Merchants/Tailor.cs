using System;
using System.Collections.Generic;
using Server.Targeting;
using Server.Items;
using Server.ContextMenus;
using Server.Misc;

namespace Server.Mobiles
{
	public class Tailor : BaseVendor
	{
		private List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos{ get { return m_SBInfos; } }
		public override bool IsBlackMarket { get { return true; } }

		public override string TalkGumpTitle{ get{ return "Altering Cloaks And Robes"; } }
		public override string TalkGumpSubject{ get{ return "Tailor"; } }

		public override NpcGuild NpcGuild{ get{ return NpcGuild.TailorsGuild; } }

		[Constructable]
		public Tailor() : base( "the tailor" )
		{
			SetSkill( SkillName.Tailoring, 64.0, 100.0 );
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
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.Cloth,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Rare,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Resource,	ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
				}
			}

			public class InternalSellInfo : GenericSellInfo
			{
				public InternalSellInfo()
				{
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.Cloth,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Rare,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Resource,	ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Tailor,		ItemSalesInfo.World.None,	null	 );
				}
			}
		}

		public override void UpdateBlackMarket()
		{
			base.UpdateBlackMarket();

			if ( IsBlackMarket && MyServerSettings.BlackMarket() )
			{
				int v=2; while ( v > 0 ){ v--;
				ItemInformation.BlackMarketList( this, ItemSalesInfo.Category.None,		ItemSalesInfo.Material.Cloth,		ItemSalesInfo.Market.All,		ItemSalesInfo.World.None	 );
				}
			}
		}

		private class FixEntry : ContextMenuEntry
		{
			private Tailor m_Tailor;
			private Mobile m_From;

			public FixEntry( Tailor Tailor, Mobile from ) : base( 6120, 12 )
			{
				m_Tailor = Tailor;
				m_From = from;
				Enabled = Tailor.CheckVendorAccess( from );
			}

			public override void OnClick()
			{
				m_Tailor.BeginServices( m_From );
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
            private Tailor m_Tailor;

            public RepairTarget(Tailor tailor) : base(12, false, TargetFlags.None)
            {
                m_Tailor = tailor;
            }

            protected override void OnTarget(Mobile from, object targeted)
			{
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if ( targeted is BaseOuterTorso && from.Backpack != null )
                {
                    Item ba = targeted as Item;
                    Container pack = from.Backpack;
                    int toConsume = 0;

                    if ( ba.ItemID != 0x1F03 && ba.ItemID != 0x1F04 )
                    {
						int nCost = 5;

						if ( BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
						{
							nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost ); if ( nCost < 1 ){ nCost = 1; }
						}
						toConsume = nCost;
                    }
                    else
                    {
						m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.that_does_not_need_my_services"));
                    }

                    if (toConsume == 0)
                        return;

                    if (pack.ConsumeTotal(typeof(Gold), toConsume))
                    {
						if ( BeggingPose(from) > 0 ){ Titles.AwardKarma( from, -BeggingKarma( from ), true ); } // DO ANY KARMA LOSS
                        m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.here_is_your_robe"));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.you_pay_0_gold", toConsume));
                        Effects.PlaySound(from.Location, from.Map, 0x248);
						ba.ItemID = 0x1F03;
						ba.Name = "robe";
                    }
                    else
                    {
                        m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.it_would_cost_you_0_gold_to_have_that_done", toConsume));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_do_not_have_enough_gold"));
                    }
                }
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                else if ( targeted is BaseCloak && from.Backpack != null )
                {
                    Item ba = targeted as Item;
                    Container pack = from.Backpack;
                    int toConsume = 0;

                    if ( ba.ItemID != 0x1515 && ba.ItemID != 0x1530 )
                    {
						int nCost = 5;

						if ( BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
						{
							nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost ); if ( nCost < 1 ){ nCost = 1; }
						}
						toConsume = nCost;
                    }
                    else
                    {
						m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.that_does_not_need_my_services"));
                    }

                    if (toConsume == 0)
                        return;

                    if (pack.ConsumeTotal(typeof(Gold), toConsume))
                    {
						if ( BeggingPose(from) > 0 ){ Titles.AwardKarma( from, -BeggingKarma( from ), true ); } // DO ANY KARMA LOSS
                        m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.here_is_your_cloak"));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.you_pay_0_gold", toConsume));
                        Effects.PlaySound(from.Location, from.Map, 0x248);
						ba.ItemID = 0x1515;
						ba.Name = "cloak";
                    }
                    else
                    {
                        m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.it_would_cost_you_0_gold_to_have_that_done", toConsume));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_do_not_have_enough_gold"));
                    }
                }
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				else if ( ( targeted is BaseHat || targeted is BaseLevelHat || targeted is BaseGiftHat ) && from.Backpack != null && ((Item)targeted).ItemID != 0x1545 && ((Item)targeted).ItemID != 0x1546 && ((Item)targeted).ItemID != 0x1547 && ((Item)targeted).ItemID != 0x1548 && ((Item)targeted).ItemID != 0x2B6D && ((Item)targeted).ItemID != 0x3164 )
				{
                    BaseClothing ba = targeted as BaseClothing;
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
						m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.that_does_not_need_to_be_repaired"));
                    }
					else
					{
						m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.i_cannot_repair_that"));
					}

                    if (toConsume == 0)
                        return;

                    if (pack.ConsumeTotal(typeof(Gold), toConsume))
                    {
						if ( BeggingPose(from) > 0 ){ Titles.AwardKarma( from, -BeggingKarma( from ), true ); } // DO ANY KARMA LOSS
                        m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.here_is_your_hat"));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.you_pay_0_gold", toConsume));
                        Effects.PlaySound(from.Location, from.Map, 0x248);
                        ba.MaxHitPoints -= 1;
                        ba.HitPoints = ba.MaxHitPoints;
                    }
                    else
                    {
                        m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.it_would_cost_you_0_gold_to_have_that_repaired", toConsume));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_do_not_have_enough_gold"));
                    }
                }
				else
					m_Tailor.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.that_does_not_need_my_services"));
            }
        }

		public Tailor( Serial serial ) : base( serial )
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
