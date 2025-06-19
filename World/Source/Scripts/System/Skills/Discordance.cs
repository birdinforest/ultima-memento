using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

namespace Server.SkillHandlers
{
	public class Discordance
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Discordance].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			m.RevealingAction();

			BaseInstrument.PickInstrument( m, new InstrumentPickedCallback( OnPickedInstrument ) );

			return TimeSpan.FromSeconds( 1.0 ); // Cannot use another skill for 1 second
		}

		public static void OnPickedInstrument( Mobile from, BaseInstrument instrument )
		{
			from.RevealingAction();
			from.SendLocalizedMessage( 1049541 ); // Choose the target for your song of discordance.
			from.Target = new DiscordanceTarget( from, instrument );
			from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 6.0 );
		}

		private class DiscordanceInfo
		{
			public readonly Mobile m_From;
			public readonly Mobile m_Creature;
			public readonly int m_Effect;
			public readonly ArrayList m_Mods;
			public readonly double m_Scalar;
			public Timer m_Timer;
			public DateTime m_EndTime;

			public DiscordanceInfo( Mobile from, Mobile creature, int effect, ArrayList mods, double scalar )
			{
				m_From = from;
				m_Creature = creature;
				m_Effect = effect;
				m_Mods = mods;
				m_Scalar = scalar;
				m_EndTime = DateTime.Now;

				Apply();
			}

			public void IncreaseDuration( int seconds )
			{
				m_EndTime = m_EndTime.AddSeconds( seconds );
			}

			public void Apply()
			{
				for ( int i = 0; i < m_Mods.Count; ++i )
				{
					object mod = m_Mods[i];

					if ( mod is ResistanceMod )
						m_Creature.AddResistanceMod( (ResistanceMod) mod );
					else if ( mod is StatMod )
						m_Creature.AddStatMod( (StatMod) mod );
					else if ( mod is SkillMod )
						m_Creature.AddSkillMod( (SkillMod) mod );
				}
			}

			public void Clear()
			{
				for ( int i = 0; i < m_Mods.Count; ++i )
				{
					object mod = m_Mods[i];

					if ( mod is ResistanceMod )
						m_Creature.RemoveResistanceMod( (ResistanceMod) mod );
					else if ( mod is StatMod )
						m_Creature.RemoveStatMod( ((StatMod) mod).Name );
					else if ( mod is SkillMod )
						m_Creature.RemoveSkillMod( (SkillMod) mod );

					BuffInfo.RemoveBuff( m_Creature, BuffIcon.Discordance );
				}
			}
		}

		private static readonly Hashtable m_Table = new Hashtable();

		public static bool GetEffect( Mobile targ, ref int effect )
		{
			DiscordanceInfo info = m_Table[targ] as DiscordanceInfo;

			if ( info == null )
				return false;

			effect = info.m_Effect;
			return true;
		}

		private class DiscordanceTimer : Timer
		{
			private readonly DiscordanceInfo m_Info;

			public DiscordanceTimer( DiscordanceInfo info ) : base( TimeSpan.Zero, TimeSpan.FromSeconds( 1 ) )
			{
				m_Info = info;
			}

			protected override void OnTick()
			{
				Mobile from = m_Info.m_From;
				Mobile targ = m_Info.m_Creature;

				bool ends = !targ.Alive 
					|| targ.Deleted 
					|| !from.Alive 
					|| from.Hidden 
					|| from.Map != targ.Map
					|| m_Info.m_EndTime < DateTime.Now;

                // Separate heavier check
                if ( !ends )
				{
					int range = (int) targ.GetDistanceToSqrt( from );
					int maxRange = BaseInstrument.GetBardRange( from, SkillName.Discordance );

					ends = range > maxRange;
				}

				if ( ends )
				{
					Stop();
					m_Info.Clear();
					m_Table.Remove( targ );
					return;
				}

				targ.FixedEffect( 0x376A, 1, 32 );
			}
		}

		public class DiscordanceTarget : Target
		{
			private readonly BaseInstrument m_Instrument;

			public DiscordanceTarget( Mobile from, BaseInstrument inst ) : base( BaseInstrument.GetBardRange( from, SkillName.Discordance ), false, TargetFlags.None )
			{
				m_Instrument = inst;
			}

			protected override void OnTarget( Mobile from, object target )
			{
				from.RevealingAction();
				from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 1.0 ); // Default cooldown

				if ( m_Instrument.Parent != from && !m_Instrument.IsChildOf( from.Backpack ) )
				{
					from.SendLocalizedMessage( 1062488 ); // The instrument you are trying to play is no longer in your backpack!
					return;
				}

				Mobile targ = target as Mobile;
				if ( targ == null || from.Player && targ.Player )
				{
					from.SendLocalizedMessage( 1049535 ); // A song of discord would have no effect on that.
					return;
				}

				if ( targ == from || (targ is BaseCreature && !from.CanBeHarmful( targ, false ) && ((BaseCreature)targ).ControlMaster != from) )
				{
					from.SendLocalizedMessage( 1049535 ); // A song of discord would have no effect on that.
					return;
				}

				double diff = m_Instrument.GetDifficultyFor( targ ) - 10.0;

				double music = from.Skills[SkillName.Musicianship].Value;

				if ( music > 100.0 )
					diff -= (music - 100.0) * 0.5;

				double minSkill = diff - 25;
				double maxSkill = diff + 25;

				if ( from.Skills[SkillName.Discordance].Value < minSkill )
				{
                    // from.SendMessage("You are not skilled enough to disrupt the target.");
					from.SendMessage("You need at least '{0}' Discordance skill to disrupt the target.", minSkill);
					return;
				}

				from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 3.0 ); // Flat cooldown

				if ( !BaseInstrument.CheckMusicianship( from ) )
				{
					from.SendLocalizedMessage( 500612 ); // You play poorly, and there is no effect.
					m_Instrument.PlayInstrumentBadly( from );
					m_Instrument.ConsumeUse( from );
					return;
				}

				// Do skill check early, for gains
				var discordSuccess = from.CheckTargetSkill( SkillName.Discordance, target, minSkill, maxSkill );

				if ( targ.Player && Utility.RandomMinMax( 0, 125 ) <= targ.Skills[SkillName.MagicResist].Value)
				{
					from.SendLocalizedMessage( 1049540 ); // You attempt to disrupt your target, but fail.
					targ.SendMessage( "You magically resist the affects of the song." );
					m_Instrument.PlayInstrumentBadly( from );
					m_Instrument.ConsumeUse( from );
					return;
				}

				if ( discordSuccess )
                {
                    from.SendLocalizedMessage(1049539); // You play the song surpressing your targets strength
                    m_Instrument.PlayInstrumentWell( from );
                    m_Instrument.ConsumeUse( from );
				}
				else
                {
                    from.SendMessage("Your fingers fumble, but you daze the target.");
                    m_Instrument.PlayInstrumentBadly( from );
                    m_Instrument.ConsumeUse( from );
                }
				
				if ( targ.Player )
				{
					targ.SendMessage("You hear jarring music, suppressing your abilities.");
				}

				DiscordanceInfo info = m_Table[targ] as DiscordanceInfo;

				if ( info == null )
				{
					ArrayList mods = new ArrayList();
					int effect;
					double scalar;

					effect = 0 - (int)( from.Skills[SkillName.Discordance].Value / 5.0 );
					if ( BaseInstrument.GetBaseDifficulty( targ ) >= 160.0 ) effect /= 2; // Reduce against difficult mobs

					scalar = effect * 0.01;

					// No longer reduce stats with Discordance changes
					//mods.Add( new StatMod( StatType.Str, "DiscordanceStr", (int)( targ.RawStr * scalar ), TimeSpan.Zero ) );
					//mods.Add( new StatMod( StatType.Int, "DiscordanceInt", (int)( targ.RawInt * scalar ), TimeSpan.Zero ) );
					//mods.Add( new StatMod( StatType.Dex, "DiscordanceDex", (int)( targ.RawDex * scalar ), TimeSpan.Zero ) );

					mods.Add( new ResistanceMod( ResistanceType.Physical, effect ) );
					mods.Add( new ResistanceMod( ResistanceType.Fire, effect ) );
					mods.Add( new ResistanceMod( ResistanceType.Cold, effect ) );
					mods.Add( new ResistanceMod( ResistanceType.Poison, effect ) );
					mods.Add( new ResistanceMod( ResistanceType.Energy, effect ) );

					for ( int i = 0; i < targ.Skills.Length; ++i )
					{
						if ( targ.Skills[i].Value > 0 )
							mods.Add( new DefaultSkillMod( (SkillName)i, true, targ.Skills[i].Value * scalar ) );
					}

					info = new DiscordanceInfo( from, targ, Math.Abs( effect ), mods, scalar );
					info.m_Timer = new DiscordanceTimer( info );

					m_Table[targ] = info;
				}

				info.IncreaseDuration( discordSuccess ? 10 : 4 );
				BuffInfo.RemoveBuff( targ, BuffIcon.Discordance );
				
				string args = String.Format("{0}\t{1}\t{2}\t{3}\t{4}", info.m_Effect, 0, 0, 0, info.m_Scalar);
				BuffInfo.AddBuff( targ, new BuffInfo( BuffIcon.Discordance, 1063662, args.ToString() ));

				if ( !info.m_Timer.Running )
					info.m_Timer.Start();
			}
		}
	}
}