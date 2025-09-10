using Server.Gumps;
using Server.Network;
using System;

namespace Server.Engines.PuzzleChest
{
	public enum PuzzleGumpPageType
	{
		Default,
		Help
	}

	public class PuzzleGump : Gump
	{
		private enum ActionButtonType
		{
			Submit = 1,
			ResetGuess = 2,

			Cylinder_None,
			Cylinder_LightBlue,
			Cylinder_Blue,
			Cylinder_Green,
			Cylinder_Orange,
			Cylinder_Purple,
			Cylinder_Red,
			Cylinder_DarkBlue,
			Cylinder_Yellow,

			PedestalBase = 50,
		}

		private const int CYLINDER_PADDING = 15;
		private const int CYLINDER_WIDTH = 32;
		private const int GUMP_HEIGHT = 485;
		private const int GUMP_WIDTH = 425;
		private const int HALF_GUMP_WIDTH = GUMP_WIDTH / 2;
		private const int HALF_SECTION_INDENT = SECTION_INDENT / 2;
		private const int LABEL_PADDING_BOTTOM = 20;
		private const int PEDESTAL_PADDING = 33;
		private const int PEDESTAL_WIDTH = 32;
		private const int SECTION_INDENT = 20;

		private readonly PuzzleChest m_Chest;
		private readonly Mobile m_From;
		private readonly bool[] m_SelectedPedestalState;
		private readonly PuzzleChestSolution m_Solution;

		public PuzzleGump(Mobile from, PuzzleChest chest, PuzzleChestSolution solution, bool[] selectedPedestalState) : base(50, 50)
		{
			m_From = from;
			m_Chest = chest;
			m_Solution = solution;
			m_SelectedPedestalState = selectedPedestalState;

			// Add the image multiple times to decrease the transparency
			AddImage(0, 0, 9580, Server.Misc.PlayerSettings.GetGumpHue(from));
			AddImage(0, 0, 9580, Server.Misc.PlayerSettings.GetGumpHue(from));
			AddImage(0, 0, 9580, Server.Misc.PlayerSettings.GetGumpHue(from));
			AddAlphaRegion(0, 0, GUMP_WIDTH, GUMP_HEIGHT);

			const int SECTION_LABEL_WIDTH = GUMP_WIDTH - HALF_SECTION_INDENT;
			var y = HALF_SECTION_INDENT;

			TextDefinition.AddHtmlText(this, HALF_SECTION_INDENT, y, SECTION_LABEL_WIDTH, 20, "A Puzzle Lock", HtmlColors.WHITE);
			y += LABEL_PADDING_BOTTOM;

			var lastGuess = chest.GetLastGuess(from);
			if (lastGuess == null)
			{
				TextDefinition.AddHtmlText(this, SECTION_INDENT, y, GUMP_WIDTH - 2 * HALF_SECTION_INDENT, 60, "Correctly choose the sequence of cylinders needed to open the latch. Each cylinder may potentially be used more than once. Beware! A false attempt could be deadly!", HtmlColors.WHITE);
				y += 56;
			}

			var lockpicking = (int)from.Skills.Lockpicking.Base;
			if (60 <= lockpicking)
			{
				y += 10;
				TextDefinition.AddHtmlText(this, HALF_SECTION_INDENT, y, 150, 20, "Lockpicking hints", HtmlColors.WHITE);
				y += LABEL_PADDING_BOTTOM;

				AddLockpickingHints(SECTION_INDENT, y, lockpicking, chest);
				y += 63;
			}

			if (lastGuess != null)
			{
				y += 10;
				TextDefinition.AddHtmlText(this, HALF_SECTION_INDENT, y, SECTION_LABEL_WIDTH, 20, "Thy previous guess", HtmlColors.WHITE);
				y += LABEL_PADDING_BOTTOM;

				AddLastGuess(SECTION_INDENT, y, lastGuess);
				y += 62;
			}

			y += 10;
			TextDefinition.AddHtmlText(this, HALF_SECTION_INDENT, y, SECTION_LABEL_WIDTH, 20, "Select the slots to change", HtmlColors.WHITE);
			y += LABEL_PADDING_BOTTOM + 3;
			AddPedestals(55, y, solution, m_SelectedPedestalState);
			y += 97;

			y += 10;
			TextDefinition.AddHtmlText(this, HALF_SECTION_INDENT, y, SECTION_LABEL_WIDTH, 20, "Select a color to use", HtmlColors.WHITE);
			y += LABEL_PADDING_BOTTOM + 4;
			AddCylinderOptions(37, y);

			var RIGHT_BUTTON_X = GUMP_WIDTH - 85;
			y = GUMP_HEIGHT - 33;

			if (lastGuess != null)
			{
				// Reset button
				AddButton(HALF_SECTION_INDENT, y, 0xFB1, 0xFB1, (int)ActionButtonType.ResetGuess, GumpButtonType.Reply, 0); // X
				TextDefinition.AddHtmlText(this, HALF_SECTION_INDENT + 33, y + 3, 100, 20, "Reset", HtmlColors.WHITE);
			}

			// Submit button
			AddButton(RIGHT_BUTTON_X, y, 0xFA5, 0xFA7, (int)ActionButtonType.Submit, GumpButtonType.Reply, 0); // Right arrow
			TextDefinition.AddHtmlText(this, RIGHT_BUTTON_X + 33, y + 3, 53, 20, "Submit", HtmlColors.WHITE);
		}

