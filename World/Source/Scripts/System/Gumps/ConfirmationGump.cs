using System;

namespace Server.Gumps
{
    public class ConfirmationGump : Gump
    {
        private readonly Action _onConfirmed;
        private readonly Action _onDeclined;

        /// <summary>
        /// Immediately executes `onConfirmed` if `shouldSend` is `true`.
        /// Otherwise, sends a confirmation gump.
        /// </summary>
        public static void PromptIfFalse(Mobile from, bool shouldSend, Action onConfirmed, Func<Action, ConfirmationGump> gumpFactory)
        {
            if (shouldSend)
            {
                ConfirmationGump gump = gumpFactory(onConfirmed);
                from.SendGump(gump);
            }
            else
            {
                onConfirmed();
            }
        }

        public ConfirmationGump(Mobile player, string message, Action onConfirmed = null, Action onDeclined = null)
            : this(player, null, message, onConfirmed, onDeclined)
        {
        }

        public ConfirmationGump(Mobile player, string title, string message, Action onConfirmed = null, Action onDeclined = null) : base(25, 25)
        {
            player.CloseGump(typeof(ConfirmationGump));

            _onConfirmed = onConfirmed;
            _onDeclined = onDeclined;

            AddPage(0);
            AddImage(0, 0, 10901);

            if (!string.IsNullOrWhiteSpace(title))
                AddHtml(80, 22, 320, 100, @"<BODY><BASEFONT><CENTER>" + title + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);

            AddHtml(67, 97, 352, 170, @"<BODY><BASEFONT COLOR=#000000><BIG>" + message + "</BIG></BASEFONT></BODY>", (bool)false, 255 < message.Length);

            int buttonY = 280;
            AddButton(100, buttonY, 4005, 4005, 1, GumpButtonType.Reply, 0);
            AddHtml(137, buttonY, 58, 20, @"<BODY><BASEFONT><BIG>Yes</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
            AddButton(308, buttonY, 4020, 4020, 2, GumpButtonType.Reply, 0);
            AddHtml(346, buttonY, 58, 20, @"<BODY><BASEFONT><BIG>No</BIG></BASEFONT></BODY>", (bool)false, (bool)false);
        }

        public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                if (_onConfirmed != null)
                    _onConfirmed();
            }
            else
            {
                if (_onDeclined != null)
                    _onDeclined();
            }
        }
    }
}