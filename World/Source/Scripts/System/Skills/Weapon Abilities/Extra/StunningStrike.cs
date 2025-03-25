using System;
using Server;

namespace Server.Items
{
	public class StunningStrike : WeaponAbility
	{
		public StunningStrike(){}

		public override int BaseMana { get { return 20; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You have seriously stunned your opponent!");
			defender.SendMessage("You are seriously stunned!");

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;

			double skilltouse = GetWeaponSkill(attacker, weapon);
			int todam = (int)(skilltouse / 20);
			defender.Freeze( TimeSpan.FromSeconds( (Utility.RandomDouble() * todam / 3 ) + 3 ) );
			base.OnHit(attacker, defender, damage);
		}
	}
}