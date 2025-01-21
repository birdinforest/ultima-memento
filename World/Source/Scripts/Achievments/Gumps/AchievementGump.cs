using Server.Gumps;
using System.Collections.Generic;
using Server.Network;
using System.Linq;
using System;
using Server;

namespace Scripts.Mythik.Systems.Achievements.Gumps
{
    class AchievementGump : Gump
    {
        public const int COLOR_HTML = 0xf7fbde; // RGB888
        public const int COLOR_LABEL = 1918; // Hue from files
        public const int COLOR_LOCALIZED = 0xf7db; // RGB565

        private readonly int m_curTotal;
        private readonly Dictionary<int, AchieveData> m_curAchieves;

        public AchievementGump(Dictionary<int, AchieveData> achieves, int total, int selectedCategoryId = -1) : base(25, 25)
        {
            m_curAchieves = achieves;
            m_curTotal = total;
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 1053, 605, 3600); // Border
            AddImage(15, 14, 13000); // UO background
            AddImage(419, 23, 13001); // Achievements

            var selectedCategory = AchievementSystem.Categories.FirstOrDefault(c => c.ID == selectedCategoryId);
            var categories = selectedCategory != null
                ? AchievementSystem.Categories.Where(cat =>
                    cat == selectedCategory // Match
                    || cat.ID == selectedCategory.Parent // Parent of selected category
                    || (0 < cat.Parent && cat.Parent == selectedCategory.ID) // Child of selected category
                    || (0 < cat.Parent && cat.Parent == selectedCategory.Parent) // Sibling of selected category
                )
                : AchievementSystem.Categories.Where(cat => cat.Parent == 0);
            var categoryList = categories.ToList();

            var addBackButton = selectedCategory != null; // TODO: Add visual
            if (addBackButton)
            {
                AddButton(27, 29, 4014, 4016, 1, GumpButtonType.Reply, 0);
                AddLabel(60, 32, 1153, "All Categories");
            }

            int i = 0;
            for (int index = 0; index < categoryList.Count; index++)
            {
                int x = 27;
                int bgID = 3600;
                var category = categoryList[index];

                const int CATEGORY_WIDTH = 209;
                if (category.Parent == 0)
                {
                    AddBackground(x, 68 + (i * 56), CATEGORY_WIDTH, 50, bgID);
                }
                else
                {
                    x += 20;
                    AddBackground(x, 68 + (i * 56), CATEGORY_WIDTH - 40, 50, bgID);
                }

                if (category.ID == selectedCategoryId) // selected
                    AddImage(x + 17, 86 + (i * 56), 1210, 1152);
                else
                    AddButton(x + 17, 86 + (i * 56), 1209, 1210, 5000 + category.ID, GumpButtonType.Reply, 0);

                TextDefinition.AddHtmlText(this, x + 37, 83 + (i * 56), CATEGORY_WIDTH, 16, category.Name, false, false, COLOR_LOCALIZED, COLOR_HTML);
                ++i;
            }

            const int ITEMS_PER_PAGE = 6;
            var achievements = selectedCategory != null
                ? AchievementSystem.Achievements.Where(ac => ac.CategoryID == selectedCategoryId
                    && !ac.HiddenTillComplete
                    && (
                        ac.PreReq == null // No Pre-req
                        || (achieves.ContainsKey(ac.PreReq.ID) && achieves[ac.PreReq.ID].IsComplete) // Pre-req is complete
                    )
                )
                : AchievementSystem.Achievements
                    .Where(ac => achieves.ContainsKey(ac.ID) && achieves[ac.ID].IsComplete)
                    .OrderByDescending(ac => achieves[ac.ID].CompletedOn)
                    .Take(ITEMS_PER_PAGE);

            var achievementList = achievements.ToList();

