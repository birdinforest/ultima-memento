using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Commands;
using Server.Localization;

namespace Server.Gumps
{
    public class FameKarma : Gump
    {
		public int m_Origin;

		private static string ResolveText( Mobile from, string text )
		{
			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

        public FameKarma( Mobile from, int origin ) : base( 50, 50 )
        {
			m_Origin = origin;
			string color = "#e87373";
			from.SendSound( 0x4A ); 

			this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);

			AddImage(0, 0, 9578, Server.Misc.PlayerSettings.GetGumpHue( from ));
			AddButton(859, 9, 4017, 4017, 0, GumpButtonType.Reply, 0);
			AddHtml( 12, 12, 576, 20, @"<BODY><BASEFONT Color=" + color + ">" + ResolveText( from, "FAME AND KARMA" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);

			AddHtml( 20, 80, 137, 20, @"<BODY><BASEFONT Color=" + color + ">" + ResolveText( from, "KARMA" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 164, 50, 720, 20, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + ResolveText( from, "FAME" ) + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);

			string col1 = "<BR><BR>";
			col1 = col1 + ResolveText( from, "10,000 & up" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "9,999 to 5,000" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "4,999 to 2,500" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "2,499 to 1,250" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "1,249 to 625" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "624 to -624" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "-625 to -1,249" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "-1,250 to -2,499" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "-2,500 to -4,999" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "-5,000 to -9,999" ) + "<BR><BR>";
			col1 = col1 + ResolveText( from, "-10,000 & lower" ) + "<BR><BR>";

			string col2 = "";
			col2 = col2 + ResolveText( from, "0 to 1,249" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Trustworthy" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Honest" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Good" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Kind" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Fair" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "No Title" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Rude" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Unsavory" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Scoundrel" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Despicable" ) + "<BR><BR>";
			col2 = col2 + ResolveText( from, "Outcast" ) + "<BR><BR>";

			string col3 = "";
			col3 = col3 + ResolveText( from, "1,250 to 2,499" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Estimable" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Commendable" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Honorable" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Respectable" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Upstanding" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Notable" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Disreputable" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Dishonorable" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Malicious" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Dastardly" ) + "<BR><BR>";
			col3 = col3 + ResolveText( from, "Wretched" ) + "<BR><BR>";

			string col4 = "";
			col4 = col4 + ResolveText( from, "2500 to 4,999" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Great" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Famed" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Admirable" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Proper" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Reputable" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Prominent" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Notorious" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Ignoble" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Vile" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Wicked" ) + "<BR><BR>";
			col4 = col4 + ResolveText( from, "Nefarious" ) + "<BR><BR>";

			string col5 = "";
			col5 = col5 + ResolveText( from, "5,000 to 9,999" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Glorious" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Illustrious" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Noble" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Eminent" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Distinguished" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Renowned" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Infamous" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Sinister" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Villainous" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Evil" ) + "<BR><BR>";
			col5 = col5 + ResolveText( from, "Dread" ) + "<BR><BR>";

			string col6 = "";
			col6 = col6 + ResolveText( from, "10,000 & up" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Glorious Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Illustrious Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Noble Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Eminent Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Distinguished Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Dishonored Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Sinister Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Dark Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Evil Lord" ) + "<BR><BR>";
			col6 = col6 + ResolveText( from, "Dread Lord" ) + "<BR><BR>";

			AddHtml( 20, 80, 144, 495, @"<BODY><BASEFONT Color=" + color + ">" + col1 + "</BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 164, 80, 144, 495, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + col2 + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 308, 80, 144, 495, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + col3 + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 452, 80, 144, 495, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + col4 + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 596, 80, 144, 495, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + col5 + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);
			AddHtml( 740, 80, 144, 495, @"<BODY><BASEFONT Color=" + color + "><CENTER>" + col6 + "</CENTER></BASEFONT></BODY>", (bool)false, (bool)false);
        }

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;
			from.SendSound( 0x4A ); 
			if ( m_Origin > 0 ){ from.SendGump( new Server.Engines.Help.HelpGump( from, 1 ) ); }
		}
    }
}