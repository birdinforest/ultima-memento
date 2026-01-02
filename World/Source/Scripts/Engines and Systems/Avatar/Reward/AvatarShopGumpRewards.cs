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
		private const int ONE_HUNDRED_GOLD = 1000;
		private const int ONE_THOUSAND_GOLD = 10000;
		private const int TEN_GOLD = 100;

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
							!context.UnlockPrimarySkillBoost && context.UnlockRecordSkillCaps
								? ActionReward.Create(
									2 * ONE_HUNDRED_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Jack of No Trades",
									"Learn from the greatest masters. Unlock the ability to restore Primary skills.",
									true,
									() => {
										context.UnlockPrimarySkillBoost = true;
										context.ClearRewardCache(Categories.PrimaryBoosts);
										context.ClearRewardCache(Categories.SecondaryBoosts);
										m_From.SendMessage("Some of your Primary skills are now available in the Skill Archive.");
									}
								)
								: null,
							!context.UnlockSecondarySkillBoost && context.UnlockRecordSkillCaps
								? ActionReward.Create(
									2 * ONE_HUNDRED_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Artisan's Mastery",
									"Master the crafts. Unlock the ability to restore Secondary skills.",
									true,
									() => {
										context.UnlockSecondarySkillBoost = true;
										context.ClearRewardCache(Categories.PrimaryBoosts);
										context.ClearRewardCache(Categories.SecondaryBoosts);
										m_From.SendMessage("Some of your Secondary skills are now available in the Skill Archive.");
									}
								)
								: null,
							context.ImprovedTemplateCount < Constants.IMPROVED_TEMPLATE_MAX_COUNT
								? ActionReward.Create(
									ONE_THOUSAND_GOLD * (context.ImprovedTemplateCount + 1),
									AvatarShopGump.NO_ITEM_ID,
									string.Format("Blessed Beginnings ({0} of {1})", context.ImprovedTemplateCount, Constants.IMPROVED_TEMPLATE_MAX_COUNT),
									string.Format("Awaken to your true potential. Ancestral relatives may enhance your template choices."),
									true,
									() => {
										context.ImprovedTemplateCount += 1;
										m_From.SendMessage("Your templates may now spawn as (Improved).");
									}
								)
								: null,
							!context.UnlockTemptations
								? ActionReward.Create(
									5 * ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Power Overwhelming",
									"Answer the seductive call of power. Gain strength through temptation and desire.",
									true,
									() => {
										context.UnlockTemptations = true;
										m_From.SendMessage("You have unlocked the ability to use Temptations.");
									}
								)
								: null,
							!context.UnlockMonsterRaces
								? ActionReward.Create(
									50 * ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Bestial Transformation",
									"Embrace your monstrous nature. Live among the supernatural and the beastly races of Ultima.",
									true,
									() => {
										context.UnlockMonsterRaces = true;
										m_From.SendMessage("You have unlocked the option to select a non-human race.");
									}
								)
								: null,
							!context.UnlockSavageRace
								? ActionReward.Create(
									100 * ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Primal Awakening",
									"Return to your untamed roots. Live life as a savage and embrace your barbaric heritage.",
									true,
									() => {
										context.UnlockSavageRace = true;
										m_From.SendMessage("You have unlocked a new tarot card for Humans.");
									}
								)
								: null,
							!context.UnlockFugitiveMode
								? ActionReward.Create(
									150 * ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Outlaw's Mark",
									"Bear the mark of the hunted. Strengthen your core and live as an exile.",
									true,
									() => {
										context.UnlockFugitiveMode = true;
										m_From.SendMessage("You have unlocked a new tarot card for Monsters and Humans.");
									}
								)
								: null,
							!context.UnlockRecordSkillCaps
								? ActionReward.Create(
									ONE_HUNDRED_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Erudian Teachings",
									"Reinforce your mind. Higher learning will become second nature.",
									true,
									() => {
										context.UnlockRecordSkillCaps = true;
										context.ClearRewardCache(Categories.PrimaryBoosts);
										context.ClearRewardCache(Categories.SecondaryBoosts);
										m_From.SendMessage("Your increased skill caps are now permanently unlocked.");
									}
								)
								: null,
							!context.UnlockRecordRecipes
								? ActionReward.Create(
									ONE_THOUSAND_GOLD,
									AvatarShopGump.NO_ITEM_ID,
									"Crafter Lineage",
									"Record recipes that you have learned.",
									true,
									() => {
										context.UnlockRecordRecipes = true;
										m_From.SendMessage("Your recipes are now permanently unlocked.");
									}
								)
								: null,
						}.Where(r => r != null).ToList();
					}

				case Categories.Limits:
					{
						var skillCapCost = 50 + 25 * (context.SkillCapLevel / 10);
						var currentErudianBonus = context.GetRecordedSkillCap();
						var erudianCapCost = 0;
						if (currentErudianBonus < 70) erudianCapCost = SecondOrderCost(100, context.RecordedSkillCapLevel + 1);
						else if (currentErudianBonus < 90) erudianCapCost = SecondOrderCost(200, context.RecordedSkillCapLevel + 1);
						else if (currentErudianBonus < 100) erudianCapCost = SecondOrderCost(400, context.RecordedSkillCapLevel + 1);
						else erudianCapCost = SecondOrderCost(800, context.RecordedSkillCapLevel + 1);

						return new List<IReward>
						{
							context.UnlockRecordSkillCaps
								? ActionReward.Create(
									erudianCapCost,
									AvatarShopGump.FAT_BOTTLE_ITEM_ID,
									string.Format("Erudian Knowledge ({0} of {1})", context.RecordedSkillCapLevel, Constants.RECORDED_SKILL_CAP_MAX_LEVEL),
									string.Format("Increases the maximum of skill that Boosts can provide by {0}. Current maximum: {1}", Constants.RECORDED_SKILL_CAP_INTERVAL, context.GetRecordedSkillCap()),
									context.RecordedSkillCapLevel < Constants.RECORDED_SKILL_CAP_MAX_LEVEL,
									() =>
									{
										context.RecordedSkillCapLevel += 1;
										context.ClearRewardCache(Categories.PrimaryBoosts);
										context.ClearRewardCache(Categories.SecondaryBoosts);
									}
								).AsStatic()
								: null,
							ActionReward.Create(
								SecondOrderCost(skillCapCost, context.SkillCapLevel + 1),
								AvatarShopGump.FAT_BOTTLE_ITEM_ID,
								string.Format("Skill Cap ({0} of {1})", context.SkillCapLevel, Constants.SKILL_CAP_MAX_LEVEL),
								string.Format("Increases the skill cap by {0}. Current bonus: {1}", Constants.SKILL_CAP_PER_LEVEL, Constants.SKILL_CAP_PER_LEVEL * context.SkillCapLevel),
								context.SkillCapLevel < Constants.SKILL_CAP_MAX_LEVEL,
								() => context.SkillCapLevel += 1
							).AsStatic(),

							ActionReward.Create(
								SecondOrderCost(100, context.StatCapLevel + 1),
								AvatarShopGump.FAT_BOTTLE_ITEM_ID,
								string.Format("Stat Cap ({0} of {1})", context.StatCapLevel, Constants.STAT_CAP_MAX_LEVEL),
								string.Format("Increases the stat cap by {0}. Current bonus: {1}", Constants.STAT_CAP_PER_LEVEL, Constants.STAT_CAP_PER_LEVEL * context.StatCapLevel),
								context.StatCapLevel < Constants.STAT_CAP_MAX_LEVEL,
								() => context.StatCapLevel += 1
							).AsStatic(),
						}.Where(r => r != null).ToList();
					}

				case Categories.Rates:
					{
						return new List<IReward>
						{
							ActionReward.Create(
								SecondOrderCost(100, context.PointGainRateLevel + 1),
								AvatarShopGump.NO_ITEM_ID,
								string.Format("Coins Gain Rate ({0} of {1})", context.PointGainRateLevel, Constants.POINT_GAIN_RATE_MAX_LEVEL),
								string.Format("Increases the coins gain rate by {0}%. Current bonus: {1}%", Constants.POINT_GAIN_RATE_PER_LEVEL, Constants.POINT_GAIN_RATE_PER_LEVEL * context.PointGainRateLevel),
								context.PointGainRateLevel < Constants.POINT_GAIN_RATE_MAX_LEVEL,
								() => context.PointGainRateLevel += 1
							).AsStatic(),
							ActionReward.Create(
								ExponentialCost(2000, context.SkillGainRateLevel + 1),
								AvatarShopGump.NO_ITEM_ID,
								string.Format("Skill Gain Rate ({0} of {1})", context.SkillGainRateLevel, Constants.SKILL_GAIN_RATE_MAX_LEVEL),
								string.Format("Increases the skill gain rate by {0}%. Current bonus: {1}%", Constants.SKILL_GAIN_RATE_PER_LEVEL, Constants.SKILL_GAIN_RATE_PER_LEVEL * context.SkillGainRateLevel),
								context.SkillGainRateLevel < Constants.SKILL_GAIN_RATE_MAX_LEVEL,
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

				case Categories.PrimaryBoosts:
				case Categories.SecondaryBoosts:
					{
						var rewards = new List<IReward>();

						// Skills
						if ((context.UnlockPrimarySkillBoost && selectedCategory == Categories.PrimaryBoosts) || (context.UnlockSecondarySkillBoost && selectedCategory == Categories.SecondaryBoosts))
						{
							var skills = new List<Skill>();
							for (var i = 0; i < m_From.Skills.Length; i++)
							{
								var skill = m_From.Skills[i];
								if (skill.SkillName == SkillName.Mysticism) continue;
								if (skill.SkillName == SkillName.Imbuing) continue;
								if (skill.SkillName == SkillName.Throwing) continue;

								if (!skill.IsSecondarySkill())
								{
									if (!context.UnlockPrimarySkillBoost) continue;
								}
								else
								{
									if (!context.UnlockSecondarySkillBoost) continue;
								}

								skills.Add(skill);
							}

							foreach (var skill in skills)
							{
								const int NEOPHYTE_SKILL_VALUE = 300;
								var archiveValue = context.Skills[skill.SkillName];
								if (archiveValue < NEOPHYTE_SKILL_VALUE) continue;

								var maxValue = Math.Min(archiveValue / 10f, context.GetRecordedSkillCap());
								var maxValueFixedPoint = (int)(maxValue * 10);
								rewards.Add(
									ActionReward.Create(
										AvatarShopGump.COST_FREE,
										AvatarShopGump.NO_ITEM_ID,
										string.Format("{0}", skill.Name),
										string.Format("Raise your skill in {0} up to {1:n1}", skill.Name, maxValue),
										skill.BaseFixedPoint < maxValueFixedPoint,
										() =>
										{
											if (skill.IsSecondarySkill())
											{
												skill.BaseFixedPoint = maxValueFixedPoint;
											}
											else
											{
												var amountToGain = maxValueFixedPoint - skill.BaseFixedPoint;
												var amountAvailable = Math.Max(0, m_From.SkillsCap - m_From.SkillsTotal);

												var amountToIncrease = amountToGain;
												if (amountAvailable < amountToIncrease)
												{
													var amountRequired = amountToIncrease - amountAvailable;
													for (int i = 0; i < m_From.Skills.Length; ++i)
													{
														if (m_From.Skills[i].Lock != SkillLock.Down)
															continue;

														if (amountRequired >= m_From.Skills[i].BaseFixedPoint)
														{
															amountRequired -= m_From.Skills[i].BaseFixedPoint;
															m_From.Skills[i].Base = 0.0;
														}
														else
														{
															m_From.Skills[i].BaseFixedPoint -= amountRequired;
															amountRequired = 0;
															break;
														}
													}

													// Didn't get enough free points, so we'll just take what we can get
													if (0 < amountRequired)
														amountToIncrease -= amountRequired;
												}

												skill.BaseFixedPoint += amountToIncrease;
											}
										}
									)
								);
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
								ONE_THOUSAND_GOLD,
								true,
								() => { return new Gold(500); },
								amount: 500,
								graphicOverride: AvatarShopGump.GOLD_STACK_ITEM_ID
							),
							ItemReward.Create(
								5 * ONE_THOUSAND_GOLD,
								true,
								() => { return new Gold(5000); },
								amount: 5000,
								graphicOverride: AvatarShopGump.GOLD_STACK_ITEM_ID
							),

							// Resources
							ItemReward.Create(
								2 * ONE_HUNDRED_GOLD,
								true,
								() => { return new IronIngot(50); },
								50
							).WithDescription("A handful of ingots to get you started."),
							ItemReward.Create(
								ONE_HUNDRED_GOLD,
								true,
								() => { return new Fabric(50); },
								50
							).WithDescription("A handful of fabric to get you started."),
							ItemReward.Create(
								5 * TEN_GOLD,
								true,
								() => { return new Bottle(); },
								10
							).WithDescription("A handful of bottles to get you started."),

							// Tools
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => { return new TinkerTools(); }
							),
							ItemReward.Create(
								5 * TEN_GOLD,
								true,
								() => { return new SmithHammer(); }
							),
							ItemReward.Create(
								5 * TEN_GOLD,
								true,
								() => { return new CarpenterTools(); }
							),
							ItemReward.Create(
								5 * TEN_GOLD,
								true,
								() => { return new SewingKit(); }
							),
							ItemReward.Create(
								5 * TEN_GOLD,
								true,
								() => { return new Hatchet(); }
							),
							ItemReward.Create(
								5 * TEN_GOLD,
								true,
								() => { return new Spade(); }
							),
							ItemReward.Create(
								5 * TEN_GOLD,
								true,
								() => { return new FishingPole(); }
							),
							ItemReward.Create(
								5 * TEN_GOLD,
								true,
								() => { return new Scissors(); }
							),

							// Equipment
							ItemReward.Create(
								ONE_THOUSAND_GOLD,
								true,
								() => { return new HikingBoots(); }
							),
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => { return new BookOfChivalry(); }
							),
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => { return new NecromancerSpellbook(); }
							).WithName("Necromancer Spellbook (Empty)"),
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => { return new Spellbook(); }
							).WithName("Mage's Spellbook (Empty)"),
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => { return new ElementalSpellbook(); }
							).WithName("Elementalist Spellbook (Empty)"),

							// Utility
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => {
									var bag = new Bag();
									bag.AddItem(new Scissors());
									bag.AddItem(new Fabric(50));

									return bag;
								 }
							).WithName("Healer's Kit").WithDescription("Contains scissors and fabric."),
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => {
									var bag = new Bag();
									bag.AddItem(new CurePotion() { Amount = 10 });
									bag.AddItem(new HealPotion() { Amount = 10 });
									bag.AddItem(new RefreshPotion() { Amount = 10 });

									return bag;
								 }
							).WithName("Warrior's Potion Bag").WithDescription("Contains healing, cure, and refresh potions."),
							ItemReward.Create(
								ONE_THOUSAND_GOLD,
								true,
								() => { return new BagOfReagents(); }
							).WithName("Bag of Reagents").WithDescription("Contains magery reagents for spells."),
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => { return new BagOfNecroReagents(); }
							).WithName("Bag of Necro Reagents").WithDescription("Contains necromancy reagents for spells."),
							ItemReward.Create(
								10 * ONE_THOUSAND_GOLD,
								true,
								() => { return new SmallBoatDeed(); }
							).WithDescription("Hit the seas sailing!"),
							ItemReward.Create(
								30 * ONE_THOUSAND_GOLD,
								true,
								() => { return new MagicCarpetADeed(); }
							).WithDescription("I can show you the world!"),

							// Tames
							ItemReward.Create(
								5 * ONE_HUNDRED_GOLD,
								true,
								() => { return new CagedHorse(); }
							).WithDescription("A horse is a great way to get around."),
							ItemReward.Create(
								ONE_THOUSAND_GOLD,
								true,
								() => { return new CagedPackHorse(); }
							).WithDescription("For when you're too weak to carry things yourself."),
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