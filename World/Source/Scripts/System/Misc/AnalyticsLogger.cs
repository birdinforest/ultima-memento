using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Scripts.Mythik.Systems.Achievements;
using Server.Accounting;
using Server;
using Server.Engines.GlobalShoppe;
using Server.Engines.Avatar;
using Server.Mobiles;
using Server.Network;
using Server.Temptation;

namespace Server.Misc
{
	/// <summary>
	/// Phase-1 local analytics: JSONL events + CSV server samples under Logs/analytics/ (same root as Saves/, relative to Core.BaseDirectory).
	/// No third-party endpoints; no chat, IP, hardware, or coordinates in logs.
	/// </summary>
	public static class AnalyticsLogger
	{
		private static readonly object m_Lock = new object();
		private static bool m_SaltWarningIssued;
		private static HashSet<int> m_FirstDeathSerials;
		private static Dictionary<int, DateTime> m_LoginTimes;

		public static void Initialize()
		{
			if ( !MySettings.S_AnalyticsEnabled )
				return;

			m_FirstDeathSerials = new HashSet<int>();
			m_LoginTimes = new Dictionary<int, DateTime>();

			EventSink.Login += OnLogin;
			EventSink.Logout += OnLogout;
			EventSink.CharacterCreated += OnCharacterCreated;
			EventSink.PlayerDeath += OnPlayerDeath;

			CustomEventSink.BeginJourney += OnBeginJourney;
			CustomEventSink.AchievementObtained += OnAchievementObtained;
			CustomEventSink.CombatQuestCompleted += OnCombatQuestCompleted;

			Timer.DelayCall( TimeSpan.FromMinutes( 5 ), TimeSpan.FromMinutes( 5 ), 0, OnServerSample );
		}

		public static void OnAccountCreated( Account acc )
		{
			if ( !MySettings.S_AnalyticsEnabled || acc == null )
				return;

			var d = BaseAccountFields( acc, null );
			d["event_type"] = "account_created";
			d["feature_name"] = "account";
			d["feature_variant"] = "auto";
			Emit( d );
		}

		public static void LogTarotEnterLand( Mobile m, int page )
		{
			if ( !MySettings.S_AnalyticsEnabled || m == null )
				return;

			var acc = m.Account as Account;
			var pm = m as PlayerMobile;
			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "tarot_card_selected";
			d["feature_name"] = "gypsy_tarot";
			d["feature_variant"] = m.RaceID > 0 ? "creature" : "human";
			d["tarot_page"] = page.ToString( CultureInfo.InvariantCulture );
			d["race_id"] = m.RaceID.ToString( CultureInfo.InvariantCulture );
			Emit( d );
		}

		public static void LogTemptationAccepted( PlayerMobile m, TemptationFlags flags )
		{
			if ( !MySettings.S_AnalyticsEnabled || m == null )
				return;

			var acc = m.Account as Account;
			var d = BaseAccountFields( acc, m );
			d["event_type"] = "temptation_selected";
			d["feature_name"] = "temptation";
			d["feature_variant"] = ( (int) flags ).ToString( CultureInfo.InvariantCulture );
			d["temptation_flags_hex"] = ( (int) flags ).ToString( "X", CultureInfo.InvariantCulture );
			Emit( d );
		}

		public static void LogRacePotionSelected( Mobile m, int raceId )
		{
			if ( !MySettings.S_AnalyticsEnabled || m == null )
				return;

			var acc = m.Account as Account;
			var pm = m as PlayerMobile;
			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "race_potion_selected";
			d["feature_name"] = "race_potion";
			d["feature_variant"] = raceId.ToString( CultureInfo.InvariantCulture );
			Emit( d );
		}