		public static bool[] GetDefaultSelectedPedestalState()
		{
			return new bool[PuzzleChestSolution.Length];
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 0 || m_Chest.IsSolved || !m_From.CheckAlive() || m_From.Blessed) return;

			if (!m_Chest.IsInRange(m_From))
			{
				m_From.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500446); // That is too far away.
				return;
			}

			switch ((ActionButtonType)info.ButtonID)
			{
				case ActionButtonType.Submit:
					{
						// In case they just submitted this solution
						int cylinders, colors;
						if (!m_Solution.Matches(m_Chest.GetLastGuess(m_From), out cylinders, out colors))
						{
							m_Chest.SubmitSolution(m_From, m_Solution);
							if (m_Chest.IsSolved || !m_From.Alive) return;
						}

						m_From.SendGump(new PuzzleGump(m_From, m_Chest, m_Solution, GetDefaultSelectedPedestalState()));
						break;
					}

				case ActionButtonType.ResetGuess:
					{
						var lastGuess = m_Chest.GetLastGuess(m_From);
						if (lastGuess == null) return;

						for (var i = 0; i < lastGuess.Cylinders.Length; i++)
						{
							m_Solution.Cylinders[i] = lastGuess.Cylinders[i];
						}

						m_From.SendGump(new PuzzleGump(m_From, m_Chest, m_Solution, GetDefaultSelectedPedestalState()));
						break;
					}

				case ActionButtonType.Cylinder_None:
				case ActionButtonType.Cylinder_LightBlue:
				case ActionButtonType.Cylinder_Blue:
				case ActionButtonType.Cylinder_Green:
				case ActionButtonType.Cylinder_Orange:
				case ActionButtonType.Cylinder_Purple:
				case ActionButtonType.Cylinder_Red:
				case ActionButtonType.Cylinder_DarkBlue:
				case ActionButtonType.Cylinder_Yellow:
					{
						PuzzleChestCylinder cylinder;
						switch ((ActionButtonType)info.ButtonID)
						{
							case ActionButtonType.Cylinder_LightBlue: cylinder = PuzzleChestCylinder.LightBlue; break;
							case ActionButtonType.Cylinder_Blue: cylinder = PuzzleChestCylinder.Blue; break;
							case ActionButtonType.Cylinder_Green: cylinder = PuzzleChestCylinder.Green; break;
							case ActionButtonType.Cylinder_Orange: cylinder = PuzzleChestCylinder.Orange; break;
							case ActionButtonType.Cylinder_Purple: cylinder = PuzzleChestCylinder.Purple; break;
							case ActionButtonType.Cylinder_Red: cylinder = PuzzleChestCylinder.Red; break;
							case ActionButtonType.Cylinder_DarkBlue: cylinder = PuzzleChestCylinder.DarkBlue; break;
							case ActionButtonType.Cylinder_Yellow: cylinder = PuzzleChestCylinder.Yellow; break;
							default: return;
						}

						for (var i = 0; i < m_SelectedPedestalState.Length; i++)
						{
							var selected = m_SelectedPedestalState[i];
							if (!selected) continue;

							m_Solution.Cylinders[i] = cylinder;
						}

						m_From.SendGump(new PuzzleGump(m_From, m_Chest, m_Solution, GetDefaultSelectedPedestalState()));
					}
					break;

				default:
					{
						if ((int)ActionButtonType.PedestalBase <= info.ButtonID)
						{
							var index = info.ButtonID - (int)ActionButtonType.PedestalBase;
							m_SelectedPedestalState[index] = !m_SelectedPedestalState[index];

							m_From.SendGump(new PuzzleGump(m_From, m_Chest, m_Solution, m_SelectedPedestalState));
						}
						break;
					}
			}
		}

		private void AddCylinder(int x, int y, PuzzleChestCylinder cylinder)
		{
			if (cylinder != PuzzleChestCylinder.None)
				AddItem(x, y, (int)cylinder);
			else
				AddItem(x + 9, y - 1, (int)cylinder);
		}

		private void AddCylinderOption(int x, int y, PuzzleChestCylinder cylinder)
		{
			AddImage(x, y, 10351, LabelColors.GRAY);

			if (cylinder != PuzzleChestCylinder.None)
			{
				AddCylinder(x - 7, y + 10, cylinder);
			}
		}

		private void AddCylinderOptions(int x, int y)
		{
			AddCylinderOptionWithButton(x, y, PuzzleChestCylinder.LightBlue);

			x += CYLINDER_WIDTH + CYLINDER_PADDING;
			AddCylinderOptionWithButton(x, y, PuzzleChestCylinder.Blue);

			x += CYLINDER_WIDTH + CYLINDER_PADDING;
			AddCylinderOptionWithButton(x, y, PuzzleChestCylinder.Green);

			x += CYLINDER_WIDTH + CYLINDER_PADDING;
			AddCylinderOptionWithButton(x, y, PuzzleChestCylinder.Orange);

			x += CYLINDER_WIDTH + CYLINDER_PADDING;
			AddCylinderOptionWithButton(x, y, PuzzleChestCylinder.Purple);

			x += CYLINDER_WIDTH + CYLINDER_PADDING;
			AddCylinderOptionWithButton(x, y, PuzzleChestCylinder.Red);

			x += CYLINDER_WIDTH + CYLINDER_PADDING;
			AddCylinderOptionWithButton(x, y, PuzzleChestCylinder.DarkBlue);

			x += CYLINDER_WIDTH + CYLINDER_PADDING;
			AddCylinderOptionWithButton(x, y, PuzzleChestCylinder.Yellow);
		}

		private void AddCylinderOptionWithButton(int x, int y, PuzzleChestCylinder cylinder)
		{
			int buttonId;
			switch (cylinder)
			{
				case PuzzleChestCylinder.LightBlue: buttonId = (int)ActionButtonType.Cylinder_LightBlue; break;
				case PuzzleChestCylinder.Blue: buttonId = (int)ActionButtonType.Cylinder_Blue; break;
				case PuzzleChestCylinder.Green: buttonId = (int)ActionButtonType.Cylinder_Green; break;
				case PuzzleChestCylinder.Orange: buttonId = (int)ActionButtonType.Cylinder_Orange; break;
				case PuzzleChestCylinder.Purple: buttonId = (int)ActionButtonType.Cylinder_Purple; break;
				case PuzzleChestCylinder.Red: buttonId = (int)ActionButtonType.Cylinder_Red; break;
				case PuzzleChestCylinder.DarkBlue: buttonId = (int)ActionButtonType.Cylinder_DarkBlue; break;
				case PuzzleChestCylinder.Yellow: buttonId = (int)ActionButtonType.Cylinder_Yellow; break;

				default:
					Console.WriteLine("Invalid cylinder: {0}", cylinder);
					return;
			}

			AddButton(x, y, 10351, 10351, buttonId, GumpButtonType.Reply, 0);
			AddCylinder(x - 7, y + 10, cylinder);
		}

		private void AddLastGuess(int x, int y, PuzzleChestSolutionAndTime lastGuess)
		{
			int correctCylinders, correctColors;
			if (m_Chest.Solution.Matches(lastGuess, out correctCylinders, out correctColors)) return;

			var initialX = x;

			const int SECTION_LABEL_WIDTH = HALF_GUMP_WIDTH - SECTION_INDENT;

			TextDefinition.AddHtmlText(this, x, y, SECTION_LABEL_WIDTH, 20, string.Format("Correctly placed colors: {0}", TextDefinition.GetColorizedText(correctCylinders.ToString(), HtmlColors.MINT_GREEN)), HtmlColors.WHITE);

			x = HALF_GUMP_WIDTH; // Set
			TextDefinition.AddHtmlText(this, x, y, SECTION_LABEL_WIDTH, 20, string.Format("Used colors in wrong slots: {0}", TextDefinition.GetColorizedText(correctColors.ToString(), HtmlColors.RED)), HtmlColors.WHITE);
			y += LABEL_PADDING_BOTTOM + 12;

			const int OFFSET = 65;

			x = initialX + 30;
			AddBackground(x, y - 8, 314, 25, 2620);

			x += 6;
			AddCylinder(x, y, lastGuess.First);

			x += OFFSET;
			AddCylinder(x, y, lastGuess.Second);

			x += OFFSET;
			AddCylinder(x, y, lastGuess.Third);

			x += OFFSET;
			AddCylinder(x, y, lastGuess.Fourth);

			x += OFFSET;
			AddCylinder(x, y, lastGuess.Fifth);
		}

		private void AddLockpickingHints(int x, int y, int lockpicking, PuzzleChest chest)
		{
			const int SECTION_LABEL_WIDTH = HALF_GUMP_WIDTH - SECTION_INDENT;
			const int SECTION_LABEL_PADDING_BOTTOM = LABEL_PADDING_BOTTOM + 4;

			if (lockpicking >= 80.0)
			{
				TextDefinition.AddHtmlText(this, x, y, SECTION_LABEL_WIDTH, 20, "In the first slot", HtmlColors.WHITE);
				AddCylinderOption(x + SECTION_INDENT, y + SECTION_LABEL_PADDING_BOTTOM, chest.Solution.First);

				x = HALF_GUMP_WIDTH; // Set
				TextDefinition.AddHtmlText(this, x, y, SECTION_LABEL_WIDTH, 20, "Used in unknown slot", HtmlColors.WHITE);

				x += SECTION_INDENT;
				AddCylinderOption(x, y + SECTION_LABEL_PADDING_BOTTOM, chest.FirstHint);

				y += SECTION_LABEL_PADDING_BOTTOM;

				if (lockpicking >= 90.0)
				{
					x += CYLINDER_WIDTH + CYLINDER_PADDING;
					AddCylinderOption(x, y, chest.SecondHint);
				}

				if (lockpicking >= 100.0)
				{
					x += CYLINDER_WIDTH + CYLINDER_PADDING;
					AddCylinderOption(x, y, chest.ThirdHint);
				}
			}
			else
			{
				TextDefinition.AddHtmlText(this, x, y, 425, 20, "Used in unknown slot", HtmlColors.WHITE);
				x += SECTION_INDENT;
				y += SECTION_LABEL_PADDING_BOTTOM;
				AddCylinderOption(x, y, chest.FirstHint);

				if (lockpicking >= 70.0)
				{
					x += CYLINDER_WIDTH + CYLINDER_PADDING;
					y += SECTION_LABEL_PADDING_BOTTOM;
					AddCylinderOption(x, y, chest.SecondHint);
				}
			}
		}

		private void AddPedestalOption(int x, int y, PuzzleChestCylinder cylinder, int switchId, bool isChecked)
		{
			AddItem(x, y, 0xB10);
			AddItem(x - 23, y + 12, 0xB12);
			AddItem(x + 23, y + 12, 0xB13);
			AddItem(x, y + 23, 0xB11);

			if (cylinder != PuzzleChestCylinder.None)
			{
				AddItem(x, y + 2, 0x51A);
				AddCylinder(x - 1, y + 19, cylinder);
			}
			else
			{
				AddItem(x, y + 2, 0x521);
			}

			// AddRadio(x + 7, y + 65, 0x867, 0x86A, initialState, switchID);
			var icon = isChecked ? 0x86A : 0x867;
			AddButton(x + 7, y + 65, icon, icon, (int)ActionButtonType.PedestalBase + switchId, GumpButtonType.Reply, 0);
		}

		private void AddPedestals(int x, int y, PuzzleChestSolution solution, bool[] selectedPedestals)
		{
			AddPedestalOption(x, y, solution.First, 0, selectedPedestals[0]);

			x += PEDESTAL_WIDTH + PEDESTAL_PADDING;
			AddPedestalOption(x, y, solution.Second, 1, selectedPedestals[1]);

			x += PEDESTAL_WIDTH + PEDESTAL_PADDING;
			AddPedestalOption(x, y, solution.Third, 2, selectedPedestals[2]);

			x += PEDESTAL_WIDTH + PEDESTAL_PADDING;
			AddPedestalOption(x, y, solution.Fourth, 3, selectedPedestals[3]);

			x += PEDESTAL_WIDTH + PEDESTAL_PADDING;
			AddPedestalOption(x, y, solution.Fifth, 4, selectedPedestals[4]);
		}
	}
}