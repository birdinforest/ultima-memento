using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Rewards
{
    public class ConstructibleItemReward : BaseReward
    {
        private readonly Func<PlayerMobile, Item> m_CreateItem;

        public ConstructibleItemReward(Func<PlayerMobile, Item> createItem)
            : this(null, createItem)
        {
        }

        public ConstructibleItemReward(TextDefinition name, Func<PlayerMobile, Item> createItem)
            : base(name)
        {
            m_CreateItem = createItem;
        }

        public override void AddRewardItems(PlayerMobile pm, List<Item> rewards)
        {
            Item reward = m_CreateItem(pm);
            if (reward == null) return;

            rewards.Add(reward);
        }
    }
}
