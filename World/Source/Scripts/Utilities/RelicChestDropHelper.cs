using System;
using System.Collections.Generic;
using System.Globalization;
using Server.Items;
using Server.Localization;
using Server.Mobiles;
using Server.Misc;

namespace Server
{
	public class RelicsDropContext
	{
		public string EncounterId;
		public string DropGate;
		public string BossKey;
		public string BossType;
		public int DamageRank;
		public int DamageDealt;
		public int DamageTotal;
		public int EligibleTop3Count;
		public bool IsFirstTime;
		public int Luck;
		public double RollMaxPct;
		public double RollActualPct;
		public bool RollSuccess;
		public string DeliveryPath;
		public bool HasLootRight = true;
	}

	public class RelicEligiblePlayer
	{
		public PlayerMobile Player;
		public int Rank;
		public int Damage;
	}

	public class VaultRelicSnapshot
	{
		public List<int> EligibleSerials = new List<int>();
		public int[] EligibleRanks = new int[0];
		public int[] EligibleDamages = new int[0];
		public int DamageTotal;
		public int EligibleCount;
		public string EncounterId = "";
		public string BossType = "";
	}

	public static class RelicChestDropHelper
	{
		private static readonly double[] s_FirstMaxPct = new double[] { 0, 20, 10, 5 };
		private static readonly double[] s_RepeatMaxPct = new double[] { 0, 5, 4, 3 };

		public static string BuildEncounterId( BaseCreature boss )
		{
			if ( boss == null )
				return DateTime.UtcNow.ToString( "yyyyMMddTHHmmss", CultureInfo.InvariantCulture );

			return DateTime.UtcNow.ToString( "yyyyMMddTHHmmss", CultureInfo.InvariantCulture )
				+ "-0x" + boss.Serial.Value.ToString( "X8", CultureInfo.InvariantCulture );
		}

		public static double GetRollMaxPct( int rank, bool isFirstTime )
		{
			if ( rank < 1 || rank > 3 )
				return 0;

			return isFirstTime ? s_FirstMaxPct[rank] : s_RepeatMaxPct[rank];
		}

		public static double GetRollActualPct( int luck, int rank, bool isFirstTime )
		{
			double maxPct = GetRollMaxPct( rank, isFirstTime );
			int cappedLuck = luck;

			if ( cappedLuck < 0 )
				cappedLuck = 0;
			else if ( cappedLuck > 2000 )
				cappedLuck = 2000;

			return cappedLuck * maxPct / 2000.0;
		}

		public static bool RollRelicsChance( int luck, int rank, bool isFirstTime )
		{
			double actualPct = GetRollActualPct( luck, rank, isFirstTime );

			if ( actualPct <= 0 )
				return false;

			int threshold = (int)( actualPct * 100.0 );

			if ( threshold <= 0 )
				return false;

			return Utility.RandomMinMax( 1, 10000 ) <= threshold;
		}

		public static List<RelicEligiblePlayer> GetEligibleTopPlayers( BaseCreature boss, int topN, int range )
		{
			var result = new List<RelicEligiblePlayer>();

			if ( boss == null || boss.Map == null || topN <= 0 )
				return result;

			List<DamageStore> rights = BaseCreature.GetLootingRights( boss.DamageEntries, boss.HitsMax );

			if ( rights == null || rights.Count == 0 )
				return result;

			int rank = 0;

			for ( int i = 0; i < rights.Count && rank < topN; i++ )
			{
				DamageStore ds = rights[i];

				if ( ds == null || !ds.m_HasRight || ds.m_Mobile == null )
					continue;

				PlayerMobile pm = ds.m_Mobile as PlayerMobile;

				if ( pm == null || pm.Deleted || pm.Blessed )
					continue;

				if ( pm.Map != boss.Map || !pm.InRange( boss.Location, range ) )
					continue;

				rank++;
				result.Add( new RelicEligiblePlayer { Player = pm, Rank = rank, Damage = ds.m_Damage } );
			}

			return result;
		}

		public static int SumEncounterDamage( BaseCreature boss )
		{
			if ( boss == null )
				return 0;

			List<DamageStore> rights = BaseCreature.GetLootingRights( boss.DamageEntries, boss.HitsMax );

			if ( rights == null || rights.Count == 0 )
				return 0;

			int total = 0;

			for ( int i = 0; i < rights.Count; i++ )
			{
				DamageStore ds = rights[i];

				if ( ds != null )
					total += ds.m_Damage;
			}

			return total;
		}

