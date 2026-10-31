using System;
using Server.Mobiles;
using Server.Targeting;
using Server.Localization;

namespace Server.Spells.DeathKnight
{
	public class DevilPactSpell : DeathKnightSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 752,
				IconGraphic = 0x5005,
				Name = "Devil Pact",
				PowerWords = "Deumus Foedus",
				Description = "Summons the devil to battle with the death knight.",
				ManaCost = 60,
				TithingCost = 98,
				MinSkill = 90,
				TargetType = TargetFlags.Beneficial
			},
			269,
			9050,
			false
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 1 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public DevilPactSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override bool CheckCast()
		{
			if ( !base.CheckCast() )
				return false;

			if ( ( Caster.Followers + 4 ) > Caster.FollowersMax )
			{
				Caster.SendLocalizedMessage( 1049645 ); // You have too many followers to summon that creature.
				return false;
			}

			return true;
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( IPoint3D p )
		{
			Map map = Caster.Map;

			SpellHelper.GetSurfaceTop( ref p );

			if ( map == null || !map.CanSpawnMobile( p.X, p.Y, p.Z ) )
			{
				Caster.SendLocalizedMessage( 501942 ); // That location is blocked.
			}
			else if ( SpellHelper.CheckTown( p, Caster ) && CheckSequence() )
			{
				TimeSpan duration;

				int nBenefit = 0;
				if ( Caster is PlayerMobile )
				{
					nBenefit = (int)(Caster.Skills[SkillName.Knightship].Value / 2);
				}

				if ( Core.AOS )
					duration = TimeSpan.FromSeconds( 90.0 + nBenefit );
				else
					duration = TimeSpan.FromSeconds( Utility.Random( 80, 40 ) + nBenefit );

				BaseCreature.Summon( new DevilPact(), false, Caster, new Point3D( p ), 0x212, duration );

				Caster.SendMessage( StringCatalog.Resolve( Caster.Account, "You can double click the summoned to dispel them." ) );
				DrainSoulsInLantern( Caster, RequiredTithing );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private DevilPactSpell m_Owner;

			public InternalTarget( DevilPactSpell owner ) : base( Core.ML ? 10 : 12, true, TargetFlags.None )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is IPoint3D )
					m_Owner.Target( (IPoint3D)o );
			}

			protected override void OnTargetOutOfLOS( Mobile from, object o )
			{
				from.SendLocalizedMessage( 501943 ); // Target cannot be seen. Try again.
				from.Target = new InternalTarget( m_Owner );
				from.Target.BeginTimeout( from, TimeoutTime - DateTime.Now );
				m_Owner = null;
			}

			protected override void OnTargetFinish( Mobile from )
			{
				if ( m_Owner != null )
					m_Owner.FinishSequence();
			}
		}
	}
}
