using Server.Engines.Craft;
using Server.Items;
using System;

namespace Server.Engines.GlobalShoppe
{
	public sealed class TailorRewardCalculator : BaseCraftRewardCalculator<EquipmentOrderContext>
	{
		public static readonly TailorRewardCalculator Instance = new TailorRewardCalculator();

		protected override int ComputeGold(TradeSkillContext context, EquipmentOrderContext order)
		{
			var gold = base.ComputeGold(context, order);
			if (gold < 1) return gold;
			if (order.Resource == CraftResource.RegularLeather) return gold;

			return (int)(gold * ShoppeOrderConstants.GoldRatios.Tailor);
		}

		protected override int ComputePoints(TradeSkillContext context, EquipmentOrderContext order)
		{
			return (int)(base.ComputePoints(context, order) * ShoppeOrderConstants.PointRatios.Tailor);
		}

		protected override int ComputeReputation(TradeSkillContext context, EquipmentOrderContext order)
		{
			return (int)(base.ComputeReputation(context, order) * ShoppeOrderConstants.ReputationRatios.Tailor);
		}
		protected override int ComputeRewardFromResourceValue(int quantity, bool exceptional, CraftResource resource, Type type)
		{
			return 2 * base.ComputeRewardFromResourceValue(quantity, exceptional, resource, type);
		}

		protected override CraftItem FindCraftItem(Type type)
		{
			var craftItem = DefLeatherworking.CraftSystem.CraftItems.SearchFor(type);
			if (craftItem != null) return craftItem;

			Console.WriteLine("Failed to find tailor craft item for '{0}'", type);

			return null;
		}

		protected override Type GetCraftResource(CraftItem craftItem, CraftResource resource)
		{
			if (resource == CraftResource.RegularWood || resource == CraftResource.None) return typeof(Leather);
			if (resource == CraftResource.HornedLeather) return typeof(HornedLeather);
			if (resource == CraftResource.BarbedLeather) return typeof(BarbedLeather);
			if (resource == CraftResource.NecroticLeather) return typeof(NecroticLeather);
			if (resource == CraftResource.VolcanicLeather) return typeof(VolcanicLeather);
			if (resource == CraftResource.FrozenLeather) return typeof(FrozenLeather);
			if (resource == CraftResource.SpinedLeather) return typeof(SpinedLeather);
			if (resource == CraftResource.GoliathLeather) return typeof(GoliathLeather);
			if (resource == CraftResource.DraconicLeather) return typeof(DraconicLeather);
			if (resource == CraftResource.HellishLeather) return typeof(HellishLeather);
			if (resource == CraftResource.DinosaurLeather) return typeof(DinosaurLeather);
			if (resource == CraftResource.AlienLeather) return typeof(AlienLeather);

			Console.WriteLine("Failed to find leather type for '{0}'", craftItem.ItemType);

			return null;
		}

		protected override int GetResourceAmountPerCraft(CraftItem craftItem, CraftResource resource)
		{
			foreach (var o in craftItem.Resources)
			{
				var res = o as CraftRes;
				if (res == null) continue;
				if (res.ItemType == typeof(Leather)) return res.Amount;
			}

			Console.WriteLine("Failed to find leather in resources for '{0}'", craftItem.ItemType);

			return 0;
		}

		protected override double GetResourceMultiplier(CraftResource resource)
		{
			if (resource >= CraftResource.HornedLeather)
			{
				int resourceBaseline = (int)CraftResource.HornedLeather - 1; // Normalize "None" to just below Horned
				int resourceTier = (int)resource - resourceBaseline;
				double materialMultiplier = 0;
				switch (resource)
				{
					case CraftResource.HellishLeather:
						materialMultiplier = (1.15f * Math.Pow((int)CraftResource.Valorite, 2));
						break;

					case CraftResource.DinosaurLeather:
						materialMultiplier = (1.25f * Math.Pow((int)CraftResource.Valorite, 2));
						break;

					case CraftResource.AlienLeather:
						materialMultiplier = 5 * Math.Pow((int)CraftResource.Valorite, 2);
						break;

					default:
						if (resource <= CraftResource.DraconicLeather)
						{
							materialMultiplier = Math.Pow(resourceTier, 2);
						}
						break;
				}

				return materialMultiplier;
			}

			return 0;
		}
	}
}