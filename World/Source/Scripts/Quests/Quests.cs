using Server.Commands;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
	public class Quests
	{
        public static void Initialize()
        {
            CommandSystem.Register("quests", AccessLevel.Player, new CommandEventHandler( MyQuests_OnCommand ));
        }

		[Usage( "quests" )]
		[Description( "Opens Quest Gump." )]
        private static void MyQuests_OnCommand( CommandEventArgs e )
        {
			PlayerMobile from = e.Mobile as PlayerMobile;
            if (from == null) return;

			from.CloseGump( typeof( QuestsGump ) );
			from.SendGump( new QuestsGump( from ) );
            from.ViewQuestLog();
        }
    }
}