using Server.Engines.MobileEnhancement;
using Server.Items;
using Server.Misc;
using System;
using Server.Targeting;

namespace Server.Spells.Song
{
	public class EnchantingEtudeSong : Song
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 352,
				IconGraphic = 0x405,
				Name = "Enchanting Etude",
				PowerWords = "*plays an enchanting etude*",
				Description = "An area of effect that raises the intelligence of your party.",
				ManaCost = 20,
				MinSkill = 60,
				TargetType = TargetFlags.Beneficial
			},
	-1
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.5); } }
		public override double RequiredSkill { get { return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana { get { return SpellInfo.SpellDefinition.ManaCost; } }

		public EnchantingEtudeSong(Mobile caster, Item scroll) : base(caster, scroll, SpellInfo)
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
					var recipient = new EnchantingEtudeRecipient(Caster, friend, duration);
					Engine.Instance.AddEnhancement(friend, recipient);
				}

				sings = true;
			}

			BardFunctions.UseBardInstrument(BaseInstrument.GetInstrument(Caster), sings, Caster);
			FinishSequence();
		}

		private class EnchantingEtudeRecipient : TimeDependentRecipient<EnchantingEtudeSong>
		{
			private const string StatModName = "[Bard] EnchantingEtudeSong";
			private readonly Mobile Caster;

			public EnchantingEtudeRecipient(Mobile caster, Mobile targetMobile, TimeSpan duration) : base(targetMobile, duration)
			{
				Caster = caster;
			}

			protected override void RemoveInternal()
			{
				var m = TargetMobile;
				m.RemoveStatMod(StatModName);

				BuffInfo.RemoveBuff(m, BuffIcon.EnchantingEtude);
				m.SendMessage("The effect of {0} wears off.", SpellInfo.SpellDefinition.Name);
			}

			protected override bool TryApplyInternal()
			{
				var m = TargetMobile;
				int amount = MyServerSettings.PlayerLevelMod(MusicSkill(Caster) / 16, Caster);

				StatMod mod = new StatMod(StatType.Int, StatModName, +amount, TimeSpan.Zero);
				m.AddStatMod(mod);

				m.FixedParticles(0x375A, 10, 15, 5017, 0x1F8, 3, EffectLayer.Waist);

				string args = String.Format("{0}", amount);
				BuffInfo.RemoveBuff(m, BuffIcon.EnchantingEtude);
				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.EnchantingEtude, 1063563, 1063564, Duration, null, args));

				return true;
			}
		}
	}
}