using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Localization;
using Server.Misc;
using Server.Regions;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Spells.Shinobi;
using Server.Utilities;

namespace Server.Mobiles
{
	public enum DisguiseSellRefuseReason
	{
		None,
		StandardVendorBlock,
		NoDisguise,
		Criminal,
		Begging,
		VendorRejectCooldown,
		SessionNotOpened
	}

	public static class MurdererDisguiseSell
	{
		private sealed class SessionState
		{
			public int SuccessfulSales;
			public int SessionGold;
			public long TrickIdentity;
			public bool OpeningPassed;
		}

		private sealed class DailyStats
		{
			public DateTime Day;
			public int KarmaApplied;
			public int FameApplied;
		}

		private static readonly Dictionary<int, SessionState> s_Sessions = new Dictionary<int, SessionState>();
		private static readonly Dictionary<string, DateTime> s_VendorRejectUntil = new Dictionary<string, DateTime>();
		private static readonly Dictionary<int, DailyStats> s_DailyStats = new Dictionary<int, DailyStats>();

		public static void Configure()
		{
			EventSink.Logout += OnLogout;
			EventSink.PlayerDeath += OnPlayerDeath;
			EventSink.Disconnected += OnDisconnected;
		}

		public static void NotifyTrickEnded( Mobile from )
		{
			ClearSession( from );
		}

		public static void Initialize()
		{
			MurdererDisguiseSellPersistance.EnsureExistence();
		}

		public static bool IsWantedForDisguiseSell( Mobile m )
		{
			if ( !( m is PlayerMobile ) )
				return false;

			PlayerMobile pm = (PlayerMobile)m;

			return pm.Kills > 0 || pm.Fugitive == 1;
		}

		public static bool IsAllowedRegion( Mobile from )
		{
			if ( from == null )
				return false;

			if ( from.Region.IsPartOf( typeof( PublicRegion ) ) )
				return true;

			if ( from.Region.IsPartOf( typeof( StartRegion ) ) )
				return true;

			if ( from.Region.IsPartOf( typeof( SafeRegion ) ) )
				return true;

			if ( from.Region.IsPartOf( typeof( ProtectedRegion ) ) )
				return true;

			if ( from.Region.IsPartOf( typeof( NecromancerRegion ) ) && GetPlayerInfo.EvilPlayer( from ) )
				return true;

			return false;
		}

		public static bool NeedsDisguiseSellPath( Mobile from )
		{
			return MySettings.S_MurdererDisguiseSell
				&& IsWantedForDisguiseSell( from )
				&& !IsAllowedRegion( from );
		}

		public static bool IsTrickActive( Mobile from )
		{
			return GetTrickIdentity( from ) != 0;
		}

		public static bool IsBegging( Mobile from, BaseVendor vendor )
		{
			return from != null && vendor != null && BaseVendor.BeggingPose( from ) > 0;
		}

		public static bool IsVendorRejectCooldown( Mobile from, BaseVendor vendor )
		{
			if ( from == null || vendor == null )
				return false;

			string key = RejectKey( from, vendor );
			DateTime until;

			if ( !s_VendorRejectUntil.TryGetValue( key, out until ) )
				return false;

			if ( DateTime.UtcNow >= until )
			{
				s_VendorRejectUntil.Remove( key );
				return false;
			}

			return true;
		}

		public static bool CanAttemptDisguiseSell( Mobile from, BaseVendor vendor, out DisguiseSellRefuseReason reason )
		{
			reason = DisguiseSellRefuseReason.None;

			if ( !NeedsDisguiseSellPath( from ) )
			{
				reason = DisguiseSellRefuseReason.StandardVendorBlock;
				return false;
			}

			if ( IsBegging( from, vendor ) )
			{
				reason = DisguiseSellRefuseReason.Begging;
				return false;
			}

			if ( from.Criminal && !MySettings.S_MurdererDisguiseAllowCriminal )
			{
				reason = DisguiseSellRefuseReason.Criminal;
				return false;
			}

			if ( IsVendorRejectCooldown( from, vendor ) )
			{
				reason = DisguiseSellRefuseReason.VendorRejectCooldown;
				return false;
			}

			if ( !IsTrickActive( from ) )
			{
				reason = DisguiseSellRefuseReason.NoDisguise;
				return false;
			}

			if ( IntelligentAction.GetMyEnemies( from, vendor, true ) )
			{
				reason = DisguiseSellRefuseReason.StandardVendorBlock;
				return false;
			}

			return true;
		}

