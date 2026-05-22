using Server.Localization;

namespace Server.Items
{
    public class ElementalSharpeningStone : SplitElementalItemBase
    {
        [Constructable]
        public ElementalSharpeningStone() : base(0x1F14)
        {
            Name = StringCatalog.Resolve(null, "Elemental Sharpening Stone");
        }

        public ElementalSharpeningStone(Serial serial) : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (BuildingPropertyListLocale != null)
                AddLocalizedProperty(list, "prop.sharpening.restrict.bladed");
            else
                list.Add(1049644, "[Only usable on bladed weapons]");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Skills[SkillName.Blacksmith].Value < 100.0 || from.Skills[SkillName.Magery].Value < 100.0)
            {
                from.SendMessage(32, StringCatalog.Resolve(from.Account, "You need at least 100 Blacksmithing and 100 Magery to use this"));
                return;
            }

            base.OnDoubleClick(from);
        }

        protected override bool Validate(Mobile from, BaseWeapon weapon)
        {
            if (!base.Validate(from, weapon)) return false;

            if (false == (weapon is BaseSword || weapon is BaseKnife || weapon is BaseAxe || weapon is BaseSpear))
            {
                from.SendMessage(32, StringCatalog.Resolve(from.Account, "You may only use this on bladed weapons"));
                return false;
            }

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            if (IsLegacyItem) return;

            int version = reader.ReadInt();
        }
    }
}