using Server.Engines.Craft;
using Server.Items;
using System;

namespace Server.Engines.GlobalShoppe
{
	public class CookingRewardCalculator : ResourceSellPriceRewardCalculator
	{
		public static readonly CookingRewardCalculator Instance = new CookingRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			return ComputeRewardFromResourceValue(order.Type, order.MaxAmount);
		}

		protected override int ComputePoints(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) / 1.5);
		}

		protected override int ComputeReputation(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			var reward = ComputeRewardFromResourceValue(order.Type, order.MaxAmount) / 25;

			reward = (int)Math.Max(10, reward - 0.5 * ((double)context.Reputation / ShoppeConstants.MAX_REPUTATION));

			return reward;
		}

		protected override CraftItem FindCraftItem(Type type)
		{
			var craftItem = DefCooking.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find Cooking craft item for '{0}'", type);

			return null;
		}

		protected override int GetSellPrice(Type resourceType)
		{
			if (resourceType == typeof(BaseBeverage)) return 0;

			return base.GetSellPrice(resourceType);
		}
	}
}