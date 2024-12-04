using Server.Engines.MLQuests.Gumps;
using Server.Gumps;

namespace Server.Engines.MLQuests.Objectives
{
    public class DummyObjective : BaseObjective
    {
        public readonly TextDefinition Name;

        public DummyObjective(TextDefinition message)
        {
            Name = message;
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            if (Name.Number > 0)
                g.AddHtmlLocalized(98, y, 312, 32, Name.Number, BaseQuestGump.COLOR_LOCALIZED, false, false);
            else if (Name.String != null)
                g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, Name.String);

            y += 16;
        }

        public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
        {
            return new DummyObjectiveInstance(instance, this);
        }
    }

    public class DummyObjectiveInstance : BaseObjectiveInstance
    {
        public DummyObjectiveInstance(MLQuestInstance instance, BaseObjective obj) : base(instance, obj)
        {
        }

        public override bool IsCompleted()
        {
            return true;
        }
    }
}
