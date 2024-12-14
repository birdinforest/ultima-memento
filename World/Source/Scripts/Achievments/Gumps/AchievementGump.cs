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
                    .Take(5);

            var achievementList = achievements.ToList();
            for (int index = 0; index < achievementList.Count; index++)
            {
                var ac = achievementList[index];
                if (achieves.ContainsKey(ac.ID))
                {
                    AddAchieve(ac, index, achieves[ac.ID]);
                }
                else
                {
                    AddAchieve(ac, index, null);
                }
            }
        }

        private void AddAchieve(BaseAchievement ac, int i, AchieveData acheiveData)
        {
            int index = i % 5;
            if (index == 0)
            {
                AddButton(974, 549, 4005, 4006, 0, GumpButtonType.Page, (i / 5) + 1); // Next
                AddPage((i / 5) + 1);
                AddLabel(615, 551, COLOR_LABEL, "Page " + ((i / 5) + 1));
                AddButton(282, 549, 4014, 4015, 0, GumpButtonType.Page, i / 5); // Prev
            }

            AddBackground(277, 68 + (index * 100), 727, 97, 3600);
            AddLabel(350, 83 + (index * 100), 49, ac.Title);
            if (ac.ItemIcon > 0)
                AddItem(294, 93 + (index * 100), ac.ItemIcon);

            if (1 < ac.CompletionTotal)
            {
                AddImageTiled(353, 136 + (index * 100), 95, 9, 9750); // Gray progress

                var progress = 0;
                if (acheiveData != null)
                {
                    progress = acheiveData.Progress;
                    var step = 95.0 / ac.CompletionTotal;

                    if (0 < progress)
                        AddImageTiled(353, 136 + (index * 100), (int)(progress * step), 9, 9752); // Green progress
                }

                AddLabel(459, 131 + (index * 100), COLOR_LABEL, progress + @" / " + ac.CompletionTotal);
            }

            TextDefinition.AddHtmlText(this, 355, 102 + (index * 100), 613, 16, ac.Desc, false, false, COLOR_LOCALIZED, COLOR_HTML);
            if (acheiveData != null && acheiveData.IsComplete)
                AddLabel(911, 83 + (index * 100), 61, acheiveData.CompletedOn.ToShortDateString());

            var progressColor = acheiveData != null && acheiveData.IsComplete ? 61 : 32;
            AddLabel(311, 131 + (index * 100), progressColor, ac.RewardPoints.ToString());

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

