using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.GlobalShoppe
{
    [Flipable(0x3CF9, 0x3CFA)]
    public class TailorShoppe : ShoppeBase
    {
        [Constructable]
        public TailorShoppe() : base(0x3CF9)
        {
            Name = "Tailor Work Shoppe";
        }

        public TailorShoppe(Serial serial) : base(serial)
        {
        }

        public override NpcGuild Guild { get { return NpcGuild.TailorsGuild; } }

        protected override SkillName PrimarySkill { get { return SkillName.Tailoring; } }
        protected override ShoppeType ShoppeType { get { return ShoppeType.Tailor; } }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is GodSewing || dropped is SewingKit)
                return AddTools(from, dropped);
            if (dropped is BaseFabric)
                return AddResource(from, dropped);

            return base.OnDragDrop(from, dropped);
        }

        protected override string CreateTask(TradeSkillContext context)
        {
            string task = null;

            switch (Utility.RandomMinMax(1, 9))
            {
                case 1: task = "Repair"; break;
                case 2: task = "Fix"; break;
                case 3: task = "Enhance"; break;
                case 4: task = "Modify"; break;
                case 5: task = "Resize"; break;
                case 6: task = "Embroider"; break;
                case 7: task = "Stitch"; break;
                case 8: task = "Patch"; break;
                case 9: task = "Alter"; break;
            }

            if (Utility.RandomMinMax(1, 4) == 1)
            {
                Item item = null;

                switch (Utility.RandomMinMax(1, 6))
                {
                    case 1: item = new Robe(); break;
                    case 2: item = new Cloak(); break;
                    case 3: item = new Belt(); break;
                    case 4: item = new Boots(); break;
                    case 5: item = new FloppyHat(); break;
                    case 6: item = new BodySash(); break;
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
                string[] sCloths = new string[] { " brocade ", " cotton ", " ermine ", " silk ", " wool ", " fur ", " spider silk ", " cloth ", " lace ", " leather ", " felt ", " hemp ", " linen ", " quilted " };
                string sCloth = sCloths[Utility.RandomMinMax(0, (sCloths.Length - 1))];

                task = task + " their" + sCloth;

                switch (Utility.RandomMinMax(1, 17))
                {
                    case 1: task += "shirt"; break;
                    case 2: task += "short pants"; break;
                    case 3: task += "long pants"; break;
                    case 4: task += "fancy dress"; break;
                    case 5: task += "plain dress"; break;
                    case 6: task += "kilt"; break;
                    case 7: task += "half apron"; break;
                    case 8: task += "loin cloth"; break;
                    case 9: task += "cloak"; break;
                    case 10: task += "doublet"; break;
                    case 11: task += "tunic"; break;
                    case 12: task += "floppy hat"; break;
                    case 13: task += "wizard hat"; break;
                    case 14: task += "witch hat"; break;
                    case 15: task += "robe"; break;
                    case 16: task += "breeches"; break;
                    case 17: task += "stockings"; break;
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
                "TAILOR WORK SHOPPE",
                "Sewing Kits",
                "Cloth"
            );
        }

        protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobFailed(from, context, customer);

            from.SendSound(0x248); // Scissors
        }

        protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobSuccess(from, context, customer);

            from.SendSound(0x248); // Scissors
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