using System;
using System.Collections;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Localization;

namespace Server.Items
{
	public class ShardOfHatred : Item
	{
		[Constructable]
		public ShardOfHatred() : base( 0x3155 )
		{
			Name = "Shard of Hatred";
			Weight = 1.0;
			Hue = 0x48E;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) ) 
			{
				from.SendMessage( StringCatalog.Resolve( from.Account, "This must be in your backpack to use." ) );
				return;
			}
			else
			{
				from.SendMessage( StringCatalog.Resolve( from.Account, "You feel the hate emanating from this shard." ) );
			}
		}

		public ShardOfHatred(Serial serial) : base(serial)
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