		public static void TrySaySellRefused( BaseVendor vendor, Mobile from )
		{
			DisguiseSellRefuseReason reason = DisguiseSellRefuseReason.StandardVendorBlock;

			if ( NeedsDisguiseSellPath( from ) )
				CanAttemptDisguiseSell( from, vendor, out reason );

			if ( !NeedsDisguiseSellPath( from ) || reason == DisguiseSellRefuseReason.StandardVendorBlock )
			{
				CitizenLocalization.SayToLocalizedByKey( vendor, from, "mob.other.i_have_no_business_with_you", "I have no business with you." );
				return;
			}

			string key;

			switch ( reason )
			{
				case DisguiseSellRefuseReason.NoDisguise:
					key = "mob.other.disguise_sell_no_disguise";
					break;
				case DisguiseSellRefuseReason.Criminal:
					key = "mob.other.disguise_sell_criminal_blocked";
					break;
				case DisguiseSellRefuseReason.Begging:
					key = "mob.other.disguise_sell_begging_blocked";
					break;
				case DisguiseSellRefuseReason.VendorRejectCooldown:
					key = "mob.other.disguise_sell_reject_cd";
					break;
				case DisguiseSellRefuseReason.SessionNotOpened:
					key = "mob.other.disguise_sell_session_not_open";
					break;
				default:
					key = "mob.other.i_have_no_business_with_you";
					break;
			}

			string fallback = StringCatalog.ResolveByKey( null, key );
			CitizenLocalization.SayToLocalizedByKey( vendor, from, key, fallback );
		}

		public static bool TryOpenSession( Mobile from, BaseVendor vendor )
		{
			if ( !NeedsDisguiseSellPath( from ) )
				return true;

			DisguiseSellRefuseReason reason;

			if ( !CanAttemptDisguiseSell( from, vendor, out reason ) )
			{
				TrySaySellRefused( vendor, from );
				return false;
			}

			SessionState session = EnsureSession( from );

			if ( session == null )
			{
				TrySaySellRefused( vendor, from, DisguiseSellRefuseReason.NoDisguise );
				return false;
			}

			if ( RollCheck( from, vendor, 0, false ) )
			{
				session.OpeningPassed = true;
				return true;
			}

			OnCheckFailed( vendor, from, "open_session" );
			return false;
		}

		public static bool TryCompleteSaleRoll( Mobile from, BaseVendor vendor, int batchGold )
		{
			if ( !NeedsDisguiseSellPath( from ) )
				return true;

			DisguiseSellRefuseReason reason;

			if ( !CanAttemptDisguiseSell( from, vendor, out reason ) )
			{
				TrySaySellRefused( vendor, from );
				return false;
			}

			SessionState session = EnsureSession( from );

			if ( session == null )
			{
				TrySaySellRefused( vendor, from, DisguiseSellRefuseReason.NoDisguise );
				return false;
			}

			if ( !session.OpeningPassed )
			{
				TrySaySellRefused( vendor, from, DisguiseSellRefuseReason.SessionNotOpened );
				return false;
			}

			if ( RollCheck( from, vendor, batchGold, true ) )
				return true;

			OnCheckFailed( vendor, from, "sale_roll" );
			return false;
		}

		public static int ApplyPriceMult( int gold )
		{
			if ( gold <= 0 )
				return gold;

			int pct = MySettings.S_MurdererDisguiseSellPriceMultPercent;

			if ( pct <= 0 )
				return 0;

			if ( pct >= 100 )
				return gold;

			return Math.Max( 1, ( gold * pct ) / 100 );
		}

