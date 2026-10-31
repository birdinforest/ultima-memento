using System;
using Server.Targeting;

namespace Server.Spells.DeathKnight
{
	public class StrikeSpell : DeathKnightSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 761,
				IconGraphic = 0x12,
				Name = "Strike",
				PowerWords = "Naberius Impetus",
				Description = "The death knight unleashes the forces of hell unto his nearby enemies, causing much damage.",
				ManaCost = 12,
				TithingCost = 14,
				MinSkill = 10,
				TargetType = TargetFlags.Harmful
			},
			230,
			9022
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public StrikeSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
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
				m.FixedParticles( 0x36BD, 20, 10, 5044, EffectLayer.Head );
				m.PlaySound( 0x307 );

				SpellHelper.Turn( Caster, m );

				double damage = GetKarmaPower( Caster ) / 2;

				SpellHelper.Damage( TimeSpan.Zero, m, Caster, damage, 0, 0, 0, 0, 100 );
				DrainSoulsInLantern( Caster, RequiredTithing );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private StrikeSpell m_Owner;

			public InternalTarget( StrikeSpell owner ) : base( 12, false, TargetFlags.Harmful )
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
