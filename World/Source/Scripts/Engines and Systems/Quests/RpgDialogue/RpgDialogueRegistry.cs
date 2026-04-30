using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.RpgDialogue
{
	public static class RpgDialogueRegistry
	{
		private static readonly Dictionary<string, Dictionary<string, RpgDialogueNode>> Scripts =
			new Dictionary<string, Dictionary<string, RpgDialogueNode>>(StringComparer.Ordinal);

		private static readonly HashSet<int> s_DemoGoldOnce = new HashSet<int>();

		static RpgDialogueRegistry()
		{
			RegisterDemo();
		}

		private static void RegisterDemo()
		{
			// Demo nodes use English literals only (StringCatalog + extractor).
			RpgDialogueNode start = new RpgDialogueNode(
				"Hail, traveler. I am Eldrin, a herald for this new dialogue pane.<BR><BR>What would you like to say?",
				new[]
				{
					new RpgDialogueOption("What is this window?", "ui_help"),
					new RpgDialogueOption("Tell me a short tale.", "story"),
					new RpgDialogueOption("Farewell.", null)
				});

			RpgDialogueNode uiHelp = new RpgDialogueNode(
				"This is an RPG-style conversation sheet: the portrait on the left is a placeholder, my words sit here, and your answers line up beneath.<BR><BR>It stays separate from the classic ML quest offer books.",
				new[] { new RpgDialogueOption("I understand.", "start") });

			RpgDialogueNode story = new RpgDialogueNode(
				"Once, scribes brushed candle wax through long nights. Now we brush pixels across Britannia.<BR><BR>Help me exercise the buttons and I can spare a few coins.",
				new[]
				{
					new RpgDialogueOption("I will test it. Pay me.", "pay"),
					new RpgDialogueOption("Perhaps later.", "start")
				});

			RpgDialogueNode pay = new RpgDialogueNode(
				"The imaginary sponsors thank you. Here is your stipend for playing along.",
				new[] { new RpgDialogueOption("Thanks!", "start") },
				(pm, npc) =>
				{
					if (pm == null || pm.Deleted)
						return;

					if (!s_DemoGoldOnce.Add(pm.Serial.Value))
						return;

					pm.SendMessage("You receive fifty gold for testing the dialogue quest.");
					pm.AddToBackpack(new Gold(50));
				});

			var nodes = new Dictionary<string, RpgDialogueNode>(StringComparer.Ordinal)
			{
				["start"] = start,
				["ui_help"] = uiHelp,
				["story"] = story,
				["pay"] = pay
			};

			Scripts[RpgDialogueScripts.Demo] = nodes;
		}

		public static bool TryGetNode(string scriptId, string nodeId, out RpgDialogueNode node)
		{
			node = null;

			if (scriptId == null || nodeId == null)
				return false;

			Dictionary<string, RpgDialogueNode> script;

			if (!Scripts.TryGetValue(scriptId, out script))
				return false;

			return script.TryGetValue(nodeId, out node);
		}
	}
}
