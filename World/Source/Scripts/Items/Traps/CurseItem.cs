using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Localization;

namespace Server.Items
{
	public class CurseItem : LockableContainer
	{
		public override bool DisplayLootType{ get{ return false; } }
		public override bool DisplaysContent{ get{ return false; } }
		public override bool DisplayWeight{ get{ return false; } }
		public override string DisplayNameLocalizationKey => "item.trap.curse.item";

		private Timer m_AutoRemoveTimer;

		[Constructable]
		public CurseItem() : base( 0x9A8 )
		{
			Name = "cursed item";
			Locked = true;
			LockLevel = 1000;
			MaxLockLevel = 1000;
			RequiredSkill = 1000;
			Weight = 40.0;
			VirtualContainer = true;
			ColorText1 = StringCatalog.ResolveByKey(null, "trap.curse.colortext1");
			ColorText3 = StringCatalog.ResolveByKey(null, "trap.curse.colortext3");
			ColorText4 = StringCatalog.ResolveByKey(null, "trap.curse.colortext4");
			ColorText5 = StringCatalog.ResolveByKey(null, "trap.curse.colortext5");
			ColorHue1 = ColorHue3 = ColorHue4 = ColorHue5 = "E15656";

			m_AutoRemoveTimer = Timer.DelayCall(TimeSpan.FromHours(24), new TimerCallback(RemoveCurse));
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.Skills.Spiritualism.Value >= 70 )
			{
				if ( from.CheckSkill( SkillName.Spiritualism, 70, 120 ) )
				{
					from.SendMessage( StringCatalog.ResolveByKey(from.Account, "trap.curse.success") );
					RemoveCurse();
				}
				else
				{
					from.SendMessage( StringCatalog.ResolveByKey(from.Account, "trap.curse.failure") );
				}
			}
			else
			{
				from.SendMessage( StringCatalog.ResolveByKey(from.Account, "trap.curse.noskills") );
			}
		}

		public void RemoveCurse()
		{
			if ( m_AutoRemoveTimer != null )
			{
				m_AutoRemoveTimer.Stop();
				m_AutoRemoveTimer = null;
			}
			if ( !Deleted && TotalItems > 0 )
			{
				List<Item> items = new List<Item>(Items);
				foreach ( Item item in items )
				{
					if ( RootParent is Mobile )
						((Mobile)RootParent).AddToBackpack(item);
					else
						item.MoveToWorld(Location, Map);
				}
			}
			Delete();
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

		public CurseItem( Serial serial ) : base( serial )
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
			if ( TotalItems > 0 )
			{
				m_AutoRemoveTimer = Timer.DelayCall(TimeSpan.FromHours(24), new TimerCallback(RemoveCurse));
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "trap.curse.tooltip" );
			else
				list.Add(1049644, StringCatalog.ResolveByKey(null, "trap.curse.tooltip"));
		}
	}
}
