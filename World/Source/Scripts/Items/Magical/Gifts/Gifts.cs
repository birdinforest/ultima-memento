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