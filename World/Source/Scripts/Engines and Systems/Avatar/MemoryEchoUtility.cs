using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Misc;

namespace Server.Engines.Avatar
{
	public static class MemoryEchoUtility
	{
		public static List<SearchBase> FindSearchBasesInRegion( string regionName )
		{
			var list = new List<SearchBase>();

			if ( string.IsNullOrEmpty( regionName ) )
				return list;

			foreach ( Item item in World.Items.Values )
			{
				if ( item == null || item.Deleted || item.Map == null || item.Map == Map.Internal )
					continue;

				SearchBase searchBase = item as SearchBase;

				if ( searchBase == null )
					continue;

				if ( Insensitive.Equals( Worlds.GetRegionName( searchBase.Map, searchBase.Location ), regionName ) )
					list.Add( searchBase );
			}

			return list;
		}

		public static SearchBase PickSearchBaseInRegion( string regionName )
		{
			List<SearchBase> list = FindSearchBasesInRegion( regionName );

			if ( list.Count == 0 )
				return null;

			return list[Utility.Random( list.Count )];
		}

		public static SearchBase PickAnySearchBaseForResearch( ResearchBag bag )
		{
			if ( bag == null )
				return null;

			string[] locations =
			{
				bag.RuneLocation,
				bag.SpellsMageLocation,
				bag.SpellsNecroLocation,
				bag.BagInkLocation,
				bag.ResearchLocation
			};

			for ( int i = 0; i < locations.Length; ++i )
			{
				if ( string.IsNullOrEmpty( locations[i] ) )
					continue;

				SearchBase searchBase = PickSearchBaseInRegion( locations[i] );

				if ( searchBase != null )
					return searchBase;
			}

			foreach ( Item item in World.Items.Values )
			{
				if ( item == null || item.Deleted || item.Map == null || item.Map == Map.Internal )
					continue;

				SearchBase searchBase = item as SearchBase;

				if ( searchBase != null )
					return searchBase;
			}

			return null;
		}

		public static bool TryResolveGoTarget( PlayerContext ctx, out Map map, out Point3D location, out SearchBase searchBase )
		{
			map = null;
			location = Point3D.Zero;
			searchBase = null;

			if ( ctx == null || string.IsNullOrEmpty( ctx.CurrentResonanceLocation ) )
				return false;

			if ( ctx.MemoryEchoSearchBaseSerial != Serial.Zero )
			{
				Item item = World.FindItem( ctx.MemoryEchoSearchBaseSerial );
				searchBase = item as SearchBase;

				if ( searchBase != null && !searchBase.Deleted && searchBase.Map != null && searchBase.Map != Map.Internal )
				{
					map = searchBase.Map;
					location = GetStandLocationNear( searchBase );
					return location != Point3D.Zero;
				}
			}

			return TryResolveGoTarget( ctx.CurrentResonanceLocation, out map, out location, out searchBase );
		}

		public static bool TryResolveGoTarget( string regionName, out Map map, out Point3D location, out SearchBase searchBase )
		{
			map = null;
			location = Point3D.Zero;
			searchBase = null;

			if ( string.IsNullOrEmpty( regionName ) )
				return false;

			List<SearchBase> bases = FindSearchBasesInRegion( regionName );

			if ( bases.Count > 0 )
			{
				for ( int attempt = 0; attempt < bases.Count; ++attempt )
				{
					SearchBase candidate = bases[Utility.Random( bases.Count )];
					Point3D stand = GetStandLocationNear( candidate );

					if ( stand != Point3D.Zero )
					{
						searchBase = candidate;
						map = candidate.Map;
						location = stand;
						return true;
					}
				}
			}

			for ( int i = 0; i < Map.AllMaps.Count; ++i )
			{
				Map m = Map.AllMaps[i];

				if ( m == null || m == Map.Internal || m.MapIndex == 0x7F || m.MapIndex == 0xFF )
					continue;

				foreach ( Region r in m.Regions.Values )
				{
					if ( !Insensitive.Equals( r.Name, regionName ) )
						continue;

					if ( r.GoLocation != Point3D.Zero && m.CanFit( r.GoLocation, 16, false, false ) )
					{
						map = m;
						location = r.GoLocation;
						return true;
					}
				}
			}

			return false;
		}

