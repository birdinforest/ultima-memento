using Server.Engines.Craft;
using Server.Items;
using System;

namespace Server.Engines.GlobalShoppe
{
	public sealed class TinkerRewardCalculator : BaseCraftRewardCalculator<TinkerOrderContext>
	{
		public static readonly TinkerRewardCalculator Instance = new TinkerRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, TinkerOrderContext order)
		{
			var pricePerCraftedItem = ComputePricePerCraftedItem(order.Resource, order.Type);
			if (pricePerCraftedItem < 1) return 0;

			var gemTypePrice = GetGemTypeSellPrice(order.GemType);
			if (0 < gemTypePrice) pricePerCraftedItem += gemTypePrice;

			double price = order.MaxAmount * pricePerCraftedItem;

			// Exceptional bonus
			if (order.RequireExceptional)
				price *= 1.25;

			var resourceMultiplier = CraftResources.GetGold(order.Resource);
			if (0 < resourceMultiplier)
				price = (int)(price * resourceMultiplier);

			// Reduce by arbitrary amount
			return (int)(price / 2);
		}

		protected override int ComputePoints(TradeSkillContext context, TinkerOrderContext order)
		{
			// Reduce by arbitrary amount
			return ComputeRewardFromResourceValue(order.MaxAmount, order.RequireExceptional, order.Resource, order.GemType, order.Type) / 3;
		}

		protected override int ComputeReputation(TradeSkillContext context, TinkerOrderContext order)
		{
			// Reduce by arbitrary amount
			var reward = ComputeRewardFromResourceValue(order.MaxAmount, order.RequireExceptional, order.Resource, order.GemType, order.Type) / 50;

			reward = (int)Math.Max(10, reward - 0.5 * ((double)context.Reputation / ShoppeConstants.MAX_REPUTATION));

			return reward;
		}

		protected override CraftItem FindCraftItem(Type type)
		{
			var craftItem = DefTinkering.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find tinkering craft item for '{0}'", type);

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

		private int ComputeRewardFromResourceValue(int quantity, bool exceptional, CraftResource resource, GemType gemType, Type type)
		{
			var pricePerCraftedItem = ComputePricePerCraftedItem(resource, type);
			if (pricePerCraftedItem < 1) return 0;

			var gemTypePrice = GetGemTypeSellPrice(gemType);
			if (gemType == GemType.Pearl) gemTypePrice *= 2; // Double the value for Pearls
			if (0 < gemTypePrice) pricePerCraftedItem += gemTypePrice;

			double points = quantity * pricePerCraftedItem;

			// Exceptional bonus
			if (exceptional)
				points *= 1.25;

			// Flat material bonus
			var materialMultiplier = GetResourceMultiplier(resource);
			if (0 < materialMultiplier)
				points += (int)(quantity * materialMultiplier + materialMultiplier); // Major bonus per item + Flat material bonus

			return (int)points;
		}

		private int GetGemTypeSellPrice(GemType gemType)
		{
			return ItemInformation.GetValue(gemType);
		}
	}
}