using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Network;
using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Localization;

namespace Server.Misc
{
	public class Weather
	{
		private static Dictionary<Map, List<Weather>> m_WeatherByFacet = new Dictionary<Map, List<Weather>>();

		public static void Initialize()
		{
			/* Static weather:
			 * 
			 * Format:
			 *   AddWeather( map, temperature, chanceOfPercipitation, chanceOfExtremeTemperature, <area ...> );
			 */

			AddWeather( Map.Lodor, +15, 100, 0, new Rectangle2D( 298, 3461, 283, 239 ), new Rectangle2D( 322, 3689, 244, 114 ), new Rectangle2D( 6466, 3044, 553, 878 ) );
			AddWeather( Map.Lodor, -15, 100, 0, new Rectangle2D( 1965, 700, 177, 376 ), new Rectangle2D( 2035, 233, 1130, 676 ), new Rectangle2D( 2942, 311, 304, 650 ), new Rectangle2D( 3103, 943, 162, 123 ), new Rectangle2D( 3229, 187, 666, 989 ), new Rectangle2D( 3885, 343, 575, 844 ), new Rectangle2D( 4235, 1145, 249, 154 ) );
			AddWeather( Map.Lodor, +15, 25, 0, new Rectangle2D( 5154, 1099, 144, 310 ), new Rectangle2D( 6851, 115, 209, 214 ) );
			AddWeather( Map.Sosaria, +15, 25, 0, new Rectangle2D( 698, 3129, 1574, 961 ) );
			AddWeather( Map.Sosaria, +15, 25, 0, new Rectangle2D( 5122, 3035, 998, 1052 ) );
			AddWeather( Map.Sosaria, +15, 25, 0, new Rectangle2D( 6642, 82, 316, 273 ) );
			AddWeather( Map.Sosaria, +15, 25, 0, new Rectangle2D( 6908, 526, 252, 259 ) );
			AddWeather( Map.Sosaria, +15, 25, 0, new Rectangle2D( 6126, 828, 1035, 1911 ) );
			AddWeather( Map.Sosaria, -15, 100, 0, new Rectangle2D( 4514, 812, 333, 273 ), new Rectangle2D( 4233, 1076, 880, 339 ) );
			AddWeather( Map.IslesDread, -15, 100, 0, new Rectangle2D( 10, 10, 544, 551 ), new Rectangle2D( 255, 520, 288, 162 ) );
			AddWeather( Map.SavagedEmpire, -15, 100, 0, new Rectangle2D( 134, 4, 283, 128 ), new Rectangle2D( 411, 9, 206, 138 ), new Rectangle2D( 752, 4, 205, 110 ), new Rectangle2D( 908, 8, 188, 147 ), new Rectangle2D( 1075, 3, 92, 291 ) );
			AddWeather( Map.SavagedEmpire, +15, 25, 0, new Rectangle2D( 1000, 1866, 195, 135 ), new Rectangle2D( 0, 2048, 182, 416 ) );
			AddWeather( Map.SavagedEmpire, +15, 25, 0, new Rectangle2D( 193, 2557, 70, 100 ) );

			/* Dynamic weather:
			 * 
			 * Format:
			 *   AddDynamicWeather( map, temperature, chanceOfPercipitation, chanceOfExtremeTemperature, moveSpeed, width, height, bounds );
			 */

			for ( int i = 0; i < 15; ++i )
				AddDynamicWeather( Map.Lodor, +15, 100, 0, 8, 400, 400, new Rectangle2D( 0, 0, 5122, 4090 ) );

			for ( int i = 0; i < 15; ++i )
				AddDynamicWeather( Map.Sosaria, +15, 100, 0, 8, 400, 400, new Rectangle2D( 0, 0, 5122, 3130 ) );

			for ( int i = 0; i < 15; ++i )
				AddDynamicWeather( Map.SerpentIsland, +15, 100, 0, 4, 200, 200, new Rectangle2D( 0, 0, 1865, 2040 ) );

			for ( int i = 0; i < 15; ++i )
				AddDynamicWeather( Map.IslesDread, +15, 100, 0, 2, 100, 100, new Rectangle2D( 0, 0, 1445, 1445 ) );

			for ( int i = 0; i < 15; ++i )
				AddDynamicWeather( Map.SavagedEmpire, +15, 100, 0, 4, 100, 100, new Rectangle2D( 130, 0, 1032, 1794 ) );
		}

