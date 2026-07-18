using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using Server.Targeting;

namespace Server.Misc
{
	// Epic Tribute P1 Solution B: a personal, temporary SetDifficultyForMonster-strengthened
	// encounter that gates the paid EpicCharacter Tribute path's SummonItems key.
	//
	// Design reference: uo-dev-documentations/memento/game-design-idea/
	// EPIC_TRIBUTE_ACQUISITION_DIFFICULTY_REDESIGN.md section 6.2 / 6.2.1 (P1 Solution B, confirmed 2026-07-16).
	//
	// This does NOT touch the shared SummonCarriers.cs 59-mob pool (still used, unchanged, by
	// Magical Prison and any player not on the Epic Tribute path). Instead it spawns a separate,
	// personal, temporary guardian at a random passable location within the region tied to the
	// requesting player's currently assigned Epic Tribute key (2026-07-18: region-wide random
	// placement via BaseRegion.RandomSpawnLocation, steered away from the original named mob's
	// fixed SummonCarriers.cs Home coordinate when one exists -- see PickRandomChallengeLocation),
	// and packs a SummonItems tagged EpicChallengeSource = true -- the only kind of SummonItems
	// that satisfies EpicCharacter.HaveSpecialItemRequirement().
	public static class EpicTributeChallenge
	{
		public class ChallengeTemplate
		{
			public string RegionName;
			public Type MonsterType;
			public string NameOverride;
			public string TitleOverride;
			public int Hue;
			public int Body;
			public int ItemId;
			public int ItemHue;

			// The original SummonCarriers.cs named mob's fixed Home coordinate for this dungeon,
			// when one exists (SummonQuests.IsInLocation check) -- used only to steer the personal
			// challenge's random spawn point away from it. (0, 0) means the original branch has no
			// fixed-location check (any matching live monster type in the region qualifies), so
			// there is nothing to avoid.
			public int HomeX;
			public int HomeY;

			public ChallengeTemplate( string regionName, Type monsterType, string nameOverride, string titleOverride, int hue, int body, int itemId, int itemHue, int homeX, int homeY )
			{
				RegionName = regionName;
				MonsterType = monsterType;
				NameOverride = nameOverride;
				TitleOverride = titleOverride;
				Hue = hue;
				Body = body;
				ItemId = itemId;
				ItemHue = itemHue;
				HomeX = homeX;
				HomeY = homeY;
			}
		}

