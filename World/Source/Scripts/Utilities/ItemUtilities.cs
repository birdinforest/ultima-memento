using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Utilities
{
	public static class ItemUtilities
	{
		public static IEnumerable<Item> AddStacks(int totalAmount, Func<Item> createItem)
		{
			const int MAX_STACK_SIZE = 60000;
			int amountRemaining = totalAmount;

			while (0 < amountRemaining)
			{
				Item item = createItem();
				if (MAX_STACK_SIZE < amountRemaining)
				{
					item.Amount = MAX_STACK_SIZE;
					amountRemaining -= MAX_STACK_SIZE;

					yield return item;
				}
				else
				{
					item.Amount = amountRemaining;
					yield return item;
					yield break;
				}
			}
		}

		public static bool IsExceptional(Item item)
		{
			if (item == null) return false;

			if (item is BaseWeapon) return ((BaseWeapon)item).Quality == WeaponQuality.Exceptional;
			if (item is BaseArmor) return ((BaseArmor)item).Quality == ArmorQuality.Exceptional;
			if (item is BaseClothing) return ((BaseClothing)item).Quality == ClothingQuality.Exceptional;
			if (item is BaseInstrument) return ((BaseInstrument)item).Quality == InstrumentQuality.Exceptional;
			if (item is BaseInstrument) return ((BaseInstrument)item).Quality == InstrumentQuality.Exceptional;
			if (item is BaseTool) return ((BaseTool)item).Quality == ToolQuality.Exceptional;
			if (item is BaseHarvestTool) return ((BaseHarvestTool)item).Quality == ToolQuality.Exceptional;

			return false;
		}
	}
}