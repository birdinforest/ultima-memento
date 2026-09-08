using System;
using Server.Targeting;

namespace Server.Spells.HolyMan
{
	public class DampenSpiritSpell : HolyManSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 771,
				IconGraphic = 0x966,
				Name = "Dampen Spirit",
				PowerWords = "Accipe Spiritum",
				Description = "Absorbs mana from others and bestows it to the priest.",
				ManaCost = 35,
				TithingCost = 140,
				MinSkill = 70,
				TargetType = TargetFlags.Harmful
			},
			266,
			9040
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public DampenSpiritSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
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
			else if ( CheckHSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				Mobile source = Caster;
				bool hitThrough = SpellHelper.ResolveMagicDefense( (int)SpellCircle.Seventh, ref source, ref m );

				m.FixedParticles( 0x374A, 10, 15, 5028, EffectLayer.Waist );
				m.PlaySound( 0x1FB );

				if ( hitThrough )
				{
					if ( m.Spell != null )
						m.Spell.OnCasterHurt();

					m.Paralyzed = false;
					BuffInfo.CleanupIcons( m, true );

					int toDrain = 0;

					if ( m.Karma > 0 )
						Caster.SendMessage( "The gods will not smite such a kindly soul." );
					else
					{
						toDrain = (int)(GetDamageSkill( Caster ) - GetResistSkill( m ));

						if ( !m.Player )
							toDrain /= 2;

						if ( toDrain < 0 )
							toDrain = 0;
						else if ( toDrain > m.Mana )
							toDrain = m.Mana;
					}

					if ( toDrain > (Caster.ManaMax - Caster.Mana) )
						toDrain = Caster.ManaMax - Caster.Mana;

					m.Mana -= toDrain;
					Caster.Mana += toDrain;

					HarmfulSpell( m );
				}
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private DampenSpiritSpell m_Owner;

			public InternalTarget( DampenSpiritSpell owner ) : base( Core.ML ? 10 : 12, false, TargetFlags.Harmful )
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