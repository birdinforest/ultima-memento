using Server.Engines.Craft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Commands
{
    public class ItemPrices
    {
        private static readonly List<CraftSystem> AllCraftSystems = new List<CraftSystem>
        {
            DefAlchemy.CraftSystem,
            // DefApothecary.CraftSystem, // Godcrafting
            DefBlacksmithy.CraftSystem,
            DefBonecrafting.CraftSystem,
            DefBowFletching.CraftSystem,
            DefCarpentry.CraftSystem,
            DefCartography.CraftSystem,
            DefCooking.CraftSystem,
            DefDraconic.CraftSystem,
            DefDruidism.CraftSystem,
            DefGlassblowing.CraftSystem,
            DefInscription.CraftSystem,
            // DefLapidary.CraftSystem, // Godcrafting
            DefLeatherworking.CraftSystem,
            DefMasonry.CraftSystem,
            DefShelves.CraftSystem,
            // DefStitching.CraftSystem, // Godcrafting
            DefTailoring.CraftSystem,
            DefTinkering.CraftSystem,
            DefWands.CraftSystem,
            DefWaxingPot.CraftSystem,
            DefWitchery.CraftSystem,
        };

        private delegate void EvaluateCraftItem(CraftItem craftItem, int craftItemSaleInfoIndex, int totalSalePrice, int totalBuyPrice);

        public static void Initialize()
        {
            CommandSystem.Register("ItemPrices-ExportCraft", AccessLevel.Administrator, new CommandEventHandler(OnExportCraftCommand));
            CommandSystem.Register("ItemPrices-RecreateCraft", AccessLevel.Administrator, new CommandEventHandler(OnRecreateCraftCommand));
        }

        [Usage("ItemPrices-ExportCraft")]
        [Description("Creates a custom CSV file that details prices for craftable items")]
        public static void OnExportCraftCommand(CommandEventArgs e)
        {
            var allSellInfo = ItemSalesInfo.m_SellingInfo;
            Directory.CreateDirectory("Data/_Prices");

            foreach (var craftSystem in AllCraftSystems)
            {
                var sPath = string.Format("Data/_Prices/{0}.csv", craftSystem.GetType().Name);
                if (File.Exists(sPath))
                    File.Delete(sPath);

                using (var writer = new StreamWriter(sPath))
                {
                    writer.WriteLine("{0},{1},{2},{3},{4}", "ItemType", "Buy_From_VendorPrice", "BuyAllResources_From_VendorPrice", "Sell_ToVendor_Price", "Sell All Resources_ToVendor_Price");

                    CalculateCraftedItemResourcePrice(craftSystem, allSellInfo, (craftItem, craftItemSaleInfoIndex, totalSalePrice, totalBuyPrice) =>
                    {
                        var saleInfo = allSellInfo[craftItemSaleInfoIndex];
                        var buyPrice = ItemInformation.GetBuysPrice(craftItemSaleInfoIndex, false, null, false, false);

                        writer.WriteLine("{0},{1},{2},{3},{4}", craftItem.ItemType.Name, saleInfo.iPrice, totalSalePrice, buyPrice, totalBuyPrice);
                    });

                    writer.Flush();
                }

                e.Mobile.SendMessage("Created file: {0}", sPath);
            }
        }

        [Usage("ItemPrices-RecreateCraft")]
        [Description("Creates a file that details price changes and produces copy/pastable output")]
        public static void OnRecreateCraftCommand(CommandEventArgs e)
        {
            var allSellInfo = ItemSalesInfo.m_SellingInfo;
            var updatedSaleInfoLookup = new Dictionary<int, ItemSalesInfo>();

            foreach (var craftSystem in AllCraftSystems)
            {
                CalculateCraftedItemResourcePrice(craftSystem, allSellInfo, (craftItem, craftItemSaleInfoIndex, totalSalePrice, totalBuyPrice) =>
                {
                    var saleInfo = allSellInfo[craftItemSaleInfoIndex];
                    var buyPrice = ItemInformation.GetBuysPrice(craftItemSaleInfoIndex, false, null, false, false);

                    ItemSalesInfo existing;
                    if (updatedSaleInfoLookup.TryGetValue(craftItemSaleInfoIndex, out existing))
                    {
                        Console.WriteLine("An item has already had it's price calculated: {0}", craftItem.ItemType);
                    }

                    // Items should sell for twice the value of the reagents required to craft it
                    var calculatedSalePrice = 2 * totalSalePrice;

                    updatedSaleInfoLookup[craftItemSaleInfoIndex] = new ItemSalesInfo(saleInfo.ItemsType, calculatedSalePrice, saleInfo.iQty, saleInfo.iRarity, saleInfo.iSells, saleInfo.iBuys, saleInfo.iWorld, saleInfo.iCategory, saleInfo.iMaterial, saleInfo.iMarket);
                });
            }

            // Write out a file with all items in the same order
            // Use the updated prices where appropriate
            Directory.CreateDirectory("Data/_UpdatedPrices");
            string sPath = "Data/_UpdatedPrices/updated.txt";
            if (File.Exists(sPath))
                File.Delete(sPath);

            using (var writer = new StreamWriter(sPath))
            {
                writer.WriteLine("Updated prices:");
                writer.WriteLine("TypeName, OldPrice, NewPrice, Delta");
                foreach (var kvp in updatedSaleInfoLookup.OrderBy(x => x.Value.ItemsType.Name))
                {
                    var oldItem = allSellInfo[kvp.Key];
                    var newItem = kvp.Value;
                    writer.WriteLine("{0}, {1}, {2}, {3}", newItem.ItemsType.Name, oldItem.iPrice, newItem.iPrice, newItem.iPrice - oldItem.iPrice);
                }

                writer.WriteLine();
                writer.WriteLine("File contents:");
                writer.WriteLine();

                for (var i = 0; i < allSellInfo.Length; i++)
                {
                    ItemSalesInfo saleInfo;
                    if (!updatedSaleInfoLookup.TryGetValue(i, out saleInfo))
                        saleInfo = allSellInfo[i]; // Default to the pre-existing value

                    writer.WriteLine("			new ItemSalesInfo( typeof(	{0}	),	{1}	,	{2}	,	{3}	,	{4}	,	{5}	,	World.{6}	,	Category.{7}	,	Material.{8}	,	Market.{9}	),", saleInfo.ItemsType.Name, saleInfo.iPrice, saleInfo.iQty, saleInfo.iRarity, saleInfo.iSells ? "true" : "false", saleInfo.iBuys ? "true" : "false", saleInfo.iWorld, saleInfo.iCategory, saleInfo.iMaterial, saleInfo.iMarket);
                }

                writer.Flush();
            }
        }

        private static void CalculateCraftedItemResourcePrice(CraftSystem craftSystem, ItemSalesInfo[] allSellInfo, EvaluateCraftItem evaluate)
        {
            for (int groupIndex = 0; groupIndex < craftSystem.CraftGroups.Count; groupIndex++)
            {
                var group = craftSystem.CraftGroups.GetAt(groupIndex);

                for (int itemIndex = 0; itemIndex < group.CraftItems.Count; itemIndex++)
                {
                    var craftItem = group.CraftItems.GetAt(itemIndex);
                    var craftItemSaleInfoIndex = Array.FindIndex(allSellInfo, info => info.ItemsType == craftItem.ItemType);
                    if (craftItemSaleInfoIndex < 0)
                    {
                        Console.WriteLine("An item is craftable but does not have an entry for sale: '{0}'", craftItem.ItemType.Name);
                        continue;
                    }

                    var totalSalePrice = 0;
                    var totalBuyPrice = 0;
                    for (int resourceIndex = 0; resourceIndex < craftItem.Resources.Count; resourceIndex++)
                    {
                        var resource = craftItem.Resources.GetAt(resourceIndex);
                        if (resource == null) break;

                        var craftResourceItemSaleInfoindex = Array.FindIndex(allSellInfo, info => info.ItemsType == resource.ItemType);
                        if (craftResourceItemSaleInfoindex < 0)
                        {
                            Console.WriteLine("A resource does not have an entry for sale: '{0}'", resource.ItemType.Name);
                            continue;
                        }

                        var resourceSaleInfo = allSellInfo[craftResourceItemSaleInfoindex];
                        var resourceBuyPrice = ItemInformation.GetBuysPrice(craftResourceItemSaleInfoindex, false, null, false, false);

                        totalSalePrice += resource.Amount * resourceSaleInfo.iPrice;
                        totalBuyPrice += resource.Amount * resourceBuyPrice;
                    }

                    evaluate(craftItem, craftItemSaleInfoIndex, totalSalePrice, totalBuyPrice);
                }
            }
        }
    }
}