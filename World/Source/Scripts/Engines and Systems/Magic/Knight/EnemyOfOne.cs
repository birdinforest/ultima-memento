using System;
using System.Collections;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Chivalry
{
	public class EnemyOfOneSpell : PaladinSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
				new SpellDefinition
				{
					SpellID = 205,
					IconGraphic = 0x5105,
					Name = "Enemy of One",
					PowerWords = 1060723, // Forul Solum
					Description = 1061495, // The next target hit becomes the Knight’s Mortal Enemy.  All damage dealt to that exact creature type is increased, but the Knight takes extra damage from all other creature types. Mortal Enemy creature types will highlight Orange to the Knight.  Duration of the spell is affected by the Caster’s Karma.
					ManaCost = 20,
					TithingCost = 10,
					MinSkill = 45,
					TargetType = TargetFlags.None,
				},
				-1,
				9002
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.5 ); } }

		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override int MantraNumber{ get{ return SpellInfo.SpellDefinition.PowerWords.Number; } }
		public override bool BlocksMovement{ get{ return false; } }

		public EnemyOfOneSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override void OnCast()
		{
			if ( CheckSequence() )
			{
				Caster.PlaySound( 0x0F5 );
				Caster.PlaySound( 0x1ED );
				Caster.FixedParticles( 0x375A, 1, 30, 9966, 33, 2, EffectLayer.Head );
				Caster.FixedParticles( 0x37B9, 1, 30, 9502, 43, 3, EffectLayer.Head );

				Timer t = (Timer)m_Table[Caster];

				if ( t != null )
					t.Stop();

				double delay = (double)ComputePowerValue( 1 ) / 60;

				// TODO: Should caps be applied?
				if ( delay < 1.5 )
					delay = 1.5;
				else if ( delay > 3.5 )
					delay = 3.5;

				m_Table[Caster] = Timer.DelayCall( TimeSpan.FromMinutes( delay ), new TimerStateCallback( Expire_Callback ), Caster );

				if ( Caster is PlayerMobile )
				{
					((PlayerMobile)Caster).EnemyOfOneType = null;
					((PlayerMobile)Caster).WaitingForEnemy = true;

					BuffInfo.AddBuff ( Caster, new BuffInfo ( BuffIcon.EnemyOfOne, 1075653, 1044111, TimeSpan.FromMinutes ( delay ), Caster ) );
				}
			}

			FinishSequence();
		}

		private static Hashtable m_Table = new Hashtable();

		private static void Expire_Callback( object state )
		{
			Mobile m = (Mobile)state;

			m_Table.Remove( m );

			m.PlaySound( 0x1F8 );

			if ( m is PlayerMobile )
			{
				((PlayerMobile)m).EnemyOfOneType = null;
				((PlayerMobile)m).WaitingForEnemy = false;
			}
		}
	}
}