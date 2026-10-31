using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
	public class GenericSellInfo : IShopSellInfo
	{
		private Dictionary<Type, int> m_Table = new Dictionary<Type, int>();
		private Type[] m_Types;

		// Catalog base names whose Gift/Level counterparts use different type names
		// (e.g. JewelryRing → GiftGoldRing, Scepter → GiftSceptre).
		private static readonly Dictionary<string, string[]> VariantAliases = new Dictionary<string, string[]>
		{
			{ "JewelryRing", new[] {
				"GiftGoldRing", "GiftSilverRing", "LevelGoldRing", "LevelSilverRing",
				"GiftSash", "LevelSash",
				"GiftTorch", "GiftCandle", "GiftLantern"
			} },
			{ "JewelryBracelet", new[] {
				"GiftGoldBracelet", "GiftSilverBracelet", "LevelGoldBracelet", "LevelSilverBracelet"
			} },
			{ "JewelryEarrings", new[] {
				"GiftGoldEarrings", "GiftSilverEarrings", "LevelGoldEarrings", "LevelSilverEarrings"
			} },
			{ "JewelryNecklace", new[] {
				"GiftGoldNecklace", "GiftSilverNecklace", "GiftGoldBeadNecklace", "GiftSilverBeadNecklace", "GiftNecklace",
				"LevelGoldNecklace", "LevelSilverNecklace", "LevelGoldBeadNecklace", "LevelSilverBeadNecklace", "LevelNecklace"
			} },
			{ "TrinketTalisman", new[] {
				"GiftTalismanHoly", "GiftTalismanLeather", "GiftTalismanSnake", "GiftTalismanTotem",
				"LevelTalismanHoly", "LevelTalismanLeather", "LevelTalismanSnake", "LevelTalismanTotem"
			} },
			{ "Scepter", new[] { "GiftSceptre", "LevelSceptre" } },
			{ "WizardStaff", new[] { "GiftStave", "LevelStave" } },
			{ "ClothCowl", new[] { "GiftCowl", "LevelCowl" } },
			{ "ClothHood", new[] { "GiftHood", "LevelHood" } }
		};

		public GenericSellInfo()
		{
		}

		public void Add( Type type, int price )
		{
			if ( type == null )
				return;

			m_Table[type] = price;

			// Magical loot / Legendary variants use GiftX / LevelX types that do not
			// inherit from X — register them at the same base price when they exist.
			TryAddNamedVariant( "Gift" + type.Name, price );
			TryAddNamedVariant( "Level" + type.Name, price );

			string[] aliases;
			if ( VariantAliases.TryGetValue( type.Name, out aliases ) )
			{
				for ( int i = 0; i < aliases.Length; i++ )
					TryAddNamedVariant( aliases[i], price );
			}

			m_Types = null;
		}

		private void TryAddNamedVariant( string typeName, int price )
		{
			Type variant = Server.ScriptCompiler.FindTypeByName( typeName );

			if ( variant == null || !typeof( Item ).IsAssignableFrom( variant ) )
				return;

			if ( !m_Table.ContainsKey( variant ) )
				m_Table[variant] = price;
		}

		public int GetSellPriceFor( Item item, int barter )
		{
			if ( item.Purchased ) return 1; // Previously purchased items should be worth nothing

			int price = 0;
			if ( !m_Table.TryGetValue( item.GetType(), out price ) )
				price = 0;

			price = ItemInformation.AddUpBenefits( item, price, false, false );

			price = (int)(price / 2);
				if ( barter > 0 )
				{
					if ( barter > 100 ){ barter = 100; }
					double nId = 1 + ( barter * 0.01 );
					price = (int)(price * nId);
				}
				if ( price < 1 )
					price = 1;

			return price;
		}

		public int GetBuyPriceFor( Item item )
		{
			return (int)( 1.90 * GetSellPriceFor( item, 0 ) );
		}

		public Type[] Types
		{
			get
			{
				if ( m_Types == null )
				{
					m_Types = new Type[m_Table.Keys.Count];
					m_Table.Keys.CopyTo( m_Types, 0 );
				}

				return m_Types;
			}
		}

		public string GetNameFor( Item item )
		{
			if ( item.Name != null )
				return item.Name;
			else
				return item.LabelNumber.ToString();
		}

		public bool IsSellable( Item item )
		{
			return IsInList( item.GetType() );
		}
	 
		public bool IsResellable( Item item )
		{
			return false;
		}

		public bool IsInList( Type type )
		{
			return m_Table.ContainsKey( type );
		}
	}
}
