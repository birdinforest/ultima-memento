using Server.Engines.Avatar;
using Server.Gumps;
using Server.Mobiles;
using Server.Misc;

namespace Server.Gumps
{
	public class AvatarRebirthReportGump : Gump
	{
		private readonly PlayerMobile m_Player;

		public AvatarRebirthReportGump( PlayerMobile player ) : base( 25, 25 )
		{
			m_Player = player;
			var ctx = player.Avatar;

			const int WIDTH = 540;
			const int HEIGHT = 480;

			AddPage( 0 );
			AddBackground( 0, 0, WIDTH, HEIGHT, 2620 );

			string title = ResearchLocalization.Key( player, "research.resonance.gump.rebirth.title", "Avatar Rebirth Report" );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 20, 20, WIDTH - 40, 35,
				string.Format( "<CENTER><BASEFONT Color=#44FF44>{0}</BASEFONT></CENTER>", title ) );

			string echo = ctx.CurrentResonanceLocation ?? "?";
			string echoType = ResearchLocalization.EchoTypeLabel( player, ctx.ResonanceLocationType );

			ResearchLocalization.AddLocalizedHtmlFormat( this, 20, 55, WIDTH - 40, 70, player,
				"research.resonance.gump.memory_echo.intro",
				"Your Memory Echo lies at <B>{0}</B>—the {1} where your path of discovery once led you. Return there and open an ancient search chest to awaken the dormant runes in your banked pack.",
				false, echo, echoType );

			ResearchLocalization.AddLocalizedHtmlFormat( this, 20, 125, WIDTH - 40, 200, player,
				"research.resonance.gump.rebirth.body",
				"- Research Bag: Dormant in your bank (awaiting resonance)<BR>" +
				"- Ancient Spellbook: Departed Soul in your bank (awaiting resonance)<BR>" +
				"- Ancient research knowledge: Preserved in spirit<BR>" +
				"- Prepared spells: {0} → {1}<BR>" +
				"- Ink: {2} → {3}<BR>" +
				"- Quills: {4} → {5}<BR>" +
				"- Blank scrolls: {6} → {7}",
				false,
				ctx.RebirthReportPrepBefore, ctx.RebirthReportPrepAfter,
				ctx.RebirthReportInkBefore, ctx.RebirthReportInkAfter,
				ctx.RebirthReportQuillsBefore, ctx.RebirthReportQuillsAfter,
				ctx.RebirthReportScrollsBefore, ctx.RebirthReportScrollsAfter );

			AddButton( WIDTH / 2 - 50, HEIGHT - 55, 4005, 4006, 1, GumpButtonType.Reply, 0 );
			ResearchLocalization.AddLocalizedHtml( this, WIDTH / 2 - 15, HEIGHT - 52, 100, 25, player,
				"research.resonance.gump.rebirth.close", "Close" );
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( m_Player == null || info.ButtonID != 1 )
				return;

			m_Player.Avatar.HasPendingRebirthReport = false;
			AvatarCoreItemMigration.ReattachCoreItems( m_Player );

			string echo = m_Player.Avatar.CurrentResonanceLocation;
			if ( !string.IsNullOrEmpty( echo ) )
			{
				ResearchLocalization.SendFormat( m_Player, "research.resonance.msg.bank_reminder",
					"Your research pack awaits resonance in the bank. Memory Echo: {0}.", echo );
			}
		}
	}
}
