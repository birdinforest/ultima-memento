using Server.Misc;
using Server.Mobiles;
using System.Collections;
using System.IO;
using System;

namespace Server.Items
{
    public class TaskManagerDaily : Item
	{
		[Constructable]
		public TaskManagerDaily () : base( 0x0EDE )
		{
			Movable = false;
			Name = "Task Manager Daily";
			Visible = false;
			TaskTimer thisTimer = new TaskTimer( this ); 
			thisTimer.Start(); 
		}

        public TaskManagerDaily(Serial serial) : base(serial)
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
		}

		public class TaskTimer : Timer 
		{
			private Item i_item; 
			public TaskTimer( Item task ) : base( TimeSpan.FromHours( 24.0 ) )
			{ 
				Priority = TimerPriority.OneMinute; 
				i_item = task; 
			} 

			protected override void OnTick() 
			{
				RunThis( i_item );
			} 
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

		public static void RunThis( Item item )
		{
			TaskTimer thisTimer = new TaskTimer( item ); 
			thisTimer.Start(); 
			
			///// MOVE THE SEARCH PEDESTALS //////////////////////////////////////
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Reconfigure Quest Pedestals..." ); }
			BuildQuests.SearchCreate();

			///// MAKE THE STEAL PEDS LOOK DIFFERENT /////////////////////////////
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Reconfigure Thief Pedestals..." ); }
			BuildPedestals.CreateStealPeds();
			
			///// CLEANUP THE CREATURES MASS SPREAD OUT IN THE LAND //////////////

			ArrayList targets = new ArrayList();
			ArrayList healers = new ArrayList();
			ArrayList exodus = new ArrayList();
			ArrayList serpent = new ArrayList();
			ArrayList gargoyle = new ArrayList();
			ArrayList cleanup = new ArrayList();
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Respawn The Mass Spawns..." ); }
			foreach ( Mobile creature in World.Mobiles.Values )
			{
				if ( creature is CodexGargoyleA || creature is CodexGargoyleB )
				{
					gargoyle.Add( creature );
				}
				else if ( creature is BaseCreature && creature.Map == Map.Internal )
				{
					if (((BaseCreature)creature).IsStabled){} // DO NOTHING
					else if ( creature is BaseMount && ((BaseMount)creature).Rider != null ){} // DO NOTHING
					else { cleanup.Add( creature ); }
				}
				else if ( creature.WhisperHue == 999 || creature.WhisperHue == 911 )
				{
					if ( creature != null )
					{
						if ( creature is Adventurers || creature is WanderingHealer || creature is Courier || creature is Syth || creature is Jedi ){ healers.Add( creature ); }
						else if ( creature is Exodus ){ exodus.Add( creature ); }
						else if ( creature is Jormungandr ){ serpent.Add( creature ); }
						else { targets.Add( creature ); }
					}
				}
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Remove Lost Boats..." ); }
			Server.Multis.BaseBoat.ClearShip(); // SAFETY CATCH TO CLEAR THE SHIPS OFF THE SEA

			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Respawn Sea..." ); }
			for ( int i = 0; i < targets.Count; ++i )
			{
				Mobile creature = ( Mobile )targets[ i ];
				if ( creature.Hidden == false )
				{
					if ( creature.WhisperHue == 911 )
					{
						Effects.SendLocationEffect( creature.Location, creature.Map, 0x3400, 60, 0x6E4, 0 );
						Effects.PlaySound( creature.Location, creature.Map, 0x108 );
					}
					else
					{
						creature.PlaySound( 0x026 );
						Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
					}
				}
				creature.Delete();
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Move Exodus..." ); }
			for ( int i = 0; i < exodus.Count; ++i )
			{
				Mobile creature = ( Mobile )exodus[ i ];
				Server.Misc.IntelligentAction.BurnAway( creature );
				Worlds.MoveToRandomDungeon( creature );
				Server.Misc.IntelligentAction.BurnAway( creature );
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Special Sea Serpent..." ); }
			for ( int i = 0; i < serpent.Count; ++i )
			{
				Mobile creature = ( Mobile )serpent[ i ];
				creature.PlaySound( 0x026 );
				Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
				Worlds.MoveToRandomOcean( creature );
				creature.PlaySound( 0x026 );
				Effects.SendLocationEffect( creature.Location, creature.Map, 0x352D, 16, 4 );
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Respawn Gargoyles..." ); }
			for ( int i = 0; i < gargoyle.Count; ++i )
			{
				Mobile creature = ( Mobile )gargoyle[ i ];
				Server.Misc.IntelligentAction.BurnAway( creature );
				Worlds.MoveToRandomDungeon( creature );
				Server.Misc.IntelligentAction.BurnAway( creature );
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Remove Lost Creatures..." ); }
			for ( int i = 0; i < cleanup.Count; ++i )
			{
				Mobile creature = ( Mobile )cleanup[ i ];
				creature.Delete();
			}
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Respawn Jedis, Healers, etc..." ); }
			for ( int i = 0; i < healers.Count; ++i )
			{
				Mobile healer = ( Mobile )healers[ i ];
				if ( !(healer is Citizens) )
				{
					Effects.SendLocationParticles( EffectItem.Create( healer.Location, healer.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
					healer.PlaySound( 0x1FE );
				}
				healer.Delete();
			}

			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Respawn Tavern Patrons..." ); }
			ArrayList drinkers = new ArrayList();
			foreach ( Mobile drunk in World.Mobiles.Values )
			if ( drunk is AdventurerWest || drunk is AdventurerSouth || drunk is AdventurerNorth || drunk is AdventurerEast || drunk is TavernPatronWest || drunk is TavernPatronSouth || drunk is TavernPatronNorth || drunk is TavernPatronEast )
			{
				if ( drunk != null )
				{
					drinkers.Add( drunk );
				}
			}
			for ( int i = 0; i < drinkers.Count; ++i )
			{
				Mobile drunk = ( Mobile )drinkers[ i ];
				drunk.Delete();
			}

			if ( MySettings.ConsoleLog ){ Console.WriteLine( "(Daily) Tasks Complete!" ); }
		}
	}
}
