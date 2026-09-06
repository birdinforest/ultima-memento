using System;
using System.Collections.Generic;
using Server.Engines.PartySystem;
using Server.Mobiles;
using Server.RateConfig;

namespace Server.Engines.Avatar
{
	/// <summary>
	/// Hunt-drop chance bonus granted by Avatar's Ascent presence.
	/// Recipients are whoever already has Top-3 loot rights: an Avatar (self, 1.5–2.0) or a
	/// nearby party member when an eligible Avatar is present (half the bonus, 1.25–1.50).
	/// No Avatar in the encounter → 1.0. Does not expand loot eligibility.
	/// </summary>
	public static class AscentHuntBonus
	{
		private const string KeyEnabled = "avatar.fortune.enabled";
		private const string KeyMultMin = "avatar.fortune.multMin";
		private const string KeyMultMax = "avatar.fortune.multMax";
		private const string KeyAllyFraction = "avatar.fortune.allyFraction";
		private const string KeyDragonEggEnabled = "avatar.fortune.dragonEggEnabled";
		private const string KeyDragonScrollEnabled = "avatar.fortune.dragonRidingScrollEnabled";

		public static bool IsEnabled()
		{
			return RateConfigEngine.GetDouble( KeyEnabled, 1.0 ) >= 0.5;
		}

		public static bool IsDragonEggEnabled()
		{
			return IsEnabled() && RateConfigEngine.GetDouble( KeyDragonEggEnabled, 1.0 ) >= 0.5;
		}

		public static bool IsDragonRidingScrollEnabled()
		{
			return IsEnabled() && RateConfigEngine.GetDouble( KeyDragonScrollEnabled, 1.0 ) >= 0.5;
		}

		public static double GetDropChanceMultiplier(
			PlayerMobile beneficiary,
			BaseCreature boss,
			int topN,
			int range,
			List<RelicEligiblePlayer> eligibleOrNull )
		{
			if ( beneficiary == null || boss == null || boss.Map == null || !IsEnabled() )
				return 1.0;

			List<RelicEligiblePlayer> eligible = eligibleOrNull;

			if ( eligible == null )
				eligible = RelicChestDropHelper.GetEligibleTopPlayers( boss, topN, range );

			if ( !IsEligiblePlayer( beneficiary, eligible ) )
				return 1.0;

			double selfMult = beneficiary.Avatar.Active ? GetSelfMultiplier( beneficiary ) : 1.0;
			double allyMult = GetBestAllyMultiplier( beneficiary, boss, range, eligible );

			if ( selfMult <= 1.0 && allyMult <= 1.0 )
				return 1.0;

			return Math.Max( selfMult, allyMult );
		}

		/// <summary>
		/// Corpse-level rolls: resolve the killing player, then apply the same self/ally hunt bonus.
		/// Returns 1.0 when the killer is not Avatar and no eligible party Avatar is present.
		/// </summary>
		public static double GetMultiplierForKiller( BaseCreature boss, int topN, int range )
		{
			if ( boss == null || !IsEnabled() )
				return 1.0;

			PlayerMobile killer = MobileUtilities.TryGetKillingPlayer( boss );

			if ( killer == null )
				return 1.0;

			return GetDropChanceMultiplier( killer, boss, topN, range, null );
		}

		public static double GetVaultDropChanceMultiplier(
			PlayerMobile opener,
			VaultRelicSnapshot snap,
			Map map,
			Point3D loc,
			int range )
		{
			if ( opener == null || snap == null || map == null || !IsEnabled() )
				return 1.0;

			if ( !IsInSnapshot( opener, snap ) )
				return 1.0;

			double selfMult = opener.Avatar.Active ? GetSelfMultiplier( opener ) : 1.0;
			double allyMult = GetBestVaultAllyMultiplier( opener, snap, map, loc, range );

			if ( selfMult <= 1.0 && allyMult <= 1.0 )
				return 1.0;

			return Math.Max( selfMult, allyMult );
		}