		// Keyed by the exact SummonItems.Name used both by SummonPrison.GetItemNeeded(choice, 3)
		// (player-visible "symbol of your valor" requirement text) and by the matching
		// SummonCarriers.cs branch (region + live monster type + cosmetic overrides). Cross-
		// referenced 1:1 against SummonCarriers.cs, items 1-59 (item 60, "scale of Scarthis", is
		// outside Epic Tribute's Utility.RandomMinMax(1, 59) range and is intentionally omitted).
		private static readonly Dictionary<string, ChallengeTemplate> Templates = new Dictionary<string, ChallengeTemplate>()
		{
			{ "heart of ash", new ChallengeTemplate( "Stonegate Castle", typeof( AshDragon ), null, null, 0, 0, 0xF91, 0x76C, 0, 0 ) },
			{ "mystical wax", new ChallengeTemplate( "the Vault of the Black Knight", typeof( WaxSculpture ), "a mystical wax golem", null, 0, 0, 0x1422, 0x490, 6421, 237 ) },
			{ "vampire teeth", new ChallengeTemplate( "the Crypts of Dracula", typeof( VampirePrince ), null, "the son of Dracula", 0, 0, 0x5738, 0x47E, 5741, 2788 ) },
			{ "face of the ancient king", new ChallengeTemplate( "the Lodoria Catacombs", typeof( RottingCorpse ), null, "of the ancient king", 0, 0, 0x1CE1, 0, 5502, 1806 ) },
			{ "wand of Talosh", new ChallengeTemplate( "Dungeon Deceit", typeof( LichLord ), "Talosh", "the wizard of fear", 0, 0, 0xDF4, 0, 5318, 749 ) },
			{ "head of Urg", new ChallengeTemplate( "Dungeon Despise", typeof( Troll ), "Urg", "the troll warlord", 0xA50, 0, 0x0919, 0xA50, 5503, 921 ) },
			{ "flame of Dramulox", new ChallengeTemplate( "Dungeon Destard", typeof( ShadowWyrm ), "Dramulox", "of the shadows", 0, 0, 0xDE3, 0, 5132, 852 ) },
			{ "crown of Vorgol", new ChallengeTemplate( "the City of Embers", typeof( LichLord ), "Vorgol", "the baron of flame", 0x9C6, 0, 0x3166, 0x9C6, 5667, 1314 ) },
			{ "claw of Saramon", new ChallengeTemplate( "Dungeon Hythloth", typeof( Daemon ), "Saramon", "the slayer of souls", 0x9C6, 9, 0x5721, 0x9C6, 6111, 84 ) },
			{ "horn of the frozen hells", new ChallengeTemplate( "the Ice Fiend Lair", typeof( Daemon ), null, "of the frozen hells", 0, 88, 0x2DB7, 0x480, 5672, 326 ) },
			{ "elemental salt", new ChallengeTemplate( "Dungeon Shame", typeof( WaterElemental ), "a salt water elemental", null, 0x48D, 0, 0x423A, 0x47E, 5596, 219 ) },
			{ "eye of plagues", new ChallengeTemplate( "Terathan Keep", typeof( Dragons ), null, "the dragon of blight", 0x9C4, 0, 0x3199, 0x9C9, 5307, 1611 ) },
			{ "hair of the earth", new ChallengeTemplate( "the Halls of Undermountain", typeof( WeedElemental ), "a tangle weed", null, 0, 0, 0xCB0, 0, 5332, 478 ) },
			{ "skull of Turlox", new ChallengeTemplate( "the Volcanic Cave", typeof( FireGiant ), "Turlox", "the warlord of the sun", 0xB73, 0, 0x2203, 0x54F, 5994, 3414 ) },
			{ "tattered robe of Mezlo", new ChallengeTemplate( "the Mausoleum", typeof( AncientLich ), "Mezlo", "of the green death", 0x58B, 0, 0x3174, 0x54F, 3827, 3299 ) },
			{ "blood of the forest", new ChallengeTemplate( "the Tower of Brass", typeof( Daemon ), null, "of the dark forest", 0xA60, 0, 0x122A, 0xA60, 6519, 3572 ) },
			{ "cinders of life", new ChallengeTemplate( "Vordo's Dungeon", typeof( MagmaElemental ), "a magma flow", null, 0x550, 0, 0x223A, 0x550, 6470, 466 ) },
			{ "crystal scales", new ChallengeTemplate( "the Dragon's Maw", typeof( CrystalDragon ), null, null, 0, 0, 0x2248, 0xA0B, 4498, 3924 ) },
			{ "chest of suffering", new ChallengeTemplate( "the Ancient Pyramid", typeof( Lich ), null, "the pharaoh of suffering", 0x9C7, 0, 0x1B17, 0x9C7, 5325, 957 ) },
			{ "whip from below", new ChallengeTemplate( "Dungeon Exodus", typeof( Daemon ), null, "the torturer from below", 0x9D3, 9, 0x166E, 0, 5944, 628 ) },
			{ "scale of the sea", new ChallengeTemplate( "the Caverns of Poseidon", typeof( WaterNaga ), null, "the naga from the deep", 0xA09, 0, 0x26B5, 0xA09, 5902, 1769 ) },
			{ "braclet of war", new ChallengeTemplate( "Dungeon Clues", typeof( Titan ), "Marxas", "the titan of war", 0, 0, 0x4212, 0x9D3, 5971, 2232 ) },
			{ "stump of the ancients", new ChallengeTemplate( "Dardin's Pit", typeof( WalkingReaper ), null, "the ancient reaper", 0, 0, 0xE57, 0, 5616, 400 ) },
			{ "dark blood", new ChallengeTemplate( "Dungeon Doom", typeof( BloodElemental ), "a dark blood elemental", null, 0x5B5, 0, 0x122D, 0x5B5, 5325, 331 ) },
			{ "firescale tooth", new ChallengeTemplate( "the Fires of Hell", typeof( Drake ), "a firescale drake", null, 0x54C, 0, 0x5747, 0x54C, 5712, 1280 ) },
			{ "ichor of Xthizx", new ChallengeTemplate( "the Mines of Morinia", typeof( AntaurKing ), null, null, 0, 0, 0x2827, 0xB96, 0, 0 ) },
			{ "heart of a vampire queen", new ChallengeTemplate( "the Perinian Depths", typeof( VampireLord ), null, "the vampire queen", 0, 0, 0x24B, 0, 5918, 419 ) },
			{ "hourglass of ages", new ChallengeTemplate( "the Dungeon of Time Awaits", typeof( Daemon ), null, "the daemon of ages", 0xA65, 9, 0x1810, 0xB90, 5736, 793 ) },
			{ "shackles of Saramak", new ChallengeTemplate( "the Ancient Prison", typeof( DeadWizard ), "Saramak", "the forgotten prisoner", 0, 0x190, 0x1262, 0, 1928, 569 ) },
			{ "mouth of embers", new ChallengeTemplate( "the Cave of Fire", typeof( Dragons ), null, "the dragon of embers", 0x501, 0, 0x2DB4, 0x501, 2052, 911 ) },
			{ "cowl of shadegloom", new ChallengeTemplate( "the Cave of Souls", typeof( RottingCorpse ), "a zombie", "of the shadegloom thief", 0, 0, 0x278F, 0, 2466, 153 ) },
			{ "wedding dress of virtue", new ChallengeTemplate( "Dungeon Ankh", typeof( DeadWizard ), null, "the dutchess of virtue", 0, 0x191, 0x1F00, 0, 2044, 174 ) },
			{ "lilly pad of the bog", new ChallengeTemplate( "Dungeon Bane", typeof( ToxicElemental ), "a swamp elemental", null, 0xA04, 0, 0xDBC, 0, 1973, 224 ) },
			{ "immortal bones", new ChallengeTemplate( "Dungeon Hate", typeof( VampireLord ), null, "the immortal one", 0, 0, 0x1B10, 0x66C, 2229, 389 ) },
			{ "staff of scorn", new ChallengeTemplate( "Dungeon Scorn", typeof( OphidianArchmage ), "Sylpha", "the princess of scorn", 0, 0, 0x2556, 0, 2237, 812 ) },
			{ "mind of allurement", new ChallengeTemplate( "Dungeon Torment", typeof( Succubus ), "Hertana", "of vile allurement", 0, 0, 0x1CF0, 0, 1977, 839 ) },
			{ "mask of the ghost", new ChallengeTemplate( "Dungeon Vile", typeof( EvilMage ), null, "the wanderer of mystics", 0, 0, 0x154B, 0x47E, 2336, 495 ) },
			{ "dead venom flies", new ChallengeTemplate( "Dungeon Wicked", typeof( PoisonElemental ), "an insect swarm", null, 0xA04, 0, 0xF34, 0xA04, 2180, 208 ) },
			{ "branch of the reaper", new ChallengeTemplate( "Dungeon Wrath", typeof( Reaper ), "a reaping willow", null, 0, 0, 0x3AD9, 0, 2334, 861 ) },
			{ "ink of the deep", new ChallengeTemplate( "the Flooded Temple", typeof( Kraken ), "a deep sea squid", null, 0xA1F, 0, 0x1D96, 0x969, 2447, 872 ) },
			{ "amulet of the stygian abyss", new ChallengeTemplate( "the Gargoyle Crypts", typeof( SpectralGargoyle ), "a spirit", "of a gargoyle priest", 0, 0, 0x4210, 0, 2047, 548 ) },
			{ "skin of the guardian", new ChallengeTemplate( "the Serpent Sanctum", typeof( OphidianKnight ), "Siluphtis", "the guardian of the sanctum", 0, 0, 0x20FE, 0x842, 2456, 498 ) },
			{ "orb of the fallen wizard", new ChallengeTemplate( "the Tomb of the Fallen Wizard", typeof( AncientLich ), null, "the fallen wizard", 0, 0, 0xE2E, 0x4A7, 2334, 32 ) },
			{ "bleeding crystal", new ChallengeTemplate( "the Blood Temple", typeof( BloodElemental ), "a bloody mist", null, 0x5B5, 13, 0x1F1C, 0x48E, 701, 2537 ) },
			{ "jade idol of Nesfatiti", new ChallengeTemplate( "the Dungeon of the Mad Archmage", typeof( Archmage ), null, null, 0, 0, 0x1224, 0xB93, 762, 1924 ) },
			{ "scroll of Abraxus", new ChallengeTemplate( "the Tombs", typeof( AncientLich ), null, "the seeker of the words", 0, 0, 0x227B, 0, 114, 2687 ) },
			{ "sphere of the dark circle", new ChallengeTemplate( "the Dungeon of the Lich King", typeof( Demon ), "Permaxumus", "the ruler of the dark circle", 0xA3A, 9, 0x573E, 0, 342, 2179 ) },
			{ "urn of Ulmarek's ashes", new ChallengeTemplate( "the Forgotten Halls", typeof( AncientLich ), "Ulmarek", null, 0, 0, 0x42B3, 0xB92, 56, 3245 ) },
			{ "crystal of everfrost", new ChallengeTemplate( "the Ice Queen Fortress", typeof( IceColossus ), "a greater ice elemental", null, 0, 0, 0x1F19, 0x480, 266, 2801 ) },
			{ "tablet of the wizard wars", new ChallengeTemplate( "the Halls of Ogrimar", typeof( OrkMage ), null, "of the war wizards", 0, 0, 0xED8, 0xB8B, 950, 2335 ) },
			{ "stone of the night gargoyle", new ChallengeTemplate( "Dungeon Rock", typeof( GargoyleOnyx ), null, "the gargoyle of night", 0, 0, 0x364E, 0, 645, 2193 ) },
			{ "pearl of Neptune", new ChallengeTemplate( "the Scurvy Reef", typeof( DeepSeaDevil ), null, "the defiler of the sea", 0, 0, 0x3199, 0xA37, 369, 3866 ) },
			{ "Black Beard's brandy", new ChallengeTemplate( "the Undersea Castle", typeof( SeaDragon ), null, "the coral dragon", 0xA07, 0, 0x4686, 0, 704, 3789 ) },
			{ "lamp of the desert", new ChallengeTemplate( "the Tomb of Kazibal", typeof( Fiend ), "Tutamak", "the sand fiend", 0x83B, 9, 0xA16, 0x5B7, 438, 3298 ) },
			{ "azure dust", new ChallengeTemplate( "the Azure Castle", typeof( Ifreet ), null, "the soul of azure", 0x538, 0, 0x2DB5, 0x532, 0, 0 ) },
			{ "skull of Azerok", new ChallengeTemplate( "the Catacombs of Azerok", typeof( DeadWizard ), "Azerok", "of the Deathly Veil", 0, 0x190, 0x26AB, 0xB71, 0, 0 ) },
			{ "egg of the harpy hen", new ChallengeTemplate( "Dungeon Covetous", typeof( HarpyHen ), null, null, 0, 0, 0x41BF, 0, 0, 0 ) },
			{ "bone of the frost giant", new ChallengeTemplate( "the Glacial Scar", typeof( FrostGiant ), "Murgor", "the frost giant chief", 0, 325, 0x2559, 0x482, 1949, 1512 ) },
			{ "mind of silver", new ChallengeTemplate( "the Temple of Osirus", typeof( Drake ), "a silver drake", null, 0x430, 0, 0x1CF0, 0x9C4, 6143, 3607 ) },
		};

