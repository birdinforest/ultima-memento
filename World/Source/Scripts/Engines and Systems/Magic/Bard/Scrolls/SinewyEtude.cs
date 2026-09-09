using Server.Spells.Song;

namespace Server.Items
{
	public class SinewyEtudeScroll : SpellScroll
	{
		public override string DefaultDescription{ get{ return SongBook.SpellDescription( SpellID ); } }

		[Constructable]
		public SinewyEtudeScroll() : this( 1 )
		{
		}

		[Constructable]
		public SinewyEtudeScroll( int amount ) : base( SinewyEtudeSong.SpellInfo.SpellDefinition.SpellID, 0x1F4B, amount )
		{
			Name = string.Format("{0} sheet music", BardSongProvider.GetDefinition(SpellID).Name);
			Hue = 0x96;
			Stackable = true;
        }

		public SinewyEtudeScroll( Serial serial ) : base( serial )
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