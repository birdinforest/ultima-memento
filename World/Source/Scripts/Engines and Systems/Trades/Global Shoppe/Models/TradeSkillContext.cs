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
			CanRefreshOrders = true;
			Orders = new List<IOrderContext>();
		}

		public TradeSkillContext(ShoppeType shoppeType, GenericReader reader)
		{
			CanRefreshCustomers = true;
			Customers = new List<CustomerContext>();
			CanRefreshOrders = true;
			Orders = new List<IOrderContext>();

			int version = reader.ReadInt();

			FeePaid = reader.ReadBool();
			Tools = reader.ReadInt();
			Resources = reader.ReadInt();
			Reputation = reader.ReadInt();
			Gold = reader.ReadInt();
			if (1 < version)
				Points = reader.ReadInt();

			var count = reader.ReadInt();
			for (int i = 0; i < count; ++i)
			{
				var customer = new CustomerContext(reader);
				Customers.Add(customer);
			}

			if (0 < version)
			{
				var orderCount = reader.ReadInt();
				for (int i = 0; i < orderCount; ++i)
				{
					var order = OrderFactory.Create(shoppeType, reader);
					if (!order.IsValid) continue;

					Orders.Add(order);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CanRefreshCustomers { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CanRefreshOrders { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public List<CustomerContext> Customers { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool FeePaid { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Gold { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextCustomerRefresh { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextOrderRefresh { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public List<IOrderContext> Orders { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Points { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Reputation { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Resources { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Tools { get; set; }

		public void Serialize(GenericWriter writer)
		{
			writer.Write((int)2); // version

			writer.Write(FeePaid);
			writer.Write(Tools);
			writer.Write(Resources);
			writer.Write(Reputation);
			writer.Write(Gold);
			writer.Write(Points);

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

			writer.Write(Orders.Count);
			for (int i = 0; i < Orders.Count; ++i)
			{
				var order = Orders[i];
				order.Serialize(writer);
			}
		}

		public override string ToString()
		{
			return "...";
		}
	}
}