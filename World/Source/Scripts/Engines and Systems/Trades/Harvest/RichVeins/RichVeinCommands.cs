using Server.Commands;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Server.Engines.Harvest
{
	public class RichVeinCommands
	{
		private static readonly HashSet<string> ForceIncludedRegions = new HashSet<string>
		{
			"the Mines of Morinia",
			"the Ancient Crash Site"
		};

		private static readonly HashSet<string> IgnoredRegions = new HashSet<string>
		{
			RichVeinEngine.UNKNOWN_REGION_NAME,

			"the Port",
			"a Dungeon Dwelling",
			"Sky Home",
			"Lyceum",
			"Shimazu's Dojo",
			"the City of the Dead",
			"the Tomb of Zoda the Jedi Master",
			"Seggallions's Cave",
			"the Chamber of Virtue",
			"Cavern of Sermillon",
		};

		private static readonly Type[] IgnoredRegionTypes = new[]
		{
			typeof(DungeonRegion),
			typeof(OutDoorBadRegion),
			typeof(BardDungeonRegion),
			typeof(SafeRegion),
		};

		public static void Initialize()
		{
			CommandSystem.Register("OreVein-Build", AccessLevel.Administrator, OnBuild);
			CommandSystem.Register("OreVein-Clear", AccessLevel.Administrator, OnClear);
			CommandSystem.Register("OreVein-DebugMe", AccessLevel.GameMaster, OnDebugMe);
			CommandSystem.Register("OreVein-ExportData", AccessLevel.Administrator, OnExportData);
			CommandSystem.Register("OreVein-ExportMarkers", AccessLevel.Administrator, OnExportMarkers);
			CommandSystem.Register("OreVein-LoadData", AccessLevel.Administrator, OnLoadData);
			CommandSystem.Register("OreVein-Restart", AccessLevel.Administrator, OnRestart);
			CommandSystem.Register("OreVein", AccessLevel.GameMaster, args => { args.Mobile.SendGump(new RichVeinGump(args.Mobile)); });
		}

		[Usage("OreVein-Build <MapId> | NULL")]
		[Description("Builds spawners for all maps unless a `MapId` is specified.")]
		public static void OnBuild(CommandEventArgs e)
		{
			IEnumerable<Map> maps;
			if (!TryGetMaps(e.Mobile, e.Arguments, out maps)) return;

			var sw = Stopwatch.StartNew();
			foreach (var map in maps)
			{
				var start = sw.ElapsedMilliseconds;

				foreach (var configuration in RichVeinUtilities.Build(map, ValidateRegion))
				{
					RichVeinEngine.Instance.AddConfig(map, configuration);
				}

				Console.WriteLine("Built configuration for '{0}' in {1} ms", map.Name, sw.ElapsedMilliseconds - start);
			}

			if (e.Mobile != null)
			{
				e.Mobile.SendMessage("Configuring OreVeinEngine took {0} ms for: {1}.", sw.ElapsedMilliseconds, string.Join(", ", maps.Select(map => map.Name)));
			}
		}

		[Usage("OreVein-Clear")]
		[Description("Clears all automatically created entities created by the OreVeinEngine.")]
		public static void OnClear(CommandEventArgs e)
		{
			// Don't delete manually created Spawners
			var toDelete = World.Items.Values.Where(i => i is RichVeinMineable || i is RichVeinSpawner && ((RichVeinSpawner)i).AutomaticCleanup).ToList();
			foreach (var item in toDelete)
			{
				item.Delete();
			}

			if (e.Mobile != null)
			{
				e.Mobile.SendMessage("Deleted '{0}' entities created by the OreVeinEngine", toDelete.Count);
			}
		}

		[Usage("OreVein-DebugMe")]
		[Description("Evaluates your location to determine if it is a valid location.")]
		public static void OnDebugMe(CommandEventArgs e)
		{
			var map = e.Mobile.Map;
			var x = e.Mobile.X;
			var y = e.Mobile.Y;
			RichVeinUtilities.GetCandidate(map, x, y, ValidateRegion,
				(hasMiningNode, landTile, region, regionName) =>
					{
						var z = map.GetHighestPoint(x, y);
						var point = new Point3D(x, y, z + 1);
						e.Mobile.SendMessage("This is a valid location for an OreVein.");
					},
				message => e.Mobile.SendMessage(message));
		}

		[Usage("OreVein-ExportData <MapId> | NULL")]
		[Description("Exports XML configuration for all maps unless a `MapId` is specified.")]
		public static void OnExportData(CommandEventArgs e)
		{
			IEnumerable<Map> maps;
			if (!TryGetMaps(e.Mobile, e.Arguments, out maps)) return;

			var sw = Stopwatch.StartNew();
			foreach (var map in maps)
			{
				var start = sw.ElapsedMilliseconds;
				var filename = Path.Combine("Data/Mining", map.Name + ".xml");
				if (File.Exists(filename)) File.Delete(filename);

				Directory.CreateDirectory(Directory.GetParent(filename).FullName);

				using (var filestream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					var configurations = RichVeinUtilities.Build(map, ValidateRegion).OrderBy(x => x.Region).ThenByDescending(x => x.Region == RichVeinEngine.UNKNOWN_REGION_NAME);

					var root = new XElement("XmlRoot");
					foreach (var config in configurations)
					{
						var regionElement = new XElement("Config");
						regionElement.Add(new XAttribute("region", config.Region));
						regionElement.Add(new XAttribute("minDistance", config.MinDistance));
						regionElement.Add(new XAttribute("maxNodes", config.MaxNodes));

						var wrapperElement = new XElement("Candidates");
						wrapperElement.Add(new XAttribute("totalCandidates", config.Candidates.Count)); // Informational only

						regionElement.Add(wrapperElement);

						config.Candidates.ForEach(c =>
						{
							var pointElement = new XElement("Point");
							pointElement.Add(new XAttribute("x", c.X));
							pointElement.Add(new XAttribute("y", c.Y));
							pointElement.Add(new XAttribute("z", c.Z));

							wrapperElement.Add(pointElement);
						});

						root.Add(regionElement);
					}

					root.Save(filestream);

					Console.WriteLine("Exported configuration for '{0}' in {1} ms", map.Name, sw.ElapsedMilliseconds - start);
				}
			}

			if (e.Mobile != null)
			{
				e.Mobile.SendMessage("Exporting configuration for OreVeinEngine took {0} ms for: {1}.", sw.ElapsedMilliseconds, string.Join(", ", maps.Select(map => map.Name)));
			}
		}

		[Usage("OreVein-ExportMarkers <MapId> | NULL")]
		[Description("Exports a list of candidate locations as client markers for ClassicUO for all maps unless a `MapId` is specified.")]
		public static void OnExportMarkers(CommandEventArgs e)
		{
			IEnumerable<Map> maps;
			if (!TryGetMaps(e.Mobile, e.Arguments, out maps)) return;

			var debug = true;
			const string MINEABLE_ICON = "LEVEL3";
			const string NON_MINEABLE_ICON = "TERRAINX";

			foreach (var map in maps)
			{
				var filename = Path.Combine("Data/Mining", map.Name + ".csv");
				if (File.Exists(filename)) File.Delete(filename);

				Directory.CreateDirectory(Directory.GetParent(filename).FullName);

				using (var filestream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
				using (StreamWriter writer = new StreamWriter(filestream))
				{
					var pointsByRegion = new Dictionary<string, List<string>>();

					var sw = Stopwatch.StartNew();
					for (var x = 0; x < map.Width; x++)
					{
						for (var y = 0; y < map.Height; y++)
						{
							RichVeinUtilities.GetCandidate(map, x, y, ValidateRegion, (hasMiningNode, landTile, region, regionName) =>
							{
								// Note: Remove this if you want ALL edges
								if (!hasMiningNode) return;

								List<string> points;
								if (!pointsByRegion.TryGetValue(regionName, out points))
									pointsByRegion[regionName] = points = new List<string>();

								var z = map.GetHighestPoint(x, y);
								var point = new Point3D(x, y, z + 1);

								var label = debug ? string.Format("{0} ({1})", regionName, region.GetType().Name) : "";
								var icon = hasMiningNode ? MINEABLE_ICON : NON_MINEABLE_ICON;
								points.Add(string.Format("{0},{1},{2},{3},{4},,3", x, y, map.MapID, label, icon));
							}, false);
						}
					}

					// Write out nodes by Region, with the Unknown region being Last
					foreach (var region in pointsByRegion.Keys.OrderBy(x => x).ThenByDescending(x => x == RichVeinEngine.UNKNOWN_REGION_NAME))
					{
						writer.WriteLine("# {0}", region);

						pointsByRegion[region].ForEach(point => writer.WriteLine(point));
					}

					Console.WriteLine("Exported client markers for '{0}' to: {1}", map.Name, filename);

					Console.WriteLine("Completed in {0}ms ({1})", sw.ElapsedMilliseconds, map.Name);
				}
			}

			if (e.Mobile != null)
			{
				e.Mobile.SendMessage("Export of mineable edge tiles for all maps has completed.");
			}
		}

		[Usage("OreVein-LoadData <MapId> | NULL")]
		[Description("Imports XML configuration for all maps unless a `MapId` is specified.")]
		public static void OnLoadData(CommandEventArgs e)
		{
			IEnumerable<Map> maps;
			if (!TryGetMaps(e.Mobile, e.Arguments, out maps)) return;

			var sw = Stopwatch.StartNew();
			foreach (var map in maps)
			{
				var start = sw.ElapsedMilliseconds;
				var filename = Path.Combine("Data/RichVeins", map.Name + ".xml");
				if (!File.Exists(filename)) continue;

				using (var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
				{
					var root = XElement.Load(filestream);
					foreach (var configElement in root.Elements("Config"))
					{
						var configuration = new RichVeinConfig
						{
							Region = configElement.Attribute("region").Value,
							MinDistance = int.Parse(configElement.Attribute("minDistance").Value),
							MaxNodes = int.Parse(configElement.Attribute("maxNodes").Value),
							Candidates = configElement.Element("Candidates").Elements("Point").Select(pointElement =>
								{
									var x = int.Parse(pointElement.Attribute("x").Value);
									var y = int.Parse(pointElement.Attribute("y").Value);
									var z = int.Parse(pointElement.Attribute("z").Value);

									return new Point3D(x, y, z);
								}).ToList()
						};

						RichVeinEngine.Instance.AddConfig(map, configuration);
					}
				}

				Console.WriteLine("Loaded configuration for '{0}' in {1} ms", map.Name, sw.ElapsedMilliseconds - start);
			}

			if (e.Mobile != null)
			{
				e.Mobile.SendMessage("Loading configuration for OreVeinEngine took {0} ms for: {1}.", sw.ElapsedMilliseconds, string.Join(", ", maps.Select(map => map.Name)));
			}
		}

		[Usage("OreVein-Restart <DelaySeconds>")]
		[Description("Restarts the OreVeinEngine timer and sets it to execute after `DelaySeconds` seconds.")]
		public static void OnRestart(CommandEventArgs e)
		{
			if (e.Arguments.Length != 1)
			{
				e.Mobile.SendMessage("OreVein-Restart <DelaySeconds>");
				return;
			}

			var delaySeconds = e.GetInt32(0);
			RichVeinEngine.Instance.StartTimer(delaySeconds);
			e.Mobile.SendMessage("The OreVeinEngine will restart in '{0}' seconds", delaySeconds);
		}

		private static IEnumerable<Map> GetAllMaps()
		{
			foreach (var map in Map.AllMaps)
			{
				if (map == Map.Internal) continue;
				if (map == Map.Atlantis) continue; // Under development

				yield return map;
			}

			yield break;
		}

		private static bool TryGetMaps(Mobile mobile, string[] arguments, out IEnumerable<Map> maps)
		{
			maps = null;

			if (arguments.Length < 1)
			{
				maps = GetAllMaps();
				return true;
			}

			string mapIdArg = arguments[0];
			int mapId;
			if (!int.TryParse(mapIdArg, out mapId))
			{
				mobile.SendMessage("Map ID must be a valid number.");
				return false;
			}

			var map = Map.AllMaps.FirstOrDefault(m => m.MapID == mapId);
			if (map == null)
			{
				mobile.SendMessage("Failed to find a map with the ID '{0}'.", mapIdArg);
				return false;
			}

			maps = new[] { map };

			return true;
		}

		private static bool ValidateRegion(Region region)
		{
			if (ForceIncludedRegions.Contains(region.Name)) return true;

			if (IgnoredRegions.Contains(region.Name)) return false;
			if (IgnoredRegionTypes.Any(type => region.IsPartOf(type))) return false;

			return true;
		}
	}
}