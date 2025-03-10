using System;
using System.Collections.Generic;

namespace Server.Engines.Harvest
{
	public class VolatileHarvestDefinition : HarvestDefinition
	{
		private readonly HarvestDefinition Source;
		private readonly Dictionary<Map, List<VolatileHarvestBank>> m_BanksByMap;
		private readonly Dictionary<Mobile, VolatileHarvestBank> m_BankByMobile;

		public VolatileHarvestDefinition(HarvestDefinition source)
		{
			Source = source;

			m_BanksByMap = new Dictionary<Map, List<VolatileHarvestBank>>();
			m_BankByMobile = new Dictionary<Mobile, VolatileHarvestBank>();
		}

		public override HarvestBank GetBank(Map map, int x, int y)
		{
			List<VolatileHarvestBank> banks;
			if (!m_BanksByMap.TryGetValue(map, out banks)) return null;

			return banks.Find(b => b.Point.X == x && b.Point.Y == y);
		}

		public bool HasBank(Mobile creator)
		{
			return m_BankByMobile.ContainsKey(creator);
		}

		public void TryCreateBank(Mobile creator, IPoint3D point, Func<IPoint3D, Item> nodeFactory)
		{
			// Only one bank per Player
			if (HasBank(creator)) return;

			var map = creator.Map;
			List<VolatileHarvestBank> banks;
			if (!m_BanksByMap.TryGetValue(map, out banks)) m_BanksByMap[map] = banks = new List<VolatileHarvestBank>();

			// Only one bank per Location
			var existing = banks.Find(b => b.Point == point);
			if (existing != null) return;

			// Create bank
			var item = nodeFactory(point);
			var bank = new VolatileHarvestBank(creator, map, point, this, base.GetBank(map, point.X, point.Y), wasConsumed =>
			{
				if (!wasConsumed) Effects.PlaySound(point, map, 0x1D6); // wisp5

				item.Delete();
			});
			banks.Add(bank);
			m_BankByMobile[creator] = bank;

			// Start it
			bank.Start();
		}

		public void TryRemoveBank(Mobile creator, bool deplete)
		{
			VolatileHarvestBank bank;
			if (!m_BankByMobile.TryGetValue(creator, out bank)) return;

			List<VolatileHarvestBank> banks;
			if (m_BanksByMap.TryGetValue(bank.Map, out banks))
				banks.Remove(bank);

			if (deplete)
			{
				var sourceBank = Source.GetBank(bank.Map, bank.Point.X, bank.Point.Y);
				sourceBank.Deplete(creator);
			}

			m_BankByMobile.Remove(creator);
		}
	}
}