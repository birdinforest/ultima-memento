using System;

namespace Server.Engines.Avatar
{
	public class ActionReward : IReward
	{
		private ActionReward()
		{
		}

		public bool CanSelect { get; set; }
		public int Cost { get; private set; }
		public string Description { get; private set; }
		public int Graphic { get; private set; }
		public string Name { get; private set; }
		public Action OnSelect { get; set; }
		public bool Static { get; set; }

		public static ActionReward Create(int cost, int graphic, string name, string description, bool canSelect, Action onSelect)
		{
			return new ActionReward
			{
				Graphic = graphic,
				Name = name,
				Description = description,
				Cost = cost,
				CanSelect = canSelect,
				OnSelect = onSelect,
				Static = false,
			};
		}

		public ActionReward AsStatic()
		{
			Static = true;
			return this;
		}
	}
}