using System.Text;
using Server.Accounting;
using Server.Gumps;
using Server.Localization;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.RpgDialogue
{
	public class RpgDialogueGump : Gump
	{
		public const int CloseButtonId = 100;
		private const int OptionIdBase = 1;

		private readonly Mobile m_Quester;
		private readonly PlayerMobile m_Viewer;
		private readonly string m_ScriptId;
		private readonly string m_NodeId;
		private readonly RpgDialogueNode m_Node;

		public RpgDialogueGump(Mobile quester, PlayerMobile viewer, string scriptId, string nodeId)
			: base(75, 25)
		{
			m_Quester = quester;
			m_Viewer = viewer;
			m_ScriptId = scriptId;
			m_NodeId = nodeId;

			Closable = true;
			Disposable = true;
			Dragable = true;

			if (viewer != null)
			{
				viewer.CloseGump(typeof(RpgDialogueGump));
				viewer.CloseGump(typeof(DynamicRpgDialogueGump));
			}

			if (!RpgDialogueRegistry.TryGetNode(scriptId, nodeId, out m_Node) || m_Node == null)
			{
				BuildEmpty("Dialogue not found.");
				return;
			}

			if (m_Node.OnShow != null && quester != null && !quester.Deleted && viewer != null && !viewer.Deleted)
				m_Node.OnShow(viewer, quester);

			BuildChrome();
		}

		private static void EscapeHtml(StringBuilder sb, string s)
		{
			if (string.IsNullOrEmpty(s))
				return;

			for (int i = 0; i < s.Length; ++i)
			{
				char c = s[i];

				switch (c)
				{
					case '&': sb.Append("&amp;"); break;
					case '<': sb.Append("&lt;"); break;
					case '>': sb.Append("&gt;"); break;
					default: sb.Append(c); break;
				}
			}
		}

		private static string EscapeHtml(string s)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			StringBuilder sb = new StringBuilder(s.Length + 8);
			EscapeHtml(sb, s);
			return sb.ToString();
		}

		private static string ResolveLine(PlayerMobile pm, string english)
		{
			if (english == null || english.Length == 0)
				return english;

			IAccount acct = pm != null ? pm.Account : null;

			if (acct == null)
				return english;

			string lang = AccountLang.GetLanguageCode(acct);
			string r = StringCatalog.TryResolve(lang, english) ?? english;

			if (AccountLang.IsChinese(lang))
				r = QuestCompositeResolver.ResolveComposite(pm, r);

			return r;
		}

		private void BuildEmpty(string message)
		{
			AddPage(0);
			AddImage(50, 20, 0x1452);
			AddImage(90, 33, 0x232D);
			string body = "<BODY><BASEFONT COLOR=#" + HtmlColors.LIGHT_GOLD.ToString("X6") + ">" + EscapeHtml(ResolveLine(m_Viewer, message)) + "</BASEFONT></BODY>";
			AddHtml(130, 110, 330, 80, body, false, false);
			AddButton(363, 425, 0x2EE6, 0x2EE8, CloseButtonId, GumpButtonType.Reply, 0);
		}

		private void BuildChrome()
		{
			AddPage(0);

			AddImage(50, 20, 0x1452);
			AddImage(90, 33, 0x232D);

			string title = ResolveLine(m_Viewer, "Conversation");
			AddHtml(130, 45, 270, 20,
				"<BODY><BASEFONT COLOR=#" + HtmlColors.WHITE.ToString("X6") + ">" + EscapeHtml(title) + "</BASEFONT></BODY>",
				false, false);

			AddImageTiled(130, 65, 320, 1, 0x238D);

			string npcLine = m_Quester != null && !m_Quester.Deleted
				? (m_Quester.Name + (string.IsNullOrEmpty(m_Quester.Title) ? "" : " " + m_Quester.Title))
				: ResolveLine(m_Viewer, "Someone");

			AddHtml(210, 74, 250, 32,
				"<BODY><BASEFONT COLOR=#" + HtmlColors.LIGHT_GOLD.ToString("X6") + ">" + EscapeHtml(npcLine) + "</BASEFONT></BODY>",
				false, false);

			// Portrait placeholder panel
			AddImageTiled(95, 115, 102, 122, 0xBBC);
			AddImageTiled(94, 114, 104, 124, 0x23C3);
			string portraitHint = ResolveLine(m_Viewer, "(Portrait placeholder)");
			AddHtml(95, 158, 102, 40,
				"<BODY><DIV ALIGN=CENTER><BASEFONT COLOR=#" + HtmlColors.GRAY.ToString("X6") + ">" + EscapeHtml(portraitHint) + "</BASEFONT></DIV></BODY>",
				false, false);

			string resolvedBody = ResolveLine(m_Viewer, m_Node.BodyEnglish);
			string bodyHtml = "<BODY><BASEFONT COLOR=#" + HtmlColors.OFFWHITE.ToString("X6") + ">" + resolvedBody + "</BASEFONT></BODY>";
			AddHtml(210, 115, 240, 200, bodyHtml, false, true);

			int optY = 330;

			for (int i = 0; i < m_Node.Options.Length && i < 6; ++i)
			{
				int bid = OptionIdBase + i;
				AddButton(115, optY, 0x4B9, 0x4BA, bid, GumpButtonType.Reply, 0);
				string lbl = ResolveLine(m_Viewer, m_Node.Options[i].LabelEnglish);
				AddHtml(145, optY, 305, 24,
					"<BODY><BASEFONT COLOR=#" + HtmlColors.COOL_BLUE.ToString("X6") + ">" + EscapeHtml(lbl) + "</BASEFONT></BODY>",
					false, false);
				optY += 28;
			}

			AddButton(363, 425, 0x2EE6, 0x2EE8, CloseButtonId, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			PlayerMobile pm = sender != null ? sender.Mobile as PlayerMobile : null;

			if (pm == null || m_Node == null)
				return;

			if (info.ButtonID == CloseButtonId || info.ButtonID == 0)
				return;

			int idx = info.ButtonID - OptionIdBase;

			if (idx < 0 || idx >= m_Node.Options.Length)
				return;

			RpgDialogueOption opt = m_Node.Options[idx];
			string next = opt.NextNodeId;

			if (string.IsNullOrEmpty(next))
				return;

			Mobile q = m_Quester;

			if (q == null || q.Deleted)
				return;

			pm.SendGump(new RpgDialogueGump(q, pm, m_ScriptId, next));
		}
	}
}
