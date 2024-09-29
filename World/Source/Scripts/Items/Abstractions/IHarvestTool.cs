using Server.Engines.Harvest;

namespace Server.Items.Abstractions
{
    public interface IHarvestTool
    {
        HarvestSystem HarvestSystem { get; }

        bool HasHarvestSystem { get; }
    }
}