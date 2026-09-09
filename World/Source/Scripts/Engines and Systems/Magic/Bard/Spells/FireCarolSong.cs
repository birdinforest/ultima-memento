using System;
using Server.Mobiles;
using Server.Misc;
using Server.Engines.MobileEnhancement;
using Server.Items;
using Server.Targeting;

namespace Server.Spells.Song
{
	public class FireCarolSong : Song
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 355,
				IconGraphic = 0x408,
				Name = "Fire Carol",
				PowerWords = "*plays a fire carol*",
				Description = "An area of effect that raises the fire resistance of your party.",
				ManaCost = 12,
				MinSkill = 50,
				TargetType = TargetFlags.Beneficial
			},
	-1
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.5); } }
		public override double RequiredSkill { get { return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana { get { return SpellInfo.SpellDefinition.ManaCost; } }

		public FireCarolSong(Mobile caster, Item scroll) : base(caster, scroll, SpellInfo)
		{
		}

		public override void OnCast()
		{
			base.OnCast();

			bool sings = false;

			if (CheckSequence())
			{
				var durationSeconds = 0.24 * MusicSkill(Caster) + 30;
				var duration = TimeSpan.FromSeconds(durationSeconds);

				foreach (var friend in GetNearbyFriends())
				{
					var recipient = new FireCarolRecipient(Caster, friend, duration);
					Engine.Instance.AddEnhancement(friend, recipient);
				}

				sings = true;
			}

			BardFunctions.UseBardInstrument(BaseInstrument.GetInstrument(Caster), sings, Caster);
			FinishSequence();
		}

		private class FireCarolRecipient : TimeDependentRecipient<FireCarolSong>
		{
			private readonly Mobile Caster;
			private ResistanceMod m_Mod;

			public FireCarolRecipient(Mobile caster, Mobile targetMobile, TimeSpan duration) : base(targetMobile, duration)
			{
				Caster = caster;
			}

			protected override void RemoveInternal()
			{
				if (m_Mod == null) return;

				var m = TargetMobile;
				m.RemoveResistanceMod(m_Mod);
				m_Mod = null;

				BuffInfo.RemoveBuff(m, BuffIcon.FireCarol);
				m.SendMessage("The effect of {0} wears off.", SpellInfo.SpellDefinition.Name);
			}

			protected override bool TryApplyInternal()
			{
				var m = TargetMobile;
				var amount = MyServerSettings.PlayerLevelMod(MusicSkill(Caster) / 16, Caster);

				// Clamp creature resistance bonus to player max
				if (m is BaseCreature)
				{
					if ((amount + m.FireResistance) > MySettings.S_MaxResistance)
					{
						amount = MySettings.S_MaxResistance - m.FireResistance;
						if (amount < 1) return false;
					}
				}

				m.SendMessage("Your resistance to fire has increased.");
				m_Mod = new ResistanceMod(ResistanceType.Fire, +amount);
				m.AddResistanceMod(m_Mod);
				m.FixedParticles(0x373A, 10, 15, 5012, 0x21, 3, EffectLayer.Waist);

				string args = String.Format("{0}", amount);
				BuffInfo.RemoveBuff(m, BuffIcon.FireCarol);
				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.FireCarol, 1063569, 1063570, Duration, null, args));

				return true;
			}
		}
	}
}