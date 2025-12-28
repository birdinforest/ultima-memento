using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Avatar
{
	public class RewardFactory
	{
		private const int ONE_THOUSAND_GOLD = 10000;

		public static List<IReward> CreateRewards(PlayerMobile m_From, Categories selectedCategory, PlayerContext context)
		{
			switch (selectedCategory)
			{
				default:
				case Categories.Information:
					{
						// Never reached
						return null;
					}

				case Categories.Unlocks:
					{
						return new List<IReward>
						{
							!context.UnlockPrimarySkillBoost
								? ActionReward.Create(
									ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Boost - Primary Skills",
									"Unlock the ability to boost your primary skills.",
									true,
									() => context.UnlockPrimarySkillBoost = true
								)
								: null,
							!context.UnlockSecondarySkillBoost
								? ActionReward.Create(
									ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Boost - Secondary Skills",
									"Unlock the ability to boost your secondary skills.",
									true,
									() => context.UnlockSecondarySkillBoost = true
								)
								: null,
							context.ImprovedTemplateCount < PlayerContext.IMPROVED_TEMPLATE_MAX_COUNT
								? ActionReward.Create(
									ONE_THOUSAND_GOLD * (context.ImprovedTemplateCount + 1),
									AvatarShopGump.NO_ITEM_ID,
									string.Format("Improved Starter Template ({0} of {1})", context.ImprovedTemplateCount, PlayerContext.IMPROVED_TEMPLATE_MAX_COUNT),
									string.Format("An Improved template received 10 additional skill points for each skill it starts with."),
									true,
									() => context.ImprovedTemplateCount += 1
								)
								: null,
							!context.UnlockTemptations
								? ActionReward.Create(
									5 * ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Temptation System",
									"Unlock the ability to use the Temptations system.",
									true,
									() => context.UnlockTemptations = true
								)
								: null,
							!context.UnlockMonsterRaces
								? ActionReward.Create(
									50 * ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Monster Characters",
									"Unlock the ability to create monster characters.",
									true,
									() => context.UnlockMonsterRaces = true
								)
								: null,
							!context.UnlockSavageRace
								? ActionReward.Create(
									100 * ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Savage Character",
									"Unlock the ability to create savage characters.",
									true,
									() => context.UnlockSavageRace = true
								)
								: null,
							!context.UnlockFugitiveMode
								? ActionReward.Create(
									150 * ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Fugitive Character",
									"Unlock the ability to use fugitive mode.",
									true,
									() => context.UnlockFugitiveMode = true
								)
								: null,
						}.Where(r => r != null).ToList();
					}

				case Categories.Limits:
					{
						var skillCapCost = 50 + 25 * (context.SkillCapLevel / 10);
						return new List<IReward>
						{
							ActionReward.Create(
								SecondOrderCost(skillCapCost, context.SkillCapLevel + 1),
								AvatarShopGump.FAT_BOTTLE_ITEM_ID,
								string.Format("Skill Cap ({0} of {1})", context.SkillCapLevel, PlayerContext.SKILL_CAP_MAX_LEVEL),
								string.Format("Increases the skill cap by {0}. Current bonus: {1}", PlayerContext.SKILL_CAP_PER_LEVEL, PlayerContext.SKILL_CAP_PER_LEVEL * context.SkillCapLevel),
								context.SkillCapLevel < PlayerContext.SKILL_CAP_MAX_LEVEL,
								() => context.SkillCapLevel += 1
							).AsStatic(),

							ActionReward.Create(
								SecondOrderCost(100, context.StatCapLevel + 1),
								AvatarShopGump.FAT_BOTTLE_ITEM_ID,
								string.Format("Stat Cap ({0} of {1})", context.StatCapLevel, PlayerContext.STAT_CAP_MAX_LEVEL),
								string.Format("Increases the stat cap by {0}. Current bonus: {1}", PlayerContext.STAT_CAP_PER_LEVEL, PlayerContext.STAT_CAP_PER_LEVEL * context.StatCapLevel),
								context.StatCapLevel < PlayerContext.STAT_CAP_MAX_LEVEL,
								() => context.StatCapLevel += 1
							).AsStatic(),
						};
					}

				case Categories.Rates:
					{
						return new List<IReward>
						{
							ActionReward.Create(
								SecondOrderCost(100, context.PointGainRateLevel + 1),
								AvatarShopGump.NO_ITEM_ID,
								string.Format("Coins Gain Rate ({0} of {1})", context.PointGainRateLevel, PlayerContext.POINT_GAIN_RATE_MAX_LEVEL),
								string.Format("Increases the coins gain rate by {0}%. Current bonus: {1}%", PlayerContext.POINT_GAIN_RATE_PER_LEVEL, PlayerContext.POINT_GAIN_RATE_PER_LEVEL * context.PointGainRateLevel),
								context.PointGainRateLevel < PlayerContext.POINT_GAIN_RATE_MAX_LEVEL,
								() => context.PointGainRateLevel += 1
							).AsStatic(),
							ActionReward.Create(
								ExponentialCost(2000, context.SkillGainRateLevel + 1),
								AvatarShopGump.NO_ITEM_ID,
								string.Format("Skill Gain Rate ({0} of {1})", context.SkillGainRateLevel, PlayerContext.SKILL_GAIN_RATE_MAX_LEVEL),
								string.Format("Increases the skill gain rate by {0}%. Current bonus: {1}%", PlayerContext.SKILL_GAIN_RATE_PER_LEVEL, PlayerContext.SKILL_GAIN_RATE_PER_LEVEL * context.SkillGainRateLevel),
								context.SkillGainRateLevel < PlayerContext.SKILL_GAIN_RATE_MAX_LEVEL,
								() => context.SkillGainRateLevel += 1
							).AsStatic(),
						};
					}

				case Categories.Templates:
					{
						Action<Func<PlayerMobile, bool>> applyTemplate = action =>
						{
							if (m_From.NetState == null) return;

							var confirmation = new ConfirmationGump(
								m_From,
								"Select Template?",
								string.Format("Are you sure you wish to select this template? This is a {0} that will recreate your backpack, reduce existing stats, and change skills.", TextDefinition.GetColorizedText("destructive action", HtmlColors.RED)),
								() =>
								{
									if (m_From.Backpack != null)
										m_From.Backpack.Delete();

									SkillCheck.DisableSkillGains = true;

									// Reduce all skills to 0
									for (var i = 0; i < m_From.Skills.Length; i++)
									{
										var skill = m_From.Skills[i];
										if (0 < skill.Base)
											skill.Base = 0;
									}

									m_From.NetState.BlockAllPackets = true;
									CharacterCreation.InitializeBackpack(m_From);
									m_From.NetState.BlockAllPackets = false;

									var boosted = action(m_From);

									// Set Lock status and boost if necessary
									for (var i = 0; i < m_From.Skills.Length; i++)
									{
										Skill skill = m_From.Skills[i];
										if (skill == null) continue;

										if (0 < skill.Value)
										{
											skill.SetLockNoRelay(SkillLock.Up);
											if (boosted)
												skill.BaseFixedPoint += 100; // +10 to each skill that was set
										}
										else
										{
											skill.SetLockNoRelay(SkillLock.Locked);
										}
									}

									SkillCheck.DisableSkillGains = false;

									AvatarEngine.Instance.ApplyContext(m_From, m_From.Avatar);
									m_From.OnSkillsQuery(m_From);
									m_From.SendMessage("Your skills have been set to the chosen template. All other skills have been set to Locked.");
								}
							);
							m_From.SendGump(confirmation);
						};
						var rewards = new List<IReward>
						{
							ActionReward.Create(
								AvatarShopGump.COST_FREE,
								AvatarShopGump.NO_ITEM_ID,
								"The Brute",
								"Starts with 60 strength, 10 dexterity, and 10 intelligence.",
								true,
								() =>
								{
									applyTemplate(
										player =>
										{
											m_From.InitStats(60, 10, 10);
											return false;
										}
									);
								}
							),
							ActionReward.Create(
								AvatarShopGump.COST_FREE,
								AvatarShopGump.NO_ITEM_ID,
								"The Acrobat",
								"Starts with 10 strength, 60 dexterity, and 10 intelligence.",
								true,
								() =>
								{
									applyTemplate(
										player =>
										{
											m_From.InitStats(10, 60, 10);
											return false;
										}
									);
								}
							),
							ActionReward.Create(
								AvatarShopGump.COST_FREE,
								AvatarShopGump.NO_ITEM_ID,
								"The Scholar",
								"Starts with 10 strength, 10 dexterity, and 60 intelligence.",
								true,
								() =>
								{
									applyTemplate(
										player =>
										{
											m_From.InitStats(10, 10, 60);
											return false;
										}
									);
								}
							),
						};

						var professions = new List<StarterProfessions>
						{
							// StarterProfessions.Custom,
							StarterProfessions.Ninja,
							StarterProfessions.Bard,
							StarterProfessions.Druid,
							StarterProfessions.Knight,
							StarterProfessions.Warrior,
							StarterProfessions.Mage,
							StarterProfessions.Archer,
						};

						var boostedProfessions = new HashSet<StarterProfessions>();
						if (0 < context.ImprovedTemplateCount && context.ImprovedTemplateCount <= professions.Count)
						{
							// Keep boosting a random profession until we reach our max
							while (boostedProfessions.Count != context.ImprovedTemplateCount)
							{
								boostedProfessions.Add(Utility.Random(professions));
							}
						}

						foreach (var profession in professions.OrderBy(p => p.ToString()))
						{
							var boosted = 0 < boostedProfessions.Count && boostedProfessions.Contains(profession);
							rewards.Add(ActionReward.Create(
								AvatarShopGump.COST_FREE,
								AvatarShopGump.NO_ITEM_ID,
								string.Format("The {0}{1}", profession.ToString(), boosted ? " (Improved)" : ""),
								string.Format("Start with the stats, skills, and items of a {0}.", profession.ToString()),
								true,
								() =>
								{
									applyTemplate(
										player =>
										{
											var skills = CharacterCreation.SetTemplateSkills(player, profession);
											CharacterCreation.AddSkillBasedItems(player, skills);
											return boosted;
										}
									);
								}
							));
						}
						return rewards;
					}

				case Categories.Boosts:
					{
						var rewards = new List<IReward>();

						var nextStatCost = SecondOrderCost(1, Math.Max(1, m_From.RawStatTotal - 90));
						var canSelectStatBoost = m_From.RawStatTotal < m_From.StatCap;

						// Stats
						rewards.Add(
							ActionReward.Create(
								nextStatCost,
								AvatarShopGump.NO_ITEM_ID,
								"Strength Boost",
								"Increases your strength by 1 point.",
								canSelectStatBoost,
								() =>
								{
									m_From.RawStr += 1;
								}
							)
						);
						rewards.Add(
							ActionReward.Create(
								nextStatCost,
								AvatarShopGump.NO_ITEM_ID,
								"Dexterity Boost",
								"Increases your dexterity by 1 point.",
								canSelectStatBoost,
								() =>
								{
									m_From.RawDex += 1;
								}
							)
						);
						rewards.Add(
							ActionReward.Create(
								nextStatCost,
								AvatarShopGump.NO_ITEM_ID,
								"Intelligence Boost",
								"Increases your intelligence by 1 point.",
								canSelectStatBoost,
								() =>
								{
									m_From.RawInt += 1;
								}
							)
						);

						// Skills
						if (context.UnlockPrimarySkillBoost || context.UnlockSecondarySkillBoost)
						{
							var primarySkills = new List<Skill>();
							var secondarySkills = new List<Skill>();
							for (var i = 0; i < m_From.Skills.Length; i++)
							{
								var skill = m_From.Skills[i];
								if (skill.IsSecondarySkill())
								{
									secondarySkills.Add(skill);
								}
								else
								{
									primarySkills.Add(skill);
								}
							}

							if (context.UnlockPrimarySkillBoost)
							{
								var nextPrimarySkillCost = SecondOrderCost(1, Math.Max(1, primarySkills.Sum(s => s.BaseFixedPoint) / 10));
								foreach (var skill in primarySkills)
								{
									rewards.Add(
										ActionReward.Create(
											nextPrimarySkillCost,
											AvatarShopGump.NO_ITEM_ID,
											string.Format("{0} ({1} of {2})", skill.Name, skill.BaseFixedPoint / 10, skill.CapFixedPoint / 10),
											string.Format("Increases your skill in {0} by 1 point.", skill.Name),
											skill.BaseFixedPoint < skill.CapFixedPoint,
											() =>
											{
												skill.BaseFixedPoint += 10;
											}
										)
									);
								}
							}

							if (context.UnlockSecondarySkillBoost)
							{
								var nextSecondarySkillCost = SecondOrderCost(1, Math.Max(1, secondarySkills.Sum(s => s.BaseFixedPoint) / 10));
								foreach (var skill in secondarySkills)
								{
									rewards.Add(
										ActionReward.Create(
											nextSecondarySkillCost,
											AvatarShopGump.NO_ITEM_ID,
											string.Format("{0} ({1} of {2})", skill.Name, skill.BaseFixedPoint / 10, skill.CapFixedPoint / 10),
											string.Format("Increases your skill in {0} by 1 point.", skill.Name),
											skill.BaseFixedPoint < skill.CapFixedPoint,
											() =>
											{
												skill.BaseFixedPoint += 10;
											}
										)
									);
								}
							}
						}

						return rewards;
					}

				case Categories.Items:
					{
						return new List<IReward>
						{
							// Currency
							ItemReward.Create(
								1000,
								true,
								() => { return new Gold(500); },
								amount: 500,
								graphicOverride: AvatarShopGump.GOLD_STACK_ITEM_ID
							),
							ItemReward.Create(
								10000,
								true,
								() => { return new Gold(5000); },
								amount: 5000,
								graphicOverride: AvatarShopGump.GOLD_STACK_ITEM_ID
							),

							// Resources
							ItemReward.Create(
								500,
								true,
								() => { return new IronIngot(100); },
								100
							),
							ItemReward.Create(
								500,
								true,
								() => { return new Fabric(100); },
								100
							),
							ItemReward.Create(
								200,
								true,
								() => { return new Bottle(); },
								10
							),

							// Tools
							ItemReward.Create(
								500,
								true,
								() => { return new TinkerTools(); }
							),
							ItemReward.Create(
								500,
								true,
								() => { return new SmithHammer(); }
							),
							ItemReward.Create(
								500,
								true,
								() => { return new CarpenterTools(); }
							),
							ItemReward.Create(
								500,
								true,
								() => { return new SewingKit(); }
							),
							ItemReward.Create(
								500,
								true,
								() => { return new Hatchet(); }
							),
							ItemReward.Create(
								500,
								true,
								() => { return new Spade(); }
							),
							ItemReward.Create(
								500,
								true,
								() => { return new FishingPole(); }
							),
							ItemReward.Create(
								200,
								true,
								() => { return new Scissors(); }
							),

							// Equipment
							ItemReward.Create(
								1000,
								true,
								() => { return new HikingBoots(); }
							),
							ItemReward.Create(
								500,
								true,
								() => { return new BookOfChivalry(); }
							),
							ItemReward.Create(
								500,
								true,
								() => { return new NecromancerSpellbook(); }
							).WithName("Necromancer Spellbook (Empty)"),
							ItemReward.Create(
								500,
								true,
								() => { return new Spellbook(); }
							).WithName("Mage's Spellbook (Empty)"),
							ItemReward.Create(
								500,
								true,
								() => { return new ElementalSpellbook(); }
							).WithName("Elementalist Spellbook (Empty)"),

							// Utility
							ItemReward.Create(
								1000,
								true,
								() => {
									var bag = new Bag();
									bag.AddItem(new Bandage(50));
									bag.AddItem(new Scissors());
									bag.AddItem(new Fabric(50));

									return bag;
								 }
							).WithName("Healer's Kit"),
							ItemReward.Create(
								1000,
								true,
								() => {
									var bag = new Bag();
									bag.AddItem(new CurePotion() { Amount = 10 });
									bag.AddItem(new HealPotion() { Amount = 10 });
									bag.AddItem(new RefreshPotion() { Amount = 10 });

									return bag;
								 }
							).WithName("Warrior's Potion Bag"),
							ItemReward.Create(
								1000,
								true,
								() => { return new BagOfReagents(); }
							).WithName("Bag of Reagents"),
							ItemReward.Create(
								500,
								true,
								() => { return new BagOfNecroReagents(); }
							).WithName("Bag of Necro Reagents"),
							ItemReward.Create(
								10000,
								true,
								() => { return new SmallBoatDeed(); }
							),
							ItemReward.Create(
								30000,
								true,
								() => { return new MagicCarpetADeed(); }
							),

							// Tames
							ItemReward.Create(
								500,
								true,
								() => { return new CagedHorse(); }
							),
							ItemReward.Create(
								1000,
								true,
								() => { return new CagedPackHorse(); }
							),
						};
					}
			}
		}

		private static int ExponentialCost(int baseCost, int level)
		{
			var cost = baseCost;
			if (level <= 0) return cost;

			for (int i = 0; i < level; i++)
			{
				cost *= 2;
			}

			return cost;
		}

		private static int SecondOrderCost(double baseCost, int level)
		{
			return (int)(baseCost * Math.Pow(level, 2) + baseCost * level);
		}
	}
}