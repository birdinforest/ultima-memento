using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Craft
{
	public class BulkCraftGump : Gump
	{
		private readonly PlayerMobile m_Player;

		private readonly BulkCraftContext m_Context;

		public BulkCraftGump(PlayerMobile player, BulkCraftContext context, bool isComplete = false) : base(25, 25)
		{
			m_Player = player;
			m_Context = context;

			AddBackground(0, 0, 180, 170, 2620); // Slate background gold trim

			int x = 10;
			int y = 10;

			if (context.Paused)
				TextDefinition.AddHtmlText(this, x, y - 4, 200, 16, "Session is paused...", false, false, HtmlColors.RED, HtmlColors.RED);
			else if (isComplete)
				TextDefinition.AddHtmlText(this, x, y - 4, 200, 16, "Crafting session complete...", false, false, HtmlColors.RED, HtmlColors.RED);
			else
				TextDefinition.AddHtmlText(this, x, y - 4, 200, 16, string.Format("Crafting '{0}' items...", context.Amount), false, false, LabelColors.OFFWHITE, HtmlColors.OFFWHITE);
			y += 20;

			// Add progress bar
			AddImageTiled(x, y, 95, 9, 9750); // Gray progress
			var step = 95.0 / context.Amount;
			AddProgressBar(x, y, context.Fail + context.Success, step, true);
			AddProgressBar(x, y, context.Fail, step, false);
			y += 20;

			AddLabelWithValue(x, y, "Fail:", context.Fail);
			y += 20;

			AddLabelWithValue(x, y, "Success:", context.Success);
			y += 20;

			AddLabelWithValue(x, y, "Exceptional:", context.Exceptional);
			y += 20;

			if (isComplete)
			{
				AddButtonWithLabel(x, y, "Close", 4);
			}
			else
			{
				if (context.Paused)
					AddButtonWithLabel(x, y, "Resume", 2);
				else
					AddButtonWithLabel(x, y, "Pause", 3);

				y += 28;
				AddButtonWithLabel(x, y, "Stop", 4);
			}
		}

		private void AddLabelWithValue(int x, int y, string label, int value)
		{
			TextDefinition.AddHtmlText(this, x, y - 4, 100, 16, label, false, false, LabelColors.OFFWHITE, HtmlColors.OFFWHITE);
			TextDefinition.AddHtmlText(this, x + 100, y - 4, 65, 16, string.Format("<RIGHT>{0}</RIGHT>", value), false, false, LabelColors.OFFWHITE, HtmlColors.OFFWHITE);
		}

		private void AddButtonWithLabel(int x, int y, string text, int buttonID)
		{
			const int RIGHT_CURVED_ARROW = 4005;
			TextDefinition.AddHtmlText(this, x + 35, y + 3, 100, 16, text, false, false, LabelColors.OFFWHITE, HtmlColors.OFFWHITE);
			AddButton(x, y, RIGHT_CURVED_ARROW, RIGHT_CURVED_ARROW, buttonID, GumpButtonType.Reply, 0);
		}

		private void AddProgressBar(int x, int y, int currentValue, double step, bool isGood)
		{
			if (0 < currentValue)
			{
				if (isGood)
					AddImageTiled(x, y, (int)(currentValue * step), 9, 9752); // Green progress
				else
					AddImageTiled(x, y, (int)(currentValue * step), 9, 40); // Red progress
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID < 1) return;

			switch (info.ButtonID)
			{
				case 1: // Stop showing gump
					m_Context.Suppressed = true;
					break;

				case 2: // Resume
					m_Context.Paused = false;
					break;

				case 3: // Pause
					m_Context.Paused = true;
					m_Player.CloseGump(typeof(BulkCraftGump));
					m_Player.SendGump(new BulkCraftGump(m_Player, m_Context, false));
					break;

				case 4: // Stop
					m_Context.Suppressed = true;
					m_Player.SendMessage("You crafted {0} of {1} items ({2} failed, {3} exceptional).", m_Context.Success, m_Context.Current, m_Context.Fail, m_Context.Exceptional);
					BulkCraft.StopTimer(m_Player);
					break;
			}
		}
	}
}