		private class ActiveChallenge
		{
			public BaseCreature Monster;
			public string RegionName;
			public DateTime LastCombatSeen;
			public WatchdogTimer Timer;
		}

		private static readonly Dictionary<Mobile, ActiveChallenge> Active = new Dictionary<Mobile, ActiveChallenge>();

		public static void Initialize()
		{
			CommandSystem.Register( "epic-tribute-loc", AccessLevel.GameMaster, new CommandEventHandler( EpicTributeLoc_OnCommand ) );
		}

		[Usage( "epic-tribute-loc" )]
		[Description( "Target a player to show their active Epic Tribute challenge mob location and [Go x y z] coordinates." )]
		private static void EpicTributeLoc_OnCommand( CommandEventArgs e )
		{
			e.Mobile.SendMessage( "Target the player whose Epic Tribute challenge you wish to inspect." );
			e.Mobile.Target = new EpicTributeLocTarget();
		}

		// GM/staff helper (also used by [epic-tribute-loc]): reports the personal challenge mob's
		// current world location after the player has entered the assigned dungeon region.
		public static void ReportChallengeLocation( Mobile staff, PlayerMobile player )
		{
			if ( staff == null || player == null )
				return;

			string questItem = EpicCharacter.GetSpecialItemRequirement( player );

			if ( string.IsNullOrEmpty( questItem ) || questItem == "NEW" )
			{
				staff.SendMessage( "{0} has no active Epic Tribute key requirement.", player.Name );
				return;
			}

			ChallengeTemplate template;
			if ( !Templates.TryGetValue( questItem, out template ) )
			{
				staff.SendMessage( "{0} requires \"{1}\", but no challenge template exists for that item.", player.Name, questItem );
				return;
			}

			staff.SendMessage( "Epic Tribute challenge for {0}: \"{1}\" in {2}.", player.Name, questItem, template.RegionName );

			ActiveChallenge active;
			if ( !Active.TryGetValue( player, out active ) )
			{
				staff.SendMessage( "No personal challenge mob is currently spawned. The player must enter \"{0}\" while the requirement is active.", template.RegionName );
				return;
			}

			BaseCreature monster = active.Monster;

			if ( monster == null || monster.Deleted )
			{
				staff.SendMessage( "Challenge record exists but the mob is gone (killed or cleaned up). Re-enter \"{0}\" to re-trigger if still needed.", template.RegionName );
				return;
			}

			string mobLabel = monster.Name;

			if ( !string.IsNullOrEmpty( monster.Title ) )
				mobLabel = mobLabel + " " + monster.Title;

			if ( string.IsNullOrEmpty( mobLabel ) )
				mobLabel = template.MonsterType.Name;

			staff.SendMessage( "Challenge mob: {0}{1}", mobLabel, monster.Alive ? "" : " (dead)" );
			staff.SendMessage( "Map: {0}", monster.Map != null ? monster.Map.Name : "?" );
			staff.SendMessage( "Coordinates: {0}, {1}, {2}", monster.X, monster.Y, monster.Z );
			staff.SendMessage( "Go command: [Go {0} {1} {2}]", monster.X, monster.Y, monster.Z );
		}

