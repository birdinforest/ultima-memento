using System;

namespace Server.Engines.GlobalShoppe
{
    public interface IOrderContext
    {
        int CurrentAmount { get; set; }
        int GoldReward { get; set; }
        int GraphicId { get; }
        bool IsComplete { get; }
        bool IsInitialized { get; }
        bool IsValid { get; }
        string ItemName { get; }
        int MaxAmount { get; }
        string Person { get; }
        int PointReward { get; set; }
        int ReputationReward { get; set; }
        bool RequireExceptional { get; }
        CraftResource Resource { get; }
        Type Type { get; }

        void Serialize(GenericWriter writer);
    }
}