using System;
using System.Collections.Generic;
using System.Linq;
using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Targeting;
using Server.Utilities;

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
            CommandSystem.Register("Shoppe-Order-Export", AccessLevel.GameMaster, new CommandEventHandler(OnShoppesOrderExport));
            CommandSystem.Register("Shoppe-Order-Export-All", AccessLevel.GameMaster, new CommandEventHandler(OnShoppesOrderExportAll));
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

        [Usage("Shoppe-Order-Export")]
        [Description("Writes a unique list of Orders for the targeted Shoppe to the Console")]
        public static void OnShoppesOrderExport(CommandEventArgs e)
        {
            ShoppeCraftSystem.Instance.Export(e.Mobile);
        }

        [Usage("Shoppe-Order-Export-All")]
        [Description("Writes a unique list of Orders for all Shoppes to the Console")]
        public static void OnShoppesOrderExportAll(CommandEventArgs e)
        {
            var types = new HashSet<Type>();
            foreach (var shoppe in WorldUtilities.ForEachItem<Item>(item => item is IDiagnosticOrderShoppe).Cast<IDiagnosticOrderShoppe>())
            {
                if (types.Contains(shoppe.GetType())) continue;

                Console.WriteLine("Exporting Orders for {0}", shoppe.GetType().Name);
                ShoppeCraftSystem.Instance.ExportOrders(e.Mobile, shoppe);
				Console.WriteLine();
				Console.WriteLine();
                types.Add(shoppe.GetType());
            }
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