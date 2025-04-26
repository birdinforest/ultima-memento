using System;

namespace Server.Engines.GlobalShoppe
{
    public interface IOrderContext
    {
        int CurrentAmount { get; set; }
        int GoldReward { get; set; }
        int GraphicId { get; set; }
        bool IsComplete { get; }
        bool IsInitialized { get; set; }
        bool IsValid { get; }
        string ItemName { get; set; }
        int MaxAmount { get; set; }
        string Person { get; set; }
        int PointReward { get; set; }
        int ReputationReward { get; set; }
        bool RequireExceptional { get; set; }
        CraftResource Resource { get; set; }
        Type Type { get; }

        void Serialize(GenericWriter writer);
    }
}