using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Engines.Craft
{
	public class BulkCraft
	{
		public static void Configure()
		{
			EventSink.Disconnected += args => StopTimer(args.Mobile as PlayerMobile);
			EventSink.PlayerDeath += args => StopTimer(args.Mobile as PlayerMobile);
		}

		private static readonly Dictionary<Serial, BulkCraftTimer> m_Timers = new Dictionary<Serial, BulkCraftTimer>();

		private static BulkCraftTimer GetTimer(PlayerMobile player)
		{
			if (player == null) return null;

			BulkCraftTimer timer;
			return m_Timers.TryGetValue(player.Serial, out timer) ? timer : null;
		}

		public static void StartTimer(BulkCraftTimer newTimer)
		{
			StopTimer(newTimer.Player);
			newTimer.Start();
			m_Timers[newTimer.Player.Serial] = newTimer;
		}

		public static void Pause(PlayerMobile player)
		{
			if (player == null) return;

			var timer = GetTimer(player);
			if (timer != null)
				timer.Pause();
		}

		public static void StopTimer(PlayerMobile player)
		{
			if (player == null) return;

			var timer = GetTimer(player);
			if (timer != null)
				timer.Cancel();

			m_Timers[player.Serial] = null;
		}
	}
}