using Server.Engines.Craft;

namespace Server.Engines.GlobalShoppe
{
	public interface IDiagnosticOrderShoppe
	{
		CraftSystem CraftSystem { get; }

		IOrderContext CreateOrder(CraftSystem craftSystem, Mobile from, TradeSkillContext context);

		void PrepareOrders(TradeSkillContext context);
	}
}