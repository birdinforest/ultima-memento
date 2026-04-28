using Server.Mobiles;
using Server.Gumps;
using System.Collections.Generic;
using Server.Engines.MLQuests.Gumps;

namespace Server.Engines.MLQuests.Rewards
{
	public abstract class BaseReward
	{
		private TextDefinition m_Name;

		public TextDefinition Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		public BaseReward( TextDefinition name )
		{
			m_Name = name;
		}

		protected virtual int LabelHeight { get { return 16; } }

		public void WriteToGump( Gump g, int x, ref int y )
		{
			TextDefinition resolved = BaseQuestGump.ResolveQuestTextDefinition( g, m_Name ) ?? m_Name;
			TextDefinition.AddHtmlText( g, x, y, 280, LabelHeight, resolved, false, false, BaseQuestGump.COLOR_LOCALIZED, BaseQuestGump.COLOR_HTML );
		}

		public abstract void AddRewardItems( PlayerMobile pm, List<Item> rewards );
	}
}