		public static List<Weather> GetWeatherList( Map facet )
		{
			if ( facet == null )
				return null;

			List<Weather> list = null;
			m_WeatherByFacet.TryGetValue( facet, out list );

			if ( list == null )
				m_WeatherByFacet[facet] = list = new List<Weather>();

			return list;
		}

		public static void AddDynamicWeather( Map map, int temperature, int chanceOfPercipitation, int chanceOfExtremeTemperature, int moveSpeed, int width, int height, Rectangle2D bounds )
		{
			Rectangle2D area = new Rectangle2D();
			bool isValid = false;

			for ( int j = 0; j < 10; ++j )
			{
				area = new Rectangle2D( bounds.X + Utility.Random( bounds.Width - width ), bounds.Y + Utility.Random( bounds.Height - height ), width, height );

				if ( !CheckWeatherConflict( map, null, area ) )
					isValid = true;

				if ( isValid )
					break;
			}

			if ( isValid )
			{
				Weather w = new Weather( map, new Rectangle2D[]{ area }, temperature, chanceOfPercipitation, chanceOfExtremeTemperature, TimeSpan.FromSeconds( 30.0 ) );

				w.m_Bounds = bounds;
				w.m_MoveSpeed = moveSpeed;
			}
		}

		public static void AddWeather( Map map, int temperature, int chanceOfPercipitation, int chanceOfExtremeTemperature, params Rectangle2D[] area )
		{
			new Weather( map, area, temperature, chanceOfPercipitation, chanceOfExtremeTemperature, TimeSpan.FromSeconds( 30.0 ) );
		}

		public static bool CheckWeatherConflict( Map facet, Weather exclude, Rectangle2D area )
		{
			List<Weather> list = GetWeatherList( facet );

			if ( list == null )
				return false;

			for ( int i = 0; i < list.Count; ++i )
			{
				Weather w = list[i];

				if ( w != exclude && w.IntersectsWith( area ) )
					return true;
			}

			return false;
		}

		private Map m_Facet;
		private Rectangle2D[] m_Area;
		private int m_Temperature;
		private int m_ChanceOfPercipitation;
		private int m_ChanceOfExtremeTemperature;

		public Map Facet{ get{ return m_Facet; } }
		public Rectangle2D[] Area{ get{ return m_Area; } set{ m_Area = value; } }
		public int Temperature{ get{ return m_Temperature; } set{ m_Temperature = value; } }
		public int ChanceOfPercipitation{ get{ return m_ChanceOfPercipitation; } set{ m_ChanceOfPercipitation = value; } }
		public int ChanceOfExtremeTemperature{ get{ return m_ChanceOfExtremeTemperature; } set{ m_ChanceOfExtremeTemperature = value; } }

		// For dynamic weather:
		private Rectangle2D m_Bounds;
		private int m_MoveSpeed;
		private int m_MoveAngleX, m_MoveAngleY;

		public Rectangle2D Bounds{ get{ return m_Bounds; } set{ m_Bounds = value; } }
		public int MoveSpeed{ get{ return m_MoveSpeed; } set{ m_MoveSpeed = value; } }
		public int MoveAngleX{ get{ return m_MoveAngleX; } set{ m_MoveAngleX = value; } }
		public int MoveAngleY{ get{ return m_MoveAngleY; } set{ m_MoveAngleY = value; } }

		public static bool CheckIntersection( Rectangle2D r1, Rectangle2D r2 )
		{
			if ( r1.X >= (r2.X + r2.Width) )
				return false;

			if ( r2.X >= (r1.X + r1.Width) )
				return false;

			if ( r1.Y >= (r2.Y + r2.Height) )
				return false;

			if ( r2.Y >= (r1.Y + r1.Height) )
				return false;

			return true;
		}

		public static bool CheckContains( Rectangle2D big, Rectangle2D small )
		{
			if ( small.X < big.X )
				return false;

			if ( small.Y < big.Y )
				return false;

			if ( (small.X + small.Width) > (big.X + big.Width) )
				return false;

			if ( (small.Y + small.Height) > (big.Y + big.Height) )
				return false;

			return true;
		}

