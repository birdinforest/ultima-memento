using System;
using Server;
using Server.Gumps;
using Server.Localization;
using Server.Misc;
using Server.Network;

namespace Server.Gumps
{
	public class PrisonGump : Gump
	{
		public PrisonGump( Mobile from ) : base( 50, 50 )
		{
			from.SendSound( 0x4A );
			string color = "#e98650";

			this.Closable = true;
			this.Disposable = false;
			this.Dragable = true;
			this.Resizable = false;

			AddPage( 0 );

			AddImage( 0, 0, 7021, PlayerSettings.GetGumpHue( from ) );
			AddHtml( 13, 13, 415, 20,
				@"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey( from.Account, "prison.gump.title" ) + "</BASEFONT></BODY>",
				false, false );
			AddButton( 466, 10, 4017, 4017, 0, GumpButtonType.Reply, 0 );
			AddHtml( 16, 46, 475, 246,
				@"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveByKey( from.Account, "prison.gump.body" ) + "</BASEFONT></BODY>",
				false, false );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;
			from.SendSound( 0x4A );
		}
	}
}
