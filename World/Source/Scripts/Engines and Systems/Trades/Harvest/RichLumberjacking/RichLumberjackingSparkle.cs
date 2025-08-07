using Server.Items;
using Server.Items.Abstractions;

namespace Server.Engines.Harvest
{
    /// <summary>
    /// This should only be programatically created
    /// </summary>
    public class RichLumberjackingSparkle : Item, IValidate
    {
		private Item m_LightSource;

        public RichLumberjackingSparkle() : base(0x373A)
        {
            Name = "A rich tree";
			m_LightSource = new LighterSource();
        }

        public RichLumberjackingSparkle(Serial serial) : base(serial)
        {
        }

		public override void OnDelete()
		{
			base.OnDelete();

			if (m_LightSource != null && !m_LightSource.Deleted)
				m_LightSource.Delete();
		}

		public override void OnMapChange()
		{
			base.OnMapChange();

			if (m_LightSource != null && !m_LightSource.Deleted)
				m_LightSource.MoveToWorld(Location, Map);
		}

		public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

			if (0 < version)
				m_LightSource = reader.ReadItem();
			
			ValidationQueue.Add(this);
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

            writer.Write((int)1); // version
			writer.Write(m_LightSource);
        }

		public void Validate()
		{
			Delete();
		}
	}
}