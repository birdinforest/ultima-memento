using Server.Localization;

namespace Server.Items
{
    public abstract class DamageIncreaseWeightingStoneBase : AddDamageItemBase
    {
        public DamageIncreaseWeightingStoneBase(Serial serial) : base(serial)
        {
        }

        protected DamageIncreaseWeightingStoneBase(int uses) : base(uses,0x1F14)
        {
        }

        protected override void AfterItemDeleted(Mobile from)
        {
            from.SendMessage(32, StringCatalog.Resolve(from.Account, "You use the last of the weighting stone"));
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (BuildingPropertyListLocale != null)
                AddLocalizedProperty(list, "prop.sharpening.damage.increase");
            else
                list.Add("Adds damage increase");

            if (BuildingPropertyListLocale != null)
                AddLocalizedProperty(list, "prop.sharpening.restrict.blunt");
            else
                list.Add(1049644, "[Only usable on blunt weapons]");
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