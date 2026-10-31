using System.Collections.Generic;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Rewards
{
	public class FameReward : BaseReward
	{
		public int Amount { get; private set; }

		public FameReward(int amount) : base("Fame")
		{
			Amount = amount;
		}

		public override void AddRewardItems(PlayerMobile pm, List<Item> rewards)
		{
		}

		public void Give(PlayerMobile pm)
		{
			Titles.AwardFame(pm, Amount, true);
		}
	}
}
