using System;
using Server.Targeting;

namespace Server.Spells.HolyMan
{
	public class TouchOfLifeSpell : HolyManSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 782,
				IconGraphic = 0x971,
				Name = "Touch of Life",
				PowerWords = "Tactus Vitae",
				Description = "Restores health and stamina to the weary.",
				ManaCost = 10,
				TithingCost = 40,
				MinSkill = 20,
				TargetType = TargetFlags.Beneficial
			},
			266,
			9040
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public TouchOfLifeSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckBSequence( m, false ) )
			{
				SpellHelper.Turn( Caster, m );

				int toHeal = 1 + (int)( (Caster.Skills[SkillName.Healing].Value / 10) + (Caster.Skills[SkillName.Spiritualism].Value / 10) );

				toHeal = MyServerSettings.PlayerLevelMod( toHeal, Caster );

				SpellHelper.Heal( toHeal, m, Caster );
				m.Stam = m.Stam + toHeal;

				m.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
				m.PlaySound( 0x202 );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private TouchOfLifeSpell m_Owner;

			public InternalTarget( TouchOfLifeSpell owner ) : base( 12, false, TargetFlags.Beneficial )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
				{
					m_Owner.Target( (Mobile)o );
				}
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
