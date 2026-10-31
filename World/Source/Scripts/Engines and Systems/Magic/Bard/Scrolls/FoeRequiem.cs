using Server.Spells.Song;

namespace Server.Items
{
	public class FoeRequiemScroll : SpellScroll
	{
		public override string DefaultDescription{ get{ return SongBook.SpellDescription( SpellID ); } }

		[Constructable]
		public FoeRequiemScroll() : this( 1 )
		{
		}

		[Constructable]
		public FoeRequiemScroll( int amount ) : base( FoeRequiemSong.SpellInfo.SpellDefinition.SpellID, 0x1F47, amount )
		{
			Name = string.Format("{0} sheet music", BardSongProvider.GetDefinition(SpellID).Name);
			Hue = 0x96;
			Stackable = true;
        }

		public FoeRequiemScroll( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "The sheet music must be in your music book." );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}