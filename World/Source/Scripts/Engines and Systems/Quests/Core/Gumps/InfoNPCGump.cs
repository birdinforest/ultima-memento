using Server.Mobiles;

namespace Server.Engines.MLQuests.Gumps
{
	public class InfoNPCGump : BaseQuestGump
	{
		public InfoNPCGump(TextDefinition title, TextDefinition message, PlayerMobile viewer = null)
			: base(1060668, viewer) // INFORMATION
		{
			RegisterButton(ButtonPosition.Left, ButtonGraphic.Close, 3);

			SetPageCount(1);

			BuildPage();
			TextDefinition.AddHtmlText(this, 160, 108, 250, 16, ResolveQuestTextDefinition(this, title), false, false, BaseQuestGump.COLOR_LOCALIZED, BaseQuestGump.COLOR_LOCALIZED);
			TextDefinition.AddHtmlText(this, 98, 156, 312, 180, ResolveQuestTextDefinition(this, message), false, true, BaseQuestGump.COLOR_LOCALIZED, BaseQuestGump.COLOR_LOCALIZED);
		}
	}
}
