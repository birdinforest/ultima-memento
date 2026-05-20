using System;
using Server;
using Server.Localization;

namespace Server.Items
{
	public class TotalRefreshPotion : BaseRefreshPotion
	{
		public override string DefaultDescription{ get{ return StringCatalog.Resolve( null, "These potions will fully recover your stamina." ); } }

		public override double Refresh{ get{ return 1.0; } }

		[Constructable]
		public TotalRefreshPotion() : base( PotionEffect.RefreshTotal )
		{
			ItemID = 0x25FF;
			Name = "total refresh potion";
		}

		public TotalRefreshPotion( Serial serial ) : base( serial )
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
			Name = "total refresh potion";
		}
	}
}
