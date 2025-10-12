using System;
using Server.Items;
using System.Linq;
using System.Collections.Generic;
using Server.Items.Abstractions;

namespace Server.Engines.Harvest
{
    public class RichLumberjacking : Lumberjacking
	{
        private const int SECRET_TILE_ID = -2;
		private static RichLumberjacking m_System;

		private readonly VolatileHarvestDefinition m_VolatileDefinition;
        private readonly HashSet<int> m_LumberjackTiles;

		public static new RichLumberjacking System
		{
			get
			{
				if ( m_System == null )
					m_System = new RichLumberjacking();

				return m_System;
			}
		}

		private RichLumberjacking()
		{
			#region Rich Lumberjacking
            var lumberDefinition = Definition;
            m_LumberjackTiles = lumberDefinition.Tiles.Distinct().ToHashSet();

			m_VolatileDefinition = new VolatileHarvestDefinition(lumberDefinition)
            {
				// Resource banks are every 1x1 tiles
				BankWidth = 1,
				BankHeight = 1,

				// Every bank holds from 8 to 18 logs
				MinTotal = 8,
				MaxTotal = 18,

				// A resource bank will respawn its content every 20 to 30 minutes
				MinRespawn = TimeSpan.FromMinutes( 20.0 ),
				MaxRespawn = TimeSpan.FromMinutes( 30.0 ),

				// Skill checking is done on the Lumberjacking skill
				Skill = SkillName.Lumberjacking,

				// Set the list of harvestable tiles
				Tiles = new[] { SECRET_TILE_ID },

				// Players must be within 2 tiles to harvest
				MaxRange = 2,

				// Ten logs per harvest action
				ConsumedPerHarvest = int.MaxValue,
				ConsumedPerIslesDreadHarvest = int.MaxValue,

				// The chopping effect
				EffectActions = new int[]{ 13 },
				EffectSounds = new int[]{ 0x13E },
				EffectCounts = new int[]{ 1 },
				EffectDelay = TimeSpan.FromSeconds( 0.5 ),
				EffectSoundDelay = TimeSpan.FromSeconds( 0.5 ),

				NoResourcesMessage = 500493, // There's not enough wood here to harvest.
				FailMessage = 500495, // You hack at the tree for a while, but fail to produce any useable wood.
				OutOfRangeMessage = 500446, // That is too far away.
				PackFullMessage = 500720, // You don't have enough room in your backpack!
				ToolBrokeMessage = 500499, // You broke your axe.
                RandomizeVeins = true,
                Resources = new HarvestResource[]
                {
					new HarvestResource(  00.0, 00.0, 85.0, "", typeof( Log ) ),
					new HarvestResource(  55.0, 25.0, 90.0, "", typeof( AshLog ) ),
					new HarvestResource(  60.0, 30.0, 95.0, "", typeof( CherryLog ) ),
					new HarvestResource(  65.0, 35.0, 100.0, "", typeof( EbonyLog ) ),
					new HarvestResource(  70.0, 40.0, 105.0, "", typeof( GoldenOakLog ) ),
					new HarvestResource(  75.0, 45.0, 110.0, "", typeof( HickoryLog ) ),
					new HarvestResource(  80.0, 50.0, 115.0, "", typeof( MahoganyLog ) ),
					new HarvestResource(  85.0, 55.0, 120.0, "", typeof( OakLog ) ),
					new HarvestResource(  90.0, 65.0, 125.0, "", typeof( PineLog ) ),
					new HarvestResource(  95.0, 75.0, 130.0, "", typeof( RosewoodLog ) ),
					new HarvestResource(  100.0, 85.0, 135.0, "", typeof( WalnutLog ) ),
					new HarvestResource(  100.1, 95.0, 140.0, "", typeof( ElvenLog ) )
                },
            };
			
            var res = m_VolatileDefinition.Resources;
            m_VolatileDefinition.Veins = new HarvestVein[]
                {
					new HarvestVein(    0, 0, res[0], null ),	// Ordinary Logs
					new HarvestVein( 30.0, 0, res[1], res[0] ), // Ash
					new HarvestVein( 15.0, 0, res[2], res[0] ), // Cherry
					new HarvestVein( 10.0, 0, res[3], res[0] ), // Ebony
					new HarvestVein( 09.0, 0, res[4], res[0] ), // Golden Oak
					new HarvestVein( 08.0, 0, res[5], res[0] ), // Hickory
					new HarvestVein( 07.0, 0, res[6], res[0] ), // Mahogany
					new HarvestVein( 06.0, 0, res[7], res[0] ), // Oak
					new HarvestVein( 05.0, 0, res[8], res[0] ), // Pine
					new HarvestVein( 04.0, 0, res[9], res[0] ), // Rosewood
					new HarvestVein( 03.5, 0, res[10], res[0] ), // Walnut
					new HarvestVein( 02.5, 0, res[11], res[0] ) // Elven
                };

			Definitions.Add( m_VolatileDefinition );
			#endregion
		}

        public override bool GetHarvestDetails(Mobile from, Item tool, object toHarvest, out int tileID, out Map map, out Point3D loc)
        {
			if (toHarvest is RichLumberjackingSparkle) // When double clicked or targeted directly
			{
				var sparkle = (Item)toHarvest;
				map = sparkle.Map;
				loc = sparkle.Location;
				tileID = SECRET_TILE_ID;

				return map != null && map != Map.Internal;
			}

            if (!base.GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc)) return false;

			var harvestTool = tool as IHarvestTool;
			if (harvestTool == null || harvestTool.HarvestSystem != RichLumberjacking.System) return true; // Don't overwrite behavior 

			if (!m_VolatileDefinition.HasBank(from) || m_VolatileDefinition.GetBank(map, loc.X, loc.Y) == null) return true;

			tileID = SECRET_TILE_ID;

			return true;
        }

        public override void OnHarvestFinished( Mobile from, Item tool, HarvestDefinition def, HarvestVein vein, HarvestBank bank, HarvestResource resource, object harvested )
		{
			base.OnHarvestFinished(from, tool, def, vein, bank, resource, harvested);

			if (harvested is IPoint3D)
			{
				if (55 <= from.Skills[SkillName.Lumberjacking].Base && bank.IsEmpty && Utility.Random(100) < 25 && !m_VolatileDefinition.HasBank(from))
				{
                    const int MAX_DISTANCE = 8;
					var location = (IPoint3D)harvested;
					var candidates = new List<Point3D>();
					for (var x = 0 - MAX_DISTANCE; x <= MAX_DISTANCE; x++)
					{
						for (var y = 0 - MAX_DISTANCE; y <= MAX_DISTANCE; y++)
						{
							var testLocation = new Point3D(location.X + x, location.Y + y, location.Z);
							if (testLocation == location) continue;

							foreach (var tile in from.Map.Tiles.GetStaticTiles(testLocation.X, testLocation.Y))
							{
								if (!m_LumberjackTiles.Contains(tile.ID + TileData.MaxLandValue)) continue; // Or 0x4000 ???

								candidates.Add(testLocation);
							}
						}
					}

					if (0 < candidates.Count)
					{
						m_VolatileDefinition.TryCreateBank(from, candidates[Utility.Random(candidates.Count)], point =>
						{
							var node = new RichLumberjackingSparkle();
							var destination = new Point3D(point) { Z = from.Map.GetAverageZ(point.X, point.Y) + 1 };
							node.MoveToWorld(destination, from.Map);
							Effects.PlaySound(point, from.Map, 0x28E); // agility
                			Effects.SendTargetEffect(node, 0x3039, 10, 12); // Swirl

							return node;
						});
					}
				}
            }
		}
	}
}