using Server;
using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
using Server.Commands;
using Server.Misc;
using Scripts.Mythik.Systems.Achievements.Gumps;

namespace Scripts.Mythik.Systems.Achievements
{
    public class AchievementSystem
    {
        public class AchievementCategory
        {
            public int ID { get; set; }
            public int Parent { get; set; }
            public string Name;


            public AchievementCategory(int id, int parent, string v3)
            {
                ID = id;
                Parent = parent;
                Name = v3;
            }
        }

        public static List<BaseAchievement> Achievements = new List<BaseAchievement>();
        public static List<AchievementCategory> Categories = new List<AchievementCategory>();
        private static Dictionary<string, Dictionary<int, AchieveData>> m_featData = new Dictionary<string, Dictionary<int, AchieveData>>();
        private static Dictionary<Serial, int> m_pointsTotal = new Dictionary<Serial, int>();

        private static int GetPlayerPointsTotal(PlayerMobile m)
        {
            if (!m_pointsTotal.ContainsKey(m.Serial))
                m_pointsTotal.Add(m.Serial, 0);
            return m_pointsTotal[m.Serial];
        }

        private static void AddPoints(PlayerMobile m, int points)
        {
            if (!m_pointsTotal.ContainsKey(m.Serial))
                m_pointsTotal.Add(m.Serial, 0);
            m_pointsTotal[m.Serial] += points;
        }

        public static void Initialize()
        {
            BaseAchievement achieve = null;
            const int DISCOVER_LAND_GRAPHIC = 0x14EB; // Map

            #region Exploration - 1 to 999
            Categories.Add(new AchievementCategory(1, 0, "Exploration"));
            var discoverSosaria       = Add(new DiscoverLandAchievement(1,  1, DISCOVER_LAND_GRAPHIC, false, null, "One Small Step", "Discover the World of Sosaria", 5, Land.Sosaria));
            var discoverUmberVeil     = Add(new DiscoverLandAchievement(2,  1, DISCOVER_LAND_GRAPHIC, false, null, "Through The Veil", "Discover the land of Umber Veil", 5, Land.UmberVeil));
            var discoverAmbrosia      = Add(new DiscoverLandAchievement(3,  1, DISCOVER_LAND_GRAPHIC, false, null, "The Lost Land", "Discover the land of Ambrosia", 5, Land.Ambrosia));
            var discoverLodoria       = Add(new DiscoverLandAchievement(4,  1, DISCOVER_LAND_GRAPHIC, false, null, "One Does Not Simply...", "Discover the Elven World of Lodoria", 5, Land.Lodoria));
            var discoverSerpentIsland = Add(new DiscoverLandAchievement(5,  1, DISCOVER_LAND_GRAPHIC, false, null, "Hisstory In The Making", "Discover the Serpent Island", 5, Land.Serpent));
            var discoverDreadIsles    = Add(new DiscoverLandAchievement(6,  1, DISCOVER_LAND_GRAPHIC, false, null, "Dread The Unknown", "Discover the Isles of Dread", 5, Land.IslesDread));
            var discoverSavagedEmpire = Add(new DiscoverLandAchievement(7,  1, DISCOVER_LAND_GRAPHIC, false, null, "This Party Is Savage", "Discover the Savaged Empire", 5, Land.Savaged));
            var discoverBottleWorld   = Add(new DiscoverLandAchievement(8,  1, DISCOVER_LAND_GRAPHIC, false, null, "Message In A Bottle", "Discover the Bottle World of Kuldar", 5, Land.Kuldar));
            var discoverUnderworld    = Add(new DiscoverLandAchievement(9,  1, DISCOVER_LAND_GRAPHIC, false, null, "Into The Darkness", "Discover the Underworld", 5, Land.Underworld));
            var discoverLuna          = Add(new DiscoverLandAchievement(10, 1, DISCOVER_LAND_GRAPHIC, false, null, "Blastoff!", "Discover the City of Luna", 5, Land.Luna));
            var discoverSkara         = Add(new DiscoverLandAchievement(11, 1, DISCOVER_LAND_GRAPHIC, false, null, "Help! I'm Trapped!", "Discover the Town of Skara Brae", 5, Land.SkaraBrae));
            var discoverAtlantis      = Add(new DiscoverLandAchievement(12, 1, DISCOVER_LAND_GRAPHIC, true,  null, "The Lost City", "Discover the World of Atlantis", 5, Land.Atlantis)); // TODO: No way atm?
            // 13
            // 14
            // 15

            Categories.Add(new AchievementCategory(2, 1, "Towns"));
            Categories.Add(new AchievementCategory(3, 1, "Dungeons"));
            Categories.Add(new AchievementCategory(4, 1, "Points of Interest"));
            #endregion Exploration - 1 to 999

            #region Hunting - 1000 to 1999
            Categories.Add(new AchievementCategory(1000, 0, "Hunting"));
            #endregion Hunting - 1000 to 1999

            #region Resource Gathering - 2000 to 2999
            Categories.Add(new AchievementCategory(2000, 0, "Resource Gathering"));
            #endregion Resource Gathering - 2000 to 2999

            #region Crafting - 3000 to 3999
            Categories.Add(new AchievementCategory(3000, 0, "Crafting"));
            Categories.Add(new AchievementCategory(3001, 3000, "Alchemy"));
            Categories.Add(new AchievementCategory(3002, 3000, "Blacksmith"));
            Categories.Add(new AchievementCategory(3003, 3000, "Bowcraft"));
            Categories.Add(new AchievementCategory(3004, 3000, "Carpentry"));
            Categories.Add(new AchievementCategory(3005, 3000, "Cooking"));
            Categories.Add(new AchievementCategory(3006, 3000, "Inscription"));
            Categories.Add(new AchievementCategory(3007, 3000, "Tailoring"));
            Categories.Add(new AchievementCategory(3008, 3000, "Tinkering"));
            #endregion Crafting - 3000 to 3999

            #region Character Development - 4000 to 4999
            Categories.Add(new AchievementCategory(4000, 0, "Character Development"));
            #endregion Character Development - 4000 to 4999

            CommandSystem.Register("feats", AccessLevel.Player, new CommandEventHandler(OpenGumpCommand));
            EventSink.WorldSave += EventSink_WorldSave;
            LoadData();
        }

