using System;
using Server.Targeting;

namespace Server.Spells.HolyMan
{
	public class HeavenlyLightSpell : HolyManSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 774,
				IconGraphic = 0x969,
				Name = "Heavenly Light",
				PowerWords = "Caelesti Lumine",
				Description = "Destroys the darkness, allowing for one to see better.",
				ManaCost = 5,
				TithingCost = 20,
				MinSkill = 10,
				TargetType = TargetFlags.None
			},
			266,
			9040
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public HeavenlyLightSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new NightSightTarget( this );
		}

		private class NightSightTarget : Target
		{
			private Spell m_Spell;

			public NightSightTarget( Spell spell ) : base( 12, false, TargetFlags.Beneficial )
			{
				m_Spell = spell;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Mobile && m_Spell.CheckBSequence( (Mobile) targeted ) )
				{
					Mobile targ = (Mobile)targeted;

					SpellHelper.Turn( m_Spell.Caster, targ );

					if ( targ.BeginAction( typeof( LightCycle ) ) )
					{
						new LightCycle.NightSightTimer( targ ).Start();
						int level = (int)( LightCycle.DungeonLevel * ( (Core.AOS ? targ.Skills[SkillName.Healing].Value : from.Skills[SkillName.Healing].Value )/ 100 ) );

						if ( level < 0 )
							level = 0;

						targ.LightLevel = level;

						targ.FixedParticles( 0x376A, 9, 32, 5007, EffectLayer.Waist );
						targ.PlaySound( 0x1E3 );

						BuffInfo.AddBuff( targ, new BuffInfo( BuffIcon.NightSight, 1075643 ) );	//Night Sight/You ignore lighting effects
					}
					else
					{
						from.SendMessage( "{0} already have nightsight.", from == targ ? "You" : "They" );
					}
				}

				m_Spell.FinishSequence();
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Spell.FinishSequence();
			}
		}
	}
}
