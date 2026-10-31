using Server.Mobiles;
using System;

namespace Server.Engines.Avatar
{
	public class CoinRewardCalculator
	{
		public static int GetKillCoinValue(BaseCreature creature)
		{
			var value = GetHitsBase(creature);
			if (value < 1) return 0;

			value += GetDefenseBase(creature);

			value += Misc.IntelligentAction.GetCreatureLevel(creature);

			value = ApplyArchetypeMultiplier(value, creature);
			value = ApplyBreathMultiplier(value, creature);

			if (creature.HitPoison != null) value = ApplyMultiplier(value, 115);
			if (creature.IsParagon) value = ApplyMultiplier(value, 150);

			// Passive creature rewards are notably penalized
			if (creature.FightMode == FightMode.Aggressor) value = ApplyMultiplier(value, 25);

			return 0 < value ? value : 0;
		}

		private static int ApplyArchetypeMultiplier(int value, BaseCreature creature)
		{
			var multiplier = 0;

			var accuracySkill = Math.Max(
				(int)creature.Skills[SkillName.FistFighting].Value,
				(int)creature.Skills[SkillName.Marksmanship].Value
			);
			if (30 < accuracySkill)
			{
				if (creature.Weapon != null)
				{
					int minDamage;
					int maxDamage;
					creature.Weapon.GetStatusDamage(creature, out minDamage, out maxDamage);

					var damageMinScalar = (int)(0.3 * minDamage);
					if (0 < damageMinScalar) multiplier += damageMinScalar * 5;

					var damageMaxScalar = (int)(0.7 * maxDamage);
					if (0 < damageMaxScalar) multiplier += damageMaxScalar * 5;
				}
				else
				{
					// No weapon, this is our best guess
					var damageMinScalar = creature.DamageMin / 10;
					if (0 < damageMinScalar) multiplier += damageMinScalar * 10;

					var damageMaxScalar = creature.DamageMax / 10;
					if (0 < damageMaxScalar) multiplier += damageMaxScalar * 10;

					var anatomySkill = (int)creature.Skills[SkillName.Anatomy].Value;
					if (100 < anatomySkill) multiplier += Math.Min(25, anatomySkill - 100);
					if (50 < anatomySkill) multiplier += 25;
					if (30 < anatomySkill) multiplier += 10;

					var tacticsSkill = (int)creature.Skills[SkillName.Tactics].Value;
					if (100 < tacticsSkill) multiplier += Math.Min(25, tacticsSkill - 100);
					if (50 < tacticsSkill) multiplier += 25;
					if (30 < tacticsSkill) multiplier += 10;

					if (0 < multiplier)
					{
						var strScalar = creature.RawStr / 200;
						if (0 < strScalar) multiplier += strScalar * 10;
					}
				}
			}

			var magerySkill = (int)creature.Skills[SkillName.Magery].Value;
			var necromancySkill = (int)creature.Skills[SkillName.Necromancy].Value;
			if (30 < magerySkill || 30 < necromancySkill)
			{
				if (100 < magerySkill) multiplier += Math.Min(25, magerySkill - 100);
				if (80 < magerySkill) multiplier += 25;
				if (50 < magerySkill) multiplier += 25;
				if (30 < magerySkill) multiplier += 10;

				if (0 < magerySkill)
				{
					var psychologySkill = (int)creature.Skills[SkillName.Psychology].Value;
					if (100 < psychologySkill) multiplier += Math.Min(25, psychologySkill - 100);
					if (80 < psychologySkill) multiplier += 25;
					if (50 < psychologySkill) multiplier += 25;
					if (30 < psychologySkill) multiplier += 10;
				}

				if (80 < necromancySkill) multiplier += Math.Min(25, necromancySkill - 80);
				if (50 < necromancySkill) multiplier += 25;
				if (30 < necromancySkill) multiplier += 10;

				// Int only matters if the mob can actually cast
				if (0 < multiplier)
				{
					var intScalar = creature.RawInt / 100;
					if (0 < intScalar) multiplier += intScalar * 20;
				}
			}

			return 0 < multiplier ? ApplyMultiplier(value, 100 + multiplier) : value;
		}

		private static int ApplyBreathMultiplier(int value, BaseCreature creature)
		{
			if (!creature.HasBreath) return value;

			value = ApplyMultiplier(value, GetBreathFormMultiplier(creature));

			if (creature.BreathDamageScalar >= 0.60) return ApplyMultiplier(value, 135);
			if (creature.BreathDamageScalar >= 0.40) return ApplyMultiplier(value, 120);
			if (creature.BreathDamageScalar > 0.20) return ApplyMultiplier(value, 110);

			return value;
		}

		private static int ApplyMultiplier(int value, int multiplier)
		{
			return value * multiplier / 100;
		}

		private static int GetBreathElementMultiplier(BaseCreature creature)
		{
			var highest = Math.Max(
				Math.Max(creature.BreathPhysicalDamage, creature.BreathFireDamage),
				Math.Max(
					creature.BreathColdDamage,
					Math.Max(creature.BreathPoisonDamage, creature.BreathEnergyDamage)
				)
			);

			if (highest == creature.BreathPoisonDamage) return 130;
			if (highest == creature.BreathPhysicalDamage) return 125;
			// TODO: Drains??
			// TODO: No special behavior should be LESS value

			return 135;
		}

		private static int GetBreathFormMultiplier(BaseCreature creature)
		{
			switch (creature.BreathAttackForm)
			{
				case 2:
				case 3:
				case 5:
					return Constants.KILL_COIN_BREATH_TRICK;

				case 4:
				case 6:
				case 7:
				case 15:
					return Constants.KILL_COIN_BREATH_SPECIAL;

				case 14:
				case 29:
				case 30:
				case 31:
				case 32:
				case 33:
				case 37:
				case 41:
				case 42:
				case 43:
				case 44:
				case 45:
					return Constants.KILL_COIN_BREATH_AREA;

				case 1:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 16:
				case 23:
				case 24:
				case 25:
				case 34:
				case 36:
				case 46:
				case 47:
				case 50:
					return Constants.KILL_COIN_BREATH_LARGE;

				case 17:
				case 18:
				case 19:
				case 20:
				case 21:
				case 22:
				case 26:
				case 27:
				case 28:
				case 35:
				case 38:
				case 39:
				case 40:
				case 48:
				case 49:
				case 51:
				case 52:
					return Constants.KILL_COIN_BREATH_SMALL;

				default:
					return GetBreathElementMultiplier(creature);
			}
		}

		private static int GetDefenseBase(BaseCreature creature)
		{
			var lowestResistance = Math.Min(
				creature.PhysicalResistance,
				Math.Min(
					Math.Min(creature.ColdResistance, creature.FireResistance),
					Math.Min(creature.PoisonResistance, creature.EnergyResistance)
				)
			);
			if (lowestResistance < 20) return 0;

			return lowestResistance / 3;
		}

		private static int GetHitsBase(BaseCreature creature)
		{
			var hits = creature.HitsMax;
			if (hits < 1) return 0;
			if (hits < 25) return 5;
			if (hits < 150) return 15;
			if (hits < 300) return 20;
			if (hits < 500) return 30;
			if (hits < 800) return 60;

			return 120;
		}
	}
}