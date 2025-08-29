using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.GlobalShoppe
{
	[PropertyObject]
	public class PlayerContext
	{
		private readonly TradeSkillContext[] m_Trades;

		public PlayerContext()
		{
			m_Trades = CreateContextList();
		}

		public PlayerContext(GenericReader reader)
		{
			int version = reader.ReadInt();

			m_Trades = CreateContextList();
			int count = reader.ReadInt();
			for (int i = 0; i < count; ++i)
			{
				bool hasData = reader.ReadBool();
				if (hasData)
					m_Trades[i] = new TradeSkillContext((ShoppeType)i, reader);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Alchemist
		{ get { return this[ShoppeType.Alchemist]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Baker
		{ get { return this[ShoppeType.Baker]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Blacksmith
		{ get { return this[ShoppeType.Blacksmith]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Bowyer
		{ get { return this[ShoppeType.Bowyer]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Carpentry
		{ get { return this[ShoppeType.Carpentry]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Cartography
		{ get { return this[ShoppeType.Cartography]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Herbalist
		{ get { return this[ShoppeType.Herbalist]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Librarian
		{ get { return this[ShoppeType.Librarian]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Mortician
		{ get { return this[ShoppeType.Mortician]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Tailor
		{ get { return this[ShoppeType.Tailor]; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TradeSkillContext Tinker
		{ get { return this[ShoppeType.Tinker]; } set { } }

		public IEnumerable<TradeSkillContext> Trades
		{ get { return m_Trades.Where(x => x != null); } }

		public TradeSkillContext this[ShoppeType name]
		{
			get { return this[(int)name]; }
		}

		public TradeSkillContext this[int id]
		{
			get
			{
				if (id < 0 || id >= m_Trades.Length) return null;

				TradeSkillContext context = m_Trades[id];
				if (context != null) return context;

				return m_Trades[id] = new TradeSkillContext();
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(m_Trades.Length);
			for (int i = 0; i < m_Trades.Length; ++i)
			{
				TradeSkillContext trade = m_Trades[i];
				bool hasData = trade != null;
				writer.Write(hasData);
				if (hasData)
					trade.Serialize(writer);
			}
		}

		public override string ToString()
		{
			return GetType().Name;
		}

		private TradeSkillContext[] CreateContextList()
		{
			var count = Enum.GetValues(typeof(ShoppeType)).Cast<ShoppeType>().Count();

			return new TradeSkillContext[count];
		}
	}
}