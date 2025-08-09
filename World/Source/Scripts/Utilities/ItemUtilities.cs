using Server.Items;

namespace Server.Utilities
{
	public static class ItemUtilities
	{
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