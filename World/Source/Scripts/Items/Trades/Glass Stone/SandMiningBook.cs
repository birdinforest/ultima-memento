using System;
using Server.Items;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public class SandMiningBook : Item
	{
		public override bool IsContentLocalized => true;

		public override string DefaultName
		{
			get { return "Find Glass-Quality Sand"; }
		}

		public override string DisplayNameLocalizationKey => "item.trade.name.book.sand.mining";

		[Constructable]
		public SandMiningBook() : base( 0xFF4 )
		{
			Weight = 1.0;
		}

		public SandMiningBook( Serial serial ) : base( serial )
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
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.book.sandmining.need.gm" ) );
			}
			else if ( pm.SandMining )
			{
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "prop.trade.book.sandmining.already" ) );
			}
			else
			{
				pm.SandMining = true;
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "prop.trade.book.sandmining.success" ) );
				Delete();
			}
		}
	}
}