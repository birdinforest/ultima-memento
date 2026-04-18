using System;
using Server;
using Server.Gumps;
using Server.Localization;

namespace Knives.TownHouses
{
	public class InfoGump : GumpPlusLight
	{
		private int c_Width, c_Height;
		private string c_Text;
		private bool c_Scroll;

		private static string ResolveText( Mobile m, string text )
		{
			string lang = AccountLang.GetLanguageCode( m.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		public InfoGump( Mobile m, int width, int height, string text, bool scroll ) : base( m, 100, 100 )
		{
			c_Width = width;
			c_Height = height;
			c_Text = ResolveText( m, text );
			c_Scroll = scroll;

			NewGump();
		}

		protected override void BuildGump()
		{
			AddBackground( 0, 0, c_Width, c_Height, 0x1453 );

			AddHtml( 20, 20, c_Width-40, c_Height-40, HTML.White + c_Text, false, c_Scroll );
		}
	}
}