		private class EpicTributeLocTarget : Target
		{
			public EpicTributeLocTarget() : base( -1, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				PlayerMobile player = targeted as PlayerMobile;

				if ( player == null )
				{
					from.SendMessage( "That is not a player." );
					return;
				}

				ReportChallengeLocation( from, player );
			}
		}

		// Called from BaseRegion.OnEnter (and once from EpicCharacter.SetSpecialItemRequirement,
		// to cover the case where the player is already standing in the target region when the
		// key requirement is first rolled).
		public static void TryTrigger( Mobile m )
		{
			if ( !( m is PlayerMobile ) || m.Map == null || m.Map == Map.Internal )
				return;

			PlayerMobile player = (PlayerMobile)m;

			string questItem = EpicCharacter.GetSpecialItemRequirement( player );

			if ( string.IsNullOrEmpty( questItem ) || questItem == "NEW" )
				return;

			if ( Active.ContainsKey( player ) )
				return;

			ChallengeTemplate template;
			if ( !Templates.TryGetValue( questItem, out template ) )
				return;

			Region reg = Region.Find( player.Location, player.Map );
			if ( reg == null || !reg.IsPartOf( template.RegionName ) )
				return;

			if ( EpicCharacter.HaveSpecialItemRequirement( player ) )
				return;

			Spawn( player, template );
		}

