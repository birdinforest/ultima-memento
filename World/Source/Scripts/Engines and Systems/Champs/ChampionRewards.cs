using System;

namespace Server.Engines.CannedEvil
{
	public class ChampionRewards
	{
		public static int GetBossItemDropChance(int spawnSize, Difficulty difficulty)
		{
			return 100;
		}

		public static int GetGoldPercent(int spawnSize, Difficulty difficulty)
		{
			const int FLAT_BONUS = 25;

			// 10% per Difficulty -- up to 60%
			var difficultyBonus = 10 * (int)difficulty;

			// 5% per Size -- up to 60%
			var sizeBonus = 5 * spawnSize;

			// Size starts at 1, so flat + 5% = 30% base
			return Math.Min(100, FLAT_BONUS + difficultyBonus + sizeBonus);
		}

		public static int GetArtifactDropChance(int spawnSize, Difficulty difficulty)
		{
			// 10% per Difficulty -- up to 60%
			var difficultyBonus = 10 * (int)difficulty;

			// 5% per Size -- up to 60%
			var sizeBonus = 5 * spawnSize;

			return Math.Min(100, difficultyBonus + sizeBonus);
		}

		public static int GetPowerscrollDropCount(int spawnSize, Difficulty difficulty)
		{
			// 1 per Difficulty -- up to 6
			var difficultyBonus = 1 + (int)difficulty;

			// 1 per 3 Size -- up to 4
			var sizeBonus = spawnSize / 3;

			return difficultyBonus + sizeBonus;
		}

		public static int GetTreasureChestDropChance(int spawnSize, Difficulty difficulty)
		{
			// 20% per Difficulty -- up to 60%
			var difficultyBonus = 20 * (int)difficulty;

			// 10% per Size -- up to 120%
			var sizeBonus = 10 * spawnSize;

			return Math.Min(100, difficultyBonus + sizeBonus);
		}
	}
}