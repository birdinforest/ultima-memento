using Server.Engines.Avatar;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Misc;

namespace Server.Gumps
{
	public class ResearchResonanceConfirmGump : Gump
	{
		private readonly PlayerMobile m_Player;
		private readonly ResearchBag m_Bag;

		public ResearchResonanceConfirmGump( PlayerMobile player, ResearchBag bag ) : base( 25, 25 )
		{
			m_Player = player;
			m_Bag = bag;

			const int WIDTH = 480;
			const int HEIGHT = 280;

			AddPage( 0 );
			AddBackground( 0, 0, WIDTH, HEIGHT, 2620 );

			string title = ResearchLocalization.Key( player, "research.resonance.gump.echo.title", "Rite of Memory Echo Resonance" );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 20, 20, WIDTH - 40, 35, string.Format( "<CENTER>{0}</CENTER>", title ) );

			ResearchLocalization.AddLocalizedHtml( this, 20, 60, WIDTH - 40, 120, player,
				"research.resonance.gump.echo.body",
				"The runes in your pack stir at this place of memory. Touch the chest with your soul to complete the Rite." );

			AddButton( WIDTH - 220, HEIGHT - 50, 4005, 4006, 1, GumpButtonType.Reply, 0 );
			ResearchLocalization.AddLocalizedHtml( this, WIDTH - 185, HEIGHT - 47, 160, 25, player,
				"research.resonance.gump.echo.button", "Resonate" );
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( info.ButtonID != 1 || m_Player == null || m_Bag == null )
				return;

			AvatarCoreItemMigration.CompleteResonance( m_Player, m_Bag, ResonancePath.Search );
		}
	}
}
