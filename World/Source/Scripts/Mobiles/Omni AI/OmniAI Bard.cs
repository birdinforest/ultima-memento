//Created by Peoharen
using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Network;
using Server.Misc;
using Server.Mobiles;
using Server.Regions;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fourth;
using Server.Spells.Fifth;
using Server.Spells.Sixth;
using Server.Spells.Seventh;
using Server.Spells.Eighth;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Mystic;
using Server.Targeting;

namespace Server.Mobiles
{
	public partial class OmniAI : BaseAI
	{
		public void BardPower()
		{
			if ( m_Mobile.Debug )
				m_Mobile.SayHued( 1162, "" );

			CheckInstrument();

			switch( Utility.Random( 3 ) )
			{
				case 0: UseDiscord(); break;
				case 1: UseProvocation(); break;
				case 2: UsePeacemaking(); break;
			}

			// TODO Bard Spell support

			return;
		}

		public bool CheckInstrument()
		{
			BaseInstrument inst = BaseInstrument.GetInstrument( m_Mobile );

			if ( inst != null )
				return true;

			if ( m_Mobile.Debug )
				m_Mobile.SayHued( 1162, "I need an instrument, fixing my problem." );

			if ( m_Mobile.Backpack == null )
				return false;

			inst = (BaseInstrument)m_Mobile.Backpack.FindItemByType( typeof( BaseInstrument ) );

			if ( inst == null )
			{
				inst = new Harp();
				inst.SuccessSound = 0x58B;
				inst.FailureSound = 0x58C;

				// Got Better Music?
				// inst.DiscordSound = inst.PeaceSound = 0x58B;
				// inst.ProvocationSound = 0x58A;
			}

			BaseInstrument.SetInstrument( m_Mobile, inst );
			return true;
		}

		#region discord
		public void UseDiscord()
		{
			Mobile target = m_Mobile.Combatant;

			if ( target == null )
				return;

			if ( !m_Mobile.UseSkill( SkillName.Discordance ) )
				return;

			if ( m_Mobile.Debug )
				m_Mobile.SayHued( 1162, "Discording" );

			// Discord's target flag is harmful so the AI already targets it's combatant.
			// However players are immune to Discord hence the following.
			if ( target is PlayerMobile )
			{
				double effect = -(m_Mobile.Skills[SkillName.Discordance].Value / 5.0);
				TimeSpan duration = TimeSpan.FromSeconds( m_Mobile.Skills[SkillName.Discordance].Value * 2 );

				ResistanceMod[] mods = 
				{
					new ResistanceMod( ResistanceType.Physical, (int)(effect * 0.01) ), 
					new ResistanceMod( ResistanceType.Fire, (int)(effect * 0.01) ), 
					new ResistanceMod( ResistanceType.Cold, (int)(effect * 0.01) ), 
					new ResistanceMod( ResistanceType.Poison, (int)(effect * 0.01) ), 
					new ResistanceMod( ResistanceType.Energy, (int)(effect * 0.01) )
				};
		
				// TimedResistanceMod.AddMod( target, "Discordance", mods, duration );
				target.AddStatMod( new StatMod( StatType.Str, "DiscordanceStr", (int)(target.RawStr * effect), duration ) );
				target.AddStatMod( new StatMod( StatType.Int, "DiscordanceInt", (int)(target.RawInt * effect), duration ) );
				target.AddStatMod( new StatMod( StatType.Dex, "DiscordanceDex", (int)(target.RawDex * effect), duration ) );
				new DiscordEffectTimer( target, duration ).Start();
			}
		}

		public class DiscordEffectTimer : Timer
		{
			public Mobile Mob;
			public int Count;
			public int MaxCount;

			public DiscordEffectTimer( Mobile mob, TimeSpan duration ) : base( TimeSpan.FromSeconds( 1.25 ), TimeSpan.FromSeconds( 1.25 ) )
			{
				Mob = mob;
				Count = 0;
				MaxCount = (int)((double)duration.TotalSeconds / 1.25);
			}

			protected override void OnTick()
			{
				if ( Count >= MaxCount )
					Stop();
				else
				{
					Mob.FixedEffect( 0x376A, 1, 32 );
					Count++;
				}
			}
		}
		#endregion


		public bool UseProvocation()
		{
			if ( !m_Mobile.UseSkill( SkillName.Provocation ) )
				return false;
			else if ( m_Mobile.Target != null )
				m_Mobile.Target.Cancel( m_Mobile, TargetCancelType.Canceled );

			Mobile target = m_Mobile.Combatant;

			if ( m_Mobile.Combatant is BaseCreature )
			{
				BaseCreature bc = m_Mobile.Combatant as BaseCreature;
				target = bc.GetMaster();

				if ( target != null && bc.CanBeHarmful( target ) )
				{
					if ( m_Mobile.Debug )
						m_Mobile.SayHued( 1162, "Provocation: Pet to Master" );

					bc.Provoke( m_Mobile, target, true );
					return true;
				}
			}

			List<BaseCreature> list = new List<BaseCreature>();

			foreach( Mobile m in m_Mobile.GetMobilesInRange( 5 ) )
			{
				if ( m != null && m is BaseCreature && m != m_Mobile )
				{
					BaseCreature bc = m as BaseCreature;

					if ( m_Mobile.Controlled != bc.Controlled )
						continue;

					if ( m_Mobile.Summoned != bc.Summoned )
						continue;

					list.Add( bc );
				}
			}

			if ( list.Count == 0 )
				return false;

			for ( int i = 0; i < list.Count; i++ )
			{
				if ( list[i].CanBeHarmful( target ) )
				{
					if ( m_Mobile.Debug )
						m_Mobile.SayHued( 1162, "Provocation: " + list[i].Name + " to " + target.Name );

					list[i].Provoke( m_Mobile, target, true );
					return true;
				}
			}

			return false;
		}

		public void UsePeacemaking()
		{
			if ( !m_Mobile.UseSkill( SkillName.Peacemaking ) )
				return;

			if ( m_Mobile.Combatant is PlayerMobile )
			{
				if ( m_Mobile.Debug )
					m_Mobile.SayHued( 1162, "Peacemaking: Player" );

				PlayerMobile pm = m_Mobile.Combatant as PlayerMobile;

				if ( pm.PeacedUntil <= DateTime.Now )
				{
					pm.PeacedUntil = DateTime.Now + TimeSpan.FromSeconds( (int)(m_Mobile.Skills[SkillName.Peacemaking].Value / 5) );
					pm.SendLocalizedMessage( 500616 ); // You hear lovely music, and forget to continue battling!					
				}
			}
			else if ( m_Mobile.Target != null )
			{
				if ( m_Mobile.Debug )
					m_Mobile.SayHued( 1162, "Peacemaking" );

				m_Mobile.Target.Invoke( m_Mobile, m_Mobile.Combatant );
			}
		}

	}
}