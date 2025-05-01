namespace Server.Engines.GlobalShoppe
{
    public interface IOrderShoppe
    {
        void CompleteOrder(int index, Mobile from, TradeSkillContext context);

        string GetDescription(IOrderContext order);

        void OpenOrderGump(int index, Mobile from, TradeSkillContext context);

        void RejectOrder(int index, TradeSkillContext context);
    }
}