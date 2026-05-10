using Server.Items;
using Server.Localization;
using Server.Mobiles;

namespace Server.Misc
{
	/// <summary>
	/// Pays the same reputation penalty as dropping gold on a quest board / atonement box,
	/// but from bank first then backpack so players can abandon from the quest log gump.
	/// </summary>
	public static class ContractQuestAbandon
	{
		public static int GetPenalty(Mobile m, string questKey)
		{
			switch (questKey)
			{
				case "StandardQuest": return StandardQuestFunctions.QuestFailure(m);
				case "FishingQuest": return FishingQuestFunctions.QuestFailure(m);
				case "AssassinQuest": return AssassinFunctions.QuestFailure(m);
				default: return 0;
			}
		}

		public static bool TryAbandon(Mobile m, string questKey, out int paid, out string message)
		{
			paid = 0;
			message = null;

			if (!(m is PlayerMobile) || m.Backpack == null)
			{
				message = ResolveText(m, "You cannot do that right now.");
				return false;
			}

			if (!PlayerSettings.GetQuestState(m, questKey))
			{
				message = ResolveText(m, "You do not have an active contract of that type.");
				return false;
			}

			int cost = GetPenalty(m, questKey);
			if (cost <= 0)
			{
				message = ResolveText(m, "You do not have an active contract of that type.");
				return false;
			}

			if (!TryPayGold(m, cost, out int available))
			{
				int shortfall = cost - available;
				string goldWord = ResolveText(m, "gold");
				message = string.Format(ResolveText(m, "You need {0:#,0} {1}, but only have {2:#,0} ({3:#,0} short)."),
					cost, goldWord, available, shortfall);
				return false;
			}

			paid = cost;

			switch (questKey)
			{
				case "StandardQuest":
					PlayerSettings.ClearQuestInfo(m, "StandardQuest");
					StandardQuestFunctions.QuestTimeAllowed(m);
					message = ResolveText(m, "Someone else will eventually take care of this.");
					break;
				case "FishingQuest":
					PlayerSettings.ClearQuestInfo(m, "FishingQuest");
					FishingQuestFunctions.QuestTimeAllowed(m);
					message = ResolveText(m, "Someone else will eventually take care of this.");
					break;
				case "AssassinQuest":
					PlayerSettings.ClearQuestInfo(m, "AssassinQuest");
					AssassinFunctions.QuestTimeAllowed(m);
					message = ResolveText(m, "Your failure in this task has been forgiven.");
					break;
				default:
					message = ResolveText(m, "Unknown contract type.");
					return false;
			}

			return true;
		}

		private static bool TryPayGold(Mobile from, int amount, out int available)
		{
			int bank = Banker.GetBalance(from);
			int pack = from.Backpack != null ? from.Backpack.GetAmount(typeof(Gold)) : 0;
			available = bank + pack;

			if (available < amount)
				return false;

			int fromBank = amount <= bank ? amount : bank;
			if (fromBank > 0 && !Banker.Withdraw(from, fromBank))
				return false;

			int fromPack = amount - fromBank;
			if (fromPack <= 0)
				return true;

			return from.Backpack != null && from.Backpack.ConsumeTotal(typeof(Gold), fromPack);
		}

		private static string ResolveText(Mobile from, string english)
		{
			string lang = AccountLang.GetLanguageCode(from != null ? from.Account : null);
			string resolved = StringCatalog.TryResolve(lang, english);
			return !string.IsNullOrEmpty(resolved) ? resolved : english;
		}
	}
}
