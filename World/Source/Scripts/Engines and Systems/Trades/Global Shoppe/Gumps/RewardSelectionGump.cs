using System;
using Server.Gumps;
using Server.Network;

namespace Server.Engines.GlobalShoppe
{
	public class RewardSelectionGump : Gump
	{
		private enum Actions
		{
			Close = 0,
			SelectGold = 1,
			SelectPoints = 2,
			SelectReputation = 3,
			Claim = 10
		}

		private const int CHECKED_BOX = 0xFB1;
		private const int UNCHECKED_BOX = 0xE19;
		private const int GOLD_ITEM_ID = 3823;
		private const int POINTS_ITEM_ID = 0x0EEC;
		private const int REPUTATION_ITEM_ID = 10283;

		private readonly Mobile m_From;
		private readonly ShoppeBase m_Shoppe;
		private readonly TradeSkillContext m_Context;
		private readonly IOrderContext m_Order;
		private readonly RewardType m_SelectedReward;

		public RewardSelectionGump(
			Mobile from,
			ShoppeBase shoppe,
			TradeSkillContext context,
			IOrderContext order,
			RewardType selectedReward = RewardType.None
			) : base(100, 100)
		{
			m_From = from;
			m_Shoppe = shoppe;
			m_Context = context;
			m_Order = order;
			m_SelectedReward = selectedReward;

			AddPage(0);

			AddBackground(0, 0, 400, 300, 0x1453); //Tan box
			AddImageTiled(8, 8, 384, 284, 2624); // Black box
			AddAlphaRegion(8, 8, 384, 284);

			if (false == (shoppe is IOrderShoppe))
			{
				Console.WriteLine("Invalid shoppe provided.");
				return;
			}

			TextDefinition.AddHtmlText(this, 20, 20, 360, 25, "<CENTER>Order Completed</CENTER>", HtmlColors.MUSTARD);

			TextDefinition.AddHtmlText(this, 20, 50, 360, 40,
				string.Format("{0} thanks you for the {1}. {2}",
					order.Person,
					((IOrderShoppe)m_Shoppe).GetDescription(order).Replace("Craft ", ""),
					m_SelectedReward == RewardType.None ? TextDefinition.GetColorizedText("Choose your fee.", HtmlColors.OFFWHITE) : ""
				), HtmlColors.BROWN);

			int y = 120;
			int BOX_WIDTH = 100;
			int TOTAL_WIDTH = BOX_WIDTH * 3;
			int START_X = (400 - TOTAL_WIDTH) / 2;

			int BOX_X = START_X + (BOX_WIDTH / 2);
			AddRewardOption(Actions.SelectReputation, RewardType.Reputation, BOX_X, y, REPUTATION_ITEM_ID, m_Order.ReputationReward.ToString(), "Reputation");
			BOX_X += BOX_WIDTH;

			AddRewardOption(Actions.SelectGold, RewardType.Gold, BOX_X, y, GOLD_ITEM_ID, m_Order.GoldReward.ToString(), "Gold");
			BOX_X += BOX_WIDTH;

			AddRewardOption(Actions.SelectPoints, RewardType.Points, BOX_X, y, POINTS_ITEM_ID, m_Order.PointReward.ToString(), "Points");

			int BUTTON_Y = 250;
			int CLAIM_BUTTON_X = 200;

			if (m_SelectedReward != RewardType.None)
			{
				AddButton(CLAIM_BUTTON_X, BUTTON_Y, 4023, 4023, (int)Actions.Claim, GumpButtonType.Reply, 0);
				TextDefinition.AddHtmlText(this, CLAIM_BUTTON_X + 35, BUTTON_Y + 3, 100, 20, "Claim Your Fee", HtmlColors.MUSTARD);
			}
		}

		private void AddRewardOption(Actions action, RewardType rewardType, int x, int y, int itemId, string amount, string label)
		{
			bool IS_SELECTED = m_SelectedReward == rewardType;
			bool IS_REPUTATION_ACTION = action == Actions.SelectReputation;

			int BOX_WIDTH = 80;
			int BOX_HEIGHT = 100;
			int BOX_X = x - (BOX_WIDTH / 2);
			int BOX_Y = y - 15;

			if (!IS_REPUTATION_ACTION)
			{
				for (int TILE_Y = 0; TILE_Y < 4; TILE_Y++)
				{
					for (int TILE_X = 0; TILE_X < 3; TILE_X++)
					{
						int TILE_POS_X = BOX_X + 2 + (TILE_X * 23);
						int TILE_POS_Y = BOX_Y + 2 + (TILE_Y * 22);
						AddButton(TILE_POS_X, TILE_POS_Y, 0x9C, 0x9C, (int)action, GumpButtonType.Reply, 0);
					}
				}
			}

			AddBackground(BOX_X, BOX_Y, BOX_WIDTH, BOX_HEIGHT, 0xA3C);

			int ICON_X = x - 22;
			int ICON_Y = y + 5;

			//Reputation icon is 21 wide, so needs a custom offset.
			if (itemId == REPUTATION_ITEM_ID)
			{
				ICON_X = x - 11;
			}

			if (itemId == POINTS_ITEM_ID)
			{
				AddItem(ICON_X, ICON_Y, itemId, 0x44C); // 1072
			}
			else
			{
				AddItem(ICON_X, ICON_Y, itemId);
			}

			TextDefinition.AddHtmlText(this, BOX_X, ICON_Y + 35, BOX_WIDTH, 20,
				string.Format("<CENTER>{0}</CENTER>", amount),
				IS_SELECTED ? HtmlColors.WHITE : HtmlColors.MUSTARD);

			TextDefinition.AddHtmlText(this, BOX_X, ICON_Y + 55, BOX_WIDTH, 20,
				string.Format("<CENTER>{0}</CENTER>", label),
				IS_SELECTED ? HtmlColors.WHITE : HtmlColors.BROWN);

			if (!IS_REPUTATION_ACTION)
			{
				//Checkbox must be reversed if IS_SELECTED
				if (IS_SELECTED)
				{
					AddButton(x - 15, ICON_Y + 85, CHECKED_BOX, UNCHECKED_BOX, (int)action, GumpButtonType.Reply, 0);
				}
				else
				{
					AddButton(x - 15, ICON_Y + 85, UNCHECKED_BOX, CHECKED_BOX, (int)action, GumpButtonType.Reply, 0);
				}
				if (m_SelectedReward != RewardType.None && m_SelectedReward != rewardType)
					AddAlphaRegion(BOX_X, BOX_Y, BOX_WIDTH, BOX_HEIGHT);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var BUTTON_ID = info.ButtonID;

			switch ((Actions)BUTTON_ID)
			{
				case Actions.Close:
					m_Shoppe.OpenGump(m_From, false);
					break;

				case Actions.SelectGold:
				case Actions.SelectPoints:
				case Actions.SelectReputation:
					RewardType newSelection = (RewardType)BUTTON_ID;
					sender.Mobile.SendGump(new RewardSelectionGump(
						m_From,
						m_Shoppe,
						m_Context,
						m_Order,
						newSelection == m_SelectedReward ? RewardType.None : newSelection
					));
					break;

				case Actions.Claim:
					if (m_SelectedReward != RewardType.None)
					{
						((IOrderShoppe)m_Shoppe).CompleteOrder(m_Order, m_From, m_Context, m_SelectedReward);
						m_Shoppe.OpenGump(m_From, false);
					}
					break;
			}
		}

	}
}