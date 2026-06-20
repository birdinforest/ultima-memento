using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Engines.MLQuests.Objectives;
using Server.Gumps;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.MLQuests.Gumps
{
	public class QuestLogGump : BaseQuestGump
	{
		private const int ContentTop = 158;
		private const int MaxScrollViewportHeight = 204;
		private const int MLRowHeight = 28;
		private const int ContractPanelX = 90;
		private const int ContractPanelWidth = 366;
		private const int ContractActionButtonX = 368;
		private const int ContractBlockHeight = 120;
		private const int ContractSectionGap = 14;
		private const int StatusVisibleLines = 5;
		private const int StatusLineHeight = 16;
		private const int MinStatusHeight = StatusVisibleLines * StatusLineHeight;
		private const int FooterGap = 58;
		private const int DefaultFooterButtonY = 425;
		private const int ScrollStep = 60;
		private const int BtnScrollUp = 4;
		private const int BtnScrollDown = 5;
		private const int BtnAbandonStandard = 5001;
		private const int BtnAbandonFishing = 5002;
		private const int BtnAbandonAssassin = 5003;

		private PlayerMobile m_Owner;
		private bool m_CloseGumps;
		private int m_ScrollOffset;
		private int m_ViewportHeight;
		private int m_FooterButtonY = DefaultFooterButtonY;

		protected override int FooterButtonY { get { return m_FooterButtonY; } }
		protected override int FooterNavButtonY { get { return m_FooterButtonY - 25; } }

		public QuestLogGump(PlayerMobile pm)
			: this(pm, true, 0)
		{
		}

		public QuestLogGump(PlayerMobile pm, bool closeGumps)
			: this(pm, closeGumps, 0)
		{
		}

		public QuestLogGump(PlayerMobile pm, bool closeGumps, int scrollOffset)
			: base(1046026, pm) // Quest Log
		{
			m_Owner = pm;
			m_CloseGumps = closeGumps;
			m_ScrollOffset = Math.Max(0, scrollOffset);

			if (closeGumps)
			{
				pm.CloseGump(typeof(QuestLogGump));
				pm.CloseGump(typeof(QuestLogDetailedGump));
				pm.CloseGump(typeof(ContractQuestAbandonConfirmGump));
			}

			MLQuestContext context = MLQuestSystem.GetContext(pm);
			List<MLQuestInstance> instances = (context != null) ? context.QuestInstances : null;
			int mlCount = (instances != null) ? instances.Count : 0;

			string statusStd = PlayerSettings.GetQuestState(pm, "StandardQuest") ? StandardQuestFunctions.QuestStatus(pm) : null;
			string statusFish = PlayerSettings.GetQuestState(pm, "FishingQuest") ? FishingQuestFunctions.QuestStatus(pm) : null;
			string statusAss = PlayerSettings.GetQuestState(pm, "AssassinQuest") ? AssassinFunctions.QuestStatus(pm) : null;

			int nContracts = 0;
			if (!string.IsNullOrEmpty(statusStd))
				nContracts++;
			if (!string.IsNullOrEmpty(statusFish))
				nContracts++;
			if (!string.IsNullOrEmpty(statusAss))
				nContracts++;

			int totalHeight = mlCount * MLRowHeight;
			if (nContracts > 0)
				totalHeight += ContractSectionGap + nContracts * ContractBlockHeight;

			int maxScroll = Math.Max(0, totalHeight - MaxScrollViewportHeight);
			if (m_ScrollOffset > maxScroll)
				m_ScrollOffset = maxScroll;

			bool useScrollViewport = maxScroll > 0;
			m_ViewportHeight = useScrollViewport ? MaxScrollViewportHeight : totalHeight;

			if (m_ViewportHeight > 0)
			{
				AddImageTiled(90, ContentTop, 366, m_ViewportHeight, 0xA8E);
				m_FooterButtonY = useScrollViewport
					? DefaultFooterButtonY
					: ContentTop + m_ViewportHeight + FooterGap;
			}
			else
				m_FooterButtonY = DefaultFooterButtonY;

			RegisterButton(ButtonPosition.Right, ButtonGraphic.Okay, 3);
			SetPageCount(1);
			BuildPage();

			int virtualY = 0;

			if (instances != null)
			{
				for (int i = 0; i < instances.Count; ++i)
				{
					if (IsEntryVisible(virtualY, MLRowHeight))
					{
						int screenY = ContentTop + virtualY - m_ScrollOffset;
						AddMLQuestRow(instances[i], i, screenY);
					}

					virtualY += MLRowHeight;
				}
			}

			if (nContracts > 0)
			{
				virtualY += ContractSectionGap;

				if (!string.IsNullOrEmpty(statusStd))
				{
					if (IsEntryVisible(virtualY, ContractBlockHeight))
					{
						int screenY = ContentTop + virtualY - m_ScrollOffset;
						AddContractBlock(screenY, ContractBlockHeight, ResolveText(pm, "Seeking Brave Adventurers"), statusStd, BtnAbandonStandard);
					}

					virtualY += ContractBlockHeight;
				}

				if (!string.IsNullOrEmpty(statusFish))
				{
					if (IsEntryVisible(virtualY, ContractBlockHeight))
					{
						int screenY = ContentTop + virtualY - m_ScrollOffset;
						AddContractBlock(screenY, ContractBlockHeight, ResolveText(pm, "Fishing Delivery"), statusFish, BtnAbandonFishing);
					}

					virtualY += ContractBlockHeight;
				}

				if (!string.IsNullOrEmpty(statusAss))
				{
					if (IsEntryVisible(virtualY, ContractBlockHeight))
					{
						int screenY = ContentTop + virtualY - m_ScrollOffset;
						AddContractBlock(screenY, ContractBlockHeight, ResolveText(pm, "Assassination Contract"), statusAss, BtnAbandonAssassin);
					}
				}
			}

			if (useScrollViewport)
			{
				if (m_ScrollOffset > 0)
					AddButton(458, ContentTop + 4, 0x15E0, 0x15E4, BtnScrollUp, GumpButtonType.Reply, 0);

				if (m_ScrollOffset < maxScroll)
					AddButton(458, ContentTop + m_ViewportHeight - 24, 0x15E2, 0x15E6, BtnScrollDown, GumpButtonType.Reply, 0);
			}
		}

		private bool IsEntryVisible(int virtualY, int height)
		{
			return virtualY + height > m_ScrollOffset && virtualY < m_ScrollOffset + m_ViewportHeight;
		}

		private void AddMLQuestRow(MLQuestInstance questInstance, int index, int rowY)
		{
			int numberColor, stringColor;

			if (questInstance.Failed)
			{
				numberColor = 0x3C00;
				stringColor = 0x7B0000;
			}
			else
			{
				numberColor = BaseQuestGump.COLOR_LOCALIZED;
				stringColor = BaseQuestGump.COLOR_HTML;
			}

			TextDefinition.AddHtmlText(this, 98, rowY, 268, MLRowHeight - 4, ResolveQuestTextDefinition(questInstance.Quest.Title), false, false, numberColor, stringColor);
			AddButton(368, rowY + 2, 0x26B0, 0x26B1, 6 + 1 + index * 1000, GumpButtonType.Reply, 1); // Arrow
			if (!questInstance.IsCompleted()
				&& !questInstance.Failed
				&& questInstance.Objectives.Any(x => x is CollectObjectiveInstance || x is CraftObjectiveInstance))
			{
				AddButton(368 + 21 + 5, rowY + 2, 13006, 13006, 6 + 2 + index * 1000, GumpButtonType.Reply, 1); // Crosshair
			}
		}

		private void AddContractBlock(int y, int blockH, string title, string status, int abandonButtonId)
		{
			int innerH = blockH - 2;
			if (innerH < 40)
				innerH = 40;

			AddImageTiled(ContractPanelX, y, ContractPanelWidth, innerH, 0xA8E);

			AddHtml(98, y + 4, 318, 20,
				"<BODY>" + TextDefinition.GetColorizedText(EscapeForHtml(title), HtmlColors.LIGHT_GOLD) + "</BODY>",
				false, false);

			int statusY = y + 24;
			int statusH = innerH - 36;
			if (statusH < 22)
				statusH = 22;

			bool statusScrollable = statusH < MinStatusHeight;
			AddHtml(98, statusY, 300, statusH,
				"<BODY>" + TextDefinition.GetColorizedText(EscapeForHtml(status), HtmlColors.GRAY) + "</BODY>",
				false, statusScrollable);

			int btnY = y + innerH - 22;
			if (btnY < y + 28)
				btnY = y + 28;

			AddButton(ContractActionButtonX, btnY, (int)ButtonGraphic.Refuse, (int)ButtonGraphic.Refuse + 2, abandonButtonId, GumpButtonType.Reply, 0);
		}

		private static string ResolveText(Mobile from, string english)
		{
			string lang = AccountLang.GetLanguageCode(from != null ? from.Account : null);
			string resolved = StringCatalog.TryResolve(lang, english);
			return !string.IsNullOrEmpty(resolved) ? resolved : english;
		}

		private static string EscapeForHtml(string s)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			StringBuilder sb = new StringBuilder(s.Length + 8);
			for (int i = 0; i < s.Length; ++i)
			{
				char c = s[i];
				switch (c)
				{
					case '&': sb.Append("&amp;"); break;
					case '<': sb.Append("&lt;"); break;
					case '>': sb.Append("&gt;"); break;
					default: sb.Append(c); break;
				}
			}
			return sb.ToString();
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			switch (info.ButtonID)
			{
				case BtnScrollUp:
					sender.Mobile.SendGump(new QuestLogGump(m_Owner, m_CloseGumps, m_ScrollOffset - ScrollStep));
					return;
				case BtnScrollDown:
					sender.Mobile.SendGump(new QuestLogGump(m_Owner, m_CloseGumps, m_ScrollOffset + ScrollStep));
					return;
				case BtnAbandonStandard:
					if (PlayerSettings.GetQuestState(m_Owner, "StandardQuest"))
						sender.Mobile.SendGump(new ContractQuestAbandonConfirmGump(m_Owner, "StandardQuest", m_CloseGumps));
					return;
				case BtnAbandonFishing:
					if (PlayerSettings.GetQuestState(m_Owner, "FishingQuest"))
						sender.Mobile.SendGump(new ContractQuestAbandonConfirmGump(m_Owner, "FishingQuest", m_CloseGumps));
					return;
				case BtnAbandonAssassin:
					if (PlayerSettings.GetQuestState(m_Owner, "AssassinQuest"))
						sender.Mobile.SendGump(new ContractQuestAbandonConfirmGump(m_Owner, "AssassinQuest", m_CloseGumps));
					return;
			}

			if (info.ButtonID < 6)
				return;

			MLQuestContext context = MLQuestSystem.GetContext(m_Owner);
			if (context == null)
				return;

			List<MLQuestInstance> instances = context.QuestInstances;
			int buttonId = info.ButtonID - 6;
			int index = buttonId / 1000;
			if (index >= instances.Count)
				return;

			var questInstance = instances[index];
			var actionId = buttonId - index * 1000;
			switch (actionId)
			{
				case 1: // Get Info
					sender.Mobile.SendGump(new QuestLogDetailedGump(questInstance, m_CloseGumps));
					break;

				case 2: // Toggle Quest Items
					var player = (PlayerMobile)sender.Mobile;
					player.ToggleQuestItem();
					sender.Mobile.SendGump(new QuestLogGump(player, m_CloseGumps, m_ScrollOffset));
					break;

				default:
					break;
			}
		}
	}
}
