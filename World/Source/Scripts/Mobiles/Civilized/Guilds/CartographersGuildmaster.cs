using System;
using System.Collections.Generic;
using Server;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.ContextMenus;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

namespace Server.Mobiles
{
	public class CartographersGuildmaster : BaseGuildmaster
	{
		public override NpcGuild NpcGuild{ get{ return NpcGuild.CartographersGuild; } }

		public override string TalkGumpTitle{ get{ return "X Marks The Spot"; } }
		public override string TalkGumpSubject{ get{ return "Mapmaker"; } }

		[Constructable]
		public CartographersGuildmaster() : base( "cartographer" )
		{
			SetSkill( SkillName.Cartography, 90.0, 100.0 );
		}

		public override void InitSBInfo( Mobile m )
		{
			m_Merchant = m;
			SBInfos.Add( new MyStock() );
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
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.All,		ItemSalesInfo.Material.All,		ItemSalesInfo.Market.Cartographer,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.All,		ItemSalesInfo.Material.All,		ItemSalesInfo.Market.Cartographer,		ItemSalesInfo.World.None,	typeof( BlankScroll )	 );
				}
			}

			public class InternalSellInfo : GenericSellInfo
			{
				public InternalSellInfo()
				{
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.All,		ItemSalesInfo.Material.All,		ItemSalesInfo.Market.Cartographer,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.All,		ItemSalesInfo.Material.All,		ItemSalesInfo.Market.Cartographer,		ItemSalesInfo.World.None,	typeof( BlankScroll )	 );
				}
			}
		}

		private class FixEntry : ContextMenuEntry
		{
			private CartographersGuildmaster m_CartographersGuildmaster;
			private Mobile m_From;

			public FixEntry( CartographersGuildmaster CartographersGuildmaster, Mobile from ) : base( 6120, 12 )
			{
				m_CartographersGuildmaster = CartographersGuildmaster;
				m_From = from;
				Enabled = m_CartographersGuildmaster.CheckVendorAccess( from );
			}

			public override void OnClick()
			{
				m_CartographersGuildmaster.BeginServices( m_From );
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
			int money = 1000;

			double w = money * (MyServerSettings.GetGoldCutRate() * .01);
			money = (int)w;

            if ( Deleted || !from.Alive )
                return;

			int nCost = money;

			if ( BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
			{
				nCost = nCost - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * nCost ); if ( nCost < 1 ){ nCost = 1; }
				SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.since_you_are_begging_do_you_still_want_me_to_decipher", nCost));
			}
			else { SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.if_you_want_me_to_decipher_a_treasure_map_for_you_it_wi", nCost)); }

            from.Target = new RepairTarget(this);
        }

        private class RepairTarget : Target
        {
            private CartographersGuildmaster m_CartographersGuildmaster;

            public RepairTarget(CartographersGuildmaster CartographersGuildmaster) : base(12, false, TargetFlags.None)
            {
                m_CartographersGuildmaster = CartographersGuildmaster;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
				int money = 1000;

				double w = money * (MyServerSettings.GetGoldCutRate() * .01);
				money = (int)w;

                if (targeted is TreasureMap && from.Backpack != null)
                {
                    TreasureMap tmap = targeted as TreasureMap;
                    Container pack = from.Backpack;
                    int toConsume = tmap.Level * money;

					if ( BeggingPose(from) > 0 ) // LET US SEE IF THEY ARE BEGGING
					{
						toConsume = toConsume - (int)( ( from.Skills[SkillName.Begging].Value * 0.005 ) * toConsume );
					}

                    if (toConsume == 0)
                        return;

					if ( tmap.Decoder != null )
					{
						m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.that_map_has_already_been_deciphered"));
					}
                    else if (pack.ConsumeTotal(typeof(Gold), toConsume))
                    {
						if ( BeggingPose(from) > 0 ){ Titles.AwardKarma( from, -BeggingKarma( from ), true ); } // DO ANY KARMA LOSS
						if ( tmap.Level == 1 ){ m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.this_map_was_really_quite_simple")); }
						else if ( tmap.Level == 2 ){ m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.seemed_pretty_easy_so_here_it_is")); }
						else if ( tmap.Level == 3 ){ m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.this_map_was_a_bit_of_a_challenge")); }
						else if ( tmap.Level == 4 ){ m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.whoever_drew_this_map_did_not_want_it_found")); }
						else if ( tmap.Level == 5 ){ m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.this_took_more_research_than_normal")); }
						else { m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.with_the_ancient_writings_and_riddles_this_map_should_n")); }
                        from.SendMessage(Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.you_pay_0_gold", toConsume));
                        Effects.PlaySound(from.Location, from.Map, 0x249);
						tmap.Decoder = from;
                    }
                    else
                    {
                        m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.it_would_cost_you_0_gold_for_me_to_decipher_that_map", toConsume));
                        from.SendMessage(Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_do_not_have_enough_gold"));
                    }
                }
				else
				{
					m_CartographersGuildmaster.SayTo(from, Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.that_does_not_need_my_services"));
				}
            }
        }

		public CartographersGuildmaster( Serial serial ) : base( serial )
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