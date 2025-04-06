using Server.Engines.Craft;
using System;
using System.Linq;

namespace Server.Engines.GlobalShoppe
{
    public abstract class BaseRewardCalculator
    {
        public abstract int ComputeGold(int quantity, bool exceptional, CraftResource resource, Type type);

        public abstract int ComputePoints(int quantity, bool exceptional, CraftResource resource, Type type);

        public abstract int ComputeReputation(int quantity, bool exceptional, CraftResource resource, Type type, int currentReputation);

        protected int GetResourcePerCraft(CraftItem craftItem, Type baseResourceType)
        {
            int resourcePerCraft = 0;
            for (int i = 0; i < craftItem.Resources.Count; i++)
            {
                var resource = craftItem.Resources.GetAt(i);
                if (resource == null) break;

                if (baseResourceType.IsAssignableFrom(resource.ItemType))
                {
                    resourcePerCraft = resource.Amount;
                    break;
                }
            }

            return resourcePerCraft;
        }

        protected int GetSellPrice(Type resourceType)
        {
            var sellInfo = ItemSalesInfo.m_SellingInfo.FirstOrDefault(info => info.ItemsType == resourceType);
            if (sellInfo == null)
            {
                Console.WriteLine("Failed to find item price for '{0}'", resourceType);
                return 0;
            }

            return sellInfo.iPrice;
        }
    }
}