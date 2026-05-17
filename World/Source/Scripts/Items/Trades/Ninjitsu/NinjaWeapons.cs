using System;
using Server.ContextMenus;
using Server.Localization;
using Server.Mobiles;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Targeting;

/*
 * There really was no prettier way to do this,  other than the one
 * suggestion to make a rigged baseninjaweapon class that bypasses its
 * own serialization, due to the way these weapons were originaly coded.
 */

namespace Server.Items
{
	public interface INinjaAmmo : IUsesRemaining
	{
		int PoisonCharges { get; set; }
		Poison Poison { get; set; }
	}

	public interface INinjaWeapon : IUsesRemaining
	{
		int NoFreeHandMessage { get; }
		int EmptyWeaponMessage { get; }
		int RecentlyUsedMessage { get; }
		int FullWeaponMessage { get; }
		int WrongAmmoMessage { get; }
		Type AmmoType { get; }
		int PoisonCharges { get; set; }
		Poison Poison { get; set; }
		int WeaponDamage { get; }
		int WeaponMinRange { get; }
		int WeaponMaxRange { get; }

		void AttackAnimation(Mobile from, Mobile to);
	}

	public class NinjaWeapon
	{
		private const int MaxUses = 10;

		private static string NinjaGameplayMessageKey( int clilocId )
		{
			switch ( clilocId )
			{
				case 1063303: return "prop.trade.ninja.msg.target.too.close";
				case 1070767: return "prop.trade.ninja.msg.unload.stronger.projectile";
				case 1063302: return "prop.trade.ninja.msg.full.shuriken";
				case 1063330: return "prop.trade.ninja.msg.full.fukiya.darts";
				case 1063299: return "prop.trade.ninja.belt.msg.need.free.hand";
				case 1063297: return "prop.trade.ninja.belt.msg.empty";
				case 1063298: return "prop.trade.ninja.belt.msg.cooldown";
				case 1063301: return "prop.trade.ninja.belt.msg.wrong.ammo";
				case 1063327: return "prop.trade.ninja.fukiya.msg.need.free.hand";
				case 1063325: return "prop.trade.ninja.fukiya.msg.empty";
				case 1063326: return "prop.trade.ninja.fukiya.msg.cooldown";
				case 1063329: return "prop.trade.ninja.fukiya.msg.wrong.ammo";
				default: return null;
			}
		}

		private static void SendNinjaGameplayMessage( Mobile m, int clilocId )
		{
			if ( m == null )
				return;

			string key = NinjaGameplayMessageKey( clilocId );

			if ( key == null )
			{
				m.SendLocalizedMessage( clilocId );
				return;
			}

			string lang = AccountLang.GetLanguageCode( m.Account );
			string text = StringCatalog.TryResolveByKey( lang, key );

			if ( text != null && text.Length > 0 )
				m.SendMessage( text );
			else
				m.SendLocalizedMessage( clilocId );
		}

		public static void AttemptShoot(PlayerMobile from, INinjaWeapon weapon)
		{
			if (CanUseWeapon(from, weapon))
			{
				from.BeginTarget(weapon.WeaponMaxRange, false, TargetFlags.Harmful, new TargetStateCallback<INinjaWeapon>(OnTarget), weapon);
			}
		}

		private static void Shoot(PlayerMobile from, Mobile target, INinjaWeapon weapon)
		{
			if (from != target && CanUseWeapon(from, weapon) && from.CanBeHarmful(target))
			{
				if (weapon.WeaponMinRange == 0 || !from.InRange(target, weapon.WeaponMinRange))
				{
					from.NinjaWepCooldown = true;

					from.Direction = from.GetDirectionTo(target);

					from.RevealingAction();

					weapon.AttackAnimation(from, target);

					ConsumeUse(weapon);

					if (CombatCheck(from, target))
					{
						Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback<object[]>(OnHit), new object[] { from, target, weapon });
					}

					Timer.DelayCall(TimeSpan.FromSeconds(2.5), new TimerStateCallback<PlayerMobile>(Resetusing), from);
				}
				else
				{
					SendNinjaGameplayMessage( from, 1063303 );
				}
			}
		}

		private static void Resetusing(PlayerMobile from)
		{
			from.NinjaWepCooldown = false;
		}

		private static void Unload(Mobile from, INinjaWeapon weapon)
		{
			if (weapon.UsesRemaining > 0)
			{
				INinjaAmmo ammo = Activator.CreateInstance(weapon.AmmoType, new object[] { weapon.UsesRemaining }) as INinjaAmmo;

				ammo.Poison = weapon.Poison;
				ammo.PoisonCharges = weapon.PoisonCharges;

				from.AddToBackpack((Item)ammo);

				weapon.UsesRemaining = 0;
				weapon.PoisonCharges = 0;
				weapon.Poison = null;
			}
		}

