using System;
using System.Collections;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.DeathKnight
{
	public class GrimReaperSpell : DeathKnightSpell
	{

		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 753,
				IconGraphic = 0x402,
				Name = "Grim Reaper",
				PowerWords = "Astaroth Mortem",
				Description = "The death knight's target is marked by the grim reaper. All damage dealt to it is increased, but the death knight takes extra damage from other kinds of creatures.",
				ManaCost = 28,
				TithingCost = 42,
				MinSkill = 30,
				TargetType = TargetFlags.Harmful
			},
			-1,
			9002
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 0.5 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override bool BlocksMovement{ get{ return false; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public GrimReaperSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
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

					BuffInfo.AddBuff ( Caster, new BuffInfo ( BuffIcon.GrimReaper, 1063546, 1063547, TimeSpan.FromMinutes ( delay ), Caster ) );
				}
				DrainSoulsInLantern( Caster, RequiredTithing );
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
