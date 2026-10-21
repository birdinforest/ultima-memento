using Server;
using System;
using Server.Items;
using Server.Gumps;
using Server.Localization;

namespace Server.Mobiles
{
	[CorpseName( "a Cu Sidhe corpse" )]
	public class CuSidhe : BaseMount
	{
		[Constructable]
		public CuSidhe() : this( "a Cu Sidhe" )
		{
		}

		private const int HueRollTotal = 21000;
		private const int BlazeHueWeight = 105;
		private const int RareHueWeight = 2100;
		private const int UncommonHueWeight = 6300;

		[Constructable]
		public CuSidhe( string name ) : base( name, 277, 0x3E91, AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			AssignSpawnHue(); 

			SetStr( 1200, 1225 );
			SetDex( 150, 170 );
			SetInt( 250, 285 );
			
			SetHits( 1010, 1275 );

			SetDamage( 21, 28 );

			SetDamageType( ResistanceType.Physical, 0 );
			SetDamageType( ResistanceType.Cold, 50 );
			SetDamageType( ResistanceType.Energy, 50 );

			SetResistance( ResistanceType.Physical, 50, 65 );
			SetResistance( ResistanceType.Fire, 25, 45 );
			SetResistance( ResistanceType.Cold, 70, 85 );
			SetResistance( ResistanceType.Poison, 30, 50 );
			SetResistance( ResistanceType.Energy, 70, 85 );

			SetSkill( SkillName.FistFighting, 90.1, 96.8 );
			SetSkill( SkillName.Tactics, 90.3, 99.3 );
			SetSkill( SkillName.MagicResist, 75.3, 90.0 );
			SetSkill( SkillName.Anatomy, 65.5, 69.4 );
			SetSkill( SkillName.Healing, 72.2, 98.9 );

			Fame = 5000;  //Guessing here
			Karma = 5000;  //Guessing here

			Tamable = true;
			ControlSlots = 4;
			MinTameSkill = 101.1;					

			if ( Utility.RandomDouble() < 0.2 )
				PackItem( new TreasureMap( 5, Map, Location, X, Y ) );

			PackGold( 500, 800 );
		}	

		public override void GenerateLoot()
		{
			AddLoot( LootPack.MonsterFilthyRich, 5 );
		}

		/// <summary>
		/// Non-elves must wear <see cref="Artifact_PadsOfTheCuSidhe"/> on the shoes layer. Elves and staff above Player skip.
		/// Called from <see cref="OnDoubleClick"/> and <see cref="BaseMount.Ride"/>.
		/// </summary>
		public static bool CheckPadsForRider( Mobile from, bool message )
		{
			if ( from == null || from.Deleted )
				return false;

			if ( from.Race == Race.Elf || from.AccessLevel > AccessLevel.Player )
				return true;

			Item pads = from.FindItemOnLayer( Layer.Shoes );

			if ( pads is Artifact_PadsOfTheCuSidhe )
			{
				if ( message )
					from.SendLocalizedMessage( 1071981 ); // Your boots allow you to mount the Cu Sidhe.

				return true;
			}

			if ( message )
				from.SendLocalizedMessage( 1072203 ); // Only Elves may use this.

			return false;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from == ControlMaster && !CheckPadsForRider( from, true ) )
				return;

			base.OnDoubleClick( from );
		}

		public override void OnThink()
		{
			base.OnThink();

			Mobile rider = Rider;

			if ( rider == null || rider.Deleted )
				return;

			if ( rider.Race == Race.Elf || rider.AccessLevel > AccessLevel.Player )
				return;

			Item pads = rider.FindItemOnLayer( Layer.Shoes );

			if ( pads is Artifact_PadsOfTheCuSidhe )
				return;

			BaseMount.Dismount( rider );

			if ( rider is PlayerMobile pm )
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "mob.other.cu_sidhe.pads_left" ) );
		}
		
		public override bool CanHeal{ get{ return true; } }
		public override bool CanHealOwner{ get{ return true; } }
		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies; } }
		public override bool CanAngerOnTame{ get { return true; } }
		public override int Hides{ get{ return 10; } }	
		public override int Meat{ get{ return 3; } }	

		public override WeaponAbility GetWeaponAbility()
		{
			return WeaponAbility.BleedAttack;
		}

		private void AssignSpawnHue()
		{
			int roll = Utility.Random( HueRollTotal );
			int rareCeiling = BlazeHueWeight + RareHueWeight;
			int uncommonCeiling = rareCeiling + UncommonHueWeight;

			if ( roll < BlazeHueWeight )
				Hue = 1161;
			else if ( roll < rareCeiling )
				Hue = Utility.RandomList( 1301, 1154, 1201, 1652, 1109, 1153 );
			else if ( roll < uncommonCeiling )
				Hue = Utility.RandomList( 2426, 2218, 1447, 2424, 2305, 1319, 2220 );
		}

		public CuSidhe( Serial serial ) : base( serial )
		{
		}

		public override int GetIdleSound() { return 0x577; }
		public override int GetAttackSound() { return 0x576; }
		public override int GetAngerSound() { return 0x578; }
		public override int GetHurtSound() { return 0x576; }
		public override int GetDeathSound()	{ return 0x579; }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}
