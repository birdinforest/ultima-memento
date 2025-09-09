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

		public void DoDamage(Mobile to)
		{
			switch (Utility.Random(4))
			{
				case 0:
					{
						Effects.SendLocationEffect(to, to.Map, 0x113A, 20, 10);
						to.PlaySound(0x231);
						to.LocalOverheadMessage(MessageType.Regular, 0x44, 1010523); // A toxic vapor envelops thee.

						to.ApplyPoison(to, Poison.Regular);

						break;
					}
				case 1:
					{
						Effects.SendLocationEffect(to, to.Map, 0x3709, 30);
						to.PlaySound(0x54);
						to.LocalOverheadMessage(MessageType.Regular, 0xEE, 1010524); // Searing heat scorches thy skin.

						AOS.Damage(to, to, Utility.RandomMinMax(10, 40), 0, 100, 0, 0, 0);

						break;
					}
				case 2:
					{
						to.PlaySound(0x223);
						to.LocalOverheadMessage(MessageType.Regular, 0x62, 1010525); // Pain lances through thee from a sharp metal blade.

						AOS.Damage(to, to, Utility.RandomMinMax(10, 40), 100, 0, 0, 0, 0);

						break;
					}
				default:
					{
						to.BoltEffect(0);
						to.LocalOverheadMessage(MessageType.Regular, 0xDA, 1010526); // Lightning arcs through thy body.

						AOS.Damage(to, to, Utility.RandomMinMax(10, 40), 0, 0, 0, 0, 100);

						break;
					}
			}
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
				DoDamage(m);
			}
		}

		protected void GenerateTreasure()
		{
			// DropItem(new Gold(600, 900));

			// List<Item> gems = new List<Item>();
			// for (int i = 0; i < 9; i++)
			// {
			// 	Item gem = Loot.RandomGem();
			// 	Type gemType = gem.GetType();

			// 	foreach (Item listGem in gems)
			// 	{
			// 		if (listGem.GetType() == gemType)
			// 		{
			// 			listGem.Amount++;
			// 			gem.Delete();
			// 			break;
			// 		}
			// 	}

			// 	if (!gem.Deleted)
			// 		gems.Add(gem);
			// }

			// foreach (Item gem in gems)
			// 	DropItem(gem);

			// if (0.2 > Utility.RandomDouble())
			// 	DropItem(new BagOfReagents(50));

			// for (int i = 0; i < 2; i++)
			// {
			// 	Item item;

			// 	if (Core.AOS)
			// 		item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();
			// 	else
			// 		item = Loot.RandomArmorOrShieldOrWeapon();

			// 	if (item is BaseWeapon)
			// 	{
			// 		BaseWeapon weapon = (BaseWeapon)item;

			// 		if (Core.AOS)
			// 		{
			// 			int attributeCount;
			// 			int min, max;

			// 			GetRandomAOSStats(out attributeCount, out min, out max);

			// 			BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);
			// 		}
			// 		else
			// 		{
			// 			weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(6);
			// 			weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(6);
			// 			weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random(6);
			// 		}

			// 		DropItem(item);
			// 	}
			// 	else if (item is BaseArmor)
			// 	{
			// 		BaseArmor armor = (BaseArmor)item;

			// 		if (Core.AOS)
			// 		{
			// 			int attributeCount;
			// 			int min, max;

			// 			GetRandomAOSStats(out attributeCount, out min, out max);

			// 			BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);
			// 		}
			// 		else
			// 		{
			// 			armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(6);
			// 			armor.Durability = (ArmorDurabilityLevel)Utility.Random(6);
			// 		}

			// 		DropItem(item);
			// 	}
			// 	else if (item is BaseHat)
			// 	{
			// 		BaseHat hat = (BaseHat)item;

			// 		if (Core.AOS)
			// 		{
			// 			int attributeCount;
			// 			int min, max;

			// 			GetRandomAOSStats(out attributeCount, out min, out max);

			// 			BaseRunicTool.ApplyAttributesTo(hat, attributeCount, min, max);
			// 		}

			// 		DropItem(item);
			// 	}
			// 	else if (item is BaseJewel)
			// 	{
			// 		int attributeCount;
			// 		int min, max;

			// 		GetRandomAOSStats(out attributeCount, out min, out max);

			// 		BaseRunicTool.ApplyAttributesTo((BaseJewel)item, attributeCount, min, max);

			// 		DropItem(item);
			// 	}
			// }

			Solution = new PuzzleChestSolution();
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