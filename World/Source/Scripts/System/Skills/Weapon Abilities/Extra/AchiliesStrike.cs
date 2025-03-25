using System;
using Server;

namespace Server.Items
{
	public class AchillesStrike : WeaponAbility
	{
		public AchillesStrike(){}

		public override int BaseMana { get { return 20; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You damage their Achilles tendon!");
			defender.SendMessage("Your Achilles tendon was hurt!");

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;

			double skilltouse = GetWeaponSkill(attacker, weapon);
			int todam = (int)(skilltouse / 20);
			WeaponStrikes.AchillesStrike( defender, TimeSpan.FromSeconds( (Utility.RandomDouble() * todam / 2 ) + 3 ) );
			AOS.Damage( defender, attacker, (Utility.RandomMinMax(5, 25) + todam), true, 100, 0, 0, 0, 0 );
			base.OnHit(attacker, defender, damage);
		}
	}
}