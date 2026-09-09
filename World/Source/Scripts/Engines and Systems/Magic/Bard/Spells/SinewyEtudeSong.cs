using Server.Engines.MobileEnhancement;
using Server.Misc;
using System;
using Server.Items;
using Server.Targeting;

namespace Server.Spells.Song
{
	public class SinewyEtudeSong : Song
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 366,
				IconGraphic = 0x414,
				Name = "Sinewy Etude",
				PowerWords = "*plays a sinewy etude*",
				Description = "An area of effect that raises the strength of your party.",
				ManaCost = 20,
				MinSkill = 60,
				TargetType = TargetFlags.Beneficial
			},
	-1
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.5); } }
		public override double RequiredSkill { get { return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana { get { return SpellInfo.SpellDefinition.ManaCost; } }

		public SinewyEtudeSong(Mobile caster, Item scroll) : base(caster, scroll, SpellInfo)
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
					var recipient = new SinewyEtudeRecipient(Caster, friend, duration);
					Engine.Instance.AddEnhancement(friend, recipient);
				}

				sings = true;
			}

			BardFunctions.UseBardInstrument(BaseInstrument.GetInstrument(Caster), sings, Caster);
			FinishSequence();
		}

		private class SinewyEtudeRecipient : TimeDependentRecipient<SinewyEtudeSong>
		{
			private const string StatModName = "[Bard] SinewyEtudeSong";
			private readonly Mobile Caster;

			public SinewyEtudeRecipient(Mobile caster, Mobile targetMobile, TimeSpan duration) : base(targetMobile, duration)
			{
				Caster = caster;
			}

			protected override void RemoveInternal()
			{
				var m = TargetMobile;
				m.RemoveStatMod(StatModName);

				BuffInfo.RemoveBuff(m, BuffIcon.SinewyEtude);
				m.SendMessage("The effect of {0} wears off.", SpellInfo.SpellDefinition.Name);
			}

			protected override bool TryApplyInternal()
			{
				var m = TargetMobile;
				int amount = MyServerSettings.PlayerLevelMod(MusicSkill(Caster) / 16, Caster);

				StatMod mod = new StatMod(StatType.Str, StatModName, +amount, TimeSpan.Zero);
				m.AddStatMod(mod);

				m.FixedParticles(0x375A, 10, 15, 5017, 0x224, 3, EffectLayer.Waist);

				string args = String.Format("{0}", amount);
				BuffInfo.RemoveBuff(m, BuffIcon.SinewyEtude);
				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.SinewyEtude, 1063587, 1063588, Duration, null, args));

				return true;
			}
		}
	}
}