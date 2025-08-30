using Server.Engines.Craft;
using Server.Items;
using System;

namespace Server.Engines.GlobalShoppe
{
	public sealed class BlacksmithRewardCalculator : BaseCraftRewardCalculator<EquipmentOrderContext>
	{
		public static readonly BlacksmithRewardCalculator Instance = new BlacksmithRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, EquipmentOrderContext order)
		{
			var gold = base.ComputeGold(context, order);
			if (gold < 1) return gold;
			if (order.Resource == CraftResource.Iron) return gold;

			return (int)(gold * ShoppeOrderConstants.GoldRatios.Blacksmith);
		}

		protected override int ComputePoints(TradeSkillContext context, EquipmentOrderContext order)
		{
			return (int)(base.ComputePoints(context, order) * ShoppeOrderConstants.PointRatios.Blacksmith);
		}

		protected override int ComputeReputation(TradeSkillContext context, EquipmentOrderContext order)
		{
			return (int)(base.ComputeReputation(context, order) * ShoppeOrderConstants.ReputationRatios.Blacksmith);
		}

		protected override CraftItem FindCraftItem(Type type)
		{
			var craftItem = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find blacksmith craft item for '{0}'", type);

			return null;
		}

		protected override Type GetCraftResource(CraftItem craftItem, CraftResource resource)
		{
			if (resource == CraftResource.Iron || resource == CraftResource.None) return typeof(IronIngot);
			if (resource == CraftResource.DullCopper) return typeof(DullCopperIngot);
			if (resource == CraftResource.ShadowIron) return typeof(ShadowIronIngot);
			if (resource == CraftResource.Copper) return typeof(CopperIngot);
			if (resource == CraftResource.Bronze) return typeof(BronzeIngot);
			if (resource == CraftResource.Gold) return typeof(GoldIngot);
			if (resource == CraftResource.Agapite) return typeof(AgapiteIngot);
			if (resource == CraftResource.Verite) return typeof(VeriteIngot);
			if (resource == CraftResource.Valorite) return typeof(ValoriteIngot);
			if (resource == CraftResource.Nepturite) return typeof(NepturiteIngot);
			if (resource == CraftResource.Obsidian) return typeof(ObsidianIngot);
			if (resource == CraftResource.Steel) return typeof(SteelIngot);
			if (resource == CraftResource.Brass) return typeof(BrassIngot);
			if (resource == CraftResource.Mithril) return typeof(MithrilIngot);
			if (resource == CraftResource.Xormite) return typeof(XormiteIngot);
			if (resource == CraftResource.Dwarven) return typeof(DwarvenIngot);

			Console.WriteLine("Failed to find ingot type for '{0}'", craftItem.ItemType);

			return null;
		}

		protected override int GetResourceAmountPerCraft(CraftItem craftItem, CraftResource resource)
		{
			foreach (var o in craftItem.Resources)
			{
				var res = o as CraftRes;
				if (res == null) continue;
				if (res.ItemType == typeof(IronIngot)) return res.Amount;
			}

			Console.WriteLine("Failed to find ingot in resources for '{0}'", craftItem.ItemType);

			return 0;
		}

		protected override double GetResourceMultiplier(CraftResource resource)
		{
			if (CraftResource.DullCopper < resource) return 0;

			int resourceBaseline = (int)CraftResource.DullCopper - 1; // Normalize "None" to just below DullCopper
			int resourceTier = (int)resource - resourceBaseline;
			double materialMultiplier = 0;
			switch (resource)
			{
				case CraftResource.Dwarven:
					materialMultiplier = 5 * Math.Pow((int)CraftResource.Valorite, 2);
					break;

				case CraftResource.Xormite:
					materialMultiplier = 2 * Math.Pow((int)CraftResource.Valorite, 2);
					break;

				default:
					if (resource <= CraftResource.Valorite)
					{
						// Original metal
						materialMultiplier = Math.Pow(resourceTier, 2);
					}
					else if (resource < CraftResource.Dwarven)
					{
						// Custom metals (that aren't handled above)
						materialMultiplier = (1 + 0.05 * (resourceTier - (int)CraftResource.Valorite)) * Math.Pow((int)CraftResource.Valorite, 2); // Every level above Valorite is worth +0.05
					}
					break;
			}

			return materialMultiplier;
		}
	}
}