		public virtual bool IntersectsWith( Rectangle2D area )
		{
			for ( int i = 0; i < m_Area.Length; ++i )
			{
				if ( CheckIntersection( area, m_Area[i] ) )
					return true;
			}

			return false;
		}

		public Weather( Map facet, Rectangle2D[] area, int temperature, int chanceOfPercipitation, int chanceOfExtremeTemperature, TimeSpan interval )
		{
			m_Facet = facet;
			m_Area = area;
			m_Temperature = temperature;
			m_ChanceOfPercipitation = chanceOfPercipitation;
			m_ChanceOfExtremeTemperature = chanceOfExtremeTemperature;
			m_Interval = interval;

			List<Weather> list = GetWeatherList( facet );

			if ( list != null )
				list.Add( this );

			m_WeatherTimer = Timer.DelayCall( TimeSpan.FromSeconds( (0.2+(Utility.RandomDouble()*0.8)) * interval.TotalSeconds ), interval, new TimerCallback( OnTick ) );
		}

		protected Timer m_WeatherTimer;
		protected TimeSpan m_Interval;
		protected bool m_IsTemporary;

		protected void StopWeatherTimer()
		{
			if ( m_WeatherTimer != null )
			{
				m_WeatherTimer.Stop();
				m_WeatherTimer = null;
			}
		}

		protected void RestartWeatherTimer( TimeSpan initialDelay )
		{
			StopWeatherTimer();
			m_WeatherTimer = Timer.DelayCall( initialDelay, m_Interval, new TimerCallback( OnTick ) );
		}

		public void RemoveFromFacet()
		{
			List<Weather> list = GetWeatherList( m_Facet );

			if ( list != null )
				list.Remove( this );
		}

		public bool ContainsLocation( Point3D loc )
		{
			if ( m_Area.Length == 0 )
				return true;

			for ( int i = 0; i < m_Area.Length; ++i )
			{
				if ( m_Area[i].Contains( loc ) )
					return true;
			}

			return false;
		}

		protected void SendWeatherPacket( int type, int density, int temperature )
		{
			List<NetState> states = NetState.Instances;
			Packet weatherPacket = null;

			for ( int i = 0; i < states.Count; ++i )
			{
				NetState ns = states[i];
				Mobile mob = ns.Mobile;

				if ( mob == null || mob.Map != m_Facet )
					continue;

				if ( !ContainsLocation( mob.Location ) )
					continue;

				if ( weatherPacket == null )
					weatherPacket = Packet.Acquire( new Server.Network.Weather( type, density, temperature ) );

				ns.Send( weatherPacket );
			}

			Packet.Release( weatherPacket );
		}

		public static Rectangle2D CreateAreaAt( Mobile from, int size )
		{
			int half = size / 2;
			return new Rectangle2D( from.X - half, from.Y - half, size, size );
		}

		public virtual void Reposition()
		{
			if ( m_Area.Length == 0 )
				return;

			int width = m_Area[0].Width;
			int height = m_Area[0].Height;

			Rectangle2D area = new Rectangle2D();
			bool isValid = false;

			for ( int j = 0; j < 10; ++j )
			{
				area = new Rectangle2D( m_Bounds.X + Utility.Random( m_Bounds.Width - width ), m_Bounds.Y + Utility.Random( m_Bounds.Height - height ), width, height );

				if ( !CheckWeatherConflict( m_Facet, this, area ) )
					isValid = true;

				if ( isValid )
					break;
			}

			if ( !isValid )
				return;

			m_Area[0] = area;
		}

		public virtual void RecalculateMovementAngle()
		{
			double angle = Utility.RandomDouble() * Math.PI * 2.0;

			double cos = Math.Cos( angle );
			double sin = Math.Sin( angle );

			m_MoveAngleX = (int)(100 * cos);
			m_MoveAngleY = (int)(100 * sin);
		}

