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
			if ( BuildingPropertyListLocale != null )
			{
				if ( ItemOwner != null )
					AddLocalizedProperty(list, "prop.trinket.savage.for", ItemOwner.Name);
				else
					AddLocalizedProperty(list, "prop.trinket.trinket");
			}
			else
			{
				if ( ItemOwner != null ){ list.Add( 1070722, "Talisman for " + ItemOwner.Name + "" ); } else { list.Add( 1070722, "Trinket"); }
			}
        }

		public override bool OnEquip( Mobile from )
		{
			if ( this.ItemOwner != from )
			{
				from.LocalOverheadMessage( MessageType.Emote, 0x916, true, StringCatalog.Resolve( from.Account, "This talisman belongs to another!" ) );
				return false;
			}
			return true;
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "Talismans are worn in the upper right slot." ) );
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