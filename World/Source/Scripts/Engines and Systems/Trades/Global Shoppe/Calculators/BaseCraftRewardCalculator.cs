using Server.Engines.Craft;
using Server.Items;
using System;

namespace Server.Engines.GlobalShoppe
{
	public abstract class BaseCraftRewardCalculator<TOrderContext> : BaseRewardCalculator
		where TOrderContext : class, IOrderContext, IResourceItem, IExceptionalItem
	{
		public void SetRewards(TradeSkillContext context, TOrderContext order)
		{
			order.GoldReward = ComputeGold(context, order);
			order.PointReward = ComputePoints(context, order);
			order.ReputationReward = ComputeReputation(context, order);
		}

		protected virtual int ComputeGold(TradeSkillContext context, TOrderContext order)
		{
			var pricePerCraftedItem = ComputePricePerCraftedItem(order.Resource, order.Type);
			if (pricePerCraftedItem < 1) return 0;

			double price = order.MaxAmount * pricePerCraftedItem;

			// Exceptional bonus
			if (order.RequireExceptional)
				price *= 1.25;

			var resourceMultiplier = CraftResources.GetGold(order.Resource);
			if (0 < resourceMultiplier)
				price = (int)(price * resourceMultiplier);

			// Reduce by arbitrary amount
			return (int)(price / 3);
		}

		protected virtual int ComputePoints(TradeSkillContext context, TOrderContext order)
		{
			// Reduce by arbitrary amount
			return ComputeRewardFromResourceValue(order.MaxAmount, order.RequireExceptional, order.Resource, order.Type) / 5;
		}

		protected int ComputePricePerCraftedItem(CraftResource resource, Type type)
		{
			// Keep the value of each item in sync with the primary resource investment
			var craftItem = FindCraftItem(type);
			if (craftItem == null) return 0;

			var resourcePerCraft = GetResourceAmountPerCraft(craftItem, resource);
			if (resourcePerCraft < 1) return 0;

			var craftResourceType = GetCraftResource(craftItem, resource);
			if (craftResourceType == null) return 0;

			var resourceSellPrice = GetSellPrice(craftResourceType);
			if (resourceSellPrice < 1) return 0;

			var pricePerCraftedItem = resourceSellPrice * resourcePerCraft;

			return pricePerCraftedItem;
		}

		protected virtual int ComputeReputation(TradeSkillContext context, TOrderContext order)
		{
			// Reduce by arbitrary amount
			var reward = ComputeRewardFromResourceValue(order.MaxAmount, order.RequireExceptional, order.Resource, order.Type) / 100;

			reward = (int)Math.Max(10, reward - 0.5 * ((double)context.Reputation / ShoppeConstants.MAX_REPUTATION));

			return reward;
		}

		protected virtual int ComputeRewardFromResourceValue(int quantity, bool exceptional, CraftResource resource, Type type)
		{
			var pricePerCraftedItem = ComputePricePerCraftedItem(resource, type);
			if (pricePerCraftedItem < 1) return 0;

			double points = quantity * pricePerCraftedItem;

			// Exceptional bonus
			if (exceptional)
				points *= 1.25;

			// Flat material bonus
			var materialMultiplier = GetResourceMultiplier(resource);
			if (0 < materialMultiplier)
				points += (int)(quantity * materialMultiplier + materialMultiplier); // Major bonus per item + Flat material bonus

			return (int)points;
		}

		protected abstract CraftItem FindCraftItem(Type type);

		protected abstract Type GetCraftResource(CraftItem craftItem, CraftResource resource);

		protected abstract int GetResourceAmountPerCraft(CraftItem craftItem, CraftResource resource);

		protected abstract double GetResourceMultiplier(CraftResource resource);
	}
}