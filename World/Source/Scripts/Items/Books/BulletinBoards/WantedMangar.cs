using System;
using Server;
using Server.Items;
using System.Text;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;
using Server.Localization;

namespace Server.Items
{
	[Flipable(0x52FE, 0x52FF)]
	public class WantedMangar : Item
	{
		public override bool IsContentLocalized => true;

		public override string DefaultName
		{
			get { return "Wanted!"; }
		}

		public override string DisplayNameLocalizationKey => "item.bulletin.wantedmangar";

		[Constructable]
		public WantedMangar( ) : base( 0x52FE )
		{
		}

		public class WantedMangarGump : Gump
		{
			private static string LocalizedHtml( Mobile from, string color, string key )
			{
				string text = StringCatalog.ResolveByKey( from?.Account, key );
				return @"<BODY><BASEFONT Color=" + color + ">" + text + "</BASEFONT></BODY>";
			}

			public WantedMangarGump( Mobile from ): base( 50, 50 )
			{
				from.SendSound( 0x59 ); 
				string color = "#a2a2cb";

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				AddImage(0, 0, 9584, Server.Misc.PlayerSettings.GetGumpHue( from ));
				AddImage(582, 204, 10906);
				AddButton(568, 9, 4017, 4017, 0, GumpButtonType.Reply, 0);
				AddItem(265, 3, 7905);
				AddItem(437, 6, 577);
				AddItem(461, 28, 577);
				AddItem(485, 51, 577);
				AddItem(462, 63, 26404);
				AddItem(509, 74, 577);
				AddItem(530, 96, 579);
				AddItem(288, 90, 5360, 0xB98);
				AddHtml( 11, 11, 243, 20, LocalizedHtml( from, color, "book.wantedmangar.gump.headline" ), (bool)false, (bool)false);
				AddHtml( 316, 17, 115, 20, LocalizedHtml( from, color, "book.wantedmangar.gump.magic_mouth" ), (bool)false, (bool)false);
				AddHtml( 496, 34, 115, 20, LocalizedHtml( from, color, "book.wantedmangar.gump.secret_doors" ), (bool)false, (bool)false);
				AddHtml( 339, 94, 115, 20, LocalizedHtml( from, color, "book.wantedmangar.gump.clues" ), (bool)false, (bool)false);
				AddHtml( 10, 169, 563, 160, LocalizedHtml( from, color, "book.wantedmangar.gump.body" ), (bool)false, (bool)false);
			}

			public override void OnResponse(NetState state, RelayInfo info)
			{
				Mobile from = state.Mobile;
				from.SendSound( 0x59 ); 
			}
		}

		public override void OnDoubleClick( Mobile e )
		{
			if ( e.InRange( this.GetWorldLocation(), 4 ) )
			{
				e.CloseGump( typeof( WantedMangarGump ) );
				e.SendGump( new WantedMangarGump( e ) );
			}
			else
			{
				e.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

		public WantedMangar(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}