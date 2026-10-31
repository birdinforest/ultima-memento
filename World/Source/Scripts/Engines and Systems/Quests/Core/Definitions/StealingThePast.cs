using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Engines.MLQuests.Gumps;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Misc.Cache;

namespace Server.Engines.MLQuests.Definitions
{
	public abstract class StealingThePastQuest : MLQuest
	{
		public readonly Type ItemType;

		protected StealingThePastQuest(Type itemType)
		{
			ItemType = itemType;
			
			var itemSnapshot = ItemSnapshotCache.GetOrCreate(itemType);
			if (itemSnapshot == null) throw new Exception("Failed to create Item Snapshot for " + itemType.Name);

			Activated = true;
			OneTimeOnly = true;

			var itemName = itemSnapshot.NameNonNormalized;
			Title = "Stealing the Past";
			var description = new StringBuilder();
			description.Append("There are nobles across the land with more gold than sense, and they'll pay handsomely for relics rumored to lie hidden within the deepest dungeons. That is where you come in.<br><br>");
			description.Append("I need a thief with enough cunning to slip past monsters, traps, and rival adventurers, find the relic, and bring it back to me. Deliver the prize to the Guild, and I'll see that you receive your fee.<br><br>");
			description.Append("Understand this: I commission only one relic at a time. These treasures are rare, and there's no guarantee they'll still be waiting when you arrive. Another thief may have claimed it before you, leaving you to wait until it resurfaces.<br><br>");
			description.Append("So don't waste time. Royalty is impatient, and their eagerness is where our profit lies!");
			Description = description.ToString();

			var refusal = new StringBuilder();
			refusal.Append("Very well. If you lack the nerve, I'll find another thief who doesn't.<br><br>");
			refusal.Append("The offer stands for now, but don't expect the prize to wait for you. When you've found your courage - or your greed - come back to me. If the relic is still available, there may yet be coin in it for you.");
			RefusalMessage = refusal.ToString();

			var completion = new StringBuilder();
			completion.Append("Hand it over. The noble who commissioned it is already eager to pay, and I'm not one to keep a client waiting.<br><br>");
			completion.Append("*The thief inspects the item*<br>");
			completion.Append("Now that's what I like to see... Yeah... a fine bit of work...<br><br>");
			completion.Append("You found the relic, brought it back, and you even managed to not to get yourself killed along the way. You did better than the last guy.<br><br>");
			completion.Append("As promised, here's your fee. Spend it wisely - or don't. That's your affair.<br><br>");
			completion.Append("If you're looking for another job, keep your ears open. There's always someone willing to pay for what others are foolish enough to leave behind.");
			CompletionMessage = completion.ToString();

			var inProgress = new StringBuilder();
			inProgress.Append("Mark the item as a quest item when you are ready.<br><br>");
			inProgress.Append("- Click yourself to view your Quest Log<br>");
			inProgress.Append("- Click the reticle next to the quest<br>");
			inProgress.Append(string.Format("- Target the {0}<br>", itemName));
			inProgress.Append("- Return to the Thief Guildmaster");
			InProgressMessage = inProgress.ToString();

			Objectives.Add(new DummyObjective("Locate and acquire the relic:"));
			Objectives.Add(new CollectObjective(1, itemType, itemName));
			Objectives.Add(new StealLocationObjective(itemType));

			var value = (int)(itemSnapshot.CoinPrice * (MyServerSettings.GetGoldCutRate() * .01));
			Rewards.Add(new ItemReward("Gold Coins", typeof(Gold), value));
			Rewards.Add(new FameReward(itemSnapshot.CoinPrice));
		}

		public override void OnRewardClaimed(MLQuestInstance instance)
		{
			foreach (var reward in Rewards)
			{
				var fameReward = reward as FameReward;
				if (fameReward == null) continue;

				fameReward.Give(instance.Player);
			}
		}

		public override Type QuestRecipient { get { return typeof(ThiefGuildmaster); } }

		public override IEnumerable<Type> GetQuestGivers()
		{
			yield return QuestRecipient;
		}

		public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
		{
			if (!base.CanOffer(quester, pm, context, message)) return false;

			if (pm.NpcGuild != NpcGuild.ThievesGuild)
			{
				if (message)
					MLQuestSystem.Tell(quester, pm, "This job is for guild members only.");

				return false;
			}

			return true;
		}

		private class StealLocationObjective : DummyObjective
		{
			private readonly Map m_Map;
			private readonly Point3D m_Location;

			public StealLocationObjective(Type itemType)
				: base("")
			{
				var entry = StealableArtifactsSpawner.Entries.FirstOrDefault(e => e.Type == itemType);
				if (entry == null) throw new Exception("Failed to find entry for " + itemType.Name);

				m_Map = entry.Map;
				m_Location = entry.Location;
			}

			public override void WriteToGump(Gump g, ref int y)
			{
				var region = Region.Find(m_Location, m_Map);
				var regionName = region != null && !string.IsNullOrEmpty(region.Name) ? region.Name : "Unknown";
				g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, string.Format("   {0}", regionName));
				y += 16;
				
