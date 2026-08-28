using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Avatar
{
	public partial class RewardFactory
	{
		public static List<IReward> CreateTemplateRewards(PlayerMobile from, PlayerContext context)
		{
			Action<Func<PlayerMobile, bool>> applyTemplate = action =>
			{
				if (from.NetState == null) return;

				var confirmation = new ConfirmationGump(
					from,
								AvatarLocalization.Key(from, "avatar.confirm.select_template_title", "Select Template?"),
					string.Format("Are you sure you wish to select this template? This is a {0} that will recreate your backpack, reduce existing stats, and change skills.", TextDefinition.GetColorizedText("destructive action", HtmlColors.RED)),
					() =>
					{
						SkillCheck.DisableSkillGains = true;

						// Auto-Lock Focus and Meditation to prevent them from naturally raising
						from.Skills.Focus.SetLockNoRelay(SkillLock.Locked);
						from.Skills.Meditation.SetLockNoRelay(SkillLock.Locked);

						// Reduce all skills to 0
						for (var i = 0; i < from.Skills.Length; i++)
						{
							var skill = from.Skills[i];
							if (0 < skill.Base)
								skill.Base = 0;
						}

						var boosted = action(from);

						// Boost skills if necessary
						for (var i = 0; i < from.Skills.Length; i++)
						{
							Skill skill = from.Skills[i];
							if (skill == null) continue;

							if (0 < skill.Value)
							{
								if (boosted)
									skill.BaseFixedPoint += 100; // +10 to each skill that was set
							}
						}

						SkillCheck.DisableSkillGains = false;

						AvatarEngine.Instance.ApplyContext(from, from.Avatar);
						from.OnSkillsQuery(from);
						from.SendMessage("Your skills have been set to the chosen template. Focus and Meditation have been set to Locked.");
					}
				);
				from.SendGump(confirmation);
			};

			var rewards = new List<IReward>
			{
				ActionReward.Create(
					AvatarShopGump.COST_FREE,
					AvatarShopGump.NO_ITEM_ID,
								AvatarLocalization.Key(from, "avatar.template.brute", "The Brute"),
								AvatarLocalization.Key(from, "avatar.template.brute.desc", "Starts with 60 strength, 10 dexterity, and 10 intelligence."),
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(60, 10, 10);
								context.SelectedTemplate = AvatarStarterTemplates.Brute;
								return false;
							}
						);
					}
				).AsStatic(context.DraftModeEnabled),
				ActionReward.Create(
					AvatarShopGump.COST_FREE,
					AvatarShopGump.NO_ITEM_ID,
								AvatarLocalization.Key(from, "avatar.template.acrobat", "The Acrobat"),
								AvatarLocalization.Key(from, "avatar.template.acrobat.desc", "Starts with 10 strength, 60 dexterity, and 10 intelligence."),
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(10, 60, 10);
								context.SelectedTemplate = AvatarStarterTemplates.Acrobat;
								return false;
							}
						);
					}
				).AsStatic(context.DraftModeEnabled),
				ActionReward.Create(
					AvatarShopGump.COST_FREE,
					AvatarShopGump.NO_ITEM_ID,
								AvatarLocalization.Key(from, "avatar.template.scholar", "The Scholar"),
								AvatarLocalization.Key(from, "avatar.template.scholar.desc", "Starts with 10 strength, 10 dexterity, and 60 intelligence."),
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(10, 10, 60);
								context.SelectedTemplate = AvatarStarterTemplates.Scholar;

								return false;
							}
						);
					}
				).AsStatic(context.DraftModeEnabled),
				ActionReward.Create(
					AvatarShopGump.COST_FREE,
					AvatarShopGump.NO_ITEM_ID,
					"The Well-Rounded",
					"Starts with 40 strength, 20 dexterity, and 20 intelligence.",
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(40, 20, 20);
								context.SelectedTemplate = AvatarStarterTemplates.WellRoundedStats;

								return false;
							}
						);
					}
				).AsStatic(context.DraftModeEnabled),
			};

			if (context.UnlockTemplateJester)
			{
				rewards.Add(ActionReward.Create(
					AvatarShopGump.COST_FREE,
					ITEM_ID_JESTER,
					"The Jester",
					context.DraftModeEnabled
						? "Start with a Bag of Tricks."
						: "Start with a Bag of Tricks and learn the skills of a Jester from your Skill Archive.",
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(20, 20, 20);
								context.SelectedTemplate = AvatarStarterTemplates.Jester;
								return false;
							}
						);
					}
				));
			}

			if (context.UnlockTemplateMystic)
			{
				rewards.Add(ActionReward.Create(
					AvatarShopGump.COST_FREE,
					ITEM_ID_MYSTIC,
					"The Mystic",
					context.DraftModeEnabled
						? "Start with a Monk's Tome."
						: "Start with a Monk's Tome and learn the skills of a Mystic from your Skill Archive.",
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(20, 20, 20);
								context.SelectedTemplate = AvatarStarterTemplates.Mystic;
								return false;
							}
						);
					}
				));
			}

			if (context.UnlockTemplateShinobi)
			{
				rewards.Add(ActionReward.Create(
					AvatarShopGump.COST_FREE,
					ITEM_ID_SHINOB,
					"The Shinobi",
					context.DraftModeEnabled
						? "Start with a Shinobi Scroll."
						: "Start with a Shinobi Scroll and learn the skills of a Shinobi from your Skill Archive.",
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(20, 20, 20);
								context.SelectedTemplate = AvatarStarterTemplates.Shinobi;
								return false;
							}
						);
					}
				));
			}

			if (context.UnlockTemplateDeathKnight)
			{
				rewards.Add(ActionReward.Create(
					AvatarShopGump.COST_FREE,
					ITEM_ID_DEATH_KNIGHT,
					"The Death Knight",
					context.DraftModeEnabled
						? "Start with a Death Knight book."
						: "Start with a Death Knight book and learn the skills of a Death Knight from your Skill Archive.",
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(20, 20, 20);
								context.SelectedTemplate = AvatarStarterTemplates.DeathKnight;
								return false;
							}
						);
					}
				));
			}

			if (context.UnlockTemplateHolyMan)
			{
				rewards.Add(ActionReward.Create(
					AvatarShopGump.COST_FREE,
					ITEM_ID_HOLY_MAN,
					"The Holy Man",
					context.DraftModeEnabled
						? "Start with a Holy Man book."
						: "Start with a Holy Man book and learn the skills of a Holy Man from your Skill Archive.",
					() =>
					{
						applyTemplate(
							player =>
							{
								from.InitStats(20, 20, 20);
								context.SelectedTemplate = AvatarStarterTemplates.HolyMan;
								return false;
							}
						);
					}
				));
			}

			var templates = new List<AvatarStarterTemplates>
			{
				// StarterProfessions.Custom,
				AvatarStarterTemplates.Ninja,
				AvatarStarterTemplates.Bard,
				AvatarStarterTemplates.Druid,
				AvatarStarterTemplates.Knight,
				AvatarStarterTemplates.Warrior,
				AvatarStarterTemplates.Mage,
				AvatarStarterTemplates.Archer,
			};
			if (context.DraftModeEnabled) templates.Clear();

			HashSet<AvatarStarterTemplates> boostedTemplates = context.BoostedTemplateCache;
			if (boostedTemplates == null)
			{
				context.BoostedTemplateCache = boostedTemplates = new HashSet<AvatarStarterTemplates>();
			}

			if (0 < context.ImprovedTemplateCount && context.ImprovedTemplateCount <= templates.Count)
			{
				// Keep boosting a random profession until we reach our max
				while (boostedTemplates.Count != context.ImprovedTemplateCount)
				{
					boostedTemplates.Add(Utility.Random(templates));
				}
			}

			foreach (var template in templates.OrderBy(p => p.ToString()))
			{
				var boosted = 0 < boostedTemplates.Count && boostedTemplates.Contains(template);
				rewards.Add(ActionReward.Create(
					AvatarShopGump.COST_FREE,
					AvatarShopGump.NO_ITEM_ID,
					AvatarLocalization.KeyFormat(from, "avatar.template.profession.name", "The {0}{1}", template.ToString(), boosted ? AvatarLocalization.Key(from, "avatar.term.improved_suffix", " (Improved)") : string.Empty),
					AvatarLocalization.KeyFormat(from, "avatar.template.profession.desc", "Start with the stats, skills, and items of a {0}.", template.ToString()),
					() =>
					{
						applyTemplate(
							player =>
							{
								CharacterCreation.SetTemplateSkills(player, (StarterProfessions)template);
								context.SelectedTemplate = template;
								return boosted;
							}
						);
					}
				));
			}

			return rewards;
		}
	}
}