		/// <param name="preResetContext">Career snapshot from before character reset; preferred over post-reset Avatar for statistics.</param>
		public static void LogAvatarEnabled( PlayerMobile m, Server.Engines.Avatar.PlayerContext preResetContext = null )
		{
			if ( !MySettings.S_AnalyticsEnabled || m == null )
				return;

			var acc = m.Account as Account;
			var d = BaseAccountFields( acc, m );
			d["event_type"] = "avatar_enabled";
			d["feature_name"] = "avatar";
			d["feature_variant"] = "avatar-enable";
			var ctx = preResetContext ?? m.Avatar;
			if ( ctx != null )
			{
				d["avatar_lifetime_deaths"] = ctx.LifetimeDeaths.ToString( CultureInfo.InvariantCulture );
				d["avatar_lifetime_combat_quests"] = ctx.LifetimeCombatQuestCompletions.ToString( CultureInfo.InvariantCulture );
				d["avatar_lifetime_creature_kills"] = ctx.LifetimeCreatureKills.ToString( CultureInfo.InvariantCulture );
				d["avatar_points_farmed"] = ctx.PointsFarmed.ToString( CultureInfo.InvariantCulture );
				d["avatar_points_saved"] = ctx.PointsSaved.ToString( CultureInfo.InvariantCulture );
				d["avatar_improved_template_count"] = ctx.ImprovedTemplateCount.ToString( CultureInfo.InvariantCulture );
				d["avatar_selected_template"] = ( (int) ctx.SelectedTemplate ).ToString( CultureInfo.InvariantCulture );
			}
			Emit( d );
		}

		public static void LogBulkCraftStarted( PlayerMobile m, int amount, string craftSystemName )
		{
			if ( !MySettings.S_AnalyticsEnabled || m == null || amount <= 1 )
				return;

			var acc = m.Account as Account;
			var d = BaseAccountFields( acc, m );
			d["event_type"] = "bulk_craft_started";
			d["feature_name"] = "bulk_craft";
			d["feature_variant"] = craftSystemName ?? "";
			d["craft_amount"] = amount.ToString( CultureInfo.InvariantCulture );
			Emit( d );
		}

		/// <summary>Called from Account.RemoveYoungStatus — natural 40h milestone or voluntary renounce.</summary>
		public static void LogYoungStatusRemoved( Account acc, PlayerMobile pm, int messageId )
		{
			if ( !MySettings.S_AnalyticsEnabled || acc == null )
				return;

			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "young_status_removed";
			d["feature_name"] = "young";
			d["removal_reason"] = messageId == 1019038 ? "age_threshold" : ( messageId == 502085 ? "voluntary" : "other" );
			d["feature_variant"] = d["removal_reason"];
			Emit( d );
		}

		/// <summary>Called from ResurrectCostGump after Resurrect() succeeds.</summary>
		public static void LogResurrectionCompleted( PlayerMobile from, string payment, int costGold )
		{
			if ( !MySettings.S_AnalyticsEnabled || from == null )
				return;

			var acc = from.Account as Account;
			var d = BaseAccountFields( acc, from );
			d["event_type"] = "resurrection_completed";
			d["feature_name"] = "death";
			d["resurrect_payment"] = payment;
			d["resurrect_cost_gold"] = costGold.ToString( CultureInfo.InvariantCulture );
			d["feature_variant"] = payment;
			Emit( d );
		}

		/// <summary>Called from BaseGuildmaster after NPC guild membership is granted.</summary>
		public static void LogGuildJoined( PlayerMobile from, NpcGuild guild )
		{
			if ( !MySettings.S_AnalyticsEnabled || from == null )
				return;

			var acc = from.Account as Account;
			var d = BaseAccountFields( acc, from );
			d["event_type"] = "guild_joined";
			d["feature_name"] = "npc_guild";
			d["guild_name"] = guild.ToString();
			d["feature_variant"] = d["guild_name"];
			Emit( d );
		}

		/// <summary>Called from CustomerOrderShoppe.CompleteOrder after rewards applied.</summary>
		public static void LogShoppeOrderCompleted( PlayerMobile from, RewardType selectedReward, int reputationDelta, string craftSystemName )
		{
			if ( !MySettings.S_AnalyticsEnabled || from == null )
				return;

			var acc = from.Account as Account;
			var d = BaseAccountFields( acc, from );
			d["event_type"] = "shoppe_order_completed";
			d["feature_name"] = "global_shoppe";
			d["reward_type"] = selectedReward.ToString();
			d["reputation_delta"] = reputationDelta.ToString( CultureInfo.InvariantCulture );
			d["shoppe_craft_system"] = craftSystemName ?? "";
			d["feature_variant"] = d["reward_type"];
			Emit( d );
		}