		public static double GetSelfMultiplier( PlayerMobile pm )
		{
			if ( pm == null || !pm.Avatar.Active )
				return 1.0;

			double multMin = RateConfigEngine.GetDouble( KeyMultMin, 1.5 );
			double multMax = RateConfigEngine.GetDouble( KeyMultMax, 2.0 );

			if ( multMax < multMin )
			{
				double swap = multMin;
				multMin = multMax;
				multMax = swap;
			}

			return multMin + GetProgress( pm ) * ( multMax - multMin );
		}

		private static double GetProgress( PlayerMobile pm )
		{
			double statProgress = (double)pm.Avatar.StatCapLevel / Constants.STAT_CAP_MAX_LEVEL;
			double skillProgress = (double)pm.Avatar.SkillCapLevel / Constants.SKILL_CAP_MAX_LEVEL;
			double progress = Math.Max( statProgress, skillProgress );

			if ( progress < 0.0 )
				progress = 0.0;
			else if ( progress > 1.0 )
				progress = 1.0;

			return progress;
		}

		private static double ToAllyMultiplier( double selfMult )
		{
			if ( selfMult <= 1.0 )
				return 1.0;

			double fraction = RateConfigEngine.GetDouble( KeyAllyFraction, 0.5 );

			if ( fraction < 0.0 )
				fraction = 0.0;
			else if ( fraction > 1.0 )
				fraction = 1.0;

			return 1.0 + ( selfMult - 1.0 ) * fraction;
		}

		private static double GetBestAllyMultiplier(
			PlayerMobile beneficiary,
			BaseCreature boss,
			int range,
			List<RelicEligiblePlayer> eligible )
		{
			Party party = Party.Get( beneficiary );

			if ( party == null )
				return 1.0;

			double best = 1.0;

			for ( int i = 0; i < party.Members.Count; i++ )
			{
				PartyMemberInfo pmi = party.Members[i];
				PlayerMobile avatar = pmi.Mobile as PlayerMobile;

				if ( avatar == null || avatar == beneficiary || avatar.Deleted || avatar.Blessed )
					continue;

				if ( !avatar.Avatar.Active || !IsEligiblePlayer( avatar, eligible ) )
					continue;

				if ( avatar.Map != boss.Map || !avatar.InRange( boss.Location, range ) )
					continue;

				double allyMult = ToAllyMultiplier( GetSelfMultiplier( avatar ) );

				if ( allyMult > best )
					best = allyMult;
			}

			return best;
		}

		private static double GetBestVaultAllyMultiplier(
			PlayerMobile opener,
			VaultRelicSnapshot snap,
			Map map,
			Point3D loc,
			int range )
		{
			Party party = Party.Get( opener );

			if ( party == null )
				return 1.0;

			double best = 1.0;

			for ( int i = 0; i < party.Members.Count; i++ )
			{
				PartyMemberInfo pmi = party.Members[i];
				PlayerMobile avatar = pmi.Mobile as PlayerMobile;

				if ( avatar == null || avatar == opener || avatar.Deleted || avatar.Blessed )
					continue;

				if ( !avatar.Avatar.Active || !IsInSnapshot( avatar, snap ) )
					continue;

				if ( avatar.Map != map || !avatar.InRange( loc, range ) )
					continue;

				double allyMult = ToAllyMultiplier( GetSelfMultiplier( avatar ) );

				if ( allyMult > best )
					best = allyMult;
			}

			return best;
		}

		private static bool IsEligiblePlayer( PlayerMobile pm, List<RelicEligiblePlayer> eligible )
		{
			if ( pm == null || eligible == null )
				return false;

			for ( int i = 0; i < eligible.Count; i++ )
			{
				if ( eligible[i].Player == pm )
					return true;
			}

			return false;
		}

		private static bool IsInSnapshot( PlayerMobile pm, VaultRelicSnapshot snap )
		{
			return pm != null
				&& snap != null
				&& snap.EligibleSerials != null
				&& snap.EligibleSerials.Contains( pm.Serial.Value );
		}
	}
}
