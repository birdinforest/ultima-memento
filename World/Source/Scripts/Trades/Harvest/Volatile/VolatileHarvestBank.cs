using System;

namespace Server.Engines.Harvest
{
	public class VolatileHarvestBank : HarvestBank
	{
		public readonly IPoint3D Point;
		public readonly Map Map;

		private readonly VolatileHarvestDefinition m_Def;
		private readonly Mobile m_Creator;
		private Timer m_Timer;

		public VolatileHarvestBank(Mobile creator, Map map, IPoint3D point, VolatileHarvestDefinition def, HarvestBank harvestBank) : base(def, harvestBank.DefaultVein)
		{
			m_Creator = creator;
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

			// Pulse continually
			var i = 0;
			m_Timer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1), () =>
			{
				if (i == 0) Effects.PlaySound(Point, Map, 0x28E); // agility

				// Animate
				Effects.SendLocationEffect(Point, Map, 0x373A, 15); // Sparkle
				if (++i < 15) return;

				Stop(false);
				Effects.PlaySound(Point, Map, 0x1D6); // wisp5
			});
			m_Timer.Start();
		}

		public void Stop(bool wasConsumed)
		{
			if (m_Timer == null) return;

			m_Def.TryRemoveBank(m_Creator, wasConsumed);
			m_Timer.Stop();
			m_Timer = null;
		}
	}
}