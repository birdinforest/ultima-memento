using System;
using System.Collections.Generic;
using System.Text;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class BlacksmithBasicsQuest : MLQuest
    {
        public override Type NextQuest { get { return typeof(HintRingArmorQuest); } } // Optional delivery

        public BlacksmithBasicsQuest()
        {
            Activated = true;
            Title = "The Flame's Call";
            var builder = new StringBuilder();
            builder.Append("The sound of the hammer striking metal rings out like a heartbeat in the air, steady and rhythmic. In the nearby forge, you see glowing coals crackle and hiss as they feed the flames.");
            builder.Append("<br><br>");
            builder.Append("\"You there,\" the nearby Smith says, his voice gruff but warm, like a voice that has shouted over the roar of the forge for years. \"I don't take kindly to idle hands, but if you've got the fire in your belly and the grit in your bones, maybe it's time to teach you how to strike. You've got that look in your eyes - the one that says you want to learn, not just watch. Well, there's no better way than by swinging a hammer yourself.\"");
            builder.Append("<br><br>");
            builder.Append("If you're up for it, head to the mines and gather some Ore.");
            Description = builder.ToString();
            RefusalMessage = "RefusalMessage BlacksmithBasicsQuest";
            InProgressMessage = "InProgressMessage BlacksmithBasicsQuest";
            CompletionMessage = "CompletionMessage BlacksmithBasicsQuest";

            Objectives.Add(new DummyObjective("Collect the following:"));
            Objectives.Add(new CollectObjective(500, typeof(IronIngot), "Iron Ingots") { DoNotConsume = true });
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("* Click yourself to view your Quest Log"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- Tinkers can craft Shovels"));
            Objectives.Add(new DummyObjective("- Tinkers and Miners sell Shovels"));
            Objectives.Add(new DummyObjective("- Double-click an item to use it"));
            Objectives.Add(new DummyObjective("- Shovels are used on the mountainside or cave floor"));
            Objectives.Add(new DummyObjective("- Ore is used on a Forge to make Ingots"));

            Rewards.Add(new ItemReward("Smith Hammer", typeof(SmithHammer)));
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(Blacksmith); // Quest Giver - Any Blacksmith
        }

        public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
        {
            if (!base.CanOffer(quester, pm, context, message)) return false;

            return pm.Skills.Blacksmith.Value < 60; // Only offer to "beginners" in the skill
        }

        public override void OnAccepted(IQuestGiver quester, MLQuestInstance instance)
        {
            base.OnAccepted(quester, instance);
            MLQuestSystem.Tell(quester, instance.Player, "Take this shovel.");
            instance.Player.AddToBackpack(new TrainingShovel());
            instance.Player.SendMessage("You have received a Training Shovel.");
        }
    }
}

#region Items

namespace Server.Items
{
    public class TrainingShovel : Spade
    {
        [Constructable]
        public TrainingShovel() : base()
        {
            Name = "training shovel";
            Weight = 1;
        }

        public TrainingShovel(Serial serial) : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add("Drag onto Paperdoll");
            list.Add("Only mines Iron Ore");
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
#endregion
