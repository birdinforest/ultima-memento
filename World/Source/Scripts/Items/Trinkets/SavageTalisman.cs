using Server.Network;
using Server.Localization;

namespace Server.Items
{
	public class SavageTalisman : TrinketTalisman
	{
		public Mobile ItemOwner;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Item_Owner { get{ return ItemOwner; } set{ ItemOwner = value; } }

		[Constructable]
		public SavageTalisman() : this(80, 50)
		{
		}

		[Constructable]
		public SavageTalisman(int campingBonus, int cookingBonus)
		{
			Name = "barbaric talisman";
			ItemID = 0x2F5A;
			Resource = CraftResource.None;
			Layer = Layer.Trinket;
			Weight = 1.0;
			Hue = 0;
			SkillBonuses.SetValues(0, SkillName.Camping, campingBonus);
			SkillBonuses.SetValues(1, SkillName.Cooking, cookingBonus);
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( ItemOwner != null )
				AddLocalizedProperty(list, "prop.equip.trinket.savage.for", ItemOwner.Name);
        }

		public override bool CanEquip( Mobile from )
		{
			if ( ItemOwner != from )
			{
				from.LocalOverheadMessage( MessageType.Emote, 0x916, true, StringCatalog.ResolveByKey( from.Account, "prop.equip.trinket.msg.belongs.other" ) );
				return false;
			}
			return base.CanEquip( from );
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.equip.trinket.msg.hip" ) );
			return;
		}

		public SavageTalisman( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
			writer.Write( (Mobile)ItemOwner );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			ItemOwner = reader.ReadMobile();
		}
	}
}