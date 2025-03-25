using System;
using Server;

namespace Server.Items
{
	public class EarthStrike : WeaponAbility
	{
		public EarthStrike(){}

		public override int BaseMana { get { return 20; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You strike them with extreme physical force!");
			defender.SendMessage("You where struck with extreme physical force!");

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;

			double skilltouse = GetWeaponSkill(attacker, weapon);
			int todam = (int)(skilltouse / 20);
			AOS.Damage( defender, attacker, (Utility.RandomMinMax(15, 35) + todam), true, 100, 0, 0, 0, 0 );
			base.OnHit(attacker, defender, damage);
		}
	}
}