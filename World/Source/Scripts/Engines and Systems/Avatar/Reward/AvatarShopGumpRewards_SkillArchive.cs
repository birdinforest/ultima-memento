using Server.Gumps;
using Server.Mobiles;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Avatar
{
	public partial class RewardFactory
	{
		public static List<IReward> CreateSkillArchiveRewards(PlayerMobile from, PlayerContext context)
		{
			var skills = new List<Skill>();
			for (var i = 0; i < from.Skills.Length; i++)
			{
				var skill = from.Skills[i];
				if (skill.SkillName == SkillName.Mysticism) continue;
				if (skill.SkillName == SkillName.Imbuing) continue;
				if (skill.SkillName == SkillName.Throwing) continue;

				var archiveValue = context.Skills[skill.SkillName];
				if (archiveValue < 1) continue;

				skills.Add(skill);
			}

			var rewards = new List<IReward>();
			foreach (var skill in skills.OrderBy(s => s.IsSecondarySkill()).ThenBy(s => from.Skills[s.SkillName].Name))
			{
				var value = context.Skills[skill.SkillName] / 10f;

				rewards.Add(
					ActionReward.Create(
						AvatarShopGump.COST_FREE,
						AvatarShopGump.NO_ITEM_ID,
						skill.Name,
						AvatarLocalization.KeyFormat(from, "avatar.skill_archive.entry.desc", "{0} skill. Your highest value was: {1:n1}", !skill.IsSecondarySkill() ? AvatarLocalization.Key(from, "avatar.term.primary", "Primary") : AvatarLocalization.Key(from, "avatar.term.secondary", "Secondary"), value),
						() =>
						{
							var gump = new ConfirmationGump(
								from,
								AvatarLocalization.Key(from, "avatar.skill_archive.drop.title", "Reduce Skill to Zero?"),
								AvatarLocalization.KeyFormat(from, "avatar.skill_archive.drop.body", "Are you sure you want to lower this skill to zero? This is a {0} and cannot be undone.", TextDefinition.GetColorizedText(AvatarLocalization.Key(from, "avatar.term.destructive_action", "destructive action"), HtmlColors.RED)),
								() =>
								{
									from.SendMessage(AvatarLocalization.KeyFormat(from, "avatar.skill_archive.drop.done", "You have reduced '{0}' to zero.", skill.Name));
									from.Skills[skill.SkillName].Base = 0;
								}
							);
							from.SendGump(gump);
						}
					)
				);
			}

			return rewards;
		}
	}
}