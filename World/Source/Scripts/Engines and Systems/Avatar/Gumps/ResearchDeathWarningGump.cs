using System;
using Server.Engines.Avatar;
using Server.Gumps;
using Server.Mobiles;
using Server.Misc;

namespace Server.Gumps
{
	public class ResearchDeathWarningGump : Gump
	{
		private readonly Action m_OnContinue;

		public ResearchDeathWarningGump( PlayerMobile player, Action onContinue ) : base( 25, 25 )
		{
			m_OnContinue = onContinue;

			const int WIDTH = 590;
			const int HEIGHT = 360;

			AddPage( 0 );
			AddBackground( 0, 0, WIDTH, HEIGHT, 2620 );

			string title = ResearchLocalization.Key( player, "research.resonance.gump.death_warning.title", "Ancient Research Will Survive" );
			ResearchLocalization.AddLocalizedHtmlRaw( this, 20, 20, WIDTH - 40, 40,
				string.Format( "<CENTER><BASEFONT Color=#FFD700>{0}</BASEFONT></CENTER>", title ) );

			ResearchLocalization.AddLocalizedHtml( this, 20, 70, WIDTH - 40, 230, player,
				"research.resonance.gump.death_warning.body",
				"You bear the Ancient Archmage's core research items.<BR><BR>" +
				"<B>Preserved (Soul):</B> all research knowledge (Cubes, learned spells).<BR>" +
				"<B>Dormant (Artifact):</B> Research Bag and Ancient Spellbook move to your bank; complete the Rite of Memory Echo Resonance before use.<BR>" +
				"<B>Reduced (Resources):</B> prepared spells ~50%; ink, scrolls, and quills ~50%." );

			AddButton( WIDTH / 2 - 60, HEIGHT - 50, 4005, 4006, 1, GumpButtonType.Reply, 0 );
			ResearchLocalization.AddLocalizedHtml( this, WIDTH / 2 - 25, HEIGHT - 47, 220, 25, player,
				"research.resonance.gump.death_warning.continue", "I Understand — Continue" );
		}

		public override void OnResponse( Server.Network.NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1 && m_OnContinue != null )
				m_OnContinue();
		}
	}
}
