using Server.Engines.Craft;
using System;

namespace Server.Engines.GlobalShoppe
{
	public class HerbalistRewardCalculator : ResourceSellPriceRewardCalculator
	{
		public static readonly HerbalistRewardCalculator Instance = new HerbalistRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.GoldRatios.Herbalist);
		}

		protected override int ComputePoints(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.PointRatios.Herbalist);
		}

		protected override int ComputeReputation(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.ReputationRatios.Herbalist);
		}

		protected override CraftItem FindCraftItem(Type type)
		{
			var craftItem = DefDruidism.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find Herbalist craft item for '{0}'", type);

			return null;
		}
	}
}