		public static Point3D GetStandLocationNear( SearchBase searchBase )
		{
			if ( searchBase == null || searchBase.Map == null || searchBase.Map == Map.Internal )
				return Point3D.Zero;

			Map map = searchBase.Map;
			Point3D center = searchBase.GetWorldLocation();

			int[] dx = { 0, 1, -1, 0, 0, 1, -1, 1, -1, 2, -2, 2, -2 };
			int[] dy = { 0, 0, 0, 1, -1, 1, -1, -1, 1, 0, 0, 1, -1 };

			for ( int i = 0; i < dx.Length; ++i )
			{
				Point3D test = new Point3D( center.X + dx[i], center.Y + dy[i], center.Z );

				if ( map.CanFit( test, 16, false, false ) )
					return test;
			}

			if ( map.CanFit( center, 16, false, false ) )
				return center;

			return Point3D.Zero;
		}

		public static string FormatGoCommand( Map map, Point3D loc, string regionName )
		{
			if ( map == null || loc == Point3D.Zero )
				return null;

			return string.Format( "[Go {0} {1} {2}]", loc.X, loc.Y, loc.Z );
		}

		public static void ReportGoLocation( Mobile staff, PlayerMobile player, PlayerContext ctx )
		{
			if ( staff == null || player == null || ctx == null )
				return;

			ResearchBag bag = ctx.GetResearchBag() ?? AvatarCoreItemMigration.FindResearchBag( player );

			if ( bag != null )
				AvatarCoreItemMigration.EnsureMemoryEchoAssignment( ctx, bag );

			string regionName = ctx.CurrentResonanceLocation;

			if ( string.IsNullOrEmpty( regionName ) )
			{
				staff.SendMessage( "That player has no Memory Echo assigned." );
				return;
			}

			string typeLabel = ResearchLocalization.EchoTypeLabel( player, ctx.ResonanceLocationType );
			Map map;
			Point3D loc;
			SearchBase searchBase;

			staff.SendMessage( "Memory Echo for {0}: {1} ({2})", player.Name, regionName, typeLabel );

			if ( TryResolveGoTarget( ctx, out map, out loc, out searchBase ) )
			{
				string goCmd = FormatGoCommand( map, loc, regionName );
				staff.SendMessage( "Map: {0}", map != null ? map.Name : "?" );
				staff.SendMessage( "Coordinates: {0}, {1}, {2}", loc.X, loc.Y, loc.Z );
				staff.SendMessage( "Go command: {0}", goCmd );

				if ( searchBase != null )
				{
					Point3D chestLoc = searchBase.GetWorldLocation();
					staff.SendMessage( "Search chest at: {0}, {1}, {2} — interact within 2 tiles to begin resonance.", chestLoc.X, chestLoc.Y, chestLoc.Z );
				}
			}
			else
			{
				staff.SendMessage( "Could not resolve a reachable SearchBase for region \"{0}\". Re-assigning echo…", regionName );

				if ( bag != null )
				{
					AvatarCoreItemMigration.AssignMemoryEcho( ctx, bag );

					if ( TryResolveGoTarget( ctx, out map, out loc, out searchBase ) )
					{
						string goCmd = FormatGoCommand( map, loc, ctx.CurrentResonanceLocation );
						staff.SendMessage( "Re-assigned to: {0}", ctx.CurrentResonanceLocation );
						staff.SendMessage( "Map: {0}", map != null ? map.Name : "?" );
						staff.SendMessage( "Coordinates: {0}, {1}, {2}", loc.X, loc.Y, loc.Z );
						staff.SendMessage( "Go command: {0}", goCmd );
					}
					else
					{
						staff.SendMessage( "Still could not resolve coordinates. Check that SearchBase chests exist in the world." );
					}
				}
			}
		}
	}
}
