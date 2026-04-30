using System;
using Server.Accounting;
using Server.Commands;
using Server.Gumps;
using Server.Localization;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.MLQuests.Gumps
{
	/// <summary>
	/// GM: cancel in-progress ML quests or clear completion/cooldown records (for testing repeat offers, e.g. restart delay).
	/// </summary>
	public class GmClientMLQuestRemovalGump : Gump
	{
		private const int BtnActiveBase = 100;
		private const int BtnDoneBase = 600;

		private readonly PlayerMobile m_Target;

		public GmClientMLQuestRemovalGump( Mobile staff, PlayerMobile target )
			: base( 50, 50 )
		{
			m_Target = target;

			Closable = true;
			Disposable = true;
			Resizable = false;

			Account staffAcct = staff.Account as Account;
			string lang = ( staffAcct != null ) ? AccountLang.GetLanguageCode( staffAcct ) : "en";

			MLQuestContext ctx = MLQuestSystem.GetContext( m_Target );
			int activeCount = ( ctx != null ) ? ctx.QuestInstances.Count : 0;
			int doneCount = ( ctx != null ) ? ctx.DoneQuestRecordCount : 0;

			int rowH = 34;
			int y = 82;
			int gh = y + 24 + ( activeCount > 0 ? activeCount * rowH : 28 ) + 28 + ( doneCount > 0 ? doneCount * rowH : 28 ) + 55;

			AddPage( 0 );
			AddBackground( 0, 0, 440, gh, 5054 );

			AddHtml( 10, 10, 400, 22, StringCatalog.TryResolve( lang, "<CENTER><BASEFONT COLOR=#FFFFFF>ML Quest: clear progress (GM)</BASEFONT></CENTER>" ) ?? "<CENTER><BASEFONT COLOR=#FFFFFF>ML Quest: clear progress (GM)</BASEFONT></CENTER>", false, false );
			AddHtml( 10, 32, 400, 44, StringCatalog.TryResolve( lang, "<BASEFONT COLOR=#CCCCCC>Del = cancel active quest. Clr = remove completion/cooldown record so the quest can be offered again (testing).</BASEFONT>" ) ?? "<BASEFONT COLOR=#CCCCCC>Del = cancel active quest. Clr = remove completion/cooldown record so the quest can be offered again (testing).</BASEFONT>", false, false );
			AddLabelCropped( 10, 72, 400, 20, 0x480, String.Format( "{0} (0x{1:X})", m_Target.Name, m_Target.Serial.Value ) );

			y = 96;
			AddHtml( 10, y, 400, 20, StringCatalog.TryResolve( lang, "<BASEFONT COLOR=#AAAAFF>Active quests</BASEFONT>" ) ?? "<BASEFONT COLOR=#AAAAFF>Active quests</BASEFONT>", false, false );
			y += 22;

			if ( ctx == null || activeCount == 0 )
			{
				AddHtml( 10, y, 400, 24, StringCatalog.TryResolve( lang, "<BASEFONT COLOR=#CCCCCC>(none)</BASEFONT>" ) ?? "<BASEFONT COLOR=#CCCCCC>(none)</BASEFONT>", false, false );
				y += 28;
			}
			else
			{
				for ( int i = 0; i < activeCount; ++i )
				{
					MLQuestInstance inst = ctx.QuestInstances[i];
					string title = DescribeQuestTitle( inst.Quest, m_Target );
					string status = DescribeStatus( inst );
					string line = String.Format( "{0}. {1} -- {2}", i + 1, title, status );

					AddLabelCropped( 10, y, 318, rowH - 6, 0x480, line );
					AddButton( 332, y + 4, 0xFA5, 0xFA7, BtnActiveBase + i, GumpButtonType.Reply, 0 );
					AddLabel( 368, y + 4, 0x480, StringCatalog.TryResolve( lang, "Del" ) ?? "Del" );

					y += rowH;
				}
			}

			y += 6;
			AddHtml( 10, y, 400, 20, StringCatalog.TryResolve( lang, "<BASEFONT COLOR=#FFCC88>Completed / cooldown records</BASEFONT>" ) ?? "<BASEFONT COLOR=#FFCC88>Completed / cooldown records</BASEFONT>", false, false );
			y += 22;

			if ( ctx == null || doneCount == 0 )
			{
				AddHtml( 10, y, 400, 24, StringCatalog.TryResolve( lang, "<BASEFONT COLOR=#CCCCCC>(none)</BASEFONT>" ) ?? "<BASEFONT COLOR=#CCCCCC>(none)</BASEFONT>", false, false );
				y += 28;
			}
			else
			{
				for ( int i = 0; i < doneCount; ++i )
				{
					MLQuest quest;
					DateTime nextAv;

					if ( !ctx.TryGetDoneQuestRecord( i, out quest, out nextAv ) || quest == null )
						continue;

					string title = DescribeQuestTitle( quest, m_Target );
					string cool = DescribeCooldown( nextAv );
					string line = String.Format( "{0}. {1} -- {2}", i + 1, title, cool );

					AddLabelCropped( 10, y, 318, rowH - 6, 0x480, line );
					AddButton( 332, y + 4, 0xFA5, 0xFA7, BtnDoneBase + i, GumpButtonType.Reply, 0 );
					AddLabel( 368, y + 4, 0x480, StringCatalog.TryResolve( lang, "Clr" ) ?? "Clr" );

					y += rowH;
				}
			}

			AddButton( 10, gh - 35, 0xFB1, 0xFB3, 0, GumpButtonType.Reply, 0 );
			AddLabel( 45, gh - 35, 0x480, StringCatalog.TryResolve( lang, "Close" ) ?? "Close" );
		}

		private static string DescribeCooldown( DateTime nextAvailable )
		{
			if ( nextAvailable <= DateTime.UtcNow || nextAvailable == DateTime.MinValue )
				return "may repeat now";

			TimeSpan remain = nextAvailable - DateTime.UtcNow;

			return String.Format( "cooldown ~{0}d {1:D2}h", remain.Days, remain.Hours );
		}

		private static string DescribeStatus( MLQuestInstance inst )
		{
			if ( inst == null )
				return "?";

			if ( inst.ClaimReward )
				return "awaiting reward";
			if ( inst.Failed )
				return "failed";
			if ( inst.IsCompleted() )
				return "objectives complete";

			return "in progress";
		}

		private static string DescribeQuestTitle( MLQuest quest, PlayerMobile subj )
		{
			if ( quest == null )
				return "?";

			TextDefinition def = quest.Title;

			if ( def.Number > 0 )
				return String.Format( "#{0}", def.Number );

			if ( def.String == null )
				return quest.GetType().Name;

			if ( subj == null || subj.Account == null )
				return def.String;

			string lang = AccountLang.GetLanguageCode( subj.Account );
			string direct = StringCatalog.TryResolveLogicalOrHash( lang, def.String ) ?? def.String;
			string resolved;

			if ( direct != def.String )
				resolved = direct;
			else if ( def.String.IndexOf( "<br", StringComparison.OrdinalIgnoreCase ) >= 0 )
				resolved = QuestHtmlSegmentCatalogResolver.Resolve( subj, def.String );
			else
			{
				resolved = def.String;

				if ( AccountLang.IsChinese( lang ) )
					resolved = QuestCompositeResolver.ResolveComposite( subj, resolved );
			}

			if ( resolved != null )
			{
				int idx = resolved.IndexOf( '<' );

				if ( idx >= 0 )
					resolved = resolved.Substring( 0, idx );

				resolved = resolved.Trim();

				if ( resolved.Length > 72 )
					resolved = resolved.Substring( 0, 69 ) + "...";
			}

			return resolved ?? quest.GetType().Name;
		}

		private static bool MayUseTool( Mobile from, PlayerMobile target )
		{
			if ( from == null || target == null || target.Deleted )
				return false;

			if ( from.AccessLevel < AccessLevel.GameMaster )
				return false;

			// Self-service for GM testing, or higher access than target.
			return from == target || from.AccessLevel > target.AccessLevel;
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			if ( m_Target == null || m_Target.Deleted )
			{
				from.SendMessage( "That player no longer exists." );
				return;
			}

			if ( !MayUseTool( from, m_Target ) )
			{
				from.SendMessage( "You cannot use this tool." );
				return;
			}

			if ( info.ButtonID == 0 )
				return;

			MLQuestContext ctx = MLQuestSystem.GetContext( m_Target );

			if ( ctx == null )
			{
				from.SendMessage( "No ML quest data for this character." );
				from.SendGump( new GmClientMLQuestRemovalGump( from, m_Target ) );
				return;
			}

			// Clear completion / cooldown record
			if ( info.ButtonID >= BtnDoneBase )
			{
				int idx = info.ButtonID - BtnDoneBase;

				if ( idx < 0 || idx >= ctx.DoneQuestRecordCount )
				{
					from.SendMessage( "That record slot is no longer valid; refreshing the list." );
					from.SendGump( new GmClientMLQuestRemovalGump( from, m_Target ) );
					return;
				}

				MLQuest quest;
				DateTime nextAv;

				if ( !ctx.TryGetDoneQuestRecord( idx, out quest, out nextAv ) || quest == null )
				{
					from.SendMessage( "That record slot is no longer valid; refreshing the list." );
					from.SendGump( new GmClientMLQuestRemovalGump( from, m_Target ) );
					return;
				}

				string questLabel = DescribeQuestTitle( quest, m_Target );

				ctx.RemoveDoneQuest( quest );
				ctx.ChainOffers.Remove( quest );

				CommandLogging.WriteLine( from, "{0} {1} cleared ML quest completion record for {2}: ({3})", from.AccessLevel, CommandLogging.Format( from ), CommandLogging.Format( m_Target ), questLabel );

				from.SendMessage( "Cleared ML quest completion record: {0}", questLabel );
				from.SendGump( new GmClientMLQuestRemovalGump( from, m_Target ) );
				return;
			}

			// Cancel active instance
			if ( info.ButtonID >= BtnActiveBase )
			{
				int idx = info.ButtonID - BtnActiveBase;

				if ( idx < 0 || idx >= ctx.QuestInstances.Count )
				{
					from.SendMessage( "That quest slot is no longer valid; refreshing the list." );
					from.SendGump( new GmClientMLQuestRemovalGump( from, m_Target ) );
					return;
				}

				MLQuestInstance inst = ctx.QuestInstances[idx];
				string questLabel = DescribeQuestTitle( inst.Quest, m_Target );

				inst.Cancel( true );

				CommandLogging.WriteLine( from, "{0} {1} removed ML quest progress for {2}: slot {3} ({4})", from.AccessLevel, CommandLogging.Format( from ), CommandLogging.Format( m_Target ), idx, questLabel );

				from.SendMessage( "Removed ML quest: {0}", questLabel );
				from.SendGump( new GmClientMLQuestRemovalGump( from, m_Target ) );
			}
		}
	}
}
