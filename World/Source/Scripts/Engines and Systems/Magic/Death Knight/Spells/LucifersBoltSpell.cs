using System;
using Server.Targeting;

namespace Server.Spells.DeathKnight
{
	public class LucifersBoltSpell : DeathKnightSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 756,
				IconGraphic = 0x5DC0,
				Name = "Lucifer's Bolt",
				PowerWords = "Lucifer Fulgur",
				Description = "Calls down a bolt of energy from Lucifer himself, and temporarily stuns the enemy.",
				ManaCost = 24,
				TithingCost = 35,
				MinSkill = 25,
				TargetType = TargetFlags.Harmful
			},
			230,
			9022
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public LucifersBoltSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
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
			else if ( Core.AOS && (m.Frozen || m.Paralyzed || (m.Spell != null && m.Spell.IsCasting)) )
			{
				Caster.SendLocalizedMessage( 1061923 ); // The target is already frozen.
			}
			else if ( CheckHSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				Mobile source = Caster;
				bool hitThrough = SpellHelper.ResolveMagicDefense( 4, ref source, ref m );

				m.FixedEffect( 0x376A, 6, 1 );
				m.BoltEffect( 0 );

				if ( hitThrough )
				{
					double duration = 7.0 + ( GetKarmaPower( m ) * 0.2 );

					m.Paralyze( TimeSpan.FromSeconds( duration ) );
				}

				DrainSoulsInLantern( Caster, RequiredTithing );
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private LucifersBoltSpell m_Owner;

			public InternalTarget( LucifersBoltSpell owner ) : base( 12, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile)o );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
