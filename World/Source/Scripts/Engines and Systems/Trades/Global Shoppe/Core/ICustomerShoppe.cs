using Server.Mobiles;

namespace Server.Engines.GlobalShoppe
{
    public interface ICustomerShoppe
    {
        bool CanAcceptCustomer(TradeSkillContext context, CustomerContext customer);

        int GetReputationBonus(PlayerMobile from);

        int GetSuccessChance(PlayerMobile from, int difficulty);

        bool HasEnoughResources(TradeSkillContext context, CustomerContext customer);

        bool HasEnoughTools(TradeSkillContext context, CustomerContext customer);

        bool HasGoldCapacity(TradeSkillContext context, CustomerContext customer);
    }
}