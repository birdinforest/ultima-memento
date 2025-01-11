using Server.Accounting;
using Server.Commands;
using Server.Mobiles;
using System.Collections;
using System.IO;
using System;

namespace Server.Items
{
    public class TaskManager60Min : Item
	{
		[Constructable]
		public TaskManager60Min () : base( 0x0EDE )
		{
			Movable = false;
			Name = "Task Manager 1 Hour";
			Visible = false;
			TaskTimer thisTimer = new TaskTimer( this ); 
			thisTimer.Start(); 
		}

        public TaskManager60Min(Serial serial) : base(serial)
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

			if ( MySettings.S_RunRoutinesAtStartup && !( File.Exists( "Data/Data.ref" ) ) )
			{
				FirstTimer thisTimer = new FirstTimer( this ); 
				thisTimer.Start();
			}
			else
			{
				TaskTimer thisTimer = new TaskTimer( this ); 
				thisTimer.Start(); 
			}

			if ( File.Exists( "Data/Data.ref" ) )
			{
				BuildWorlds buildTimer = new BuildWorlds( this ); 
				buildTimer.Start();
			}
		}

		public class TaskTimer : Timer 
		{
			private Item i_item; 
			public TaskTimer( Item task ) : base( TimeSpan.FromMinutes( 60.0 ) )
			{ 
				Priority = TimerPriority.OneMinute; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				RunThis( i_item );
			} 
		}

		public class BuildWorlds : Timer 
		{ 
			private Item i_item; 
			public BuildWorlds( Item task ) : base( TimeSpan.FromSeconds( 10.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				BuildThis( i_item );
			} 
		}

		public static void BuildThis( Item itm )
		{
			Mobile from = null;

			foreach ( Account a in Accounts.GetAccounts() )
			{
				if (a == null)
					break;

				int index = 0;

				for (int i = 0; i < a.Length; ++i)
				{
					Mobile m = a[i];

					if (m == null)
						continue;

					if ( m.AccessLevel == AccessLevel.Owner )
						from = m;

					++index;
				}
			}

			if ( from != null )
				CommandSystem.Handle(from, String.Format("{0}{1}", CommandSystem.Prefix, "BuildWorld"));

			Console.WriteLine("You may now play " + MySettings.S_ServerName + "!");
			Console.WriteLine("");
		}

		public class FirstTimer : Timer 
		{ 
			private Item i_item; 
			public FirstTimer( Item task ) : base( TimeSpan.FromSeconds( 10.0 ) )
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				RunThis( i_item );
			} 
		}

		public static void RunThis( Item itm )
		{
			TaskTimer thisTimer = new TaskTimer( itm ); 
			thisTimer.Start(); 

			// SWITCH UP THE MAGIC MIRRORS
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Hourly) Reconfigure Magic Mirrors..." ); }
			Server.Items.MagicMirror.SetMirrors();

			// REMOVE ACTION SET PIECES
			Server.Items.ActionFunc.RemoveActions( false, itm.Location, itm.Map );

			// MOVE SHOPKEEPERS AND GUARDS TO WHERE THEY BELONG...IN CASE THEY MOVED FAR AWAY
			// ALSO CLEAN UP ANY GRAVE DISTURBED MONSTERS OR TREASURE MAP MONSTERS
			ArrayList vendors = new ArrayList();
			ArrayList citizens = new ArrayList();
			ArrayList wanderers = new ArrayList();
			ArrayList monsters = new ArrayList();
			foreach ( Mobile vendor in World.Mobiles.Values )
			{
				if ( vendor is BaseVendor && vendor.WhisperHue != 911 && vendor.WhisperHue != 999 && !(vendor is PlayerVendor) && !(vendor is PlayerBarkeeper) )
				{
					vendors.Add( vendor );
				}
				else if ( vendor is TownGuards )
				{
					vendors.Add( vendor );
				}
				else if ( vendor is Citizens && vendor.Fame > 0 )
				{
					citizens.Add( vendor );
				}
				else if ( vendor is BaseCreature && ( ((BaseCreature)vendor).WhisperHue == 999 || ((BaseCreature)vendor).WhisperHue == 999 ) && vendor.Location == ((BaseCreature)vendor).Home )
				{
					wanderers.Add( vendor );
				}
				else if ( vendor is BaseCreature && ((BaseCreature)vendor).ControlSlots == 666 && vendor.Combatant == null )
				{
					monsters.Add( vendor );
				}
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Hourly) Move Vendors and Guards Back..." ); }
			for ( int i = 0; i < vendors.Count; ++i )
			{
				Mobile vendor = ( Mobile )vendors[ i ];
				BaseCreature vendur = ( BaseCreature )vendors[ i ];

				vendor.Location = vendur.Home;

				if ( Server.Items.ActionFunc.HasActs( vendor ) )
					Server.Items.ActionFunc.MakeActs( (BaseCreature)vendor );
				else if ( vendor is BaseVendor )
					((BaseCreature)vendor).RangeHome = ((BaseCreature)vendor).ControlSlots;
			}
			for ( int i = 0; i < citizens.Count; ++i )
			{
				Mobile citizen = ( Mobile )citizens[ i ];
				citizen.Fame = 0;
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Hourly) Removing Wandering Creatures..." ); }
			for ( int i = 0; i < wanderers.Count; ++i )
			{
				Mobile wanderer = ( Mobile )wanderers[ i ];
				wanderer.Delete();
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Hourly) Remove Certain Creatures..." ); }
			for ( int i = 0; i < monsters.Count; ++i )
			{
				Mobile ridof = ( Mobile )monsters[ i ];
				Effects.SendLocationParticles( EffectItem.Create( ridof.Location, ridof.Map, EffectItem.DefaultDuration ), 0x3728, 8, 20, 5042 );
				Effects.PlaySound( ridof, ridof.Map, 0x201 );
				ridof.Delete();
			}
			
