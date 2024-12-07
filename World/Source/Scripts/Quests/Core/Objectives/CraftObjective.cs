using System;
using Server.Gumps;
using Server.Engines.MLQuests.Gumps;

namespace Server.Engines.MLQuests.Objectives
{
	/// <summary>
	/// Basically a clone of CollectObjective, just used for better messaging
	/// </summary>
	public class CraftObjective : CollectObjective
	{
		public CraftObjective()
			: this(0, null, null)
		{
		}

		public CraftObjective(int amount, Type type, TextDefinition name) : base(amount, type, name)
		{
		}

		public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
		{
			return new CraftObjectiveInstance(this, instance);
		}

		public void WriteToGump(Gump g, CraftObjectiveInstance instance, ref int y)
		{
			if (ShowDetailed)
			{
				if (instance == null)
					g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, string.Format("- {0:n0} {1}", DesiredAmount, Name.String));
				else
					g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, string.Format("- {0:n0} / {1} {2}", instance.GetCurrentTotal(), DesiredAmount, Name.String));
			}
			else
			{
				if (Name.Number > 0)
					g.AddHtmlLocalized(98, y, 312, 32, Name.Number, BaseQuestGump.COLOR_LOCALIZED, false, false);
				else if (Name.String != null)
					g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, Name.String);
			}

			y += 16;
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			WriteToGump(g, null, ref y);
		}
	}

	#region Timed

	public class TimedCraftObjective : CraftObjective
	{
		private TimeSpan m_Duration;

		public override bool IsTimed { get { return true; } }
		public override TimeSpan Duration { get { return m_Duration; } }

		public TimedCraftObjective(TimeSpan duration, int amount, Type type, TextDefinition name)
			: base(amount, type, name)
		{
			m_Duration = duration;
		}
	}

	#endregion

	public class CraftObjectiveInstance : CollectObjectiveInstance<CraftObjective>
	{
		public CraftObjectiveInstance(CraftObjective objective, MLQuestInstance instance)
			: base(objective, instance)
		{
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			Objective.WriteToGump(g, this, ref y);
		}
	}
}