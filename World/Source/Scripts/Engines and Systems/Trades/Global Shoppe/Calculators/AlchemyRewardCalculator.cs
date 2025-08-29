using Server.Engines.Craft;
using System;

namespace Server.Engines.GlobalShoppe
{
	public class AlchemyRewardCalculator : ResourceSellPriceRewardCalculator
	{
		public static readonly AlchemyRewardCalculator Instance = new AlchemyRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			return ComputeRewardFromResourceValue(order.Type, order.MaxAmount) / 3;
		}

		protected override int ComputePoints(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			return ComputeRewardFromResourceValue(order.Type, order.MaxAmount) / 5;
		}

		protected override int ComputeReputation(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			var reward = ComputeRewardFromResourceValue(order.Type, order.MaxAmount) / 50;

			reward = (int)Math.Max(10, reward - 0.5 * ((double)context.Reputation / ShoppeConstants.MAX_REPUTATION));

			return reward;
		}

		protected override CraftItem FindCraftItem(Type type)
		{
			var craftItem = DefAlchemy.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find Alchemy craft item for '{0}'", type);

			return null;
		}
	}
}