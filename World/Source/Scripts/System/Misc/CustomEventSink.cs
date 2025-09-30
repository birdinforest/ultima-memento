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
		public readonly Mobile Mobile;
		public readonly string Message;

		public ChatMessageEventArgs(Mobile from, string msg)
		{
			Mobile = from;
			Message = msg;
		}
	}

	public class LootPullEventArgs
	{
		public readonly Mobile Mobile;
		public readonly Item Item;

		public LootPullEventArgs(Mobile from, Item item)
		{
			Mobile = from;
			Item = item;
		}
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