		/// <summary>
		/// Gold the seller actually receives after merchant-purse clamp (mirrors BaseVendor sell payout).
		/// </summary>
		public static int ComputePayableGold( int quotedGold, int vendorCoins )
		{
			if ( quotedGold <= 0 )
				return 0;

			if ( MySettings.S_RichMerchants )
				return quotedGold;

			if ( quotedGold > vendorCoins )
				return vendorCoins;

			return quotedGold;
		}

		public static void OnSaleSuccess( Mobile seller, BaseVendor vendor, int goldReceived, int itemsSold )
		{
			if ( !NeedsDisguiseSellPath( seller ) || goldReceived <= 0 )
				return;

			SessionState session = EnsureSession( seller );

			if ( session == null )
				return;

			session.SuccessfulSales++;
			session.SessionGold += goldReceived;

			int karmaAward = ComputeTierAward(
				goldReceived,
				MySettings.S_MurdererDisguiseKarmaGoldStep,
				MySettings.S_MurdererDisguiseKarmaPerStep,
				MySettings.S_MurdererDisguiseKarmaPerSaleCap );

			int fameAward = ComputeTierAward(
				goldReceived,
				MySettings.S_MurdererDisguiseFameGoldStep,
				MySettings.S_MurdererDisguiseFamePerStep,
				MySettings.S_MurdererDisguiseFamePerSaleCap );

			DailyStats daily = GetDailyStats( seller );
			int karmaApplied = 0;
			int fameApplied = 0;

			if ( karmaAward > 0 )
			{
				int remaining = MySettings.S_MurdererDisguiseKarmaDailyCap - daily.KarmaApplied;

				if ( remaining > 0 )
				{
					karmaApplied = Math.Min( karmaAward, remaining );
					Titles.AwardKarma( seller, -karmaApplied, false );
					daily.KarmaApplied += karmaApplied;
				}
			}

			if ( fameAward > 0 )
			{
				int remaining = MySettings.S_MurdererDisguiseFameDailyCap - daily.FameApplied;

				if ( remaining > 0 )
				{
					fameApplied = Math.Min( fameAward, remaining );
					Titles.AwardFame( seller, fameApplied, false );
					daily.FameApplied += fameApplied;
				}
			}

			if ( karmaApplied > 0 || fameApplied > 0 )
			{
				if ( Utility.RandomMinMax( 1, 5 ) == 1 )
					seller.SendMessage( StringCatalog.ResolveByKey( seller.Account, "sys.disguise_sell.karma_fame_success" ) );
			}

			if ( itemsSold > 0 )
			{
				SkillUtilities.DoSkillChecks( seller, SkillName.Mercantile, 1, itemsSold );
				SkillUtilities.DoSkillChecks( seller, SkillName.Stealth, 1, 1 );
			}

			PlayerMobile pm = seller as PlayerMobile;

			if ( pm != null )
			{
				AnalyticsLogger.LogMurdererDisguiseSellCompleted(
					pm,
					vendor,
					goldReceived,
					itemsSold,
					karmaAward,
					karmaApplied,
					fameAward,
					fameApplied,
					session.SuccessfulSales,
					session.SessionGold,
					daily.KarmaApplied,
					daily.FameApplied );
			}
		}

		public static void OnCheckFailed( BaseVendor vendor, Mobile seller )
		{
			OnCheckFailed( vendor, seller, "unknown" );
		}

		public static void OnCheckFailed( BaseVendor vendor, Mobile seller, string failPhase )
		{
			if ( seller == null || vendor == null )
				return;

			SessionState prior = null;
			s_Sessions.TryGetValue( seller.Serial.Value, out prior );
			int priorSales = prior != null ? prior.SuccessfulSales : 0;
			int priorGold = prior != null ? prior.SessionGold : 0;

			PlayerMobile pm = seller as PlayerMobile;

			if ( pm != null )
				AnalyticsLogger.LogMurdererDisguiseSellFailed( pm, vendor, failPhase, priorSales, priorGold );

			RemoveActiveTrick( seller );
			seller.Criminal = true;

			ClearSession( seller );

			s_VendorRejectUntil[RejectKey( seller, vendor )] = DateTime.UtcNow.AddMinutes( MySettings.S_MurdererDisguiseVendorRejectMinutes );

			CitizenLocalization.SayLocalizedByKey( vendor, "mob.other.disguise_sell_revealed", "Guards! This one is a fraud!" );
			CitizenLocalization.SayLocalizedByKey( vendor, "mob.other.disguise_sell_help_guards", "Help! Guards!" );
		}

