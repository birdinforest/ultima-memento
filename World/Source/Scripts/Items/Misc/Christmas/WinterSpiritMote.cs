using System;
using Server.Engines.Events.Christmas;

namespace Server.Items
{
	/// <summary>
	/// Midwinter event currency. Dropped from monster kills during December.
	/// Redeem with Eira of the Winter Court for Category.Christmas rewards.
	/// </summary>
	public class WinterSpiritMote : Item
	{
		[Constructable]
		public WinterSpiritMote() : this(1)
		{
		}

		[Constructable]
		public WinterSpiritMote(int amount) : base(0x232E)
		{
			Name = "Winter Spirit Mote";
			Hue = 1150;
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
			LootType = LootType.Blessed;
		}

		public WinterSpiritMote(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add("Redeem with Eira of the Winter Court");

			if (ChristmasEvent.IsActive)
				list.Add("Midwinter Favor — event currency");
			else
				list.Add("A lingering shard of midwinter frost");
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
