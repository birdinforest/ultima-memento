using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.GlobalShoppe
{
    [Flipable(0x3CE9, 0x3CEA)]
    public class BowyerShoppe : ShoppeBase
    {
        [Constructable]
        public BowyerShoppe() : base(0x3CE9)
        {
            Name = "Bowyer Work Shoppe";
        }

        public BowyerShoppe(Serial serial) : base(serial)
        {
        }

        public override NpcGuild Guild { get { return NpcGuild.ArchersGuild; } }

        protected override SkillName PrimarySkill { get { return SkillName.Bowcraft; } }
        protected override ShoppeType ShoppeType { get { return ShoppeType.Bowyer; } }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is FletcherTools)
                return AddTools(from, dropped);
            if (dropped is BaseWoodBoard)
                return AddResource(from, dropped);

            return base.OnDragDrop(from, dropped);
        }

        protected override string CreateTask(TradeSkillContext context)
        {
            string task = null;

            switch (Utility.RandomMinMax(1, 10))
            {
                case 1: task = "Repair"; break;
                case 2: task = "Fix"; break;
                case 3: task = "Enhance"; break;
                case 4: task = "Modify"; break;
                case 5: task = "Restring"; break;
                case 6: task = "Engrave"; break;
                case 7: task = "Adjust"; break;
                case 8: task = "Improve"; break;
                case 9: task = "Align"; break;
                case 10: task = "Balance"; break;
            }

            if (Utility.RandomMinMax(1, 5) == 1)
            {
                Item item = null;

                switch (Utility.RandomMinMax(1, 7))
                {
                    case 1: item = new Bow(); break;
                    case 2: item = new Crossbow(); break;
                    case 3: item = new HeavyCrossbow(); break;
                    case 4: item = new RepeatingCrossbow(); break;
                    case 5: item = new CompositeBow(); break;
                    case 6: item = new MagicalShortbow(); break;
                    case 7: item = new ElvenCompositeLongbow(); break;
                }

                bool evil = false;
                bool orient = false;

                switch (Utility.RandomMinMax(1, 8))
                {
                    case 1: evil = true; break;
                    case 2: orient = true; break;
                }

                string sAdjective = "unusual";
                string eAdjective = "might";

                sAdjective = RandomThings.MagicItemAdj("start", orient, evil, item.ItemID);
                eAdjective = RandomThings.MagicItemAdj("end", orient, evil, item.ItemID);

                string name = "item";
                string xName = ContainerFunctions.GetOwner("property");

                if (item.Name != null && item.Name != "") { name = item.Name.ToLower(); }
                if (name == "item") { item.SyncName(); name = (item.Name).ToLower(); }

                switch (Utility.RandomMinMax(0, 5))
                {
                    case 0: name = sAdjective + " " + name + " of " + xName; break;
                    case 1: name = name + " of " + xName; break;
                    case 2: name = sAdjective + " " + name; break;
                    case 3: name = sAdjective + " " + name + " of " + xName; break;
                    case 4: name = name + " of " + xName; break;
                    case 5: name = sAdjective + " " + name; break;
                }

                item.Delete();

                task = task + " their " + name;
            }
            else
            {
                string[] sWoods = new string[] { " ", " ash wood ", " cherry wood ", " ebony wood ", " golden oak ", " hickory ", " mahogany ", " oak ", " pine ", " ghost wood ", " rosewood ", " walnut wood ", " petrified wood ", " diftwood ", " elven wood " };
                string sWood = sWoods[Utility.RandomMinMax(0, (sWoods.Length - 1))];

                task = task + " their" + sWood;

                switch (Utility.RandomMinMax(1, 8))
                {
                    case 1: task += "bow"; break;
                    case 2: task += "crossbow"; break;
                    case 3: task += "longbow"; break;
                    case 4: task += "shortbow"; break;
                    case 5: task += "repeating crossbow"; break;
                    case 6: task += "heavy crossbow"; break;
                    case 7: task += "composite longbow"; break;
                    case 8: task += "composite shortbow"; break;
                }
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
                "BOWYER WORK SHOPPE",
                "Bowcrafting Tools",
                "Boards"
            );
        }

        protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobFailed(from, context, customer);

            from.SendSound(0x55); // Turnpage
        }

        protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobSuccess(from, context, customer);

            from.SendSound(0x55); // Turnpage
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