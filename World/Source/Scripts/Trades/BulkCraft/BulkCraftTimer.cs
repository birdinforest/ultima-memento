using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Craft
{
	public class BulkCraftTimer : Timer
	{
		public readonly PlayerMobile Player;

		private readonly BulkCraftContext m_Context;
		private readonly CraftSystem m_CraftSystem;
		private readonly CraftItem m_CraftItem;
		private readonly BaseTool m_Tool;
		private readonly Type m_TypeRes;

		public BulkCraftTimer(CraftItem craftItem, PlayerMobile player, CraftSystem craftSystem, BaseTool tool, Type typeRes, int amount) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1))
		{
			Player = player;
			m_CraftItem = craftItem;
			m_CraftSystem = craftSystem;
			m_Tool = tool;
			m_TypeRes = typeRes;
			m_Context = new BulkCraftContext { Amount = amount };
		}

		protected override void OnTick()
		{
			if (m_CraftItem.Craft(Player, m_CraftSystem, m_TypeRes, m_Tool, m_Context))
				Player.EndAction(typeof(CraftSystem));

			if (m_Context.Amount <= m_Context.Current || m_Tool.Deleted)
				Cancel(true);
		}

		public void Cancel(bool useTool = false)
		{
			if (!Running) return;

			Player.SendMessage("You crafted {0} of {1} items ({2} failed, {3} exceptional).", m_Context.Success, m_Context.Current, m_Context.Fail, m_Context.Exceptional);

			if (useTool && !m_Tool.Deleted)
				m_Tool.OnDoubleClick(Player);

			Stop();
		}
	}
}