using Server.Gumps;

namespace Server.Engines.GlobalShoppe
{
	public class ExplainShoppeGump : Gump
	{
		public ExplainShoppeGump(Mobile from) : base(50, 50)
		{
			from.SendSound(0x4A);
			AddPage(0);

			AddImage(0, 0, 9547, Server.Misc.PlayerSettings.GetGumpHue(from));

			TextDefinition.AddHtmlText(this, 11, 11, 200, 20, @"SETTING UP SHOPPE", HtmlColors.BROWN);
			TextDefinition.AddHtmlText(this, 13, 44, 582, 487,
				string.Format(
					"So you want to setup a Shoppe of your own. In order to do that, you would to find somewhere with a lot of travelers. These Shoppes usually demand you to part with {0} gold, but they can quickly pay for themselves if you are good at your craft. Shoppes require to be stocked with tools and resources, and the Shoppe will indicate what those are. Simply drop such things onto your Shoppe to amass an inventory. When you drop tools onto your Shoppe, the number of tool uses will add to the Shoppe's tool count. A Shoppe may only hold {1} tools and {2} resources. These will get used up as you perform tasks for others. After a set period of time, customers will make requests of you which you can fulfill or refuse. Each request will display the task, who it is for, the amount of tools needed, the amount of resources required, your chance to fulfill the request (based on the difficulty and your skill), and the amount of reputation your Shoppe will acquire if you are successful.<br><br>If you fail to perform a selected task, or refuse to do it, your Shoppe's reputation will drop by that same value you would have been rewarded with. Word of mouth travels fast in the land and you will have less prestigious work if your reputation is low. Any gold earned will stay within the Shoppe until you single click the Shoppe and Transfer the funds out of it. Your Shoppe can have no more than {3} gold at a time, and you will not be able to conduct any more business in it until you withdraw the funds so it can amass more. The reputation for the Shoppe cannot go below 0, and it cannot go higher than 10,000. Again, the higher the reputation, the more lucrative work you will be asked to do. If you are a member of the associated crafting guild, your reputation will have a bonus toward it based on your crafting skill.",
					ShoppeConstants.SHOPPE_FEE, ShoppeConstants.MAX_TOOLS, ShoppeConstants.MAX_RESOURCES, ShoppeConstants.MAX_GOLD
				), HtmlColors.BROWN
			);
			AddButton(568, 9, 4017, 4017, 0, GumpButtonType.Reply, 0);
		}
	}
}