		// Looks up the full named dungeon Region object (all of its Area rectangles), as opposed
		// to whatever (possibly smaller/nested) region Region.Find( player.Location, ... ) would
		// return for the player's exact standing tile. Mirrors the by-name scan pattern used by
		// Engines.MLQuests.QuestArea.Validate().
		private static BaseRegion FindNamedRegion( string name, Map map )
		{
			foreach ( Region r in Region.Regions )
			{
				if ( r.Map == map && r.Name == name && r is BaseRegion )
					return (BaseRegion)r;
			}

			return null;
		}

		// Minimum tile distance (Chebyshev/box, matching Utility.InRange) a random challenge spawn
		// must keep from the original SummonCarriers.cs named mob's fixed Home coordinate, so the
		// personal challenge doesn't land on top of the shared carrier still used by Magical Prison.
		private const int MinDistanceFromOriginalHome = 10;

		// Random, passable, in-region placement via the same weighted-rectangle algorithm the
		// dungeon Spawner/SpawnEntry system already uses (BaseRegion.RandomSpawnLocation with
		// home = Point3D.Zero spans the whole region rather than a fixed radius). Retries a few
		// times to steer away from the original named mob's spot when one is known; falls back to
		// the first valid in-region tile found if that soft constraint can't be satisfied.
		private static Point3D PickRandomChallengeLocation( BaseRegion region, int homeX, int homeY )
		{
			bool hasOriginalHome = ( homeX != 0 || homeY != 0 );
			Point3D originalHome = new Point3D( homeX, homeY, 0 );
			Point3D firstValid = Point3D.Zero;

			for ( int attempt = 0; attempt < 20; ++attempt )
			{
				Point3D candidate = region.RandomSpawnLocation( 16, true, false, Point3D.Zero, 0 );

				if ( candidate == Point3D.Zero )
					continue;

				if ( !hasOriginalHome || !Utility.InRange( candidate, originalHome, MinDistanceFromOriginalHome ) )
					return candidate;

				if ( firstValid == Point3D.Zero )
					firstValid = candidate; // keep as a fallback if every attempt lands near the original spot
			}

			return firstValid;
		}