		internal static void SerializeDailyStats( GenericWriter writer )
		{
			PruneStaleDailyStats();

			writer.Write( s_DailyStats.Count );

			foreach ( KeyValuePair<int, DailyStats> entry in s_DailyStats )
			{
				writer.Write( entry.Key );
				writer.Write( entry.Value.Day );
				writer.Write( entry.Value.KarmaApplied );
				writer.Write( entry.Value.FameApplied );
			}
		}

		internal static void DeserializeDailyStats( GenericReader reader )
		{
			int count = reader.ReadInt();

			for ( int i = 0; i < count; ++i )
			{
				int serial = reader.ReadInt();
				DateTime day = reader.ReadDateTime();
				int karma = reader.ReadInt();
				int fame = reader.ReadInt();

				s_DailyStats[serial] = new DailyStats
				{
					Day = day,
					KarmaApplied = karma,
					FameApplied = fame
				};
			}
		}

		private static void TrySaySellRefused( BaseVendor vendor, Mobile from, DisguiseSellRefuseReason reason )
		{
			string key;

			switch ( reason )
			{
				case DisguiseSellRefuseReason.NoDisguise:
					key = "mob.other.disguise_sell_no_disguise";
					break;
				case DisguiseSellRefuseReason.Criminal:
					key = "mob.other.disguise_sell_criminal_blocked";
					break;
				case DisguiseSellRefuseReason.Begging:
					key = "mob.other.disguise_sell_begging_blocked";
					break;
				case DisguiseSellRefuseReason.VendorRejectCooldown:
					key = "mob.other.disguise_sell_reject_cd";
					break;
				case DisguiseSellRefuseReason.SessionNotOpened:
					key = "mob.other.disguise_sell_session_not_open";
					break;
				default:
					key = "mob.other.i_have_no_business_with_you";
					break;
			}

			string fallback = StringCatalog.ResolveByKey( null, key );
			CitizenLocalization.SayToLocalizedByKey( vendor, from, key, fallback );
		}

		/// <summary>
		/// Returns the live session for an active trick, or null when no trick is active.
		/// Never returns a throwaway SessionState that is not tracked in <see cref="s_Sessions"/>.
		/// </summary>
		private static SessionState EnsureSession( Mobile from )
		{
			if ( from == null )
				return null;

			PruneStaleSessions();

			int key = from.Serial.Value;
			long trickId = GetTrickIdentity( from );

			if ( trickId == 0 )
			{
				ClearSession( from );
				return null;
			}

			SessionState session;

			if ( !s_Sessions.TryGetValue( key, out session ) || session.TrickIdentity != trickId )
			{
				session = new SessionState { TrickIdentity = trickId };
				s_Sessions[key] = session;
			}

			return session;
		}

		private static void ClearSession( Mobile from )
		{
			if ( from == null )
				return;

			s_Sessions.Remove( from.Serial.Value );
		}

		private static DailyStats GetDailyStats( Mobile from )
		{
			PruneStaleDailyStats();

			int key = from.Serial.Value;
			DailyStats daily;
			DateTime today = DateTime.UtcNow.Date;

			if ( !s_DailyStats.TryGetValue( key, out daily ) || daily.Day != today )
			{
				daily = new DailyStats { Day = today };
				s_DailyStats[key] = daily;
			}

			return daily;
		}

