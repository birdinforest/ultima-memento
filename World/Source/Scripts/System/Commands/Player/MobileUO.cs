using Server.Mobiles;

namespace Server.Commands
{
    public class MobileUO
    {
        public static void Initialize()
        {
            CommandSystem.Register("SuppressTooltips", AccessLevel.Player, new CommandEventHandler(OnToggleSuppressTooltips));
        }

        [Usage("SuppressTooltips")]
        [Description("Enables or disables the vendor tooltips.")]
        private static void OnToggleSuppressTooltips(CommandEventArgs e)
        {
            var player = e.Mobile as PlayerMobile;
            if (player == null) return;

			player.SuppressVendorTooltip = !player.SuppressVendorTooltip;

            var message = player.SuppressVendorTooltip
                ? "Vendor tooltips disabled. Vendor tooltips will no longer be sent to the Client."
                : "Vendor tooltips have been enabled.";
            player.SendMessage(68, message);
        }
    }
}
