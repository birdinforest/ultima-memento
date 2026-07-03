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
	[CorpseName( "an evil essence" )]
	public class Shadowlord : BaseCreature
	{
		[Constructable]
		public Shadowlord() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Title = "the shadowlord";

			Body = 0x190;
			Hue = 0x4001;
			BaseSoundID = 0x47D;
			NameHue = 0x22;
			EmoteHue = 123;

			Robe robe = new Robe();
				robe.ItemID = 0x2687;
			  	robe.Hue = 0x541;
				robe.LootType = LootType.Blessed;
				robe.Name = "shadowlord robe";
			  	AddItem( robe );

			SetStr( 986, 1185 );
			SetDex( 177, 255 );
			SetInt( 151, 250 );

			SetHits( 592, 711 );

			SetDamage( 22, 29 );

			SetDamageType( ResistanceType.Physical, 50 );
			SetDamageType( ResistanceType.Fire, 25 );
			SetDamageType( ResistanceType.Energy, 25 );

			SetResistance( ResistanceType.Physical, 65, 80 );
			SetResistance( ResistanceType.Fire, 60, 80 );
			SetResistance( ResistanceType.Cold, 50, 60 );
			SetResistance( ResistanceType.Poison, 100 );
			SetResistance( ResistanceType.Energy, 40, 50 );

			SetSkill( SkillName.Anatomy, 25.1, 50.0 );
			SetSkill( SkillName.Psychology, 90.1, 100.0 );
			SetSkill( SkillName.Magery, 95.5, 100.0 );
			SetSkill( SkillName.Meditation, 25.1, 50.0 );
			SetSkill( SkillName.MagicResist, 100.5, 150.0 );
			SetSkill( SkillName.Tactics, 90.1, 100.0 );
			SetSkill( SkillName.FistFighting, 90.1, 100.0 );

			Fame = 24000;
			Karma = -24000;

			VirtualArmor = 90;
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich, 3 );
			AddLoot( LootPack.Rich );
		}

		public override bool OnBeforeDeath()
		{
			int CanDie = 0;
			Mobile winner = this;
			ArrayList targets = new ArrayList();

			foreach ( Mobile m in this.GetMobilesInRange( 30 ) )
			{
				if ( m is PlayerMobile && !m.Blessed )
				{
					if ( this.Name == "Astaroth" )
					{
						Item flame = m.Backpack.FindItemByType( typeof ( CandleOfLove ) );
						if ( flame != null && flame is CandleOfLove && ((CandleOfLove)flame).Owner == m )
						{
							targets.Add( flame );
							CanDie = 1;
							winner = m;
							m.SendMessage( StringCatalog.ResolveByKey(m.Account, "mob.other.the_candle_of_love_has_vanished_after_dispatching_the_s") );
							Server.Items.QuestSouvenir.GiveReward( m, flame.Name, flame.Hue, flame.ItemID );
						}
					}
					else if ( this.Name == "Faulinei" )
					{
						Item flame = m.Backpack.FindItemByType( typeof ( BookOfTruth ) );
						if ( flame != null && flame is BookOfTruth && ((BookOfTruth)flame).Owner == m )
						{
							targets.Add( flame );
							CanDie = 1;
							winner = m;
							m.SendMessage( StringCatalog.ResolveByKey(m.Account, "mob.other.the_book_of_truth_has_vanished_after_dispatching_the_sh") );
							Server.Items.QuestSouvenir.GiveReward( m, flame.Name, flame.Hue, flame.ItemID );
						}
					}
					else
					{
						Item flame = m.Backpack.FindItemByType( typeof ( BellOfCourage ) );
						if ( flame != null && flame is BellOfCourage && ((BellOfCourage)flame).Owner == m )
						{
							targets.Add( flame );
							CanDie = 1;
							winner = m;
							m.SendMessage( StringCatalog.ResolveByKey(m.Account, "mob.other.the_bell_of_courage_has_vanished_after_dispatching_the") );
							Server.Items.QuestSouvenir.GiveReward( m, flame.Name, flame.Hue, flame.ItemID );
						}
					}
				}
			}

			if ( CanDie == 0 )
			{
				Say(StringCatalog.ResolveByKey(this.Account, "mob.other.foolish_mortal_you_cannot_defeat_me"));
				this.Hits = this.HitsMax;
				this.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
				this.PlaySound( 0x202 );
				return false;
			}
			else
			{
				this.Body = 13;
				this.Hue = 0x497;

				string Iam = this.Name + " the Shadowlord";
				PlayerMobile killer = MobileUtilities.TryGetKillingPlayer( this );
				Server.Misc.LoggingFunctions.LogSlayingLord( killer, Iam );

				for ( int i = 0; i < targets.Count; ++i )
				{
					Item item = ( Item )targets[ i ];
					item.Delete();
				}

				if ( this.Name == "Astaroth" && winner is PlayerMobile )
				{
					winner.AddToBackpack( new ShardOfHatred() );
					winner.SendMessage( StringCatalog.ResolveByKey(winner.Account, "mob.other.you_have_obtained_the_shard_of_hatred") );
					LoggingFunctions.LogGenericQuest( winner, StringCatalog.ResolveByKey(this.Account, "mob.other.has_obtained_the_shard_of_hatred") );
				}
				else if ( this.Name == "Faulinei" && winner is PlayerMobile )
				{
					winner.AddToBackpack( new ShardOfFalsehood() );
					winner.SendMessage( StringCatalog.ResolveByKey(winner.Account, "mob.other.you_have_obtained_the_shard_of_falsehood") );
					LoggingFunctions.LogGenericQuest( winner, StringCatalog.ResolveByKey(this.Account, "mob.other.has_obtained_the_shard_of_falsehood") );
				}
				else if ( this.Name == "Nosfentor" && winner is PlayerMobile )
				{
					winner.AddToBackpack( new ShardOfCowardice() );
					winner.SendMessage( StringCatalog.ResolveByKey(winner.Account, "mob.other.you_have_obtained_the_shard_of_cowardice") );
					LoggingFunctions.LogGenericQuest( winner, StringCatalog.ResolveByKey(this.Account, "mob.other.has_obtained_the_shard_of_cowardice") );
				}

				if ( winner != null )
				{
					if ( winner is BaseCreature )
						winner = ((BaseCreature)winner).GetMaster();

					string encounterId = RelicChestDropHelper.BuildEncounterId( this );
					string shadowName = this.Name;

					RelicChestDropHelper.TryAwardRelics(
						this,
						encounterId,
						"shadowlord",
						shadowName,
						"backpack",
						20,
						delegate( PlayerMobile player, bool isFirst )
						{
							ManualOfItems book = new ManualOfItems();
							book.Hue = 0x541;
							book.Name = "Chest of Shadowlord Relics";
							book.m_Charges = 1;
							book.m_Skill_1 = 99;
							book.m_Skill_2 = 32;
							book.m_Skill_3 = 0;
							book.m_Skill_4 = 0;
							book.m_Skill_5 = 0;
							book.m_Value_1 = 10.0;
							book.m_Value_2 = 10.0;
							book.m_Value_3 = 0.0;
							book.m_Value_4 = 0.0;
							book.m_Value_5 = 0.0;
							book.m_Slayer_1 = 5;
							book.m_Slayer_2 = 0;
							book.m_Owner = player;
							book.m_Extra = "of the Shadows";
							book.m_FromWho = "Spawned from the Shadowlords";
							book.m_HowGiven = "Acquired by";
							book.m_Points = 200;
							book.m_Hue = 0x541;
							return book;
						},
						delegate( PlayerMobile player, ManualOfItems book )
						{
							player.AddToBackpack( book );
							player.SendMessage( "An item has appeared in your backpack!" );
						} );
				}
			}
			return base.OnBeforeDeath();
		}

        public override void OnAfterSpawn()
        {
			base.OnAfterSpawn();

			if ( this.Home.X == 6124 && this.Home.Y == 2639 ){ this.Name = "Nosfentor"; }
			else if ( this.Home.X == 6159 && this.Home.Y == 2845 ){ this.Name = "Faulinei"; }
			else if ( this.Home.X == 6537 && this.Home.Y == 2616 ){ this.Name = "Astaroth"; }

			Effects.SendLocationParticles( EffectItem.Create( this.Location, this.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
			this.PlaySound( 0x1FE );
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override int TreasureMapLevel{ get{ return 6; } }
		public override bool BleedImmune{ get{ return true; } }
		public override bool BardImmune { get { return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }
		public override bool IsScaredOfScaryThings{ get{ return false; } }
		public override bool IsScaryToPets{ get{ return true; } }
		public override bool ClickTitle{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool AlwaysAttackable{ get{ return true; } }

		public Shadowlord( Serial serial ) : base( serial )
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