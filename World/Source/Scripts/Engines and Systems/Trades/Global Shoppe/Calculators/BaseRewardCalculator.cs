using Server.Engines.Craft;
using System;
using System.Linq;

namespace Server.Engines.GlobalShoppe
{
	public abstract class BaseRewardCalculator
	{
		protected int ScaleReputation(TradeSkillContext context, int reputation)
		{
			// Minimum of 10
			// Reduce it based on how far the Shoppe's current reputation is from the maximum
			return (int)Math.Max(10, reputation - 0.5 * ((double)context.Reputation / ShoppeConstants.MAX_REPUTATION));
		}

		protected int GetResourcePerCraft(CraftItem craftItem, Type baseResourceType)
		{
			int resourcePerCraft = 0;
			for (int i = 0; i < craftItem.Resources.Count; i++)
			{
				var resource = craftItem.Resources.GetAt(i);
				if (resource == null) break;

				if (baseResourceType.IsAssignableFrom(resource.ItemType))
				{
					resourcePerCraft = resource.Amount;
					break;
				}
			}

			return resourcePerCraft;
		}

		protected virtual int GetSellPrice(Type resourceType)
		{
			var sellInfo = ItemSalesInfo.m_SellingInfo.FirstOrDefault(info => info.ItemsType == resourceType);
			if (sellInfo == null)
			{
				Console.WriteLine("Failed to find item price for '{0}'", resourceType);
				return 0;
			}

			return sellInfo.iPrice;
		}
	}
}