        private static DiscoverLandAchievement Add(DiscoverLandAchievement achievement)
        {
            achievement.HideDesc = true;
            Achievements.Add(achievement);

            return achievement;
        }

        private static void LoadData()
        {
            Persistence.Deserialize(
                "Saves//Achievements.bin",
                reader =>
                {
                    int count = reader.ReadInt();

                    for (int i = 0; i < count; ++i)
                    {
                        m_pointsTotal.Add(reader.ReadInt(), reader.ReadInt());
                    }

                    count = reader.ReadInt();

                    for (int i = 0; i < count; ++i)
                    {
                        var id = reader.ReadString();
                        var dict = new Dictionary<int, AchieveData>();
                        int iCount = reader.ReadInt();
                        if (iCount > 0)
                        {
                            for (int x = 0; x < iCount; x++)
                            {
                                dict.Add(reader.ReadInt(), new AchieveData(reader));
                            }

                        }
                        m_featData.Add(id, dict);
                    }
                    Console.WriteLine("Loaded Achievements store: " + m_featData.Count);
                }
            );
        }

        private static void EventSink_WorldSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                "Saves//Achievements.bin",
                writer =>
                {
                    writer.Write(m_pointsTotal.Count);
                    foreach (var kv in m_pointsTotal)
                    {
                        writer.Write(kv.Key);
                        writer.Write(kv.Value);
                    }

                    writer.Write(m_featData.Count);
                    foreach (var kv in m_featData)
                    {
                        writer.Write(kv.Key);

                        writer.Write(kv.Value.Count);

                        foreach (var ckv in kv.Value)
                        {
                            writer.Write(ckv.Key);
                            ckv.Value.Serialize(writer);
                        }
                    }
                }
            );
        }

        public static void OpenGump(Mobile from, Mobile target)
        {
            if (from == null || target == null) return;

            var player = target as PlayerMobile;
            if (player == null || player.Account == null) return;

            var id = player.Account.Username;
            if (!m_featData.ContainsKey(id)) m_featData.Add(id, new Dictionary<int, AchieveData>());

            var achieves = m_featData[id];
            var total = GetPlayerPointsTotal(player);

            from.SendGump(new AchievementGump(achieves, total));
        }

        [Usage("feats"), Aliases("achievement", "achievements")]
        [Description("Opens the Achievements gump")]
        private static void OpenGumpCommand(CommandEventArgs e)
        {
            OpenGump(e.Mobile, e.Mobile);
        }

        internal static void SetAchievementStatus(PlayerMobile player, BaseAchievement ach, int progress)
        {
            if (player == null || player.Account == null) return;

            var id = player.Account.Username;
            if (!m_featData.ContainsKey(id)) m_featData.Add(id, new Dictionary<int, AchieveData>());

            var achieves = m_featData[id];

            if (achieves.ContainsKey(ach.ID))
            {
                if (achieves[ach.ID].Progress >= ach.CompletionTotal) return;

                achieves[ach.ID].Progress += progress;
            }
            else
            {
                achieves.Add(ach.ID, new AchieveData() { Progress = progress });
            }

            if (achieves[ach.ID].Progress >= ach.CompletionTotal)
            {
                player.SendAsciiMessage("You have earned the achievement: {0}.", ach.Title);
                player.PlaySound(0x215); // Summon creature
                player.SendGump(new AchievementObtainedGump(ach), false);
                achieves[ach.ID].CompletedOn = DateTime.UtcNow;

                AddPoints(player, ach.RewardPoints);

                if (ach.RewardItems != null && ach.RewardItems.Length > 0)
                {
                    bool success = false;
                    foreach (var itemType in ach.RewardItems)
                    {
                        try
                        {
                            var item = (Item)Activator.CreateInstance(itemType);
                            if (!WeightOverloading.IsOverloaded(player))
                                player.Backpack.DropItem(item);
                            else
                                player.BankBox.DropItem(item);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to create achievement reward ({0}). {1}", itemType, ex.Message);
                        }
                    }

                    if (success)
                        player.SendAsciiMessage("You have recieved an award for completing this achievment!");
                }
            }
        }
    }
}
