using System;
using Server;

namespace Server.Items
{
	public class ElementalStrike : WeaponAbility
	{
		public ElementalStrike(){}

		public override int BaseMana { get { return 20; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You strike them with extreme force!");
			defender.SendMessage("You where struck with extreme force!");

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;

			double skilltouse = GetWeaponSkill(attacker, weapon);
			int todam = (int)(skilltouse / 20);
			AOS.Damage( defender, attacker, (Utility.RandomMinMax(15, 35) + todam), true, 0, 25, 25, 25, 25 );
			base.OnHit(attacker, defender, damage);
		}
	}
}