		public virtual void MoveForward()
		{
			if ( m_Area.Length == 0 )
				return;

			for ( int i = 0; i < 5; ++i ) // try 5 times to find a valid spot
			{
				int xOffset = (m_MoveSpeed * m_MoveAngleX) / 100;
				int yOffset = (m_MoveSpeed * m_MoveAngleY) / 100;

				Rectangle2D oldArea = m_Area[0];
				Rectangle2D newArea = new Rectangle2D( oldArea.X + xOffset, oldArea.Y + yOffset, oldArea.Width, oldArea.Height );

				if ( !CheckWeatherConflict( m_Facet, this, newArea ) && CheckContains( m_Bounds, newArea ) )
				{
					m_Area[0] = newArea;
					break;
				}

				RecalculateMovementAngle();
			}
		}

		private int m_Stage;
		protected bool m_Active;
		private bool m_ExtremeTemperature;

		public virtual void OnTick()
		{
			if ( m_Stage == 0 )
			{
				m_Active = ( m_ChanceOfPercipitation > Utility.Random( 100 ) );
				m_ExtremeTemperature = ( m_ChanceOfExtremeTemperature > Utility.Random( 100 ) );

				if ( m_MoveSpeed > 0 )
				{
					Reposition();
					RecalculateMovementAngle();
				}
			}

			if ( m_Active )
			{
				if ( m_Stage > 0 && m_MoveSpeed > 0 )
					MoveForward();

				int type, density, temperature;

				temperature = m_Temperature;

				if ( m_ExtremeTemperature )
					temperature *= -1;

				if ( m_Stage < 15 )
				{
					density = m_Stage * 5;
				}
				else
				{
					density = 150 - (m_Stage * 5);

					if ( density < 10 )
						density = 10;
					else if ( density > 70 )
						density = 70;
				}

				if ( density == 0 )
					type = 0xFE;
				else if ( temperature > 0 )
					type = 0;
				else
					type = 2;

				SendWeatherPacket( type, density, temperature );
			}

			m_Stage++;
			m_Stage %= 30;
		}
	}

	public class TemporaryWeather : Weather
	{
		private static readonly List<TemporaryWeather> m_TemporaryWeather = new List<TemporaryWeather>();

		public static IEnumerable<TemporaryWeather> All { get { return m_TemporaryWeather; } }

		private Timer m_CleanupTimer;
		private int m_CustomDensity;
		private bool m_UseCustomDensity;

		public TemporaryWeather( Map facet, Rectangle2D[] area, int temperature, int chanceOfPercipitation, int chanceOfExtremeTemperature, TimeSpan interval, TimeSpan duration )
			: base( facet, area, temperature, chanceOfPercipitation, chanceOfExtremeTemperature, interval )
		{
			m_IsTemporary = true;
			m_TemporaryWeather.Add( this );
			RestartWeatherTimer( TimeSpan.FromSeconds( 0.5 ) );
			m_CleanupTimer = Timer.DelayCall( duration, new TimerCallback( Cleanup ) );
		}

		public TemporaryWeather( Map facet, Rectangle2D[] area, int temperature, int customDensity, TimeSpan interval, TimeSpan duration )
			: base( facet, area, temperature, 100, 0, interval )
		{
			m_IsTemporary = true;
			m_UseCustomDensity = true;
			m_CustomDensity = customDensity;
			m_Active = true;
			m_TemporaryWeather.Add( this );
			RestartWeatherTimer( TimeSpan.FromSeconds( 0.5 ) );
			m_CleanupTimer = Timer.DelayCall( duration, new TimerCallback( Cleanup ) );
		}

		public override void OnTick()
		{
			if ( m_UseCustomDensity )
			{
				if ( !m_Active )
					return;

				int density = m_CustomDensity;
				int temperature = this.Temperature;
				int type;

				if ( density == 0 )
					type = 0xFE;
				else if ( temperature > 0 )
					type = 0;
				else
					type = 2;

				SendWeatherPacket( type, density, temperature );
				return;
			}

			base.OnTick();
		}

		public void Cleanup()
		{
			if ( m_CleanupTimer != null )
			{
				m_CleanupTimer.Stop();
				m_CleanupTimer = null;
			}

			StopWeatherTimer();
			RemoveFromFacet();
			m_TemporaryWeather.Remove( this );
			SendWeatherPacket( 0xFE, 0, 0 );
		}

