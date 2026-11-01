using System;
using Server.Localization;
using System.Collections;
using Server.Targeting;

namespace Server.Spells.HolyMan
{
	public class TrialByFireSpell : HolyManSpell
	{
		public static readonly SpellInfo SpellInfo = new SpellInfo(
			new SpellDefinition
			{
				SpellID = 783,
				IconGraphic = 0x972,
				Name = "Trial by Fire",
				PowerWords = "Igne Iudicii",
				Description = "Engulfs the priest in holy flames, reflecting magic back at the caster.",
				ManaCost = 15,
				TithingCost = 500,
				MinSkill = 30,
				TargetType = TargetFlags.None
			},
			266,
			9040
		);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3 ); } }
		public override int RequiredTithing{ get{ return SpellInfo.SpellDefinition.TithingCost; } }
		public override double RequiredSkill{ get{ return SpellInfo.SpellDefinition.MinSkill; } }
		public override int RequiredMana{ get{ return SpellInfo.SpellDefinition.ManaCost; } }

		public TrialByFireSpell( Mobile caster, Item scroll ) : base( caster, scroll, SpellInfo )
		{
		}

		public override bool CheckCast()
		{
			DefensiveSpell.EndDefense( Caster );

			if ( !base.CheckCast() )
				return false;

			if ( Caster.MagicDamageAbsorb > 0 )
			{
				Caster.SendMessage( StringCatalog.Resolve( Caster.Account, "You are already under the effects of this prayer." ) );
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
				Caster.SendMessage( StringCatalog.Resolve( Caster.Account, "You are already under the effects of this prayer." ) );
			}
			else if ( CheckSequence() )
			{
				int value = (int)( ( Caster.Skills[SkillName.Healing].Value + Caster.Skills[SkillName.Spiritualism].Value ) / 4 );
				Caster.MagicDamageAbsorb = value;
				Fifth.MagicReflectSpell.AddReflect( Caster );
				Caster.SendMessage( StringCatalog.Resolve( Caster.Account, "Your body is covered by holy flames." ) );
				Caster.FixedParticles( 0x3709, 10, 30, 5052, 0x480, 0, EffectLayer.LeftFoot );
				Caster.PlaySound( 0x208 );
				BuffInfo.RemoveBuff( Caster, BuffIcon.TrialByFire );
				BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.TrialByFire, 1063540 ) );
			}

			FinishSequence();
		}
	}
}