		private static void Reload(PlayerMobile from, INinjaWeapon weapon, INinjaAmmo ammo)
		{
			if (weapon.UsesRemaining < MaxUses)
			{
				int need = Math.Min((MaxUses - weapon.UsesRemaining), ammo.UsesRemaining);

				if (need > 0)
				{
					if (weapon.Poison != null && (ammo.Poison == null || weapon.Poison.Level > ammo.Poison.Level))
					{
						SendNinjaGameplayMessage( from, 1070767 );
					}
					else
					{
						if (weapon.UsesRemaining > 0)
						{
							if ((weapon.Poison == null && ammo.Poison != null)
								|| ((weapon.Poison != null && ammo.Poison != null) && weapon.Poison.Level != ammo.Poison.Level))
							{
								Unload(from, weapon);
								need = Math.Min(MaxUses, ammo.UsesRemaining);
							}
						}
						int poisonneeded = Math.Min((MaxUses - weapon.PoisonCharges), ammo.PoisonCharges);

						weapon.UsesRemaining += need;
						weapon.PoisonCharges += poisonneeded;

						if (weapon.PoisonCharges > 0)
						{
							weapon.Poison = ammo.Poison;
						}

						ammo.PoisonCharges -= poisonneeded;
						ammo.UsesRemaining -= need;

						if (ammo.UsesRemaining < 1)
						{
							((Item)ammo).Delete();
						}
						else if (ammo.PoisonCharges < 1)
						{
							ammo.Poison = null;
						}
					}
				} // "else" here would mean they targeted "ammo" with 0 uses.  undefined behavior.
			}
			else
			{
				SendNinjaGameplayMessage( from, weapon.FullWeaponMessage );
			}
		}

		private static void ConsumeUse(INinjaWeapon weapon)
		{
			if (weapon.UsesRemaining > 0)
			{
				weapon.UsesRemaining--;

				if (weapon.UsesRemaining < 1)
				{
					weapon.PoisonCharges = 0;
					weapon.Poison = null;
				}
			}
		}

		private static bool CanUseWeapon(PlayerMobile from, INinjaWeapon weapon)
		{
			if (WeaponIsValid(weapon, from))
			{
				if (weapon.UsesRemaining > 0)
				{
					if (!from.NinjaWepCooldown)
					{
						if (BasePotion.HasFreeHand(from))
						{
							return true;
						}
						else
						{
							SendNinjaGameplayMessage( from, weapon.NoFreeHandMessage );
						}
					}
					else
					{
						SendNinjaGameplayMessage( from, weapon.RecentlyUsedMessage );
					}
				}
				else
				{
					SendNinjaGameplayMessage( from, weapon.EmptyWeaponMessage );
				}
			}
			return false;
		}

		private static bool CombatCheck(Mobile attacker, Mobile defender) /* mod'd from baseweapon */
		{
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = defender.Skills.Ninjitsu;
			Skill defSkill = defender.Skills[defWeapon.Skill];

			double atSkillValue = attacker.Skills.Ninjitsu.Value;
			double defSkillValue = defWeapon.GetDefendSkillValue(attacker, defender);

			double attackValue = AosAttributes.GetValue(attacker, AosAttribute.AttackChance);

			if (defSkillValue <= -20.0)
			{
				defSkillValue = -19.9;
			}

			if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
			{
				attackValue += 10;
			}

			if (AnimalForm.UnderTransformation(attacker, typeof(GreyWolf)) || AnimalForm.UnderTransformation(attacker, typeof(MysticalFox)))
			{
				attackValue += 20;
			}

			if (HitLower.IsUnderAttackEffect(attacker))
			{
				attackValue -= 25;
			}

			if (attackValue > 45)
			{
				attackValue = 45;
			}

			attackValue = (atSkillValue + 20.0) * (100 + attackValue);

			double defenseValue = AosAttributes.GetValue(defender, AosAttribute.DefendChance);

			if (Spells.Chivalry.DivineFurySpell.UnderEffect(defender))
			{
				defenseValue -= 20;
			}

			if (HitLower.IsUnderDefenseEffect(defender))
			{
				defenseValue -= 25;
			}

			int refBonus = 0;

			if (Block.GetBonus(defender, ref refBonus))
			{
				defenseValue += refBonus;
			}

			if (SkillHandlers.Discordance.GetEffect(attacker, ref refBonus))
			{
				defenseValue -= refBonus;
			}

			if (defenseValue > 45)
			{
				defenseValue = 45;
			}

			defenseValue = (defSkillValue + 20.0) * (100 + defenseValue);

			double chance = attackValue / (defenseValue * 2.0);

			if (chance < 0.02)
			{
				chance = 0.02;
			}

			return attacker.CheckSkill(atkSkill.SkillName, chance);
		}

