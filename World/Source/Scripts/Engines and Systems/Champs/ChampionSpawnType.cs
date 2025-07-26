using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.CannedEvil
{
	public enum ChampionSpawnType
	{
		Abyss,
		Arachnid,
		ColdBlood,
		ForestLord,
		VerminHorde,
		UnholyTerror,
	}

	public class ChampionSpawnInfo
	{
		private string m_Name;
		private Type m_Champion;
		private Type[][] m_SpawnTypes;

		public string Name { get { return m_Name; } }
		public Type Champion { get { return m_Champion; } }
		public Type[][] SpawnTypes { get { return m_SpawnTypes; } }

		public ChampionSpawnInfo(string name, Type champion, Type[][] spawnTypes)
		{
			m_Name = name;
			m_Champion = champion;
			m_SpawnTypes = spawnTypes;
		}

		public static ChampionSpawnInfo[] Table { get { return m_Table; } }

		private static readonly ChampionSpawnInfo[] m_Table = new ChampionSpawnInfo[]
			{
				new ChampionSpawnInfo( "Abyss", typeof( Semidar ), new Type[][]		// Abyss
				{																	// Abyss
					new Type[]{ typeof( GreaterMongbat ), typeof( Imp ) },			// Level 1 - AnimalHunter, Exorcism
					new Type[]{ typeof( Gargoyle ), typeof( Harpy ) },				// Level 2 - GargoylesFoe / Exorcism, AvianHunter
					new Type[]{ typeof( FireGargoyle ), typeof( StoneGargoyle ) },	// Level 3 - GargoylesFoe / FlameDousing / Exorcism
					new Type[]{ typeof( Daemon ), typeof( Succubus ) }				// Level 4 - Exorcism / DaemonDismissal, BloodDrinking / Exorcism
				} ),
				new ChampionSpawnInfo( "Arachnid", typeof( Mephitis ), new Type[][]		// Arachnid
				{																		// Arachnid
					new Type[]{ typeof( Scorpion ), typeof( GiantSpider ) },			// Level 1 - ElementalHealth / ArachnidDoom / ScorpionsBane, ArachnidDoom / SpidersDeath
					new Type[]{ typeof( TerathanDrone ), typeof( TerathanWarrior ) },	// Level 2 - ArachnidDoom / Terathan
					new Type[]{ typeof( DreadSpider ), typeof( TerathanMatriarch ) },	// Level 3 - ArachnidDoom / SpidersDeath, ArachnidDoom / Terathan
					new Type[]{ typeof( PoisonElemental ), typeof( TerathanAvenger ) }	// Level 4 - ElementalBan / ElementalHealth, ArachnidDoom / Terathan
				} ),
				new ChampionSpawnInfo( "Cold Blood", typeof( Rikktor ), new Type[][]	// Cold Blood
				{																		// Cold Blood
					new Type[]{ typeof( Lizardman ), typeof( Snake ) },					// Level 1 - LizardmanSlaughter, SnakesBane
					new Type[]{ typeof( LavaLizard ), typeof( OphidianWarrior ) },		// Level 2 - FlameDousing, Ophidian / ReptilianDeath
					new Type[]{ typeof( Drake ), typeof( OphidianArchmage ) },			// Level 3 - DragonSlaying / ReptilianDeath, Ophidian / ReptilianDeath
					new Type[]{ typeof( Dragons ), typeof( OphidianKnight ) }			// Level 4 - DragonSlaying / ReptilianDeath, Ophidian / ReptilianDeath
				} ),
				new ChampionSpawnInfo( "Forest Lord", typeof( LordOaks ), new Type[][]	// Forest Lord
				{																		// Forest Lord
					new Type[]{ typeof( Pixie ), typeof( ShadowWisp ) },				// Level 1 - Fey
					new Type[]{ typeof( Kirin ), typeof( Wisp ) },						// Level 2 - Fey
					new Type[]{ typeof( Centaur ), typeof( Unicorn ) },					// Level 3 - Fey / AnimalHunter
					new Type[]{ typeof( EtherealWarrior ), typeof( SerpentineDragon ) }	// Level 4 - Fey, DragonSlaying / ReptilianDeath
				} ),
				new ChampionSpawnInfo( "Vermin Horde", typeof( Barracoon ), new Type[][]	// Vermin Horde
				{																			// Vermin Horde
					new Type[]{ typeof( GiantRat ), typeof( Slime ) },						// Level 1 - AnimalHunter, SlimyScourge
					new Type[]{ typeof( WolfDire ), typeof( Ratman ) },						// Level 2 - AnimalHunter, Repond
					new Type[]{ typeof( HellHound ), typeof( RatmanMage ) },				// Level 3 - AnimalHunter, Repond / WizardSlayer
					new Type[]{ typeof( RatmanArcher ), typeof( SilverSerpent ) }			// Level 4 - Repond, ReptilianDeath / SnakesBane
				} ),
				new ChampionSpawnInfo( "Unholy Terror", typeof( Neira ), new Type[][]						// Unholy Terror
				{																							// Unholy Terror
					new Type[]{ typeof( Ghoul ), typeof( Shade ), typeof( Spectre ), typeof( Wraith ) },	// Level 1 - Silver
					new Type[]{ typeof( BoneMagi ), typeof( Mummy ), typeof( SkeletalMage ) },				// Level 2 - Silver / WizardSlayer
					new Type[]{ typeof( BoneKnight ), typeof( Lich ), typeof( SkeletalKnight ) },			// Level 3 - Silver / WizardSlayer
					new Type[]{ typeof( LichLord ), typeof( RottingCorpse ) }								// Level 4 - Silver / WizardSlayer
				} ),
			};

		public static ChampionSpawnInfo GetInfo(ChampionSpawnType type)
		{
			int v = (int)type;

			if (v < 0 || v >= m_Table.Length)
				v = 0;

			return m_Table[v];
		}

		public static string GetName(ChampionSpawnType type)
		{
			switch (type)
			{
				case ChampionSpawnType.ColdBlood: return "Cold Blood";
				case ChampionSpawnType.ForestLord: return "Forest Lord";
				case ChampionSpawnType.Arachnid: return "Arachnid";
				case ChampionSpawnType.VerminHorde: return "Vermin Horde";
				case ChampionSpawnType.UnholyTerror: return "Unholy Terror";
				case ChampionSpawnType.Abyss: return "Abyss";
			}

			return "Unknown";
		}

		public static string GetChampionName(ChampionSpawnType type)
		{
			switch (type)
			{
				case ChampionSpawnType.ColdBlood: return "Rikktor";
				case ChampionSpawnType.ForestLord: return "Lord Oaks";
				case ChampionSpawnType.Arachnid: return "Mephitis";
				case ChampionSpawnType.VerminHorde: return "Barracoon";
				case ChampionSpawnType.UnholyTerror: return "Neira";
				case ChampionSpawnType.Abyss: return "Semidar";
			}

			return "Unknown";
		}

		public static IEnumerable<SlayerName> GetRelevantSlayers(ChampionSpawnType type)
		{
			switch (type)
			{
				case ChampionSpawnType.ColdBlood:
					return new SlayerName[]
					{
						SlayerName.DragonSlaying,
						SlayerName.LizardmanSlaughter,
						SlayerName.Ophidian,
						SlayerName.ReptilianDeath,
						SlayerName.SnakesBane,
					};

				case ChampionSpawnType.ForestLord:
					return new SlayerName[]
					{
						SlayerName.AnimalHunter,
						SlayerName.DragonSlaying,
						SlayerName.Fey,
						SlayerName.ReptilianDeath,
					};

				case ChampionSpawnType.Arachnid:
					return new SlayerName[]
					{
						SlayerName.ArachnidDoom,
						SlayerName.ElementalBan,
						SlayerName.ElementalHealth,
						SlayerName.ScorpionsBane,
						SlayerName.SpidersDeath,
						SlayerName.Terathan,
					};

				case ChampionSpawnType.VerminHorde:
					return new SlayerName[]
					{
						SlayerName.AnimalHunter,
						SlayerName.Repond,
						SlayerName.ReptilianDeath,
						SlayerName.SlimyScourge,
						SlayerName.SnakesBane,
						SlayerName.WizardSlayer,
					};

				case ChampionSpawnType.UnholyTerror:
					return new SlayerName[]
					{
						SlayerName.Silver,
						SlayerName.WizardSlayer,
					};

				case ChampionSpawnType.Abyss:
					return new SlayerName[]
					{
						SlayerName.AnimalHunter,
						SlayerName.AvianHunter,
						SlayerName.BloodDrinking,
						SlayerName.DaemonDismissal,
						SlayerName.DaemonDismissal,
						SlayerName.Exorcism,
						SlayerName.FlameDousing,
						SlayerName.GargoylesFoe,
					};

				default:
					return null;
			}
		}
	}
}