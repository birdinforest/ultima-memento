using System;
using Server;

namespace Server.Items
{
	public class FireStrike : WeaponAbility
	{
		public FireStrike(){}

		public override int BaseMana { get { return 20; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You strike them with burning force!");
			defender.SendMessage("You where struck with burning force!");

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;

			double skilltouse = GetWeaponSkill(attacker, weapon);
			int todam = (int)(skilltouse / 20);
			AOS.Damage( defender, attacker, (Utility.RandomMinMax(15, 35) + todam), true, 0, 100, 0, 0, 0 );
			base.OnHit(attacker, defender, damage);
		}
	}
}