		private static void OnHit(object[] states)
		{
			Mobile from = states[0] as Mobile;
			Mobile target = states[1] as Mobile;
			INinjaWeapon weapon = states[2] as INinjaWeapon;

			if (from.CanBeHarmful(target))
			{
				from.DoHarmful(target);

				AOS.Damage(target, from, weapon.WeaponDamage, 100, 0, 0, 0, 0);

				if (weapon.Poison != null && weapon.PoisonCharges > 0)
				{
					if (EvilOmenSpell.TryEndEffect(target))
					{
						target.ApplyPoison(from, Poison.GetPoison(weapon.Poison.Level + 1));
					}
					else
					{
						target.ApplyPoison(from, weapon.Poison);
					}

					weapon.PoisonCharges--;

					if (weapon.PoisonCharges < 1)
					{
						weapon.Poison = null;
					}
				}
			}
		}

		private static void OnTarget(Mobile from, object targeted, INinjaWeapon weapon)
		{
			PlayerMobile player = from as PlayerMobile;

			if (WeaponIsValid(weapon, from))
			{
				if (targeted is Mobile)
				{
					Shoot(player, (Mobile)targeted, weapon);
				}
				else if (targeted.GetType() == weapon.AmmoType)
				{
					Reload(player, weapon, (INinjaAmmo)targeted);
				}
				else
				{
					SendNinjaGameplayMessage( player, weapon.WrongAmmoMessage );
				}
			}
		}

		private static bool WeaponIsValid(INinjaWeapon weapon, Mobile from)
		{
			Item item = weapon as Item;

			if (!item.Deleted && item.RootParent == from)
			{
				return true;
			}
			return false;
		}

		public class LoadEntry : ContextMenuEntry
		{
			private INinjaWeapon weapon;

			public LoadEntry(INinjaWeapon wep, int entry)
				: base(entry, 0)
			{
				weapon = wep;
			}

			public override void OnClick()
			{
				if (WeaponIsValid(weapon, Owner.From))
				{
					Owner.From.BeginTarget(10, false, TargetFlags.Harmful, new TargetStateCallback<INinjaWeapon>(OnTarget), weapon);
				}
			}
		}

		public class UnloadEntry : ContextMenuEntry
		{
			private INinjaWeapon weapon;

			public UnloadEntry(INinjaWeapon wep, int entry)
				: base(entry, 0)
			{
				weapon = wep;

				Enabled = (weapon.UsesRemaining > 0);
			}

			public override void OnClick()
			{
				if (WeaponIsValid(weapon, Owner.From))
				{
					Unload(Owner.From, weapon);
				}
			}
		}
	}

	internal static class NinjaAmmoOplProperties
	{
		public static void AddUsesAndPoisonProperties( Item item, ObjectPropertyList list, int usesRemaining, Poison poison, int poisonCharges )
		{
			if ( Item.OplBuildingLocale != null )
			{
				string loc = Item.OplBuildingLocale;
				string usesWord = StringCatalog.TryResolveByKey( loc, "prop.trade.common.uses.word" ) ?? "Uses";
				string lineFmt = StringCatalog.TryResolveByKey( loc, "prop.trade.common.uses.line" ) ?? "{0}\t{1}";
				list.Add( string.Format( lineFmt, usesRemaining, usesWord ) );

				if ( poison != null && poisonCharges > 0 )
				{
					string pk = PoisonChargeKey( poison.Level );
					string pf = StringCatalog.TryResolveByKey( loc, pk );

					if ( pf != null && pf.Length > 0 )
						list.Add( string.Format( pf, poisonCharges ) );
					else
						list.Add( 1062412 + poison.Level, poisonCharges.ToString() );
				}
			}
			else
			{
				list.Add( 1060584, "{0}\t{1}", usesRemaining.ToString(), "Uses" );

				if ( poison != null && poisonCharges > 0 )
					list.Add( 1062412 + poison.Level, poisonCharges.ToString() );
			}
		}

		private static string PoisonChargeKey( int level )
		{
			switch ( level )
			{
				case 0: return "prop.trade.ninja.poison.lesser";
				case 1: return "prop.trade.ninja.poison.regular";
				case 2: return "prop.trade.ninja.poison.greater";
				case 3: return "prop.trade.ninja.poison.deadly";
				case 4: return "prop.trade.ninja.poison.lethal";
				default: return "prop.trade.ninja.poison.lesser";
			}
		}
	}
}