		private static void OnLogin( LoginEventArgs e )
		{
			if ( !MySettings.S_AnalyticsEnabled || e == null || e.Mobile == null )
				return;

			var pm = e.Mobile as PlayerMobile;
			if ( pm == null )
				return;

			var acc = pm.Account as Account;
			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "login";
			d["feature_name"] = "session";
			d["feature_variant"] = "login";
			Emit( d );

			if ( m_LoginTimes != null )
			{
				lock ( m_Lock )
					m_LoginTimes[pm.Serial.Value] = DateTime.UtcNow;
			}
		}

		private static void OnLogout( LogoutEventArgs e )
		{
			if ( !MySettings.S_AnalyticsEnabled || e == null || e.Mobile == null )
				return;

			var pm = e.Mobile as PlayerMobile;
			if ( pm == null )
				return;

			var acc = pm.Account as Account;
			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "logout";
			d["feature_name"] = "session";
			d["feature_variant"] = "logout";

			if ( m_LoginTimes != null )
			{
				lock ( m_Lock )
				{
					DateTime t0;
					if ( m_LoginTimes.TryGetValue( pm.Serial.Value, out t0 ) )
					{
						d["session_duration_minutes"] = ( ( DateTime.UtcNow - t0 ).TotalMinutes ).ToString( "0.##", CultureInfo.InvariantCulture );
						m_LoginTimes.Remove( pm.Serial.Value );
					}
					else
						d["session_duration_minutes"] = "";
				}
			}

			Emit( d );
		}

		private static void OnCharacterCreated( CharacterCreatedEventArgs e )
		{
			if ( !MySettings.S_AnalyticsEnabled || e == null )
				return;

			// Other handlers (CharacterCreation) set e.Mobile in the same invocation; defer to next slice so Mobile exists.
			Timer.DelayCall( TimeSpan.Zero, () => CharacterCreatedDeferred( e ) );
		}

		private static void CharacterCreatedDeferred( CharacterCreatedEventArgs e )
		{
			try
			{
				if ( e == null || e.Mobile == null )
					return;

				var pm = e.Mobile as PlayerMobile;
				if ( pm == null )
					return;

				var acc = e.Account as Account;
				var d = BaseAccountFields( acc, pm );
				d["event_type"] = "character_created";
				d["feature_name"] = "character";
				d["feature_variant"] = e.Profession.ToString( CultureInfo.InvariantCulture );
				Emit( d );
			}
			catch
			{
			}
		}

		private static void OnPlayerDeath( PlayerDeathEventArgs e )
		{
			if ( !MySettings.S_AnalyticsEnabled || e == null || e.Mobile == null )
				return;

			var pm = e.Mobile as PlayerMobile;
			if ( pm == null )
				return;

			var acc = pm.Account as Account;
			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "player_death";
			d["feature_name"] = "death";
			d["feature_variant"] = "death";
			Emit( d );

			lock ( m_Lock )
			{
				if ( m_FirstDeathSerials != null && m_FirstDeathSerials.Add( pm.Serial ) )
				{
					var fd = BaseAccountFields( acc, pm );
					fd["event_type"] = "first_death";
					fd["feature_name"] = "death";
					fd["feature_variant"] = "first_in_process";
					Emit( fd );
				}
			}
		}

		private static void OnBeginJourney( BeginJourneyArgs e )
		{
			if ( !MySettings.S_AnalyticsEnabled || e == null || e.Mobile == null )
				return;

			var pm = e.Mobile;
			var acc = pm.Account as Account;
			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "begin_journey";
			d["feature_name"] = "onboarding";
			d["feature_variant"] = "tarot_complete";
			Emit( d );
		}

		private static void OnAchievementObtained( AchievementObtainedArgs e )
		{
			if ( !MySettings.S_AnalyticsEnabled || e == null || e.Mobile == null || e.Achievement == null )
				return;

			var pm = e.Mobile;
			var acc = pm.Account as Account;
			var ach = e.Achievement;
			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "achievement_obtained";
			d["feature_name"] = "achievement";
			d["achievement_id"] = ach.ID.ToString( CultureInfo.InvariantCulture );
			d["achievement_category_id"] = ach.CategoryID.ToString( CultureInfo.InvariantCulture );
			d["achievement_reward_points"] = ach.RewardPoints.ToString( CultureInfo.InvariantCulture );
			d["achievement_title"] = ach.Title ?? "";
			d["feature_variant"] = d["achievement_id"];
			Emit( d );
		}

