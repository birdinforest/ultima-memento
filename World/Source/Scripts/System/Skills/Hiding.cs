using System;
using Server.Network;
using Server.Multis;
using Server.Mobiles;

namespace Server.SkillHandlers
{
	public class Hiding
	{
		private static bool m_CombatOverride;

		public static bool CombatOverride
		{
			get { return m_CombatOverride; }
			set { m_CombatOverride = value; }
		}

		public static void Initialize()
		{
			SkillInfo.Table[21].Callback = new SkillUseCallback(OnUse);
		}

		public static TimeSpan OnUse(Mobile m)
		{
			if (m.Spell != null)
			{
				m.SendLocalizedMessage(501238); // You are busy doing something else and cannot hide.
				return TimeSpan.FromSeconds(1.0);
			}

			if (m.Target != null)
			{
				Targeting.Target.Cancel(m);
			}

			// As long as we're not being overridden... do a skill check, but cap at 100 skill
			var outOfCombatHideSuccess = m_CombatOverride;
			if (!outOfCombatHideSuccess)
			{
				// Guaranteed out of combat success at 100
				outOfCombatHideSuccess = m.CheckSkillExplicit(SkillName.Hiding, 0, 100);

				// Force success if we are Friend+ in a house
				if (!outOfCombatHideSuccess)
				{
					BaseHouse house = BaseHouse.FindHouseAt(m);
					outOfCombatHideSuccess = house != null && house.IsFriend(m);
				}
			}

			// If in combat, do a secondary skill check
			var hidingSkill = m.Skills[SkillName.Hiding].Value;
			var range = 4
				+ Math.Max(0, // At least 4 tiles
					Math.Min(10, // At most 4+10 = 14 tiles
						(int)((100 - hidingSkill) / 10)
					)
				);

			// Check if we're fighting someone
			var hasVisibleCombatant = !m_CombatOverride
				&& m.Combatant != null
				&& m.InRange(m.Combatant.Location, range)
				&& m.Combatant.InLOS(m);

			if (!hasVisibleCombatant && !m_CombatOverride)
			{
				// Check if someone is fighting us
				foreach (Mobile check in m.GetMobilesInRange(range))
				{
					if (check.InLOS(m) && check.Combatant == m)
					{
						hasVisibleCombatant = true;
						break;
					}
				}
			}

			bool success = !hasVisibleCombatant && outOfCombatHideSuccess;

			// If we failed but have enough skill, try again
			if (!success && 100 < hidingSkill)
			{
				var successChance = (hidingSkill - 100) * 0.015;
				if (hidingSkill == 125) successChance += 0.025; // Extra bonus. 37.5% -> 40%

				success = m.CheckSkill(SkillName.Hiding, successChance);
			}

			if (!success)
			{
				m.RevealingAction();

				if (hasVisibleCombatant)
					m.LocalOverheadMessage(MessageType.Regular, 0x22, 501237); // You can't seem to hide right now.
				else
					m.LocalOverheadMessage(MessageType.Regular, 0x22, 501241); // You can't seem to hide here.

				return TimeSpan.FromSeconds(4.0);
			}
			else
			{
				m.Hidden = true;
				m.Warmode = false;

				var showMessage = true;
				if (100 <= hidingSkill) // GM+ Hiding will automatically attempt to Stealth
				{
					if (Stealth.TryStealth(m, false))
					{
						showMessage = false;
					}
				}

				if (showMessage)
					m.LocalOverheadMessage(MessageType.Regular, 0x1F4, 501240); // You have hidden yourself well.

				foreach (Mobile pet in World.Mobiles.Values)
				{
					if (pet is BaseCreature)
					{
						BaseCreature bc = (BaseCreature)pet;
						if (bc.Controlled && bc.ControlMaster == m)
							pet.Hidden = true;
					}
				}

				return TimeSpan.FromSeconds(4.0);
			}
		}
	}
}