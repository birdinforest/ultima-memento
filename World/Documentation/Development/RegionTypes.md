# Region Types

Descriptions of each region type on this shard, based on the region classes under `World/Source/Scripts/System/Regions/` and usage in [`Regions.xml`](Regions.xml).

## Settlements

- **VillageRegion**
	- Towns/cities (Britain, Lodoria, etc.). Mostly no housing; enter/exit logging + village music; some special light (e.g. Underworld).
- **BardTownRegion**
	- Skara Brae only. No housing; town music/logging.
- **UmbraRegion**
	- Undercity of Umbra. No housing; night lighting; dungeon-style music.

## Dungeons / underground

- **DungeonRegion**
	- Full dungeons. No housing; dungeon light (with a few named exceptions); dungeon music; last player leaving cleans up doors/NPCs.
- **CaveRegion**
	- Smaller caves/mines. No housing; cave light + cave music.
- **BardDungeonRegion**
	- Bard’s Tale dungeons (Mangar’s, Catacombs, etc.). No housing; cave light; dungeon music.
- **DeadRegion**
	- Dead/hostile underground areas. No housing; dungeon light; danger music.
- **BargeDeadRegion**
	- Barge of the Dead. Same idea as DeadRegion, no music override.
- **MoonCore**
	- Moon cores. No housing; spells blocked; special damage/skill gates.

## Outdoors

- **OutDoorRegion**
	- Friendly outdoor POIs (shrines, forges, Ranger Outpost). No housing except Ranger Outpost (high Camping/Tracking).
- **OutDoorBadRegion**
	- Hostile outdoor POIs (ruins, forts, graveyards). No housing; danger music.
- **PirateRegion**
	- Pirate waters/islands. Pirate music; pirate crews return home when you leave.
- **MazeRegion**
	- Maze areas. No housing; logging only.
- **NecromancerRegion**
	- Evil-aligned areas (Ravendark, etc.). Evil players can house; dungeon light; necromancer music.

## Safe / restricted

- **SafeRegion**
	- Safe docks/forts. No combat (Warrior exception); limited spells; often high priority.
- **PublicRegion**
	- Indoor public spaces (guilds, bank, tavern, Lyceum). No housing/combat; instant logout; limited spells.
- **ProtectedRegion**
	- Fully protected chambers (e.g. Codex). No combat; all spells blocked.
- **StartRegion**
	- New-character start biomes. No combat/spells; shows welcome gumps.
- **WantedRegion**
	- Britain jail cell. Fully locked down; jail messaging.
- **PrisonArea**
	- Prison interiors. Locked down; opens Prison gump.
- **SavageRegion**
	- Primitive hut. Locked down; night light.
- **CrashRegion**
	- Crash Site. Locked down; enter message.

## Housing / special map pockets

- **UnderHouseRegion**
	- Housing allowed underground-style areas; night light.
- **DungeonHomeRegion**
	- Dungeon dwelling map pockets (no new housing placement).
- **SkyHomeDwelling**
	- Sky dwelling pockets.
- **DawnRegion / LunaRegion**
	- Moon areas with Magery/Necro/Elementalism ≥ 80 gate (or you get teleported off).
- **GargoyleRegion**
	- Gargoyle locales; special light in Burning Mines.

## Runtime / unused

- **HouseRegion**
	- Created per player house at runtime (not in XML). Access, lockdown, friend PvP rules.
- **GuardedRegion / TownRegion / Jail / NoHousingRegion / SeaSpawnRegion**
	- Exist in code but are not used in `Regions.xml` (stubs or legacy).

## Default / no named region

Areas with **no matching rect** use the map’s **DefaultRegion** (`IsDefault`). The teleporter connectivity tools treat that as **Overworld**.
