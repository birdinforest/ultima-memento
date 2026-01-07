using Server.Items;
using Server.Misc;
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
			if (4 < version)
			{
				RivalSlayerName = (SlayerName)reader.ReadInt();
				RivalBonusEnabled = reader.ReadBool();
				RivalBonusPoints = reader.ReadInt();
			}
			else
				GenerateRivalry();

			if (5 < version)
			{
				SelectedProfession = (StarterProfessions)reader.ReadInt();
				LifetimePointsGained = reader.ReadInt();
				LifetimeDeaths = reader.ReadInt();
			}

			if (6 < version)
			{
				UnlockRecordDiscovered = reader.ReadBool();
				LifetimeEnemyFactionKills = reader.ReadInt();
				LifetimeGameTime = reader.ReadTimeSpan();
				LifetimeCombatQuestCompletions = reader.ReadInt();
				LifetimeCreatureKills = reader.ReadInt();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{ get { return this != Default; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ImprovedTemplateCount { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int LifetimeCombatQuestCompletions { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int LifetimeCreatureKills { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int LifetimeDeaths { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int LifetimeEnemyFactionKills { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan LifetimeGameTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int LifetimePointsGained { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointGainRateLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointsFarmed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointsSaved { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int RecordedSkillCapLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RivalBonusEnabled { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int RivalBonusPoints { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName RivalSlayerName { get; set; }

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
		public bool UnlockRecordDiscovered { get; set; }

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
			writer.Write(7); // version

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
			writer.Write((int)RivalSlayerName);
			writer.Write(RivalBonusEnabled);
			writer.Write(RivalBonusPoints);
			writer.Write((int)SelectedProfession);
			writer.Write(LifetimePointsGained);
			writer.Write(LifetimeDeaths);
			writer.Write(UnlockRecordDiscovered);
			writer.Write(LifetimeEnemyFactionKills);
			writer.Write(LifetimeGameTime);
			writer.Write(LifetimeCombatQuestCompletions);
			writer.Write(LifetimeCreatureKills);
		}

		public override string ToString()
		{
			return "...";
		}
	}

	public partial class PlayerContext
	{
		public HashSet<StarterProfessions> BoostedTemplateCache { get; set; }

		public Dictionary<Categories, List<int>> RewardCache { get; set; }

		public StarterProfessions SelectedProfession { get; set; }

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

	public partial class PlayerContext
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasRivalFaction
		{ get { return RivalSlayerName != SlayerName.None; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public string RivalFactionName
		{
			get
			{
				switch (RivalSlayerName)
				{
					case SlayerName.None: return "None";
					case SlayerName.Silver: return "The Returned";
					case SlayerName.Repond: return "The Oathbreakers";
					case SlayerName.ReptilianDeath: return "The Scaled Ones";
					case SlayerName.Exorcism: return "The Dreadwings";
					case SlayerName.ArachnidDoom: return "The Doom Weavers";
					case SlayerName.ElementalBan: return "The Riftborn";
					case SlayerName.WizardSlayer: return "The Spellreavers";
					case SlayerName.AvianHunter: return "The Skycleave Talons";
					case SlayerName.SlimyScourge: return "The Oozen Swarm";
					case SlayerName.AnimalHunter: return "The Pack";
					case SlayerName.GiantKiller: return "The Colossal";
					case SlayerName.GolemDestruction: return "The Construct";
					case SlayerName.WeedRuin: return "The Briarblight";
					case SlayerName.NeptunesBane: return "The Tidebreakers";
					case SlayerName.Fey: return "The Faeborn Circle";

					default:
						return "Unknown Rival Race";
				}
			}
		}

		public void GenerateRivalry()
		{
			RivalSlayerName = Utility.Random(new SlayerName[]
			{
				SlayerName.Silver,
				SlayerName.Repond,
				SlayerName.ReptilianDeath,
				SlayerName.Exorcism,
				SlayerName.ArachnidDoom,
				SlayerName.ElementalBan,
				SlayerName.WizardSlayer,
				SlayerName.AvianHunter,
				SlayerName.SlimyScourge,
				SlayerName.AnimalHunter,
				SlayerName.GiantKiller,
				SlayerName.GolemDestruction,
				SlayerName.WeedRuin,
				SlayerName.NeptunesBane,
				SlayerName.Fey,
			});
			RivalBonusEnabled = true;
		}
	}
}