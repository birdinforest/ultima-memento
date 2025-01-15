using System;
using Server.Mobiles;
using Server.Gumps;
using Server.Items;
using Server.Engines.MLQuests.Gumps;

namespace Server.Engines.MLQuests.Objectives
{
	public class CollectObjective : BaseObjective
	{
		public int DesiredAmount { get; set; }
		public Type AcceptedType { get; set; }
		public TextDefinition Name { get; set; }
		public bool DoNotConsume { get; set; }

		public virtual bool ShowDetailed { get { return true; } }

		public CollectObjective()
			: this(0, null, null)
		{
		}

		public CollectObjective(int amount, Type type, TextDefinition name)
		{
			DesiredAmount = amount;
			AcceptedType = type;
			Name = name;

			if (MLQuestSystem.Debug && ShowDetailed && name.Number > 0)
			{
				int itemid = LabelToItemID(name.Number);

				if (itemid <= 0 || itemid > 0x4000)
					Console.WriteLine("Warning: cliloc {0} is likely giving the wrong item ID", name.Number);
			}
		}

		public bool CheckType(Type type)
		{
			return (AcceptedType != null && AcceptedType.IsAssignableFrom(type));
		}

		public virtual bool CheckItem(Item item)
		{
			return true;
		}

		public static int LabelToItemID(int label)
		{
			if (label < 1078872)
				return (label - 1020000);
			else
				return (label - 1078872);
		}

		public void WriteToGump(Gump g, CollectObjectiveInstance instance, ref int y)
		{
			if (ShowDetailed)
			{
				if (instance == null)
					g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, string.Format("- {0:n0} {1}", DesiredAmount, Name.String));
				else
					g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, string.Format("- {0:n0} of {1} {2}", instance.GetCurrentTotal(), DesiredAmount, Name.String));
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

		public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
		{
			return new CollectObjectiveInstance(this, instance);
		}
	}

	#region Timed

	public class TimedCollectObjective : CollectObjective
	{
		private TimeSpan m_Duration;

		public override bool IsTimed { get { return true; } }
		public override TimeSpan Duration { get { return m_Duration; } }

		public TimedCollectObjective(TimeSpan duration, int amount, Type type, TextDefinition name)
			: base(amount, type, name)
		{
			m_Duration = duration;
		}
	}

	#endregion

	public class CollectObjectiveInstance : CollectObjectiveInstance<CollectObjective>
	{
		public CollectObjectiveInstance(CollectObjective objective, MLQuestInstance instance)
			: base(objective, instance)
		{
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			Objective.WriteToGump(g, this, ref y);
		}
	}

	public class CollectObjectiveInstance<TObjective> : BaseObjectiveInstance
		where TObjective : CollectObjective
	{
		public TObjective Objective { get; private set; }

		public CollectObjectiveInstance(TObjective objective, MLQuestInstance instance)
			: base(instance, objective)
		{
			Objective = objective;
		}

		public int GetCurrentTotal()
		{
			Container pack = Instance.Player.Backpack;

			if (pack == null)
				return 0;

			Item[] items = pack.FindItemsByType(Objective.AcceptedType, false); // Note: subclasses are included
			int total = 0;

			foreach (Item item in items)
			{
				if (item.QuestItem && Objective.CheckItem(item))
					total += item.Amount;
			}

			return total;
		}

		public override bool AllowsQuestItem(Item item, Type type)
		{
			return Objective.CheckType(type) && Objective.CheckItem(item);
		}

		public override bool IsCompleted()
		{
			return GetCurrentTotal() >= Objective.DesiredAmount;
		}

		public override void OnQuestCancelled()
		{
			PlayerMobile pm = Instance.Player;
			Container pack = pm.Backpack;

			if (pack == null)
				return;

			Type checkType = Objective.AcceptedType;
			Item[] items = pack.FindItemsByType(checkType, false);

			foreach (Item item in items)
			{
				if (item.QuestItem && !MLQuestSystem.CanMarkQuestItem(pm, item, checkType)) // does another quest still need this item? (OSI just unmarks everything)
					item.QuestItem = false;
			}
		}

		// Should only be called after IsComplete() is checked to be true
		public override void OnClaimReward()
		{
			Container pack = Instance.Player.Backpack;

			if (pack == null)
				return;

			// TODO: OSI also counts the item in the cursor?

			Item[] items = pack.FindItemsByType(Objective.AcceptedType, false);
			int left = Objective.DesiredAmount;

			foreach (Item item in items)
			{
				if (item.QuestItem && Objective.CheckItem(item))
				{
					if (left == 0)
						return;

					if (item.Amount > left)
					{
						if (!Objective.DoNotConsume) // Not not ... So do.
							item.Consume(left);

						left = 0;
					}
					else
					{
						left -= item.Amount;
						if (Objective.DoNotConsume) continue;

						item.Delete();
					}
				}
			}
		}

		public override void OnAfterClaimReward()
		{
			OnQuestCancelled(); // same thing, clear other quest items
		}

		public override void OnExpire()
		{
			OnQuestCancelled();

			// No message
		}
	}
}
