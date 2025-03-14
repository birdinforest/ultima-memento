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
            CommandSystem.Register("shoppecontext", AccessLevel.GameMaster, new CommandEventHandler(args =>
            {
                if (args.Mobile == null) return;

                args.Mobile.Target = new InternalTarget();
            }));
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

                var context = ShoppeEngine.Instance.GetOrCreateContext(from);
                from.SendGump(new PropertiesGump(from, context));
            }
        }
    }
}