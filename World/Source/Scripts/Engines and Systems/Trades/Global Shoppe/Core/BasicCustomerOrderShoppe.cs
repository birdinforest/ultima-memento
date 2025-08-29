using Server.Items;

namespace Server.Engines.GlobalShoppe
{
	[SkipSerializeReq]
	public abstract class BasicCustomerOrderShoppe : CustomerOrderShoppe<EquipmentOrderContext>
	{
		protected BasicCustomerOrderShoppe(Serial serial) : base(serial)
		{
		}

		protected BasicCustomerOrderShoppe(int itemId) : base(itemId)
		{
		}

		protected override string GetDescription(EquipmentOrderContext order)
		{
			var description = string.Format("Craft {0}", order.MaxAmount);
			if (order.RequireExceptional) description += " exceptional";
			if (order.Resource != CraftResource.None) description = string.Format("{0} {1}", description, CraftResources.GetResourceName(order.Resource));

			description = string.Format("{0} {1}", description, order.ItemName);

			return description;
		}
	}
}