		public static int StopAt( Mobile from )
		{
			Map map = from.Map;

			if ( map == null || map == Map.Internal )
				return 0;

			List<TemporaryWeather> toRemove = new List<TemporaryWeather>();

			for ( int i = 0; i < m_TemporaryWeather.Count; ++i )
			{
				TemporaryWeather w = m_TemporaryWeather[i];

				if ( w.Facet != map )
					continue;

				if ( w.ContainsLocation( from.Location ) )
					toRemove.Add( w );
			}

			for ( int i = 0; i < toRemove.Count; ++i )
				toRemove[i].Cleanup();

			return toRemove.Count;
		}

		public static TemporaryWeather CreateCommandWeather( Mobile from, int temperature, int size, TimeSpan duration )
		{
			Map map = from.Map;

			if ( map == null || map == Map.Internal )
				return null;

			Rectangle2D area = CreateAreaAt( from, size );
			TimeSpan interval = TimeSpan.FromSeconds( 30.0 );

			return new TemporaryWeather( map, new Rectangle2D[] { area }, temperature, 100, 0, interval, duration );
		}

		public static TemporaryWeather CreateUiWeather( Mobile from, int temperature, int density, int size, int durationMinutes )
		{
			Map map = from.Map;

			if ( map == null || map == Map.Internal )
				return null;

			Rectangle2D area = CreateAreaAt( from, size );
			TimeSpan interval = TimeSpan.FromSeconds( 30.0 );
			TimeSpan duration = TimeSpan.FromMinutes( durationMinutes );

			return new TemporaryWeather( map, new Rectangle2D[] { area }, temperature, density, interval, duration );
		}
	}

	public class WeatherCommands
    {
		private const int DefaultAreaSize = 50;
		private static readonly TimeSpan DefaultDuration = TimeSpan.FromMinutes( 5.0 );

		public static void Initialize()
		{
            CommandSystem.Register( "weatherzones", AccessLevel.Administrator, new CommandEventHandler( WeatherZones_OnCommand ) );
            CommandSystem.Register( "rain", AccessLevel.GameMaster, new CommandEventHandler( Rain_OnCommand ) );
            CommandSystem.Register( "snow", AccessLevel.GameMaster, new CommandEventHandler( Snow_OnCommand ) );
            CommandSystem.Register( "storm", AccessLevel.GameMaster, new CommandEventHandler( Storm_OnCommand ) );
            CommandSystem.Register( "stopweather", AccessLevel.GameMaster, new CommandEventHandler( StopWeather_OnCommand ) );
		}

		public static void Register( string command, AccessLevel access, CommandEventHandler handler )
		{
            CommandSystem.Register(command, access, handler);
		}

		[Usage( "weatherzones" )]
		[Description( "Lists permanent weather zones on the current map." )]
		public static void WeatherZones_OnCommand( CommandEventArgs e )
        {
			Mobile from = e.Mobile;
			Map facet = from.Map;

			if ( facet == null )
				return;

			List<Weather> list = Weather.GetWeatherList( facet );

			for ( int i = 0; i < list.Count; ++i )
			{
				Weather w = list[i];

				if ( w is TemporaryWeather )
					continue;

				for ( int j = 0; j < w.Area.Length; ++j )
				{
					Rectangle2D area = w.Area[j];
					from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "weatherzones.entry", area.X, area.Y, area.Width, area.Height ) );
				}
			}
		}

		private static bool TryGetMap( Mobile from )
		{
			if ( from.Map != null && from.Map != Map.Internal )
				return true;

			from.SendMessage( StringCatalog.ResolveByKey( from.Account, "weather.cmd.no_map" ) );
			return false;
		}

		[Usage( "rain" )]
		[Description( "Creates temporary rain centered on your location." )]
		public static void Rain_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( !TryGetMap( from ) )
				return;

			TemporaryWeather.CreateCommandWeather( from, +15, DefaultAreaSize, DefaultDuration );
		}

		[Usage( "snow" )]
		[Description( "Creates temporary snow centered on your location." )]
		public static void Snow_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( !TryGetMap( from ) )
				return;

			TemporaryWeather.CreateCommandWeather( from, -15, DefaultAreaSize, DefaultDuration );
		}

		[Usage( "storm" )]
		[Description( "Creates temporary storm centered on your location." )]
		public static void Storm_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( !TryGetMap( from ) )
				return;

			TemporaryWeather.CreateCommandWeather( from, +25, DefaultAreaSize, DefaultDuration );
		}

		[Usage( "stopweather" )]
		[Description( "Stops temporary weather at your location." )]
		public static void StopWeather_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			int stopped = TemporaryWeather.StopAt( from );

			if ( stopped > 0 )
				from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "weather.cmd.stopped", stopped ) );
			else
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "weather.cmd.no_temp_weather" ) );
		}
	}
}

