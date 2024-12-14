using Server;
using Server.Gumps;

namespace Scripts.Mythik.Systems.Achievements.Gumps
{
    class AchievementObtainedGump : Gump
    {
        public AchievementObtainedGump(BaseAchievement ach) : base(25, 25)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;
            AddPage(0);
            AddBackground(0, 13, 350, 100, 3600);
            TextDefinition.AddHtmlText(this, 0, 0, 350, 16, "<CENTER>ACHIEVEMENT COMPLETED</CENTER>", false, false, AchievementGump.COLOR_LOCALIZED, AchievementGump.COLOR_HTML);

            Rectangle2D bounds = ItemBounds.Table[ach.ItemIcon];
            int y = 36;
            if (ach.ItemIcon > 0)
                AddItem(41 - bounds.Width / 2 - bounds.X, (30 - bounds.Height / 2 - bounds.Y) + y, ach.ItemIcon);

            AddLabel(82, 31, 49, ach.Title);
            TextDefinition.AddHtmlText(this, 81, 56, 250, 41, ach.Desc, false, false, AchievementGump.COLOR_LOCALIZED, AchievementGump.COLOR_HTML);
        }
    }
}
