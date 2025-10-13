using System;
using Server;
using System.Collections;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Mobiles;

namespace Server.Spells.Shinobi
{
	public class CheetahPaws : ShinobiSpell
	{
		public override int spellIndex { get { return 290; } }
		private static SpellInfo m_Info = new SpellInfo(
				"Cheetah Paws", "Chita no ashi",
				-1,
				0
			);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3.0 ); } }
		public override double RequiredSkill{ get{ return (double)(Int32.Parse(  Server.Items.ShinobiScroll.ShinobiInfo( spellIndex, "skill" ))); } }
		public override int RequiredTithing{ get{ return Int32.Parse(  Server.Items.ShinobiScroll.ShinobiInfo( spellIndex, "points" )); } }
		public override int RequiredMana{ get{ return Int32.Parse(  Server.Items.ShinobiScroll.ShinobiInfo( spellIndex, "mana" )); } }

		public CheetahPaws( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

        private readonly static Hashtable m_Timers = new Hashtable();

        public static bool StopTimer(Mobile m)
        {
            Timer t = (Timer)m_Timers[m];

            if (t != null)
            {
                t.Stop();
                m_Timers.Remove(m);
            }

            return (t != null);
        }

		public static bool UnderEffect( Mobile m )
		{
			return m_Timers.Contains( m );
		}

		public static void RemoveEffect( Mobile m )
		{
			if (StopTimer( m ))
			{
				BuffInfo.RemoveBuff( m, BuffIcon.CheetahPaws );
				m.EndAction( typeof( CheetahPaws ) );
				m.PlaySound( 0x64C ); // Cleansing winds
				m.SendMessage("You feel the wind around you dissipate");
				FastPlayer.Refresh(m as PlayerMobile);
			}
		}

		public override void OnCast()
		{
            if ( Caster.Mounted )
            {
                Caster.SendMessage( "You cannot use this ability while on a mount!" );
            }
			else if ( CheckSequence( ) )
			{
				if ( !Caster.CanBeginAction( typeof( CheetahPaws ) ) )
				{
					RemoveEffect( Caster );
				}

				Caster.BeginAction( typeof( CheetahPaws ) );

				int TotalTime = (int)( Caster.Skills[SkillName.Ninjitsu].Value * 5 );
				if ( TotalTime < 600 ){ TotalTime = 600; }

				m_Timers[Caster] = Timer.DelayCall(TimeSpan.FromSeconds( TotalTime ), () => RemoveEffect(Caster));
				FastPlayer.Refresh(Caster as PlayerMobile);

				BuffInfo.RemoveBuff( Caster, BuffIcon.CheetahPaws );
				BuffInfo.AddBuff( Caster, new BuffInfo( BuffIcon.CheetahPaws, 1063490, TimeSpan.FromSeconds( TotalTime ), Caster ) );
				Caster.PlaySound( 0x077 ); // Cougar
			}

            FinishSequence();
		}
	}
}