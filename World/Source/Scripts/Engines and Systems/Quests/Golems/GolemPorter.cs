using System; 
using System.Collections; 
using Server.Misc; 
using Server.Items; 
using Server.Mobiles; 
using Server.Network;
using System.Collections.Generic;
using Server.ContextMenus;

namespace Server.Mobiles 
{
	[CorpseName( "a broken machine" )] 
	public class GolemPorter : BaseCreature
	{
		public int PorterExodus;
		[CommandProperty(AccessLevel.Owner)]
		public int Porter_Exodus{ get { return PorterExodus; } set { PorterExodus = value; InvalidateProperties(); } }

		private DateTime m_NextTalking;
		public DateTime NextTalking{ get{ return m_NextTalking; } set{ m_NextTalking = value; } }
		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if ( DateTime.Now >= m_NextTalking && InRange( m, 20 ) )
			{
				this.Loyalty = 100;
				m_NextTalking = (DateTime.Now + TimeSpan.FromSeconds( 300 ));
			}
		}

		[Constructable] 
		public GolemPorter( ) : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			m_NextTalking = (DateTime.Now + TimeSpan.FromSeconds( 60 ));

			Name = "a golem";
			Body = 752;
			ControlSlots = 5;
			Blessed = true;
			ActiveSpeed = 0.1;
			PassiveSpeed = 0.2;

			SetStr( 100 );

			Container pack = Backpack;

			if ( pack != null )
				pack.Delete();

			pack = new StrongBackpack();
			pack.Movable = false;

			AddItem( pack );
		}

		public override bool ClickTitle{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool AlwaysAttackable{ get{ return false; } }
		public override bool InitialInnocent{ get{ return true; } }
		public override bool DeleteOnRelease{ get{ return true; } }
		public override bool DeleteCorpseOnDeath{ get{ return true; } }
		public override bool IsDispellable { get { return false; } }
		public override bool IsBondable{ get{ return false; } }
		public override bool CanBeRenamedBy( Mobile from ){ return true; }

		public GolemPorter( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void OnAfterSpawn()
		{
			if ( Hue == 2118 )
			{
				Title = "of Exodus";

				int cores = PorterExodus;
				if ( cores > 3 ) cores = 3; // safety clamp for old data

				int coreBonus = 0;
				if ( cores >= 1 ) coreBonus += 130;
				if ( cores >= 2 ) coreBonus += 70;
				if ( cores >= 3 ) coreBonus += 40;
				// coreBonus: 1→130, 2→200, 3→240

				// Base Str from resource (same as non-Exodus path)
				int baseStr = 100;
				if ( Resource == CraftResource.DullCopper ) baseStr = 150;
				else if ( Resource == CraftResource.ShadowIron ) baseStr = 200;
				else if ( Resource == CraftResource.Copper ) baseStr = 250;
				else if ( Resource == CraftResource.Bronze ) baseStr = 300;
				else if ( Resource == CraftResource.Gold ) baseStr = 350;
				else if ( Resource == CraftResource.Agapite ) baseStr = 400;
				else if ( Resource == CraftResource.Verite ) baseStr = 450;
				else if ( Resource == CraftResource.Valorite ) baseStr = 500;

				SetStr( baseStr + coreBonus );
			}
			else if ( Resource == CraftResource.DullCopper ){ SetStr( 150 ); }
			else if ( Resource == CraftResource.ShadowIron ){ SetStr( 200 ); }
			else if ( Resource == CraftResource.Copper ){ SetStr( 250 ); }
			else if ( Resource == CraftResource.Bronze ){ SetStr( 300 ); }
			else if ( Resource == CraftResource.Gold ){ SetStr( 350 ); }
			else if ( Resource == CraftResource.Agapite ){ SetStr( 400 ); }
			else if ( Resource == CraftResource.Verite ){ SetStr( 450 ); }
			else if ( Resource == CraftResource.Valorite ){ SetStr( 500 ); }
			else { Hue = 0x430; SetStr( 100 ); }
		}

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); // version
            writer.Write( PorterExodus );
			Loyalty = 100;
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt();
			PorterExodus = reader.ReadInt();

			LeaveNowTimer thisTimer = new LeaveNowTimer( this ); 
			thisTimer.Start(); 
		} 

		public override bool IsSnoop( Mobile from )
		{
			return false;
		}

		public override bool OnDragDrop( Mobile from, Item item )
		{
			if ( PackAnimal.CheckAccess( this, from ) )
			{
				AddToBackpack( item );
				return true;
			}

			return base.OnDragDrop( from, item );
		}

		public override bool CheckNonlocalDrop( Mobile from, Item item, Item target )
		{
			return PackAnimal.CheckAccess( this, from );
		}

		public override bool CheckNonlocalLift( Mobile from, Item item )
		{
			return PackAnimal.CheckAccess( this, from );
		}

		public override void OnDoubleClick( Mobile from )
		{
			PackAnimal.TryPackOpen( this, from );
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
		{
			base.GetContextMenuEntries( from, list );

			PackAnimal.GetContextMenuEntries( this, from, list );
		}
	}
}