namespace Server.Engines.GlobalShoppe
{
    public class OrderFactory
    {
        public static IOrderContext Create(ShoppeType shoppeType, GenericReader reader)
        {
            switch (shoppeType)
            {
                case ShoppeType.Blacksmith:
                case ShoppeType.Bowyer:
                case ShoppeType.Carpentry:
                case ShoppeType.Tailor:
                    return new EquipmentOrderContext(reader);

                case ShoppeType.Tinker:
                    return new TinkerOrderContext(reader);

                case ShoppeType.Alchemist:
                    return new OrderContext(reader);

                case ShoppeType.Baker:
                case ShoppeType.Cartography:
                case ShoppeType.Herbalist:
                case ShoppeType.Librarian:
                case ShoppeType.Mortician:
                    break;
            }

            return null;
        }
    }
}