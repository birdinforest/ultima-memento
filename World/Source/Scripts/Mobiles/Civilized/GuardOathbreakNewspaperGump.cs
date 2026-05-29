using System;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Localization;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server.Gumps
{
	/// <summary>
	/// Scrollable "newspaper" gump for the Guard Oathbreak short story (Library + login until opted out).
	/// </summary>
	public class GuardOathbreakNewspaperGump : Gump
	{
		private const int PanelWidth = 800;
		private const int PanelHeight = 600;

		private const int ResizePicNewspaper = 3500;
		private const int Border = 20;

		private const int HeaderBandHeight = 80;
		private const int FooterBandHeight = 44;

		private const int ContentPad = 8;
		private const int OrnamentLeft = 0x39;
		private const int OrnamentRight = 0x3B;
		private const int RuleGump = 2624;

		private const string TextColor = "#000000";

		private readonly int m_Origin;

		private static bool s_Configured;
		private static readonly object s_ConfigLock = new object();

		public static void Configure()
		{
			EnsureConfigured();
		}

		public static void EnsureConfigured()
		{
			lock ( s_ConfigLock )
			{
				if ( s_Configured )
					return;

				EventSink.Login += OnLogin;
				CommandSystem.Register( "oathnewspaper", AccessLevel.Player, new CommandEventHandler( OnCommand ) );
				s_Configured = true;
			}
		}

		private static void OnCommand( CommandEventArgs e )
		{
			if ( e.Mobile is PlayerMobile pm )
				SendGump( pm, 0 );
		}

		private static void OnLogin( LoginEventArgs e )
		{
			if ( !(e.Mobile is PlayerMobile pm) )
				return;

			if ( pm.Preferences.HideOathbreakNewspaperAtLogin )
				return;

			if ( pm.Region is StartRegion )
				return;

			Timer.DelayCall( TimeSpan.FromSeconds( 2.5 ), () => TryShowAtLogin( pm ) );
		}

		private static void TryShowAtLogin( PlayerMobile pm )
		{
			if ( pm == null || pm.Deleted || pm.NetState == null )
				return;

			if ( pm.Preferences.HideOathbreakNewspaperAtLogin )
				return;

			if ( pm.Region is StartRegion )
				return;

			SendGump( pm, 0 );
		}

		public static void SendGump( Mobile mob, int origin )
		{
			EnsureConfigured();
			mob.CloseGump( typeof( GuardOathbreakNewspaperGump ) );
			mob.SendSound( 0x55 );
			mob.SendGump( new GuardOathbreakNewspaperGump( mob, origin ) );
		}

		public GuardOathbreakNewspaperGump( Mobile from, int origin ) : base( 50, 50 )
		{
			EnsureConfigured();
			m_Origin = origin;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			PlayerMobile pm = from as PlayerMobile;
			int checkButton = 4018;
			if ( pm != null && pm.Preferences.HideOathbreakNewspaperAtLogin )
				checkButton = 3609;

			AddPage( 0 );

			AddBackground( 0, 0, PanelWidth, PanelHeight, ResizePicNewspaper );

			int contentX = Border + ContentPad;
			int contentWidth = PanelWidth - ( Border * 2 ) - ( ContentPad * 2 );

			int bodyTop = Border + HeaderBandHeight;
			int bodyBottom = PanelHeight - Border - FooterBandHeight;
			int contentHeight = bodyBottom - bodyTop;

			int headerBandTop = Border;
			int headerBandBottom = bodyTop;
			int headerMidY = ( headerBandTop + headerBandBottom ) / 2;

			int footerBandTop = bodyBottom;
			int footerBandBottom = PanelHeight - Border;
			int footerMidY = ( footerBandTop + footerBandBottom ) / 2;

			AddNewspaperChrome( contentX, contentWidth, headerBandTop, headerBandBottom, footerBandTop, footerBandBottom );

			string masthead = ResolveKey( from, "guard.oathbreak.newspaper.masthead", "Montor Evening Gazette" );
			string title = ResolveKey( from, "guard.oathbreak.newspaper.title", "The Oath-Eaten Blade" );
			string edition = ResolveKey( from, "guard.oathbreak.newspaper.edition", "Evening Edition" );

			int mastheadY = headerMidY - 26;
			int titleY = headerMidY - 2;
			int editionY = headerMidY + 18;

			AddHtml( contentX, mastheadY, contentWidth, 22,
				@"<BODY><BASEFONT Color=" + TextColor + @"><CENTER><BIG>" + masthead + @"</BIG></CENTER></BASEFONT></BODY>",
				false, false );

			AddHtml( contentX, titleY, contentWidth, 22,
				@"<BODY><BASEFONT Color=" + TextColor + @"><CENTER><B>" + title + @"</B></CENTER></BASEFONT></BODY>",
				false, false );

			AddHtml( contentX, editionY, contentWidth, 18,
				@"<BODY><BASEFONT Color=" + TextColor + @"><CENTER><I>" + edition + @"</I></CENTER></BASEFONT></BODY>",
				false, false );

			AddButton( PanelWidth - Border - 22, headerBandTop + 6, 4017, 4017, 0, GumpButtonType.Reply, 0 );

			AddHtml( contentX, bodyTop, contentWidth, contentHeight, BuildArticleHtml( from ), false, true );

			int checkY = footerMidY - 10;
			AddButton( contentX, checkY, checkButton, checkButton, 1, GumpButtonType.Reply, 0 );
			AddHtml( contentX + 36, checkY + 2, 320, 20,
				@"<BODY><BASEFONT Color=" + TextColor + ">" + ResolveKey( from, "guard.oathbreak.newspaper.hide_at_login", "Do not show at login" ) + "</BASEFONT></BODY>",
				false, false );
		}

		private void AddNewspaperChrome( int contentX, int contentWidth, int headerTop, int headerBottom, int footerTop, int footerBottom )
		{
			int centerX = contentX + ( contentWidth / 2 );

			AddImageTiled( contentX, headerTop + 6, contentWidth, 3, RuleGump );
			AddImageTiled( contentX, headerBottom - 8, contentWidth, 3, RuleGump );

			AddImage( centerX - 120, headerTop + 14, OrnamentLeft );
			AddImage( centerX + 92, headerTop + 14, OrnamentRight );

			AddImageTiled( contentX, footerTop + 6, contentWidth, 3, RuleGump );
			AddImageTiled( contentX, footerBottom - 10, contentWidth, 3, RuleGump );

			AddImage( centerX - 90, footerTop + 10, OrnamentLeft );
			AddImage( centerX + 62, footerTop + 10, OrnamentRight );
		}

		private static string ResolveKey( Mobile from, string key, string fallback )
		{
			string lang = AccountLang.GetLanguageCode( from?.Account );
			return StringCatalog.TryResolveByKey( lang, key ) ?? fallback;
		}

		private static string BuildArticleHtml( Mobile from )
		{
			string before = ResolveKey( from, "guard.oathbreak.newspaper.body.before", "" );
			string after = ResolveKey( from, "guard.oathbreak.newspaper.body.after", "" );

			return @"<BODY><BASEFONT Color=" + TextColor + ">" + before + @"<BR><BR>" + after + @"</BASEFONT></BODY>";
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;
			PlayerMobile pm = from as PlayerMobile;

			if ( info.ButtonID == 1 && pm != null )
			{
				pm.Preferences.HideOathbreakNewspaperAtLogin = !pm.Preferences.HideOathbreakNewspaperAtLogin;
				SendGump( from, m_Origin );
				from.SendSound( 0x4A );
				return;
			}

			from.SendSound( 0x4A );

			if ( m_Origin > 0 )
				from.SendGump( new Engines.Help.HelpGump( from, 1 ) );
		}
	}
}
