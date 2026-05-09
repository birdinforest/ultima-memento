using Server.Gumps;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.MLQuests.Gumps
{
	public class ContractQuestAbandonConfirmGump : Gump
	{
		private readonly PlayerMobile m_Player;
		private readonly string m_QuestKey;
		private readonly bool m_CloseGumps;

		public ContractQuestAbandonConfirmGump(PlayerMobile pm, string questKey, bool closeGumps)
			: base(180, 120)
		{
			m_Player = pm;
			m_QuestKey = questKey;
			m_CloseGumps = closeGumps;

			Closable = true;
			Disposable = true;
			Dragable = true;

			AddPage(0);

			AddBackground(0, 0, 380, 220, 0x13BE);
			AddImageTiled(10, 10, 360, 20, 0xA40);
			AddHtml(10, 12, 360, 20, "<CENTER><BASEFONT COLOR=#FFFFAA>" + EscapeForHtml(ResolveText(pm, "Give up contract?")) + "</BASEFONT></CENTER>", false, false);

			int cost = ContractQuestAbandon.GetPenalty(pm, questKey);
			string currency = ResolveText(pm, "gold");
			string body = string.Format(
				"<BODY><BASEFONT COLOR=#CCCCCC>"
				+ EscapeForHtml(ResolveText(pm, "Abandoning this contract costs the same as paying off your reputation:"))
				+ " <BASEFONT COLOR=#FFFFFF>{0:#,0} {1}</BASEFONT>. "
				+ EscapeForHtml(ResolveText(pm, "Gold is taken from your bank box first, then coins in your backpack."))
				+ "<BR><BR>"
				+ EscapeForHtml(ResolveText(pm, "This is the same rule as paying on a quest bulletin board - you can do it here instead."))
				+ "</BASEFONT></BODY>",
				cost, EscapeForHtml(currency));

			AddImageTiled(10, 40, 360, 120, 0xA40);
			AddHtml(18, 46, 344, 112, body, false, true);

			AddButton(40, 180, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtml(75, 180, 120, 22, "<BASEFONT COLOR=#44FF44>" + EscapeForHtml(ResolveText(pm, "Pay and refuse")) + "</BASEFONT>", false, false);

			AddButton(220, 180, 4017, 4019, 0, GumpButtonType.Reply, 0);
			AddHtml(255, 180, 120, 22, "<BASEFONT COLOR=#FF8888>" + EscapeForHtml(ResolveText(pm, "Keep quest")) + "</BASEFONT>", false, false);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Player == null || m_Player.Deleted)
				return;

			if (info.ButtonID == 1)
			{
				if (ContractQuestAbandon.TryAbandon(m_Player, m_QuestKey, out int paid, out string msg))
				{
					m_Player.SendMessage(0x44, msg);
					if (paid > 0)
						m_Player.PlaySound(0x32);
				}
				else
					m_Player.SendMessage(0x22, msg ?? ResolveText(m_Player, "You cannot refuse that contract."));
			}

			m_Player.SendGump(new QuestLogGump(m_Player, m_CloseGumps));
		}

		private static string ResolveText(Mobile from, string english)
		{
			string lang = AccountLang.GetLanguageCode(from != null ? from.Account : null);
			string resolved = StringCatalog.TryResolve(lang, english);
			return !string.IsNullOrEmpty(resolved) ? resolved : english;
		}

		private static string EscapeForHtml(string s)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
		}
	}
}
