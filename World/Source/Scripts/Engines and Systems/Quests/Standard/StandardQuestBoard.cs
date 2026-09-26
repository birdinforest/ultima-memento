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
		const string BRAVE_ADVENTURERS_TITLE = "SEEKING BRAVE ADVENTURERS";

		// Localization helper (Memento zh-Hans): resolve per-account language, fall back to English.
		private static string ResolveText( Mobile from, string text )
		{
			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		private static string ResolveFormat( Mobile from, string format, params object[] args )
		{
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
					e.PrivateOverheadMessage(MessageType.Regular, 1150, false, ResolveText( e, "Your quest is broken." ), e.NetState);
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

			message = ResolveFormat( e, "The townsfolk are looking for brave adventurers, {0}. Adventurers are given bounties in which they must search for and slay, or items they are to search for and retrieve. Each quest must be completed to get another. If you fail at one quest, the townsfolk will not grant another unless reparations are given. The more famous an adventurer, the better chance to get a high priced bounty or valuable item to find. Of course the more gold for a reward, usually means how difficult the quest may be.<br><br>", e.Name );
			message += ResolveText( e, "These quests do not send you to a land you have never been, but they may send you to any dungeon in lands you have traveled. If you do not know the location of a particular place, you had better begin your exploration of such areas. Any other details of the quest can be read in the quest log (typing '[quests'). When such a quest is completed, return to any of these bulletin boards and select that you are 'Done'. You will be rewarded with some gold and fame. You will gain some karma unless your karma is locked. In that case, you will lose karma instead.<br><br>" );

			// Quest on cooldown
			if ( 0 < nWhenForAnotherQuest )
			{
				message += TextDefinition.GetColorizedText(ResolveFormat(e, "There are no quests at the moment. Check back in {0} minutes.", nWhenForAnotherQuest), HtmlColors.MUSTARD);

				e.SendGump( new BoardGump( e, ResolveText( e, BRAVE_ADVENTURERS_TITLE ), message, "#e9e9e9", false ) );
				return;
			}

			OfferQuest( e, message );
		}

		private void AbandonQuestPrompt( Mobile e, string questStatus )
		{
			var message = ResolveText( e, "You are currently on a quest that should not be too difficulty for someone as hardy as yourself. If you feel this quest is beyond your bravery, you may never get asked to do another unless reparations are paid. If you wish to rid yourself of this quest, then you must pay the reward offered to restore your reputation with the townsfolk.<br><br>" );
			message += string.Format("{0}.", TextDefinition.GetColorizedText(questStatus, HtmlColors.MUSTARD));

			var cost = StandardQuestFunctions.QuestFailure( e );
			e.SendGump( new BoardGump(
				e, ResolveText( e, BRAVE_ADVENTURERS_TITLE ), message, "#e9e9e9", false, null, null,
				TextDefinition.GetColorizedText(ResolveFormat(e, "Concede and pay {0:n0} gold in reparations", cost ), HtmlColors.RED),
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
						e.SendMessage(ResolveText(e, "You cannot afford to pay the reparations."));
					}

					Timer.DelayCall(TimeSpan.FromMilliseconds( 500 ), () => OnDoubleClick( e ));
				})
			);
		}

		private void OfferQuest( Mobile e, string message )
		{
			e.SendGump( new BoardGump(
				e, ResolveText( e, BRAVE_ADVENTURERS_TITLE ), message, "#e9e9e9", false,
				TextDefinition.GetColorizedText(ResolveText(e, "Offer your services"), HtmlColors.MUSTARD),
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
						message += TextDefinition.GetColorizedText(ResolveText(e, "There are no quests at the moment."), HtmlColors.MUSTARD);
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

			var message = ResolveFormat( e, "The townsfolk are looking for brave adventurers, {0}. Adventurers are given bounties in which they must search for and slay, or items they are to search for and retrieve. Each quest must be completed to get another. If you fail at one quest, the townsfolk will not grant another unless reparations are given. The more famous an adventurer, the better chance to get a high priced bounty or valuable item to find. Of course the more gold for a reward, usually means how difficult the quest may be.<br><br>", e.Name );
			message += ResolveText( e, "These quests do not send you to a land you have never been, but they may send you to any dungeon in lands you have traveled. If you do not know the location of a particular place, you had better begin your exploration of such areas. Any other details of the quest can be read in the quest log (typing '[quests'). When such a quest is completed, return to any of these bulletin boards and select that you are 'Done'. You will be rewarded with some gold and fame. You will gain some karma unless your karma is locked. In that case, you will lose karma instead.<br><br>" );

			e.SendGump( new BoardGump(
				e, ResolveText( e, BRAVE_ADVENTURERS_TITLE ), message, "#e9e9e9", false,
				TextDefinition.GetColorizedText(ResolveText(e, "Collect your reward"), HtmlColors.MUSTARD),
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
