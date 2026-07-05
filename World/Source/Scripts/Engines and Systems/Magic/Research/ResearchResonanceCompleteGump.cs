using System;
using Server.Engines.Avatar;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Misc;

namespace Server.Gumps
{
	public class ResearchResonanceCompleteGump : Gump
	{
		private readonly PlayerMobile m_Player;
		private readonly ResearchBag m_Bag;

		public ResearchResonanceCompleteGump( PlayerMobile player, ResearchBag bag ) : base( 25, 25 )
		{
			m_Player = player;
			m_Bag = bag;

			const int WIDTH = 460;
			const int HEIGHT = 260;

			AddPage( 0 );
			AddBackground( 0, 0, WIDTH, HEIGHT, 2620 );

			string title = ResearchLocalization.Key( player, "research.resonance.gump.complete.title", "The Runes Answer" );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 20, 20, WIDTH - 40, 35,
				string.Format( "<CENTER><BASEFONT Color=#FFD700>{0}</BASEFONT></CENTER>", title ) );

			ResearchLocalization.AddLocalizedHtml( this, 20, 60, WIDTH - 40, 100, player,
				"research.resonance.gump.complete.body",
				"The runes answer as one; ancient knowledge opens anew." );

			int cubes = AvatarCoreItemMigration.CountCubesFound( bag );
			string stats = string.Format( "Cubes of Power: {0}/26", cubes );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 20, 150, WIDTH - 40, 40, stats );

			AddButton( WIDTH - 220, HEIGHT - 50, 4005, 4006, 1, GumpButtonType.Reply, 0 );
			ResearchLocalization.AddLocalizedHtml( this, WIDTH - 185, HEIGHT - 47, 180, 25, player,
				"research.resonance.gump.complete.open", "Open Research Pack" );

			Timer.DelayCall( TimeSpan.FromSeconds( 3 ), () =>
			{
				if ( m_Player != null && m_Bag != null && !m_Bag.IsDormant && m_Bag.IsChildOf( m_Player.Backpack ) )
					OpenResearchGump();
			} );
		}

		private void OpenResearchGump()
		{
			if ( m_Player == null || m_Bag == null || m_Bag.IsDormant )
				return;

			m_Player.CloseGump( typeof( ResearchBag.ResearchGump ) );
			m_Bag.BagPage = 1;
			m_Player.SendGump( new ResearchBag.ResearchGump( m_Bag, m_Player ) );
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1 )
				OpenResearchGump();
		}
	}
}
