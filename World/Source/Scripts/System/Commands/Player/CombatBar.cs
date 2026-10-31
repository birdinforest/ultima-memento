using Server.Mobiles;
using Server.Commands;
using Server.Items;

namespace Server.Gumps
{
	public class CombatBar
	{
		const int LABEL_AMOUNT_OFFSET_X = 70;

		public static void Initialize()
		{
			CommandSystem.Register("combatbar", AccessLevel.Player, new CommandEventHandler(ToolBar_OnCommand));
		}

		public static void Register(string command, AccessLevel access, CommandEventHandler handler)
		{
			CommandSystem.Register(command, access, handler);
		}

		[Usage("combatbar")]
		[Description("Opens the Combat Bar.")]
		public static void ToolBar_OnCommand(CommandEventArgs e)
		{
			Refresh(e.Mobile as PlayerMobile, true);
		}

		public static void Refresh(Mobile from, bool force = false)
		{
			if (from is PlayerMobile)
			{
				if (from.HasGump(typeof(CombatBarGump)) || force)
				{
					from.CloseGump(typeof(CombatBarGump));
					from.SendGump(new CombatBarGump((PlayerMobile)from));
				}
			}
		}

		public class CombatBarGump : Gump
		{
			public CombatBarGump(PlayerMobile player) : base(25, 25)
			{
				this.Closable = true;
				this.Disposable = true;
				this.Dragable = true;
				this.Resizable = false;

				const int GAP_SMALL = 10;
				const int GAP_MEDIUM = 20;
				const int GAP_LARGE = 25;

				AddPage(0);

				AddImage(0, 0, 30008); // Drag handle, Blue gem

				int x = 20;
				int y = 15;

				Add(x, y, "Hunger", player.Hunger, 5);
				y += GAP_MEDIUM;

				Add(x, y, "Thirst", player.Thirst, 5);
				y += GAP_MEDIUM;

				y += GAP_SMALL; // Spacer

				Add(x, y, "Karma", player.Karma);
				y += GAP_MEDIUM;

				Add(x, y, "Fame", player.Fame);
				y += GAP_MEDIUM;

				y += GAP_SMALL; // Spacer

				if (player.TithingPoints > 0)
				{
					AddItem(x + 20, y, 0x2252); // Chivalry Book
					Add(x + LABEL_AMOUNT_OFFSET_X, y, player.TithingPoints, 300);
					y += GAP_LARGE;
				}
				if (player.Backpack != null && (5 <= player.Skills[SkillName.Healing].Value || 5 <= player.Skills[SkillName.Veterinary].Value))
				{
					AddItem(x + 20, y, 0xE21); // Bandage
					Add(x + LABEL_AMOUNT_OFFSET_X, y, player.Backpack.GetAmount(typeof(Bandage), true), 50);
					y += GAP_LARGE;
				}

				if (player.Avatar.Active)
				{
					var coins = player.Avatar.PointsFarmed + player.Avatar.PointsSaved;
					AddHtmlLabel(x, y, "--- Avatar ---", width: 120);
					y += GAP_MEDIUM;

					Add(x, y, "Coins", coins);
					y += GAP_MEDIUM;

					if (player.Avatar.DraftModeEnabled)
					{
						if (player.Avatar.DraftLevel < Engines.Avatar.Constants.DRAFT_MAX_LEVEL)
						{
							Add(x, y, "Next Pick", player.Avatar.DraftExperienceToNextPick);
							y += GAP_MEDIUM;

							if (player.Avatar.DraftPicksSpent < player.Avatar.DraftPicksAvailable)
								AddHtmlLabel(x, y, "* Picks Available *", HtmlColors.COOL_BLUE, 120);
						}
					}
				}
			}

			private void Add(int x, int y, int amount, int warningAmount = -1)
			{
				AddHtml(x, y, 50, 40, TextDefinition.GetColorizedText(amount.ToString("n0"), warningAmount >= 0 && amount <= warningAmount ? HtmlColors.RED : HtmlColors.YELLOW), false, false);
			}

			private void Add(int x, int y, string label, int amount, int warningAmount = -1)
			{
				Add(x, y, label, amount, false, warningAmount);
			}

			private void Add(int x, int y, string label, int amount, bool multiLineLabel, int warningAmount = -1)
			{
				AddHtmlLabel(x, y, label);

				if (multiLineLabel) y += 20;
				AddHtml(x + LABEL_AMOUNT_OFFSET_X, y, 75, 20, TextDefinition.GetColorizedText(amount.ToString("n0"), warningAmount >= 0 && amount <= warningAmount ? HtmlColors.RED : HtmlColors.YELLOW), false, false);
			}

			private void AddHtmlLabel(int x, int y, string label, int color = HtmlColors.YELLOW, int width = 60)
			{
				AddHtml(x, y, width, 40, TextDefinition.GetColorizedText(label, color), false, false);
			}
		}
	}
}
