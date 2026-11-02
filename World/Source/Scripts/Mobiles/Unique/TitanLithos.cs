using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Items;
using Server.Misc;
using Server.Engines.PartySystem;
using Server.Localization;

namespace Server.Mobiles
{
	[CorpseName( "a titan corpse" )]
	public class TitanLithos : BaseCreature
	{
		public override int BreathPhysicalDamage{ get{ return 100; } }
		public override int BreathFireDamage{ get{ return 0; } }
		public override int BreathColdDamage{ get{ return 0; } }
		public override int BreathPoisonDamage{ get{ return 0; } }
		public override int BreathEnergyDamage{ get{ return 0; } }
		public override int BreathEffectHue{ get{ return (0xB61-1); } }
		public override int BreathEffectItemID{ get{ return 0; } }
		public override int BreathEffectSound{ get{ return 0x65A; } }
		public override bool ReacquireOnMovement{ get{ return !Controlled; } }
		public override bool HasBreath{ get{ return true; } }
		public override double BreathEffectDelay{ get{ return 0.1; } }
		public override void BreathDealDamage( Mobile target, int form ){ base.BreathDealDamage( target, 41 ); }

		// Tracks cooldowns for special mechanics (not serialized; reset on restart is acceptable).
		private DateTime m_NextStoneShatter;
		private DateTime m_NextRegen;

		[Constructable]
		public TitanLithos () : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "Lithos";
			Title = "the titan of earth";
			Body = 485;
			Hue = 0xB2F;
			BaseSoundID = 609;
			NameHue = 0x22;

			SetStr( 986, 1085 );
			SetDex( 86, 175 );
			SetInt( 586, 675 );

			// HP pool requires sustained team DPS; a solo player cannot outlast the passive regen.
			SetHits( 2000, 2400 );

			// High melee damage demands a dedicated front-line tank (warrior + pet) to absorb hits.
			SetDamage( 36, 48 );

			SetDamageType( ResistanceType.Physical, 100 );

			// Extreme physical resist forces magic DPS (energy is the elemental weakness).
			SetResistance( ResistanceType.Physical, 78, 88 );
			SetResistance( ResistanceType.Cold,     55, 65 );
			SetResistance( ResistanceType.Fire,     55, 65 );
			SetResistance( ResistanceType.Poison,   50, 60 );
			SetResistance( ResistanceType.Energy,   40, 50 );

			SetSkill( SkillName.MagicResist,  120.0, 160.0 );
			SetSkill( SkillName.Tactics,      105.0, 120.0 );
			SetSkill( SkillName.FistFighting, 105.0, 120.0 );

			Fame = 24000;
			Karma = -24000;

			VirtualArmor = 90;

			m_NextStoneShatter = DateTime.Now + TimeSpan.FromSeconds( 12.0 );
			m_NextRegen        = DateTime.Now + TimeSpan.FromSeconds(  5.0 );
		}

		// Stone Skin: 30 % chance to halve incoming melee damage.
		// Solo melee Warriors are barely able to dent Lithos; they must be paired with magic DPS.
		public override void AlterMeleeDamageFrom( Mobile from, ref int damage )
		{
			if ( Utility.RandomMinMax( 1, 10 ) <= 3 )
				damage = (int)( damage * 0.5 );
		}

		public override void OnThink()
		{
			base.OnThink();

			if ( !Alive || Combatant == null )
				return;

			// Passive regen: punishes teams that cannot sustain damage output.
			if ( DateTime.Now >= m_NextRegen && Hits < HitsMax )
			{
				Hits = Math.Min( HitsMax, Hits + 25 );
				m_NextRegen = DateTime.Now + TimeSpan.FromSeconds( 5.0 );
			}

			// Stone Shatter: large AoE physical burst.
			// Without a melee tank and pet soaking front-line hits, the whole team is overwhelmed.
			if ( DateTime.Now >= m_NextStoneShatter )
			{
				DoStoneShatter();
				m_NextStoneShatter = DateTime.Now + TimeSpan.FromSeconds( 22.0 );
			}
		}

