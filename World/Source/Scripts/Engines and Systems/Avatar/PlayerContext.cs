using System;
using System.Collections.Generic;

namespace Server.Engines.Avatar
{
	[PropertyObject]
	public partial class PlayerContext
	{
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
			if (1 < version) UnlockRecordSkillCaps = reader.ReadBool();
			Skills = 1 < version ? new SkillArchive(reader) : new SkillArchive();
			RecordedSkillCapLevel = 2 < version ? reader.ReadInt() : UnlockRecordSkillCaps ? 1 : 0;
			if (3 < version) UnlockRecordRecipes = reader.ReadBool();
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
		public bool UnlockRecordRecipes { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockRecordSkillCaps { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockSavageRace { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockSecondarySkillBoost { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockTemptations { get; set; }

		public void Serialize(GenericWriter writer)
		{
			writer.Write(4); // version

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
			writer.Write(UnlockRecordRecipes);
		}

		public override string ToString()
		{
			return "...";
		}
	}

	public partial class PlayerContext
	{
		public Dictionary<Categories, List<int>> RewardCache { get; set; }

		public void ClearRewardCache(Categories category)
		{
			if (RewardCache == null) return;

			RewardCache.Remove(category);
		}

		public int GetRecordedSkillCap()
		{
			return Math.Min(Constants.RECORDED_SKILL_CAP_MAX_AMOUNT, Constants.RECORDED_SKILL_CAP_MIN_AMOUNT + (RecordedSkillCapLevel * Constants.RECORDED_SKILL_CAP_INTERVAL));
		}
	}
}