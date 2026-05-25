using System;
using Server;
using Server.ContextMenus;
using System.Collections;
using System.Collections.Generic;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Localization;
using Server.Mobiles;

namespace Server.Mobiles
{
	public class TradesmanMiner : Citizens
	{
		[Constructable]
		public TradesmanMiner()
		{
			CitizenType = 7;
			SetupCitizen();
			Blessed = true;
			CantWalk = true;
			AI = AIType.AI_Melee;
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
		}

		public override void OnThink()
		{
			if ( DateTime.Now >= m_NextTalk )
			{
				foreach ( Item anvil in this.GetItemsInRange( 1 ) )
				{
					if ( anvil is RockHit )
					{
						if ( this.FindItemOnLayer( Layer.FirstValid ) != null && !(this.FindItemOnLayer( Layer.FirstValid ) is Pickaxe) ) { this.Delete(); }
						else if ( this.FindItemOnLayer( Layer.OneHanded ) != null && !(this.FindItemOnLayer( Layer.OneHanded ) is Pickaxe) ) { this.Delete(); }
						else if ( this.FindItemOnLayer( Layer.TwoHanded ) != null ){ this.FindItemOnLayer( Layer.TwoHanded ).Delete(); }
						RockHit smith = (RockHit)anvil;
						smith.OnDoubleClick( this );
						m_NextTalk = (DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 2, 5 ) ));
					}
				}
			}
		}

		public override void OnAfterSpawn()
		{
			base.OnAfterSpawn();
			Server.Misc.TavernPatrons.RemoveSomeGear( this, false );
			Server.Misc.MorphingTime.CheckNecromancer( this );
			Item hammer = new Pickaxe();
			hammer.Movable = false;
			AddItem( hammer );
		}

		public TradesmanMiner( Serial serial ) : base( serial )
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

namespace Server.Items
{
	public class RockHit : Item
	{
		[Constructable]
		public RockHit() : base( 0x1775 )
		{
			Name = StringCatalog.ResolveByKey(null, "mob.other.rock");
			Visible = false;
			Movable = false;
			Weight = -2.0;
		}

		public RockHit( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.FindItemOnLayer( Layer.OneHanded ) is BaseWeapon && from is Citizens )
			{
				BaseWeapon weapon = ( BaseWeapon )( from.FindItemOnLayer( Layer.OneHanded ) );
				from.Direction = from.GetDirectionTo( GetWorldLocation() );
				weapon.PlaySwingAnimation( from );
				from.PlaySound( Utility.RandomList( 0x125, 0x126 ) );
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
			Weight = -2.0;
		}
	}
}

namespace Server.Items
{
	public class CrateOfOre : Item
	{
		[Constructable]
		public CrateOfOre() : base( 0x50B5 )
		{
			Name = StringCatalog.ResolveByKey(null, "mob.other.crate_of_ore");
			Weight = 10;
			Limits = Utility.RandomMinMax( 4, 12 ) * 100;
			Fill();
		}

		public void Fill()
		{
			ItemID = 0x50B5;

			if ( Resource == CraftResource.None || Resource == CraftResource.Steel && Resource == CraftResource.Brass )
				ResourceMods.SetRandomResource( false, true, this, CraftResource.Iron, false, null );

			if ( Resource == CraftResource.Steel || Resource == CraftResource.Brass )
				ResourceMods.SetRandomResource( false, true, this, CraftResource.Valorite, false, null );

			Name = StringCatalog.ResolveFormatByKey(null, "mob.fmt.crate_of_0_ore", CraftResources.GetName(Resource));
			Hue = CraftResources.GetHue(Resource);

			if ( Resource == CraftResource.Iron )
				ItemID = 0x5084;
		}

		public CrateOfOre( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) ) 
			{
				from.SendMessage( Server.Localization.StringCatalog.Resolve(from.Account, "This must be in your backpack to open.") );
				return;
			}
			else
			{
				from.PlaySound( 0x02D );
				from.AddToBackpack ( new LargeCrate() );

				if ( Resource == CraftResource.DullCopper ){ from.AddToBackpack ( new DullCopperOre( Limits ) ); }
				else if ( Resource == CraftResource.ShadowIron ){ from.AddToBackpack ( new ShadowIronOre( Limits ) ); }
				else if ( Resource == CraftResource.Copper ){ from.AddToBackpack ( new CopperOre( Limits ) ); }
				else if ( Resource == CraftResource.Bronze ){ from.AddToBackpack ( new BronzeOre( Limits ) ); }
				else if ( Resource == CraftResource.Gold ){ from.AddToBackpack ( new GoldOre( Limits ) ); }
				else if ( Resource == CraftResource.Agapite ){ from.AddToBackpack ( new AgapiteOre( Limits ) ); }
				else if ( Resource == CraftResource.Verite ){ from.AddToBackpack ( new VeriteOre( Limits ) ); }
				else if ( Resource == CraftResource.Valorite ){ from.AddToBackpack ( new ValoriteOre( Limits ) ); }
				else if ( Resource == CraftResource.Nepturite ){ from.AddToBackpack ( new NepturiteOre( Limits ) ); }
				else if ( Resource == CraftResource.Obsidian ){ from.AddToBackpack ( new ObsidianOre( Limits ) ); }
				else if ( Resource == CraftResource.Mithril ){ from.AddToBackpack ( new MithrilOre( Limits ) ); }
				else if ( Resource == CraftResource.Xormite ){ from.AddToBackpack ( new XormiteOre( Limits ) ); }
				else if ( Resource == CraftResource.Dwarven ){ from.AddToBackpack ( new DwarvenOre( Limits ) ); }
				else { from.AddToBackpack ( new IronOre( Limits ) ); }

				from.PrivateOverheadMessage(MessageType.Regular, 0x14C, false, "You separate the ore into your backpack", from.NetState);
				this.Delete();
			}
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);

			list.Add( 1070722, Server.Localization.StringCatalog.ResolveFormatByKey(null, "mob.fmt.contains_0_1_ore", Limits, CraftResources.GetName(Resource) ) );
			list.Add( 1049644, Server.Localization.StringCatalog.ResolveByKey(null, "mob.other.open_to_remove_them_from_the_crate") );
        }

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( version < 1 )
			{
				Limits = reader.ReadInt();
				string CrateItem = reader.ReadString();
				Fill();
			}
		}
	}
}