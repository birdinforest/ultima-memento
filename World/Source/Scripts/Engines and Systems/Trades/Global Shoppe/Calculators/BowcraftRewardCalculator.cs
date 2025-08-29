using Server.Engines.Craft;
using Server.Items;
using System;

namespace Server.Engines.GlobalShoppe
{
	public sealed class BowcraftRewardCalculator : BaseCraftRewardCalculator<EquipmentOrderContext>
	{
		public static readonly BowcraftRewardCalculator Instance = new BowcraftRewardCalculator();

		protected override int ComputeRewardFromResourceValue(int quantity, bool exceptional, CraftResource resource, Type type)
		{
			return 2 * base.ComputeRewardFromResourceValue(quantity, exceptional, resource, type);
		}

		protected override CraftItem FindCraftItem(Type type)
		{
			var craftItem = DefBowFletching.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find fletching craft item for '{0}'", type);

			return null;
		}

		protected override Type GetCraftResource(CraftItem craftItem, CraftResource resource)
		{
			if (resource == CraftResource.RegularWood || resource == CraftResource.None) return typeof(Board);
			if (resource == CraftResource.AshTree) return typeof(AshBoard);
			if (resource == CraftResource.CherryTree) return typeof(CherryBoard);
			if (resource == CraftResource.EbonyTree) return typeof(EbonyBoard);
			if (resource == CraftResource.GoldenOakTree) return typeof(GoldenOakBoard);
			if (resource == CraftResource.HickoryTree) return typeof(HickoryBoard);
			if (resource == CraftResource.MahoganyTree) return typeof(MahoganyBoard);
			if (resource == CraftResource.OakTree) return typeof(OakBoard);
			if (resource == CraftResource.PineTree) return typeof(PineBoard);
			if (resource == CraftResource.GhostTree) return typeof(GhostBoard);
			if (resource == CraftResource.RosewoodTree) return typeof(RosewoodBoard);
			if (resource == CraftResource.WalnutTree) return typeof(WalnutBoard);
			if (resource == CraftResource.PetrifiedTree) return typeof(PetrifiedBoard);
			if (resource == CraftResource.DriftwoodTree) return typeof(DriftwoodBoard);
			if (resource == CraftResource.ElvenTree) return typeof(ElvenBoard);

			Console.WriteLine("Failed to find wood type for '{0}'", craftItem.ItemType);

			return null;
		}

		protected override int GetResourceAmountPerCraft(CraftItem craftItem, CraftResource resource)
		{
			foreach (var o in craftItem.Resources)
			{
				var res = o as CraftRes;
				if (res == null) continue;
				if (res.ItemType == typeof(Board)) return res.Amount;
			}

			Console.WriteLine("Failed to find board in resources for '{0}'", craftItem.ItemType);

			return 0;
		}

		protected override double GetResourceMultiplier(CraftResource resource)
		{
			if (resource >= CraftResource.AshTree)
			{
				int resourceBaseline = (int)CraftResource.AshTree - 1; // Normalize "None" to just below Ash
				int resourceTier = (int)resource - resourceBaseline;
				double materialMultiplier = 0;
				switch (resource)
				{
					case CraftResource.ElvenTree:
						materialMultiplier = 5 * Math.Pow((int)CraftResource.Valorite, 2);
						break;

					default:
						if (resource <= CraftResource.PineTree)
						{
							// Original metal
							materialMultiplier = Math.Pow(resourceTier, 2);
						}
						else if (resource < CraftResource.ElvenTree)
						{
							// Custom metals (that aren't handled above)
							materialMultiplier = (1 + 0.05 * (resourceTier - (int)CraftResource.Valorite)) * Math.Pow((int)CraftResource.Valorite, 2); // Every level above Valorite is worth +0.05
						}
						break;
				}

				return materialMultiplier;
			}

			return 0;
		}
	}
}