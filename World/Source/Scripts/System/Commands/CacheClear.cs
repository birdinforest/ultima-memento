using Server.Network;

namespace Server.Commands
{
    public class CacheClear
    {
        public static void Initialize()
        {
            CommandSystem.Register("CacheClear", AccessLevel.Administrator, new CommandEventHandler(ClearCache_All_Command));
            CommandSystem.Register("CacheClear-ChangeUpdateRange", AccessLevel.Administrator, new CommandEventHandler(ClearCache_ChangeUpdateRange_Command));
            CommandSystem.Register("CacheClear-GlobalLightLevel", AccessLevel.Administrator, new CommandEventHandler(ClearCache_GlobalLightLevel_Command));
            CommandSystem.Register("CacheClear-SeasonChange", AccessLevel.Administrator, new CommandEventHandler(ClearCache_SeasonChange_Command));
            CommandSystem.Register("CacheClear-PingACk", AccessLevel.Administrator, new CommandEventHandler(ClearCache_PingAck_Command));
            CommandSystem.Register("CacheClear-MovementAck", AccessLevel.Administrator, new CommandEventHandler(ClearCache_MovementAck_Command));
        }

        [Usage("CacheClear")]
        [Description("Clears all caches.")]
        private static void ClearCache_All_Command(CommandEventArgs e)
        {
            ChangeUpdateRange.Reset();
            GlobalLightLevel.Reset();
            SeasonChange.Reset();
            PingAck.Reset();
            MovementAck.Reset();

            e.Mobile.SendMessage("All caches cleared.");
        }

        [Usage("CacheClear-ChangeUpdateRange")]
        [Description("Clears the 'ChangeUpdateRange' cache.")]
        private static void ClearCache_ChangeUpdateRange_Command(CommandEventArgs e)
        {
            ChangeUpdateRange.Reset();
            e.Mobile.SendMessage("Cache cleared.");
        }

        [Usage("CacheClear-GlobalLightLevel")]
        [Description("Clears the 'GlobalLightLevel' cache.")]
        private static void ClearCache_GlobalLightLevel_Command(CommandEventArgs e)
        {
            GlobalLightLevel.Reset();
            e.Mobile.SendMessage("Cache cleared.");
        }

        [Usage("CacheClear-SeasonChange")]
        [Description("Clears the 'SeasonChange' cache.")]
        private static void ClearCache_SeasonChange_Command(CommandEventArgs e)
        {
            SeasonChange.Reset();
            e.Mobile.SendMessage("Cache cleared.");
        }

        [Usage("CacheClear-PingAck")]
        [Description("Clears the 'PingAck' cache.")]
        private static void ClearCache_PingAck_Command(CommandEventArgs e)
        {
            PingAck.Reset();
            e.Mobile.SendMessage("Cache cleared.");
        }

        [Usage("CacheClear-MovementAck")]
        [Description("Clears the 'MovementAck' cache.")]
        private static void ClearCache_MovementAck_Command(CommandEventArgs e)
        {
            MovementAck.Reset();
            e.Mobile.SendMessage("Cache cleared.");
        }
    }
}
