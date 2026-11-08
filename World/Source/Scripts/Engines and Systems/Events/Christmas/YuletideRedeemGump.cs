using System.Collections.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Events.Christmas
{
	public class YuletideRedeemGump : Gump
	{
		private const int ItemsPerPage = 8;
		private const int CardHeight = 58;
		private const int GumpWidth = 520;
		private const int GumpHeight = 560;

		private readonly Mobile m_From;
		private readonly List<ChristmasRewardEntry> m_Rewards;
		private readonly int m_Page;

		public YuletideRedeemGump(Mobile from) : this(from, 0)
		{
		}

		public YuletideRedeemGump(Mobile from, int page) : base(50, 50)
		{
			m_From = from;
			m_Rewards = ChristmasEvent.GetChristmasRewards();
			m_Page = page;

			from.CloseGump(typeof(YuletideRedeemGump));

			AddPage(0);
			AddBackground(0, 0, GumpWidth, GumpHeight, 9270);
			AddImageTiled(12, 12, GumpWidth - 24, GumpHeight - 24, 2624);
			AddAlphaRegion(12, 12, GumpWidth - 24, GumpHeight - 24);

			TextDefinition.AddHtmlText(this, 20, 20, GumpWidth - 40, 22,
				"<CENTER>Midwinter Favor Exchange</CENTER>", HtmlColors.MUSTARD);

			int moteCount = ChristmasEvent.CountMotes(from);
			TextDefinition.AddHtmlText(this, 20, 48, GumpWidth - 40, 20,
				string.Format("Winter Spirit Motes: {0}", moteCount), HtmlColors.OFFWHITE);

			TextDefinition.AddHtmlText(this, 20, 70, GumpWidth - 40, 36,
				"Offer motes gathered from midwinter battles to claim festive gifts of the season.",
				HtmlColors.BROWN);

			int skip = page * ItemsPerPage;
			int y = 118;

			for (int i = 0; i < ItemsPerPage && (skip + i) < m_Rewards.Count; i++)
			{
				ChristmasRewardEntry reward = m_Rewards[skip + i];
				int index = skip + i;
				bool canAfford = moteCount >= reward.MoteCost;

				AddImageTiled(20, y, GumpWidth - 40, CardHeight - 6, 2624);
				AddItem(28, y + 8, reward.Graphic);
				TextDefinition.AddHtmlText(this, 80, y + 6, 280, 20, reward.DisplayName,
					canAfford ? HtmlColors.OFFWHITE : HtmlColors.GRAY);
				TextDefinition.AddHtmlText(this, 80, y + 28, 280, 18,
					string.Format("{0} motes", reward.MoteCost),
					canAfford ? HtmlColors.MUSTARD : HtmlColors.PALE_RED);

				if (canAfford)
					AddButton(GumpWidth - 70, y + 12, 4005, 4007, 100 + index, GumpButtonType.Reply, 0);

				y += CardHeight;
			}

			int bottom = GumpHeight - 40;

			if (page > 0)
				AddButton(30, bottom, 4014, 4015, 1, GumpButtonType.Reply, 0);

			if (skip + ItemsPerPage < m_Rewards.Count)
				AddButton(GumpWidth - 60, bottom, 4005, 4007, 2, GumpButtonType.Reply, 0);

			AddButton(GumpWidth / 2 - 20, bottom, 4017, 4018, 0, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;
			if (from == null || from != m_From)
				return;

			if (info.ButtonID == 0)
				return;

			if (info.ButtonID == 1)
			{
				from.SendGump(new YuletideRedeemGump(from, m_Page - 1));
				return;
			}

			if (info.ButtonID == 2)
			{
				from.SendGump(new YuletideRedeemGump(from, m_Page + 1));
				return;
			}

			if (info.ButtonID >= 100)
			{
				int index = info.ButtonID - 100;
				if (index < 0 || index >= m_Rewards.Count)
					return;

				ChristmasRewardEntry reward = m_Rewards[index];

				if (!ChristmasEvent.ConsumeMotes(from, reward.MoteCost))
				{
					from.SendMessage("You do not have enough Winter Spirit Motes.");
					from.SendGump(new YuletideRedeemGump(from, m_Page));
					return;
				}

				Item item = reward.Create();
				if (item == null)
				{
					from.AddToBackpack(new WinterSpiritMote(reward.MoteCost));
					from.SendMessage("The favor failed to take form. Your motes have been returned.");
					from.SendGump(new YuletideRedeemGump(from, m_Page));
					return;
				}

				from.AddToBackpack(item);
				from.SendMessage("You exchange {0} Winter Spirit Motes for {1}.", reward.MoteCost, reward.DisplayName);
				from.SendGump(new YuletideRedeemGump(from, m_Page));
			}
		}
	}
}
