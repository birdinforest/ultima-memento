using System;
using System.Collections.Generic;

namespace Server.Engines.GlobalShoppe
{
    public class ShoppeEngine
    {
        private static ShoppeEngine m_Engine;

        private readonly Dictionary<string, PlayerContext> m_Context = new Dictionary<string, PlayerContext>();

        private InternalTimer m_RefreshTimer;

        public static ShoppeEngine Instance
        {
            get
            {
                if (m_Engine == null)
                    m_Engine = new ShoppeEngine();

                return m_Engine;
            }
        }

        public static void Configure()
        {
            EventSink.WorldLoad += OnWorldLoad;
            EventSink.WorldSave += OnWorldSave;
        }

        public static void Initialize()
        {
            LoadData();
        }

        public PlayerContext GetOrCreateContext(Mobile mobile)
        {
            var username = mobile.Account.Username;

            PlayerContext context;
            if (m_Context.TryGetValue(username, out context)) return context;

            return m_Context[username] = new PlayerContext();
        }

        public TradeSkillContext GetOrCreateShoppeContext(Mobile mobile, ShoppeType shoppeType)
        {
            var context = GetOrCreateContext(mobile);

            return context[shoppeType];
        }

        private static void LoadData()
        {
            Persistence.Deserialize(
                "Saves//Craft//Shoppes.bin",
                reader =>
                {
                    int version = reader.ReadInt();
                    int count = reader.ReadInt();

                    for (int i = 0; i < count; ++i)
                    {
                        var username = reader.ReadString();
                        var context = new PlayerContext(reader);
                        Instance.m_Context.Add(username, context);
                    }

                    Console.WriteLine("Loaded Global Shoppe data for '{0}' accounts", Instance.m_Context.Count);
                }
            );
        }

        private static void OnWorldLoad()
        {
            // Clean up any deleted Accounts
            // foreach (var username in Instance.m_Context.Keys.ToList())
            // {
            //     if (Accounts.GetAccount(username) == null) continue;

            //     Instance.m_Context.Remove(username);
            // }

            var interval = ShoppeConstants.CUSTOMER_REFRESH_INTERVAL;
            if (ShoppeConstants.CUSTOMER_REFRESH_DELAY < interval)
            {
                interval = TimeSpan.FromSeconds(ShoppeConstants.CUSTOMER_REFRESH_DELAY.TotalSeconds / 2);
                Console.WriteLine("CUSTOMER_REFRESH_DELAY must be larger the CUSTOMER_REFRESH_INTERVAL. Guessing at interval.");
            }

            Instance.m_RefreshTimer = new InternalTimer(interval);
            Instance.m_RefreshTimer.Start();
        }

        private static void OnWorldSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                "Saves//Craft//Shoppes.bin",
                writer =>
                {
                    writer.Write(0); // version

                    writer.Write(Instance.m_Context.Count);
                    foreach (var kv in Instance.m_Context)
                    {
                        writer.Write(kv.Key);
                        kv.Value.Serialize(writer);
                    }
                }
            );
        }

        private class InternalTimer : Timer
        {
            public InternalTimer(TimeSpan interval) : base(TimeSpan.Zero, interval)
            {
            }

            protected override void OnTick()
            {
                var now = DateTime.UtcNow;
                foreach (var context in Instance.m_Context.Values)
                {
                    foreach (var trade in context.Trades)
                    {
                        if (trade.CanRefreshCustomers || now < trade.NextCustomerRefresh) continue;

                        trade.CanRefreshCustomers = true;
                    }
                }
            }
        }
    }
}