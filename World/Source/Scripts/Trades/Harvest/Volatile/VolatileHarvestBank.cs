using System;

namespace Server.Engines.Harvest
{
	public class VolatileHarvestBank : HarvestBank
	{
		public readonly IPoint3D Point;
		public readonly Map Map;

		private readonly VolatileHarvestDefinition m_Def;
		private readonly Mobile m_Creator;
		private readonly Action<bool> m_OnStop;
		private Timer m_Timer;

		public VolatileHarvestBank(Mobile creator, Map map, IPoint3D point, VolatileHarvestDefinition def, HarvestBank harvestBank, Action<bool> onStopped) : base(def, harvestBank.DefaultVein)
		{
			m_Creator = creator;
			m_OnStop = onStopped;
			Map = map;
			m_Def = def;
			Point = point;
		}

		public override void Consume(int amount, Mobile from)
		{
			base.Consume(amount, from);
			if (!IsEmpty) return;

			Stop(true);
		}

		public void Start()
		{
			if (m_Timer != null) m_Timer.Stop();

			m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(30), () => Stop(false));
			m_Timer.Start();
		}

		public void Stop(bool wasConsumed)
		{
			if (m_Timer == null) return;

			m_Def.TryRemoveBank(m_Creator, wasConsumed);
			m_OnStop(wasConsumed);
			m_Timer.Stop();
			m_Timer = null;
		}
	}
}