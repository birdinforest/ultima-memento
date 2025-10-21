using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.GlobalShoppe
{
    [Flipable(0x3CFD, 0x3CFE)]
    public class CartographyShoppe : CustomerShoppe
    {
        [Constructable]
        public CartographyShoppe() : base(0x3CFD)
        {
            Name = "Cartography Work Shoppe";
        }

        public CartographyShoppe(Serial serial) : base(serial)
        {
        }

        public override NpcGuild Guild { get { return NpcGuild.CartographersGuild; } }
        public override ShoppeType ShoppeType { get { return ShoppeType.Cartography; } }

        protected override SkillName PrimarySkill { get { return SkillName.Cartography; } }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is MapmakersPen)
                return AddTools(from, dropped);
            if (dropped is BlankScroll)
                return AddResource(from, dropped);

            return base.OnDragDrop(from, dropped);
        }

        protected override string CreateTask(TradeSkillContext context)
        {
            string task = null;

            switch (Utility.RandomMinMax(0, 2))
            {
                case 0: task = Server.Misc.RandomThings.MadeUpCity(); break;
                case 1: task = Server.Misc.RandomThings.MadeUpDungeon(); break;
                case 2: task = "the " + RandomThings.GetRandomKingdomName() + " " + RandomThings.GetRandomKingdom(); break;
            }

            switch (Utility.RandomMinMax(0, 10))
            {
                case 0: task = "Redraw this map of " + task; break;
                case 1: task = "Decipher this map of " + task; break;
                case 2: task = "Clean up this map of " + task; break;
                case 3: task = "Verify this map of " + task; break;
                case 4: task = "Copy this map of " + task; break;
                case 5: task = "Make an atlas for these maps of " + task; break;
                case 6: task = "Draw a trail map to " + task; break;
                case 7: task = "Decode this treasure map for " + task; break;
                case 8: task = "Take this map of " + task + " and make it a larger scale"; break;
                case 9: task = "Take this map of " + task + " and make it a smaller scale"; break;
                case 10: task = "Encode this treasure map for " + task; break;
            }

            return task;
        }

        protected override ShoppeGump GetGump(PlayerMobile from)
        {
            var context = GetOrCreateContext(from);

            return new ShoppeGump(
                from,
                this,
                context,
                "CARTOGRAPHY WORK SHOPPE",
                "Cartography Quills",
                "Blank Scrolls"
            );
        }

        protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobFailed(from, context, customer);

            from.SendSound(0x249); // Scribe
        }

        protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobSuccess(from, context, customer);

            from.SendSound(0x249); // Scribe
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
    }
}