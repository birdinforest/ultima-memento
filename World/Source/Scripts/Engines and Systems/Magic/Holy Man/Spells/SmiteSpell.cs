using System;
using Server.Targeting;
using Server.Items;

namespace Server.Spells.HolyMan
{
	public class SmiteSpell : HolyManSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 781,
				IconGraphic = 0x970,
				Name = "Smite",
				PowerWords = "Percutiat",
				Description = "Calls down a bolt from the heavens, doing double damage to demons and undead.",
				ManaCost = 20,
				TithingCost = 80,
				MinSkill = 40,
				TargetType = TargetFlags.Harmful
			},
			266,
			9040
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public SmiteSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			SlayerEntry holyundead = SlayerGroup.GetEntryByName( SlayerName.Silver );
			SlayerEntry holydemons = SlayerGroup.GetEntryByName( SlayerName.Exorcism );

			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckHSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );

				Mobile source = Caster;
				bool hitThrough = SpellHelper.ResolveMagicDefense( (int)SpellCircle.Fourth, ref source, ref m );

				m.BoltEffect( 0 );

				if ( hitThrough )
				{
					int nBenefit = (int)( (Caster.Skills[SkillName.Healing].Value / 10) + (Caster.Skills[SkillName.Spiritualism].Value / 10) );

					if ( holyundead.Slays(m) || holydemons.Slays(m) )
						nBenefit = nBenefit * 2;

					double damage = GetNewAosDamage( 23, 1, 4, m ) + nBenefit;

					SpellHelper.Damage( this, m, damage, 0, 0, 0, 0, 100 );
				}
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private SmiteSpell m_Owner;

			public InternalTarget( SmiteSpell owner ) : base( 12, false, TargetFlags.Harmful )
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
