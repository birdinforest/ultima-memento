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
		private const int YListStart = 158;
		private const int MLRowHeight = 28;
		private const int ContractBlockHeight = 92;
		private const int MaxContentBottom = 362;
		private const int BtnAbandonStandard = 5001;
		private const int BtnAbandonFishing = 5002;
		private const int BtnAbandonAssassin = 5003;

		private PlayerMobile m_Owner;
		private bool m_CloseGumps;

		public QuestLogGump(PlayerMobile pm)
			: this(pm, true)
		{
		}

		public QuestLogGump(PlayerMobile pm, bool closeGumps)
			: base(1046026, pm) // Quest Log
		{
			m_Owner = pm;
			m_CloseGumps = closeGumps;

			if (closeGumps)
			{
				pm.CloseGump(typeof(QuestLogGump));
				pm.CloseGump(typeof(QuestLogDetailedGump));
				pm.CloseGump(typeof(ContractQuestAbandonConfirmGump));
			}

			RegisterButton(ButtonPosition.Right, ButtonGraphic.Okay, 3);

			SetPageCount(1);

			BuildPage();

			int numberColor, stringColor;

			MLQuestContext context = MLQuestSystem.GetContext(pm);
			List<MLQuestInstance> instances = (context != null) ? context.QuestInstances : null;
			int mlCount = (instances != null) ? instances.Count : 0;

			if (instances != null)
			{
				for (int i = 0; i < instances.Count; ++i)
				{
					var questInstance = instances[i];
					int rowY = YListStart + MLRowHeight * i;

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
					AddButton(368, rowY + 2, 0x26B0, 0x26B1, 6 + 1 + i * 1000, GumpButtonType.Reply, 1); // Arrow
					if (!questInstance.IsCompleted()
						&& !questInstance.Failed
						&& questInstance.Objectives.Any(x => x is CollectObjectiveInstance || x is CraftObjectiveInstance))
					{
						AddButton(368 + 21 + 5, rowY + 2, 13006, 13006, 6 + 2 + i * 1000, GumpButtonType.Reply, 1); // Crosshair
					}
				}
			}

			int yContract = YListStart + MLRowHeight * mlCount + 14;

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

			int blockH = ContractBlockHeight;
			if (nContracts > 0)
			{
				int room = MaxContentBottom - yContract;
				if (room < nContracts * blockH)
					blockH = Math.Max(58, room / nContracts);
			}

			int contractIndex = 0;

			if (!string.IsNullOrEmpty(statusStd))
			{
				int yb = yContract + contractIndex * blockH;
				AddContractBlock(yb, blockH, ResolveText(pm, "Seeking Brave Adventurers"), statusStd, BtnAbandonStandard);
				contractIndex++;
			}
			if (!string.IsNullOrEmpty(statusFish))
			{
				int yb = yContract + contractIndex * blockH;
				AddContractBlock(yb, blockH, ResolveText(pm, "Fishing Delivery"), statusFish, BtnAbandonFishing);
				contractIndex++;
			}
			if (!string.IsNullOrEmpty(statusAss))
			{
				int yb = yContract + contractIndex * blockH;
				AddContractBlock(yb, blockH, ResolveText(pm, "Assassination Contract"), statusAss, BtnAbandonAssassin);
				contractIndex++;
			}
		}

		private void AddContractBlock(int y, int blockH, string title, string status, int abandonButtonId)
		{
			int innerH = blockH - 2;
			if (innerH < 40)
				innerH = 40;

			AddImageTiled(90, y, 366, innerH, 0xA8E);

			AddHtml(98, y + 4, 318, 20,
				"<BODY>" + TextDefinition.GetColorizedText(EscapeForHtml(title), HtmlColors.LIGHT_GOLD) + "</BODY>",
				false, false);

			int statusY = y + 24;
			int statusH = innerH - 36;
			if (statusH < 22)
				statusH = 22;

			bool statusScrollable = statusH < 42;
			AddHtml(98, statusY, 300, statusH,
				"<BODY>" + TextDefinition.GetColorizedText(EscapeForHtml(status), HtmlColors.GRAY) + "</BODY>",
				false, statusScrollable);

			int btnY = y + innerH - 22;
			if (btnY < y + 28)
				btnY = y + 28;

			AddButton(404, btnY, (int)ButtonGraphic.Refuse, (int)ButtonGraphic.Refuse + 2, abandonButtonId, GumpButtonType.Reply, 0);
			AddHtml(350, btnY, 52, 22, "<BASEFONT COLOR=#E8A090>" + EscapeForHtml(ResolveRefuseText(m_Owner)) + "</BASEFONT>", false, false);
		}

		private static string ResolveText(Mobile from, string english)
		{
			string lang = AccountLang.GetLanguageCode(from != null ? from.Account : null);
			string resolved = StringCatalog.TryResolve(lang, english);
			return !string.IsNullOrEmpty(resolved) ? resolved : english;
		}

		private static string ResolveRefuseText(Mobile from)
		{
			return ResolveText(from, "Refuse");
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
					sender.Mobile.SendGump(new QuestLogGump(player, m_CloseGumps));
					break;

				default:
					break;
			}
		}
	}
}
