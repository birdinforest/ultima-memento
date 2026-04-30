using System;
using System.Text;
using Server.Accounting;
using Server.Gumps;
using Server.Localization;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.RpgDialogue
{
	public sealed class DynamicRpgDialogueOption
	{
		public string LabelEnglish { get; }
		public Action<PlayerMobile, Mobile> OnChosen { get; }

		public DynamicRpgDialogueOption(string labelEnglish, Action<PlayerMobile, Mobile> onChosen)
		{
			LabelEnglish = labelEnglish;
			OnChosen = onChosen;
		}
	}

	/// <summary>
	/// RPG-style dialogue with runtime body/options (used by quests). Each option invokes <see cref="DynamicRpgDialogueOption.OnChosen"/>; reopening the gump is the callback's responsibility.
	/// </summary>
	public class DynamicRpgDialogueGump : Gump
	{
		public const int CloseButtonId = 100;
		private const int OptionIdBase = 1;

		private readonly Mobile m_Npc;
		private readonly PlayerMobile m_Viewer;
		private readonly string m_BodyEnglish;
		private readonly DynamicRpgDialogueOption[] m_Options;

		public DynamicRpgDialogueGump(Mobile npc, PlayerMobile viewer, string bodyEnglish, DynamicRpgDialogueOption[] options)
			: base(75, 25)
		{
			m_Npc = npc;
			m_Viewer = viewer;
			m_BodyEnglish = bodyEnglish ?? "";
			m_Options = options ?? Array.Empty<DynamicRpgDialogueOption>();

			Closable = true;
			Disposable = true;
			Dragable = true;

			if (viewer != null)
			{
				viewer.CloseGump(typeof(DynamicRpgDialogueGump));
				viewer.CloseGump(typeof(RpgDialogueGump));
			}

			AddPage(0);

			AddImage(50, 20, 0x1452);
			AddImage(90, 33, 0x232D);

			string title = ResolveLine(viewer, "Conversation");
			AddHtml(130, 45, 270, 20,
				"<BODY><BASEFONT COLOR=#" + HtmlColors.WHITE.ToString("X6") + ">" + EscapeHtml(title) + "</BASEFONT></BODY>",
				false, false);

			AddImageTiled(130, 65, 320, 1, 0x238D);

			string npcLine = npc != null && !npc.Deleted
				? (npc.Name + (string.IsNullOrEmpty(npc.Title) ? "" : " " + npc.Title))
				: ResolveLine(viewer, "Someone");

			AddHtml(210, 74, 250, 32,
				"<BODY><BASEFONT COLOR=#" + HtmlColors.LIGHT_GOLD.ToString("X6") + ">" + EscapeHtml(npcLine) + "</BASEFONT></BODY>",
				false, false);

			AddImageTiled(95, 115, 102, 122, 0xBBC);
			AddImageTiled(94, 114, 104, 124, 0x23C3);
			string portraitHint = ResolveLine(viewer, "(Portrait placeholder)");
			AddHtml(95, 158, 102, 40,
				"<BODY><DIV ALIGN=CENTER><BASEFONT COLOR=#" + HtmlColors.GRAY.ToString("X6") + ">" + EscapeHtml(portraitHint) + "</BASEFONT></DIV></BODY>",
				false, false);

			string resolvedBody = ResolveLine(viewer, m_BodyEnglish);
			string bodyHtml = "<BODY><BASEFONT COLOR=#" + HtmlColors.OFFWHITE.ToString("X6") + ">" + resolvedBody + "</BASEFONT></BODY>";
			AddHtml(210, 115, 240, 200, bodyHtml, false, true);

			int optY = 330;

			for (int i = 0; i < m_Options.Length && i < 6; ++i)
			{
				int bid = OptionIdBase + i;
				AddButton(115, optY, 0x4B9, 0x4BA, bid, GumpButtonType.Reply, 0);
				string lbl = ResolveLine(viewer, m_Options[i].LabelEnglish);
				AddHtml(145, optY, 305, 24,
					"<BODY><BASEFONT COLOR=#" + HtmlColors.COOL_BLUE.ToString("X6") + ">" + EscapeHtml(lbl) + "</BASEFONT></BODY>",
					false, false);
				optY += 28;
			}

			AddButton(363, 425, 0x2EE6, 0x2EE8, CloseButtonId, GumpButtonType.Reply, 0);
		}

		private static string EscapeHtml(string s)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			StringBuilder sb = new StringBuilder(s.Length + 8);

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
			string r = StringCatalog.TryResolveLogicalOrHash(lang, english) ?? english;

			if (AccountLang.IsChinese(lang))
				r = QuestCompositeResolver.ResolveComposite(pm, r);

			return r;
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			PlayerMobile pm = sender != null ? sender.Mobile as PlayerMobile : null;

			if (pm == null)
				return;

			if (info.ButtonID == CloseButtonId || info.ButtonID == 0)
				return;

			int idx = info.ButtonID - OptionIdBase;

			if (idx < 0 || idx >= m_Options.Length)
				return;

			Action<PlayerMobile, Mobile> fn = m_Options[idx].OnChosen;

			if (fn == null)
				return;

			Mobile npc = m_Npc;

			if (npc == null || npc.Deleted)
				return;

			fn(pm, npc);
		}
	}
}
