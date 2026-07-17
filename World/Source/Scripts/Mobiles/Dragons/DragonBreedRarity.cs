using System.Collections.Generic;
using Server.RateConfig;

namespace Server.Mobiles
{
	/// <summary>
	/// Shared "Bright" (glowing, visually rare) breed-rarity gate for the 146-entry dragon breed
	/// table used identically by <see cref="RidingDragon"/>, <see cref="Dragons"/>, and
	/// <see cref="Wyrms"/> CreateDragon(). Only wild spawns are adjusted — dragons hatched from a
	/// <see cref="DragonEgg"/> lock their breed via Hue before CreateDragon runs and must bypass
	/// this gate entirely (callers check that themselves; see the Hue &gt; 0 branch in each caller).
	///
	/// GemDragon is not part of this table — its Resource (scale) is picked separately and drives
	/// its visible Hue directly, so it is gated via "gemdragon.scaleWeight.*" in <see cref="GemDragon"/>
	/// instead of this "dragon.breedWeight.*" table.
	///
	/// Config: World/Data/RateConfig/dragon-rarity.json, keys "dragon.breedWeight.&lt;name&gt;"
	/// (0..1 keep-chance for that Bright breed) plus "dragon.breedWeight.default" for every other
	/// (common, non-Bright) breed id. See World/Documentation/rate-config-system.md.
	/// </summary>
	public static class DragonBreedRarity
	{
		private const string ConfigPrefix = "dragon.breedWeight.";
		private const string DefaultKey = ConfigPrefix + "default";
		private const int MaxRerollAttempts = 8;

		private static readonly Dictionary<int, string> m_BrightBreeds = new Dictionary<int, string>
		{
			{ 5, "glare" }, { 6, "glaze" }, { 7, "radiant" }, { 22, "bright" }, { 31, "jadefire" },
			{ 38, "rubystar" }, { 54, "redstar" }, { 74, "burnt" }, { 75, "fire" }, { 76, "firelight" },
			{ 77, "lava" }, { 79, "magma" }, { 80, "vulcan" }, { 82, "cinder" }, { 83, "darkfire" },
			{ 84, "flare" }, { 85, "hell" }, { 86, "firerock" }, { 97, "nova" }, { 104, "solar" },
			{ 105, "star" }, { 106, "sun" }, { 132, "ice" }, { 133, "icescale" }, { 144, "swampfire" },
			{ 146, "xormite" }
		};

		/// <summary>
		/// Gates a wild-spawn breed candidate against its configured keep-chance. Non-Bright ids pass
		/// through unchanged. A rejected Bright id is rerolled from the same terrain pool (up to
		/// <see cref="MaxRerollAttempts"/> times); the radiation pool is 100% Bright (see risk note
		/// in rate-config-system.md), so after a couple of failed rerolls we escape into the dungeon
		/// pool so the gate can still land on a common breed instead of spinning until the cap.
		/// </summary>
		public static int AdjustWildBreed( int candidateId, string terrain )
		{
			int id = candidateId;
			string rerollTerrain = terrain;

			for ( int attempt = 0; attempt < MaxRerollAttempts; ++attempt )
			{
				string name;

				if ( !m_BrightBreeds.TryGetValue( id, out name ) )
					return id; // common breed — never gated

				double keep = RateConfigEngine.GetDouble( ConfigPrefix + name, RateConfigEngine.GetDouble( DefaultKey, 1.0 ) );

				if ( WeightedPick.KeepChance( keep ) )
					return id;

				if ( rerollTerrain == "radiation" && attempt >= 2 )
					rerollTerrain = "dungeon";

				id = PickRawBreed( rerollTerrain );
			}

			return id; // give up after the cap (e.g. an all-Bright pool); accept whatever we land on
		}

		/// <summary>
		/// Mirrors the terrain -&gt; breed-id pool selection inlined identically in
		/// RidingDragon/Dragons/Wyrms CreateDragon(). Kept here as the single source of truth used by
		/// <see cref="AdjustWildBreed"/>'s reroll — the initial per-file roll is left untouched in each
		/// caller so this refactor cannot change first-roll behavior, only the accept/reject step.
		/// </summary>
		public static int PickRawBreed( string terrain )
		{
			int dragon = Utility.RandomMinMax( 1, 145 ); // 146 is omitted — Xormite is Bright-only

			if ( terrain == "swamp" ) dragon = Utility.RandomMinMax( 139, 145 );
			else if ( terrain == "fire" ) dragon = Utility.RandomMinMax( 74, 87 );
			else if ( terrain == "snow" ) dragon = Utility.RandomMinMax( 131, 138 );
			else if ( terrain == "sea" )
			{
				dragon = Utility.RandomMinMax( 120, 130 );
				if ( Utility.RandomMinMax( 1, 20 ) == 1 ) dragon = 16;
			}
			else if ( terrain == "radiation" ) dragon = Utility.RandomList( 5, 6, 7, 54, 97, 104, 106, 146 );
			else if ( terrain == "jungle" ) dragon = Utility.RandomList( 89, 90, 93, 95, 96 );
			else if ( terrain == "forest" ) dragon = Utility.RandomMinMax( 88, 94 );
			else if ( terrain == "sand" ) dragon = Utility.RandomMinMax( 112, 119 );
			else if ( terrain == "mountain" ) dragon = Utility.RandomList( 109, 110, 111, 116 );
			else if ( terrain == "dungeon" ) dragon = Utility.RandomMinMax( 1, 73 );
			else if ( terrain == "land" ) dragon = Utility.RandomMinMax( 97, 108 );
			else if ( terrain == "sky" ) dragon = Utility.RandomList( 7, 22, 33, 66, 97, 99, 101, 104, 105, 106, 107 );

			return dragon;
		}
	}
}
