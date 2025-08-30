using Server.Engines.Craft;
using System;

namespace Server.Engines.GlobalShoppe
{
	public class MorticianRewardCalculator : ResourceSellPriceRewardCalculator
	{
		public static readonly MorticianRewardCalculator Instance = new MorticianRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.GoldRatios.Mortician);
		}

		protected override int ComputePoints(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.PointRatios.Mortician);
		}

		protected override int ComputeReputation(TradeSkillContext context, OrderContext order)
		{
			return (int)(ComputeRewardFromResourceValue(order.Type, order.MaxAmount) * ShoppeOrderConstants.ReputationRatios.Mortician);
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