using System;

namespace Server.Engines.PuzzleChest
{
	public class PuzzleChestSolutionAndTime : PuzzleChestSolution
	{
		public PuzzleChestSolutionAndTime(DateTime when, PuzzleChestSolution solution) : base(solution)
		{
			When = when;
		}

		public PuzzleChestSolutionAndTime(GenericReader reader) : base(reader)
		{
			var version = reader.ReadEncodedInt();

			When = reader.ReadDeltaTime();
		}

		public DateTime When { get; private set; }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.WriteDeltaTime(When);
		}
	}
}