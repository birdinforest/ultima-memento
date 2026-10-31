using System;
using Server;
using Server.Misc;
using Server.Network;
using System.Text;
using System.IO;
using System.Threading;
using Server.Gumps;

namespace Server.Items
{
	[Flipable(0x577B, 0x577C)]
	public class RulesBoard : Item
	{
		public override string DefaultName{ get{ return "世界贡献者"; } }

		[Constructable]
		public RulesBoard( ) : base( 0x577B )
		{
			Weight = 1.0;
			Hue = 0xB01;
		}

		public override void OnDoubleClick( Mobile e )
		{
			if ( e.InRange( this.GetWorldLocation(), 4 ) )
			{
				string rules = null;
				string path = "Info/Credits.txt";

				if ( File.Exists( path ))
				{
					StreamReader r = new StreamReader( path, System.Text.Encoding.Default, false );
					rules = r.ReadToEnd();
					r.Close();
					rules = rules.ToString();
				}
				e.CloseGump( typeof( BoardGump ) );
				e.SendGump( new BoardGump( e, "世界贡献者", "" + rules + "", "#e97f76", false ) );
			}
			else
			{
				e.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

		public RulesBoard(Serial serial) : base(serial)
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

			// Decoration and older builds stored the English law-board name on the item.
			// Constructor defaults do not rewrite names already saved in the world.
			if ( Name == "Laws of the Land" || Name == "LAWS OF THE LAND" )
				Name = null;
		}
	}
}