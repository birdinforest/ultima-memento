using System;
using Server;
using System.Collections;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
using Server.ContextMenus;
using Server.Misc;
using Server.RateConfig;

namespace Server.Mobiles
{
	[CorpseName( "a dragyn corpse" )]
	[TypeAlias( "Server.Mobiles.GemDragon" )]
	public class GemDragon : BaseMount
	{
		public override bool ReacquireOnMovement{ get{ return !Controlled; } }
		public override bool HasBreath{ get{ return true; } }
		public override void BreathDealDamage( Mobile target, int form ){ base.BreathDealDamage( target, 9 ); }

		[Constructable]
		public GemDragon() : this( "a dragyn" )
		{
		}

		[Constructable]
		public GemDragon( string name ) : base( name, 61, 585, AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			BaseSoundID = 362;
			Tamable = true;
			ControlSlots = 3;

			if ( Utility.RandomMinMax(1,4) == 1 )
			{
				Name = "an elder dragyn";
				Body = 59;
				ItemID = 586;

				SetStr( 896, 925 );
				SetDex( 136, 155 );
				SetInt( 536, 575 );

				SetHits( 578, 595 );

				SetDamage( 19, 25 );

				SetDamageType( ResistanceType.Physical, 75 );
				SetDamageType( ResistanceType.Fire, 25 );

				SetResistance( ResistanceType.Physical, 55, 65 );
				SetResistance( ResistanceType.Fire, 60, 70 );
				SetResistance( ResistanceType.Cold, 30, 40 );
				SetResistance( ResistanceType.Poison, 25, 35 );
				SetResistance( ResistanceType.Energy, 35, 45 );

				SetSkill( SkillName.Psychology, 30.1, 40.0 );
				SetSkill( SkillName.Magery, 30.1, 40.0 );
				SetSkill( SkillName.MagicResist, 99.1, 100.0 );
				SetSkill( SkillName.Tactics, 97.6, 100.0 );
				SetSkill( SkillName.FistFighting, 90.1, 92.5 );

				Fame = 17000;
				Karma = -17000;
				VirtualArmor = 65;
				MinTameSkill = 99.9;
			}
			else
			{
				Name = "a dragyn";

				SetStr( 796, 825 );
				SetDex( 86, 105 );
				SetInt( 436, 475 );

				SetHits( 478, 495 );

				SetDamage( 16, 22 );

				SetDamageType( ResistanceType.Physical, 75 );
				SetDamageType( ResistanceType.Fire, 25 );

				SetResistance( ResistanceType.Physical, 55, 65 );
				SetResistance( ResistanceType.Fire, 60, 70 );
				SetResistance( ResistanceType.Cold, 30, 40 );
				SetResistance( ResistanceType.Poison, 25, 35 );
				SetResistance( ResistanceType.Energy, 35, 45 );

				SetSkill( SkillName.Psychology, 30.1, 40.0 );
				SetSkill( SkillName.Magery, 30.1, 40.0 );
				SetSkill( SkillName.MagicResist, 99.1, 100.0 );
				SetSkill( SkillName.Tactics, 97.6, 100.0 );
				SetSkill( SkillName.FistFighting, 90.1, 92.5 );

				Fame = 15000;
				Karma = -15000;
				VirtualArmor = 60;
				MinTameSkill = 93.9;
			}
		}

		// name -> CraftResource for "gemdragon.scaleWeight.<name>" keys in Data/RateConfig/gemdragon.json.
		// Unlike RidingDragon/Dragons/Wyrms, GemDragon's Resource (scale) drives its visible Hue directly
		// (see below), so its rarity is config-driven here rather than via DragonBreedRarity.
		private static readonly Dictionary<string, CraftResource> m_ScaleByName = new Dictionary<string, CraftResource>
		{
			{ "red", CraftResource.RedScales },
			{ "yellow", CraftResource.YellowScales },
			{ "black", CraftResource.BlackScales },
			{ "green", CraftResource.GreenScales },
			{ "white", CraftResource.WhiteScales },
			{ "blue", CraftResource.BlueScales },
			{ "violet", CraftResource.VioletScales },
			{ "metallic", CraftResource.MetallicScales },
			{ "brazen", CraftResource.BrazenScales },
			{ "umber", CraftResource.UmberScales },
			{ "platinum", CraftResource.PlatinumScales }
		};

		public override void OnAfterSpawn()
		{
			var weights = RateConfigEngine.GetTable( "gemdragon.scaleWeight" );
			string picked = WeightedPick.Pick( weights );

			CraftResource resource;

			if ( picked == null || !m_ScaleByName.TryGetValue( picked, out resource ) )
				resource = CraftResource.MetallicScales; // config missing/malformed — safe fallback

			Resource = resource;
			Hue = CraftResources.GetClr( Resource );

			base.OnAfterSpawn();
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich, 2 );
			AddLoot( LootPack.Rich, 1 );
			AddLoot( LootPack.Average, 1 );
		}

		public override bool AutoDispel{ get{ return !Controlled; } }
		public override int TreasureMapLevel{ get{ return 4; } }
		public override bool BleedImmune{ get{ return true; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Gold; } }
		public override bool CanAngerOnTame { get { return true; } }
		public override int Scales{ get{ return 7; } }
		public override ScaleType ScaleType{ get{ return ResourceScales(); } }
		public override int Skin{ get{ return Utility.Random(5); } }
		public override SkinType SkinType{ get{ return SkinType.Dragon; } }
		public override int Skeletal{ get{ return Utility.Random(5); } }
		public override SkeletalType SkeletalType{ get{ return SkeletalType.Draco; } }

		public GemDragon( Serial serial ) : base( serial )
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

			if ( Body != 61 && Body != 59 )
			{
				Body = 61;
			}
		}
	}
}