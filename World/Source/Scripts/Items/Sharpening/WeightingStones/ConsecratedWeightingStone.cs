using Server.Localization;

namespace Server.Items
{
    public class ConsecratedWeightingStone : ConsecrateItemBase
    {
        public override string DisplayNameLocalizationKey => "item.sharpening.weightingstone.consecrated";

        public override string DefaultDescription { get { return StringCatalog.Resolve(null, "This blessed stone consecrates a blunt weapon (bashing weapon, staff, or pugilist glove) for 4 hours. While consecrated, each hit converts 100% of damage to the defender's weakest resistance type. Requires 100 Blacksmithing and 80 Knightship."); } }
        public override string InfoDataLocalizationKey { get { return "prop.consecrate.inspect.weighting.body"; } }

        public ConsecratedWeightingStone(Serial serial) : base(serial)
        {
        }

        [Constructable]
        public ConsecratedWeightingStone() : this(5)
        {
        }

        [Constructable]
        public ConsecratedWeightingStone(int uses) : base(uses, 0x1F14)
        {
            Name = StringCatalog.Resolve(null, "Consecrated Weighting Stone");
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (BuildingPropertyListLocale != null)
                AddLocalizedProperty(list, "prop.sharpening.restrict.blunt");
            else
                list.Add(1049644, "[Only usable on blunt weapons]");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Skills[SkillName.Blacksmith].Value < 100.0 || from.Skills[SkillName.Knightship].Value < 80.0)
            {
                from.SendMessage(32, StringCatalog.Resolve(from.Account, "You need at least 100 Blacksmithing and 80 Knightship to use this"));
                return;
            }

            base.OnDoubleClick(from);
        }

        protected override bool Validate(Mobile from, BaseWeapon weapon)
        {
            if (!base.Validate(from, weapon)) return false;

            if (false == (weapon is BaseBashing || weapon is BaseStaff || weapon is IPugilistGlove))
            {
                from.SendMessage(32, StringCatalog.Resolve(from.Account, "You may only use this on blunt weapons"));
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