namespace Server.Gumps
{
	public class WeatherControlGump : Gump
	{
		private const string TitleColor = "#FFD700";
		private const string LabelColor = "#FFFFFF";
		private const string HintColor = "#999999";

		private const int BtnClose = 0;
		private const int BtnCreate = 1;
		private const int BtnStop = 2;
		private const int BtnTempMinus = 10;
		private const int BtnTempPlus = 11;
		private const int BtnDensityMinus = 12;
		private const int BtnDensityPlus = 13;
		private const int BtnAreaMinus = 14;
		private const int BtnAreaPlus = 15;
		private const int BtnDurationMinus = 16;
		private const int BtnDurationPlus = 17;
		private const int BtnPresetLightRain = 20;
		private const int BtnPresetHeavyStorm = 21;
		private const int BtnPresetLightSnow = 22;
		private const int BtnPresetBlizzard = 23;
		private const int BtnTypeRain = 100;
		private const int BtnTypeSnow = 101;
		private const int BtnTypeStorm = 102;
		private const int BtnTypeClear = 103;

		public static void Initialize()
		{
			CommandSystem.Register( "weather", AccessLevel.GameMaster, new CommandEventHandler( Weather_OnCommand ) );
			CommandSystem.Register( "weatherui", AccessLevel.GameMaster, new CommandEventHandler( WeatherUI_OnCommand ) );
		}

		[Usage( "weather" )]
		[Description( "Opens the weather control panel." )]
		private static void Weather_OnCommand( CommandEventArgs e )
		{
			OpenFor( e.Mobile );
		}

		[Usage( "weatherui" )]
		[Description( "Opens the weather control panel." )]
		private static void WeatherUI_OnCommand( CommandEventArgs e )
		{
			OpenFor( e.Mobile );
		}

		public static void OpenFor( Mobile from )
		{
			if ( from == null )
				return;

			from.SendGump( new WeatherControlGump( from ) );
		}

		private readonly Mobile m_From;
		private int m_SelectedWeatherType;
		private int m_Temperature;
		private int m_Density;
		private int m_AreaSize;
		private int m_Duration;

		public WeatherControlGump( Mobile from )
			: this( from, 0, 10, 30, 50, 5 )
		{
		}

		public WeatherControlGump( Mobile from, int weatherType, int temperature, int density, int areaSize, int duration )
			: base( 50, 50 )
		{
			m_From = from;
			m_SelectedWeatherType = weatherType;
			m_Temperature = temperature;
			m_Density = density;
			m_AreaSize = areaSize;
			m_Duration = duration;

			from.CloseGump( typeof( WeatherControlGump ) );

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 420, 480, 9270 );
			AddAlphaRegion( 10, 10, 400, 460 );

			AddButton( 380, 15, 4017, 4018, BtnClose, GumpButtonType.Reply, 0 );
			AddHtml( 20, 15, 360, 25, Html( TitleColor, Center( W( "weather.gump.title" ) ) ), false, false );
			AddHtml( 20, 50, 380, 20, Html( LabelColor, W( "weather.gump.section.type" ) ), false, false );

			AddWeatherTypeButton( 0, W( "weather.gump.type.rain" ), 20, 75 );
			AddWeatherTypeButton( 1, W( "weather.gump.type.snow" ), 120, 75 );
			AddWeatherTypeButton( 2, W( "weather.gump.type.storm" ), 220, 75 );
			AddWeatherTypeButton( 3, W( "weather.gump.type.clear" ), 320, 75 );

