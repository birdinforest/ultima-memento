using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Timers
{
	public class RepeatableAction
	{
		private static readonly Dictionary<PlayerMobile, RepeatActionTimer> m_Timers = new Dictionary<PlayerMobile, RepeatActionTimer>();

		public static bool IsUsingSkill(PlayerMobile m)
		{
			RepeatActionTimer timer;
			if (!m_Timers.TryGetValue(m, out timer)) return false;

			return timer.IsUsingSkill;
		}

		public static void Run<T>(PlayerMobile m, bool isUsingSkill, Action action, Func<bool> predicate)
		{
			Run<T>(m, isUsingSkill, action, predicate, TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0));
		}

		public static void Run<T>(PlayerMobile m, bool isUsingSkill, Action action, Func<bool> predicate, TimeSpan delay, TimeSpan interval)
		{
			if (UnderEffect(m))
				StopTimer(m, null);

			var timer = m_Timers[m] = new RepeatActionTimer<T>(m, isUsingSkill, action, predicate, delay, interval);
			timer.Start();
		}

		public static bool StopTimer(PlayerMobile m, bool sendMessage = true)
		{
			if (!StopTimer(m, null)) return false;

			if (sendMessage)
				m.PrivateOverheadMessage(MessageType.Regular, 1150, false, "You stop what you were doing.", m.NetState);

			return true;
		}

		public static bool UnderEffect(PlayerMobile m)
		{
			return m_Timers.ContainsKey(m);
		}

		public static bool UnderEffect<T>(PlayerMobile m)
		{
			RepeatActionTimer timer;
			return m_Timers.TryGetValue(m, out timer) && timer is RepeatActionTimer<T>;
		}

		private static bool StopTimer(PlayerMobile m, Timer timer)
		{
			RepeatActionTimer t;
			if (!m_Timers.TryGetValue(m, out t)) return false;
			if (timer != null && t != timer) return false;

			t.Stop();
			m_Timers.Remove(m);

			// Forcefully set the cooldown if it was using a skill
			if (t.IsUsingSkill)
				m.NextSkillTime = DateTime.Now + t.Interval;

			return true;
		}

		private class RepeatActionTimer<T> : RepeatActionTimer
		{
			public RepeatActionTimer(PlayerMobile m, bool isUsingSkill, Action action, Func<bool> predicate, TimeSpan delay, TimeSpan interval)
				: base(m, isUsingSkill, action, predicate, delay, interval)
			{
			}
		}

		private class RepeatActionTimer : Timer
		{
			public readonly bool IsUsingSkill;

			private readonly Action m_Action;
			private readonly PlayerMobile m_Mobile;
			private readonly Func<bool> m_Predicate;

			public RepeatActionTimer(PlayerMobile m, bool isUsingSkill, Action action, Func<bool> predicate, TimeSpan delay, TimeSpan interval) : base(delay, interval)
			{
				m_Action = action;
				m_Predicate = predicate;
				m_Mobile = m;
				IsUsingSkill = isUsingSkill;
			}

			protected override void OnTick()
			{
				if (!m_Mobile.Alive || m_Mobile.Deleted || m_Mobile.NetState == null || !m_Predicate())
				{
					RepeatableAction.StopTimer(m_Mobile, this);
					return;
				}

				m_Action();
			}
		}
	}
}