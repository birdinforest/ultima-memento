using System;
using System.Collections;
using Server.Targeting;

namespace Server.Spells.Chivalry
{
	public class DivineFurySpell : PaladinSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
				new SpellDefinition
				{
					SpellID = 204,
					IconGraphic = 0x5104,
					Name = "Divine Fury",
					PowerWords = 1060722, // Divinum Furis
					Description = 1061494, // Temporarily increases the caster's swing speed, chance to hit, and damage dealt while lowering the Knight's defense chance. Upon casting, the Knight's Stamina is also refreshed by an amount based on caster's Knightship skill and Karma. Bonus effects and the duration of the spell is also affected by Caster's Karma and Knightship skill.
					ManaCost = 15,
					TithingCost = 10,
					MinSkill = 25,
					TargetType = TargetFlags.None,
				},
				-1,
				9002
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1.0 ); } }

		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override int MantraNumber{ get{ return SpellInfo.SpellDefinition.PowerWords.Number; } }
		public override bool BlocksMovement{ get{ return false; } }

		public DivineFurySpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override void OnCast()
		{
			if ( CheckSequence() )
			{
				Caster.PlaySound( 0x20F );
				Caster.PlaySound( Caster.Female ? 0x338 : 0x44A );
				Caster.FixedParticles( 0x376A, 1, 31, 9961, 1160, 0, EffectLayer.Waist );
				Caster.FixedParticles( 0x37C4, 1, 31, 9502, 43, 2, EffectLayer.Waist );

				Caster.Stam = Caster.StamMax;

				Timer t = (Timer)m_Table[Caster];

				if ( t != null )
					t.Stop();

				int delay = ComputePowerValue( 10 );

				// TODO: Should caps be applied?
				if ( delay < 7 )
					delay = 7;
				else if ( delay > 24 )
					delay = 24;

				m_Table[Caster] = t = Timer.DelayCall( TimeSpan.FromSeconds( delay ), new TimerStateCallback( Expire_Callback ), Caster );
				Caster.Delta( MobileDelta.WeaponDamage );

				BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.DivineFury, 1060589, 1075634, TimeSpan.FromSeconds(delay), Caster));
			}

			FinishSequence();
		}

		private static Hashtable m_Table = new Hashtable();

		public static bool UnderEffect( Mobile m )
		{
			return m_Table.Contains( m );
		}

		private static void Expire_Callback( object state )
		{
			Mobile m = (Mobile)state;

			m_Table.Remove( m );

			m.Delta( MobileDelta.WeaponDamage );
			m.PlaySound( 0xF8 );
		}
	}
}