			AddHtml( 20, 115, 380, 20, Html( LabelColor, W( "weather.gump.section.params" ) ), false, false );
			AddParameterRow(
				Html( LabelColor, StringCatalog.ResolveFormatByKey( m_From.Account, "weather.gump.param.temperature", m_Temperature ) ),
				Html( HintColor, W( "weather.gump.param.temperature.hint" ) ),
				BtnTempMinus, BtnTempPlus, 140 );
			AddParameterRow(
				Html( LabelColor, StringCatalog.ResolveFormatByKey( m_From.Account, "weather.gump.param.density", m_Density ) ),
				Html( HintColor, W( "weather.gump.param.density.hint" ) ),
				BtnDensityMinus, BtnDensityPlus, 190 );
			AddParameterRow(
				Html( LabelColor, StringCatalog.ResolveFormatByKey( m_From.Account, "weather.gump.param.area", m_AreaSize ) ),
				Html( HintColor, W( "weather.gump.param.area.hint" ) ),
				BtnAreaMinus, BtnAreaPlus, 240 );
			AddParameterRow(
				Html( LabelColor, StringCatalog.ResolveFormatByKey( m_From.Account, "weather.gump.param.duration", m_Duration ) ),
				Html( HintColor, W( "weather.gump.param.duration.hint" ) ),
				BtnDurationMinus, BtnDurationPlus, 290 );

			AddHtml( 20, 340, 380, 20, Html( LabelColor, W( "weather.gump.section.presets" ) ), false, false );
			AddButton( 20, 365, 4005, 4007, BtnPresetLightRain, GumpButtonType.Reply, 0 );
			AddHtml( 55, 368, 120, 20, Html( LabelColor, W( "weather.gump.preset.light_rain" ) ), false, false );
			AddButton( 210, 365, 4005, 4007, BtnPresetHeavyStorm, GumpButtonType.Reply, 0 );
			AddHtml( 245, 368, 140, 20, Html( LabelColor, W( "weather.gump.preset.heavy_storm" ) ), false, false );
			AddButton( 20, 395, 4005, 4007, BtnPresetLightSnow, GumpButtonType.Reply, 0 );
			AddHtml( 55, 398, 120, 20, Html( LabelColor, W( "weather.gump.preset.light_snow" ) ), false, false );
			AddButton( 210, 395, 4005, 4007, BtnPresetBlizzard, GumpButtonType.Reply, 0 );
			AddHtml( 245, 398, 120, 20, Html( LabelColor, W( "weather.gump.preset.blizzard" ) ), false, false );

			AddButton( 40, 435, 4005, 4007, BtnCreate, GumpButtonType.Reply, 0 );
			AddHtml( 75, 438, 120, 20, Html( LabelColor, W( "weather.gump.action.create" ) ), false, false );
			AddButton( 240, 435, 4017, 4019, BtnStop, GumpButtonType.Reply, 0 );
			AddHtml( 275, 438, 120, 20, Html( LabelColor, W( "weather.gump.action.stop" ) ), false, false );
		}

		private string W( string key )
		{
			string lang = AccountLang.GetLanguageCode( m_From != null ? m_From.Account : null );
			string s = StringCatalog.TryResolveByKey( lang, key );
			return ( s != null && s.Length > 0 ) ? s : key;
		}

		private static string Center( string text )
		{
			return string.Format( "<CENTER>{0}</CENTER>", text );
		}

		private static string Html( string color, string text )
		{
			return string.Format( "<BODY><BASEFONT Color={0}>{1}</BASEFONT></BODY>", color, text );
		}

		private void AddWeatherTypeButton( int type, string label, int x, int y )
		{
			AddButton( x, y, 4005, 4007, BtnTypeRain + type, GumpButtonType.Reply, 0 );
			AddLabel( x + 35, y + 3, m_SelectedWeatherType == type ? 53 : 1153, label );
		}

