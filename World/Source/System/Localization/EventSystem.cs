using System;
using System.Collections.Generic;
using System.IO;
using Server;

namespace Server.Localization
{
	/// <summary>
	/// Runtime event configuration from Data/System/CFG/events.json (flat string map for SimpleJsonObject).
	/// </summary>
	public static class EventSystem
	{
		private static Dictionary<string, string> m_Config;

		static EventSystem()
		{
			m_Config = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
			LoadInternal();
		}

		public static void Load()
		{
			LoadInternal();
		}

		private static void LoadInternal()
		{
			m_Config.Clear();
			string path = Path.Combine( Core.BaseDirectory, "Data/System/CFG/events.json" );
			try
			{
				if ( File.Exists( path ) )
				{
					string json = File.ReadAllText( path );
					SimpleJsonObject.ParseStringProperties( json, m_Config );
				}
			}
			catch ( Exception ex )
			{
				Console.WriteLine( "[EventSystem] Failed to load events.json: " + ex );
			}
		}

		public static IEnumerable<string> GetActiveEventIds()
		{
			string raw;

			if ( !m_Config.TryGetValue( "active_events", out raw ) || raw == null || raw.Length == 0 )
				yield break;

			string[] parts = raw.Split( new char[]{ '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries );
			foreach ( string p in parts )
			{
				string id = p.Trim();
				if ( id.Length == 0 )
					continue;
				yield return id;
			}
		}

		public static bool IsEventMasterEnabled( string eventId )
		{
			if ( eventId == null || eventId.Length == 0 )
				return false;
			string v;

			return m_Config.TryGetValue( eventId + ".enabled", out v ) && ParseBoolLoose( v );
		}

		public static bool IsEventActive( string eventId )
		{
			if ( eventId == null || eventId.Length == 0 || !IsEventMasterEnabled( eventId ) )
				return false;

			foreach ( string a in GetActiveEventIds() )
			{
				if ( Insensitive.Equals( a, eventId ) )
					return true;
			}

			return false;
		}

		public static bool IsEnabled( string eventId, string featureKey )
		{
			return IsEventActive( eventId ) && GetBoolSetting( eventId, featureKey, false );
		}

		private static bool GetBoolSetting( string eventId, string featureKey, bool defaultValue )
		{
			string v;

			if ( !m_Config.TryGetValue( eventId + "." + featureKey, out v ) || v == null )
				return defaultValue;

			return ParseBoolLoose( v );
		}

		private static bool ParseBoolLoose( string v )
		{
			if ( string.IsNullOrEmpty( v ) )
				return false;
			string t = v.Trim();

			return Insensitive.Equals( t, "true" )
				|| Insensitive.Equals( t, "yes" )
				|| t == "1";
		}

		public static int GetChance( string eventId, string chanceKey )
		{
			if ( eventId == null || chanceKey == null )
				return 0;
			string v;

			if ( !m_Config.TryGetValue( eventId + "." + chanceKey, out v ) || v == null )
				return 0;
			double d;

			if ( double.TryParse( v.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out d ) )
				return Math.Max( 0, Math.Min( 100, (int)Math.Round( d ) ) );

			return 0;
		}

		public static string GetConfigRaw( string eventId, string keySuffix )
		{
			string v;

			if ( eventId != null && m_Config.TryGetValue( eventId + "." + keySuffix, out v ) )
				return v;
			return null;
		}

		/// <summary>
		/// Returns replacement creature typename for spawn override mapping, or null.
		/// </summary>
		public static string TryGetMobSpawnReplacement( string baseCreatureType )
		{
			if ( baseCreatureType == null || baseCreatureType.Length == 0 )
				return null;

			foreach ( string eventId in GetActiveEventIds() )
			{
				if ( !IsEventActive( eventId ) || !IsEnabled( eventId, "mob_spawn_enabled" ) )
					continue;

				string key = eventId + ".spawn_override." + baseCreatureType;
				string val;

				if ( m_Config.TryGetValue( key, out val ) && val != null && val.Trim().Length > 0 )
					return val.Trim();
			}

			return null;
		}

		/// <summary>
		/// If any active event governs boss SpawnIDs, only allow spawn when boss_spawn_enabled is true for each governing event listing this id.
		/// SpawnIDs listed by an inactive event branch are unaffected.
		/// </summary>
		public static bool AllowsBossSpawn( int spawnId )
		{
			bool anyRule = false;
			bool blocked = false;

			foreach ( string eventId in GetActiveEventIds() )
			{
				if ( !IsEventActive( eventId ) )
					continue;

				string list = GetConfigRaw( eventId, "boss_spawn_ids" );

				if ( list == null || list.Trim().Length == 0 )
					continue;

				if ( !BossListContainsSpawnId( list.Trim(), spawnId ) )
					continue;

				anyRule = true;
				if ( !IsEnabled( eventId, "boss_spawn_enabled" ) )
					blocked = true;
			}

			return !blocked || !anyRule;
		}

		private static bool BossListContainsSpawnId( string listCsv, int spawnId )
		{
			string[] ids = listCsv.Split( new char[]{ ',', '|', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );
			string needle = spawnId.ToString();

			foreach ( string s in ids )
			{
				if ( needle == s.Trim() )
					return true;
			}

			return false;
		}

		/// <summary>
		/// Effective loot multiplier (basis points relative to 10000 entry chance denominator). Uses max among active loot_override_enabled events with multiplier above 100.
		/// 100 → 10000 (neutral). Values &lt;= 0 → 10000.
		/// </summary>
		public static int GetLootChanceMultiplierBp()
		{
			int best = 100;

			foreach ( string eventId in GetActiveEventIds() )
			{
				if ( !IsEnabled( eventId, "loot_override_enabled" ) )
					continue;

				int m = GetLootMultiplierPercent( eventId );
				if ( m > best )
					best = m;
			}

			return best;
		}

		private static int GetLootMultiplierPercent( string eventId )
		{
			string v;

			if ( !m_Config.TryGetValue( eventId + ".loot_chance_multiplier", out v ) || v == null )
				return 100;
			double d;

			if ( double.TryParse( v.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out d ) )
			{
				int i = (int)Math.Round( d );

				return i <= 0 ? 100 : i;
			}

			return 100;
		}

		public static bool RollTimedLordSpeechChance()
		{
			foreach ( string eventId in GetActiveEventIds() )
			{
				if ( !IsEnabled( eventId, "rumor_enabled" ) )
					continue;

				int c = GetChance( eventId, "timelord_speech_chance" );

				if ( c > 0 && Utility.RandomDouble() < c / 100.0 )
					return true;
			}

			return false;
		}

		public static bool TavernEventRumorRangeActive()
		{
			foreach ( string eventId in GetActiveEventIds() )
			{
				if ( !IsEnabled( eventId, "rumor_enabled" ) )
					continue;

				int c = GetChance( eventId, "tavern_rumor_chance" );

				if ( c > 0 && Utility.RandomDouble() < c / 100.0 )
					return true;
			}

			return false;
		}

		public static bool RollTowncrierShoutChance( out string shoutEventId )
		{
			shoutEventId = null;

			foreach ( string eventId in GetActiveEventIds() )
			{
				if ( !IsEnabled( eventId, "lore_enabled" ) )
					continue;

				int c = GetChance( eventId, "towncrier_shout_chance" );

				if ( c <= 0 )
					continue;

				if ( Utility.RandomDouble() < c / 100.0 )
				{
					shoutEventId = eventId;
					return true;
				}
			}

			return false;
		}

		public static bool RollTowncrierBookChanceForEvent( string eventId )
		{
			if ( !IsEnabled( eventId, "lore_enabled" ) )
				return false;

			int c = GetChance( eventId, "towncrier_book_chance" );

			return c >= 100 || ( c > 0 && Utility.RandomDouble() < c / 100.0 );
		}

		public const string ThinningVeilEventIdConst = "event_phase_0";
		public const string ThinningVeilBookTitleConst = "The Thinning Veil";

		public static bool TryConsumeLoreBookDropChance( string eventId )
		{
			if ( eventId == null || !IsEnabled( eventId, "lore_enabled" ) )
				return false;

			int c = GetChance( eventId, "lorebook_drop_chance" );

			return c >= 100 || ( c > 0 && Utility.RandomDouble() < c / 100.0 );
		}
	}
}
