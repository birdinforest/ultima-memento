using Server.Engines.MLQuests.Objectives;

namespace Server.Engines.MLQuests.Utilities
{
    public class DeliveryQuestUtilities
    {
        public static void AddBeginnerTravelTips(MLQuest quest)
        {
            quest.Objectives.Add(new DummyObjective("- Alt+R will bring up a mini-map"));
            quest.Objectives.Add(new DummyObjective("- Traveling the lands by foot is dangerous"));
            quest.Objectives.Add(new DummyObjective("- Use the public moongates to minimize time and hazards"));
            quest.Objectives.Add(new DummyObjective("- On your Paperdoll, click 'Help' -> 'Moongate Search'"));
            quest.Objectives.Add(new DummyObjective("to find a nearby moongate"));
            quest.Objectives.Add(new DummyObjective("- Humans can increase their movement speed by purchasing"));
            quest.Objectives.Add(new DummyObjective("a horse or llama from an Animal Trainer"));
            quest.Objectives.Add(new DummyObjective("- Humans can increase their movement speed by purchasing"));
            quest.Objectives.Add(new DummyObjective("'Hiking Boots' from a Cobbler"));
        }

        public static void AddNoviceTravelTips(MLQuest quest)
        {
            quest.Objectives.Add(new DummyObjective("- Use the Top Menu Bar to access a 'World Map'"));
            quest.Objectives.Add(new DummyObjective("- Unlock the world map via right-click -> 'Free view'"));
            quest.Objectives.Add(new DummyObjective("- Pan around the world map using left mouse click"));
            quest.Objectives.Add(new DummyObjective("- Use map markers to keep track of points of interest"));
        }
    }
}