		public static VaultRelicSnapshot BuildVaultSnapshot( BaseCreature boss, int range )
		{
			var top = GetEligibleTopPlayers( boss, 3, range );
			var snap = new VaultRelicSnapshot
			{
				EncounterId = BuildEncounterId( boss ),
				BossType = boss != null ? boss.GetType().Name : "",
				DamageTotal = SumEncounterDamage( boss ),
				EligibleCount = top.Count
			};

			snap.EligibleSerials = new List<int>( top.Count );
			snap.EligibleRanks = new int[top.Count];
			snap.EligibleDamages = new int[top.Count];

			for ( int i = 0; i < top.Count; i++ )
			{
				snap.EligibleSerials.Add( top[i].Player.Serial.Value );
				snap.EligibleRanks[i] = top[i].Rank;
				snap.EligibleDamages[i] = top[i].Damage;
			}

			return snap;
		}

		public static void ApplyVaultSnapshot( VaultRelicSnapshot snap, Item vault )
		{
			if ( snap == null || vault == null )
				return;

			if ( vault is IVaultRelicEligible eligible )
				eligible.SetVaultRelicSnapshot( snap );
		}

		public static void SerializeVaultSnapshot( GenericWriter writer, VaultRelicSnapshot snap )
		{
			if ( snap == null )
			{
				writer.Write( (string)"" );
				writer.Write( (string)"" );
				writer.Write( (int)0 );
				writer.Write( (int)0 );
				writer.Write( (int)0 );
				return;
			}

			writer.Write( snap.EncounterId ?? "" );
			writer.Write( snap.BossType ?? "" );
			writer.Write( snap.DamageTotal );
			writer.Write( snap.EligibleCount );

			int count = snap.EligibleSerials != null ? snap.EligibleSerials.Count : 0;
			writer.Write( count );

			for ( int i = 0; i < count; i++ )
			{
				writer.Write( snap.EligibleSerials[i] );
				writer.Write( snap.EligibleRanks != null && i < snap.EligibleRanks.Length ? snap.EligibleRanks[i] : 0 );
				writer.Write( snap.EligibleDamages != null && i < snap.EligibleDamages.Length ? snap.EligibleDamages[i] : 0 );
			}
		}

		public static VaultRelicSnapshot DeserializeVaultSnapshot( GenericReader reader )
		{
			var snap = new VaultRelicSnapshot
			{
				EncounterId = reader.ReadString(),
				BossType = reader.ReadString(),
				DamageTotal = reader.ReadInt(),
				EligibleCount = reader.ReadInt()
			};

			int count = reader.ReadInt();
			snap.EligibleSerials = new List<int>( count );
			snap.EligibleRanks = new int[count];
			snap.EligibleDamages = new int[count];

			for ( int i = 0; i < count; i++ )
			{
				snap.EligibleSerials.Add( reader.ReadInt() );
				snap.EligibleRanks[i] = reader.ReadInt();
				snap.EligibleDamages[i] = reader.ReadInt();
			}

			return snap;
		}

		public static void LogRelicsRollFork(
			PlayerMobile player,
			BaseCreature boss,
			string encounterId,
			string dropGate,
			string bossKey,
			int rank,
			int damageDealt,
			int damageTotal,
			int eligibleCount,
			bool isFirst,
			string deliveryPathNote )
		{
			if ( player == null )
				return;

			string bossType = boss != null ? boss.GetType().Name : "";
			double rollMaxPct = GetRollMaxPct( rank, isFirst );
			double rollActualPct = GetRollActualPct( player.Luck, rank, isFirst );

			RelicsDropContext ctx = BuildContext(
				encounterId,
				dropGate,
				bossKey,
				bossType,
				rank,
				damageDealt,
				damageTotal,
				eligibleCount,
				isFirst,
				player.Luck,
				rollMaxPct,
				rollActualPct,
				false,
				deliveryPathNote );

			AnalyticsLogger.LogRelicsRollAttempted( player, ctx );
		}