            var maxPages = (int)Math.Ceiling((double)achievementList.Count / ITEMS_PER_PAGE);
            for (int page = 0; page < maxPages; page++)
            {
                var pageNumber = page + 1;
                AddPage(pageNumber);

                // Add items
                for (int index = 0; index < ITEMS_PER_PAGE; index++)
                {
                    var itemIndex = (page * ITEMS_PER_PAGE) + index;
                    if (achievementList.Count <= itemIndex) break;

                    var achievement = achievementList[itemIndex];
                    AchieveData data = null;
                    achieves.TryGetValue(achievement.ID, out data);
                    AddAchieve(achievement, itemIndex, ITEMS_PER_PAGE, data);
                }

                if (maxPages == 1) continue; // Don't show buttons if they do nothing

                // Add page buttons
                AddButton(282, 549, 4014, 4015, 0, GumpButtonType.Page, pageNumber == 1 ? maxPages : pageNumber - 1); // Previous
                AddLabel(615, 551, COLOR_LABEL, "Page " + pageNumber);
                AddButton(974, 549, 4005, 4006, 0, GumpButtonType.Page, pageNumber == maxPages ? 1 : pageNumber + 1); // Next
            }
        }

        private void AddAchieve(BaseAchievement ac, int i, int itemsPerPage, AchieveData acheiveData)
        {
            const int CARD_HEIGHT = 68;
            const int CARD_GAP = 11;
            const int HEIGHT_PER_CARD = CARD_HEIGHT + CARD_GAP;

            int index = i % itemsPerPage; // Item index

            var isComplete = acheiveData != null && acheiveData.IsComplete;
            var title = isComplete || !ac.HideTitle ? ac.Title : "???";
            AddBackground(277, CARD_HEIGHT + (index * HEIGHT_PER_CARD), 727, 73, 3600);
            AddLabel(350, 15 + CARD_HEIGHT + (index * HEIGHT_PER_CARD), 49, title);
            if (ac.ItemIcon > 0)
            {
                Rectangle2D bounds = ItemBounds.Table[ac.ItemIcon];
                int y = 35 + CARD_HEIGHT + (index * HEIGHT_PER_CARD);
                AddItem(321 - bounds.Width / 2 - bounds.X , y - bounds.Height / 2 - bounds.Y, ac.ItemIcon);
            }

            if (!isComplete && 1 < ac.CompletionTotal)
            {
                var progress = acheiveData != null ? acheiveData.Progress : 0;
                AddImageTiled(890, 84 + (index * HEIGHT_PER_CARD), 95, 9, 9750); // Gray progress

                if (acheiveData != null)
                {
                    var step = 95.0 / ac.CompletionTotal;

                    if (0 < progress)
                        AddImageTiled(890, 84 + (index * HEIGHT_PER_CARD), (int)(progress * step), 9, 9752); // Green progress
                }

                TextDefinition.AddHtmlText(this, 912, 23 + CARD_HEIGHT + (index * HEIGHT_PER_CARD), 75, 16, string.Format("<RIGHT>{0} / {1}</RIGHT>", progress, ac.CompletionTotal), false, false, COLOR_LOCALIZED, COLOR_HTML);
            }

            var description = isComplete || !ac.HideDesc ? ac.Desc : ac.HiddenDesc;
            TextDefinition.AddHtmlText(this, 355, 34 + CARD_HEIGHT + (index * HEIGHT_PER_CARD), 613, 16, description, false, false, COLOR_LOCALIZED, COLOR_HTML);

            if (acheiveData != null && acheiveData.IsComplete)
                TextDefinition.AddHtmlText(this, 806, 12 + CARD_HEIGHT + (index * HEIGHT_PER_CARD), 185, 16, string.Format("<RIGHT>Completed {0}</RIGHT>", acheiveData.CompletedOn.ToShortDateString()), false, false, COLOR_LOCALIZED, 0x148506);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) return;

            switch (info.ButtonID)
            {
                case 0: // Close
                    return;

                case 1: // All Categories
                    sender.Mobile.SendGump(new AchievementGump(m_curAchieves, m_curTotal));
                    break;

                default:
                    var btn = info.ButtonID - 5000;
                    sender.Mobile.SendGump(new AchievementGump(m_curAchieves, m_curTotal, btn));
                    break;
            }
        }
    }
}

