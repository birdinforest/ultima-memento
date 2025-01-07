using System.Linq;
using Server.Items.Abstractions;

namespace Server.Engines.Harvest
{
    /// <summary>
    /// This should only be programatically created
    /// </summary>
    public class RichLumberjackingSparkle : Item
    {
        public RichLumberjackingSparkle() : base(0x373A)
        {
            Name = "A rich tree";
            Light = LightType.Circle300;
        }

        public RichLumberjackingSparkle(Serial serial) : base(serial)
        {
        }

        public static void Configure()
        {
            EventSink.WorldLoad += OnWorldLoad;
        }

        private static void OnWorldLoad()
        {
            // Should never exist after a Restart
            World.Items.Values
                .Where(x => x is RichLumberjackingSparkle)
                .ToList()
                .ForEach(x => x.Delete());
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            // Automatically deleted
            Visible = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            var tool = from.FindItemOnLayer(Layer.TwoHanded) as IHarvestTool;
            if (tool != null && tool != null && tool.HarvestSystem == RichLumberjacking.System)
            {
                tool.HarvestSystem.StartHarvesting(from, (Item)tool, this);
            }
            else
            {
                from.SendMessage("The tree catches your eye. An axe might help.");
            }
        }

        public override bool OnDragLift(Mobile from)
        {
            // Entity is Visible so Ctrl+Shift can show it
            // Disallow moving it
            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }
    }
}