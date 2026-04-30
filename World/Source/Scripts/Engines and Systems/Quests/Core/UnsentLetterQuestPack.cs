using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests
{
	/// <summary>
	/// JSON task pack for The Unsent Letter (<c>Data/Quests/unsent-letter-pack.json</c> under <see cref="Core.BaseDirectory"/>).
	/// On load failure, built-in defaults match legacy hardcoded values.
	/// </summary>
	public static class UnsentLetterQuestPackLoader
	{
		public const string RelativePath = "Data/Quests/unsent-letter-pack.json";

		private static bool m_Initialized;
		private static UnsentLetterPackRoot m_Root;

		public static UnsentLetterPackRoot Root
		{
			get
			{
				EnsureLoaded();
				return m_Root;
			}
		}

		public static void EnsureLoaded()
		{
			if (m_Initialized)
				return;

			m_Initialized = true;
			string path = Path.Combine(Core.BaseDirectory, RelativePath);

			try
			{
				if (File.Exists(path))
				{
					string json = File.ReadAllText(path, Encoding.UTF8);

					using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
					{
						DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(UnsentLetterPackRoot));
						UnsentLetterPackRoot loaded = ser.ReadObject(ms) as UnsentLetterPackRoot;

						if (loaded != null && loaded.schemaVersion >= 1)
						{
							m_Root = loaded;
							Console.WriteLine("UnsentLetterQuestPack: loaded {0}", path);
							return;
						}
					}

					Console.WriteLine("UnsentLetterQuestPack: invalid schemaVersion or empty object in {0}; using built-in defaults.", path);
				}
				else
					Console.WriteLine("UnsentLetterQuestPack: file not found ({0}); using built-in defaults.", path);
			}
			catch (Exception e)
			{
				Console.WriteLine("UnsentLetterQuestPack: load failed ({0}): {1}", path, e.Message);
			}

			m_Root = UnsentLetterPackRoot.CreateBuiltinDefaults();
		}
	}

	[DataContract]
	public class UnsentLetterPackRoot
	{
		[DataMember]
		public int schemaVersion { get; set; }

		[DataMember]
		public string questId { get; set; }

		[DataMember]
		public List<UnsentLetterPackSpawnerEntry> spawners { get; set; }

		[DataMember]
		public UnsentLetterPackAmbush ambush { get; set; }

		[DataMember]
		public UnsentLetterPackClerkFight clerkFight { get; set; }

		[DataMember]
		public UnsentLetterPackQuestItems questItems { get; set; }

		public static UnsentLetterPackRoot CreateBuiltinDefaults()
		{
			return new UnsentLetterPackRoot
			{
				schemaVersion = 1,
				questId = "unsent-letter",
				spawners = new List<UnsentLetterPackSpawnerEntry>
				{
					new UnsentLetterPackSpawnerEntry { typeName = "UnsentLetterMara", map = "Sosaria", x = 2999, y = 1064, z = 0 },
					new UnsentLetterPackSpawnerEntry { typeName = "UnsentLetterLina", map = "Sosaria", x = 2997, y = 1061, z = 0 },
					new UnsentLetterPackSpawnerEntry { typeName = "UnsentLetterThomas", map = "Sosaria", x = 1458, y = 3788, z = 0 },
					new UnsentLetterPackSpawnerEntry { typeName = "UnsentLetterMiner", map = "Sosaria", x = 3185, y = 2585, z = 0 },
					new UnsentLetterPackSpawnerEntry { typeName = "UnsentLetterClerk", map = "Sosaria", x = 1455, y = 3792, z = 0 }
				},
				ambush = UnsentLetterPackAmbush.CreateBuiltin(),
				clerkFight = UnsentLetterPackClerkFight.CreateBuiltin(),
				questItems = UnsentLetterPackQuestItems.CreateBuiltin()
			};
		}
	}

	[DataContract]
	public class UnsentLetterPackSpawnerEntry
	{
		[DataMember]
		public string typeName { get; set; }

		[DataMember]
		public string map { get; set; }

		[DataMember]
		public int x { get; set; }

		[DataMember]
		public int y { get; set; }

		[DataMember]
		public int z { get; set; }

		[DataMember]
		public int amount { get; set; } = 1;

		[DataMember]
		public int minDelayMinutes { get; set; } = 5;

		[DataMember]
		public int maxDelayMinutes { get; set; } = 10;

		[DataMember]
		public int team { get; set; }

		[DataMember]
		public int homeRange { get; set; } = 5;

		/// <summary>-1 = spawner default.</summary>
		[DataMember]
		public int walkingRange { get; set; } = -1;
	}

	[DataContract]
	public class UnsentLetterPackAmbush
	{
		[DataMember]
		public List<UnsentLetterPackAmbushWave> waves { get; set; }

		[DataMember]
		public int spawnOffsetMin { get; set; } = -3;

		[DataMember]
		public int spawnOffsetMax { get; set; } = 3;

		[DataMember]
		public bool buffSecondWaveHits { get; set; } = true;

		[DataMember]
		public string brigandTypeName { get; set; }

		[DataMember]
		public int brigandBody { get; set; } = -1;

		[DataMember]
		public int brigandHue { get; set; } = -1;

		[DataMember]
		public List<UnsentLetterPackEquipEntry> equipment { get; set; }

		public static UnsentLetterPackAmbush CreateBuiltin()
		{
			return new UnsentLetterPackAmbush
			{
				waves = new List<UnsentLetterPackAmbushWave>
				{
					new UnsentLetterPackAmbushWave { count = 3 },
					new UnsentLetterPackAmbushWave { count = 4 }
				},
				spawnOffsetMin = -3,
				spawnOffsetMax = 3,
				buffSecondWaveHits = true,
				brigandTypeName = "UnsentGreyCloakBrigand",
				brigandBody = -1,
				brigandHue = -1,
				equipment = new List<UnsentLetterPackEquipEntry>()
			};
		}
	}

	[DataContract]
	public class UnsentLetterPackAmbushWave
	{
		[DataMember]
		public int count { get; set; }
	}

	[DataContract]
	public class UnsentLetterPackEquipEntry
	{
		[DataMember]
		public string typeName { get; set; }

		[DataMember]
		public int hue { get; set; }

		/// <summary>Optional layer (e.g. Layer.OneHanded). 0 = omit / let item default.</summary>
		[DataMember]
		public int layer { get; set; }
	}

	[DataContract]
	public class UnsentLetterPackClerkFight
	{
		[DataMember]
		public int hirelingCount { get; set; } = 4;

		[DataMember]
		public string hirelingTypeName { get; set; }

		[DataMember]
		public int hirelingExtraHits { get; set; } = 40;

		[DataMember]
		public int hirelingBody { get; set; } = -1;

		[DataMember]
		public int hirelingHue { get; set; } = -1;

		[DataMember]
		public List<UnsentLetterPackOffset2D> offsets { get; set; }

		[DataMember]
		public List<UnsentLetterPackEquipEntry> equipment { get; set; }

		public static UnsentLetterPackClerkFight CreateBuiltin()
		{
			return new UnsentLetterPackClerkFight
			{
				hirelingCount = 4,
				hirelingTypeName = "UnsentHireling",
				hirelingExtraHits = 40,
				hirelingBody = -1,
				hirelingHue = -1,
				offsets = new List<UnsentLetterPackOffset2D>
				{
					new UnsentLetterPackOffset2D { ox = -1, oy = -1 },
					new UnsentLetterPackOffset2D { ox = 1, oy = -1 },
					new UnsentLetterPackOffset2D { ox = -1, oy = 1 },
					new UnsentLetterPackOffset2D { ox = 1, oy = 1 }
				},
				equipment = new List<UnsentLetterPackEquipEntry>()
			};
		}
	}

	[DataContract]
	public class UnsentLetterPackOffset2D
	{
		[DataMember(Name = "x")]
		public int ox { get; set; }

		[DataMember(Name = "y")]
		public int oy { get; set; }
	}

	[DataContract]
	public class UnsentLetterPackQuestItems
	{
		[DataMember]
		public UnsentLetterPackQuestItem adrianBadge { get; set; }

		[DataMember]
		public UnsentLetterPackQuestItem tornPage { get; set; }

		[DataMember]
		public UnsentLetterPackQuestItem fullLetter { get; set; }

		public static UnsentLetterPackQuestItems CreateBuiltin()
		{
			return new UnsentLetterPackQuestItems
			{
				adrianBadge = new UnsentLetterPackQuestItem { itemId = 0x14ED, hue = 2213, weight = 1.0 },
				tornPage = new UnsentLetterPackQuestItem { itemId = 0x14ED, hue = 1150, weight = 1.0 },
				fullLetter = new UnsentLetterPackQuestItem { itemId = 0x14ED, hue = 1160, weight = 1.0 }
			};
		}
	}

	[DataContract]
	public class UnsentLetterPackQuestItem
	{
		/// <summary>If non-zero and different from constructed item, ItemID is reassigned (visual only).</summary>
		[DataMember]
		public int itemId { get; set; }

		[DataMember]
		public int hue { get; set; } = -1;

		[DataMember]
		public double weight { get; set; } = -1;
	}

	/// <summary>Runtime helpers used from <see cref="Server.Engines.MLQuests.Definitions"/> quest code.</summary>
	public static class UnsentLetterQuestPackRuntime
	{
		public static int AmbushWaveCount(int waveIndex)
		{
			UnsentLetterQuestPackLoader.EnsureLoaded();
			List<UnsentLetterPackAmbushWave> waves = UnsentLetterQuestPackLoader.Root?.ambush?.waves;

			if (waves != null && waveIndex >= 0 && waveIndex < waves.Count && waves[waveIndex].count > 0)
				return waves[waveIndex].count;

			return waveIndex <= 0 ? 3 : 4;
		}

		public static void ApplyQuestItem(Item item, UnsentLetterPackQuestItem cfg)
		{
			if (item == null || cfg == null)
				return;

			try
			{
				if (cfg.itemId > 0 && item.ItemID != cfg.itemId)
					item.ItemID = cfg.itemId;

				if (cfg.hue >= 0)
					item.Hue = cfg.hue;

				if (cfg.weight >= 0)
					item.Weight = cfg.weight;
			}
			catch (Exception e)
			{
				Console.WriteLine("UnsentLetterQuestPack: ApplyQuestItem skipped: {0}", e.Message);
			}
		}

		public static UnsentLetterPackQuestItem ItemConfigForBadge()
		{
			UnsentLetterQuestPackLoader.EnsureLoaded();
			return UnsentLetterQuestPackLoader.Root?.questItems?.adrianBadge;
		}

		public static UnsentLetterPackQuestItem ItemConfigForTornPage()
		{
			UnsentLetterQuestPackLoader.EnsureLoaded();
			return UnsentLetterQuestPackLoader.Root?.questItems?.tornPage;
		}

		public static UnsentLetterPackQuestItem ItemConfigForFullLetter()
		{
			UnsentLetterQuestPackLoader.EnsureLoaded();
			return UnsentLetterQuestPackLoader.Root?.questItems?.fullLetter;
		}

		public static Mobile CreateBrigandForAmbush(PlayerMobile pm, int wavePhase)
		{
			UnsentLetterQuestPackLoader.EnsureLoaded();
			UnsentLetterPackAmbush a = UnsentLetterQuestPackLoader.Root?.ambush;
			string typeName = (a != null && !String.IsNullOrWhiteSpace(a.brigandTypeName))
				? a.brigandTypeName.Trim()
				: "UnsentGreyCloakBrigand";

			Type t = ScriptCompiler.FindTypeByName(typeName);

			if (t == null || !typeof(Mobile).IsAssignableFrom(t))
			{
				Console.WriteLine("UnsentLetterQuestPack: brigand type '{0}' invalid; using UnsentGreyCloakBrigand.", typeName);
				t = ScriptCompiler.FindTypeByName("UnsentGreyCloakBrigand");
			}

			if (t == null)
				return null;

			object o;

			try
			{
				o = Activator.CreateInstance(t);
			}
			catch (Exception e)
			{
				Console.WriteLine("UnsentLetterQuestPack: CreateInstance brigand failed: {0}", e.Message);
				return null;
			}

			Mobile m = o as Mobile;

			if (m == null)
				return null;

			ApplyMobileAppearance(m, a?.brigandBody ?? -1, a?.brigandHue ?? -1, a?.equipment);

			UnsentGreyCloakBrigand br = m as UnsentGreyCloakBrigand;

			if (br != null)
				br.QuestPlayer = pm;

			if (a != null && a.buffSecondWaveHits && wavePhase >= 2)
				m.SetHits(Math.Min(m.HitsMax + 25, m.HitsMax + (int)(m.HitsMax * 0.18)));

			return m;
		}

		public static Mobile CreateHirelingForClerk(PlayerMobile pm)
		{
			UnsentLetterQuestPackLoader.EnsureLoaded();
			UnsentLetterPackClerkFight c = UnsentLetterQuestPackLoader.Root?.clerkFight;
			string typeName = (c != null && !String.IsNullOrWhiteSpace(c.hirelingTypeName))
				? c.hirelingTypeName.Trim()
				: "UnsentHireling";

			Type t = ScriptCompiler.FindTypeByName(typeName);

			if (t == null || !typeof(Mobile).IsAssignableFrom(t))
			{
				Console.WriteLine("UnsentLetterQuestPack: hireling type '{0}' invalid; using UnsentHireling.", typeName);
				t = ScriptCompiler.FindTypeByName("UnsentHireling");
			}

			if (t == null)
				return null;

			Mobile m;

			try
			{
				m = Activator.CreateInstance(t) as Mobile;
			}
			catch (Exception e)
			{
				Console.WriteLine("UnsentLetterQuestPack: CreateInstance hireling failed: {0}", e.Message);
				return null;
			}

			if (m == null)
				return null;

			ApplyMobileAppearance(m, c?.hirelingBody ?? -1, c?.hirelingHue ?? -1, c?.equipment);

			UnsentHireling h = m as UnsentHireling;

			if (h != null)
				h.QuestPlayer = pm;
			else if (c != null && c.hirelingExtraHits > 0)
				m.SetHits(m.HitsMax + c.hirelingExtraHits);

			return m;
		}

		public static void SpawnAmbushMobile(PlayerMobile pm, Mobile m, int wavePhase)
		{
			if (pm == null || m == null)
				return;

			UnsentLetterQuestPackLoader.EnsureLoaded();
			UnsentLetterPackAmbush a = UnsentLetterQuestPackLoader.Root?.ambush;
			int min = a?.spawnOffsetMin ?? -3;
			int max = a?.spawnOffsetMax ?? 3;

			Point3D o = pm.Location;
			Map map = pm.Map;

			if (map == null || map == Map.Internal)
				return;

			m.MoveToWorld(new Point3D(o.X + Utility.RandomMinMax(min, max), o.Y + Utility.RandomMinMax(min, max), o.Z), map);

			m.Combatant = pm;
			m.FocusMob = pm;
		}

		private static void ApplyMobileAppearance(Mobile m, int body, int hue, List<UnsentLetterPackEquipEntry> equipment)
		{
			if (m == null)
				return;

			if (body >= 0)
				m.Body = body;

			if (hue >= 0)
				m.Hue = hue;

			if (equipment == null || equipment.Count == 0)
				return;

			m.DropHolding();

			List<Item> before = new List<Item>(m.Items);

			foreach (Item old in before)
				old.Delete();

			foreach (UnsentLetterPackEquipEntry e in equipment)
			{
				if (e == null || String.IsNullOrWhiteSpace(e.typeName))
					continue;

				Type t = ScriptCompiler.FindTypeByName(e.typeName.Trim());

				if (t == null || !typeof(Item).IsAssignableFrom(t))
				{
					Console.WriteLine("UnsentLetterQuestPack: equipment type '{0}' not found.", e.typeName);
					continue;
				}

				Item it;

				try
				{
					it = Activator.CreateInstance(t) as Item;
				}
				catch
				{
					continue;
				}

				if (it == null)
					continue;

				if (e.hue >= 0)
					it.Hue = e.hue;

				if (!m.EquipItem(it))
				{
					if (!m.AddItem(it))
						m.AddToBackpack(it);
				}
			}
		}

		public static int ClerkHirelingCount()
		{
			UnsentLetterQuestPackLoader.EnsureLoaded();
			int n = UnsentLetterQuestPackLoader.Root?.clerkFight?.hirelingCount ?? 4;

			return n > 0 ? n : 4;
		}

		public static Point3D ClerkOffset(int index)
		{
			UnsentLetterQuestPackLoader.EnsureLoaded();
			List<UnsentLetterPackOffset2D> offs = UnsentLetterQuestPackLoader.Root?.clerkFight?.offsets;

			if (offs != null && index >= 0 && index < offs.Count)
				return new Point3D(offs[index].ox, offs[index].oy, 0);

			int ox = (index % 2) * 2 - 1;
			int oy = (index / 2) * 2 - 1;

			return new Point3D(ox, oy, 0);
		}
	}
}
