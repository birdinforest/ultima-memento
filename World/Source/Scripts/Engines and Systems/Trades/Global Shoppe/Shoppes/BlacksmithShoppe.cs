using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.GlobalShoppe
{
    [Flipable(0x3CF7, 0x3CF8)]
    public class BlacksmithShoppe : ShoppeBase
    {
        [Constructable]
        public BlacksmithShoppe() : base(0x3CF7)
        {
            Name = "Blacksmith Work Shoppe";
        }

        public BlacksmithShoppe(Serial serial) : base(serial)
        {
        }

        public override NpcGuild Guild { get { return NpcGuild.BlacksmithsGuild; } }

        protected override SkillName PrimarySkill { get { return SkillName.Blacksmith; } }
        protected override ShoppeType ShoppeType { get { return ShoppeType.Blacksmith; } }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is GodSmithing || dropped is SmithHammer || dropped is ScalingTools)
                return AddTools(from, dropped);
            if (dropped.ItemID >= 0x1BE3 && dropped.ItemID <= 0x1BFA)
                return AddResource(from, dropped);

            return base.OnDragDrop(from, dropped);
        }

        protected override string CreateTask(TradeSkillContext context)
        {
            string task;
            switch (Utility.RandomMinMax(1, 10))
            {
                default:
                case 1: task = "Repair"; break;
                case 2: task = "Fix"; break;
                case 3: task = "Buff"; break;
                case 4: task = "Modify"; break;
                case 5: task = "Polish"; break;
                case 6: task = "Engrave"; break;
                case 7: task = "Adjust"; break;
                case 8: task = "Improve"; break;
                case 9: task = "Smooth the dents from"; break;
                case 10: task = "Remove the dents from"; break;
            }

            Item item;
            switch (Utility.RandomMinMax(1, 85))
            {
                default:
                case 1: item = new AssassinSpike(); break;
                case 2: item = new Axe(); break;
                case 3: item = new Bardiche(); break;
                case 4: item = new Bascinet(); break;
                case 5: item = new BattleAxe(); break;
                case 6: item = new BoneHarvester(); break;
                case 7: item = new Broadsword(); break;
                case 8: item = new BronzeShield(); break;
                case 9: item = new Buckler(); break;
                case 10: item = new ButcherKnife(); break;
                case 11: item = new ChainChest(); break;
                case 12: item = new ChainCoif(); break;
                case 13: item = new ChainLegs(); break;
                case 14: item = new ChampionShield(); break;
                case 15: item = new Cleaver(); break;
                case 16: item = new CloseHelm(); break;
                case 17: item = new CloseHelm(); break;
                case 18: item = new CrescentBlade(); break;
                case 19: item = new CrestedShield(); break;
                case 20: item = new Cutlass(); break;
                case 21: item = new Dagger(); break;
                case 22: item = new DarkShield(); break;
                case 23: item = new DiamondMace(); break;
                case 24: item = new DoubleAxe(); break;
                case 25: item = new DoubleBladedStaff(); break;
                case 26: item = new DreadHelm(); break;
                case 27: item = new ElvenMachete(); break;
                case 28: item = new ElvenShield(); break;
                case 29: item = new ElvenSpellblade(); break;
                case 30: item = new ExecutionersAxe(); break;
                case 31: item = new FemalePlateChest(); break;
                case 32: item = new GuardsmanShield(); break;
                case 33: item = new Halberd(); break;
                case 34: item = new HammerPick(); break;
                case 35: item = new HeaterShield(); break;
                case 36: item = new Helmet(); break;
                case 37: item = new Helmet(); break;
                case 38: item = new JeweledShield(); break;
                case 39: item = new Katana(); break;
                case 40: item = new Kryss(); break;
                case 41: item = new Lance(); break;
                case 42: item = new LargeBattleAxe(); break;
                case 43: item = new Leafblade(); break;
                case 44: item = new Longsword(); break;
                case 45: item = new Mace(); break;
                case 46: item = new Maul(); break;
                case 47: item = new MetalKiteShield(); break;
                case 48: item = new MetalShield(); break;
                case 49: item = new NorseHelm(); break;
                case 50: item = new NorseHelm(); break;
                case 51: item = new OrnateAxe(); break;
                case 52: item = new Pickaxe(); break;
                case 53: item = new Pike(); break;
                case 54: item = new Pitchfork(); break;
                case 55: item = new PlateArms(); break;
                case 56: item = new PlateChest(); break;
                case 57: item = new PlateGloves(); break;
                case 58: item = new PlateGorget(); break;
                case 59: item = new PlateHelm(); break;
                case 60: item = new PlateHelm(); break;
                case 61: item = new PlateLegs(); break;
                case 62: item = new RadiantScimitar(); break;
                case 63: item = new RingmailArms(); break;
                case 64: item = new RingmailChest(); break;
                case 65: item = new RingmailGloves(); break;
                case 66: item = new RingmailLegs(); break;
                case 67: item = new RuneBlade(); break;
                case 68: item = new Scimitar(); break;
                case 69: item = new Scythe(); break;
                case 70: item = new ShortSpear(); break;
                case 71: item = new Spear(); break;
                case 72: item = new ThinLongsword(); break;
                case 73: item = new TwoHandedAxe(); break;
                case 74: item = new VikingSword(); break;
                case 75: item = new WarAxe(); break;
                case 76: item = new WarCleaver(); break;
                case 77: item = new WarHammer(); break;
                case 78: item = new WarMace(); break;
                case 79: item = new Claymore(); break;
                case 80: item = new SpikedClub(); break;
                case 81: item = new LargeKnife(); break;
                case 82: item = new Hammers(); break;
                case 83: item = new ShortSword(); break;
                case 84: item = new SunShield(); break;
                case 85: item = new VirtueShield(); break;
            }

            if (string.IsNullOrWhiteSpace(item.Name)) { item.SyncName(); }
            var name = item.Name.ToLower();

            if (Utility.RandomMinMax(1, 5) == 1)
            {
                bool evil = false;
                bool orient = false;
                switch (Utility.RandomMinMax(1, 8))
                {
                    case 1: evil = true; break;
                    case 2: orient = true; break;
                }

                var sAdjective = RandomThings.MagicItemAdj("start", orient, evil, item.ItemID);
                string xName = ContainerFunctions.GetOwner("property");
                switch (Utility.RandomMinMax(0, 5))
                {
                    case 0: name = sAdjective + " " + name + " of " + xName; break;
                    case 1: name = name + " of " + xName; break;
                    case 2: name = sAdjective + " " + name; break;
                    case 3: name = sAdjective + " " + name + " of " + xName; break;
                    case 4: name = name + " of " + xName; break;
                    case 5: name = sAdjective + " " + name; break;
                }

                task += " their " + name;
            }
            else
            {
                string[] sMetals = new string[] { "iron ", "dull copper ", "shadow iron ", "copper ", "bronze ", "gold ", "agapite ", "verite ", "valorite ", "nepturite ", "obsidian ", "steel ", "brass ", "mithril ", "xormite ", "dwarven " };
                string sMetal = sMetals[Utility.RandomMinMax(0, sMetals.Length - 1)];

                task += " their " + sMetal + name;
            }

            item.Delete();

            return task;
        }

        protected override ShoppeGump GetGump(PlayerMobile from)
        {
            var context = GetOrCreateContext(from);

            return new ShoppeGump(
                from,
                this,
                context,
                "BLACKSMITH WORK SHOPPE",
                "Smith Hammer",
                "Ingots"
            );
        }

        protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobFailed(from, context, customer);

            from.SendSound(0x541); // Blacksmith
        }

        protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobSuccess(from, context, customer);

            from.SendSound(0x541); // Blacksmith
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