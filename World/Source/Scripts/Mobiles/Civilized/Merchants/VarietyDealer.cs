using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.ContextMenus;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Accounting;
using Server.Localization;

namespace Server.Mobiles
{
	public class VarietyDealer : BaseVendor
	{
		private List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos{ get { return m_SBInfos; } }

		public override string TalkGumpTitle{ get{ return "The Hunt For Relics"; } }
		public override string TalkGumpSubject{ get{ return "Variety"; } }

		public override NpcGuild NpcGuild{ get{ return NpcGuild.MerchantsGuild; } }

		[Constructable]
		public VarietyDealer() : base( "the art collector" )
		{
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
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Art,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Rare,		ItemSalesInfo.Material.All,			ItemSalesInfo.Market.All,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Christmas,	ItemSalesInfo.Material.All,			ItemSalesInfo.Market.All,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Halloween,	ItemSalesInfo.Material.All,			ItemSalesInfo.Market.All,		ItemSalesInfo.World.None,	null	 );
				}
			}

			public class InternalSellInfo : GenericSellInfo
			{
				public InternalSellInfo()
				{
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Art,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Rare,		ItemSalesInfo.Material.All,			ItemSalesInfo.Market.All,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Christmas,	ItemSalesInfo.Material.All,			ItemSalesInfo.Market.All,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Halloween,	ItemSalesInfo.Material.All,			ItemSalesInfo.Market.All,		ItemSalesInfo.World.None,	null	 );
				}
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( dropped is Gold )
			{
				string sMessage = "";

				if ( dropped.Amount == 500 && Server.Items.MuseumBook.IsEnabled() )
				{
					if (	Server.Misc.PlayerSettings.GetDiscovered( from, Land.Sosaria ) && 
							Server.Misc.PlayerSettings.GetDiscovered( from, Land.Lodoria ) && 
							Server.Misc.PlayerSettings.GetDiscovered( from, Land.UmberVeil ) && 
							Server.Misc.PlayerSettings.GetDiscovered( from, Land.Ambrosia ) && 
							Server.Misc.PlayerSettings.GetDiscovered( from, Land.Serpent ) && 
							Server.Misc.PlayerSettings.GetDiscovered( from, Land.IslesDread ) && 
							Server.Misc.PlayerSettings.GetDiscovered( from, Land.Savaged ) && 
							Server.Misc.PlayerSettings.GetDiscovered( from, Land.Kuldar ) && 
							Server.Misc.PlayerSettings.GetDiscovered( from, Land.Underworld )
					)
					{
						if ( AlreadyHasBook( from ) )
						{
							this.PublicOverheadMessage( MessageType.Regular, 0, false, Server.Localization.StringCatalog.ResolveByKey(null, "mob.other.here_i_see_you_already_have_a_book") ); 
						}
						else if ( PlayerSettings.GetKeys( from, "Antiques" ) )
						{
							this.PublicOverheadMessage( MessageType.Regular, 0, false, Server.Localization.StringCatalog.ResolveByKey(null, "mob.other.thank_you_but_you_already_done_that_for_me") ); 
						}
						else
						{
							MuseumBook book = new MuseumBook();
							from.PlaySound( 0x2E6 );
							book.ArtOwner = from;
							from.AddToBackpack( book );
							this.PublicOverheadMessage( MessageType.Regular, 0, false, Server.Localization.StringCatalog.ResolveByKey(null, "mob.other.good_luck_with_the_search") ); 
							PlayerSettings.SetKeys( from, "Antiques", true );
							dropped.Delete();
						}
					}
					else
					{
						sMessage = Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_need_to_discover_the_nine_lands_before_i_share_this");
						from.AddToBackpack ( dropped );
					}
				}
				else
				{
					sMessage = Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_look_like_you_need_this_more_than_i_do");
					from.AddToBackpack ( dropped );
				}

				this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sMessage, from.NetState);
			}
			else if ( dropped is MuseumBook )
			{
				MuseumBook book = (MuseumBook)dropped;
				string sMessage = "";
				if ( book.ArtOwner != from )
				{
					sMessage = Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.this_book_doesn_t_belong_to_you_so_i_will_just_get_rid");
					bool remove = true;
					foreach ( Account a in Accounts.GetAccounts() )
					{
						if (a == null)
							break;

						int index = 0;

						for (int i = 0; i < a.Length; ++i)
						{
							Mobile m = a[i];

							if (m == null)
								continue;

							if ( m == book.ArtOwner )
							{
								m.AddToBackpack( dropped );
								remove = false;
							}

							++index;
						}
					}
					if ( remove )
					{
						dropped.Delete();
					}
				}
				else if ( MuseumBook.GetNext( book ) > 60 )
				{
					PlayerSettings.SetKeys( from, "Museums", true );
					from.SendSound( 0x3D );
					from.AddToBackpack ( new BankCheck( MuseumBook.QuestValue() ) );
					sMessage = Server.Localization.StringCatalog.ResolveFormatByKey(from.Account, "mob.fmt.you_have_done_the_museum_a_great_service_here_is_0_gold", MuseumBook.QuestValue() );
					from.Fame = 15000;
					from.SendMessage( Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_have_gained_a_really_large_amount_of_fame") );
					dropped.Delete();
				}
				else
				{
					sMessage = Server.Localization.StringCatalog.ResolveByKey(from.Account, "mob.other.you_have_not_finished_your_search_yet");
				}
				this.PrivateOverheadMessage(MessageType.Regular, 1153, false, sMessage, from.NetState);
			}

			return base.OnDragDrop( from, dropped );
		}

		public static bool AlreadyHasBook( Mobile from ) /////////////////////////////////////////////////////////////////////////////////////////////
		{
			bool HasBook = false;

			ArrayList targets = new ArrayList();
			foreach ( Item item in World.Items.Values )
			{
				if ( item is MuseumBook )
				{
					MuseumBook book = (MuseumBook)item;
					if ( book.ArtOwner == from )
						targets.Add( item );
				}
			}
			for ( int i = 0; i < targets.Count; ++i )
			{
				Item item = ( Item )targets[ i ];
				from.AddToBackpack( item );
				HasBook = true;
			}

			return HasBook;
		}

		///////////////////////////////////////////////////////////////////////////

		public VarietyDealer( Serial serial ) : base( serial )
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