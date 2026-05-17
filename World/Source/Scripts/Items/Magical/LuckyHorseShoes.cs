using System;
using Server.Targeting;
using Server.Localization;

namespace Server.Items
{
	public class LuckyHorseShoes : Item
	{
		public override bool IsContentLocalized => true;

		[Constructable]
		public LuckyHorseShoes() : base(0xFB6)
		{
			Weight = 1.0;
			Name = "lucky horse shoes";
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.magical.lucky.horseshoes" );
			else
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
				from.SendMessage(StringCatalog.ResolveByKey(from.Account, "prop.magical.luck.msg.what.item"));
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
						from.SendMessage(StringCatalog.ResolveByKey(from.Account, "prop.magical.luck.msg.pack"));
						return;
					}

					if (target is BaseWeapon) Apply(from, ((BaseWeapon)target).Attributes);
					else if (target is BaseClothing) Apply(from, ((BaseClothing)target).Attributes);
					else if (target is BaseTrinket) Apply(from, ((BaseTrinket)target).Attributes);
					else if (target is BaseArmor) Apply(from, ((BaseArmor)target).Attributes);
					else if (target is Spellbook) Apply(from, ((Spellbook)target).Attributes);
					else if (target is BaseQuiver) Apply(from, ((BaseQuiver)target).Attributes);
					else if (target is BaseInstrument) Apply(from, ((BaseInstrument)target).Attributes);
					else from.SendMessage(StringCatalog.ResolveByKey(from.Account, "prop.magical.luck.msg.cannot"));
				}
				else
					from.SendMessage(StringCatalog.ResolveByKey(from.Account, "prop.magical.luck.msg.cannot"));

			}

			private void Apply(Mobile from, AosAttributes attributes)
			{
				const int MAX_LUCK = 100;
				int luck = attributes.Luck;
				if (luck >= MAX_LUCK)
				{
					from.SendMessage(StringCatalog.ResolveByKey(from.Account, "prop.magical.luck.msg.full"));
				}
				else
				{
					attributes.Luck = Math.Min(MAX_LUCK, luck + 100); // In case an item has negative luck
					from.SendMessage(StringCatalog.ResolveByKey(from.Account, "prop.magical.luck.msg.ok"));
					m_Deed.Delete();
				}
			}
		}
	}
}