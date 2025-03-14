using System;
using System.Collections.Generic;

namespace Server.Engines.GlobalShoppe
{
    [PropertyObject]
    public class TradeSkillContext
    {
        public TradeSkillContext()
        {
            CanRefreshCustomers = true;
            Customers = new List<CustomerContext>();
        }

        public TradeSkillContext(GenericReader reader)
        {
            CanRefreshCustomers = true;
            Customers = new List<CustomerContext>();

            int version = reader.ReadInt();

            FeePaid = reader.ReadBool();
            Tools = reader.ReadInt();
            Resources = reader.ReadInt();
            Reputation = reader.ReadInt();
            Gold = reader.ReadInt();

            var count = reader.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                var customer = new CustomerContext(reader);
                Customers.Add(customer);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool CanRefreshCustomers { get; set; } // Not Serialized

        [CommandProperty(AccessLevel.GameMaster)]
        public List<CustomerContext> Customers { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool FeePaid { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Gold { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextCustomerRefresh { get; set; } // Not Serialized

        [CommandProperty(AccessLevel.GameMaster)]
        public int Reputation { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Resources { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Tools { get; set; }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)0); // version

            writer.Write(FeePaid);
            writer.Write(Tools);
            writer.Write(Resources);
            writer.Write(Reputation);
            writer.Write(Gold);

            if (CanRefreshCustomers || !ShoppeConstants.SAVE_CUSTOMERS_TO_DISK)
            {
                writer.Write(0);
            }
            else
            {
                writer.Write(Customers.Count);
                for (int i = 0; i < Customers.Count; ++i)
                {
                    var customer = Customers[i];
                    customer.Serialize(writer);
                }
            }
        }

        public override string ToString()
        {
            return "...";
        }
    }
}