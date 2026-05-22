using System;
using Server;
using Server.Localization;

namespace Server.Items
{
	public class MeleeProtection2 : WeaponAbility
	{
		public MeleeProtection2(){}
		public override int BaseMana { get { return 30; } }

		public override bool OnBeforeSwing(Mobile attacker, Mobile defender, bool validate)
		{
			if (validate && (!Validate(attacker) || !CheckMana(attacker, false))) return false;
			else return true;
		}

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!CheckMana(attacker, true)) return;
			ClearCurrentAbility(attacker);
			attacker.SendMessage(StringCatalog.Resolve(attacker.Account, "You feel like you are extremely protected from most weapon attacks!"));
			attacker.MeleeDamageAbsorb = 30;
		}
	}
}