		public static void TryAwardRelics(
			BaseCreature boss,
			string encounterId,
			string dropGate,
			string bossKey,
			string deliveryPath,
			int range,
			Func<PlayerMobile, bool, ManualOfItems> createChest,
			Action<PlayerMobile, ManualOfItems> deliver,
			Func<PlayerMobile, int, bool, bool> shouldRoll = null )
		{
			if ( boss == null || createChest == null || deliver == null )
				return;

			if ( string.IsNullOrEmpty( encounterId ) )
				encounterId = BuildEncounterId( boss );

			List<RelicEligiblePlayer> top = GetEligibleTopPlayers( boss, 3, range );
			int damageTotal = SumEncounterDamage( boss );
			string bossType = boss.GetType().Name;

			for ( int i = 0; i < top.Count; i++ )
			{
				RelicEligiblePlayer entry = top[i];
				PlayerMobile player = entry.Player;

				if ( player == null )
					continue;

				bool isFirst = !PlayerSettings.GetSpecialsKilled( player, bossKey );

				if ( shouldRoll != null && !shouldRoll( player, entry.Rank, isFirst ) )
					continue;

				double rollMaxPct = GetRollMaxPct( entry.Rank, isFirst );
				double rollActualPct = GetRollActualPct( player.Luck, entry.Rank, isFirst );
				bool success = RollRelicsChance( player.Luck, entry.Rank, isFirst );

				RelicsDropContext ctx = BuildContext(
					encounterId,
					dropGate,
					bossKey,
					bossType,
					entry.Rank,
					entry.Damage,
					damageTotal,
					top.Count,
					isFirst,
					player.Luck,
					rollMaxPct,
					rollActualPct,
					success,
					deliveryPath );

				AnalyticsLogger.LogRelicsRollAttempted( player, ctx );

				if ( !success )
					continue;

				if ( isFirst )
					PlayerSettings.SetSpecialsKilled( player, bossKey, true );

				ManualOfItems chest = createChest( player, isFirst );

				if ( chest == null )
					continue;

				deliver( player, chest );
				AnalyticsLogger.LogRelicsChestAwarded( player, chest, ctx );
			}
		}

		public static bool TryProcessVaultOpen(
			Mobile from,
			VaultRelicSnapshot snap,
			string bossKey,
			string dropGate,
			Func<PlayerMobile, bool, ManualOfItems> createChest,
			LootChest targetChest,
			bool dropArtyOnFirstSuccess )
		{
			if ( from == null || snap == null || createChest == null || targetChest == null )
				return false;

			PlayerMobile pm = from as PlayerMobile;

			if ( pm == null )
				return false;

			int idx = snap.EligibleSerials != null ? snap.EligibleSerials.IndexOf( pm.Serial.Value ) : -1;

			if ( idx < 0 )
			{
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "sys.relics.vault_not_eligible" ) );
				return false;
			}

			int rank = snap.EligibleRanks[idx];
			int damage = snap.EligibleDamages[idx];
			bool isFirst = !PlayerSettings.GetSpecialsKilled( pm, bossKey );

			double rollMaxPct = GetRollMaxPct( rank, isFirst );
			double rollActualPct = GetRollActualPct( pm.Luck, rank, isFirst );
			bool success = RollRelicsChance( pm.Luck, rank, isFirst );

			RelicsDropContext ctx = BuildContext(
				snap.EncounterId,
				dropGate,
				bossKey,
				snap.BossType ?? "",
				rank,
				damage,
				snap.DamageTotal,
				snap.EligibleCount,
				isFirst,
				pm.Luck,
				rollMaxPct,
				rollActualPct,
				success,
				"vault_open" );

			AnalyticsLogger.LogRelicsRollAttempted( pm, ctx );

			if ( !success )
				return true;

			if ( isFirst )
			{
				PlayerSettings.SetSpecialsKilled( pm, bossKey, true );

				if ( dropArtyOnFirstSuccess && GetPlayerInfo.LuckyKiller( pm.Luck ) )
					targetChest.DropItem( Loot.RandomArty() );
			}

			ManualOfItems lexicon = createChest( pm, isFirst );

			if ( lexicon != null )
			{
				targetChest.DropItem( lexicon );
				AnalyticsLogger.LogRelicsChestAwarded( pm, lexicon, ctx );
			}

			return true;
		}

		private static RelicsDropContext BuildContext(
			string encounterId,
			string dropGate,
			string bossKey,
			string bossType,
			int rank,
			int damageDealt,
			int damageTotal,
			int eligibleCount,
			bool isFirst,
			int luck,
			double rollMaxPct,
			double rollActualPct,
			bool success,
			string deliveryPath )
		{
			return new RelicsDropContext
			{
				EncounterId = encounterId ?? "",
				DropGate = dropGate ?? "",
				BossKey = bossKey ?? "",
				BossType = bossType ?? "",
				DamageRank = rank,
				DamageDealt = damageDealt,
				DamageTotal = damageTotal,
				EligibleTop3Count = eligibleCount,
				IsFirstTime = isFirst,
				Luck = luck,
				RollMaxPct = rollMaxPct,
				RollActualPct = rollActualPct,
				RollSuccess = success,
				DeliveryPath = deliveryPath ?? "",
				HasLootRight = true
			};
		}
	}

	public interface IVaultRelicEligible
	{
		void SetVaultRelicSnapshot( VaultRelicSnapshot snap );
		VaultRelicSnapshot GetVaultRelicSnapshot();
	}
}
