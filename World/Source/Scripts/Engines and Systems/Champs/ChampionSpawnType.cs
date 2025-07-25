using System;
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
					new Type[]{ typeof( GreaterMongbat ), typeof( Imp ) },			// Level 1
					new Type[]{ typeof( Gargoyle ), typeof( Harpy ) },				// Level 2
					new Type[]{ typeof( FireGargoyle ), typeof( StoneGargoyle ) },	// Level 3
					new Type[]{ typeof( Daemon ), typeof( Succubus ) }				// Level 4
				} ),
				new ChampionSpawnInfo( "Arachnid", typeof( Mephitis ), new Type[][]		// Arachnid
				{																		// Arachnid
					new Type[]{ typeof( Scorpion ), typeof( GiantSpider ) },			// Level 1
					new Type[]{ typeof( TerathanDrone ), typeof( TerathanWarrior ) },	// Level 2
					new Type[]{ typeof( DreadSpider ), typeof( TerathanMatriarch ) },	// Level 3
					new Type[]{ typeof( PoisonElemental ), typeof( TerathanAvenger ) }	// Level 4
				} ),
				new ChampionSpawnInfo( "Cold Blood", typeof( Rikktor ), new Type[][]	// Cold Blood
				{																		// Cold Blood
					new Type[]{ typeof( Lizardman ), typeof( Snake ) },					// Level 1
					new Type[]{ typeof( LavaLizard ), typeof( OphidianWarrior ) },		// Level 2
					new Type[]{ typeof( Drake ), typeof( OphidianArchmage ) },			// Level 3
					new Type[]{ typeof( Dragons ), typeof( OphidianKnight ) }			// Level 4
				} ),
				new ChampionSpawnInfo( "Forest Lord", typeof( LordOaks ), new Type[][]	// Forest Lord
				{																		// Forest Lord
					new Type[]{ typeof( Pixie ), typeof( ShadowWisp ) },				// Level 1
					new Type[]{ typeof( Kirin ), typeof( Wisp ) },						// Level 2
					new Type[]{ typeof( Centaur ), typeof( Unicorn ) },					// Level 3
					new Type[]{ typeof( EtherealWarrior ), typeof( SerpentineDragon ) }	// Level 4
				} ),
				new ChampionSpawnInfo( "Vermin Horde", typeof( Barracoon ), new Type[][]	// Vermin Horde
				{																			// Vermin Horde
					new Type[]{ typeof( GiantRat ), typeof( Slime ) },						// Level 1
					new Type[]{ typeof( WolfDire ), typeof( Ratman ) },						// Level 2
					new Type[]{ typeof( HellHound ), typeof( RatmanMage ) },				// Level 3
					new Type[]{ typeof( RatmanArcher ), typeof( SilverSerpent ) }			// Level 4
				} ),
				new ChampionSpawnInfo( "Unholy Terror", typeof( Neira ), new Type[][]						// Unholy Terror
				{																							// Unholy Terror
					new Type[]{ typeof( Ghoul ), typeof( Shade ), typeof( Spectre ), typeof( Wraith ) },	// Level 1
					new Type[]{ typeof( BoneMagi ), typeof( Mummy ), typeof( SkeletalMage ) },				// Level 2
					new Type[]{ typeof( BoneKnight ), typeof( Lich ), typeof( SkeletalKnight ) },			// Level 3
					new Type[]{ typeof( LichLord ), typeof( RottingCorpse ) }								// Level 4
				} ),
			};

		public static ChampionSpawnInfo GetInfo(ChampionSpawnType type)
		{
			int v = (int)type;

			if (v < 0 || v >= m_Table.Length)
				v = 0;

			return m_Table[v];
		}
	}
}