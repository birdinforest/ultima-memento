using Server.Engines.Craft;
using System;

namespace Server.Engines.GlobalShoppe
{
	public class LibrarianRewardCalculator : ResourceSellPriceRewardCalculator
	{
		public static readonly LibrarianRewardCalculator Instance = new LibrarianRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.GoldRatios.Librarian);
		}

		protected override int ComputePoints(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.PointRatios.Librarian);
		}

		protected override int ComputeReputation(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.ReputationRatios.Librarian);
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