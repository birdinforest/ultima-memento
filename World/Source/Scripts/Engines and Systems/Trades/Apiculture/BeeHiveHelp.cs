using System;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Multis;
using Server.Targeting;
using Server.Localization;

namespace Server.Engines.Apiculture
{	
	public class apiBeeHiveHelpGump : Gump
	{
		private readonly Mobile m_From;

		public apiBeeHiveHelpGump( Mobile from, int type ) : base( 20, 20 )
		{
			m_From = from;

			Closable=true;
			Disposable=true;
			Dragable=true;
			Resizable=false;

			AddPage(0);
			AddBackground(37, 25, 386, 353, 3600);
			AddLabel(177, 42, 92, StringCatalog.ResolveByKey( from.Account, "apiculture.help.title" ) );

			AddItem(32, 277, 3311);
			AddItem(30, 193, 3311);
			AddItem(29, 107, 3311);
			AddItem(28, 24, 3311);
			AddItem(386, 277, 3307);
			AddItem(387, 191, 3307);
			AddItem(388, 108, 3307);
			AddItem(385, 26, 3307);

			AddHtml( 59, 67, 342, 257, HelpText( from, type ), true, true);
			AddButton(202, 333, 247, 248, 0, GumpButtonType.Reply, 0);
		}

		public static string HelpText( Mobile from, int type )
		{
			if ( type == 1 )
				return StringCatalog.ResolveByKey( from.Account, "apiculture.help.page1" );

			return StringCatalog.ResolveByKey( from.Account, "apiculture.help.page0" );
		}
	}
}
