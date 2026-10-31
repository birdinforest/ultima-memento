using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Avatar
{
	public partial class RewardFactory
	{
		public static List<IReward> CreateSkillBoostRewards(PlayerMobile from, bool isPrimary, PlayerContext context)
		{
			var rewards = new List<IReward>();
			var showPrimarySkills = isPrimary;
			var showSecondarySkills = !isPrimary;

			// Skills
			if (showPrimarySkills || showSecondarySkills)
			{
				var skills = new List<Skill>();
				for (var i = 0; i < from.Skills.Length; i++)
				{
					var skill = from.Skills[i];
					if (skill.SkillName == SkillName.Mysticism) continue;
					if (skill.SkillName == SkillName.Imbuing) continue;
					if (skill.SkillName == SkillName.Throwing) continue;

					if (skill.IsSecondarySkill())
					{
						if (!showSecondarySkills) continue;
					}
					else
					{
						if (!showPrimarySkills) continue;
					}

					skills.Add(skill);
				}

				foreach (var skill in skills)
				{
					const int NEOPHYTE_SKILL_VALUE = 300;
					var archiveValue = context.Skills[skill.SkillName];
					if (!context.UnlockFullSkillArchive && archiveValue < NEOPHYTE_SKILL_VALUE) continue;

					var maxValue = Math.Min(archiveValue / 10f, context.GetRecordedSkillCap());
					var maxValueFixedPoint = (int)(maxValue * 10);
					rewards.Add(
						ActionReward.Create(
							maxValueFixedPoint <= skill.BaseFixedPoint,
							AvatarShopGump.COST_FREE,
							AvatarShopGump.NO_ITEM_ID,
							string.Format("{0}", skill.Name),
										AvatarLocalization.KeyFormat(from, "avatar.skill_boost.desc", "Raise your skill in {0} up to {1:n1}", skill.Name, maxValue),
							() =>
							{
								if (skill.IsSecondarySkill())
								{
									skill.BaseFixedPoint = maxValueFixedPoint;
								}
								else
								{
									var amountToGain = maxValueFixedPoint - skill.BaseFixedPoint;
									var amountAvailable = Math.Max(0, from.SkillsCap - from.SkillsTotal);

									var amountToIncrease = amountToGain;
									if (amountAvailable < amountToIncrease)
									{
										var amountRequired = amountToIncrease - amountAvailable;
										for (int i = 0; i < from.Skills.Length; ++i)
										{
											if (from.Skills[i].Lock != SkillLock.Down)
												continue;

											if (amountRequired >= from.Skills[i].BaseFixedPoint)
											{
												amountRequired -= from.Skills[i].BaseFixedPoint;
												from.Skills[i].Base = 0.0;
											}
											else
											{
												from.Skills[i].BaseFixedPoint -= amountRequired;
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
						).WithPrereq(
							showPrimarySkills ? context.UnlockPrimarySkillBoost : context.UnlockSecondarySkillBoost,
							AvatarLocalization.Key(from, "avatar.reward.jack_of_no_trades.name", "Jack of No Trades")
						)
					);
				}
			}

			return rewards;
		}
	}
}