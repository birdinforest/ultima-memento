using System;
using Server;
using Server.Misc;
using Server.Localization;

namespace Server.Items
{
	public class OrbOfTheAbyss : TrinketTalisman
	{
		public override bool IsContentLocalized => true;

		private static string ShotkeyForAbyssName( string name )
		{
			if ( name == null )
				return null;
			switch ( name )
			{
				case "orb of the abyss": return "item.special.abyss.orb";
				case "ring of the abyss": return "item.special.abyss.ring";
				case "amulet of the abyss": return "item.special.abyss.amulet";
				case "bracelet of the abyss": return "item.special.abyss.bracelet";
				case "earrings of the abyss": return "item.special.abyss.earrings";
			}
			return null;
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
			{
				string key = ShotkeyForAbyssName( Name );
				if ( key != null )
				{
					if ( Amount <= 1 )
						AddLocalizedProperty( list, key );
					else
						list.Add( 1050039, "{0}\t{1}", Amount, ResolvePropertyText( key ) );
					return;
				}
			}
			base.AddNameProperty( list );
		}

		public Mobile owner;

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner { get{ return owner; } set{ owner = value; } }

		[Constructable]
		public OrbOfTheAbyss()
		{
			Name = "orb of the abyss";
			ItemID = 0x2C84;
			Hue = 0x489;
		}

		public override bool OnEquip( Mobile from )
		{
			if ( owner != from )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.special.orbabyss.not.yours" ) );
				return false;
			}

			return base.OnEquip( from );
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( owner == null )
				return;

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "prop.special.orbabyss.belongs", owner.Name );
			else
				list.Add( 1049644, "Belongs to " + owner.Name + "");
        } 

		public OrbOfTheAbyss( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
			writer.Write( (Mobile)owner);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			owner = reader.ReadMobile();

			if ( ItemID != 0x2C84 && ItemID < 26413 ){ ItemID = 0x2C84; Name = "orb of the abyss"; }
		}

		public static bool ChangeOrb( Mobile m, Mobile tinker, Item orb )
		{
			if ( orb.ItemID == 0x2C84 ){ orb.ItemID = 0x6745; orb.Name = "ring of the abyss"; orb.Layer = Layer.Ring; }
			else if ( orb.ItemID == 0x6745 ){ orb.ItemID = 0x6744; orb.Name = "amulet of the abyss"; orb.Layer = Layer.Neck; }
			else if ( orb.ItemID == 0x6744 ){ orb.ItemID = 0x6741; orb.Name = "bracelet of the abyss"; orb.Layer = Layer.Bracelet; }
			else if ( orb.ItemID == 0x6741 ){ orb.ItemID = 0x6743; orb.Name = "earrings of the abyss"; orb.Layer = Layer.Earrings; }
			else { orb.ItemID = 0x2C84; orb.Name = "orb of the abyss"; orb.Layer = Layer.Trinket; }

			string lang = ( tinker.Account != null ) ? AccountLang.GetLanguageCode( tinker.Account ) : "en";
			string tpl = StringCatalog.TryResolveByKey( lang, "prop.special.orbabyss.tinker.give" )
				?? StringCatalog.TryResolveByKey( "en", "prop.special.orbabyss.tinker.give" )
				?? "Here. You now have the {0}.";
			tinker.Say( string.Format( tpl, orb.Name ) );

			m.PlaySound( 0x542 );

			return false;
		}
	}
}