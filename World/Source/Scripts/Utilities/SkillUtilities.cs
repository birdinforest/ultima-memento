using System;

namespace Server.Utilities
{
	public static class SkillUtilities
	{
		public static bool DoSkillChecks(Mobile from, SkillName skill, int count)
		{
			return DoSkillChecks(from, skill, count, count);
		}

		public static bool DoSkillChecks(Mobile from, SkillName skill, int count, int maxCount)
		{
			return DoSkillChecks(from, skill, 0, 125, count, maxCount);
		}

		public static bool DoSkillChecks(Mobile from, SkillName skill, int minSkill, int maxSkill, int count, int maxCount)
		{
			if (from == null) return false;

			bool success = false;
			for (int i = 0; i < Math.Max(0, Math.Min(count, maxCount)); i++)
				success = from.CheckSkill(skill, minSkill, maxSkill) || success;

			return success;
		}
	}
}