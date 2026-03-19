using System;
using Server.Mobiles;

namespace Server.SkillHandlers
{
	public class Stealth
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Stealth].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			if ( !m.Hidden )
			{
				m.SendLocalizedMessage( 502725 ); // You must hide first
			}
			else if ( m.Skills[SkillName.Hiding].Value < ((Core.ML) ? 30.0 : (Core.SE) ? 50.0 : 80.0) )
			{
				m.SendLocalizedMessage( 502726 ); // You are not hidden well enough.  Become better at hiding.
				m.RevealingAction();
			}
			else
			{
				TryStealth( m, true );
			}

			return TimeSpan.FromSeconds( 2.0 );
		}

		public static bool TryStealth( Mobile m, bool playerActivated )
		{
			int armorRating = Server.Spells.Elementalism.ElementalSpell.ArmorFizzle( m );
			int min = armorRating - 32;
				if ( min < -20 )
					min = -20;
						if ( min > 50 )
							min = 50;
			int max = armorRating + 50;
				if ( max < ( min + 50 ) )
					max = min + 50;
				if ( max > 150 )
					max = 150;

			if ( ( armorRating - (int)(m.Skills[SkillName.Stealth].Value/5) ) > 50 )
			{
				m.SendLocalizedMessage( 502727 ); // You could not hope to move quietly wearing this much armor.

				if ( playerActivated )
					m.RevealingAction();
			}
			else if( m.CheckSkill( SkillName.Stealth, min, max ) )
			{
				int steps = (int)(m.Skills[SkillName.Stealth].Value / 5.0 );

				if( steps < 1 )
					steps = 1;

				m.AllowedStealthSteps = steps;

				PlayerMobile pm = m as PlayerMobile; // IsStealthing should be moved to Server.Mobiles

				if( pm != null )
					pm.IsStealthing = true;

				m.SendLocalizedMessage( 502730 ); // You begin to move quietly.

				return true;
			}
			else
			{
				m.SendLocalizedMessage( 502731 ); // You fail in your attempt to move unnoticed.

				if ( playerActivated )
					m.RevealingAction();
			}

			return false;
		}
	}
}