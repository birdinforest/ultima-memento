using System;
using Server.Items;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public class StoneMiningBook : Item
	{
		public override string DefaultName
		{
			get { return "Mining For Quality Stone"; }
		}

		[Constructable]
		public StoneMiningBook() : base( 0xFBE )
		{
			Weight = 1.0;
		}

		public StoneMiningBook( Serial serial ) : base( serial )
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
			else if ( pm == null || from.Skills[SkillName.Mining].Base < 100.0 )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.book.stonemining.need.gm" ) );
			}
			else if ( pm.StoneMining )
			{
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "prop.trade.book.stonemining.already" ) );
			}
			else
			{
				pm.StoneMining = true;
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "prop.trade.book.stonemining.success" ) );
				Delete();
			}
		}
	}
}