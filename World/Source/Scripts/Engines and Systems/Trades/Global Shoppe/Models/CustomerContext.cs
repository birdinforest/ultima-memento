namespace Server.Engines.GlobalShoppe
{
	[PropertyObject]
	public class CustomerContext
	{
		public CustomerContext()
		{
		}

		public CustomerContext(GenericReader reader)
		{
			int version = reader.ReadInt();

			Description = reader.ReadString();
			Person = reader.ReadString();
			GoldReward = reader.ReadInt();
			ToolCost = reader.ReadInt();
			ResourceCost = reader.ReadInt();
			Difficulty = reader.ReadInt();
			ReputationReward = reader.ReadInt();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Description { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Difficulty { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int GoldReward { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Person { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ReputationReward { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ResourceCost { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ToolCost { get; set; }

		public void Serialize(GenericWriter writer)
		{
			writer.Write((int)0); // version

			writer.Write(Description);
			writer.Write(Person);
			writer.Write(GoldReward);
			writer.Write(ToolCost);
			writer.Write(ResourceCost);
			writer.Write(Difficulty);
			writer.Write(ReputationReward);
		}
	}
}