		private static bool RollCheck( Mobile from, BaseVendor vendor, int batchGold, bool includeSessionExposure )
		{
			SessionState session = EnsureSession( from );

			if ( session == null )
				return false;

			double stealth = ( from.Skills[SkillName.Stealth].Value + from.Skills[SkillName.Hiding].Value ) / 2.0;
			double mercantile = from.Skills[SkillName.Mercantile].Value;
			double weightedSkill = ( stealth * 0.6 ) + ( mercantile * 0.4 );

			double chance = 0.50 + ( 0.35 * ( weightedSkill / 100.0 ) );

			if ( includeSessionExposure )
			{
				chance -= session.SuccessfulSales * 0.06;
				chance -= GetSessionGoldExposurePenalty( session, batchGold );
			}

			if ( batchGold > 0 )
				chance -= Math.Min( 0.15, batchGold / 5000.0 );

			TimeSpan trickRemaining = GetTrickTimeRemaining( from );

			if ( trickRemaining > TimeSpan.Zero && trickRemaining < TimeSpan.FromMinutes( 15 ) )
				chance -= 0.05;

			chance = Math.Max( 0.15, Math.Min( 0.85, chance ) );

			return Utility.RandomDouble() < chance;
		}

		private static long GetTrickIdentity( Mobile from )
		{
			if ( from == null )
				return 0;

			unchecked
			{
				long id = 0;

				if ( DisguiseTimers.IsDisguised( from ) )
					id = ( id * 31 ) + CombineTrickIdentityPart( 1, DisguiseTimers.ExpiryTicks( from ) );

				if ( !from.CanBeginAction( typeof( PolymorphSpell ) ) )
					id = ( id * 31 ) + CombineTrickIdentityPart( 2, PolymorphSpell.ExpiryTicks( from ) );

				if ( !from.CanBeginAction( typeof( IncognitoSpell ) ) )
					id = ( id * 31 ) + CombineTrickIdentityPart( 4, IncognitoSpell.ExpiryTicks( from ) );

				if ( !from.CanBeginAction( typeof( Deception ) ) )
					id = ( id * 31 ) + CombineTrickIdentityPart( 8, Deception.ExpiryTicks( from ) );

				return id;
			}
		}

		private static long CombineTrickIdentityPart( int typeBit, long expiryTicks )
		{
			if ( expiryTicks <= 0 )
				return 0;

			return ( (long)typeBit << 48 ) ^ expiryTicks;
		}

		private static TimeSpan GetTrickTimeRemaining( Mobile from )
		{
			TimeSpan max = DisguiseTimers.TimeRemaining( from );
			TimeSpan remaining;

			remaining = PolymorphSpell.TimeRemaining( from );
			if ( remaining > max )
				max = remaining;

			remaining = IncognitoSpell.TimeRemaining( from );
			if ( remaining > max )
				max = remaining;

			remaining = Deception.TimeRemaining( from );
			if ( remaining > max )
				max = remaining;

			return max;
		}

		private static double GetSessionGoldExposurePenalty( SessionState session, int pendingBatchGold )
		{
			int start = MySettings.S_MurdererDisguiseSessionGoldExposureStart;

			if ( start <= 0 || session == null )
				return 0.0;

			int cumulative = session.SessionGold + Math.Max( 0, pendingBatchGold );

			if ( cumulative < start )
				return 0.0;

			double over = cumulative - start;
			return Math.Min( 0.20, 0.08 + ( over / start ) * 0.05 );
		}

		private static void PruneStaleDailyStats()
		{
			if ( s_DailyStats.Count == 0 )
				return;

			DateTime today = DateTime.UtcNow.Date;
			List<int> remove = null;

			foreach ( KeyValuePair<int, DailyStats> entry in s_DailyStats )
			{
				if ( entry.Value.Day != today || World.FindMobile( new Serial( entry.Key ) ) == null )
				{
					if ( remove == null )
						remove = new List<int>();

					remove.Add( entry.Key );
				}
			}

			if ( remove != null )
			{
				for ( int i = 0; i < remove.Count; ++i )
					s_DailyStats.Remove( remove[i] );
			}
		}

