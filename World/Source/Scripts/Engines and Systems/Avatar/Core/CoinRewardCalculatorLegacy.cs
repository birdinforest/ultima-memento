using Server.Items;

namespace Server.Engines.Avatar
{
	public class CoinRewardCalculatorLegacy
	{
		public static int GetCoinValue(Container corpse)
		{
			int value = GetValue<DDCopper>(1, corpse);
			value += GetValue<DDSilver>(2, corpse);
			value += GetValue<DDXormite>(30, corpse);
			value += GetValue<Gold>(10, corpse);
			value += GetValue<Crystals>(50, corpse);
			value += GetValue<DDGemstones>(20, corpse);
			value += GetValue<DDJewels>(20, corpse);
			value += GetValue<DDGoldNuggets>(10, corpse);

			return value;
		}

		private static int GetValue<T>(int multiplier, Container corpse) where T : Item
		{
			var item = corpse.FindItemByType<T>();
			if (item == null) return 0;

			return item.Amount * multiplier;
		}
	}
}