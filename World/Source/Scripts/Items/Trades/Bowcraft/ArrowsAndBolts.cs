using System;
using Server;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Mobiles;
using Server.Localization;

namespace Server.Items
{
	public class ManyArrows100 : Item
	{
		public override bool IsContentLocalized => true;

		[Constructable]
		public ManyArrows100() : base( 0xF41 )
		{
			Name = "Bundle of 100 Arrows";
			Weight = 10;
		}

		public ManyArrows100( Serial serial ) : base( serial )
		{
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
				list.Add( ResolvePropertyText( "item.trade.bow.bundle.arrows.100" ) );
			else
				base.AddNameProperty( list );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.bow.msg.backpack" ) );
				return;
			}
			else
			{
				from.AddToBackpack ( new Arrow( 100 ) );
				from.PrivateOverheadMessage( MessageType.Regular, 0x14C, false, StringCatalog.ResolveByKey( from.Account, "prop.trade.bow.msg.separate.arrows" ), from.NetState );
				this.Delete();
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.trade.bow.bundle.contains.arrows.100" );
				AddLocalizedProperty( list, "prop.trade.bow.bundle.separate" );
			}
			else
			{
				list.Add( 1070722, "This Bundle Contains 100 Arrows");
				list.Add( 1049644, "Double-Click To Separate Them Into Your Pack");
			}
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
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public class ManyArrows1000 : Item
	{
		public override bool IsContentLocalized => true;

		[Constructable]
		public ManyArrows1000() : base( 0xF41 )
		{
			Name = "Bundle of 1,000 Arrows";
			Weight = 100;
		}

		public ManyArrows1000( Serial serial ) : base( serial )
		{
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
				list.Add( ResolvePropertyText( "item.trade.bow.bundle.arrows.1000" ) );
			else
				base.AddNameProperty( list );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.bow.msg.backpack" ) );
				return;
			}
			else
			{
				from.AddToBackpack ( new Arrow( 1000 ) );
				from.PrivateOverheadMessage( MessageType.Regular, 0x14C, false, StringCatalog.ResolveByKey( from.Account, "prop.trade.bow.msg.separate.arrows" ), from.NetState );
				this.Delete();
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.trade.bow.bundle.contains.arrows.1000" );
				AddLocalizedProperty( list, "prop.trade.bow.bundle.separate" );
			}
			else
			{
				list.Add( 1070722, "This Bundle Contains 1,000 Arrows");
				list.Add( 1049644, "Double-Click To Separate Them Into Your Pack");
			}
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
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public class ManyBolts100 : Item
	{
		public override bool IsContentLocalized => true;

		[Constructable]
		public ManyBolts100() : base( 0x1BFD )
		{
			Name = "Bundle of 100 Bolts";
			Weight = 10;
		}

		public ManyBolts100( Serial serial ) : base( serial )
		{
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
				list.Add( ResolvePropertyText( "item.trade.bow.bundle.bolts.100" ) );
			else
				base.AddNameProperty( list );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.bow.msg.backpack" ) );
				return;
			}
			else
			{
				from.AddToBackpack ( new Bolt( 100 ) );
				from.PrivateOverheadMessage( MessageType.Regular, 0x14C, false, StringCatalog.ResolveByKey( from.Account, "prop.trade.bow.msg.separate.bolts" ), from.NetState );
				this.Delete();
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.trade.bow.bundle.contains.bolts.100" );
				AddLocalizedProperty( list, "prop.trade.bow.bundle.separate" );
			}
			else
			{
				list.Add( 1070722, "This Bundle Contains 100 Bolts");
				list.Add( 1049644, "Double-Click To Separate Them Into Your Pack");
			}
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
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public class ManyBolts1000 : Item
	{
		public override bool IsContentLocalized => true;

		[Constructable]
		public ManyBolts1000() : base( 0x1BFD )
		{
			Name = "Bundle of 1,000 Bolts";
			Weight = 100;
		}

		public ManyBolts1000( Serial serial ) : base( serial )
		{
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
				list.Add( ResolvePropertyText( "item.trade.bow.bundle.bolts.1000" ) );
			else
				base.AddNameProperty( list );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.bow.msg.backpack" ) );
				return;
			}
			else
			{
				from.AddToBackpack ( new Bolt( 1000 ) );
				from.PrivateOverheadMessage( MessageType.Regular, 0x14C, false, StringCatalog.ResolveByKey( from.Account, "prop.trade.bow.msg.separate.bolts" ), from.NetState );
				this.Delete();
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.trade.bow.bundle.contains.bolts.1000" );
				AddLocalizedProperty( list, "prop.trade.bow.bundle.separate" );
			}
			else
			{
				list.Add( 1070722, "This Bundle Contains 1,000 Bolts");
				list.Add( 1049644, "Double-Click To Separate Them Into Your Pack");
			}
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
