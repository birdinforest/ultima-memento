using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Avatar
{
	public partial class RewardFactory
	{
		public const int ONE_HUNDRED_GOLD = 1000;
		public const int ONE_THOUSAND_GOLD = 10000;
		public const int TEN_GOLD = 100;
		
		private const int ITEM_ID_JESTER = 0x1E3F; // Bag of Tricks
		private const int ITEM_ID_MYSTIC = 0x6725; // Monk's Tome
		private const int ITEM_ID_SHINOB = 0x5C15; // Shinobi Scroll
		private const int ITEM_ID_DEATH_KNIGHT = 0x6721; // Death Knight book
		private const int ITEM_ID_HOLY_MAN = 0x672B; // Holy Man book

		public static List<IReward> CreateRewards(PlayerMobile from, Categories selectedCategory, PlayerContext context)
		{
			switch (selectedCategory)
			{
				default:
				case Categories.Information:
					{
						// Never reached
						return null;
					}

				case Categories.Ascensions:
					return CreateAscensionRewards(from, context);

				case Categories.Templates:
					return CreateTemplateRewards(from, context);

				case Categories.FullSkillArchive:
					return CreateSkillArchiveRewards(from, context);

				case Categories.PrimaryBoosts:
				case Categories.SecondaryBoosts:
					return CreateSkillBoostRewards(from, selectedCategory == Categories.PrimaryBoosts, context);

				case Categories.Items:
					return CreateItemRewards(from, context);
			}
		}

		private static int ExponentialCost(int baseCost, int level)
		{
			var cost = baseCost;
			if (level <= 0) return cost;

			for (int i = 0; i < level; i++)
			{
				cost *= 2;
			}

			return cost;
		}

		private static int SecondOrderCost(double baseCost, int level)
		{
			return (int)(baseCost * Math.Pow(level, 2) + baseCost * level);
		}
	}
}