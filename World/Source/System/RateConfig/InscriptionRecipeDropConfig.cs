using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Server.Localization;

namespace Server.RateConfig
{
	/// <summary>
	/// JSON-driven config for inscription advanced-recipe (5th–8th circle) mob drops.
	/// Numeric tuning lives in <c>Data/RateConfig/inscription-recipe-drop.json</c> (also merged
	/// into <see cref="RateConfigEngine"/>). Scroll type lists live in
	/// <c>Data/InscriptionRecipeDrop/tier-scrolls.json</c> (string values only).
	/// Hot-reload via <c>[ratereload</c> together with other RateConfig files.
	/// </summary>
	public sealed class InscriptionEnemyTierEntry
	{
		public string Id;
		public int MinFame;
		public int[] PoolWeightByTier = new int[5];
		public double Rank1MaxPct;
		public double Rank2MaxPct;
		public double Rank3MaxPct;

		public double GetRankMaxPct( int rank )
		{
			switch ( rank )
			{
				case 1: return Rank1MaxPct;
				case 2: return Rank2MaxPct;
				case 3: return Rank3MaxPct;
				default: return 0;
			}
		}
	}

	public static class InscriptionRecipeDropConfig
	{
		private static readonly object m_Lock = new object();
		private static bool m_Loaded;

		private static int m_MinFame = 5000;
		private static int m_TopN = 3;
		private static int m_Range = 20;
		private static int m_LuckCap = 2000;

		private static Type[][] m_TierTypes = new Type[5][];
		private static InscriptionEnemyTierEntry[] m_EnemyTiers = new InscriptionEnemyTierEntry[0];

		public static int MinFame { get { EnsureLoaded(); lock ( m_Lock ) { return m_MinFame; } } }
		public static int TopN { get { EnsureLoaded(); lock ( m_Lock ) { return m_TopN; } } }
		public static int Range { get { EnsureLoaded(); lock ( m_Lock ) { return m_Range; } } }
		public static int LuckCap { get { EnsureLoaded(); lock ( m_Lock ) { return m_LuckCap; } } }

		public static void Load()
		{
			lock ( m_Lock )
			{
				var raw = new Dictionary<string, string>( StringComparer.Ordinal );

				MergeJsonFile( Path.Combine( Core.BaseDirectory, "Data/RateConfig/inscription-recipe-drop.json" ), raw );
				MergeJsonFile( Path.Combine( Core.BaseDirectory, "Data/InscriptionRecipeDrop/tier-scrolls.json" ), raw );

				m_MinFame = ParseInt( raw, "inscription.drop.minFame", 5000 );
				m_TopN = ParseInt( raw, "inscription.drop.topN", 3 );
				m_Range = ParseInt( raw, "inscription.drop.range", 20 );
				m_LuckCap = ParseInt( raw, "inscription.drop.luckCap", 2000 );

				var tierTypes = new Type[5][];

				for ( int tier = 1; tier <= 4; tier++ )
				{
					string key = "inscription.tier.T" + tier + ".types";
					string csv;

					if ( raw.TryGetValue( key, out csv ) && !string.IsNullOrEmpty( csv ) )
						tierTypes[tier] = ResolveTypeList( csv, key );
					else
						tierTypes[tier] = new Type[0];
				}

				m_TierTypes = tierTypes;
				m_EnemyTiers = ParseEnemyTiers( raw );

				m_Loaded = true;

				Console.WriteLine(
					"InscriptionRecipeDropConfig: loaded {0} enemy tiers; T1={1} T2={2} T3={3} T4={4} scroll types.",
					m_EnemyTiers.Length,
					LengthOrZero( tierTypes[1] ),
					LengthOrZero( tierTypes[2] ),
					LengthOrZero( tierTypes[3] ),
					LengthOrZero( tierTypes[4] ) );
			}
		}

		public static void Reload()
		{
			Load();
		}

		public static Type[] GetTierTypes( int tier )
		{
			EnsureLoaded();

			lock ( m_Lock )
			{
				if ( tier < 1 || tier >= m_TierTypes.Length )
					return null;

				Type[] types = m_TierTypes[tier];

				return types != null && types.Length > 0 ? types : null;
			}
		}

		public static InscriptionEnemyTierEntry GetEnemyTier( int fame )
		{
			EnsureLoaded();

			lock ( m_Lock )
			{
				if ( fame < m_MinFame )
					return null;

				for ( int i = 0; i < m_EnemyTiers.Length; i++ )
				{
					InscriptionEnemyTierEntry entry = m_EnemyTiers[i];

					if ( entry != null && fame >= entry.MinFame )
						return entry;
				}

				return null;
			}
		}