		private void AddParameterRow( string label, string hint, int minusId, int plusId, int y )
		{
			AddHtml( 20, y, 220, 20, label, false, false );
			AddButton( 260, y, 4014, 4016, minusId, GumpButtonType.Reply, 0 );
			AddButton( 300, y, 4005, 4007, plusId, GumpButtonType.Reply, 0 );
			AddHtml( 20, y + 18, 360, 20, hint, false, false );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = m_From;

			if ( from == null )
				return;

			switch ( info.ButtonID )
			{
				case BtnClose:
					return;
				case BtnCreate:
					CreateWeather( from );
					from.SendGump( new WeatherControlGump( from, m_SelectedWeatherType, m_Temperature, m_Density, m_AreaSize, m_Duration ) );
					return;
				case BtnStop:
					StopWeather( from );
					from.SendGump( new WeatherControlGump( from, m_SelectedWeatherType, m_Temperature, m_Density, m_AreaSize, m_Duration ) );
					return;
				case BtnTempMinus:
					m_Temperature = Math.Max( -50, m_Temperature - 5 );
					break;
				case BtnTempPlus:
					m_Temperature = Math.Min( 50, m_Temperature + 5 );
					break;
				case BtnDensityMinus:
					m_Density = Math.Max( 0, m_Density - 10 );
					break;
				case BtnDensityPlus:
					m_Density = Math.Min( 70, m_Density + 10 );
					break;
				case BtnAreaMinus:
					m_AreaSize = Math.Max( 10, m_AreaSize - 10 );
					break;
				case BtnAreaPlus:
					m_AreaSize = Math.Min( 200, m_AreaSize + 10 );
					break;
				case BtnDurationMinus:
					m_Duration = Math.Max( 1, m_Duration - 1 );
					break;
				case BtnDurationPlus:
					m_Duration = Math.Min( 60, m_Duration + 1 );
					break;
				case BtnPresetLightRain:
					ApplyPreset( 0, 10, 30, 50, 5 );
					break;
				case BtnPresetHeavyStorm:
					ApplyPreset( 2, 25, 70, 80, 10 );
					break;
				case BtnPresetLightSnow:
					ApplyPreset( 1, -10, 30, 50, 5 );
					break;
				case BtnPresetBlizzard:
					ApplyPreset( 1, -25, 70, 100, 15 );
					break;
				case BtnTypeRain:
					SelectWeatherType( 0, Math.Max( m_Temperature, 10 ) );
					break;
				case BtnTypeSnow:
					SelectWeatherType( 1, Math.Min( m_Temperature, -10 ) );
					break;
				case BtnTypeStorm:
					SelectWeatherType( 2, Math.Max( m_Temperature, 20 ) );
					break;
				case BtnTypeClear:
					SelectWeatherType( 3, m_Temperature );
					m_Density = 0;
					break;
				default:
					return;
			}

			from.SendGump( new WeatherControlGump( from, m_SelectedWeatherType, m_Temperature, m_Density, m_AreaSize, m_Duration ) );
		}

		private void ApplyPreset( int weatherType, int temperature, int density, int areaSize, int duration )
		{
			m_SelectedWeatherType = weatherType;
			m_Temperature = temperature;
			m_Density = density;
			m_AreaSize = areaSize;
			m_Duration = duration;
		}

		private void SelectWeatherType( int weatherType, int temperature )
		{
			m_SelectedWeatherType = weatherType;
			m_Temperature = temperature;
		}

		private void CreateWeather( Mobile from )
		{
			if ( from.Map == null || from.Map == Map.Internal )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "weather.cmd.no_map" ) );
				return;
			}

			int temperature = m_Temperature;
			int density = m_Density;

			switch ( m_SelectedWeatherType )
			{
				case 0:
					if ( temperature <= 0 )
						temperature = 10;
					break;
				case 1:
					if ( temperature >= 0 )
						temperature = -10;
					break;
				case 2:
					if ( temperature < 20 )
						temperature = 25;
					break;
				case 3:
					density = 0;
					break;
			}

			Server.Misc.TemporaryWeather.CreateUiWeather( from, temperature, density, m_AreaSize, m_Duration );

			string typeKey = "weather.cmd.type.rain";

			switch ( m_SelectedWeatherType )
			{
				case 1: typeKey = "weather.cmd.type.snow"; break;
				case 2: typeKey = "weather.cmd.type.storm"; break;
				case 3: typeKey = "weather.cmd.type.clear"; break;
			}

			string typeName = StringCatalog.ResolveByKey( from.Account, typeKey );
			from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "weather.cmd.created", typeName, m_AreaSize, m_Duration ) );
		}

		private void StopWeather( Mobile from )
		{
			int stopped = Server.Misc.TemporaryWeather.StopAt( from );

			if ( stopped > 0 )
				from.SendMessage( StringCatalog.ResolveFormatByKey( from.Account, "weather.cmd.stopped", stopped ) );
			else
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "weather.cmd.no_temp_weather" ) );
		}
	}
}