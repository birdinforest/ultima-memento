using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Harvest
{
    public class RichVeinUtilities
    {
        public delegate void GotCandidateCallback(bool hasMiningNode, LandTile landTile, Region region, string keyName);

        public delegate bool RegionPredicate(Region region);

        public static IEnumerable<RichVeinConfig> Build(Map map, RegionPredicate regionPredicate)
        {
            var pointsByRegion = new Dictionary<string, List<Point3D>>();
            for (var x = 0; x < map.Width; x++)
            {
                for (var y = 0; y < map.Height; y++)
                {
                    GetCandidate(map, x, y, regionPredicate,
                        (hasMiningNode, landTile, region, regionName) =>
                            {
                                List<Point3D> points;
                                if (!pointsByRegion.TryGetValue(regionName, out points))
                                    pointsByRegion[regionName] = points = new List<Point3D>();

                                var z = map.GetHighestPoint(x, y);
                                var point = new Point3D(x, y, z + 1);

                                points.Add(point);
                            }
                    );
                }
            }

            return pointsByRegion.Keys
                .OrderBy(x => x)
                .ThenByDescending(x => x == RichVeinEngine.UNKNOWN_REGION_NAME)
                .Select(region =>
                {
                    var points = pointsByRegion[region];
                    return new RichVeinConfig
                    {
                        Region = region,
                        Candidates = points,
                        MinDistance = 25,
                        MaxNodes = 1 + (points.Count / 20) // At max 1 point for every 20
                    };
                });
        }

        public static void GetCandidate(Map map, int x, int y, RegionPredicate regionPredicate, GotCandidateCallback onCandidateFound, Action<string> onFailureMessage = null)
        {
            GetCandidate(map, x, y, regionPredicate, onCandidateFound, true, onFailureMessage);
        }

        public static void GetCandidate(Map map, int x, int y, RegionPredicate regionPredicate, GotCandidateCallback onCandidateFound, bool requireMiningNodes, Action<string> onFailureMessage = null)
        {
            var landTile = map.Tiles.GetLandTile(x, y);
            if (!IsImpassable(landTile, map, x, y))
            {
                if (onFailureMessage != null)
                {
                    onFailureMessage(string.Format("The land tile at ({0}, {1}) must be impassable to be a valid candidate.", x, y));
                }

                return;
            }

            var point = new Point3D(x, y, landTile.Z);
            Region reg = Region.Find(point, map);
            if (!regionPredicate(reg))
            {
                if (onFailureMessage != null)
                {
                    onFailureMessage(string.Format("The location at ({0}, {1}) failed the Region check.", x, y));
                }

                return;
            }

            if (!HasEmptyNeighbor(map, x, y))
            {
                if (onFailureMessage != null)
                {
                    onFailureMessage(string.Format("The location at ({0}, {1}) has no empty spaces next to it.", x, y));
                }

                return;
            }

            var hasMiningNode = Mining.System.GetDefinition(landTile.ID) == Mining.System.OreAndStone;
            if (requireMiningNodes && !hasMiningNode)
            {
                if (onFailureMessage != null)
                {
                    onFailureMessage(string.Format("The location at ({0}, {1}) is not mineable.", x, y));
                }

                return;
            }

            var regionName = RichVeinUtilities.GetRegionName(reg, map, point);
            onCandidateFound(hasMiningNode, landTile, reg, regionName);
        }

        public static string GetRegionName(Map map, Point3D point)
        {
            var region = Region.Find(point, map);
            return GetRegionName(region, map, point);
        }

        public static string GetRegionName(Region region, Map map, Point3D point)
        {
            return region.Name ?? Misc.Worlds.GetRegionName(map, point) ?? RichVeinEngine.UNKNOWN_REGION_NAME;
        }

        public static IEnumerable<Point2D> GetEmptyNeighbors(Map map, int x, int y)
        {
            for (var left = -1; left <= 1; left++)
            {
                for (var right = -1; right <= 1; right++)
                {
                    if (left == 0 && right == 0) continue; // Skip itself

                    var testX = x + left;
                    var testY = y + right;

                    // Stay within bounds
                    if (testX < 0 || testY < 0 || testX >= map.Width || testY >= map.Height) continue;

                    var adjacentTile = map.Tiles.GetLandTile(testX, testY);
                    if (!IsImpassable(adjacentTile, map, testX, testY)) yield return new Point2D(testX, testY);
                }
            }
        }

        private static bool HasEmptyNeighbor(Map map, int x, int y)
        {
            return GetEmptyNeighbors(map, x, y).Any();
        }

        private static bool IsImpassable(LandTile landTile, Map map, int x, int y)
        {
            int landID = landTile.ID & TileData.MaxLandValue;
            var flags = TileData.LandTable[landID].Flags;
            if ((flags & TileFlag.Impassable) != 0) return true;

            var statics = map.Tiles.GetStaticTiles(x, y, true);
            return statics.Any(tile =>
            {
                ItemData id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];
                return id.Impassable;
            });
        }
    }
}