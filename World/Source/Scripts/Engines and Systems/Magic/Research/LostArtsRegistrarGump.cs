using Server.Engines.Avatar;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Misc;

namespace Server.Gumps
{
	public class LostArtsRegistrarGump : Gump
	{
		private readonly PlayerMobile m_Player;
		private readonly Mobile m_Registrar;
		private readonly int m_Tab;

		public LostArtsRegistrarGump( PlayerMobile player, Mobile registrar, int tab ) : base( 50, 50 )
		{
			m_Player = player;
			m_Registrar = registrar;
			m_Tab = tab;

			const int WIDTH = 500;
			const int HEIGHT = 400;

			AddPage( 0 );
			AddBackground( 0, 0, WIDTH, HEIGHT, 2620 );

			ResearchLocalization.AddLocalizedHtml( this, 20, 20, WIDTH - 40, 35, player,
				"research.resonance.gump.registrar.title", "Lost Arts Registrar" );

			var ctx = player.Avatar;
			var bag = ctx.GetResearchBag() ?? AvatarCoreItemMigration.FindResearchBag( player );

			if ( tab == 0 )
			{
				if ( bag != null && bag.IsDormant )
				{
					ResearchLocalization.AddLocalizedHtmlFormat( this, 20, 60, WIDTH - 40, 200, player,
						"research.resonance.gump.registrar.status_dormant",
						"Research pack: Dormant. Memory Echo: {0}.", false,
						ctx.CurrentResonanceLocation ?? "?" );
				}
				else
				{
					ResearchLocalization.AddLocalizedHtml( this, 20, 60, WIDTH - 40, 200, player,
						"research.resonance.gump.registrar.status_active", "No dormant research pack found." );
				}
			}
			else if ( tab == 1 )
			{
				ResearchLocalization.AddLocalizedHtml( this, 20, 60, WIDTH - 40, 180, player,
					"research.resonance.gump.registrar.material_body",
					"Offer gold and ancient materials to resonate without traveling to the Memory Echo." );

				if ( bag != null && bag.IsDormant )
				{
					AddButton( WIDTH - 220, HEIGHT - 80, 4005, 4006, 10, GumpButtonType.Reply, 0 );
					ResearchLocalization.AddLocalizedHtml( this, WIDTH - 185, HEIGHT - 77, 160, 25, player,
						"research.resonance.gump.registrar.resonate", "Material Resonance" );
				}
			}
			else if ( tab == 2 )
			{
				ResearchLocalization.AddLocalizedHtml( this, 20, 60, WIDTH - 40, 180, player,
					"research.resonance.gump.registrar.rebuild_body",
					"Rebuild a lost research pack from your soul record (snapshot)." );

				if ( bag == null )
				{
					AddButton( WIDTH - 220, HEIGHT - 80, 4005, 4006, 20, GumpButtonType.Reply, 0 );
					ResearchLocalization.AddLocalizedHtml( this, WIDTH - 185, HEIGHT - 77, 160, 25, player,
						"research.resonance.gump.registrar.rebuild", "Rebuild Pack" );
				}
			}

			AddButton( 30, HEIGHT - 45, 4005, 4006, 1, GumpButtonType.Reply, 0 );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 65, HEIGHT - 42, 80, 25, "Status" );
			AddButton( 130, HEIGHT - 45, 4005, 4006, 2, GumpButtonType.Reply, 0 );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 165, HEIGHT - 42, 120, 25, "Material" );
			AddButton( 280, HEIGHT - 45, 4005, 4006, 3, GumpButtonType.Reply, 0 );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 315, HEIGHT - 42, 80, 25, "Rebuild" );
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( m_Player == null )
				return;

			if ( info.ButtonID >= 1 && info.ButtonID <= 3 )
			{
				m_Player.SendGump( new LostArtsRegistrarGump( m_Player, m_Registrar, info.ButtonID - 1 ) );
				return;
			}

			var ctx = m_Player.Avatar;
			var bag = ctx.GetResearchBag() ?? AvatarCoreItemMigration.FindResearchBag( m_Player );

			if ( info.ButtonID == 10 && bag != null && bag.IsDormant )
			{
				AvatarCoreItemMigration.CompleteResonance( m_Player, bag, ResonancePath.Registrar );
				return;
			}

			if ( info.ButtonID == 20 && bag == null )
			{
				bag = AvatarCoreItemMigration.RebuildResearchBagFromSnapshot( m_Player, ctx );
				if ( bag != null )
					ResearchLocalization.Send( m_Player, "research.resonance.msg.registrar_rebuilt", "A dormant research pack has been rebuilt from your soul record." );
			}
		}
	}
}
