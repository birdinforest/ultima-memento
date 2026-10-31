using System;
using System.Collections;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.HolyMan
{
	public class SacredBoonSpell : HolyManSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 778,
				IconGraphic = 0x96E,
				Name = "Sacred Boon",
				PowerWords = "Sacrum Munus",
				Description = "Surrounds one with a holy aura that heals wounds much quicker.",
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

		private static Hashtable m_Table = new Hashtable();

		public SacredBoonSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
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
				m.FixedParticles( 0x376A, 1, 62, 9923, 3, 3, EffectLayer.Waist );
				m.FixedParticles( 0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist );
				m.SendMessage( "A holy aura surrounds you causing your wounds to heal faster." );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private SacredBoonSpell m_Owner;

			public InternalTarget( SacredBoonSpell owner ) : base( 12, false, TargetFlags.Beneficial )
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
				Expire = DateTime.Now + TimeSpan.FromSeconds( 30.0 );
				BuffInfo.RemoveBuff( m, BuffIcon.SacredBoon );
				BuffInfo.AddBuff( m, new BuffInfo( BuffIcon.SacredBoon, 1063534, TimeSpan.FromSeconds( 30.0 ), m ) );
			}

			protected override void OnTick()
			{
				if ( !dest.CheckAlive() )
				{
					Stop();
					m_Table.Remove( dest );
				}

				if ( DateTime.Now < NextTick )
					return;

				if ( DateTime.Now >= NextTick )
				{
					double heal = 1 + ( source.Skills[SkillName.Healing].Value / 25.0 ) + ( source.Skills[SkillName.Spiritualism].Value / 25.0 );

					dest.Heal( MyServerSettings.PlayerLevelMod( (int)heal, dest ) );

					dest.PlaySound( 0x202 );
					dest.FixedParticles( 0x376A, 1, 62, 9923, 3, 3, EffectLayer.Waist );
					dest.FixedParticles( 0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist );
					NextTick = DateTime.Now + TimeSpan.FromSeconds( 4 );
				}

				if ( DateTime.Now >= Expire )
				{
					Stop();
					if ( m_Table.Contains( dest ) )
						m_Table.Remove( dest );
				}
			}
		}
	}
}
