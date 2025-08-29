using Server.Engines.Craft;
using System;

namespace Server.Engines.GlobalShoppe
{
	public class LibrarianRewardCalculator : ResourceSellPriceRewardCalculator
	{
		public static readonly LibrarianRewardCalculator Instance = new LibrarianRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * 0.75);
		}

		protected override int ComputePoints(TradeSkillContext context, OrderContext order)
		{
			// Reduce by arbitrary amount
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) / 2);
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
			var craftItem = DefInscription.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find Librarian craft item for '{0}'", type);

			return null;
		}
	}
}