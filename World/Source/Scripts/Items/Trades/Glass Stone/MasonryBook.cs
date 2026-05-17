using System;
using Server.Items;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public class MasonryBook : Item
	{
		public override bool IsContentLocalized => true;

		public override string DefaultName
		{
			get { return "Making Valuables With Stonecrafting"; }
		}

		public override string DisplayNameLocalizationKey => "item.trade.name.book.masonry";

		[Constructable]
		public MasonryBook() : base( 0xFBE )
		{
			Weight = 1.0;
		}

		public MasonryBook( Serial serial ) : base( serial )
		{
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

		public override void OnDoubleClick( Mobile from )
		{
			PlayerMobile pm = from as PlayerMobile;

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if ( pm == null || from.Skills[SkillName.Carpentry].Base < 100.0 )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.book.masonry.need.gm" ) );
			}
			else if ( pm.Masonry )
			{
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "prop.trade.book.masonry.already" ) );
			}
			else
			{
				pm.Masonry = true;
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "prop.trade.book.masonry.success" ) );
				Delete();
			}
		}
	}
}