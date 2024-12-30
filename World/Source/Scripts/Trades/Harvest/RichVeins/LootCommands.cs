using System.Collections.Generic;
using Server.Items;

namespace Server.Commands
{
    public class LootCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("Loot-AddPoor", AccessLevel.GameMaster, args => OnAdd(TreasureType.Poor, args));
            CommandSystem.Register("Loot-AddMeager", AccessLevel.GameMaster, args => OnAdd(TreasureType.Meager, args));
            CommandSystem.Register("Loot-AddAverage", AccessLevel.GameMaster, args => OnAdd(TreasureType.Average, args));
            CommandSystem.Register("Loot-AddRich", AccessLevel.GameMaster, args => OnAdd(TreasureType.Rich, args));
            CommandSystem.Register("Loot-AddFilthyRich", AccessLevel.GameMaster, args => OnAdd(TreasureType.FilthyRich, args));
            CommandSystem.Register("Loot-AddUltraRich", AccessLevel.GameMaster, args => OnAdd(TreasureType.UltraRich, args));
        }

        [Usage("Loot-AddXX <KillerLuck (int)> <BonusItemLevel (int [0-10])> <Iterations (int | NULL)>")]
        [Description("Generates loot for the provided configuration.")]
        private static void OnAdd(TreasureType type, CommandEventArgs e)
        {
            if (e.Mobile == null) return;

            var config = TryCreateLootConfig(e.Arguments, e.Mobile);
            if (config == null) return;

            var bag = new Bag();
            for (var i = 0; i < config.Count; i++)
            {
                var lootPack = GetLootPack(type);
                lootPack.Generate(e.Mobile, bag, config.Spawning, config.KillerLuck, 0, config.BonusItemLevel);
            }

            // Unbox any unidentified objects
            foreach (var item in bag.Items.ToArray())
            {
                if (item is NotIdentified)
                {
                    foreach (var newItem in item.Items.ToArray())
                    {
                        bag.TryDropItem(e.Mobile, newItem, false);
                    }

                    item.Delete();
                }
            }

            // Delete stackable objects that were created
            foreach (var item in bag.Items.ToArray())
            {
                if (item.Deleted) continue;
                if (item.Stackable) item.Delete();
            }

            // Rename the bag for history sake
            bag.Name = string.Format("({0}) Luck ({1}) // Bonus Item Level ({2})", type, config.KillerLuck, config.BonusItemLevel);
            e.Mobile.Backpack.AddItem(bag);
        }

        private static LootConfig TryCreateLootConfig(string[] arguments, Mobile mobile)
        {
            if (arguments.Length < 2)
            {
                mobile.SendMessage("Arguments for the command are <KillerLuck (int)> <BonusItemLevel (int [0-10])> <Iterations (int | NULL)>");
                return null;
            }

            int killerLuck;
            if (!int.TryParse(arguments[0], out killerLuck))
            {
                mobile.SendMessage("Killer Luck must be a valid number.");
                return null;
            }

            int bonusItemLevel;
            if (!int.TryParse(arguments[1], out bonusItemLevel) || bonusItemLevel < 0 || 10 < bonusItemLevel)
            {
                mobile.SendMessage("Bonus Item Level must be a number between 0 and 10.");
                return null;
            }

            int count = 1;
            if (2 < arguments.Length && !int.TryParse(arguments[2], out count))
            {
                mobile.SendMessage("Count must be a valid number.");
                return null;
            }

            return new LootConfig
            {
                Spawning = false, // TODO: Idk
                KillerLuck = killerLuck,
                BonusItemLevel = bonusItemLevel,
                Count = count
            };
        }

        private enum TreasureType
        {
            Poor,
            Meager,
            Average,
            Rich,
            FilthyRich,
            UltraRich
        }

        private static LootPack GetLootPack(TreasureType type)
        {
            switch (type)
            {
                default:
                case TreasureType.Poor: return LootPack.TreasurePoor;
                case TreasureType.Meager: return LootPack.TreasureMeager;
                case TreasureType.Average: return LootPack.TreasureAverage;
                case TreasureType.Rich: return LootPack.TreasureRich;
                case TreasureType.FilthyRich: return LootPack.TreasureFilthyRich;
                case TreasureType.UltraRich: return LootPack.TreasureUltraRich;
            }
        }

        private class LootConfig
        {
            public bool Spawning { get; set; }
            public int KillerLuck { get; set; }
            public int BonusItemLevel { get; set; } // 0 - 10
            public int Count { get; set; }
        }
    }
}