		public static int PickTier( InscriptionEnemyTierEntry entry )
		{
			if ( entry == null )
				return 1;

			var weights = new Dictionary<string, double>( 4 );

			for ( int tier = 1; tier <= 4; tier++ )
			{
				int weight = entry.PoolWeightByTier[tier];

				if ( weight > 0 )
					weights["T" + tier] = weight;
			}

			string picked = WeightedPick.Pick( weights );

			if ( picked != null && picked.Length > 1 && picked[0] == 'T' )
			{
				if ( int.TryParse( picked.Substring( 1 ), NumberStyles.Integer, CultureInfo.InvariantCulture, out int tier ) && tier >= 1 && tier <= 4 )
					return tier;
			}

			return 1;
		}

		public static double GetRollActualPct( int luck, int rank, InscriptionEnemyTierEntry entry )
		{
			if ( entry == null || rank < 1 || rank > 3 )
				return 0;

			double maxPct = entry.GetRankMaxPct( rank );
			int cappedLuck = luck;

			if ( cappedLuck < 0 )
				cappedLuck = 0;

			lock ( m_Lock )
			{
				if ( cappedLuck > m_LuckCap )
					cappedLuck = m_LuckCap;

				return cappedLuck * maxPct / (double)m_LuckCap;
			}
		}

		public static bool RollDropChance( int luck, int rank, InscriptionEnemyTierEntry entry )
		{
			double actualPct = GetRollActualPct( luck, rank, entry );

			if ( actualPct <= 0 )
				return false;

			int threshold = (int)( actualPct * 100.0 );

			if ( threshold <= 0 )
				return false;

			return Utility.RandomMinMax( 1, 10000 ) <= threshold;
		}

		private static void EnsureLoaded()
		{
			if ( !m_Loaded )
				Load();
		}

		private static void MergeJsonFile( string path, Dictionary<string, string> target )
		{
			if ( !File.Exists( path ) )
			{
				Console.WriteLine( "InscriptionRecipeDropConfig: missing {0} — using baked-in defaults where needed.", path );
				return;
			}

			try
			{
				string json = File.ReadAllText( path );
				SimpleJsonObject.ParseStringProperties( json, target );
			}
			catch ( Exception ex )
			{
				Console.WriteLine( "InscriptionRecipeDropConfig: failed to load {0}: {1}", path, ex.Message );
			}
		}

		private static InscriptionEnemyTierEntry[] ParseEnemyTiers( Dictionary<string, string> raw )
		{
			var ids = new HashSet<string>( StringComparer.Ordinal );
			const string prefix = "inscription.enemy.";
			const string suffix = ".minFame";

			foreach ( KeyValuePair<string, string> kv in raw )
			{
				if ( kv.Key.StartsWith( prefix, StringComparison.Ordinal ) && kv.Key.EndsWith( suffix, StringComparison.Ordinal ) )
				{
					string id = kv.Key.Substring( prefix.Length, kv.Key.Length - prefix.Length - suffix.Length );

					if ( id.Length > 0 )
						ids.Add( id );
				}
			}

			var list = new List<InscriptionEnemyTierEntry>();

			foreach ( string id in ids )
			{
				string baseKey = prefix + id + ".";
				var entry = new InscriptionEnemyTierEntry
				{
					Id = id,
					MinFame = ParseInt( raw, baseKey + "minFame", 0 ),
					Rank1MaxPct = ParseDouble( raw, baseKey + "rank1MaxPct", 0 ),
					Rank2MaxPct = ParseDouble( raw, baseKey + "rank2MaxPct", 0 ),
					Rank3MaxPct = ParseDouble( raw, baseKey + "rank3MaxPct", 0 )
				};

				for ( int tier = 1; tier <= 4; tier++ )
					entry.PoolWeightByTier[tier] = ParseInt( raw, baseKey + "pool.T" + tier, 0 );

				if ( entry.MinFame > 0 )
					list.Add( entry );
			}

			list.Sort( ( a, b ) => b.MinFame.CompareTo( a.MinFame ) );

			return list.ToArray();
		}

		private static Type[] ResolveTypeList( string csv, string configKey )
		{
			var result = new List<Type>();
			string[] parts = csv.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

			for ( int i = 0; i < parts.Length; i++ )
			{
				string name = parts[i].Trim();

				if ( name.Length == 0 )
					continue;

				Type type = ScriptCompiler.FindTypeByName( name );

				if ( type == null )
				{
					Console.WriteLine( "InscriptionRecipeDropConfig: unknown scroll type \"{0}\" in {1} — skipped.", name, configKey );
					continue;
				}

				result.Add( type );
			}

			return result.ToArray();
		}

		private static int ParseInt( Dictionary<string, string> raw, string key, int defaultValue )
		{
			string text;

			if ( raw.TryGetValue( key, out text ) && int.TryParse( text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value ) )
				return value;

			return defaultValue;
		}

		private static double ParseDouble( Dictionary<string, string> raw, string key, double defaultValue )
		{
			string text;

			if ( raw.TryGetValue( key, out text ) && double.TryParse( text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value ) )
				return value;

			return defaultValue;
		}

		private static int LengthOrZero( Type[] types )
		{
			return types != null ? types.Length : 0;
		}

		[CallPriority( -149 )]
		public static void Initialize()
		{
			Load();
		}
	}
}
