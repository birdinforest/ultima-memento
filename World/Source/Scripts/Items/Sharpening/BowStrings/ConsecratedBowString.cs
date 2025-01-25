namespace Server.Items
{
    public class ConsecratedBowString : ConsecrateItemBase
    {
        public ConsecratedBowString(Serial serial) : base(serial)
        {
        }

        [Constructable]
        public ConsecratedBowString() : this(5)
        {
        }

        [Constructable]
        public ConsecratedBowString(int uses) : base(uses, 0x543A)
        {
            Name = "Consecrated Bow String";
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1049644, "[Only usable on ranged weapons]");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.Skills[SkillName.Bowcraft].Value < 100.0 || from.Skills[SkillName.Knightship].Value < 80.0)
            {
                from.SendMessage(32, "You need at least 100 Bowcraft and 80 Knightship to use this");
                return;
            }

            base.OnDoubleClick(from);
        }

        protected override bool Validate(Mobile from, BaseWeapon weapon)
        {
            if (!base.Validate(from, weapon)) return false;

            if (false == weapon is BaseRanged)
            {
                from.SendMessage(32, "You may only use this on ranged weapons");
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