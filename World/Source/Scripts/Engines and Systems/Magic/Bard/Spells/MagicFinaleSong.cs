using System;
using System.Collections;
using Server.Mobiles;
using Server.Items;
using Server.Misc;
using Server.Targeting;

namespace Server.Spells.Song
{
	public class MagicFinaleSong : Song
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 362,
				IconGraphic = 0x410,
				Name = "Magic Finale",
				PowerWords = "*plays a magic finale*",
				Description = "An area of effect that dispels all summoned creatures around you.",
				ManaCost = 35,
				MinSkill = 90,
				TargetType = TargetFlags.Harmful
			},
	-1
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.5); } }
		public override double RequiredSkill { get { return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana { get { return SpellInfo.SpellDefinition.ManaCost; } }

		public MagicFinaleSong(Mobile caster, Item scroll) : base(caster, scroll, SpellInfo)
		{
		}

		public override void OnCast()
		{
			base.OnCast();

			bool sings = false;

			if (CheckSequence())
			{
				sings = true;

				ArrayList targets = new ArrayList();

				foreach (Mobile m in Caster.GetMobilesInRange(4))
				{
					if (m is BaseCreature)
					{
						BaseCreature mn = m as BaseCreature;
						if (mn.IsTempEnemy)
							targets.Add(m);
					}

					if (m is BaseCreature && ((BaseCreature)m).Summoned)
						targets.Add(m);
				}

				Caster.FixedParticles(0x3709, 1, 30, 9965, 5, 7, EffectLayer.Waist);

				for (int i = 0; i < targets.Count; ++i)
				{
					Mobile m = (Mobile)targets[i];

					Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);

					m.Delete();
				}
			}

			BardFunctions.UseBardInstrument(BaseInstrument.GetInstrument(Caster), sings, Caster);
			FinishSequence();
		}
	}
}