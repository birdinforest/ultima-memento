using Server.Items;
using Server.Misc;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Avatar
{
	[PropertyObject]
	public partial class PlayerContext
	{
		public static readonly PlayerContext Default = new PlayerContext();

		private Serial _safetyDepositBoxSerial;
		private bool _unlockFullSkillArchive;

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
			if (0 < version)
			{
				ImprovedTemplateCount = reader.ReadInt();
				UnlockPrimarySkillBoost = reader.ReadBool();
				UnlockSecondarySkillBoost = reader.ReadBool();
				UnlockFugitiveMode = reader.ReadBool();
				UnlockMonsterRaces = reader.ReadBool();
				UnlockSavageRace = reader.ReadBool();
				UnlockTemptations = reader.ReadBool();
			}
			if (1 < version) UnlockRecordSkillCaps = reader.ReadBool();
			Skills = 1 < version ? new SkillArchive(reader) : new SkillArchive();
			RecordedSkillCapLevel = 2 < version ? reader.ReadInt() : 0;
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
				SelectedTemplate = (AvatarStarterTemplates)reader.ReadInt();
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

			if (version == 7)
			{
				if (0 < ImprovedTemplateCount)
				{
					// Refund old cost
					for (int i = 1; i <= ImprovedTemplateCount; i++)
					{
						PointsSaved += i * RewardFactory.ONE_THOUSAND_GOLD;
					}

					// Deduct new cost
					for (int i = 1; i <= ImprovedTemplateCount; i++)
					{
						PointsSaved -= i * RewardFactory.ONE_HUNDRED_GOLD;
					}
				}
			}

			if (8 < version)
			{
				_safetyDepositBoxSerial = reader.ReadInt();
				SafetyDepositBoxLevel = reader.ReadInt();
			}

			BoatSpeedLevel = 9 < version ? reader.ReadInt() : 0;

			DeserializeResearchFields( reader, version );

			// v13 (local bump): Jascen's template-unlock bools ride version 10 upstream, but our
			// v11/v12 carry the local research fields — gating on 12 keeps existing saves aligned.
			if ( 12 < version )
			{
				UnlockTemplateJester = reader.ReadBool();
				UnlockTemplateMystic = reader.ReadBool();
				UnlockTemplateShinobi = reader.ReadBool();
				UnlockTemplateDeathKnight = reader.ReadBool();
				UnlockTemplateHolyMan = reader.ReadBool();
			}

			if (13 < version) // v14 (local bump): upstream v11 rides past our v13 template bools
			{
				UnlockFullSkillArchive = reader.ReadBool();
			}

			if (version < 15) // v15 (local bump): upstream v13 - refund old pre-reduction template costs; runs once per old save
			{
				if (UnlockTemplateJester) PointsSaved += 40 * RewardFactory.ONE_THOUSAND_GOLD;
				if (UnlockTemplateMystic) PointsSaved += 40 * RewardFactory.ONE_THOUSAND_GOLD;
				if (UnlockTemplateShinobi) PointsSaved += 40 * RewardFactory.ONE_THOUSAND_GOLD;
				if (UnlockTemplateDeathKnight) PointsSaved += 40 * RewardFactory.ONE_THOUSAND_GOLD;
				if (UnlockTemplateHolyMan) PointsSaved += 40 * RewardFactory.ONE_THOUSAND_GOLD;
			}

			// v16 (local bump): Jascen's Draft mode rides version 13 upstream — gating on 15 keeps
			// existing v15 saves (template bools + skill archive + cost refund) reading correctly.
			if ( 15 < version )
			{
				_draftModeEnabled = reader.ReadBool();
				if (_draftModeEnabled)
				{
					var count = reader.ReadInt();
					_draftedSkills = new HashSet<SkillName>(count);
					for (int i = 0; i < count; i++)
					{
						_draftedSkills.Add((SkillName)reader.ReadInt());
					}
				}
			}

			// v17 (local bump): Jascen's prestige/banned-skills ride upstream v15 — gating on 16 keeps
			// existing v15 saves (pre-Draft or Draft-only) reading correctly.
			if (16 < version)
			{
				if (_draftModeEnabled)
				{
					var count = reader.ReadInt();
					_draftBannedSkills = new HashSet<SkillName>(count);
					for (int i = 0; i < count; i++)
					{
						_draftBannedSkills.Add((SkillName)reader.ReadInt());
					}
				}

				PrestigeLevel = reader.ReadInt();
				UseBetaCoinAlgorithm = reader.ReadBool();
			}
			else
			{
				if (_draftModeEnabled)
				{
					_draftBannedSkills = new HashSet<SkillName>();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{ get { return this != Default; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int BoatSpeedLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int GrandTotalPoints
		{ get { return LifetimePointsGained + PointsFarmed; } }

		public bool HasSafetyDepositBox
		{ get { return _safetyDepositBoxSerial != Serial.Zero && World.Items.ContainsKey(_safetyDepositBoxSerial); } }

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
		public int PrestigeLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int RecordedSkillCapLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RivalBonusEnabled { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int RivalBonusPoints { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName RivalSlayerName { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SafetyDepositBoxLevel { get; set; }

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
		public bool UnlockFullSkillArchive
		{
			get { return _unlockFullSkillArchive; }
			set
			{
				_unlockFullSkillArchive = value;
				ClearRewardCache(Categories.PrimaryBoosts);
				ClearRewardCache(Categories.SecondaryBoosts);
			}
		}

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

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UseBetaCoinAlgorithm { get; set; }

		public SafetyDepositBox GetOrCreateSafetyDepositBox(Mobile owner)
		{
			if (!HasSafetyDepositBox)
			{
				var box = new SafetyDepositBox(owner);
				_safetyDepositBoxSerial = box.Serial;
				owner.BankBox.AddItem(box);
				box.Location = new Point3D(0, 0, 0);

				return box;
			}

			return World.Items[_safetyDepositBoxSerial] as SafetyDepositBox;
		}

		public void Serialize(GenericWriter writer)
		{
			// v17 (local bump): v16 = Draft mode persistence, v17 = prestige + draft-banned skills
			writer.Write(17); // version

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
			writer.Write((int)SelectedTemplate);
			writer.Write(LifetimePointsGained);
			writer.Write(LifetimeDeaths);
			writer.Write(UnlockRecordDiscovered);
			writer.Write(LifetimeEnemyFactionKills);
			writer.Write(LifetimeGameTime);
			writer.Write(LifetimeCombatQuestCompletions);
			writer.Write(LifetimeCreatureKills);
			writer.Write(_safetyDepositBoxSerial);
			writer.Write(SafetyDepositBoxLevel);
			writer.Write(BoatSpeedLevel);

			SerializeResearchFields( writer );

			writer.Write(UnlockTemplateJester);
			writer.Write(UnlockTemplateMystic);
			writer.Write(UnlockTemplateShinobi);
			writer.Write(UnlockTemplateDeathKnight);
			writer.Write(UnlockTemplateHolyMan);
			writer.Write(UnlockFullSkillArchive);

			writer.Write(_draftModeEnabled);
			if (_draftModeEnabled)
			{
				writer.Write(_draftedSkills.Count);
				foreach (var skill in _draftedSkills)
				{
					writer.Write((int)skill);
				}

				writer.Write(_draftBannedSkills.Count);
				foreach (var skill in _draftBannedSkills)
				{
					writer.Write((int)skill);
				}
			}

			writer.Write(PrestigeLevel);
			writer.Write(UseBetaCoinAlgorithm);
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

	public partial class PlayerContext
	{
		public HashSet<AvatarStarterTemplates> BoostedTemplateCache { get; set; }

		public bool CanUnlockTemplateDeathKnight
		{ get { return 500 <= Skills[SkillName.Knightship]; } }

		public bool CanUnlockTemplateHolyMan
		{ get { return 300 <= Skills[SkillName.Healing] && 300 <= Skills[SkillName.Spiritualism]; } }

		public bool CanUnlockTemplateJester
		{ get { return 300 <= Skills[SkillName.Begging] && 300 <= Skills[SkillName.Psychology]; } }

		public bool CanUnlockTemplateMystic
		{ get { return 1000 <= Skills[SkillName.Focus] && 1000 <= Skills[SkillName.Meditation]; } }

		public bool CanUnlockTemplateShinobi
		{ get { return 500 <= Skills[SkillName.Ninjitsu]; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ImprovedTemplateCount { get; set; }

		public AvatarStarterTemplates SelectedTemplate { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockTemplateDeathKnight { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockTemplateHolyMan { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockTemplateJester { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockTemplateMystic { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UnlockTemplateShinobi { get; set; }

		public void ApplyTemplate(PlayerMobile player, AvatarStarterTemplates template)
		{
			if (!player.Avatar.Active) return;

			switch (template)
			{
				case AvatarStarterTemplates.Jester:
					{
						if (!DraftModeEnabled)
						{
							player.Skills[SkillName.Begging].Base = player.Avatar.Skills[SkillName.Begging];
							player.Skills[SkillName.Psychology].Base = player.Avatar.Skills[SkillName.Psychology];
						}

						player.AddItem(new BagOfTricks());
						break;
					}

				case AvatarStarterTemplates.Mystic:
					{
						if (!DraftModeEnabled)
						{
							player.Skills[SkillName.Focus].Base = player.Avatar.Skills[SkillName.Focus];
							player.Skills[SkillName.Meditation].Base = player.Avatar.Skills[SkillName.Meditation];
						}

						player.AddItem(new MysticSpellbook { Owner = player });
						break;
					}

				case AvatarStarterTemplates.Shinobi:
					{
						if (!DraftModeEnabled)
						{
							player.Skills[SkillName.Ninjitsu].Base = player.Avatar.Skills[SkillName.Ninjitsu];
						}

						player.AddItem(new ShinobiScroll { Owner = player });
						break;
					}

				case AvatarStarterTemplates.DeathKnight:
					{
						if (!DraftModeEnabled)
						{
							player.Skills[SkillName.Knightship].Base = player.Avatar.Skills[SkillName.Knightship];
						}

						player.Karma = -5000;
						player.AddItem(new DeathKnightSpellbook { Owner = player });
						break;
					}

				case AvatarStarterTemplates.HolyMan:
					{
						if (!DraftModeEnabled)
						{
							player.Skills[SkillName.Healing].Base = player.Avatar.Skills[SkillName.Healing];
							player.Skills[SkillName.Spiritualism].Base = player.Avatar.Skills[SkillName.Spiritualism];
						}

						player.AddItem(new HolyManSpellbook { Owner = player });
						break;
					}

				case AvatarStarterTemplates.Brute:
				case AvatarStarterTemplates.Acrobat:
				case AvatarStarterTemplates.Scholar:
					{
						// You get NOTHING!
						break;
					}

				default:
					if (template > AvatarStarterTemplates.DEFAULT_START && template < AvatarStarterTemplates.DEFAULT_END)
					{
						var profession = (StarterProfessions)template;
						var skills = CharacterCreation.GetTemplateSkills(profession);
						CharacterCreation.AddSkillBasedItems(player, skills);
					}
					break;
			}

			if (DraftModeEnabled)
			{
				// Draft players get items for every skill they drafted regardless of their current skill level
				var skills = DraftedSkills.Select(skillName => new SkillNameValue(skillName, 1)).ToArray();
				CharacterCreation.AddSkillBasedItems(player, skills);
			}
		}
	}
}