using Server.Engines.Craft;
using System;

namespace Server.Engines.GlobalShoppe
{
	public abstract class ResourceSellPriceRewardCalculator : BaseRewardCalculator
	{
		public void SetRewards(TradeSkillContext context, OrderContext order)
		{
			order.GoldReward = ComputeGold(context, order);
			order.PointReward = ComputePoints(context, order);
			order.ReputationReward = ComputeReputation(context, order);
		}

		protected abstract int ComputeGold(TradeSkillContext context, OrderContext order);

		protected abstract int ComputePoints(TradeSkillContext context, OrderContext order);

		protected int ComputePricePerCraftedItem(Type type)
		{
			// Keep the value of each item in sync with the primary resource investment
			var craftItem = FindCraftItem(type);
			if (craftItem == null) return 0;

			int totalPrice = 0;
			for (int i = 0; i < craftItem.Resources.Count; i++)
			{
				var resource = craftItem.Resources.GetAt(i);
				if (resource == null) break;

				totalPrice += resource.Amount * GetSellPrice(resource.ItemType);
			}

			return totalPrice;
		}

		protected abstract int ComputeReputation(TradeSkillContext context, OrderContext order);

		protected int ComputeRewardFromResourceValue(Type type, int amount)
		{
			var pricePerCraftedItem = ComputePricePerCraftedItem(type);
			if (pricePerCraftedItem < 1) return 0;

			int price = amount * pricePerCraftedItem;

			return price;
		}

		protected abstract CraftItem FindCraftItem(Type type);
	}
}