		private static void OnCombatQuestCompleted( CombatQuestCompletedArgs e )
		{
			if ( !MySettings.S_AnalyticsEnabled || e == null || e.Mobile == null )
				return;

			var pm = e.Mobile as PlayerMobile;
			if ( pm == null )
				return;

			var acc = pm.Account as Account;
			var d = BaseAccountFields( acc, pm );
			d["event_type"] = "combat_quest_completed";
			d["feature_name"] = "combat_quest";
			d["quest_award_gold"] = e.Award.ToString( CultureInfo.InvariantCulture );
			d["feature_variant"] = d["quest_award_gold"];
			Emit( d );
		}

		private static void OnServerSample()
		{
			if ( !MySettings.S_AnalyticsEnabled )
				return;

			try
			{
				string dir = Path.Combine( Core.BaseDirectory, "Logs", "analytics" );
				if ( !Directory.Exists( dir ) )
					Directory.CreateDirectory( dir );

				string file = Path.Combine( dir, "server-samples-" + DateTime.UtcNow.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture ) + ".csv" );
				bool exists = File.Exists( file );
				using ( var w = new StreamWriter( file, true ) )
				{
					if ( !exists )
						w.WriteLine( "ts_utc,online_connections,accounts_total,mobiles_total,items_total" );

					w.WriteLine(
						string.Format(
							CultureInfo.InvariantCulture,
							"{0:o},{1},{2},{3},{4}",
							DateTime.UtcNow,
							NetState.Instances.Count,
							Accounts.Count,
							World.Mobiles.Count,
							World.Items.Count ) );
				}
			}
			catch
			{
			}
		}

		private static Dictionary<string, string> BaseAccountFields( Account acc, PlayerMobile pm )
		{
			var d = new Dictionary<string, string>
			{
				{ "ts_utc", DateTime.UtcNow.ToString( "o", CultureInfo.InvariantCulture ) },
				{ "access_level", pm != null ? pm.AccessLevel.ToString() : ( acc != null ? acc.AccessLevel.ToString() : "Unknown" ) }
			};

			if ( !EnsureSalt() )
			{
				d["account_hash"] = "disabled_missing_salt";
			}
			else if ( acc != null && !string.IsNullOrEmpty( acc.Username ) )
			{
				d["account_hash"] = HashAccount( acc.Username );
				d["account_age_hours"] = ( ( DateTime.UtcNow - acc.Created.ToUniversalTime() ).TotalHours ).ToString( "0.##", CultureInfo.InvariantCulture );
				d["total_game_time_minutes"] = ( acc.TotalGameTime.TotalMinutes ).ToString( "0.##", CultureInfo.InvariantCulture );
				AppendCohort( d, acc );
			}
			else
			{
				d["account_hash"] = "unknown";
				d["account_age_hours"] = "0";
				d["total_game_time_minutes"] = "0";
				d["cohort_label"] = "unclassified";
				d["cohort_rule_source"] = "unclassified";
				d["cohort_time_window"] = "";
			}

			if ( pm != null )
			{
				d["character_hash"] = !string.IsNullOrEmpty( acc != null ? acc.Username : null )
					? HashCharacter( acc.Username, pm.Serial.Value )
					: "unknown";
				d["is_young"] = pm.Young ? "true" : "false";
				d["map"] = pm.Map == null ? "" : pm.Map.Name;
				d["region"] = SafeRegionName( pm );
			}
			else
			{
				d["character_hash"] = "";
				d["is_young"] = "false";
				d["map"] = "";
				d["region"] = "";
			}

			AppendProgressBucket( d, acc, pm );

			if ( !d.ContainsKey( "feature_name" ) )
				d["feature_name"] = "";
			if ( !d.ContainsKey( "feature_variant" ) )
				d["feature_variant"] = "";

			return d;
		}

		private static void AppendProgressBucket( Dictionary<string, string> d, Account acc, PlayerMobile pm )
		{
			double tgm = acc != null ? acc.TotalGameTime.TotalMinutes : 0;
			bool young = pm != null ? pm.Young : ( acc != null && acc.Young );

			if ( tgm < 30 )
				d["progress_bucket"] = "first_30m";
			else if ( young )
				d["progress_bucket"] = "young_phase";
			else
				d["progress_bucket"] = "post_young";
		}