		private void DoStoneShatter()
		{
			Say( "Your bones will become part of the earth!" );
			PlaySound( 0x65A );
			FixedParticles( 0x36BD, 20, 10, 5044, EffectLayer.CenterFeet );

			foreach ( Mobile m in GetMobilesInRange( 5 ) )
			{
				if ( m is PlayerMobile && m.Map == Map && m.Alive && !m.Blessed )
				{
					AOS.Damage( m, this, Utility.RandomMinMax( 40, 58 ), 100, 0, 0, 0, 0 );
					m.FixedParticles( 0x3779, 1, 15, 9913, 1153, 7, EffectLayer.Waist );
				}
			}
		}

		public override bool OnBeforeDeath()
		{
			int CanDie = 0;
			int CanKillIt = 0;
			Mobile winner = this;

			foreach ( Mobile m in this.GetMobilesInRange( 30 ) )
			{
				if ( m is PlayerMobile && m.Map == this.Map && !m.Blessed )
				{
					Item obelisk = m.Backpack.FindItemByType( typeof ( ObeliskTip ) );
					if ( obelisk != null )
					{
						ObeliskTip tip = (ObeliskTip)obelisk;
						if ( tip.ObeliskOwner == m && tip.HasEarth > 0 && tip.WonEarth < 1 )
						{
							CanDie = 1;
							winner = m;
							tip.WonEarth = 1;
							m.SendMessage( StringCatalog.ResolveByKey(m.Account, "mob.other.you_absord_the_titan_s_power_into_the_heart_of_earth") );
							m.PlaySound( 0x65A );
							m.FixedParticles( 0x375A, 1, 30, 9966, 33, 2, EffectLayer.Head );
						}
					}
				}
			}
			if ( CanDie == 0 )
			{
				foreach ( Mobile m in this.GetMobilesInRange( 30 ) )
				{
					if ( m is PlayerMobile && m.Map == this.Map && !m.Blessed && ((PlayerMobile)m).IsTitanOfEther )
					{
						CanKillIt = 1;
					}
					if ( m is PlayerMobile && m.Map == this.Map && !m.Blessed ) // ANYONE WITH THE BLACKROCK CAN KILL IT
					{
						Item obelisk = m.Backpack.FindItemByType( typeof ( ObeliskTip ) );
						if ( obelisk != null )
						{
							ObeliskTip tip = (ObeliskTip)obelisk;
							if ( tip.ObeliskOwner == m && tip.HasEarth > 0 && tip.WonEarth > 0 )
							{
								CanKillIt = 1;
							}
						}
					}
				}
			}

			if ( CanDie == 0 && CanKillIt == 0 )
			{
				Say(StringCatalog.ResolveByKey(this.Account, "mob.other.you_cannot_crush_me_puny_one"));
				this.Hits = this.HitsMax;
				this.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
				this.PlaySound( 0x202 );
				return false;
			}
			else if ( CanKillIt == 0 )
			{
				string Iam = "the Titan of Earth";
				PlayerMobile killer = MobileUtilities.TryGetKillingPlayer( this );
				Server.Misc.LoggingFunctions.LogSlayingLord( killer, Iam );
				if ( winner is PlayerMobile )
				{
					LoggingFunctions.LogGenericQuest( winner, "has obtained the power of the earth titan" );
				}

				if ( winner != null )
				{
					if ( winner is BaseCreature )
						winner = ((BaseCreature)winner).GetMaster();

					if ( winner is PlayerMobile && !winner.Blessed )
					{
						Party p = Engines.PartySystem.Party.Get( winner );
						if ( p != null )
						{
							foreach ( PartyMemberInfo pmi in p.Members )
							{
								if ( pmi.Mobile is PlayerMobile && pmi.Mobile.InRange(this.Location, 20) && pmi.Mobile.Map == this.Map && !pmi.Mobile.Blessed && !Server.Misc.PlayerSettings.GetSpecialsKilled( pmi.Mobile, StringCatalog.ResolveByKey(this.Account, "mob.other.titanlithos") ) )
								{
									Server.Misc.PlayerSettings.SetSpecialsKilled( pmi.Mobile, StringCatalog.ResolveByKey(this.Account, "mob.other.titanlithos"), true );
									ManualOfItems book = new ManualOfItems();
										book.Hue = 0xAC0;
										book.ItemID = 0x1AA3;
										book.Name = "Chest of Earth Titan Relics";
										book.m_Charges = 1;
										book.m_Skill_1 = 0;
										book.m_Skill_2 = 0;
										book.m_Skill_3 = 0;
										book.m_Skill_4 = 0;
										book.m_Skill_5 = 0;
										book.m_Value_1 = 0.0;
										book.m_Value_2 = 0.0;
										book.m_Value_3 = 0.0;
										book.m_Value_4 = 0.0;
										book.m_Value_5 = 0.0;
										book.m_Slayer_1 = 5;
										book.m_Slayer_2 = 0;
										book.m_Owner = pmi.Mobile;
										book.m_Extra = "of the Earth";
										book.m_FromWho = "Taken from Lithos";
										book.m_HowGiven = "Acquired by";
										book.m_Points = 300;
										book.m_Hue = 0xAC0;
										pmi.Mobile.AddToBackpack( book );

									pmi.Mobile.SendMessage("An item has appeared in your backpack!");
								}
							}
						}
						else if ( !Server.Misc.PlayerSettings.GetSpecialsKilled( winner, StringCatalog.ResolveByKey(this.Account, "mob.other.titanlithos") ) )
						{
							Server.Misc.PlayerSettings.SetSpecialsKilled( winner, StringCatalog.ResolveByKey(this.Account, "mob.other.titanlithos"), true );
							ManualOfItems book = new ManualOfItems();
								book.Hue = 0xAC0;
								book.ItemID = 0x1AA3;
								book.Name = "Chest of Earth Titan Relics";
								book.m_Charges = 1;
								book.m_Skill_1 = 0;
								book.m_Skill_2 = 0;
								book.m_Skill_3 = 0;
								book.m_Skill_4 = 0;
								book.m_Skill_5 = 0;
								book.m_Value_1 = 0.0;
								book.m_Value_2 = 0.0;
								book.m_Value_3 = 0.0;
								book.m_Value_4 = 0.0;
								book.m_Value_5 = 0.0;
								book.m_Slayer_1 = 5;
								book.m_Slayer_2 = 0;
								book.m_Owner = winner;
								book.m_Extra = "of the Earth";
								book.m_FromWho = "Taken from Lithos";
								book.m_HowGiven = "Acquired by";
								book.m_Points = 300;
								book.m_Hue = 0xAC0;
								winner.AddToBackpack( book );

							winner.SendMessage("An item has appeared in your backpack!");
						}
					}
				}

				if ( GetPlayerInfo.LuckyKiller( winner.Luck ) && Utility.RandomMinMax( 1, 10 ) == 1 )
				{
					Item Arty = new Artifact_BootsofLithos();
					switch( Utility.RandomMinMax( 0, 3 ) )
					{
						case 1: Arty.Delete(); Arty = new Artifact_MantleofLithos(); break;
						case 2: Arty.Delete(); Arty = new Artifact_RobeofLithos(); break;
						case 3: Arty.Delete(); Arty = new Artifact_LithosTome(); break;
					}
					AddItem( Arty );
				}
			}
			return base.OnBeforeDeath();
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich, 6 );
		}

		public override int TreasureMapLevel{ get{ return 6; } }
		public override bool BardImmune { get { return true; } }

		public TitanLithos( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}