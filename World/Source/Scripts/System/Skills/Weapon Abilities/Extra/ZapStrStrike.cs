using System;
using Server;

namespace Server.Items
{
	public class ZapStrStrike : WeaponAbility
	{
		public ZapStrStrike(){}

		public override int BaseMana { get { return 25; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage("You have drained their strength!");
			defender.SendMessage("You feel weaker from the blow!");

			BaseWeapon weapon = attacker.Weapon as BaseWeapon;
			if (weapon == null) return;

			double skilltouse = GetWeaponSkill(attacker, weapon);
			int todam = (int)(skilltouse / 20);
			defender.AddStatMod( new StatMod( StatType.Str, "ZapStr", ((Utility.RandomMinMax(40, 70) + (todam * 2) ) * (-1)), TimeSpan.FromSeconds( (Utility.RandomDouble() * todam ) + 10 ) ) );
			BuffInfo.AddBuff( defender, new BuffInfo( BuffIcon.Weaken, 1063678, TimeSpan.FromSeconds( (Utility.RandomDouble() * todam ) + 10 ), defender, ((Utility.RandomMinMax(40, 70) + (todam * 2) ) * (-1)).ToString() ) );
			base.OnHit(attacker, defender, damage);
		}
	}
}