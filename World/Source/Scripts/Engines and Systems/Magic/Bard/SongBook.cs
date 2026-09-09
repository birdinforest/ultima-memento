using Server.Gumps;
using Server.Spells.Song;

namespace Server.Items
{
	[FlipableAttribute( 0x671B, 0x671C )]
	public class SongBook : Spellbook
	{
		public override string DefaultDescription{ get{ return "This book is used by bards to write the mystical songs they find. The songs within the book can be used to produce varying magical effects. These songs require the use of a musical instrument. Dropping such scrolls onto this book will place the song within its pages. Some books have enhanced properties, that are only effective when the book is held."; } }

		public override SpellbookType SpellbookType{ get{ return SpellbookType.Song; } }
		public override int BookOffset{ get{ return BardSongProvider.FirstSpellId; } }
		public override int BookCount{ get{ return BardSongProvider.SpellCount; } }

		[Constructable]
		public SongBook() : this( (ulong)0 )
		{
		}

		[Constructable]
		public SongBook( ulong content ) : base( content, 0x671B )
		{
			Name = "bardic songs";
			Layer = Layer.Trinket;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( GetWorldLocation(), 1 ) )
			{
				from.CloseGump( typeof( SongBookGump ) );
				from.SendGump( new SongBookGump( from, this, 1 ) );
			}
		}

		public static string SpellDescription( int spell )
		{
			string txt = "This is a bardic song: ";
			var definition = BardSongProvider.GetDefinition(spell);
			if ( definition == null ) return txt; // Unknown spell

			txt += definition.Description;

			return txt + " It requires at least a " + definition.MinSkill + " in Musicianship to perform.";
		}

		public SongBook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			switch( version )
			{
				case 0:
				{
					var Instrument = version < 1 ? reader.ReadItem() as BaseInstrument : null;
					break;
				}
			}

			if ( ItemID != 0x671B && ItemID != 0x671C )
				ItemID = 0x671B;
		}
	}
}
