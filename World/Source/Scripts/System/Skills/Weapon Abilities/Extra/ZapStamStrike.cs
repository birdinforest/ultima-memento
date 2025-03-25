using System;
using Server;

namespace Server.Items
{
	public class ZapStamStrike : WeaponAbility
	{
		public ZapStamStrike(){}

		public override int BaseMana { get { return 25; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You have drained their stamina!");
			defender.SendMessage("You feel exhausted from the blow!");

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;

			double skilltouse = GetWeaponSkill(attacker, weapon);
			int todam = (int)(skilltouse / 20);
			defender.Stam -= Math.Min( (defender.Stam / 2), (Utility.RandomMinMax(40, 70) + todam ));
			base.OnHit(attacker, defender, damage);
		}
	}
}