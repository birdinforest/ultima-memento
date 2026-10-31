using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using System.Collections;
using Server.Network;
using Server.Misc;
using Server.Mobiles;
using Server.Gumps;

namespace Server.Items
{
	public interface IGiftable
	{
		string Gifter{ get; set; }
		string How{ get; set; }
		Mobile Owner{ get; set; }
		int Points{ get; set; }
	}

	/// <summary>
	/// Maps known m_How string values to shotkeys for localized OPL display.
	/// </summary>
	public static class GiftHowHelper
	{
		private static readonly Dictionary<string, string> s_HowToKey = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
		{
			{ "Unearthed by", "prop.gift.how.unearthed" },
			{ "Found by",     "prop.gift.how.found" },
			{ "Tribute To",   "prop.gift.how.tribute" },
			{ "Given to",     "prop.gift.how.given" },
			{ "Belongs to",   "prop.gift.how.belongs" },
		};

		/// <summary>
		/// Returns the shotkey for a known m_How string, or null if unknown.
		/// When the m_How is unknown (e.g. GM-set via ManualOfItems), callers should fall back to
		/// <c>AddLocalizedProperty(list, "prop.gift.provenance", composedText)</c>.
		/// </summary>
		public static string GetHowKey( string how )
		{
			if ( string.IsNullOrEmpty( how ) )
				return null;

			s_HowToKey.TryGetValue( how, out string key );
			return key;
		}
	}

	/// <summary>
	/// Shared Epic Tribute / Relics skill-slot rules.
	/// Unknown codes apply nothing. They do not fall through to Alchemy.
	/// </summary>
	public static class GiftSkillBonus
	{
		public const int NativeSkillCode = 99;
		public const int MarksmanshipCode = 100;
		public const int EnhancePotionsCode = 100;

		public static bool TryResolve( Item item, int code, out SkillName skill )
		{
			skill = SkillName.Alchemy;

			if ( item == null || code <= 0 )
				return false;

			if ( code == MarksmanshipCode )
			{
				skill = SkillName.Marksmanship;
				return true;
			}

			if ( code == NativeSkillCode )
			{
				BaseWeapon weapon = item as BaseWeapon;
				if ( weapon != null )
				{
					skill = weapon.Skill;
					return true;
				}

				if ( item is BaseShield )
				{
					skill = SkillName.Parry;
					return true;
				}

				return false;
			}

			if ( code >= 1 && code <= 55 )
			{
				skill = ResourceMods.GetSkill( code );
				return true;
			}

			return false;
		}

		public static void Apply( Item item, int val1, int val2, int val3, int val4, int val5, double sk1, double sk2, double sk3, double sk4, double sk5 )
		{
			AosSkillBonuses bonuses = GetBonuses( item );
			AosAttributes attrs = GetAttributes( item );
			if ( bonuses == null || attrs == null )
				return;

			ApplySlot( item, bonuses, 0, val1, sk1 );
			ApplySlot( item, bonuses, 1, val2, sk2 );
			ApplySlot( item, bonuses, 2, val3, sk3 );
			ApplySlot( item, bonuses, 3, val4, sk4 );

			if ( val5 == EnhancePotionsCode )
				attrs.EnhancePotions = (int)sk5;
			else
				ApplySlot( item, bonuses, 4, val5, sk5 );
		}

		private static AosSkillBonuses GetBonuses( Item item )
		{
			if ( item is BaseWeapon )
				return ((BaseWeapon)item).SkillBonuses;
			if ( item is BaseArmor )
				return ((BaseArmor)item).SkillBonuses;
			if ( item is BaseClothing )
				return ((BaseClothing)item).SkillBonuses;
			if ( item is BaseTrinket )
				return ((BaseTrinket)item).SkillBonuses;
			return null;
		}

		private static AosAttributes GetAttributes( Item item )
		{
			if ( item is BaseWeapon )
				return ((BaseWeapon)item).Attributes;
			if ( item is BaseArmor )
				return ((BaseArmor)item).Attributes;
			if ( item is BaseClothing )
				return ((BaseClothing)item).Attributes;
			if ( item is BaseTrinket )
				return ((BaseTrinket)item).Attributes;
			return null;
		}

		private static void ApplySlot( Item item, AosSkillBonuses bonuses, int index, int code, double amount )
		{
			if ( amount <= 0 )
				return;

			SkillName skill = SkillName.Alchemy;
			bool resolved = false;
			if ( index == 0 )
				resolved = TryResolveFirstSlot( item, code, out skill );
			if ( !resolved )
				resolved = TryResolve( item, code, out skill );
			if ( !resolved )
				return;

			bonuses.SetValues( index, skill, amount );
		}

		/// <summary>
		/// Slot 1 only. Code 99: weapon native skill, armor or shield Parry, anything else a random catalog skill.
		/// Code 100: non-ranged weapons roll a random catalog skill. Ranged weapons, armor, clothing, and trinkets stay Marksmanship.
		/// </summary>
		private static bool TryResolveFirstSlot( Item item, int code, out SkillName skill )
		{
			skill = SkillName.Alchemy;

			if ( item == null )
				return false;

			if ( code == NativeSkillCode )
			{
				BaseWeapon weapon = item as BaseWeapon;
				if ( weapon != null )
				{
					skill = weapon.Skill;
					return true;
				}

				if ( item is BaseArmor )
				{
					skill = SkillName.Parry;
					return true;
				}

				skill = RandomCatalogSkill();
				return true;
			}

			if ( code == MarksmanshipCode )
			{
				if ( item is BaseWeapon && !( item is BaseRanged ) )
				{
					skill = RandomCatalogSkill();
					return true;
				}

				skill = SkillName.Marksmanship;
				return true;
			}

			return false;
		}

		private static SkillName RandomCatalogSkill()
		{
			return ResourceMods.GetSkill( Utility.RandomMinMax( 1, 55 ) );
		}
	}
}

namespace Server.ContextMenus
{
	public class GiftInfoEntry : ContextMenuEntry
	{
		private Item m_Item;
		private Mobile m_From;
		private GiftAttributeCategory m_Cat;

		public GiftInfoEntry( Mobile from, Item item, GiftAttributeCategory cat ) : base( 255, 3 )
		{
			m_From = from;
			m_Item = item;
			m_Cat = cat;
		}

		public override void OnClick()
		{
			Owner.From.CloseGump( typeof( GiftGump ) );
			Owner.From.SendGump( new GiftGump( m_From, m_Item, m_Cat ) ); 
		}
	}
}