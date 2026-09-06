using Server.Engines.Avatar;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server
{
	public static class DragonEggDropHelper
	{
		private const int DefaultTopN = 3;
		private const int DefaultRange = 20;

		public static void TryDropOnDeath( BaseCreature creature, Container c, int oneInN, int dragonBody, int needGold, bool useTitleInName )
		{
			if ( creature == null || c == null || oneInN <= 0 || creature.Controlled )
				return;

			PlayerMobile killer = MobileUtilities.TryGetKillingPlayer( creature );
			var eligible = RelicChestDropHelper.GetEligibleTopPlayers( creature, DefaultTopN, DefaultRange );
			double fortuneMult = 1.0;

			if ( AscentHuntBonus.IsDragonEggEnabled() && killer != null )
				fortuneMult = AscentHuntBonus.GetDropChanceMultiplier( killer, creature, DefaultTopN, DefaultRange, eligible );

			double basePct = 100.0 / oneInN;
			double actualPct = fortuneMult > 0 && fortuneMult != 1.0 ? basePct * fortuneMult : basePct;
			bool success = Utility.RandomDouble() * 100.0 < actualPct;

			if ( killer != null )
			{
				int damageDealt = 0;
				int damageRank = 0;

				if ( eligible != null )
				{
					for ( int i = 0; i < eligible.Count; i++ )
					{
						if ( eligible[i].Player == killer )
						{
							damageDealt = eligible[i].Damage;
							damageRank = eligible[i].Rank;
							break;
						}
					}
				}

				AnalyticsLogger.LogDragonEggRollAttempted(
					killer,
					new RareDropRollContext
					{
						EncounterId = RelicChestDropHelper.BuildEncounterId( creature ),
						BossType = creature.GetType().Name,
						BossKey = creature.GetType().Name,
						DamageRank = damageRank,
						DamageDealt = damageDealt,
						DamageTotal = RelicChestDropHelper.SumEncounterDamage( creature ),
						EligibleTop3Count = eligible != null ? eligible.Count : 0,
						Luck = killer.Luck,
						FortuneMult = fortuneMult,
						RollMaxPct = basePct,
						RollActualPct = actualPct,
						RollSuccess = success,
						AvatarActive = killer.Avatar.Active,
						OneInN = oneInN
					} );
			}

			if ( !success )
				return;

			DragonEgg egg = new DragonEgg();
			egg.DragonType = creature.YellHue;
			egg.DragonBody = dragonBody;
			egg.Hue = creature.Hue;
			egg.NeedGold = needGold;

			if ( useTitleInName )
				egg.Name = "egg of " + creature.Title;
			else
				egg.Name = "egg of " + creature.Name;

			c.DropItem( egg );
		}
	}
}
