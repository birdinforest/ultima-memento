using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System.Collections;
using Server.Localization;
using Server.Accounting;

namespace Server.Gumps
{
	public class LevelUpAcceptGump : Gump
	{
        private LevelUpScroll m_Scroll;
		private Mobile m_From;

		public LevelUpAcceptGump( LevelUpScroll scroll, Mobile from, Mobile smith ) : base( 0, 0 )
		{
			m_Scroll = scroll;
			m_From = from;

			IAccount acct = smith != null ? smith.Account : null;
			string bonusHtml = "";
			if (LevelItems.RewardBlacksmith && LevelItems.BlacksmithRewardAmt > 0 && acct != null)
				bonusHtml = StringCatalog.ResolveFormatByKey(acct, "god.gump.levelup.bonus.html", LevelItems.BlacksmithRewardAmt);
			string html = acct != null
				? StringCatalog.ResolveFormatByKey(acct, "god.gump.levelup.html", bonusHtml)
				: @"<CENTER><U>Max Level Increase Request</U><BR><BR>Someone has requested your expert services in increasing the max levels of a levelable item." + bonusHtml + @"<BR><BR>Do you accept their offer?</CENTER>";

			Closable=false;
			Disposable=false;
			Dragable=true;
			Resizable=false;
			AddPage(0);

            AddBackground(25, 22, 318, 268, 9390);
            AddLabel(52, 27, 0, acct != null ? StringCatalog.ResolveByKey(acct, "god.gump.levelup.title") : @"Level Increase Request");
            AddLabel(52, 60, 0, acct != null ? StringCatalog.ResolveByKey(acct, "god.gump.levelup.requested.by") : @"Requested By:");
            AddLabel(52, 81, 0, acct != null ? StringCatalog.ResolveByKey(acct, "god.gump.levelup.level.amount") : @"Level Amount:");
            AddHtml(49, 109, 271, 116, html, (bool)false, (bool)true);
            AddButton(50, 235, 4023, 4024, 1, GumpButtonType.Reply, 0);
            AddButton(83, 235, 4017, 4018, 2, GumpButtonType.Reply, 0);
            if (m_From != null)
                AddLabel(155, 60, 390, m_From.Name.ToString());
            if (m_Scroll != null)
                AddLabel(155, 81, 390, m_Scroll.Value.ToString());
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile smith = state.Mobile;

			if ( smith == null )
				return;

			IAccount sAcct = smith.Account;

			//Accept
			if ( info.ButtonID == 1 )
			{
                if ( m_From != null && m_Scroll != null)
                {
					if ( m_From != null )
					{
						m_Scroll.BlacksmithValidated = true;
                        m_From.CloseGump(typeof(AwaitingSmithApprovalGump));
                        IAccount fAcct = m_From.Account;
                        if (fAcct != null)
                            m_From.SendMessage(StringCatalog.ResolveByKey(fAcct, "god.msg.gump.validated"));
                        m_From.Target = new LevelUpScroll.LevelItemTarget(m_Scroll); // Call our target
					}

					if ( smith != null ) //Accepted... send message to smith and pay them bonus reward
					{
						if (sAcct != null)
							smith.SendMessage(StringCatalog.ResolveByKey(sAcct, "god.msg.gump.smith.thanks"));
                        if (smith != m_From && LevelItems.RewardBlacksmith && LevelItems.BlacksmithRewardAmt > 0)
                        {
                            smith.AddToBackpack(new BankCheck(LevelItems.BlacksmithRewardAmt));
                            if (sAcct != null)
	                            smith.SendMessage(StringCatalog.ResolveByKey(sAcct, "god.msg.gump.smith.bonus"));
                        }
					}
				}
				else
				{
					if ( m_From != null && smith != null )
					{
						IAccount fAcct = m_From.Account;
						if (fAcct != null)
							m_From.SendMessage(StringCatalog.ResolveByKey(fAcct, "god.msg.gump.validate.error"));
                        if (sAcct != null)
	                        smith.SendMessage(StringCatalog.ResolveByKey(sAcct, "god.msg.gump.validate.error"));
					}
				}
			}

			//Decline
			if ( info.ButtonID == 2 )
			{
				if (sAcct != null)
					smith.SendMessage(StringCatalog.ResolveByKey(sAcct, "god.msg.gump.decline.smith"));

				if ( m_From != null )
				{
                    m_From.CloseGump(typeof(AwaitingSmithApprovalGump));
                    IAccount fAcct = m_From.Account;
                    if (fAcct != null)
	                    m_From.SendMessage(StringCatalog.ResolveByKey(fAcct, "god.msg.gump.decline.client"));
				}
			}
		}
	}
}