		private static void AppendCohort( Dictionary<string, string> d, Account acc )
		{
			d["cohort_time_window"] = "phase1_window";

			if ( string.IsNullOrWhiteSpace( MySettings.S_AnalyticsPhase1StartUtc ) || string.IsNullOrWhiteSpace( MySettings.S_AnalyticsPhase1EndUtc ) )
			{
				d["cohort_label"] = "unclassified";
				d["cohort_rule_source"] = "unclassified";
				return;
			}

			DateTime start;
			DateTime end;
			if ( !DateTime.TryParse( MySettings.S_AnalyticsPhase1StartUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out start ) )
			{
				d["cohort_label"] = "unclassified";
				d["cohort_rule_source"] = "unclassified";
				return;
			}
			if ( !DateTime.TryParse( MySettings.S_AnalyticsPhase1EndUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out end ) )
			{
				d["cohort_label"] = "unclassified";
				d["cohort_rule_source"] = "unclassified";
				return;
			}

			var created = acc.Created.ToUniversalTime();
			if ( created >= start && created <= end )
			{
				d["cohort_label"] = "old_uo_phase1";
				d["cohort_rule_source"] = "time_window";
			}
			else
			{
				d["cohort_label"] = "new_player_phase2";
				d["cohort_rule_source"] = "time_window";
			}
		}

		private static bool EnsureSalt()
		{
			if ( string.IsNullOrWhiteSpace( MySettings.S_AnalyticsAccountSalt ) || MySettings.S_AnalyticsAccountSalt == "CHANGE_ME_SET_A_LONG_RANDOM_SECRET" )
			{
				if ( !m_SaltWarningIssued )
				{
					m_SaltWarningIssued = true;
					Console.WriteLine( "Warning: S_AnalyticsAccountSalt is not set. AnalyticsLogger will emit account_hash=disabled_missing_salt until configured." );
				}
				return false;
			}
			return true;
		}

		private static string HashAccount( string username )
		{
			string key = MySettings.S_AnalyticsAccountSalt + username.ToLowerInvariant();
			return Sha256Hex( key );
		}

		private static string HashCharacter( string username, int serial )
		{
			string key = MySettings.S_AnalyticsAccountSalt + username.ToLowerInvariant() + ":" + serial.ToString( CultureInfo.InvariantCulture );
			return Sha256Hex( key );
		}

		private static string Sha256Hex( string s )
		{
			using ( var sha = SHA256.Create() )
			{
				byte[] bytes = sha.ComputeHash( Encoding.UTF8.GetBytes( s ) );
				var sb = new StringBuilder( bytes.Length * 2 );
				for ( int i = 0; i < bytes.Length; i++ )
					sb.Append( bytes[i].ToString( "x2" ) );
				return sb.ToString();
			}
		}

		private static string SafeRegionName( Mobile m )
		{
			try
			{
				if ( m != null && m.Region != null && !string.IsNullOrEmpty( m.Region.Name ) )
					return m.Region.Name;
			}
			catch
			{
			}
			return "";
		}

		private static void Emit( Dictionary<string, string> fields )
		{
			lock ( m_Lock )
			{
				try
				{
					string dir = Path.Combine( Core.BaseDirectory, "Logs", "analytics" );
					if ( !Directory.Exists( dir ) )
						Directory.CreateDirectory( dir );

					string file = Path.Combine( dir, "events-" + DateTime.UtcNow.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture ) + ".jsonl" );
					using ( var w = new StreamWriter( file, true ) )
					{
						w.WriteLine( ToJson( fields ) );
					}
				}
				catch
				{
				}
			}
		}

		private static string ToJson( Dictionary<string, string> fields )
		{
			var sb = new StringBuilder();
			sb.Append( '{' );
			bool first = true;
			foreach ( var kv in fields )
			{
				if ( !first )
					sb.Append( ',' );
				first = false;
				sb.Append( '"' );
				sb.Append( JsonEscape( kv.Key ) );
				sb.Append( "\":\"" );
				sb.Append( JsonEscape( kv.Value ) );
				sb.Append( '"' );
			}
			sb.Append( '}' );
			return sb.ToString();
		}

		private static string JsonEscape( string s )
		{
			if ( string.IsNullOrEmpty( s ) )
				return "";
			return s.Replace( "\\", "\\\\" ).Replace( "\"", "\\\"" );
		}
	}
}
