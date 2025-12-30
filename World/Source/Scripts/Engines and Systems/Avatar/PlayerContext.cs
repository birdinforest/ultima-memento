using System;
using System.Collections.Generic;

namespace Server.Engines.Avatar
{
	[PropertyObject]
	public class PlayerContext
	{
		public const int IMPROVED_TEMPLATE_MAX_COUNT = 5;
		public const int POINT_GAIN_RATE_MAX_LEVEL = 150;
		public const int POINT_GAIN_RATE_PER_LEVEL = 1;
		public const int RECORDED_SKILL_CAP_INTERVAL = 5;
		public const int RECORDED_SKILL_CAP_MAX_AMOUNT = 125;
		public const int RECORDED_SKILL_CAP_MAX_LEVEL = 25;
		public const int SKILL_CAP_MAX_LEVEL = 70;
		public const int SKILL_CAP_PER_LEVEL = 10;
		public const int SKILL_GAIN_RATE_MAX_LEVEL = 10;
		public const int SKILL_GAIN_RATE_PER_LEVEL = 10;
		public const int STAT_CAP_MAX_LEVEL = 150;
		public const int STAT_CAP_PER_LEVEL = 1;

		public static readonly PlayerContext Default = new PlayerContext();

		public PlayerContext()
		{
			Skills = new SkillArchive();
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
			if (0 < version) ImprovedTemplateCount = reader.ReadInt();
			if (0 < version) UnlockPrimarySkillBoost = reader.ReadBool();
			if (0 < version) UnlockSecondarySkillBoost = reader.ReadBool();
			if (0 < version) UnlockFugitiveMode = reader.ReadBool();
			if (0 < version) UnlockMonsterRaces = reader.ReadBool();
			if (0 < version) UnlockSavageRace = reader.ReadBool();
			if (0 < version) UnlockTemptations = reader.ReadBool();
			if (0 < version) UnlockRecordSkillCaps = reader.ReadBool();
			Skills = 1 < version ? new SkillArchive(reader) : new SkillArchive();
			RecordedSkillCapLevel = 2 < version ? reader.ReadInt() : UnlockRecordSkillCaps ? 1 : 0;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{ get { return this != Default; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ImprovedTemplateCount { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointGainRateLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointsFarmed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointsSaved { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int RecordedSkillCapLevel { get; set; }

		public Dictionary<Categories, List<int>> RewardCache { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SkillCapLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SkillGainRateLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillArchive Skills { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int StatCapLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockFugitiveMode { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockMonsterRaces { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockPrimarySkillBoost { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockRecordSkillCaps { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockSavageRace { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockSecondarySkillBoost { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockTemptations { get; set; }

		public int GetRecordedSkillCap()
		{
			return Math.Min(RECORDED_SKILL_CAP_MAX_AMOUNT, 50 + (RecordedSkillCapLevel * RECORDED_SKILL_CAP_INTERVAL));
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(3); // version

			writer.Write(PointsFarmed);
			writer.Write(PointsSaved);
			writer.Write(SkillCapLevel);
			writer.Write(StatCapLevel);
			writer.Write(SkillGainRateLevel);
			writer.Write(PointGainRateLevel);
			writer.Write(ImprovedTemplateCount);
			writer.Write(UnlockPrimarySkillBoost);
			writer.Write(UnlockSecondarySkillBoost);
			writer.Write(UnlockFugitiveMode);
			writer.Write(UnlockMonsterRaces);
			writer.Write(UnlockSavageRace);
			writer.Write(UnlockTemptations);
			writer.Write(UnlockRecordSkillCaps);
			Skills.Serialize(writer);
			writer.Write(RecordedSkillCapLevel);
		}

		public override string ToString()
		{
			return "...";
		}
	}
}