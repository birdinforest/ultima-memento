using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Weaponsmith : BaseVendor
	{
		private List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos{ get { return m_SBInfos; } }
		public override bool IsBlackMarket { get { return true; } }

		public override string TalkGumpTitle{ get{ return "Weapon Repairs"; } }
		public override string TalkGumpSubject{ get{ return "Weaponsmith"; } }

		public override NpcGuild NpcGuild{ get{ return NpcGuild.WarriorsGuild; } }

		[Constructable]
		public Weaponsmith() : base( "the weaponsmith" )
		{
			SetSkill( SkillName.ArmsLore, 64.0, 100.0 );
			SetSkill( SkillName.Blacksmith, 65.0, 88.0 );
			SetSkill( SkillName.Fencing, 45.0, 68.0 );
			SetSkill( SkillName.Bludgeoning, 45.0, 68.0 );
			SetSkill( SkillName.Swords, 45.0, 68.0 );
			SetSkill( SkillName.Tactics, 36.0, 68.0 );
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
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Smith,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Weapon,		ItemSalesInfo.Material.Metal,		ItemSalesInfo.Market.Smith,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.Resource,	ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Smith,		ItemSalesInfo.World.None,	null	 );
				}
			}

			public class InternalSellInfo : GenericSellInfo
			{
				public InternalSellInfo()
				{
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.None,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Smith,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Weapon,		ItemSalesInfo.Material.Metal,		ItemSalesInfo.Market.Smith,		ItemSalesInfo.World.None,	null	 );
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.Resource,	ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Smith,		ItemSalesInfo.World.None,	null	 );
				}
			}
		}

		public override void UpdateBlackMarket()
		{
			base.UpdateBlackMarket();

			if ( IsBlackMarket && MyServerSettings.BlackMarket() )
			{
				int v=4; while ( v > 0 ){ v--;
				ItemInformation.BlackMarketList( this, ItemSalesInfo.Category.Weapon,		ItemSalesInfo.Material.All,		ItemSalesInfo.Market.Smith,		ItemSalesInfo.World.None	 );
				}
			}
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			if ( Utility.RandomBool() ){ AddItem( new Server.Items.Bandana( Utility.RandomNeutralHue() ) ); }
			if ( Utility.RandomBool() ){ AddItem( new Server.Items.SmithHammer() ); }
		}

		public Weaponsmith( Serial serial ) : base( serial )
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