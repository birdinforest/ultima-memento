using Server.Mobiles;

namespace Server
{
	public delegate void LandChangedEventHandler(LandChangedArgs e);
	public delegate void ChatMessageEventHandler(ChatMessageEventArgs e);
	public delegate void LootPullEventHandler(LootPullEventArgs e);

	public class LandChangedArgs
	{
		public readonly PlayerMobile Mobile;
		public readonly Land OldLand;
		public readonly Land NewLand;

		public LandChangedArgs(PlayerMobile mobile, Land oldLand, Land newLand)
		{
			Mobile = mobile;
			OldLand = oldLand;
			NewLand = newLand;
		}
	}

	public class ChatMessageEventArgs
	{
		private readonly Mobile m_From;
		private readonly string m_Message;

		public ChatMessageEventArgs(Mobile from, string msg)
		{
			m_From = from;
			m_Message = msg;
		}

		public Mobile Mobile { get { return m_From; } }
		public string Message { get { return m_Message; } }
	}

	public class LootPullEventArgs
	{
		private readonly Mobile m_From;
		private readonly Item m_Item;

		public LootPullEventArgs(Mobile from, Item item)
		{
			m_From = from;
			m_Item = item;
		}

		public Mobile Mobile { get { return m_From; } }
		public Item Item { get { return m_Item; } }
	}

	public class CustomEventSink
	{
		public static event LandChangedEventHandler LandChanged;
		public static event ChatMessageEventHandler ChatMessage;
		public static event LootPullEventHandler LootPull;

		public static void InvokeLandChanged(LandChangedArgs e)
		{
			if (LandChanged != null)
				LandChanged(e);
		}

		public static void InvokeChatMessage(ChatMessageEventArgs e)
		{
			if (ChatMessage != null)
				ChatMessage(e);
		}

		public static void InvokeLootPull(LootPullEventArgs e)
		{
			if (LootPull != null)
				LootPull(e);
		}
	}
}