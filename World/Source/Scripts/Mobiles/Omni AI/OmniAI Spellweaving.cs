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
using Server.Spells.Spellweaving;
using Server.Spells.Mystic;
using Server.Targeting;

namespace Server.Mobiles
{
	public partial class OmniAI : BaseAI
	{
		public void SpellweavingPower()
		{
			CheckFocus();

			// Always attune if needed
			if ( !AttuneWeaponSpell.IsAbsorbing( m_Mobile ) && m_Mobile.Mana > 24 )
			{
				if ( m_Mobile.Debug )
					m_Mobile.Say( 1436, "Casting Attune Weapon" );

				new AttuneWeaponSpell( m_Mobile, null ).Cast();
			}

			if ( m_Mobile.Combatant == null )
				return;

			// 10% chance to try and get help
			if ( Utility.RandomDouble() > 0.89 )
			{
				if ( m_Mobile.Followers < m_Mobile.FollowersMax )
				{
					Spell spell = GetSpellweavingSummon();

					if ( spell != null )
					{
						if ( m_Mobile.Debug )
							m_Mobile.Say( 1436, "Summoning help" );

						spell.Cast();
					}

					if ( m_Mobile.Target != null )
					{
						if ( m_Mobile.Debug )
							m_Mobile.Say( 1436, "Invoking Nature's Fury's target" );

						m_Mobile.Target.Invoke( m_Mobile, m_Mobile.Combatant );
					}

					return;
				}
				else if ( m_Mobile.Combatant is BaseCreature )
				{
					if ( m_Mobile.Debug )
						m_Mobile.Say( 1436, "Casting Dryad Allure" );

					new DryadAllureSpell( m_Mobile, null ).Cast();
				}
			}

			RandomSpellweavingChoice();
			return;
		}

		public void CheckFocus()
		{
			ArcaneFocus focus = ArcanistSpell.FindArcaneFocus( m_Mobile );

			if ( focus != null )
				return;

			if ( m_Mobile.Debug )
				m_Mobile.Say( 1436, "I have no Arcane Focus" );

			BaseCreature bc = null;
			int power = 1;

			foreach( Mobile m in m_Mobile.GetMobilesInRange( 10 ) )
			{
				if ( m == null )
					continue;
				else if ( m == m_Mobile )
					continue;
				else if ( !(m is BaseCreature) )
					continue;

				bc = (BaseCreature)m;

				if ( bc.Skills[SkillName.Spellweaving].Value > 50.0 )
					if ( m_Mobile.Controlled == bc.Controlled && m_Mobile.Summoned == bc.Summoned )
						power++;
			}

			if ( power > 6 )
				power = 6;
			else if ( power < 2 ) // No spellweavers found, setting to min required.
				power = 2;

			ArcaneFocus f = new ArcaneFocus( TimeSpan.FromHours( 1 ), power );

			Container pack = m_Mobile.Backpack;

			if ( pack == null )
			{
				m_Mobile.EquipItem( new Backpack() );
				pack = m_Mobile.Backpack;
			}

			pack.DropItem( f );

			if ( m_Mobile.Debug )
				m_Mobile.Say( 1436, "I created an Arcane Focus, it's level is: " + power.ToString() );
		}

		public Spell GetSpellweavingSummon()
		{
			if ( m_Mobile.Skills[SkillName.Spellweaving].Value > 38.0 )
			{
				if ( m_Mobile.Serial.Value % 2 == 0 )
					return new SummonFeySpell( m_Mobile, null );
				else
					return new SummonFiendSpell( m_Mobile, null );
			}
			else
				return new NatureFurySpell( m_Mobile, null );
		}

		public void RandomSpellweavingChoice()
		{
			// To lazy to sort spells by skill/mana.
			// Let's just pick a random valid action.

			bool end = false;

			for ( int i = 0; i < 5 && !end; i++ )
			{
				switch( Utility.Random( 7 ) )
				{
					case 0: end = Action_ArcaneEmpowerment(); break;
					case 1: end = Action_EssenceOfWind(); break;
					case 2: end = Action_GiftOfRenewal(); break;
					case 3: end = Action_StandardMelee(); break;
					case 4: end = Action_Thunderstorm(); break;
					case 5: end = Action_Wildfire(); break;
					case 6: end = Action_WordOfDeath(); break;
				}
			}
		}

		#region choices
		public bool Action_ArcaneEmpowerment()
		{
			if ( m_Mobile.Skills[SkillName.Spellweaving].Value > 44.0 && m_Mobile.Mana > 50 )
			{
				if ( m_Mobile.Debug )
					m_Mobile.Say( 1436, "Casting Arcane Empowerment" );

				new ArcaneEmpowermentSpell( m_Mobile, null ).Cast();
				return true;
			}

			return false;
		}

		public bool Action_EssenceOfWind()
		{
			if ( m_Mobile.Skills[SkillName.Spellweaving].Value > 62.0 && m_Mobile.Mana > 40 )
			{
				if ( !EssenceOfWindSpell.IsDebuffed( m_Mobile.Combatant ) )
				{
					if ( m_Mobile.Debug )
						m_Mobile.Say( 1436, "Casting Essence Of Wind" );

					new EssenceOfWindSpell( m_Mobile, null ).Cast();
					return true;
				}
			}

			return false;
		}

		public bool Action_GiftOfRenewal()
		{
			if ( m_Mobile.Skills[SkillName.Spellweaving].Value > 20.0 && m_Mobile.Mana > 24 )
			{
				if ( m_Mobile.Debug )
					m_Mobile.Say( 1436, "Casting Gift Of Renewal" );

				new GiftOfRenewalSpell( m_Mobile, null ).Cast();
				return true;
			}

			return false;
		}

		public bool Action_StandardMelee()
		{
			if ( m_Mobile.Skills[SkillName.Tactics].Value > 80.0 )
				return true;

			return false;
		}

		public bool Action_Thunderstorm()
		{
			if ( m_Mobile.Skills[SkillName.Spellweaving].Value > 20.0 && m_Mobile.Mana > 32 )
			{
				if ( m_Mobile.Debug )
					m_Mobile.Say( 1436, "Casting Thunderstorm" );

				new ThunderstormSpell( m_Mobile, null ).Cast();
				return true;
			}

			return false;
		}

		public bool Action_Wildfire()
		{
			if ( m_Mobile.Skills[SkillName.Spellweaving].Value > 66.0 && m_Mobile.Mana > 50 )
			{
				if ( m_Mobile.Debug )
					m_Mobile.Say( 1436, "Casting Wildfire" );

				new WildfireSpell( m_Mobile, null ).Cast();

				if ( m_Mobile.Target != null )
				{
					if ( m_Mobile.Debug )
						m_Mobile.Say( 1436, "Invoking Wildfire's Target." );

					m_Mobile.Target.Invoke( m_Mobile, m_Mobile.Combatant );
				}

				return true;
			}

			return false;
		}

		public bool Action_WordOfDeath()
		{
			if ( m_Mobile.Skills[SkillName.Spellweaving].Value > 90.0 && m_Mobile.Mana > 50 )
			{
				if ( m_Mobile.Debug )
					m_Mobile.Say( 1436, "Casting Word Of Death" );

				new WordOfDeathSpell( m_Mobile, null ).Cast();
				return true;
			}

			return false;
		}
		#endregion

	}
}