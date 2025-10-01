using Server.Items;
using Server.Misc;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Engines.PuzzleChest
{
	public class PuzzleChest
	{
		public const int HintsCount = 3;
		public readonly TimeSpan CleanupTime = TimeSpan.FromHours(1.0);

		private readonly StealBase m_Source;
		private Dictionary<Mobile, PuzzleChestSolutionAndTime> m_Guesses = new Dictionary<Mobile, PuzzleChestSolutionAndTime>();
		private PuzzleChestCylinder[] m_Hints = new PuzzleChestCylinder[HintsCount];
		private PuzzleChestSolution m_Solution;

		public PuzzleChest(StealBase source)
		{
			m_Source = source;
			Solution = new PuzzleChestSolution();
		}

		public PuzzleChest(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			m_Source = reader.ReadItem() as StealBase;

			m_Solution = new PuzzleChestSolution(reader);

			var length = reader.ReadEncodedInt();
			for (int i = 0; i < length; i++)
			{
				var cylinder = (PuzzleChestCylinder)reader.ReadInt();

				if (length == m_Hints.Length)
					m_Hints[i] = cylinder;
			}
			if (length != m_Hints.Length)
				InitHints();

			var guesses = reader.ReadEncodedInt();
			for (int i = 0; i < guesses; i++)
			{
				var m = reader.ReadMobile();
				m_Guesses[m] = new PuzzleChestSolutionAndTime(reader);
			}
		}

		public PuzzleChestCylinder FirstHint
		{ get { return m_Hints[0]; } set { m_Hints[0] = value; } }

		public PuzzleChestCylinder[] Hints
		{ get { return m_Hints; } }

		public bool IsSolved { get; set; }

		public PuzzleChestCylinder SecondHint
		{ get { return m_Hints[1]; } set { m_Hints[1] = value; } }

		public PuzzleChestSolution Solution
		{
			get { return m_Solution; }
			set
			{
				m_Solution = value;
				InitHints();
			}
		}

		public PuzzleChestCylinder ThirdHint
		{ get { return m_Hints[2]; } set { m_Hints[2] = value; } }

		public void CleanupGuesses()
		{
			List<Mobile> toDelete = new List<Mobile>();

			foreach (KeyValuePair<Mobile, PuzzleChestSolutionAndTime> kvp in m_Guesses)
			{
				if (DateTime.UtcNow - kvp.Value.When > CleanupTime)
					toDelete.Add(kvp.Key);
			}

			foreach (Mobile m in toDelete)
				m_Guesses.Remove(m);
		}

		public PuzzleChestSolutionAndTime GetLastGuess(Mobile m)
		{
			PuzzleChestSolutionAndTime pcst;
			m_Guesses.TryGetValue(m, out pcst);
			return pcst;
		}

		public bool IsInRange(Mobile from)
		{
			if (m_Source == null || m_Source.Deleted) return false;
			if (from.AccessLevel == AccessLevel.Player && (from.Map != m_Source.Map || !from.InRange(m_Source.GetWorldLocation(), 2))) return false;

			return true;
		}

		public void Serialize(GenericWriter writer)
		{
			CleanupGuesses();

			writer.WriteEncodedInt(0); // version

			m_Solution.Serialize(writer);

			writer.WriteEncodedInt(m_Hints.Length);
			for (var i = 0; i < m_Hints.Length; i++)
			{
				writer.Write((int)m_Hints[i]);
			}

			writer.WriteEncodedInt(m_Guesses.Count);
			foreach (KeyValuePair<Mobile, PuzzleChestSolutionAndTime> kvp in m_Guesses)
			{
				writer.Write(kvp.Key);
				kvp.Value.Serialize(writer);
			}
		}

		public void ShowGump(Mobile from)
		{
			if (IsSolved) return;

			if (!IsInRange(from))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500446); // That is too far away.
				return;
			}

			PuzzleChestSolution solution = GetLastGuess(from);
			if (solution != null)
				solution = new PuzzleChestSolution(solution);
			else
				solution = new PuzzleChestSolution(PuzzleChestCylinder.None, PuzzleChestCylinder.None, PuzzleChestCylinder.None, PuzzleChestCylinder.None, PuzzleChestCylinder.None);

			from.CloseGump(typeof(PuzzleGump));
			from.SendGump(new PuzzleGump(from, this, solution, PuzzleGump.GetDefaultSelectedPedestalState()));
		}

		public void SubmitSolution(Mobile m, PuzzleChestSolution solution)
		{
			int correctCylinders, correctColors;
			if (solution.Matches(Solution, out correctCylinders, out correctColors))
			{
				IsSolved = true;
				m_Source.Take(m);
				m.SendMessage("The chest is released and you take it with you.");
				LoggingFunctions.LogStandard(m, "has stolen an item from a pedestal.");
			}
			else
			{
				m_Guesses[m] = new PuzzleChestSolutionAndTime(DateTime.UtcNow, solution);

				if (m.CheckSkill(SkillName.RemoveTrap, 0, 125))
					m.SendMessage("You pull back just in time to avoid a trap!");
				else
					StealBase.DoDamage(m);
			}
		}

		private void InitHints()
		{
			List<PuzzleChestCylinder> list = new List<PuzzleChestCylinder>(Solution.Cylinders.Length - 1);
			for (int i = 1; i < Solution.Cylinders.Length; i++)
				list.Add(Solution.Cylinders[i]);

			m_Hints = new PuzzleChestCylinder[HintsCount];

			for (int i = 0; i < m_Hints.Length; i++)
			{
				int pos = Utility.Random(list.Count);
				m_Hints[i] = list[pos];
				list.RemoveAt(pos);
			}
		}
	}
}