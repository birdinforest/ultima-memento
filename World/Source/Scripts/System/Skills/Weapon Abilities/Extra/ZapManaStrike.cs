using System;
using Server;
using Server.Localization;

namespace Server.Items
{
	public class ZapManaStrike : WeaponAbility
	{
		public ZapManaStrike(){}

		public override int BaseMana { get { return 25; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage(StringCatalog.Resolve(attacker.Account, "You have drained their mana!"));
			defender.SendMessage(StringCatalog.Resolve(defender.Account, "You feel you mana drain from the blow!"));

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;

			double skilltouse = GetWeaponSkill(attacker, weapon);
			int todam = (int)(skilltouse / 20);
			defender.Mana -= Math.Min( (defender.Mana / 2), (Utility.RandomMinMax(40, 70) + todam ));
			base.OnHit(attacker, defender, damage);
		}
	}
}