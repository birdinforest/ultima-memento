namespace Server.Engines.PuzzleChest
{
	public class PuzzleChestSolution
	{
		public const int Length = 5;

		public PuzzleChestSolution()
		{
			Cylinders = new PuzzleChestCylinder[Length];

			for (int i = 0; i < Cylinders.Length; i++)
			{
				Cylinders[i] = RandomCylinder();
			}
		}

		public PuzzleChestSolution(PuzzleChestCylinder first, PuzzleChestCylinder second, PuzzleChestCylinder third, PuzzleChestCylinder fourth, PuzzleChestCylinder fifth)
		{
			Cylinders = new PuzzleChestCylinder[Length];

			First = first;
			Second = second;
			Third = third;
			Fourth = fourth;
			Fifth = fifth;
		}

		public PuzzleChestSolution(PuzzleChestSolution solution)
		{
			Cylinders = new PuzzleChestCylinder[Length];

			for (var i = 0; i < Cylinders.Length; i++)
			{
				Cylinders[i] = solution.Cylinders[i];
			}
		}

		public PuzzleChestSolution(GenericReader reader)
		{
			Cylinders = new PuzzleChestCylinder[Length];

			var version = reader.ReadEncodedInt();

			var length = reader.ReadEncodedInt();
			for (var i = 0; ; i++)
			{
				if (i < length)
				{
					PuzzleChestCylinder cylinder = (PuzzleChestCylinder)reader.ReadInt();

					if (i < Cylinders.Length)
						Cylinders[i] = cylinder;
				}
				else if (i < Cylinders.Length)
				{
					Cylinders[i] = RandomCylinder();
				}
				else
				{
					break;
				}
			}
		}

		public PuzzleChestCylinder[] Cylinders { get; private set; }

		public PuzzleChestCylinder Fifth
		{ get { return Cylinders[4]; } set { Cylinders[4] = value; } }

		public PuzzleChestCylinder First
		{ get { return Cylinders[0]; } set { Cylinders[0] = value; } }

		public PuzzleChestCylinder Fourth
		{ get { return Cylinders[3]; } set { Cylinders[3] = value; } }

		public PuzzleChestCylinder Second
		{ get { return Cylinders[1]; } set { Cylinders[1] = value; } }

		public PuzzleChestCylinder Third
		{ get { return Cylinders[2]; } set { Cylinders[2] = value; } }

		public static PuzzleChestCylinder RandomCylinder()
		{
			switch (Utility.Random(8))
			{
				case 0: return PuzzleChestCylinder.LightBlue;
				case 1: return PuzzleChestCylinder.Blue;
				case 2: return PuzzleChestCylinder.Green;
				case 3: return PuzzleChestCylinder.Orange;
				case 4: return PuzzleChestCylinder.Purple;
				case 5: return PuzzleChestCylinder.Red;
				case 6: return PuzzleChestCylinder.DarkBlue;
				default: return PuzzleChestCylinder.Yellow;
			}
		}

		public bool Matches(PuzzleChestSolution solution, out int cylinders, out int colors)
		{
			cylinders = 0;
			colors = 0;
			if (solution == null) return false;

			var matchesSrc = new bool[solution.Cylinders.Length];
			var matchesDst = new bool[solution.Cylinders.Length];

			for (var i = 0; i < Cylinders.Length; i++)
			{
				if (Cylinders[i] == solution.Cylinders[i])
				{
					cylinders++;

					matchesSrc[i] = true;
					matchesDst[i] = true;
				}
			}

			for (var i = 0; i < Cylinders.Length; i++)
			{
				if (!matchesSrc[i])
				{
					for (int j = 0; j < solution.Cylinders.Length; j++)
					{
						if (Cylinders[i] == solution.Cylinders[j] && !matchesDst[j])
						{
							colors++;

							matchesDst[j] = true;
						}
					}
				}
			}

			return cylinders == Cylinders.Length;
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.WriteEncodedInt(Cylinders.Length);
			for (var i = 0; i < Cylinders.Length; i++)
			{
				writer.Write((int)Cylinders[i]);
			}
		}
	}
}