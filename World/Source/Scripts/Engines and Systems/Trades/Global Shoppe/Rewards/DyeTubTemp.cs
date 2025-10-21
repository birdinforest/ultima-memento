using Server.Misc;
using Server.Targeting;

namespace Server.Items
{
	public abstract class DyeTubTempBase : Item
	{
		private int m_DefaultTubHue;
		private int m_Dye;
		private int m_Uses;

		public DyeTubTempBase(Serial serial) : base(serial)
		{
		}

		protected DyeTubTempBase() : this(20)
		{
		}

		protected DyeTubTempBase(int uses) : base(0x4C5A)
		{
			Weight = 2.0;
			m_Uses = uses;
			Reset(false);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int DefaultTubHue
		{
			get { return m_DefaultTubHue; }
			set
			{
				m_DefaultTubHue = value;
				base.Hue = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Dye
		{
			get { return m_Dye; }
			set { m_Dye = value; base.Hue = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasDye
		{
			get
			{
				return m_Dye > 0;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get
			{
				return HasDye ? m_Dye : DefaultTubHue;
			}
			set
			{
				if (HasDye) return;

				m_Dye = value;
				base.Hue = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Uses
		{
			get { return m_Uses; }
			set { m_Uses = value; InvalidateProperties(); }
		}

		protected abstract string PaintTargetMessage { get; }

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			list.Add(PaintTargetMessage);

			if (!HasDye)
			{
				list.Add("Apply dye directly");
				list.Add("(Can only be colored once)");
			}
			else
			{
				list.Add("Has been dyed");
			}

			list.Add(1060584, "{0}\t{1}", Uses.ToString(), "Uses");
		}

		public bool ApplyHue(Mobile from, int targetHue, int sound)
		{
			if (HasDye)
			{
				from.SendMessage("You cannot dye that.");
				return false;
			}

			Dye = targetHue; // Setting the base triggers necessary events

			InvalidateProperties();
			from.RevealingAction();
			from.PlaySound(sound);

			return true;
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			m_Uses = reader.ReadInt();
			m_Dye = reader.ReadInt();
			DefaultTubHue = reader.ReadInt();
		}

		public override void OnDoubleClick(Mobile from)
		{
			Target t;

			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
			}
			else
			{
				from.SendMessage("What do you want to paint?");
				t = new InternalTarget(this);
				from.Target = t;
			}
		}

		public void Reset(bool invalidate)
		{
			m_Dye = base.Hue = 0; // Setting the base triggers necessary events
			if (invalidate) InvalidateProperties();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
			writer.Write(m_Uses);
			writer.Write(value: m_Dye);
			writer.Write(DefaultTubHue);
		}

		protected abstract bool CanApplyPaint(Item item);

		private class InternalTarget : Target
		{
			private DyeTubTempBase m_Palette;

			public InternalTarget(DyeTubTempBase palette) : base(1, false, TargetFlags.None)
			{
				m_Palette = palette;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item)
				{
					Item iDye = targeted as Item;

					if (!iDye.IsChildOf(from.Backpack))
					{
						from.SendMessage("You can only paint things in your pack.");
					}
					else if ((iDye.Stackable == true) || (iDye.ItemID == 8702) || (iDye.ItemID == 4011))
					{
						from.SendMessage("You cannot paint that.");
					}
					else if (iDye.IsChildOf(from.Backpack) && m_Palette.CanApplyPaint(iDye) && !(targeted is MagicPigment))
					{
						iDye.Hue = m_Palette.Hue;
						from.RevealingAction();
						from.PlaySound(0x23F);

						if (--m_Palette.Uses < 1)
						{
							m_Palette.Delete();
						}
					}
					else
					{
						from.SendMessage("You cannot paint that with this.");
					}
				}
				else
				{
					from.SendMessage("You cannot paint that with this.");
				}
			}
		}
	}

	public class LeatherDyeTubTemp : DyeTubTempBase
	{
		public LeatherDyeTubTemp() : this(20)
		{
		}

		[Constructable]
		public LeatherDyeTubTemp(int uses) : base(uses)
		{
			Name = "Dye tub (Leather)";
			DefaultTubHue = RandomThings.GetRandomColor(CraftResourceType.Leather);
		}

		public LeatherDyeTubTemp(Serial serial) : base(serial)
		{
		}

		protected override string PaintTargetMessage
		{
			get
			{
				return "Usable on leather items";
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 0:
					break;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		protected override bool CanApplyPaint(Item item)
		{
			return CraftResources.GetType(item.Resource) == CraftResourceType.Leather;
		}
	}

	public class MetalDyeTubTemp : DyeTubTempBase
	{
		public MetalDyeTubTemp() : this(20)
		{
		}

		[Constructable]
		public MetalDyeTubTemp(int uses) : base(uses)
		{
			Name = "Dye tub (Metal)";
			DefaultTubHue = RandomThings.GetRandomColor(CraftResourceType.Metal);
		}

		public MetalDyeTubTemp(Serial serial) : base(serial)
		{
		}

		protected override string PaintTargetMessage
		{
			get
			{
				return "Usable on metal items";
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		protected override bool CanApplyPaint(Item item)
		{
			return CraftResources.GetType(item.Resource) == CraftResourceType.Metal || item is HorseArmor;
		}
	}

	public class WoodDyeTubTemp : DyeTubTempBase
	{
		public WoodDyeTubTemp() : this(20)
		{
		}

		[Constructable]
		public WoodDyeTubTemp(int uses) : base(uses)
		{
			Name = "Dye tub (Wooden)";
			DefaultTubHue = RandomThings.GetRandomColor(CraftResourceType.Wood);
		}

		public WoodDyeTubTemp(Serial serial) : base(serial)
		{
		}

		protected override string PaintTargetMessage
		{
			get
			{
				return "Usable on wooden items";
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 0:
					// if (!HasDye) Dye = RandomThings.GetRandomWoodColor();
					break;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		protected override bool CanApplyPaint(Item item)
		{
			return CraftResources.GetType(item.Resource) == CraftResourceType.Wood || FurnitureAttribute.Check(item);
		}
	}
}