using System;
using Server.Network;
using Server.Spells;
using Server.Mobiles;

namespace Server.Items
{
	public abstract class BaseKilrathi : BaseMeleeWeapon
	{
		public abstract int EffectID{ get; }
		public abstract Type AmmoType{ get; }

		public override int DefHitSound{ get{ return 0x54A; } }
		public override int DefMissSound{ get{ return 0x54A; } }

		public override SkillName DefSkill{ get{ return SkillName.Marksmanship; } }
		public override WeaponType DefType{ get{ return WeaponType.Ranged; } }
		public override WeaponAnimation DefAnimation{ get{ return WeaponAnimation.ShootXBow; } }

		public override CraftResource DefaultResource{ get{ return CraftResource.Durasteel; } }

		public override SkillName AccuracySkill{ get{ return SkillName.Marksmanship; } }

		public BaseKilrathi( int itemID ) : base( itemID )
		{
		}

		public BaseKilrathi( Serial serial ) : base( serial )
		{
		}

		public override TimeSpan OnSwing( Mobile attacker, Mobile defender )
		{
			WeaponAbility a = WeaponAbility.GetCurrentAbility( attacker );

			// Make sure we've been standing still for .25/.5/1 second depending on Era
			if ( DateTime.Now > (attacker.LastMoveTime + TimeSpan.FromSeconds( Core.SE ? 0.25 : (Core.AOS ? 0.5 : 1.0) )) || (Core.AOS && WeaponAbility.GetCurrentAbility( attacker ) is MovingShot) )
			{
				bool canSwing = true;

				canSwing = !attacker.Paralyzed && !attacker.Frozen;

				if ( canSwing )
				{
					Spell sp = attacker.Spell as Spell;
					canSwing = sp == null || !sp.IsCasting || !sp.BlocksMovement;
				}

				if ( canSwing )
				{
					PlayerMobile p = attacker as PlayerMobile;
					canSwing = p == null || p.PeacedUntil <= DateTime.Now;
				}

				if ( canSwing && attacker.HarmfulCheck( defender ) )
				{
					attacker.DisruptiveAction();
					attacker.Send( new Swing( 0, attacker, defender ) );

					if ( OnFired( attacker, defender ) )
					{
						if ( CheckHit( attacker, defender ) )
							OnHit( attacker, defender );
						else
							OnMiss( attacker, defender );
					}
				}

				if ( !( a is ShadowStrike || a is ShadowInfectiousStrike ) )
					attacker.RevealingAction();

				return GetDelay( attacker );
			}
			else
			{
				if ( !( a is ShadowStrike || a is ShadowInfectiousStrike ) )
					attacker.RevealingAction();

				return TimeSpan.FromSeconds( 0.25 );
			}
		}

		public override void OnHit( Mobile attacker, Mobile defender, double damageBonus )
		{
			base.OnHit( attacker, defender, damageBonus );
		}

		public override void OnMiss( Mobile attacker, Mobile defender )
		{
			base.OnMiss( attacker, defender );
		}

		public virtual bool OnFired( Mobile attacker, Mobile defender )
		{
			attacker.MovingEffect( defender, EffectID, 18, 1, false, false );

			Server.Gumps.QuickBar.RefreshQuickBar( attacker );

			if ( attacker.Player )
			{
				BaseQuiver quiver = attacker.FindItemOnLayer( Layer.Cloak ) as BaseQuiver;
				Container pack = attacker.Backpack;

				if ( quiver == null || Utility.Random( 100 ) >= quiver.LowerAmmoCost )
				{
					if ( quiver != null && quiver.ConsumeTotal( AmmoType, 1 ) )
						quiver.InvalidateWeight();
					else if ( pack == null || !pack.ConsumeTotal( AmmoType, 1 ) )
						return false;
				}
				else if ( quiver.FindItemByType( AmmoType ) == null && ( pack == null || pack.FindItemByType( AmmoType ) == null ) )
					return false;
			}

			return true;
		}

		public override void Serialize( GenericWriter writer ) { base.Serialize( writer ); writer.Write( (int) 0 ); }
		public override void Deserialize( GenericReader reader ) { base.Deserialize( reader ); int version = reader.ReadInt(); }
	}
}