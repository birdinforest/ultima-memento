using Server.Engines.Craft;
using System;

namespace Server.Engines.GlobalShoppe
{
	public class AlchemyRewardCalculator : ResourceSellPriceRewardCalculator
	{
		public static readonly AlchemyRewardCalculator Instance = new AlchemyRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.GoldRatios.Alchemist);
		}

		protected override int ComputePoints(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.PointRatios.Alchemist);
		}

		protected override int ComputeReputation(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.ReputationRatios.Alchemist);
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