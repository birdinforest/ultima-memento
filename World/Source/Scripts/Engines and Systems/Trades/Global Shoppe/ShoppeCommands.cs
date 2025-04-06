using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Targeting;

namespace Server.Engines.GlobalShoppe
{
    public class ShoppeCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("Shoppe-GetContext", AccessLevel.GameMaster, new CommandEventHandler(args =>
            {
                if (args.Mobile == null) return;

                args.Mobile.Target = new InternalTarget();
            }));
            CommandSystem.Register("Shoppe-Disable", AccessLevel.GameMaster, new CommandEventHandler(OnShoppesDisable));
            CommandSystem.Register("Shoppe-Enable", AccessLevel.GameMaster, new CommandEventHandler(OnShoppesEnable));
            CommandSystem.Register("Shoppe-Status", AccessLevel.GameMaster, new CommandEventHandler(OnShoppesStatus));
        }

        [Usage("Shoppe-Disable")]
        [Description("Disables the Shoppe system until server restart.")]
        public static void OnShoppesDisable(CommandEventArgs e)
        {
            ShoppeEngine.Instance.IsEnabled = false;
            e.Mobile.SendMessage("Shoppes have been disabled");
        }

        [Usage("Shoppe-Enable")]
        [Description("Enables the Shoppe system")]
        public static void OnShoppesEnable(CommandEventArgs e)
        {
            ShoppeEngine.Instance.IsEnabled = true;
            e.Mobile.SendMessage("Shoppes have been enabled.");
        }

        [Usage("Shoppe-Status")]
        [Description("Gets the Enabled status the Shoppe system")]
        public static void OnShoppesStatus(CommandEventArgs e)
        {
            var message = ShoppeEngine.Instance.IsEnabled
                ? "Shoppes are currently Enabled."
                : "Shoppes are currently Disabled.";
            e.Mobile.SendMessage(message);
        }

        private class InternalTarget : Target
        {
            public InternalTarget() : base(-1, true, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (!BaseCommand.IsAccessible(from, o))
                {
                    from.SendMessage("That is not accessible.");
                    return;
                }

                var mobile = o as Mobile;
                if (mobile == null) return;

                var context = ShoppeEngine.Instance.GetOrCreateContext(mobile);
                from.SendGump(new PropertiesGump(from, context));
            }
        }
    }
}