			ArrayList targets = new ArrayList();
			foreach ( Item item in World.Items.Values )
			if ( item is MushroomTrap || item is LandChest || item is Strange_Portal || item is StrangePortal || item is WaterChest || item is RavendarkStorm || item is HiddenTrap || item is DungeonChest || item is HiddenChest || item is AltarGodsEast || item is AltarGodsSouth || item is AltarShrineEast || item is AltarShrineSouth || item is AltarStatue || item is AltarSea || item is AltarDryad || item is AltarEvil || item is AltarDaemon )
			{
				if ( item != null )
				{
					targets.Add( item );
				}
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Hourly) Reconfigure Traps, Chests, and Altars..." ); }
			for ( int i = 0; i < targets.Count; ++i )
			{
				Item item = ( Item )targets[ i ];

				if ( item is MushroomTrap )
				{
					item.Hue = Utility.RandomList( 0x47E, 0x48B, 0x495, 0xB95, 0x5B6, 0x5B7, 0x55F, 0x55C, 0x556, 0x54F, 0x489 );

					switch( Utility.RandomMinMax( 1, 6 ) )
					{
						case 1: item.Name = "strange mushroom"; break;
						case 2: item.Name = "weird mushroom"; break;
						case 3: item.Name = "odd mushroom"; break;
						case 4: item.Name = "curious mushroom"; break;
						case 5: item.Name = "peculiar mushroom"; break;
						case 6: item.Name = "bizarre mushroom"; break;
					}
				}
				else if ( item is AltarGodsEast )
				{
					Item shrine = new AltarGodsEast();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					item.Delete();
				}
				else if ( item is AltarGodsSouth )
				{
					Item shrine = new AltarGodsSouth();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					item.Delete();
				}
				else if ( item is AltarShrineEast )
				{
					Item shrine = new AltarShrineEast();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					item.Delete();
				}
				else if ( item is AltarShrineSouth )
				{
					Item shrine = new AltarShrineSouth();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					item.Delete();
				}
				else if ( item is AltarStatue )
				{
					Item shrine = new AltarStatue();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					item.Delete();
				}
				else if ( item is AltarSea )
				{
					Item shrine = new AltarSea();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					if ( item.ItemID == 0x4FB1 || item.ItemID == 0x4FB2 )
					{
						shrine.Hue = 0;
						shrine.Name = "Shrine of Poseidon";
						shrine.ItemID = Utility.RandomList( 0x4FB1, 0x4FB2 );
					}
					else if ( item.ItemID == 0x6395 )
					{
						shrine.Hue = 0;
						shrine.Name = "Shrine of Neptune";
						shrine.ItemID = 0x6395;
					}
					item.Delete();
				}
				else if ( item is AltarEvil )
				{
					Item shrine = new AltarEvil();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					item.Delete();
				}
				else if ( item is AltarDryad )
				{
					Item shrine = new AltarDryad();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					item.Delete();
				}
				else if ( item is AltarDaemon )
				{
					Item shrine = new AltarDaemon();
					shrine.Weight = -2.0;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					if ( item.ItemID == 0x6393 || item.ItemID == 0x6394 )
					{
						shrine.Hue = 0;
						shrine.Name = "Shrine of Ktulu";
						shrine.ItemID = item.ItemID;
					}
					item.Delete();
				}
				else if ( item is AltarGargoyle )
				{
					Item shrine = new AltarGargoyle();
					shrine.Weight = -2.0;
					shrine.ItemID = item.ItemID;
					shrine.Movable = false;
					shrine.MoveToWorld (new Point3D(item.X, item.Y, item.Z), item.Map);
					item.Delete();
				}
				else if ( item is DungeonChest )
				{
					DungeonChest box = (DungeonChest)item;
					if ( box.ContainerLockable > 0 && box.ContainerTouched != 1 )
					{
						box.Locked = false;
						switch( Utility.Random( 3 ) )
						{
							case 0: box.Locked = true; break;
						}
					}
					else
					{
						box.Locked = false;
					}
					if ( box.ContainerLevel > 0 && box.ContainerTouched != 1 )
					{
						switch ( Utility.Random( 9 ) )
						{
							case 0: box.TrapType = TrapType.DartTrap; break;
							case 1: box.TrapType = TrapType.None; break;
							case 2: box.TrapType = TrapType.ExplosionTrap; break;
							case 3: box.TrapType = TrapType.MagicTrap; break;
							case 4: box.TrapType = TrapType.PoisonTrap; break;
							case 5: box.TrapType = TrapType.None; break;
							case 6: box.TrapType = TrapType.None; break;
							case 7: box.TrapType = TrapType.None; break;
							case 8: box.TrapType = TrapType.None; break;
						}
					}
				}
				else
				{
					item.Delete();
				}
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Hourly) Tasks Complete!" ); }
		}
	}
}