				var landName = Server.Lands.LandName( Server.Lands.GetLand( m_Map, m_Location, m_Location.X, m_Location.Y ) );
				g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, string.Format("   {0}", landName));
				y += 16;
			}
		}

		public class RockQuest : StealingThePastQuest
		{
			public override Type NextQuest { get { return typeof(SkullCandleQuest); } }

			public RockQuest() : base(typeof(RockArtifact))
			{
			}
		}

		public class SkullCandleQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(RockQuest); } }
			public override Type NextQuest { get { return typeof(BottleQuest); } }

			public SkullCandleQuest() : base(typeof(SkullCandleArtifact))
			{
			}
		}

		public class BottleQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(SkullCandleQuest); } }
			public override Type NextQuest { get { return typeof(DamagedBooksQuest); } }

			public BottleQuest() : base(typeof(BottleArtifact))
			{
			}
		}

		public class DamagedBooksQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(BottleQuest); } }
			public override Type NextQuest { get { return typeof(StretchedHideQuest); } }

			public DamagedBooksQuest() : base(typeof(DamagedBooksArtifact))
			{
			}
		}

		public class StretchedHideQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(DamagedBooksQuest); } }
			public override Type NextQuest { get { return typeof(BrazierQuest); } }

			public StretchedHideQuest() : base(typeof(StretchedHideArtifact))
			{
			}
		}

		public class BrazierQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(StretchedHideQuest); } }
			public override Type NextQuest { get { return typeof(LampPostQuest); } }

			public BrazierQuest() : base(typeof(BrazierArtifact))
			{
			}
		}

		public class LampPostQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(BrazierQuest); } }
			public override Type NextQuest { get { return typeof(BooksNorthQuest); } }

			public LampPostQuest() : base(typeof(LampPostArtifact))
			{
			}
		}

		public class BooksNorthQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(LampPostQuest); } }
			public override Type NextQuest { get { return typeof(BooksWestQuest); } }

			public BooksNorthQuest() : base(typeof(BooksNorthArtifact))
			{
			}
		}

		public class BooksWestQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(BooksNorthQuest); } }
			public override Type NextQuest { get { return typeof(BooksFaceDownQuest); } }

			public BooksWestQuest() : base(typeof(BooksWestArtifact))
			{
			}
		}

		public class BooksFaceDownQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(BooksWestQuest); } }
			public override Type NextQuest { get { return typeof(StuddedLeggingsQuest); } }

			public BooksFaceDownQuest() : base(typeof(BooksFaceDownArtifact))
			{
			}
		}

		public class StuddedLeggingsQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(BooksFaceDownQuest); } }
			public override Type NextQuest { get { return typeof(EggCaseQuest); } }

			public StuddedLeggingsQuest() : base(typeof(StuddedLeggingsArtifact))
			{
			}
		}

		public class EggCaseQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(StuddedLeggingsQuest); } }
			public override Type NextQuest { get { return typeof(SkinnedGoatQuest); } }

			public EggCaseQuest() : base(typeof(EggCaseArtifact))
			{
			}
		}

		public class SkinnedGoatQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(EggCaseQuest); } }
			public override Type NextQuest { get { return typeof(GruesomeStandardQuest); } }

			public SkinnedGoatQuest() : base(typeof(SkinnedGoatArtifact))
			{
			}
		}

		public class GruesomeStandardQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(SkinnedGoatQuest); } }
			public override Type NextQuest { get { return typeof(BloodyWaterQuest); } }

			public GruesomeStandardQuest() : base(typeof(GruesomeStandardArtifact))
			{
			}
		}

		public class BloodyWaterQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(GruesomeStandardQuest); } }
			public override Type NextQuest { get { return typeof(TarotCardsQuest); } }

			public BloodyWaterQuest() : base(typeof(BloodyWaterArtifact))
			{
			}
		}

		public class TarotCardsQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(BloodyWaterQuest); } }
			public override Type NextQuest { get { return typeof(BackpackQuest); } }

			public TarotCardsQuest() : base(typeof(TarotCardsArtifact))
			{
			}
		}

		public class BackpackQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(TarotCardsQuest); } }
			public override Type NextQuest { get { return typeof(StuddedTunicQuest); } }

			public BackpackQuest() : base(typeof(BackpackArtifact))
			{
			}
		}

		public class StuddedTunicQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(BackpackQuest); } }
			public override Type NextQuest { get { return typeof(CocoonQuest); } }

			public StuddedTunicQuest() : base(typeof(StuddedTunicArtifact))
			{
			}
		}

		public class CocoonQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(StuddedTunicQuest); } }
			public override Type NextQuest { get { return typeof(SkinnedDeerQuest); } }

			public CocoonQuest() : base(typeof(CocoonArtifact))
			{
			}
		}

		public class SkinnedDeerQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(CocoonQuest); } }
			public override Type NextQuest { get { return typeof(SaddleQuest); } }

			public SkinnedDeerQuest() : base(typeof(SkinnedDeerArtifact))
			{
			}
		}

		public class SaddleQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(SkinnedDeerQuest); } }
			public override Type NextQuest { get { return typeof(LeatherTunicQuest); } }

			public SaddleQuest() : base(typeof(SaddleArtifact))
			{
			}
		}

		public class LeatherTunicQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(SaddleQuest); } }
			public override Type NextQuest { get { return typeof(RuinedPaintingQuest); } }

			public LeatherTunicQuest() : base(typeof(LeatherTunicArtifact))
			{
			}
		}

		public class RuinedPaintingQuest : StealingThePastQuest
		{
			public override Type PrerequisiteQuest { get { return typeof(LeatherTunicQuest); } }

			public RuinedPaintingQuest() : base(typeof(RuinedPaintingArtifact))
			{
			}
		}
	}
}
