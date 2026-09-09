using System;
using System.Collections;
using Server.Targeting;

namespace Server.Spells.DeathKnight
{
	public class OrbOfOrcusSpell : DeathKnightSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 757,
				IconGraphic = 0x1B,
				Name = "Orb of Orcus",
				PowerWords = "Orcus Arma",
				Description = "The forces of Orcus surround the knight and reflects a certain amount of magical effects back at the caster.",
				ManaCost = 56,
				TithingCost = 200,
				MinSkill = 80,
				TargetType = TargetFlags.None
			},
			218,
			9031
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public OrbOfOrcusSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override bool CheckCast()
		{
			DefensiveSpell.EndDefense( Caster );

			if ( !base.CheckCast() )
				return false;

			if ( Caster.MagicDamageAbsorb > 0 )
			{
				Caster.SendLocalizedMessage( 1005559 ); // This spell is already in effect.
				return false;
			}

			return true;
		}

		private static Hashtable m_Table = new Hashtable();

		public override void OnCast()
		{
			DefensiveSpell.EndDefense( Caster );

			if ( Caster.MagicDamageAbsorb > 0 )
			{
				Caster.SendLocalizedMessage( 1005559 ); // This spell is already in effect.
			}
			else if ( CheckSequence() )
			{
				int value = (int)( GetKarmaPower( Caster ) / 4 );

				Caster.MagicDamageAbsorb = value;
				Fifth.MagicReflectSpell.AddReflect( Caster );

				Caster.FixedParticles( 0x375A, 10, 15, 5037, EffectLayer.Waist );
				Caster.PlaySound( 0x1E9 );

				BuffInfo.RemoveBuff( Caster, BuffIcon.OrbOfOrcus );
				BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.OrbOfOrcus, 1063551 ) );

				DrainSoulsInLantern( Caster, RequiredTithing );
			}

			FinishSequence();
		}
	}
}