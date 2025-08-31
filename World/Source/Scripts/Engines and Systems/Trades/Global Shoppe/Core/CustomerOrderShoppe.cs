using Server.Engines.Craft;
using Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.GlobalShoppe
{
	[SkipSerializeReq]
	public abstract class CustomerOrderShoppe<TOrderContext> : CustomerShoppe, IOrderShoppe, IDiagnosticOrderShoppe
		where TOrderContext : class, IOrderContext
	{
		protected CustomerOrderShoppe(Serial serial) : base(serial)
		{
		}

		protected CustomerOrderShoppe(int itemId) : base(itemId)
		{
		}

		public abstract CraftSystem CraftSystem { get; }

		public void CompleteOrder(IOrderContext order, Mobile from, TradeSkillContext context, RewardType selectedReward)
		{
			if (!order.IsComplete) return;
			if (!context.Orders.Contains(order)) return;

			switch (selectedReward)
			{
				case RewardType.Gold:
					context.Gold += order.GoldReward;
					break;
				case RewardType.Points:
					context.Points += order.PointReward;
					break;
				case RewardType.Reputation:
					context.Reputation = Math.Min(ShoppeConstants.MAX_REPUTATION, context.Reputation + order.ReputationReward);
					break;
			}

			SkillUtilities.DoSkillChecks(from, SkillName.Mercantile, 3);
			context.Orders.Remove(order);

			from.PlaySound(0x32); // Dropgem1
		}

		public IOrderContext CreateOrder(CraftSystem craftSystem, Mobile from, TradeSkillContext context)
		{
			return CreateOrders(craftSystem, from, context, 1).FirstOrDefault();
		}

		public string GetDescription(IOrderContext order)
		{
			var typed = order as TOrderContext;
			if (typed == null) return "invalid_order";

			return GetDescription(typed);
		}

		public void OpenOrderGump(int index, Mobile from, TradeSkillContext context)
		{
			if (context.Orders.Count <= index) return;

			var order = context.Orders[index];
			if (order.IsComplete) return;

			from.CloseGump(typeof(OrderGump));
			from.SendGump(new OrderGump(from, order));
		}

		public void OpenRewardSelectionGump(int index, Mobile from, TradeSkillContext context)
		{
			if (context.Orders.Count <= index) return;

			var order = context.Orders[index];
			if (!order.IsComplete) return;

			from.CloseGump(typeof(RewardSelectionGump));
			from.SendGump(new RewardSelectionGump(from, this, context, order));
		}

		public abstract void PrepareOrders(TradeSkillContext context);

		public void RejectOrder(int index, TradeSkillContext context)
		{
			if (context.Orders.Count <= index) return;

			var order = context.Orders[index];
            context.Reputation = Math.Max(0, context.Reputation - order.ReputationReward);
			context.Orders.Remove(order);
		}

		protected abstract IEnumerable<TOrderContext> CreateOrders(CraftSystem craftSystem, Mobile from, TradeSkillContext context, int amount);

		protected abstract string GetDescription(TOrderContext order);

		protected override void OpenGump(Mobile from, TradeSkillContext context)
		{
			if (context.FeePaid && context.CanRefreshOrders)
			{
				var count = ShoppeConstants.MAX_ORDERS - context.Orders.Count;
				foreach (var order in CreateOrders(CraftSystem, from, context, count))
				{
					context.Orders.Add(order);
				}

				context.CanRefreshOrders = false;
				context.NextOrderRefresh = DateTime.UtcNow.Add(ShoppeConstants.ORDER_REFRESH_DELAY);
			}

			base.OpenGump(from, context);
		}
	}
}