		private static void Spawn( PlayerMobile player, ChallengeTemplate template )
		{
			BaseCreature monster;

			try
			{
				monster = (BaseCreature)Activator.CreateInstance( template.MonsterType );
			}
			catch ( Exception e )
			{
				Console.WriteLine( "EpicTributeChallenge.Spawn: failed to create {0}: {1}", template.MonsterType, e );
				return;
			}

			SummonPrison.SetDifficultyForMonster( monster );

			Map map = player.Map;
			Point3D loc = Point3D.Zero;

			BaseRegion targetRegion = FindNamedRegion( template.RegionName, map );

			if ( targetRegion != null )
				loc = PickRandomChallengeLocation( targetRegion, template.HomeX, template.HomeY );

			if ( loc == Point3D.Zero )
			{
				// Fallback (region lookup failed, or RandomSpawnLocation found no passable tile
				// after retries): place near the player instead of failing to spawn outright.
				// Still guaranteed in-region: TryTrigger only reaches Spawn() after confirming the
				// player is standing inside template.RegionName.
				bool validLocation = false;
				loc = player.Location;

				for ( int j = 0; !validLocation && j < 10; ++j )
				{
					int x = player.X + Utility.Random( 3 ) - 1;
					int y = player.Y + Utility.Random( 3 ) - 1;
					int z = map.GetAverageZ( x, y );

					if ( validLocation = map.CanFit( x, y, player.Z, 16, false, false ) )
						loc = new Point3D( x, y, player.Z );
					else if ( validLocation = map.CanFit( x, y, z, 16, false, false ) )
						loc = new Point3D( x, y, z );
				}
			}

			if ( template.NameOverride != null )
				monster.Name = template.NameOverride;

			if ( template.TitleOverride != null )
				monster.Title = template.TitleOverride;

			if ( template.Hue > 0 )
				monster.Hue = template.Hue;

			if ( template.Body > 0 )
				monster.Body = template.Body;

			monster.Fame = 0;
			monster.Karma = 0;

			Item key = new SummonItems();
			key.Name = EpicCharacter.GetSpecialItemRequirement( player );
			key.ItemID = template.ItemId;
			key.Hue = template.ItemHue;
			( (SummonItems)key ).EpicChallengeSource = true;
			monster.PackItem( key );

			monster.MoveToWorld( loc, map );
			monster.Combatant = player;
			Effects.SendLocationParticles( EffectItem.Create( monster.Location, monster.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
			monster.PlaySound( 0x1FE );

			ActiveChallenge active = new ActiveChallenge();
			active.Monster = monster;
			active.RegionName = template.RegionName;
			active.LastCombatSeen = DateTime.UtcNow;
			active.Timer = new WatchdogTimer( player );
			active.Timer.Start();

			Active[ player ] = active;
		}

		// Called from BaseRegion.OnExit: immediate cleanup if the player leaves the dungeon
		// region tied to their active challenge without killing it. Mirrors QuestTome.BossEscaped.
		public static void OnRegionExit( Mobile m )
		{
			if ( !( m is PlayerMobile ) )
				return;

			ActiveChallenge active;
			if ( !Active.TryGetValue( m, out active ) )
				return;

			Region reg = ( m.Map == null ) ? null : Region.Find( m.Location, m.Map );

			if ( reg != null && reg.IsPartOf( active.RegionName ) )
				return; // still inside the same dungeon (moved between nested sub-regions)

			Cleanup( (PlayerMobile)m, active );
		}

		private static void Cleanup( PlayerMobile player, ActiveChallenge active )
		{
			if ( active.Timer != null )
				active.Timer.Stop();

			if ( active.Monster != null && !active.Monster.Deleted && active.Monster.Alive )
				active.Monster.Delete();

			Active.Remove( player );
		}

		private static bool IsInCombat( BaseCreature monster )
		{
			if ( monster.Combatant != null )
				return true;

			if ( monster.Aggressors != null && monster.Aggressors.Count > 0 )
				return true;

			if ( monster.Aggressed != null && monster.Aggressed.Count > 0 )
				return true;

			return false;
		}

		private class WatchdogTimer : Timer
		{
			private PlayerMobile m_Player;

			public WatchdogTimer( PlayerMobile player ) : base( TimeSpan.FromMinutes( 1.0 ), TimeSpan.FromMinutes( 1.0 ) )
			{
				m_Player = player;
				Priority = TimerPriority.OneMinute;
			}

			protected override void OnTick()
			{
				ActiveChallenge active;
				if ( !Active.TryGetValue( m_Player, out active ) )
				{
					Stop();
					return;
				}

				BaseCreature monster = active.Monster;

				if ( monster == null || monster.Deleted || !monster.Alive )
				{
					// Resolved normally (killed and looted, or removed by other means).
					Active.Remove( m_Player );
					Stop();
					return;
				}

				if ( IsInCombat( monster ) )
				{
					active.LastCombatSeen = DateTime.UtcNow;
					return;
				}

				if ( DateTime.UtcNow - active.LastCombatSeen >= TimeSpan.FromMinutes( 30.0 ) )
				{
					// Non-combat timeout fail-safe (confirmed 2026-07-16): despawn, re-triggerable
					// on the next qualifying region entry. Does not touch EpicQuestName.
					Cleanup( m_Player, active );
				}
			}
		}
	}
}
