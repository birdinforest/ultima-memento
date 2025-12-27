using System.Collections.Generic;

namespace Server.Engines.Avatar
{
	[PropertyObject]
	public class PlayerContext
	{
		public const int POINT_GAIN_RATE_MAX_LEVEL = 150;
		public const int POINT_GAIN_RATE_PER_LEVEL = 1;
		public const int SKILL_CAP_MAX_LEVEL = 70;
		public const int SKILL_CAP_PER_LEVEL = 10;
		public const int SKILL_GAIN_RATE_MAX_LEVEL = 10;
		public const int SKILL_GAIN_RATE_PER_LEVEL = 10;
		public const int STAT_CAP_MAX_LEVEL = 150;
		public const int STAT_CAP_PER_LEVEL = 1;

		public static readonly PlayerContext Default = new PlayerContext();

		public PlayerContext()
		{
		}

		public PlayerContext(GenericReader reader)
		{
			int version = reader.ReadInt();

			PointsFarmed = reader.ReadInt();
			PointsSaved = reader.ReadInt();
			SkillCapLevel = reader.ReadInt();
			StatCapLevel = reader.ReadInt();
			SkillGainRateLevel = reader.ReadInt();
			PointGainRateLevel = reader.ReadInt();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{ get { return this != Default; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointGainRateLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointsFarmed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointsSaved { get; set; }

		public Dictionary<Categories, List<int>> RewardCache { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SkillCapLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SkillGainRateLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int StatCapLevel { get; set; }

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(PointsFarmed);
			writer.Write(PointsSaved);
			writer.Write(SkillCapLevel);
			writer.Write(StatCapLevel);
			writer.Write(SkillGainRateLevel);
			writer.Write(PointGainRateLevel);
		}

		public override string ToString()
		{
			return "...";
		}
	}
}