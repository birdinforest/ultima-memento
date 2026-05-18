using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public class BookBox : LockableContainer
	{
		public override bool DisplayLootType{ get{ return false; } }
		public override bool DisplaysContent{ get{ return false; } }
		public override bool DisplayWeight{ get{ return false; } }

		[Constructable]
		public BookBox() : base( 0x0C16 )
		{
			Name = "books";
			Locked = true;
			LockLevel = 1000;
			MaxLockLevel = 1000;
			RequiredSkill = 1000;
			Weight = 40.0;
			VirtualContainer = true;
			ColorText1 = "CURSED!";
			ColorText3 = "Give to a Wizard or Knight";
			ColorText4 = "To Remove the Curse";
			ColorText5 = "Or Use Curse Removing Magic";
			ColorHue1 = ColorHue3 = ColorHue4 = ColorHue5 = "E15656";
		}

		public override void OnDoubleClick( Mobile from )
		{
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			string saved3 = m_ColorText3;
			string saved4 = m_ColorText4;
			string saved5 = m_ColorText5;

			if ( BuildingPropertyListLocale != null )
			{
				m_ColorText3 = null;
				m_ColorText4 = null;
				m_ColorText5 = null;
			}

			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
			{
				AddLocalizedProperty( list, "prop.colortext.bookbox.givetowizard" );
				AddLocalizedProperty( list, "prop.colortext.bookbox.removecurse" );
				AddLocalizedProperty( list, "prop.colortext.bookbox.curseoremove" );
			}

			m_ColorText3 = saved3;
			m_ColorText4 = saved4;
			m_ColorText5 = saved5;
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

		public BookBox( Serial serial ) : base( serial )
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