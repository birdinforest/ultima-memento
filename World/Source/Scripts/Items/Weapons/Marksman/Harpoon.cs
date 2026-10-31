using System;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public interface IHarpoon
	{
	}

	public class Harpoon : BaseRanged, IHarpoon
	{
		public override string DisplayNameLocalizationKey => "item.equip.weapon.harpoon";
		public override int EffectID{ get{ return 0x528A; } }
		public override Type AmmoType{ get{ return typeof( HarpoonRope ); } }
		public override Item Ammo{ get{ return new HarpoonRope(); } }

		public override int DefHitSound{ get{ return 0x5D2; } }
		public override int DefMissSound{ get{ return 0x5D3; } }

		public override SkillName DefSkill{ get{ return SkillName.Marksmanship; } }
		public override WeaponType DefType{ get{ return WeaponType.Ranged; } }
		public override WeaponAnimation DefAnimation{ get{ return WeaponAnimation.Pierce2H; } }

		public override SkillName AccuracySkill{ get{ return SkillName.Marksmanship; } }

		public override WeaponAbility PrimaryAbility{ get{ return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility{ get{ return WeaponAbility.ShadowStrike; } }
		public override WeaponAbility ThirdAbility{ get{ return WeaponAbility.MortalStrike; } }
		public override WeaponAbility FourthAbility{ get{ return WeaponAbility.InfectiousStrike; } }
		public override WeaponAbility FifthAbility{ get{ return WeaponAbility.DoubleShot; } }

		public override int AosStrengthReq{ get{ return 20; } }
		public override int AosMinDamage{ get{ return 15; } }
		public override int AosMaxDamage{ get{ return 19; } }
		public override int AosSpeed{ get{ return 25; } }
		public override float MlSpeed{ get{ return 4.00f; } }

		public override int OldStrengthReq{ get{ return 15; } }
		public override int OldMinDamage{ get{ return 9; } }
		public override int OldMaxDamage{ get{ return 41; } }
		public override int OldSpeed{ get{ return 20; } }

		public override int DefMaxRange{ get{ return 10; } }

		public override int InitMinHits{ get{ return 50; } }
		public override int InitMaxHits{ get{ return 90; } }

		[Constructable]
		public Harpoon() : base( 0xF63 )
		{
			Name = "harpoon";
			Weight = 7.0;
			Layer = Layer.TwoHanded;
		}

		public override bool OnEquip( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This is a throwing weapon that requires harpoon ropes to throw." ) );
			return base.OnEquip( from );
		}

		public Harpoon( Serial serial ) : base( serial )
		{
		}

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

	public class HarpoonRope : Item
	{
		public override string DisplayNameLocalizationKey => "item.equip.weapon.harpoonrope";
		public override double DefaultWeight
		{
			get { return 0.1; }
		}

		[Constructable]
		public HarpoonRope() : this( 1 )
		{
		}

		[Constructable]
		public HarpoonRope( int amount ) : base( 0x52B1 )
		{
			Name = "harpoon rope";
			Stackable = true;
			Amount = amount;
		}

		public override bool OnMoveOver( Mobile m )
		{
			if ( m is PlayerMobile && m.Alive && Movable )
			{
				m.PlaceInBackpack( this );
			}
			return true;
		}

		public HarpoonRope( Serial serial ) : base( serial )
		{
		}

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