using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
    [Furniture]
    [Flipable(0xE3D, 0xE3C)]
    public class DungeoneerCrate : Container
    {
        private static readonly InternalSellInfo SELL_INFO = new InternalSellInfo();

        private int m_CrateGold;
        private int m_PercentReduction;
        private Timer m_Timer;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsEnabled { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AllowGems { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AllowStackable { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PercentReduction
        {
            get { return m_PercentReduction; }
            set
            {
                m_PercentReduction = Math.Max(0, Math.Min(100, value));
                InvalidateProperties();
            }
        }

        [Constructable]
        public DungeoneerCrate() : base(0xE3D)
        {
            Name = "dungeoneer crate";
            Hue = 0x83F;
            Weight = 1.0;
        }

        public DungeoneerCrate(Serial serial) : base(serial)
        {
        }

        [CommandProperty(AccessLevel.Owner)]
        public int Crate_Gold { get { return m_CrateGold; } set { m_CrateGold = value; InvalidateProperties(); } }

        public override string DefaultDescription
        {
            get
            {
                return "Dungeoneer crates are something that homeowners can secure in their homes. When secured in your home, the Merchants Guild add you to their route for pick up. Merchants will leave gold in exchange for the goods. When you put something in the crate, you will be aware of the gold value of the item placed in it. There are plenty of craftsmen in town, so crafted equipment has no value to these merchants. Armor and weapons will have varying values depending on the resources used to create the item. Also durability and quality may increase the value. Other items like potions, scrolls, tools, furniture, or food can be sold for a price. <br><br>The crate will indicate how much gold it has available to transfer to yourself in the form of a bank check. Single click the crate and select the 'Transfer' option to withdraw all of the gold from the crate. Although there is a gold value indicated on the crate, the one withdrawing the amount may get more depending on whether they are in the Merchants Guild and/or they have a good Mercantile skill. These crates must be secured in a home to be of any use.";
            }
        }

        public override int DefaultMaxWeight { get { return 0; } } // A value of 0 signals unlimited weight
        public override bool DisplaysContent { get { return false; } }
        public override bool DisplayWeight { get { return false; } }
        public override bool IsDecoContainer { get { return false; } }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1049644, "Contains: " + m_CrateGold.ToString() + " Gold");
            list.Add(1070722, "For Sale: " + GetPendingTotal().ToString() + " Gold");
        }

        public void Empty()
        {
            if (!Movable && IsEnabled)
            {
                List<Item> items = Items;

                if (items.Count > 0)
                {
                    PublicOverheadMessage(MessageType.Regular, 0x3B2, true, "The items have been picked up");

                    for (int i = items.Count - 1; i >= 0; --i)
                    {
                        if (i >= items.Count)
                            continue;

                        m_CrateGold = m_CrateGold + GetItemValue(items[i], items[i].Amount, PercentReduction);
                        items[i].Delete();
                    }
                }
            }

            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            if (!Movable && BaseHouse.CheckAccessible(from, this) && m_CrateGold > 0) { list.Add(new CashOutEntry(from, this)); }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Movable)
            {
                from.SendMessage("This must be locked down in a house to use!");
            }
            else if (from.AccessLevel > AccessLevel.Player || from.InRange(GetWorldLocation(), 2))
            {
                Open(from);
            }
            else
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            }
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!CanAddItem(from, dropped))
                return false;

            if (!base.OnDragDrop(from, dropped))
                return false;

            OnAddItem(from, dropped);

            return true;
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (!CanAddItem(from, item))
                return false;

            if (!base.OnDragDropInto(from, item, p))
                return false;

            OnAddItem(from, item);

            return true;
        }

        public virtual void Open(Mobile from)
        {
            DisplayTo(from);
        }

        private static int GetItemValue(Item item, int amount, int reductionAmount)
        {
            if (item.Built)
                return 0;

            var price = SELL_INFO.GetSellPriceFor(item, 0) * amount;
            if (0 < reductionAmount)
                price = (int)(price - ((double)reductionAmount * price / 100));

            return price;
        }

        private bool CanAddItem(Mobile from, Item dropped)
        {
            if (!IsEnabled)
            {
                from.SendMessage("This crate is currently disabled.");
                return false;
            }

            if (Movable)
            {
                from.SendMessage("This must be locked down in a house to use!");
                return false;
            }

            if (dropped.Stackable)
            {
                if (dropped.Catalog == Catalogs.Gem)
                {
                    if (!AllowGems)
                    {
                        from.SendMessage("The merchants refuse gems.");
                        return false;
                    }
                }
                else if (!AllowStackable)
                {
                    from.SendMessage("The merchants refuse stackable items.");
                    return false;
                }
            }

            if (dropped.Built || dropped.BuiltBy != null)
            {
                from.SendMessage("The merchants already know plenty of craftsmen. Try something else.");
                return false;
            }

            return true;
        }

        private int GetPendingTotal()
        {
            int gold = 0;

            List<Item> items = Items;

            if (items.Count > 0)
            {
                for (int i = items.Count - 1; i >= 0; --i)
                {
                    if (i >= items.Count)
                        continue;

                    gold = gold + GetItemValue(items[i], items[i].Amount, PercentReduction);
                }
            }

            return gold;
        }

        private void OnAddItem(Mobile from, Item item)
        {
            if (!IsEnabled) return;

            from.SendMessage("The items will be picked up in a couple days");
            PublicOverheadMessage(MessageType.Regular, 0x3B2, true, "Worth " + GetItemValue(item, item.Amount, PercentReduction).ToString() + " gold");

            if (m_Timer != null)
                m_Timer.Stop();
            else
                m_Timer = new EmptyTimer(this);

            m_Timer.Start();
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_CrateGold = reader.ReadInt();
            IsEnabled = reader.ReadBool();
            PercentReduction = reader.ReadInt();
            AllowStackable = reader.ReadBool();
            AllowGems = reader.ReadBool();

            if (IsEnabled)
            {
                QuickTimer thisTimer = new QuickTimer(this);
                thisTimer.Start();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version

            writer.Write(m_CrateGold);
            writer.Write(IsEnabled);
            writer.Write(PercentReduction);
            writer.Write(AllowStackable);
            writer.Write(AllowGems);
        }

        public class CashOutEntry : ContextMenuEntry
        {
            private readonly DungeoneerCrate m_Crate;
            private readonly Mobile m_Mobile;

            public CashOutEntry(Mobile from, DungeoneerCrate crate) : base(6113, 3)
            {
                m_Mobile = from;
                m_Crate = crate;
            }

            public override void OnClick()
            {
                if (!(m_Mobile is PlayerMobile))
                    return;

                PlayerMobile mobile = (PlayerMobile)m_Mobile;
                {
                    if (m_Crate.m_CrateGold > 0)
                    {
                        double barter = (int)(m_Mobile.Skills[SkillName.Mercantile].Value / 2);

                        if (mobile.NpcGuild == NpcGuild.MerchantsGuild)
                            barter = barter + 25.0; // FOR GUILD MEMBERS

                        barter = barter / 100;

                        int bonus = (int)(m_Crate.m_CrateGold * barter);

                        int cash = m_Crate.m_CrateGold + bonus;

                        m_Mobile.AddToBackpack(new BankCheck(cash));
                        m_Mobile.SendMessage("You now have a check for " + cash.ToString() + " gold.");
                        m_Crate.m_CrateGold = 0;
                        m_Crate.InvalidateProperties();
                    }
                    else
                    {
                        m_Mobile.SendMessage("There is no gold in this crate!");
                    }
                }
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                ItemInformation.GetBuysList(null, this, ItemSalesInfo.Category.All, ItemSalesInfo.Material.All, ItemSalesInfo.Market.All, ItemSalesInfo.World.None, null, true);
            }
        }

        private class EmptyTimer : Timer
        {
            private DungeoneerCrate m_Crate;

            public EmptyTimer(DungeoneerCrate crate) : base(TimeSpan.FromHours(4))
            {
                m_Crate = crate;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                m_Crate.Empty();
            }
        }

        private class QuickTimer : Timer
        {
            private DungeoneerCrate m_Crate;

            public QuickTimer(DungeoneerCrate crate) : base(TimeSpan.FromSeconds(60.0))
            {
                m_Crate = crate;
                Priority = TimerPriority.FiveSeconds;
            }

            protected override void OnTick()
            {
                m_Crate.Empty();
            }
        }
    }
}