using System.Collections.Generic;

namespace Server.Items
{
	public class JewelryBox : LockableContainer
	{
		public override bool DisplayLootType{ get{ return false; } }
		public override bool DisplaysContent{ get{ return false; } }
		public override bool DisplayWeight{ get{ return false; } }
		private int m_JunkAmount;

		[Constructable]
		public JewelryBox() : base( 0x10E2 )
		{
			Name = "jewelry box";
			Locked = true;
			LockLevel = 1000;
			MaxLockLevel = 1000;
			RequiredSkill = 1000;
			Weight = 10.0;
			VirtualContainer = true;
			ColorText1 = "Entangled Jewelry!";
			ColorText3 = "Give to a Jeweler";
			ColorText4 = "To Separate the Jewelry";
			ColorHue1 = ColorHue3 = ColorHue4 = "E15656";
		}

		public void AddJunk(int amount)
		{
			m_JunkAmount += amount;
		}

		public Item TryGetJunk()
		{
			if (m_JunkAmount < 1) return null;

			RustyJunk broke = new RustyJunk();
			broke.ItemID = 0x10E2;
			broke.Name = "a pile of broken jewelry";
			broke.Hue = 0x9C4;
			broke.Weight = m_JunkAmount;

			m_JunkAmount = 0;

			return broke;
		}

		public static List<Item> FindCandidates( Mobile m )
		{
			if ( m == null || m.Backpack == null ) return new List<Item>();
			
			List<Item> list = new List<Item>();
			Item jw;
			if ( m.FindItemOnLayer( Layer.Bracelet ) != null ) { jw = m.FindItemOnLayer( Layer.Bracelet ); if ( jw.LootType != LootType.Blessed && jw is BaseTrinket && jw.Catalog == Catalogs.Jewelry ){ list.Add(jw); } }
			if ( m.FindItemOnLayer( Layer.Ring ) != null ) { jw = m.FindItemOnLayer( Layer.Ring ); if ( jw.LootType != LootType.Blessed && jw is BaseTrinket && jw.Catalog == Catalogs.Jewelry ){ list.Add(jw); } }
			if ( m.FindItemOnLayer( Layer.Helm ) != null ) { jw = m.FindItemOnLayer( Layer.Helm ); if ( jw.LootType != LootType.Blessed && jw is BaseTrinket && jw.Catalog == Catalogs.Jewelry ){ list.Add(jw); } }
			if ( m.FindItemOnLayer( Layer.Neck ) != null ) { jw = m.FindItemOnLayer( Layer.Neck ); if ( jw.LootType != LootType.Blessed && jw is BaseTrinket && jw.Catalog == Catalogs.Jewelry ){ list.Add(jw); } }
			if ( m.FindItemOnLayer( Layer.Earrings ) != null ) { jw = m.FindItemOnLayer( Layer.Earrings ); if ( jw.LootType != LootType.Blessed && jw is BaseTrinket && jw.Catalog == Catalogs.Jewelry ){ list.Add(jw); } }

			return list;
		}

		public override void OnDoubleClick( Mobile from )
		{
		}

		public override bool TryDropItem( Mobile from, Item dropped, bool sendFullMessage )
		{
			return false;
		}

		public override bool CheckLocked( Mobile from )
		{
			return true;
		}

		public override bool OnDragDropInto( Mobile from, Item item, Point3D p )
		{
			return false;
		}

		public override int GetTotal(TotalType type)
        {
			return 0;
        }

		public JewelryBox( Serial serial ) : base( serial )
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
	}
}