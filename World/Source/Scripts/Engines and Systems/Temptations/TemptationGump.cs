using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Temptation
{
	public class TemptationGump : Gump
	{
		private enum ActionButtonType
		{
			Close = 0,
			I_can_take_it,
			Strongest_Avenger,
			Famine,
			Puzzle_master,
			This_is_just_a_tribute,
			Deathwish,
		}

		private const int BORDER_WIDTH = 0;
		private const int GUMP_HEIGHT = 458;
		private const int GUMP_WIDTH = 708;
		private const int HALF_SECTION_INDENT = SECTION_INDENT / 2;
		private const int LABEL_PADDING_BOTTOM = 20;
		private const int SECTION_INDENT = 20;

		private readonly PlayerMobile m_Requester;
		private readonly PlayerMobile m_Target;

		public TemptationGump(PlayerMobile from, PlayerContext context, PlayerMobile requester) : base(25, 25)
		{
			from.CloseGump(typeof(TemptationGump));
			requester.CloseGump(typeof(TemptationGump));

			m_Target = from;
			m_Requester = requester;

			// Add the image multiple times to decrease the transparency
			AddImage(BORDER_WIDTH, BORDER_WIDTH, 7055, 2999);
			AddImage(BORDER_WIDTH, BORDER_WIDTH, 7055, 2999);
			AddAlphaRegion(BORDER_WIDTH, BORDER_WIDTH, GUMP_WIDTH, GUMP_HEIGHT);
			AddImage(BORDER_WIDTH, BORDER_WIDTH, 7055, Server.Misc.PlayerSettings.GetGumpHue(from));
			AddImage(BORDER_WIDTH, BORDER_WIDTH, 7055, Server.Misc.PlayerSettings.GetGumpHue(from));
			AddAlphaRegion(BORDER_WIDTH, BORDER_WIDTH, GUMP_WIDTH, GUMP_HEIGHT);

			const int SECTION_LABEL_WIDTH = GUMP_WIDTH - HALF_SECTION_INDENT;
			var y = HALF_SECTION_INDENT;

			var canEdit = m_Requester.AccessLevel >= AccessLevel.GameMaster
				|| (m_Target == m_Requester && m_Target.Region != null && m_Target.Region.Name == "the Forest");
			var title = m_Target == m_Requester && canEdit ? "Choose your Temptations" : string.Format("{0}'s Temptations", m_Target.Name);

			int x = SECTION_INDENT;
			if (!canEdit)
			{
				const int LOCK_ICON = 2092;
				AddImage(HALF_SECTION_INDENT, y + 4, LOCK_ICON);
				AddTooltip("Setting can only be changed in the Gypsy Forest");

				x += 5;
				TextDefinition.AddHtmlText(this, x, y, SECTION_LABEL_WIDTH, 20, title, HtmlColors.RED);
			}
			else
			{
				TextDefinition.AddHtmlText(this, x, y, SECTION_LABEL_WIDTH, 20, title, HtmlColors.RED);
			}

			x += HALF_SECTION_INDENT;
			y += 30;

			if (!canEdit && context.Flags == TemptationFlags.None)
				TextDefinition.AddHtmlText(this, x, y, SECTION_LABEL_WIDTH, 20, string.Format("{0} was not tempted.", m_Target.Name), HtmlColors.RED);
			else
			{
				AddOptions(x, ref y, canEdit, context);
			}
		}

		public static void Open(PlayerMobile target, PlayerMobile requester, PlayerContext context)
		{
			requester.SendGump(new TemptationGump(target, context, requester));
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 0) return;

			// Make sure we create a context if necessary
			var context = TemptationEngine.Instance.GetOrCreateContext(m_Target);

			switch ((ActionButtonType)info.ButtonID)
			{
				case ActionButtonType.I_can_take_it:
					context.IsBerserk = !context.IsBerserk;
					break;

				case ActionButtonType.Deathwish:
					context.HasPermanentDeath = !context.HasPermanentDeath;
					break;

				case ActionButtonType.Strongest_Avenger:
					context.IncreaseMobDifficulty = !context.IncreaseMobDifficulty;
					break;

				case ActionButtonType.Puzzle_master:
					context.CanUsePuzzleboxes = !context.CanUsePuzzleboxes;
					break;

				case ActionButtonType.Famine:
				case ActionButtonType.This_is_just_a_tribute:
					Console.WriteLine("[Temptation] Selected a flag that is not yet implemented: {0}", (ActionButtonType)info.ButtonID);
					return;

				case ActionButtonType.Close:
				default:
					return;
			}

			TemptationEngine.Instance.ApplyContext(m_Target, context);

			Open(m_Target, m_Requester, context);
		}

		private void AddOption(int x, ref int y, ActionButtonType actionButtonType, bool canEdit, PlayerContext context)
		{
			const int X_BUTTON = 0xFB1;
			const int BLANK_BUTTON = 0xE19;
			var isSelected = GetIsSelected(actionButtonType, context);
			if (!canEdit && !isSelected) return;

			var button = isSelected ? X_BUTTON : BLANK_BUTTON;
			if (canEdit)
			{
				AddButton(x, y, button, button, (int)actionButtonType, GumpButtonType.Reply, 0);
				x += 35;
			}

			var width = GUMP_WIDTH - x;
			TextDefinition.AddHtmlText(this, x, y + 2, width, 20, GetName(actionButtonType), HtmlColors.RED);
			y += LABEL_PADDING_BOTTOM;

			var description = GetDescription(actionButtonType);
			var height = description.Split('<').Length * 20;
			TextDefinition.AddHtmlText(this, x, y + 2, width, height, description, HtmlColors.PALE_RED);
			y += height + 10;
		}

		private void AddOptions(int x, ref int y, bool canEdit, PlayerContext context)
		{
			AddOption(x, ref y, ActionButtonType.Puzzle_master, canEdit, context);
			// AddOption(x, ref y, ActionButtonType.Famine, canEdit, context); // Incomplete
			AddOption(x, ref y, ActionButtonType.I_can_take_it, canEdit, context);
			AddOption(x, ref y, ActionButtonType.Strongest_Avenger, canEdit, context);
			// AddOption(x, ref y, ActionButtonType.This_is_just_a_tribute, canEdit, context); // Incomplete
			AddOption(x, ref y, ActionButtonType.Deathwish, canEdit, context);
		}

		private string GetDescription(ActionButtonType actionButtonType)
		{
			switch (actionButtonType)
			{
				case ActionButtonType.I_can_take_it:
					return "+ You do 10% increased damage"
					+ "<br>x You take 20% increased damage";

				case ActionButtonType.Strongest_Avenger:
					return "+ You learn how to wear pants"
					+ "<br>x Enemies cast spells faster";

				case ActionButtonType.Famine:
					return "+ Your stat and skill gain rate is impacted by your Hunger"
					+ "<br>x Hunger and thirst decay twice as fast";

				case ActionButtonType.Puzzle_master:
					return "+ You learn how to solve puzzle boxes"
					+ "<br>x Reduced skill cap bonuses for Fugitives (+200) and Titan of Ether quest (+200)"
					+ "<br>x Magical attributes from monster racial bonuses now act as Minimums. Resist/Stat bonuses still stack.";

				case ActionButtonType.This_is_just_a_tribute: return "- Tribute quests rewards are changed";
				case ActionButtonType.Deathwish:
					return "+ You gain stats and skills faster"
					+ "<br>- An old sword"
					+ "<br>x You may never be resurrected";

				default: return string.Empty;
			}
		}

		private bool GetIsSelected(ActionButtonType actionButtonType, PlayerContext context)
		{
			switch (actionButtonType)
			{
				case ActionButtonType.I_can_take_it: return context.Flags.HasFlag(TemptationFlags.I_can_take_it);
				case ActionButtonType.Strongest_Avenger: return context.Flags.HasFlag(TemptationFlags.Strongest_Avenger);
				case ActionButtonType.Famine: return context.Flags.HasFlag(TemptationFlags.Famine);
				case ActionButtonType.Puzzle_master: return context.Flags.HasFlag(TemptationFlags.Puzzle_master);
				case ActionButtonType.This_is_just_a_tribute: return context.Flags.HasFlag(TemptationFlags.This_is_just_a_tribute);
				case ActionButtonType.Deathwish: return context.Flags.HasFlag(TemptationFlags.Deathwish);
				default: return false;
			}
		}

		private string GetName(ActionButtonType actionButtonType)
		{
			switch (actionButtonType)
			{
				case ActionButtonType.I_can_take_it: return "I can take it";
				case ActionButtonType.Strongest_Avenger: return "Strongest Avenger";
				case ActionButtonType.Famine: return "Red warrior needs food, badly!";
				case ActionButtonType.Puzzle_master: return "Puzzle master";
				case ActionButtonType.This_is_just_a_tribute: return "This is just a tribute";
				case ActionButtonType.Deathwish: return "It's dangerous to go alone! Take this.";
				default: return string.Empty;
			}
		}
	}
}