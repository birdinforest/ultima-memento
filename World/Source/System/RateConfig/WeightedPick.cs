using System;
using System.Collections.Generic;

namespace Server.RateConfig
{
	/// <summary>
	/// Generic weighted-random helpers shared by any <see cref="RateConfigEngine"/> consumer.
	/// <see cref="LootPack"/> and <see cref="Server.SpawnGroup"/> each hand-roll their own subtractive
	/// weighted pick inline; this class centralizes the same algorithm for new config-driven callers
	/// so it is written (and fixed, if ever buggy) exactly once.
	/// </summary>
	public static class WeightedPick
	{
		/// <summary>
		/// Subtractive weighted random pick over a name -&gt; weight map (e.g. from
		/// <see cref="RateConfigEngine.GetTable"/>). Entries with a weight &lt;= 0 are never picked.
		/// Returns null if <paramref name="weights"/> is null/empty or every weight is &lt;= 0.
		/// </summary>
		public static string Pick( Dictionary<string, double> weights )
		{
			if ( weights == null || weights.Count == 0 )
				return null;

			double total = 0.0;

			foreach ( var kv in weights )
			{
				if ( kv.Value > 0.0 )
					total += kv.Value;
			}

			if ( total <= 0.0 )
				return null;

			double roll = Utility.RandomDouble() * total;

			foreach ( var kv in weights )
			{
				if ( kv.Value <= 0.0 )
					continue;

				if ( roll < kv.Value )
					return kv.Key;

				roll -= kv.Value;
			}

			// Floating-point edge case (roll landed exactly on the running total) — return the last
			// positive-weight entry seen rather than null, so callers always get a value when total > 0.
			string last = null;

			foreach ( var kv in weights )
			{
				if ( kv.Value > 0.0 )
					last = kv.Key;
			}

			return last;
		}

		/// <summary>
		/// True with probability <paramref name="chance"/> (0..1). Used for reject/keep gates such as
		/// "keep this rare breed" vs. "reroll from the same pool".
		/// </summary>
		public static bool KeepChance( double chance )
		{
			if ( chance >= 1.0 )
				return true;

			if ( chance <= 0.0 )
				return false;

			return Utility.RandomDouble() < chance;
		}
	}
}
