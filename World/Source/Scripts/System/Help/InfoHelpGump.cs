using System;
using Server.Network;

namespace Server.Gumps
{
	public class InfoHelpGump : Gump
    {
		private readonly Action m_OnClose;

		public InfoHelpGump( Mobile from, string title, string info, bool scrollbar = true, Action onClose = null ) : base( 50, 50 )
		{
			from.CloseGump( typeof( InfoHelpGump ) );

			m_OnClose = onClose;

            Closable=true;
			Disposable=true;
			Dragable=true;

			string color = "#ddbc4b";

			AddImage(0, 0, 9577, Server.Misc.PlayerSettings.GetGumpHue( from ));
			AddHtml( 12, 12, 239, 20, @"<BODY><BASEFONT Color=" + color + ">" + title + "</BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 12, 43, 278, 212, @"<BODY><BASEFONT Color=" + color + ">" + info + "</BASEFONT></BODY>", (bool)false, (bool)scrollbar);
			AddButton(268, 9, 4017, 4017, 0, GumpButtonType.Reply, 0);
		}

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
			from.SendSound( 0x4A );
			from.CloseGump( typeof( Server.Engines.Help.HelpGump ) );

			if ( m_OnClose != null )
				m_OnClose.Invoke();
        }
    }
}