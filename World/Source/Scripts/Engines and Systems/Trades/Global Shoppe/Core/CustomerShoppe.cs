using Server.Items;
using Server.Mobiles;
using Server.Utilities;
using System;

namespace Server.Engines.GlobalShoppe
{
	[SkipSerializeReq]
	public abstract class CustomerShoppe : ShoppeBase, ICustomerShoppe
	{
		protected CustomerShoppe(Serial serial) : base(serial)
		{
		}

		protected CustomerShoppe(int itemId) : base(itemId)
		{
		}

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

		public bool CanAcceptCustomer(TradeSkillContext context, CustomerContext customer)
		{
			return HasEnoughTools(context, customer) && HasEnoughResources(context, customer) && HasGoldCapacity(context, customer);
		}

		public int GetReputationBonus(PlayerMobile from)
		{
			if (from.NpcGuild != Guild) return 0;

			var playerSkillBonus = 5 * GetSkillValue(from);

			return 500 + playerSkillBonus;
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

		public void RejectCustomer(int index, TradeSkillContext context)
		{
			if (context.Customers.Count <= index) return;

			var customer = context.Customers[index];
			context.Reputation = Math.Max(0, context.Reputation - customer.ReputationReward);

			context.Customers.Remove(customer);
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
			SkillUtilities.DoSkillChecks(from, SkillName.Mercantile, 1);

			ProgressSkill(from);
			ProgressSkill(from); // Yep, twice
		}

		protected override void OpenGump(Mobile from, TradeSkillContext context)
		{
			if (context.FeePaid && context.CanRefreshCustomers)
			{
				while (context.Customers.Count < ShoppeConstants.MAX_CUSTOMERS)
				{
					context.Customers.Add(CreateCustomer(context));
				}

				context.CanRefreshCustomers = false;
				context.NextCustomerRefresh = DateTime.UtcNow.Add(ShoppeConstants.CUSTOMER_REFRESH_DELAY);
			}

			base.OpenGump(from, context);
		}

		protected void ProgressSkill(Mobile from)
		{
			from.CheckSkill(PrimarySkill, 0, 125);
		}
	}
}