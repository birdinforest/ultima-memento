using System;
using Server.Targeting;

namespace Server.Items
{
	public class LuckyHorseShoes : Item
	{
		[Constructable]
		public LuckyHorseShoes() : base(0xFB6)
		{
			Weight = 1.0;
			Name = "lucky horse shoes";
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			list.Add(1070722, "Adds up to 100 Luck To An Item");
		}

		public LuckyHorseShoes(Serial serial) : base(serial)
		{
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

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001);
			}
			else
			{
				from.SendMessage("What item would you like to add luck to?");
				from.Target = new InternalTarget(this);
			}
		}

		private class InternalTarget : Target
		{
			private LuckyHorseShoes m_Deed;

			public InternalTarget(LuckyHorseShoes deed) : base(1, false, TargetFlags.None)
			{
				m_Deed = deed;
			}

			protected override void OnTarget(Mobile from, object target)
			{
				Item item = target as Item;
				if (item != null)
				{
					if (item.RootParent != from)
					{
						from.SendMessage("The item must be in your pack.");
						return;
					}

					if (target is BaseWeapon) Apply(from, ((BaseWeapon)target).Attributes);
					else if (target is BaseClothing) Apply(from, ((BaseClothing)target).Attributes);
					else if (target is BaseTrinket) Apply(from, ((BaseTrinket)target).Attributes);
					else if (target is BaseArmor) Apply(from, ((BaseArmor)target).Attributes);
					else if (target is Spellbook) Apply(from, ((Spellbook)target).Attributes);
					else if (target is BaseQuiver) Apply(from, ((BaseQuiver)target).Attributes);
					else if (target is BaseInstrument) Apply(from, ((BaseInstrument)target).Attributes);
					else from.SendMessage("You cannot enhance that item with luck.");
				}
				else
					from.SendMessage("You cannot enhance that item with luck.");

			}

			private void Apply(Mobile from, AosAttributes attributes)
			{
				const int MAX_LUCK = 100;
				int luck = attributes.Luck;
				if (luck >= MAX_LUCK)
				{
					from.SendMessage("There is already enough luck on this item.");
				}
				else
				{
					attributes.Luck = Math.Min(MAX_LUCK, luck + 100); // In case an item has negative luck
					from.SendMessage("You add some extra luck to the item.");
					m_Deed.Delete();
				}
			}
		}
	}
}