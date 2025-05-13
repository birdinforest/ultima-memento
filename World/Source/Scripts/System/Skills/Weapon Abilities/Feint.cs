using System;
using System.Collections.Generic;

namespace Server.Items
{
	/// <summary>
	/// Gain a defensive advantage over your primary opponent for a short time.
	/// </summary>
	public class Feint : WeaponAbility
	{
		private static Dictionary<Mobile, FeintTimer> m_Registry = new Dictionary<Mobile, FeintTimer>();
		public static Dictionary<Mobile, FeintTimer> Registry { get { return m_Registry; } }

		public Feint()
		{
		}

		public override int BaseMana { get { return 25; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true))
				return;

			if (Registry.ContainsKey(attacker))
			{
				if (m_Registry[attacker] != null)
					m_Registry[attacker].Stop();

				Registry.Remove(attacker);
			}

			ClearCurrentAbility(attacker);

			attacker.SendLocalizedMessage(1063360); // You baffle your target with a feint!
			defender.SendLocalizedMessage(1063361); // You were deceived by an attacker's feint!

			attacker.FixedParticles(0x3728, 1, 13, 0x7F3, 0x962, 0, EffectLayer.Waist);

			// 100 (41%) // 120 (50%) // 125 (52%)
			double skill = Math.Max(attacker.Skills[SkillName.Ninjitsu].Value, attacker.Skills[SkillName.Bushido].Value);
			int bonus = (int)(20.0 + 3.0 * (skill - 50.0) / 7.0);

			FeintTimer t = new FeintTimer(attacker, defender, bonus);

			t.Start();
			m_Registry[attacker] = t;

			// TODO: Add icon and cliloc
			// string args = String.Format("{0}\t{1}", defender.Name, bonus);
			// BuffInfo.AddBuff(attacker, new BuffInfo(BuffIcon.Feint, 1151308, 1151307, TimeSpan.FromSeconds(6), attacker, args));
		}

		public class FeintTimer : Timer
		{
			public readonly Mobile Owner;
			public readonly Mobile Enemy;
			public readonly int DamageReduction;

			public FeintTimer(Mobile owner, Mobile enemy, int damageReduction)
				: base(TimeSpan.FromSeconds(6.0)) // OSI is 6s
			{
				Owner = owner;
				Enemy = enemy;
				DamageReduction = damageReduction;
				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				Registry.Remove(Owner);
				if (Enemy.Alive)
					Owner.SendMessage("Your opponent recovers their senses.");
			}
		}
	}
}