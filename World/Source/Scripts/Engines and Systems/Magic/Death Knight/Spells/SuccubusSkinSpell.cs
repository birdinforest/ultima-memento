using System;
using System.Collections;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.DeathKnight
{
	public class SuccubusSkinSpell : DeathKnightSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 762,
				IconGraphic = 0x500C,
				Name = "Succubus Skin",
				PowerWords = "Erinyes Carnem",
				Description = "The death knight's target has their skin regenerate health over time.",
				ManaCost = 32,
				TithingCost = 49,
				MinSkill = 35,
				TargetType = TargetFlags.Beneficial
			},
			236,
			9011
		);

		private static Hashtable m_Table = new Hashtable();
        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(3); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
        public override int RequiredMana { get { return SpellInfo.SpellDefinition.ManaCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }

		public SuccubusSkinSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public static bool HasEffect( Mobile m )
		{
			return ( m_Table[m] != null );
		}

		public static void RemoveEffect( Mobile m )
		{
			Timer t = (Timer)m_Table[m];

			if ( t != null )
			{
				t.Stop();
				m_Table.Remove( m );
			} 
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

			if ( m_Table.Contains( m ) )
			{
				Caster.LocalOverheadMessage( MessageType.Regular, 0x481, false, "That target already has this affect." );
			}

			else if ( CheckBSequence( m, false ) )
			{
				SpellHelper.Turn( Caster, m );

				Timer t = new InternalTimer( m, Caster );
				t.Start();
				m_Table[m] = t;
				m.PlaySound( 0x202 );
				m.FixedParticles( 0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist );
				m.SendMessage( "Your skin changes, causing your wounds to heal faster." );
				DrainSoulsInLantern( Caster, RequiredTithing );

				double timer = GetKarmaPower( Caster );

				BuffInfo.RemoveBuff( m, BuffIcon.SuccubusSkin );
				BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.SuccubusSkin, 1063559, TimeSpan.FromSeconds ( timer ), m ) );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private SuccubusSkinSpell m_Owner;

			public InternalTarget( SuccubusSkinSpell owner ) : base( 12, false, TargetFlags.Beneficial )
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

		private class InternalTimer : Timer
		{
			private Mobile dest, source;
			private DateTime NextTick;
			private DateTime Expire;

			public InternalTimer( Mobile m, Mobile from ) : base( TimeSpan.FromSeconds( 0.1 ), TimeSpan.FromSeconds( 0.1 ) )
			{
				dest = m;
				source = from;
				Priority = TimerPriority.FiftyMS;
				double timer = GetKarmaPower( from );
				Expire = DateTime.Now + TimeSpan.FromSeconds( timer );
			}

			protected override void OnTick()
			{
				if ( !dest.CheckAlive() )
				{
					Stop();
					BuffInfo.RemoveBuff( dest, BuffIcon.SuccubusSkin );
					m_Table.Remove( dest );
				}

				if ( DateTime.Now < NextTick )
					return;

				if ( DateTime.Now >= NextTick )
				{
					double heal = MyServerSettings.PlayerLevelMod( Utility.RandomMinMax( 5, 10 ), dest );
					dest.Heal( (int)heal );
					dest.FixedParticles( 0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist );
					NextTick = DateTime.Now + TimeSpan.FromSeconds( 4 );
				}

				if ( DateTime.Now >= Expire )
				{
					Stop();
					BuffInfo.RemoveBuff( dest, BuffIcon.SuccubusSkin );
					if ( m_Table.Contains( dest ) )
						m_Table.Remove( dest );
				}
			}
		}
	}
}
