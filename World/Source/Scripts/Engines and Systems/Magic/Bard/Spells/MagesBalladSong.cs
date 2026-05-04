using Server.Engines.MobileEnhancement;
using Server.Items;
using Server.Misc;
using System;
using Server.Localization;

namespace Server.Spells.Song
{
	public class MagesBalladSong : Song
	{
		private static SpellInfo m_Info = new SpellInfo(
			"Mage's Ballad", "*plays a mage's ballad*",
			-1
			);

		public override bool BlocksMovement { get{ return true; } }
		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(5); } }
		public override double RequiredSkill { get { return 55; } }
		public override int RequiredMana { get { return 15; } }

		public MagesBalladSong(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override void OnCast()
		{
			base.OnCast();

			bool sings = false;

			if (CheckSequence())
			{
				var rawMana = 5 + (MusicSkill(Caster) / 120);
				var tickAmount = MyServerSettings.PlayerLevelMod(rawMana, Caster);
				int rounds = (int)(Caster.Skills[SkillName.Musicianship].Value * .16);
				var tickInterval = TimeSpan.FromSeconds(2);
				var duration = TimeSpan.FromSeconds(tickInterval.TotalSeconds * rounds);

				foreach (var friend in GetNearbyFriends())
				{
					var recipient = new MagesBalladRecipient(friend, friend == Caster, tickAmount, tickInterval, duration);
					Engine.Instance.AddEnhancement(friend, recipient);
				}

				sings = true;
			}

			BardFunctions.UseBardInstrument(BaseInstrument.GetInstrument(Caster), sings, Caster);
			FinishSequence();
		}

		private class MagesBalladRecipient : TimeDependentRecipient<MagesBalladSong>
		{
			private readonly bool m_IsCaster;
			private readonly int m_TickAmount;
			private readonly TimeSpan m_TickInterval;
			private Timer m_Timer;

			public MagesBalladRecipient(Mobile targetMobile, bool isCaster, int tickAmount, TimeSpan tickInterval, TimeSpan duration) : base(targetMobile, duration)
			{
				m_TickAmount = tickAmount;
				m_TickInterval = tickInterval;
				m_IsCaster = isCaster;
			}

			protected override void RemoveInternal()
			{
				if (m_Timer != null)
				{
					m_Timer.Stop();
					m_Timer = null;
				}

				var m = TargetMobile;
				BuffInfo.RemoveBuff(m, BuffIcon.MagesBallad);
				m.SendMessage("The effect of {0} wears off.", m_Info.Name);
			}

			protected override bool TryApplyInternal()
			{
				var m = TargetMobile;
				m.SendMessage( StringCatalog.Resolve( m.Account, "Your mind clears." ) );
				m_Timer = Timer.DelayCall(m_TickInterval, m_TickInterval, () =>
				{
					if (m == null || m.Deleted || !m.Alive || DateTime.Now >= AppliedAt + Duration)
					{
						// Not sure how this can happen, but just in case
						if (m_Timer != null)
						{
							m_Timer.Stop();
						}

						return;
					}

					m.Mana = Math.Min(m.Mana + m_TickAmount, m.ManaMax);

					// Each tick gives the chance to gain musicianship
					if (m_IsCaster) m.CheckSkill(SkillName.Musicianship, 0.5);
				});

				BuffInfo.RemoveBuff(m, BuffIcon.MagesBallad);
				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.MagesBallad, 1063579, 1063580, Duration, m));

				m.FixedParticles(0x376A, 9, 32, 5030, 0x256, 3, EffectLayer.Waist);
				m.PlaySound(0x1F2);

				return true;
			}
		}
	}
}