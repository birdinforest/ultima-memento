using Server.Engines.Avatar;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Misc;

namespace Server.Gumps
{
	public class ResearchBagDormantSummaryGump : Gump
	{
		private readonly ResearchBag m_Bag;
		private readonly PlayerMobile m_Player;

		public ResearchBagDormantSummaryGump( ResearchBag bag, PlayerMobile player ) : base( 25, 25 )
		{
			m_Bag = bag;
			m_Player = player;
			var ctx = player.Avatar;

			const int WIDTH = 520;
			const int HEIGHT = 440;

			AddPage( 0 );
			AddBackground( 0, 0, WIDTH, HEIGHT, 2620 );

			string title = ResearchLocalization.Key( player, "research.resonance.gump.dormant.title", "Dormant Research Pack" );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 20, 20, WIDTH - 40, 35, string.Format( "<CENTER>{0}</CENTER>", title ) );

			int cubes = AvatarCoreItemMigration.CountCubesFound( bag );
			string echo = ctx != null ? ctx.CurrentResonanceLocation : "?";
			string echoType = ResearchLocalization.EchoTypeLabel( player, ctx != null ? ctx.ResonanceLocationType : "research" );

			ResearchLocalization.AddLocalizedHtmlFormat( this, 20, 55, WIDTH - 40, 55, player,
				"research.resonance.gump.dormant.lead",
				"The pack slumbers. Your research knowledge is preserved in spirit, but the runes have not yet answered.<BR><BR>Cubes of Power: {0}/26",
				false, cubes );

			ResearchLocalization.AddLocalizedHtmlFormat( this, 20, 115, WIDTH - 40, 75, player,
				"research.resonance.gump.memory_echo.intro",
				"Your Memory Echo lies at <B>{0}</B>—the {1} where your path of discovery once led you. Return there and open an ancient search chest to awaken the dormant runes in your banked pack.",
				false, echo, echoType );

			ResearchLocalization.AddLocalizedHtml( this, 20, 195, WIDTH - 40, 175, player,
				"research.resonance.gump.memory_echo.rite",
				"The runes on your pack still slumber. Your research lives on in spirit—but the artifact will not wake until you complete the <B>Rite of Memory Echo Resonance</B>.<BR><BR>" +
				"Return to the place inscribed in your memory: among the ruins and pedestals where archmages once hid their secrets, stand before an <B>ancient search chest</B>. When your soul touches the lock, the runes stir and the pack binds to you anew.<BR><BR>" +
				"Those who cannot journey may seek the <B>Lost Arts Registrar</B> for a material offering—but the echo itself remains the true gate." );

			AddButton( WIDTH - 160, HEIGHT - 50, 4005, 4006, 1, GumpButtonType.Reply, 0 );
			ResearchLocalization.AddLocalizedHtml( this, WIDTH - 125, HEIGHT - 47, 120, 25, player,
				"research.resonance.gump.dormant.close", "Close" );
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( m_Player == null || m_Bag == null )
				return;

			if ( !string.IsNullOrEmpty( m_Player.Avatar.CurrentResonanceLocation ) )
			{
				ResearchLocalization.SendFormat( m_Player, "research.resonance.msg.echo_hint",
					"Memory Echo: {0}. Open a search chest there to resonate.",
					m_Player.Avatar.CurrentResonanceLocation );
			}
		}
	}
}
