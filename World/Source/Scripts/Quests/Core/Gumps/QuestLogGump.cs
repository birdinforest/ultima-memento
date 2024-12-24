using System.Collections.Generic;
using System.Linq;
using Server.Engines.MLQuests.Objectives;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.MLQuests.Gumps
{
	public class QuestLogGump : BaseQuestGump
	{
		private PlayerMobile m_Owner;
		private bool m_CloseGumps;

		public QuestLogGump(PlayerMobile pm)
			: this(pm, true)
		{
		}

		public QuestLogGump(PlayerMobile pm, bool closeGumps)
			: base(1046026) // Quest Log
		{
			m_Owner = pm;
			m_CloseGumps = closeGumps;

			if (closeGumps)
			{
				pm.CloseGump(typeof(QuestLogGump));
				pm.CloseGump(typeof(QuestLogDetailedGump));
			}

			RegisterButton(ButtonPosition.Right, ButtonGraphic.Okay, 3);

			SetPageCount(1);

			BuildPage();

			int numberColor, stringColor;

			MLQuestContext context = MLQuestSystem.GetContext(pm);

			if (context != null)
			{
				List<MLQuestInstance> instances = context.QuestInstances;

				for (int i = 0; i < instances.Count; ++i)
				{
					var questInstance = instances[i];
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

					TextDefinition.AddHtmlText(this, 98, 140 + 21 * i, 270, 21, questInstance.Quest.Title, false, false, numberColor, stringColor);
					AddButton(368, 140 + 21 * i, 0x26B0, 0x26B1, 6 + 1 + i * 1000, GumpButtonType.Reply, 1); // Arrow
					if (!questInstance.IsCompleted()
						&& !questInstance.Failed
						&& questInstance.Objectives.Any(x => x is CollectObjectiveInstance || x is CraftObjectiveInstance))
					{
						AddButton(368 + 21 + 5, 140 + 21 * i, 13006, 13006, 6 + 2 + i * 1000, GumpButtonType.Reply, 1); // Crosshair
					}
				}
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID < 6) return;

			MLQuestContext context = MLQuestSystem.GetContext(m_Owner);
			if (context == null) return;

			List<MLQuestInstance> instances = context.QuestInstances;
			int buttonId = info.ButtonID - 6;
			int index = buttonId / 1000;
			if (index >= instances.Count) return;

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
					break;

				default:
					break;
			}
		}
	}
}
