using System;
using Server.Mobiles;

namespace Server.Engines.RpgDialogue
{
	public static class RpgDialogueScripts
	{
		public const string Demo = "rpg.dialogue.demo";
	}

	public sealed class RpgDialogueNode
	{
		public readonly string BodyEnglish;
		public readonly RpgDialogueOption[] Options;
		public readonly Action<PlayerMobile, Mobile> OnShow;

		public RpgDialogueNode(string bodyEnglish, RpgDialogueOption[] options, Action<PlayerMobile, Mobile> onShow = null)
		{
			BodyEnglish = bodyEnglish;
			Options = options ?? new RpgDialogueOption[0];
			OnShow = onShow;
		}
	}

	public sealed class RpgDialogueOption
	{
		public readonly string LabelEnglish;
		public readonly string NextNodeId;

		public RpgDialogueOption(string labelEnglish, string nextNodeId)
		{
			LabelEnglish = labelEnglish;
			NextNodeId = nextNodeId;
		}
	}
}
