using System.Collections.Generic;
using Server.Engines.MLQuests;
using Server.Engines.MLQuests.Definitions;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Rewards
{
	/// <summary>
	/// Drops a blessed counsel sash (+3 Str/Dex/Int by ending); caption follows Mara branch.
	/// </summary>
	public sealed class UnsentLetterEndingMementoReward : BaseReward
	{
		public UnsentLetterEndingMementoReward()
			: base("quest-unsent-letter-reward-memento-preview-001")
		{
		}

		public static string ResolveCaptionKey(MLQuestInstance inst)
		{
			switch (UnsentLetterQuestHelper.GetFamilyEndingChoice(inst))
			{
				case 1:
					return "quest-unsent-letter-reward-memento-public-caption-001";
				case 2:
					return "quest-unsent-letter-reward-memento-private-caption-001";
				case 3:
					return "quest-unsent-letter-reward-memento-quiet-caption-001";
				default:
					return "quest-unsent-letter-reward-memento-preview-001";
			}
		}

		public override void AddRewardItems(PlayerMobile pm, List<Item> rewards)
		{
			if (pm == null)
				return;

			MLQuestInstance inst = UnsentLetterQuestHelper.FindAnyUnsentLetterInstance(pm);
			int c = UnsentLetterQuestHelper.GetFamilyEndingChoice(inst);

			if (c < 1 || c > 3)
				c = 1;

			rewards.Add(new UnsentLetterCounselSash(c));
		}
	}
}
