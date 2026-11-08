using System;
using System.Collections.Generic;
using Server.Engines.ServerServices.Loot;
using Server.Engines.ServerServices.Time;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.Events.Christmas
{
	/// <summary>
	/// Midwinter / Christmas seasonal helpers.
	/// Active during the "christmas" season in world-time.json, the same window that gates Category.Christmas vendor stock.
	/// </summary>
	public static class ChristmasEvent
	{
		public const string SeasonId = "christmas";

		public static bool IsActive
		{
			get { return WorldTime.IsSeasonActive(SeasonId); }
		}

		/// <summary>
		/// Converts a Category.Christmas gold price into Winter Spirit Mote cost.
		/// </summary>
		public static int GoldPriceToMoteCost(int goldPrice)
		{
			return Math.Max(1, goldPrice / 10);
		}

		public static List<ChristmasRewardEntry> GetChristmasRewards()
		{
			var rewards = new List<ChristmasRewardEntry>();
			ItemSalesInfo[] list = ItemSalesInfo.m_SellingInfo;

			for (int i = 0; i < list.Length; i++)
			{
				ItemSalesInfo info = list[i];

				if (info == null || info.ItemsType == null)
					continue;

				if (info.iCategory != ItemSalesInfo.Category.Christmas)
					continue;

				rewards.Add(new ChristmasRewardEntry(
					info.ItemsType,
					info.iPrice,
					GoldPriceToMoteCost(info.iPrice)));
			}

			rewards.Sort((a, b) => a.MoteCost.CompareTo(b.MoteCost));
			return rewards;
		}

		public static int CountMotes(Mobile from)
		{
			if (from == null || from.Backpack == null)
				return 0;

			int total = 0;
			Item[] items = from.Backpack.FindItemsByType(typeof(WinterSpiritMote));

			foreach (Item item in items)
				total += item.Amount;

			return total;
		}

		public static bool ConsumeMotes(Mobile from, int amount)
		{
			if (from == null || from.Backpack == null || amount <= 0)
				return false;

			if (CountMotes(from) < amount)
				return false;

			return from.Backpack.ConsumeTotal(typeof(WinterSpiritMote), amount);
		}

		/// <summary>
		/// Chance-based corpse drop during the christmas season; rates come from the
		/// "christmas.winter_spirit_mote" rule in drop-rates.json.
		/// </summary>
		public static void TryDropMote(BaseCreature creature, Container corpse, Mobile killer, int killerLuck)
		{
			if (creature == null || corpse == null || killer == null)
				return;

			if (creature.IsStabled || creature.Controlled || creature.IsBonded)
				return;

			int amount;
			if (DropRateSystem.Roll("christmas.winter_spirit_mote", creature.Fame, killerLuck, out amount))
				corpse.DropItem(new WinterSpiritMote(amount));
		}
	}

	public class ChristmasRewardEntry
	{
		public Type ItemType { get; private set; }
		public int GoldPrice { get; private set; }
		public int MoteCost { get; private set; }

		public ChristmasRewardEntry(Type itemType, int goldPrice, int moteCost)
		{
			ItemType = itemType;
			GoldPrice = goldPrice;
			MoteCost = moteCost;
		}

		public string DisplayName
		{
			get
			{
				try
				{
					Item item = Activator.CreateInstance(ItemType) as Item;
					if (item != null)
					{
						string name = item.Name;
						if (string.IsNullOrEmpty(name))
							name = item.GetType().Name;
						item.Delete();
						return name;
					}
				}
				catch
				{
				}

				return ItemType.Name;
			}
		}

		public int Graphic
		{
			get
			{
				try
				{
					Item item = Activator.CreateInstance(ItemType) as Item;
					if (item != null)
					{
						int id = item.ItemID;
						item.Delete();
						return id;
					}
				}
				catch
				{
				}

				return 0x232E; // snowflake fallback
			}
		}

		public Item Create()
		{
			try
			{
				return Activator.CreateInstance(ItemType) as Item;
			}
			catch
			{
				return null;
			}
		}
	}
}
