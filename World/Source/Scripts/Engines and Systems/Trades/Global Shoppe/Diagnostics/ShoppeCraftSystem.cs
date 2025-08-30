using Server.Engines.Craft;
using Server.Items;
using Server.Network;
using Server.Targeting;
using System;

namespace Server.Engines.GlobalShoppe
{
	/// <summary>
	/// This is a stub CraftSystem that is used to export the craft items from a CraftSystem.
	/// </summary>
	public class ShoppeCraftSystem : CraftSystem
	{
		private static ShoppeCraftSystem m_CraftSystem;
		private SkillName _mainSkill;

		public ShoppeCraftSystem() : base(1, 1, 1.25)
		{
		}

		public static ShoppeCraftSystem Instance
		{
			get
			{
				if (m_CraftSystem == null)
					m_CraftSystem = new ShoppeCraftSystem();

				return m_CraftSystem;
			}
		}

		public override SkillName MainSkill
		{ get { return _mainSkill; } }

		public override int CanCraft(Mobile from, BaseTool tool, Type itemType)
		{
			throw new NotImplementedException();
		}

		public TradeSkillContext CreateUniqueOrders(Mobile from, IDiagnosticOrderShoppe shoppe)
		{
			var testCraftSystem = Instance;
			testCraftSystem._mainSkill = shoppe.CraftSystem.MainSkill;

			var context = new TradeSkillContext();
			foreach (var item in shoppe.CraftSystem.CraftItems)
			{
				var craftItem = item as CraftItem;
				if (craftItem == null)
				{
					Console.WriteLine("CraftItem is not a CraftItem: {0}", item.GetType());
					continue;
				}

				testCraftSystem.CraftGroups.Clear();
				if (false == (shoppe is TinkerShoppe)) // Tinker needs to retain Resources for Jewelry with Gems
					testCraftSystem.CraftSubRes.Clear();

				testCraftSystem.CraftItems.Clear();
				int index = testCraftSystem.AddCraft(craftItem.ItemType, "TEST_GROUP", "TEST_ITEM", 0, 0, craftItem.Resources.GetAt(0).ItemType, "TEST_RESOURCE", 1);
				if (1 < craftItem.Resources.Count)
				{
					for (int i = 1; i < craftItem.Resources.Count; i++)
					{
						var resource = craftItem.Resources.GetAt(i);
						testCraftSystem.AddRes(index, resource.ItemType, "TEST_RESOURCE" + i, resource.Amount);
					}
				}

				var order = shoppe.CreateOrder(testCraftSystem, from, context);
				if (order == null)
				{
					Console.WriteLine("No order created for {0}", item.GetType());
					continue;
				}

				context.Orders.Add(order);
			}

			return context;
		}

		public void Export(Mobile from)
		{
			from.Target = new InternalTarget();
		}

		public void ExportOrders(Mobile from, IDiagnosticOrderShoppe shoppe)
		{
			var context = CreateUniqueOrders(from, shoppe);

			// Only produce the most basic orders
			context.Orders.ForEach(order =>
			{
				const int AMOUNT_TO_MAKE = 10;
				if (order is EquipmentOrderContext)
				{
					var o = (EquipmentOrderContext)order;
					o.Resource = CraftResource.None;
					o.MaxAmount = AMOUNT_TO_MAKE;
					o.RequireExceptional = false;
				}
				else if (order is TinkerOrderContext)
				{
					var o = (TinkerOrderContext)order;
					o.Resource = CraftResource.None;
					o.MaxAmount = AMOUNT_TO_MAKE;
				}
				else if (order is OrderContext)
				{
					var o = (OrderContext)order;
					o.MaxAmount = AMOUNT_TO_MAKE;
				}
			});

			// Set rewards
			shoppe.PrepareOrders(context);

			Console.WriteLine(string.Format("{0},{1},{2},{3},{4}", "Type", "Amount", "ReputationReward", "GoldReward", "PointReward"));
			foreach (var order in context.Orders)
			{
				if (order is TinkerOrderContext)
				{
					Console.WriteLine(string.Format("{0},{1},{2},{3},{4}", string.Format("{0} ({1})", order.Type.Name, ((TinkerOrderContext)order).GemType), order.MaxAmount, order.ReputationReward, order.GoldReward, order.PointReward));
				}
				else
				{
					Console.WriteLine(string.Format("{0},{1},{2},{3},{4}", order.Type.Name, order.MaxAmount, order.ReputationReward, order.GoldReward, order.PointReward));
				}
			}

			Console.WriteLine("Shoppe Orders Exported");
		}

		public override double GetChanceAtMin(CraftItem item)
		{
			throw new NotImplementedException();
		}

		public override void InitCraftList()
		{
		}

		public override void PlayCraftEffect(Mobile from)
		{
			throw new NotImplementedException();
		}

		public override int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, CraftItem item)
		{
			throw new NotImplementedException();
		}

		private class InternalTarget : Target
		{
			public InternalTarget() : base(-1, false, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (from == null) return;

				if (o is CartographyShoppe) { }
				else if (o is IDiagnosticOrderShoppe) Instance.ExportOrders(from, (IDiagnosticOrderShoppe)o);
				else from.PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, string.Format("That is not a valid Shoppe. {0}", o.GetType()), from.NetState);
			}
		}
	}
}