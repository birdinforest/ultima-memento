namespace Server.Engines.GlobalShoppe
{
    public interface IOrderShoppe
    {
        void AddOrderItem(int index, Mobile from, TradeSkillContext context);

        void CompleteOrder(int index, Mobile from, TradeSkillContext context);

        void RejectOrder(int index, TradeSkillContext context);
    }
}