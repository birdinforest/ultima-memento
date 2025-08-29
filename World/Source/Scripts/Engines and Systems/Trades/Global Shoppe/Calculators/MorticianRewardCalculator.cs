using Server.Engines.Craft;
using System;

namespace Server.Engines.GlobalShoppe
{
	public class MorticianRewardCalculator : ResourceSellPriceRewardCalculator
	{
		public static readonly MorticianRewardCalculator Instance = new MorticianRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			return ComputeRewardFromResourceValue(order.Type, order.MaxAmount) / 2;
		}

		protected override int ComputePoints(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) / 2.5);
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
			var craftItem = DefWitchery.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find Mortician craft item for '{0}'", type);

			return null;
		}
	}
}