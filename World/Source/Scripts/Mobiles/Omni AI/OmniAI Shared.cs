// Created by Peoharen
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
		public DateTime m_NextWeaponSwap;

		public virtual bool m_CanStun
		{
			get { return m_Mobile is BaseVendor; }
		}

		public bool TryToHeal()
		{
			if ( m_Mobile.Summoned ) return false;
			else if ( m_Mobile.HitsMax - 10 < m_Mobile.Hits ) return false;
			else if ( DateTime.Now < m_NextHealTime ) return false;

			int diff = m_Mobile.HitsMax - m_Mobile.Hits;
			diff = ( ( m_Mobile.HitsMax * (100 - diff ) ) / 100 );
			diff = 100 - diff;

			if ( (int)(Utility.RandomDouble() * 100.0) > diff )
				return false;

			Spell spell = null;
			m_NextHealTime = DateTime.Now + TimeSpan.FromSeconds( 20 );

			if ( m_CanUseMagery )
			{
				if ( m_Mobile.Poisoned )
					spell = new CureSpell( m_Mobile, null );

				spell = new GreaterHealSpell( m_Mobile, null );

				if ( spell == null )
					spell = new HealSpell( m_Mobile, null );
			}
			else if ( m_CanUseNecromancy )
			{
				m_Mobile.UseSkill( SkillName.Spiritualism );
				m_NextHealTime = DateTime.Now + TimeSpan.FromSeconds( 10 );
			}
			else if ( m_CanUseChivalry )
			{
				if ( m_Mobile.Poisoned )
					spell = new CleanseByFireSpell( m_Mobile, null );
				else
					spell = new CloseWoundsSpell( m_Mobile, null );
			}
            // else if ( m_CanUseMystic )
            // {
            //     if (m_Mobile.Poisoned)
            //         spell = new CleansingWindsSpell(m_Mobile, null);

            //     spell = new CleansingWindsSpell(m_Mobile, null);
            //     m_NextHealTime = DateTime.Now + TimeSpan.FromSeconds(10);

            //     //if (spell == null)
            //         //spell = new CleansingWindsSpell(m_Mobile, null);
            // }
			else if ( m_Mobile.Skills[SkillName.Healing].Value > 10.0 )
			{
				int delay = (int)( 5.0 + (0.5 * ((120 - m_Mobile.Dex) / 10)) );
				new BandageContext( m_Mobile, m_Mobile, TimeSpan.FromSeconds( delay ) );
				m_NextHealTime = DateTime.Now + TimeSpan.FromSeconds( delay+1 );
				return true;
			}

			if ( spell != null )
				spell.Cast();

			return true;
		}

		public void CheckArmed( bool swap )
		{
			if ( DateTime.Now > m_NextWeaponSwap )
				return;

			if ( !m_SwapWeapons )
				return;

			Container pack = m_Mobile.Backpack;

			if ( pack == null )
			{
				m_Mobile.EquipItem( new Backpack() );
				pack = m_Mobile.Backpack;
			}

			BaseWeapon weapon = m_Mobile.Weapon as BaseWeapon;

			if ( weapon != null )
			{
				if ( !swap )
					return;

				pack.DropItem( weapon );
				weapon = null;
			}

			if ( weapon == null )
			{
				m_Mobile.DebugSay( "Searching my pack for a weapon." );

				Item[] weapons = pack.FindItemsByType( typeof( BaseMeleeWeapon ) );

				if ( weapons != null && weapons.Length != 0 )
				{
					int max = (weapons.Length == 1) ? 0 : (weapons.Length - 1);
					int whichone = Utility.RandomMinMax( 0, max );
					m_Mobile.EquipItem( weapons[whichone] );
				}
			}

			m_NextWeaponSwap = DateTime.Now + TimeSpan.FromSeconds( 15 );
		}

		public void UseWeaponStrike()
		{
			m_Mobile.DebugSay( "Picking a weapon move" );

			BaseWeapon weapon = m_Mobile.FindItemOnLayer( Layer.OneHanded ) as BaseWeapon;

			if ( weapon == null )
				weapon = m_Mobile.FindItemOnLayer( Layer.TwoHanded ) as BaseWeapon;

			if ( weapon == null )
				return;

			int whichone = Utility.RandomMinMax( 1, 2 );

			if ( whichone >= 2 && m_Mobile.Skills[weapon.Skill].Value >= 90.0 )
				WeaponAbility.SetCurrentAbility( m_Mobile, weapon.PrimaryAbility );
			else if ( m_Mobile.Skills[weapon.Skill].Value >= 60.0 )
				WeaponAbility.SetCurrentAbility( m_Mobile, weapon.SecondaryAbility );
			else if ( m_Mobile.Skills[SkillName.FistFighting].Value >= 60.0 && /*weapon == Fist &&*/ m_CanStun && !m_Mobile.StunReady )
				EventSink.InvokeStunRequest( new StunRequestEventArgs( m_Mobile ) );
		}


		public void CheckForFieldSpells()
		{
			if ( !m_IsSmart )
				return;

			bool move = false;

			IPooledEnumerable eable = m_Mobile.Map.GetItemsInRange( m_Mobile.Location, 0 );

			foreach( Item item in eable )
			{
				if ( item == null )
					continue;
				else if ( item.Z != m_Mobile.Z )
					continue;
				else
					move = IsFieldSpell( item.ItemID );
			}
			eable.Free();

			if ( move )
			{
				//TODO, make movement not so random.
				switch( Utility.Random( 9 ) )
				{
					case 0: DoMove( Direction.Up ); break;
					case 1: DoMove( Direction.North ); break;
					case 2: DoMove( Direction.Left ); break;
					case 3: DoMove( Direction.West ); break;
					case 5: DoMove( Direction.Down ); break;
					case 6: DoMove( Direction.South ); break;
					case 7: DoMove( Direction.Right ); break;
					case 8: DoMove( Direction.East ); break;
					default: DoMove( m_Mobile.Direction ); break;
				}
			}
		}

		public static bool IsFieldSpell( int ID )
		{
			if ( ID >= 14612 && ID <= 14633 ) //poison field
				return true;
			else if ( ID >= 14695 && ID <= 14730 ) //paralysis field
				return true;
			else if ( ID >= 14732 && ID <= 14751 ) //fire field
				return true;
			else
				return false;
		}
	}
}