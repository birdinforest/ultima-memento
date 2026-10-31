using System;
using Server.Targeting;

namespace Server.Spells.HolyMan
{
	public class PurgeSpell : HolyManSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 776,
				IconGraphic = 0x96B,
				Name = "Purge",
				PowerWords = "Deiectionem",
				Description = "Removes curses and other ailing effects.",
				ManaCost = 20,
				TithingCost = 80,
				MinSkill = 40,
				TargetType = TargetFlags.Beneficial
			},
			266,
			9040
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public PurgeSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
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
			else if ( CheckBSequence( m, false ) )
			{
				SpellHelper.Turn( Caster, m );

				m.PlaySound( 0xF6 );
				m.PlaySound( 0x1F7 );
				m.FixedParticles( 0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head );

				IEntity from = new Entity( Serial.Zero, new Point3D( m.X, m.Y, m.Z - 10 ), Caster.Map );
				IEntity to = new Entity( Serial.Zero, new Point3D( m.X, m.Y, m.Z + 50 ), Caster.Map );
				Effects.SendMovingParticles( from, to, 0x2255, 1, 0, false, false, 13, 3, 9501, 1, 0, EffectLayer.Head, 0x100 );

				Server.Spells.Chivalry.RemoveCurseSpell.RemoveBadThings( m );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private PurgeSpell m_Owner;

			public InternalTarget( PurgeSpell owner ) : base( 12, false, TargetFlags.Beneficial )
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
