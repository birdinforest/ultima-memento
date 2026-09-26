using Server.Misc;
using Server.Network;
using Server.Gumps;
using Server.Mobiles;
using Server.Localization;
using System;

namespace Server.Items
{
	[Flipable(0x577C, 0x577B)]
	public class StandardQuestBoard : Item
	{
		// Localization helper: logical shotkeys (quest.board.*) or legacy hash English.
		private static string ResolveText( Mobile from, string text )
		{
			if ( text != null && text.StartsWith( "quest.board." ) )
				return StringCatalog.ResolveByKey( from.Account, text );

			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		private static string ResolveFormat( Mobile from, string format, params object[] args )
		{
			if ( format != null && format.StartsWith( "quest.board." ) )
				return StringCatalog.ResolveFormatByKey( from.Account, format, args );

			return string.Format( ResolveText( from, format ), args );
		}

		[Constructable]
		public StandardQuestBoard() : base(0x577B)
		{
			Weight = 1.0;
			Name = "Seeking Brave Adventurers";
			Hue = 0xB26;
		}

		public override void OnDoubleClick( Mobile e )
		{
			if( !( e is PlayerMobile ) ) return;

			if ( !e.InRange( this.GetWorldLocation(), 4 ) )
			{
				e.SendLocalizedMessage( 502138 ); // That is too far away for you to use
				return;
			}

			string message;

			e.CloseGump( typeof( BoardGump ) );

			var alreadyHasQuest = PlayerSettings.GetQuestState( e, "StandardQuest" );
			if ( alreadyHasQuest )
			{
				var status = StandardQuestFunctions.QuestStatus( e );
				if (string.IsNullOrWhiteSpace(status))
				{
					e.PrivateOverheadMessage(MessageType.Regular, 1150, false, ResolveText( e, "quest.board.quest_broken" ), e.NetState);
					return;
				}

				if ( TryShowCompleteQuestGump( e ) ) return;

				AbandonQuestPrompt( e, status );
				return;
			}

			string _ = PlayerSettings.GetQuestInfo( e, "StandardQuest" ); // Maybe unnecessary ... maybe prevents null ref, IDK
			int nAllowedForAnotherQuest = StandardQuestFunctions.QuestTimeNew( e );
			int nServerQuestTimeAllowed = MyServerSettings.GetTimeBetweenQuests();
			int nWhenForAnotherQuest = nServerQuestTimeAllowed - nAllowedForAnotherQuest;

			message = ResolveFormat( e, "quest.board.intro_para1", e.Name );
			message += ResolveText( e, "quest.board.intro_para2" );

			// Quest on cooldown
			if ( 0 < nWhenForAnotherQuest )
			{
				message += TextDefinition.GetColorizedText(ResolveFormat(e, "quest.board.no_quests_cooldown", nWhenForAnotherQuest), HtmlColors.MUSTARD);

				e.SendGump( new BoardGump( e, ResolveText( e, "quest.board.title" ), message, "#e9e9e9", false ) );
				return;
			}

			OfferQuest( e, message );
		}

		private void AbandonQuestPrompt( Mobile e, string questStatus )
		{
			var message = ResolveText( e, "quest.board.abandon_intro" );
			message += string.Format("{0}.", TextDefinition.GetColorizedText(questStatus, HtmlColors.MUSTARD));

			var cost = StandardQuestFunctions.QuestFailure( e );
			e.SendGump( new BoardGump(
				e, ResolveText( e, "quest.board.title" ), message, "#e9e9e9", false, null, null,
				TextDefinition.GetColorizedText(ResolveFormat(e, "quest.board.concede_pay", cost ), HtmlColors.RED),
				() => {
					var paid = e.AccessLevel >= AccessLevel.GameMaster;

					var cont = e.Backpack;
					if ( !paid )
					{
						paid = cont != null && cont.ConsumeTotal( typeof( Gold ), cost );
					}

					if ( !paid )
					{
						cont = e.FindBankNoCreate();
						paid = cont != null && cont.ConsumeTotal( typeof( Gold ), cost );
					}

					if ( paid )
					{
						e.PlaySound( 0x32 );
						PlayerSettings.ClearQuestInfo( e, "StandardQuest" );
						StandardQuestFunctions.QuestTimeAllowed( e );
					}
					else
					{
						e.SendMessage(ResolveText(e, "quest.board.cannot_afford_reparations"));
					}

					Timer.DelayCall(TimeSpan.FromMilliseconds( 500 ), () => OnDoubleClick( e ));
				})
			);
		}

		private void OfferQuest( Mobile e, string message )
		{
			e.SendGump( new BoardGump(
				e, ResolveText( e, "quest.board.title" ), message, "#e9e9e9", false,
				TextDefinition.GetColorizedText(ResolveText(e, "quest.board.offer_services"), HtmlColors.MUSTARD),
				() => {
					var minFame = e.Fame;
					var maxFame = Utility.RandomMinMax( minFame, minFame * 2 ) + 2000;

					// Try to find a target multiple times
					const int MAX_RETRIES = 10;
					for (int i = 0; i < MAX_RETRIES; i++)
					{
						StandardQuestFunctions.FindTarget( e, minFame, maxFame );
						if ( !string.IsNullOrWhiteSpace( StandardQuestFunctions.QuestStatus( e ) ) ) break;
					}

					var status = StandardQuestFunctions.QuestStatus( e );
					if (string.IsNullOrWhiteSpace( status ) )
					{
						message += TextDefinition.GetColorizedText(ResolveText(e, "quest.board.no_quests"), HtmlColors.MUSTARD);
						Timer.DelayCall(TimeSpan.FromMilliseconds( 500 ), () => OnDoubleClick( e ));
					}
					else
					{
						message += string.Format("{0}.", TextDefinition.GetColorizedText(status, HtmlColors.MUSTARD));
						Timer.DelayCall(TimeSpan.FromMilliseconds( 500 ), () => OnDoubleClick( e ));
					}
				}, null, null)
			);
		}

		private bool TryShowCompleteQuestGump( Mobile e )
		{
			if ( StandardQuestFunctions.DidQuest( e ) < 1 ) return false;

			var message = ResolveFormat( e, "quest.board.intro_para1", e.Name );
			message += ResolveText( e, "quest.board.intro_para2" );

			e.SendGump( new BoardGump(
				e, ResolveText( e, "quest.board.title" ), message, "#e9e9e9", false,
				TextDefinition.GetColorizedText(ResolveText(e, "quest.board.collect_reward"), HtmlColors.MUSTARD),
				() => StandardQuestFunctions.PayAdventurer( e ),
				null, null)
			);

			return true;
		}

		public StandardQuestBoard(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
