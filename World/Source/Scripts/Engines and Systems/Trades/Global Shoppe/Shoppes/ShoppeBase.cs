using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.GlobalShoppe
{
    public abstract class ShoppeBase : Item
    {
        public ShoppeBase(Serial serial) : base(serial)
        {
        }

        protected ShoppeBase(int itemId) : base(itemId)
        {
        }

        public override bool Decays { get { return false; } }

        public abstract NpcGuild Guild { get; }

        protected virtual bool CanCreateOrders { get { return false; } }
        protected abstract SkillName PrimarySkill { get; }
        protected abstract ShoppeType ShoppeType { get; }

        public void AcceptCustomer(int index, Mobile from, TradeSkillContext context)
        {
            if (context.Customers.Count <= index) return;

            var customer = context.Customers[index];
            if (!CanAcceptCustomer(context, customer)) return;

            // Reduce things
            context.Tools -= customer.ToolCost;
            context.Resources -= customer.ResourceCost;
            context.Customers.Remove(customer);

            var successChance = GetSuccessChance(from, customer.Difficulty);
            if (successChance < Utility.RandomMinMax(1, 100))
                OnJobFailed(from, context, customer);
            else
                OnJobSuccess(from, context, customer);
        }

        public void AddOrderItem(int index, Mobile from, TradeSkillContext context)
        {
            if (context.Orders.Count <= index) return;

            var order = context.Orders[index];
            if (order.IsComplete) return;

            from.CloseGump(typeof(OrderGump));
            from.SendGump(new OrderGump(from, order));
        }

        public bool CanAcceptCustomer(TradeSkillContext context, CustomerContext customer)
        {
            return HasEnoughTools(context, customer) && HasEnoughResources(context, customer) && HasGoldCapacity(context, customer);
        }

        public void CompleteOrder(int index, Mobile from, TradeSkillContext context)
        {
            if (context.Orders.Count <= index) return;

            var order = context.Orders[index];
            if (!order.IsComplete) return;

            context.Gold += order.GoldReward;
            context.Points += order.PointReward;
            context.Reputation = Math.Min(ShoppeConstants.MAX_REPUTATION, context.Reputation + order.ReputationReward);
            context.Orders.Remove(order);
            
            from.PlaySound( 0x32 ); // Dropgem1
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            var context = GetOrCreateContext(from);
            if (0 < context.Gold)
            {
                list.Add(new CashOutEntry((PlayerMobile)from, context));
            }
        }

        public int GetReputationBonus(PlayerMobile from)
        {
            if (from.NpcGuild != Guild) return 0;

            var playerSkillBonus = 5 * GetSkillValue(from);

            return 500 + playerSkillBonus;
        }

        public string GetRequirementsMessage(Mobile from, TradeSkillContext context)
        {
            var skill = from.Skills[PrimarySkill];
            if (ShoppeConstants.MIN_SKILL <= skill.Value)
            {
                if (context.FeePaid) return null;

                return string.Format("You must pay {0} gold coins to open this shoppe.", ShoppeConstants.SHOPPE_FEE);
            }

            return string.Format("You must have at least {0} skill in {1} to use this shoppe.", ShoppeConstants.MIN_SKILL, skill.Name);
        }

        public int GetSuccessChance(Mobile from, int difficulty)
        {
            var value = GetSkillValue(from);

            return GetSuccessChance(value, difficulty);
        }

        public bool HasEnoughResources(TradeSkillContext context, CustomerContext customer)
        {
            return customer.ResourceCost <= context.Resources;
        }

        public bool HasEnoughTools(TradeSkillContext context, CustomerContext customer)
        {
            return customer.ToolCost <= context.Tools;
        }

        public bool HasGoldCapacity(TradeSkillContext context, CustomerContext customer)
        {
            return context.Gold + customer.GoldReward <= ShoppeConstants.MAX_GOLD;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!ShoppeEngine.Instance.IsEnabled)
            {
                from.LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, false, "Shoppe system is currently disabled.");
                return;
            }

            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            var context = GetOrCreateContext(from);
            if (context.FeePaid && context.CanRefreshCustomers)
            {
                while (context.Customers.Count < ShoppeConstants.MAX_CUSTOMERS)
                {
                    context.Customers.Add(CreateCustomer(context));
                }

                context.CanRefreshCustomers = false;
                context.NextCustomerRefresh = DateTime.UtcNow.Add(ShoppeConstants.CUSTOMER_REFRESH_DELAY);
            }

            if (context.FeePaid && CanCreateOrders && context.CanRefreshOrders)
            {
                var count = ShoppeConstants.MAX_ORDERS - context.Orders.Count;
                foreach (var order in CreateOrders(from, context, count))
                {
                    context.Orders.Add(order);
                }

                context.CanRefreshOrders = false;
                context.NextOrderRefresh = DateTime.UtcNow.Add(ShoppeConstants.ORDER_REFRESH_DELAY);
            }

            from.CloseGump(typeof(ShoppeGump));
            from.SendGump(GetGump((PlayerMobile)from));
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is Gold)
            {
                var context = GetOrCreateContext(from);
                if (!context.FeePaid && ShoppeConstants.SHOPPE_FEE <= dropped.Amount)
                {
                    context.FeePaid = true;
                    dropped.Amount -= ShoppeConstants.SHOPPE_FEE;
                    if (dropped.Amount < 1)
                        dropped.Delete();

                    OnDoubleClick(from);

                    return dropped.Deleted;
                }
            }

            return base.OnDragDrop(from, dropped);
        }

        public override bool OnDragLift(Mobile from)
        {
            // Entity is Visible so Ctrl+Shift can show it
            // Disallow moving it
            return false;
        }

        public void RejectCustomer(int index, TradeSkillContext context)
        {
            if (context.Customers.Count <= index) return;

            var customer = context.Customers[index];
            context.Reputation = Math.Max(0, context.Reputation - customer.ReputationReward);

            context.Customers.Remove(customer);
        }

        public void RejectOrder(int index, TradeSkillContext context)
        {
            if (context.Orders.Count <= index) return;

            var order = context.Orders[index];
            context.Reputation = Math.Max(0, context.Reputation - order.ReputationReward);

            context.Orders.Remove(order);
        }

        protected bool AddResource(Mobile from, Item item, int valuePerItem = 1)
        {
            var context = GetOrCreateContext(from);
            if (ShoppeConstants.MAX_RESOURCES <= context.Resources)
            {
                from.SendMessage("Your shoppe is already full of resources.");
                return false;
            }

            var amountMissing = (int)(valuePerItem * Math.Ceiling(((float)ShoppeConstants.MAX_RESOURCES - context.Resources) / valuePerItem));
            var amount = Math.Min(amountMissing, item.Amount * valuePerItem) / valuePerItem;
            item.Amount -= amount;
            context.Resources += amount * valuePerItem;

            if (ShoppeConstants.MAX_RESOURCES <= context.Resources)
                from.SendMessage("You add more resources, but now your shoppe is full.");
            else
                from.SendMessage("You add '{0}' resources to your shoppe.", amount);

            from.PlaySound(0x42); // Hammer01

            if (item.Amount < 1)
            {
                item.Delete();
                return true;
            }

            return false;
        }

        protected bool AddTools(Mobile from, Item item)
        {
            if ((item is IUsesRemaining) == false) return false;

            return AddTools(from, item, ((IUsesRemaining)item).UsesRemaining);
        }

        protected bool AddTools(Mobile from, Item item, int value)
        {
            var context = GetOrCreateContext(from);
            if (context == null) return false;
            if (ShoppeConstants.MAX_TOOLS <= context.Tools)
            {
                from.SendMessage("Your shoppe is already full of tools.");
                return false;
            }

            context.Tools += value;
            if (context.Tools >= ShoppeConstants.MAX_TOOLS)
            {
                context.Tools = ShoppeConstants.MAX_TOOLS;
                from.SendMessage("You add another tool, but now your shoppe is full.");
            }
            else
            {
                from.SendMessage("You add the tools to your shoppe.");
            }

            from.PlaySound(0x42); // Hammer01
            item.Delete();

            return true;
        }

        protected CustomerContext CreateCustomer(TradeSkillContext context)
        {
            string task = CreateTask(context);
            if (task == null)
            {
                Console.WriteLine("Failed to create task ({0})", GetType());
                return null;
            }

            // More Rep as you build your Reputation
            int repMax = context.Reputation / 10;
            int repMin = Math.Max(10, repMax / 5);
            if (repMax < repMin) repMax = repMin + 10;

            // More Gold as you build your Reputation
            int gold = Utility.RandomMinMax(repMin, repMax) * 4;
            gold = (int)(gold * MyServerSettings.GetGoldCutRate() * .01);

            var customer = new CustomerContext
            {
                Description = task,
                Difficulty = Math.Min(120, Math.Max(30, Utility.RandomMinMax((gold / 125) + 35, (gold / 125) + 35 + Utility.RandomMinMax(0, 5)))),
                GoldReward = gold,
                Person = CreatePersonName(),
                ReputationReward = Math.Min(50, Math.Max(5, Utility.RandomMinMax(gold / 20, (gold / 20) + Utility.RandomMinMax(0, 3)))),
                ResourceCost = Math.Min(1000, Math.Max(5, Utility.RandomMinMax(gold / 20, (gold / 20) + Utility.RandomMinMax(0, 5)))),
                ToolCost = Math.Min(10, Math.Max(1, Utility.RandomMinMax(gold / 100, (gold / 100) + Utility.RandomMinMax(0, 2)))),
            };

            return customer;
        }

        protected virtual IEnumerable<OrderContext> CreateOrders(Mobile from, TradeSkillContext context, int amount)
        {
            return null;
        }

        protected string CreatePersonName()
        {
            return string.Format("{0} {1}", Utility.RandomBool() ? NameList.RandomName("female") : NameList.RandomName("male"), TavernPatrons.GetTitle());
        }

        protected abstract string CreateTask(TradeSkillContext context);

        protected IEnumerable<CraftItem> GetCraftItems(Mobile from, CraftSystem craftSystem)
        {
            for (int i = 0; i < craftSystem.CraftGroups.Count; i++)
            {
                var group = craftSystem.CraftGroups.GetAt(i);

                for (int j = 0; j < group.CraftItems.Count; j++)
                {
                    var craftItem = group.CraftItems.GetAt(j);

                    bool hasSkills = true;
                    for (int k = 0; k < craftItem.Skills.Count; k++)
                    {
                        CraftSkill skill = craftItem.Skills.GetAt(k);
                        if (
                            from.Skills[skill.SkillToMake].Value < skill.MinSkill // Filter items you can't succeed on
                        )
                        {
                            hasSkills = false;
                            break;
                        }
                    }

                    if (hasSkills) yield return craftItem;
                }
            }
        }

        protected abstract ShoppeGump GetGump(PlayerMobile from);

        protected TradeSkillContext GetOrCreateContext(Mobile from)
        {
            return ShoppeEngine.Instance.GetOrCreateShoppeContext(from, ShoppeType);
        }

        protected virtual int GetSkillValue(Mobile from)
        {
            return (int)from.Skills[PrimarySkill].Value;
        }

        protected int GetSuccessChance(int skill, int difficulty)
        {
            int delta = skill - difficulty;
            if (delta < 1) return 0; // Too difficult
            if (delta >= 100) return 100; // Trivial

            return Math.Min(100, delta + 25);
        }

        protected virtual void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            context.Reputation = Math.Max(0, context.Reputation - customer.ReputationReward);
            from.SendSound(from.Female ? 812 : 1086); // Oops

            ProgressSkill(from);
            ProgressSkill(from); // Yep, twice
        }

        protected virtual void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            from.SendSound(0x2E6); // dropmny3
            context.Gold = Math.Min(ShoppeConstants.MAX_GOLD, context.Gold + customer.GoldReward);
            context.Reputation = Math.Min(ShoppeConstants.MAX_REPUTATION, context.Reputation + customer.ReputationReward);
            // TODO: Award Fame??

            ProgressSkill(from);
            ProgressSkill(from); // Yep, twice
        }

        protected void ProgressSkill(Mobile from)
        {
            from.CheckSkill(PrimarySkill, 0, 125);
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

    public class CashOutEntry : ContextMenuEntry
    {
        private readonly PlayerMobile m_From;
        private readonly TradeSkillContext m_Context;

        public CashOutEntry(PlayerMobile from, TradeSkillContext context) : base(6113, 3)
        {
            m_From = from;
            m_Context = context;
        }

        public override void OnClick()
        {
            if (m_Context.Gold <= 0) return;

            double barter = (int)(m_From.Skills[SkillName.Mercantile].Value / 2);
            if (m_From.NpcGuild == NpcGuild.MerchantsGuild)
                barter += 25.0; // FOR GUILD MEMBERS

            barter /= 100;

            int bonus = (int)(m_Context.Gold * barter);

            int cash = m_Context.Gold + bonus;

            m_From.AddToBackpack(new BankCheck(cash));
            m_From.SendMessage("You now have a check for " + cash.ToString() + " gold.");
            m_Context.Gold = 0;
        }
    }
}