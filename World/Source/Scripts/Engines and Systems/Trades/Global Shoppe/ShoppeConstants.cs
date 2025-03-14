using System;

namespace Server.Engines.GlobalShoppe
{
    public class ShoppeConstants
    {
        public const int MAX_CUSTOMERS = 12;
        public const int MAX_GOLD = 500000;
        public const int MAX_REPUTATION = 10000;
        public const int MAX_RESOURCES = 5000;
        public const int MAX_TOOLS = 1000;
        public const int MIN_SKILL = 50;
        public const bool SAVE_CUSTOMERS_TO_DISK = false;
        public const int SHOPPE_FEE = 10000;
        public static TimeSpan CUSTOMER_REFRESH_DELAY = TimeSpan.FromHours(4); // Time for them to respawn
        public static TimeSpan CUSTOMER_REFRESH_INTERVAL = TimeSpan.FromMinutes(5);
    }
}