		private static void PruneStaleSessions()
		{
			if ( s_Sessions.Count == 0 )
				return;

			List<int> remove = null;

			foreach ( KeyValuePair<int, SessionState> entry in s_Sessions )
			{
				if ( World.FindMobile( new Serial( entry.Key ) ) == null )
				{
					if ( remove == null )
						remove = new List<int>();

					remove.Add( entry.Key );
				}
			}

			if ( remove != null )
			{
				for ( int i = 0; i < remove.Count; ++i )
					s_Sessions.Remove( remove[i] );
			}

			if ( s_VendorRejectUntil.Count == 0 )
				return;

			List<string> expired = null;
			DateTime now = DateTime.UtcNow;

			foreach ( KeyValuePair<string, DateTime> entry in s_VendorRejectUntil )
			{
				if ( now >= entry.Value )
				{
					if ( expired == null )
						expired = new List<string>();

					expired.Add( entry.Key );
				}
			}

			if ( expired != null )
			{
				for ( int i = 0; i < expired.Count; ++i )
					s_VendorRejectUntil.Remove( expired[i] );
			}
		}

		private static void RemoveActiveTrick( Mobile from )
		{
			if ( from == null )
				return;

			// Clear spell tricks first while CanBeginAction is still blocked, so BodyMod/NameMod
			// are restored before any legacy path EndAction's without visual cleanup.
			PolymorphSpell.RemoveEffect( from );
			IncognitoSpell.RemoveEffect( from );
			Deception.RemoveEffect( from );

			// Kit disguise: attempt full visual cleanup, then always drop the timer even when
			// NameMod is already null (otherwise IsDisguised stays true forever).
			if ( DisguiseTimers.IsDisguised( from ) )
				DisguiseTimers.RemoveDisguise( from );

			DisguiseTimers.RemoveTimer( from );
		}

		private static void OnLogout( LogoutEventArgs e )
		{
			if ( e.Mobile != null )
				ClearSession( e.Mobile );
		}

		private static void OnPlayerDeath( PlayerDeathEventArgs e )
		{
			if ( e.Mobile != null )
				ClearSession( e.Mobile );
		}

		private static void OnDisconnected( DisconnectedEventArgs e )
		{
			if ( e.Mobile != null )
				ClearSession( e.Mobile );
		}

		private static int ComputeTierAward( int gold, int goldStep, int perStep, int perSaleCap )
		{
			if ( gold <= 0 || goldStep <= 0 || perStep <= 0 )
				return 0;

			int award = ( gold / goldStep ) * perStep;

			if ( perSaleCap > 0 )
				award = Math.Min( award, perSaleCap );

			return Math.Max( 0, award );
		}

		private static string RejectKey( Mobile from, BaseVendor vendor )
		{
			return from.Serial.Value + ":" + vendor.Serial.Value;
		}
	}

	public class MurdererDisguiseSellPersistance : Item
	{
		private static MurdererDisguiseSellPersistance m_Instance;

		public static MurdererDisguiseSellPersistance Instance { get { return m_Instance; } }

		public static void EnsureExistence()
		{
			if ( m_Instance == null || m_Instance.Deleted )
				m_Instance = new MurdererDisguiseSellPersistance();

			// Older Initialize always constructed a new Item whose empty Delete() left orphans in the save.
			List<Item> orphans = null;

			foreach ( Item item in World.Items.Values )
			{
				MurdererDisguiseSellPersistance other = item as MurdererDisguiseSellPersistance;

				if ( other == null || other == m_Instance || other.Deleted )
					continue;

				if ( orphans == null )
					orphans = new List<Item>();

				orphans.Add( other );
			}

			if ( orphans != null )
			{
				for ( int i = 0; i < orphans.Count; ++i )
					orphans[i].Delete();
			}
		}

		public override string DefaultName
		{
			get { return "Murderer Disguise Sell Persistance - Internal"; }
		}

		private MurdererDisguiseSellPersistance() : base( 1 )
		{
			Movable = false;
		}

		public MurdererDisguiseSellPersistance( Serial serial ) : base( serial )
		{
			m_Instance = this;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 ); // version

			MurdererDisguiseSell.SerializeDailyStats( writer );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					MurdererDisguiseSell.DeserializeDailyStats( reader );
					break;
				}
				default:
				{
					// Fail loud rather than leave unread bytes that misalign the rest of the world save.
					throw new Exception( String.Format( "Invalid MurdererDisguiseSellPersistance save version: {0}", version ) );
				}
			}
		}
	}
}
