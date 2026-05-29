using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Network;
using Server.Localization;
using Server.Mobiles;
using Server.Misc;

namespace Server.Gumps
{
	public class OathStatusGump : Gump
	{
		private const int GumpWidth = 280;
		private const int GumpHeight = 130;
		private const int ProgressBarSegments = 8;

		private readonly PlayerMobile m_Owner;
		private readonly Serial m_GuardSerial;

		private bool m_Closed;
		private Timer m_RefreshTimer;

		private static readonly Dictionary<int, int> s_OathWindowByPlayer = new Dictionary<int, int>();
		private static readonly HashSet<int> s_DismissedByPlayer = new HashSet<int>();
		private static bool s_CommandRegistered;
		private static readonly object s_Lock = new object();

		public static void EnsureCommandRegistered()
		{
			lock ( s_Lock )
			{
				if ( s_CommandRegistered )
					return;

				CommandSystem.Register( "oathstatus", AccessLevel.Player, new CommandEventHandler( OnOathStatusCommand ) );
				s_CommandRegistered = true;
			}
		}

		private static void OnOathStatusCommand( CommandEventArgs e )
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;
			if ( pm == null )
				return;

			TownGuards guard = FindGuardForCommand( pm );

			if ( guard == null )
			{
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "guard.oathbreak.gump.no_active_window" ) );
				return;
			}

			lock ( s_Lock )
			{
				s_DismissedByPlayer.Remove( pm.Serial.Value );
			}

			OpenOrRefresh( pm, guard );
		}

		private static TownGuards FindGuardForCommand( PlayerMobile pm )
		{
			int guardSerialValue;
			lock ( s_Lock )
			{
				if ( s_OathWindowByPlayer.TryGetValue( pm.Serial.Value, out guardSerialValue ) && guardSerialValue != 0 )
				{
					TownGuards tracked = World.FindMobile( (Serial)guardSerialValue ) as TownGuards;
					if ( tracked != null && !tracked.Deleted && tracked.IsOathEngaged )
						return tracked;
				}
			}

			TownGuards nearest = null;
			int bestDist = int.MaxValue;

			foreach ( Mobile m in pm.GetMobilesInRange( 24 ) )
			{
				TownGuards g = m as TownGuards;
				if ( g == null || g.Deleted || !g.IsOathEngaged )
					continue;

				int dist = pm.GetDistanceToSqrt( g );
				if ( dist < bestDist )
				{
					bestDist = dist;
					nearest = g;
				}
			}

			return nearest;
		}

		public static void ClearDismissed( PlayerMobile pm )
		{
			if ( pm == null )
				return;

			lock ( s_Lock )
			{
				s_DismissedByPlayer.Remove( pm.Serial.Value );
			}
		}

		public static void NotifyAssailant( PlayerMobile pm, TownGuards guard )
		{
			if ( pm == null || pm.Deleted || guard == null || guard.Deleted )
				return;

			lock ( s_Lock )
			{
				if ( s_DismissedByPlayer.Contains( pm.Serial.Value ) )
					return;
			}

			OpenOrRefresh( pm, guard );
		}

		public static void RefreshAllForGuard( TownGuards guard )
		{
			if ( guard == null || guard.Deleted )
				return;

			foreach ( PlayerMobile pm in guard.GetOathAssailants() )
				NotifyAssailant( pm, guard );
		}

		public static void CloseForPlayer( PlayerMobile pm )
		{
			if ( pm == null )
				return;

			pm.CloseGump( typeof( OathStatusGump ) );

			lock ( s_Lock )
			{
				s_OathWindowByPlayer.Remove( pm.Serial.Value );
			}
		}

		public static void CloseAllForGuard( TownGuards guard )
		{
			if ( guard == null )
				return;

			foreach ( PlayerMobile pm in guard.GetOathAssailants() )
				CloseForPlayer( pm );
		}

		private static void OpenOrRefresh( PlayerMobile pm, TownGuards guard )
		{
			EnsureCommandRegistered();

			lock ( s_Lock )
			{
				s_OathWindowByPlayer[pm.Serial.Value] = guard.Serial.Value;
			}

			pm.CloseGump( typeof( OathStatusGump ) );
			pm.SendGump( new OathStatusGump( pm, guard.Serial ) );
		}

		public OathStatusGump( PlayerMobile owner, Serial guardSerial ) : base( 0, 0 )
		{
			EnsureCommandRegistered();

			m_Owner = owner;
			m_GuardSerial = guardSerial;

			lock ( s_Lock )
			{
				s_OathWindowByPlayer[owner.Serial.Value] = guardSerial.Value;
			}

			Closable = true;
			Disposable = true;
			Resizable = false;

			AddPage( 0 );

			AddBackground( 0, 0, GumpWidth, GumpHeight, 0x1453 );

			TownGuards guard = World.FindMobile( m_GuardSerial ) as TownGuards;
			string title = BuildTitle( owner, guard );
			AddLabel( 10, 10, 0x7FFF, title );

			RenderContents( guard );

			AddButton( 10, GumpHeight - 25, 0xFB1, 0xFB2, 1, GumpButtonType.Reply, 0 );
			AddLabel( 50, GumpHeight - 25, 0x7FFF, StringCatalog.ResolveByKey( owner.Account, "guard.oathbreak.gump.close" ) );

			m_RefreshTimer = new RefreshTimer( this );
			m_RefreshTimer.Start();
		}

		private static string BuildTitle( PlayerMobile owner, TownGuards guard )
		{
			string guardName = guard != null && !string.IsNullOrEmpty( guard.Name )
				? guard.Name
				: StringCatalog.ResolveByKey( owner.Account, "guard.oathbreak.gump.guard_fallback" );

			return StringCatalog.ResolveFormatByKey( owner.Account, "guard.oathbreak.gump.title.format", guardName );
		}

		private void RenderContents( TownGuards guard )
		{
			if ( ShouldAutoClose( guard ) )
			{
				string ended = StringCatalog.ResolveByKey( m_Owner.Account, "guard.oathbreak.gump.ended" );
				AddHtml( 10, 38, 260, 60, "<BODY><BASEFONT Color=\"#FF8080\">" + ended + "</BASEFONT></BODY>", false, false );
				return;
			}

			int required = TownGuards.OathKnockdownsRequired;
			int breaks = guard.OathBreaks;
			int assailants = guard.OathAssailantCount;

			string progressText = StringCatalog.ResolveFormatByKey(
				m_Owner.Account,
				"guard.oathbreak.gump.progress.format",
				Math.Min( breaks, required ),
				required );

			string barHtml = BuildProgressBarHtml( breaks, required );
			AddHtml( 10, 32, 260, 22, "<BODY><BASEFONT Color=\"#7FFF00\">" + progressText + "</BASEFONT></BODY>", false, false );
			AddHtml( 10, 50, 260, 18, "<BODY><BASEFONT Color=\"#ADADAD\">" + barHtml + "</BASEFONT></BODY>", false, false );

			if ( guard.HasActiveOathWindow )
			{
				TimeSpan left = guard.GetOathWindowTimeRemaining();
				string timeColor = left.TotalSeconds < 60 ? "#FF4040" : "#FFFF00";
				string timeText = StringCatalog.ResolveFormatByKey(
					m_Owner.Account,
					"guard.oathbreak.gump.time_left.format",
					left.ToString( @"mm\:ss", CultureInfo.InvariantCulture ) );

				AddHtml( 10, 70, 260, 22, "<BODY><BASEFONT Color=\"" + timeColor + "\">" + timeText + "</BASEFONT></BODY>", false, false );
			}
			else
			{
				string pending = StringCatalog.ResolveByKey( m_Owner.Account, "guard.oathbreak.gump.pending_window" );
				AddHtml( 10, 70, 260, 22, "<BODY><BASEFONT Color=\"#FFFF00\">" + pending + "</BASEFONT></BODY>", false, false );
			}

			string attackersText = StringCatalog.ResolveFormatByKey(
				m_Owner.Account,
				"guard.oathbreak.gump.attackers.format",
				assailants );

			AddHtml( 10, 92, 260, 22, "<BODY><BASEFONT Color=\"#7FFF00\">" + attackersText + "</BASEFONT></BODY>", false, false );
		}

		private static string BuildProgressBarHtml( int breaks, int required )
		{
			if ( required <= 0 )
				required = TownGuards.OathKnockdownsRequired;

			int filled = ( breaks * ProgressBarSegments ) / required;
			if ( filled > ProgressBarSegments )
				filled = ProgressBarSegments;
			if ( filled < 0 )
				filled = 0;

			var sb = new StringBuilder( ProgressBarSegments * 2 );
			for ( int i = 0; i < ProgressBarSegments; i++ )
				sb.Append( i < filled ? "█" : "░" );

			return sb.ToString();
		}

		private bool ShouldAutoClose( TownGuards guard )
		{
			if ( guard == null || guard.Deleted || !guard.Alive )
				return true;

			if ( !guard.IsOathEngaged )
				return true;

			if ( m_Owner.Map != guard.Map )
				return true;

			if ( m_Owner.Region != guard.Region )
				return true;

			return false;
		}

		private void Refresh()
		{
			if ( m_Closed )
				return;

			TownGuards guard = World.FindMobile( m_GuardSerial ) as TownGuards;

			if ( ShouldAutoClose( guard ) )
			{
				CloseSelf();
				return;
			}

			m_Closed = true;

			if ( m_RefreshTimer != null )
			{
				m_RefreshTimer.Stop();
				m_RefreshTimer = null;
			}

			m_Owner.CloseGump( typeof( OathStatusGump ) );
			m_Owner.SendGump( new OathStatusGump( m_Owner, m_GuardSerial ) );
		}

		private void CloseSelf()
		{
			m_Closed = true;

			if ( m_RefreshTimer != null )
			{
				m_RefreshTimer.Stop();
				m_RefreshTimer = null;
			}

			CloseForPlayer( m_Owner );
		}

		private class RefreshTimer : Timer
		{
			private readonly OathStatusGump m_Gump;

			public RefreshTimer( OathStatusGump gump ) : base( TimeSpan.FromSeconds( 5 ), TimeSpan.FromSeconds( 5 ) )
			{
				m_Gump = gump;
				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				m_Gump.Refresh();
			}
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( m_Closed )
				return;

			if ( info.ButtonID == 1 )
			{
				m_Closed = true;

				if ( m_RefreshTimer != null )
				{
					m_RefreshTimer.Stop();
					m_RefreshTimer = null;
				}

				lock ( s_Lock )
				{
					s_DismissedByPlayer.Add( m_Owner.Serial.Value );
					s_OathWindowByPlayer.Remove( m_Owner.Serial.Value );
				}

				m_Owner.CloseGump( typeof( OathStatusGump ) );
			}
		}
	}
}
