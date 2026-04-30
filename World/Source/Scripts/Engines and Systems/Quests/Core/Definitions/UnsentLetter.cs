using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.ContextMenus;
using Server.Engines.MLQuests.Gumps;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Gumps;
using Server.Items;
using Server.Localization;
using Server.Mobiles;
using Server.Network;
using Server.Accounting;
using Server.Engines.MLQuests;
using Server.Engines.MLQuests.Definitions;

namespace Server.Engines.MLQuests
{
	/// <summary>
	/// Investigation / test logging for "The Unsent Letter" only. Default off; set Enabled true while debugging.
	/// Writes Console line [UnsentLetter] ... and appends to Logs/unsent-letter-quest-dev.log (Core.BaseDirectory).
	/// </summary>
	public static class UnsentLetterDevLog
	{
		public static bool Enabled;

		private static readonly object m_SyncRoot = new object();

		public static void Write(PlayerMobile pm, string phase, string detail)
		{
			// if (!Enabled)
			// 	return;

			string account = "?";
			if (pm?.Account != null && !string.IsNullOrEmpty(pm.Account.Username))
				account = pm.Account.Username;
			string who = pm == null || pm.Deleted ? "(null mobile)" : string.Format("\"{0}\" serial={1} acct={2}", pm.Name ?? "?", pm.Serial.Value, account);
			string msg = detail ?? string.Empty;

			if (80 < msg.Length)
				msg = msg.Substring(0, 77) + "...";

			string line = string.Format("{0:o} [{1}] {2}: {3}", DateTime.UtcNow, who, phase, msg);
			Console.WriteLine("[UnsentLetter] " + line);

			lock (m_SyncRoot)
			{
				try
				{
					string dir = Path.Combine(Core.BaseDirectory, "Logs");

					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);

					string path = Path.Combine(dir, "unsent-letter-quest-dev.log");

					using (StreamWriter w = File.AppendText(path))
						w.WriteLine(line);
				}
				catch
				{
				}
			}
		}
	}
}

namespace Server.Engines.MLQuests.Definitions
{
	/// <summary>Player-visible story beat for The Unsent Letter (derived from ML objective chain).</summary>
	public enum UnsentLetterQuestPhase
	{
		None,
		SpeakFamily,
		Puzzle,
		Ambush,
		Evidence,
		Clerk,
		Ending,
		ClaimReward
	}

	public static class UnsentLetterQuestHelper
	{
		public static MLQuestInstance FindInstance(PlayerMobile pm)
		{
			if (pm == null)
				return null;

			var ctx = MLQuestSystem.GetOrCreateContext(pm);
			foreach (MLQuestInstance qi in ctx.QuestInstances)
			{
				if (qi.Removed || qi.ClaimReward)
					continue;

				if (qi.Quest is UnsentLetterQuest)
					return qi;
			}

			return null;
		}

		/// <summary>Includes reward-pending instances (<see cref="MLQuestInstance.ClaimReward"/>).</summary>
		public static MLQuestInstance FindAnyUnsentLetterInstance(PlayerMobile pm)
		{
			if (pm == null)
				return null;

			var ctx = MLQuestSystem.GetOrCreateContext(pm);
			foreach (MLQuestInstance qi in ctx.QuestInstances)
			{
				if (qi.Removed)
					continue;

				if (qi.Quest is UnsentLetterQuest)
					return qi;
			}

			return null;
		}

		/// <summary>Mara ending branch: 1 = public, 2 = private, 3 = quiet.</summary>
		public static int GetFamilyEndingChoice(MLQuestInstance inst)
		{
			if (inst?.Objectives == null)
				return 0;

			foreach (BaseObjectiveInstance o in inst.Objectives)
			{
				if (o is UnsentFamilyEndingObjectiveInstance e)
					return e.EndingChoice;
			}

			return 0;
		}

		public static T FindIncompleteObjective<T>(MLQuestInstance inst) where T : BaseObjectiveInstance
		{
			if (inst == null)
				return null;

			foreach (BaseObjectiveInstance o in inst.Objectives)
			{
				if (o is T typed && !typed.IsCompleted())
					return typed;
			}

			return null;
		}

		public static UnsentLetterQuestPhase GetCurrentPhase(PlayerMobile pm)
		{
			MLQuestInstance any = FindAnyUnsentLetterInstance(pm);
			if (any == null || any.Removed)
				return UnsentLetterQuestPhase.None;

			if (any.ClaimReward)
				return UnsentLetterQuestPhase.ClaimReward;

			MLQuestInstance inst = FindInstance(pm);
			if (inst == null)
				return UnsentLetterQuestPhase.None;

			if (FindIncompleteObjective<UnsentSpeakFamilyObjectiveInstance>(inst) != null)
				return UnsentLetterQuestPhase.SpeakFamily;
			if (FindIncompleteObjective<UnsentPuzzleObjectiveInstance>(inst) != null)
				return UnsentLetterQuestPhase.Puzzle;
			if (FindIncompleteObjective<UnsentAmbushObjectiveInstance>(inst) != null)
				return UnsentLetterQuestPhase.Ambush;
			if (FindIncompleteObjective<UnsentEvidenceObjectiveInstance>(inst) != null)
				return UnsentLetterQuestPhase.Evidence;
			if (FindIncompleteObjective<UnsentClerkObjectiveInstance>(inst) != null)
				return UnsentLetterQuestPhase.Clerk;
			if (FindIncompleteObjective<UnsentFamilyEndingObjectiveInstance>(inst) != null)
				return UnsentLetterQuestPhase.Ending;

			if (inst.IsCompleted())
				return UnsentLetterQuestPhase.ClaimReward;

			return UnsentLetterQuestPhase.None;
		}

		public static T FindObjective<T>(PlayerMobile pm) where T : BaseObjectiveInstance
		{
			var inst = FindInstance(pm);
			if (inst == null)
				return null;

			foreach (var o in inst.Objectives)
			{
				if (o is T typed && !typed.IsCompleted())
					return typed;
			}

			return null;
		}

		/// <summary>Find first objective of type T even if completed (e.g. multi-step dialogue).</summary>
		public static T FindObjectiveAny<T>(PlayerMobile pm) where T : BaseObjectiveInstance
		{
			var inst = FindInstance(pm);
			if (inst == null)
				return null;

			foreach (var o in inst.Objectives)
			{
				if (o is T typed)
					return typed;
			}

			return null;
		}
	}

	/// <summary>Resolve Unsent Letter logical keys (<c>quest-…</c>) or legacy hash-backed English for <see cref="StringCatalog.TryResolveLogicalOrHash"/>.</summary>
	public static class UnsentLetterI18n
	{
		public static string Resolve(PlayerMobile pm, string keyOrEnglish)
		{
			if (string.IsNullOrEmpty(keyOrEnglish))
				return string.Empty;

			if (pm?.Account == null)
				return keyOrEnglish;

			string lang = AccountLang.GetLanguageCode(pm.Account);
			string r = StringCatalog.TryResolveLogicalOrHash(lang, keyOrEnglish) ?? keyOrEnglish;

			if (AccountLang.IsChinese(lang))
				r = QuestCompositeResolver.ResolveComposite(pm, r);

			return r;
		}

		public static void SendMessageLocalized(PlayerMobile pm, string keyOrEnglish)
		{
			if (pm == null)
				return;

			pm.SendMessage(Resolve(pm, keyOrEnglish));
		}

		public static string ResolveDefaultLang(string keyOrEnglish)
		{
			if (string.IsNullOrEmpty(keyOrEnglish))
				return string.Empty;

			return StringCatalog.TryResolveLogicalOrHash(LangConfig.DefaultLanguage, keyOrEnglish) ?? keyOrEnglish;
		}
	}
}

namespace Server.Engines.MLQuests.Objectives
{
	#region Speak with family — Mara, Lina, Thomas

	public sealed class UnsentSpeakFamilyObjective : BaseObjective
	{
		public UnsentSpeakFamilyObjective()
		{
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, BaseQuestGump.ResolveQuestCatalogString(g,
				"quest-unsent-letter-objective-speak-family-001"));
			y += 16;
		}

		public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
		{
			return new UnsentSpeakFamilyObjectiveInstance(this, instance);
		}
	}

	public sealed class UnsentSpeakFamilyObjectiveInstance : BaseObjectiveInstance, IDeserializable
	{
		public const int Version = 1;

		private readonly UnsentSpeakFamilyObjective m_Objective;

		public bool HeardMara { get; set; }
		public bool HeardLina { get; set; }
		public bool HeardThomas { get; set; }

		public UnsentSpeakFamilyObjectiveInstance(UnsentSpeakFamilyObjective def, MLQuestInstance instance)
			: base(instance, def)
		{
			m_Objective = def;
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			m_Objective.WriteToGump(g, ref y);
			base.WriteToGump(g, ref y);
		}

		public override bool IsCompleted()
		{
			return HeardMara && HeardLina && HeardThomas;
		}

		public void MarkHeard(PlayerMobile pm, int which)
		{
			switch (which)
			{
				case 0: HeardMara = true; break;
				case 1: HeardLina = true; break;
				case 2: HeardThomas = true; break;
			}

			UnsentLetterDevLog.Write(pm, "speak_family",
				string.Format("mark which={0}, mara={1}, lina={2}, thomas={3}, complete={4}", which, HeardMara, HeardLina, HeardThomas, IsCompleted()));

			CheckComplete();
		}

		public override void OnQuestAccepted()
		{
			base.OnQuestAccepted();
			UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "lifecycle", "speak_family objective accepted");
		}

		public override void OnQuestCancelled()
		{
			UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "lifecycle", "speak_family objective OnQuestCancelled");
			base.OnQuestCancelled();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Version);
			writer.Write(HeardMara);
			writer.Write(HeardLina);
			writer.Write(HeardThomas);
		}

		public void Deserialize(GenericReader reader)
		{
			int v = reader.ReadInt();
			HeardMara = reader.ReadBool();
			HeardLina = reader.ReadBool();
			HeardThomas = reader.ReadBool();
		}
	}

	#endregion

	#region Testimony puzzle — two of three keyword checks with Mara

	public sealed class UnsentPuzzleObjective : BaseObjective
	{
		public override void WriteToGump(Gump g, ref int y)
		{
			g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, BaseQuestGump.ResolveQuestCatalogString(g,
				"quest-unsent-letter-objective-puzzle-001"));
			y += 16;
		}

		public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
		{
			return new UnsentPuzzleObjectiveInstance(this, instance);
		}
	}

	public sealed class UnsentPuzzleObjectiveInstance : BaseObjectiveInstance, IDeserializable
	{
		public const int Version = 1;

		private readonly UnsentPuzzleObjective m_Objective;

		public bool Started { get; set; }
		public int Step { get; set; }
		public int Correct { get; set; }
		public int Wrong { get; set; }

		public UnsentPuzzleObjectiveInstance(UnsentPuzzleObjective def, MLQuestInstance instance)
			: base(instance, def)
		{
			m_Objective = def;
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			m_Objective.WriteToGump(g, ref y);
			base.WriteToGump(g, ref y);
		}

		public override bool IsCompleted()
		{
			return Started && Step > 3;
		}

		public void TryStart()
		{
			if (!Started)
			{
				Started = true;
				Step = 1;
				UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "puzzle", "TryStart puzzle phase open");
			}
		}

		public void TryAnswer(string speech)
		{
			if (!Started || Step < 1 || Step > 3)
				return;

			string s = speech.ToLowerInvariant();

			if (Step == 1)
			{
				if (s.IndexOf("montor", StringComparison.Ordinal) >= 0 || s.IndexOf("road", StringComparison.Ordinal) >= 0
					|| speech.IndexOf("\u8499\u6258", StringComparison.Ordinal) >= 0)
					Correct++;
				else
					Wrong++;

				Step++;
			}
			else if (Step == 2)
			{
				if (s.IndexOf("winter", StringComparison.Ordinal) >= 0
					|| speech.IndexOf("\u5bd2\u51ac", StringComparison.Ordinal) >= 0
					|| speech.IndexOf("\u51ac\u5b63", StringComparison.Ordinal) >= 0
					|| speech.IndexOf("\u51ac\u65e5", StringComparison.Ordinal) >= 0
					|| speech.IndexOf("\u6df1\u51ac", StringComparison.Ordinal) >= 0)
					Correct++;
				else
					Wrong++;

				Step++;
			}
			else if (Step == 3)
			{
				if (s.IndexOf("silence", StringComparison.Ordinal) >= 0 || s.IndexOf("order", StringComparison.Ordinal) >= 0
					|| speech.IndexOf("\u7ede\u9ed8", StringComparison.Ordinal) >= 0 || speech.IndexOf("\u6c89\u9ed8", StringComparison.Ordinal) >= 0
					|| speech.IndexOf("\u5bc2\u7136", StringComparison.Ordinal) >= 0)
					Correct++;
				else
					Wrong++;

				Step++;
			}

			UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "puzzle_after_q",
				string.Format("speech=\"{0}\" step={1} correct={2} wrong={3}",
					speech != null ? (speech.Length > 48 ? speech.Substring(0, 48) + "..." : speech) : "",
					Step,
					Correct,
					Wrong));

			if (Step > 3)
			{
				UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "puzzle",
					string.Format("puzzle_objective_completed correct={0} wrong={1}", Correct, Wrong));
				CheckComplete();
			}
		}
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Version);
			writer.Write(Started);
			writer.Write(Step);
			writer.Write(Correct);
			writer.Write(Wrong);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.ReadInt();
			Started = reader.ReadBool();
			Step = reader.ReadInt();
			Correct = reader.ReadInt();
			Wrong = reader.ReadInt();
		}
	}

	#endregion

	#region Montor road — talk miner, two waves of grey cloaks

	public sealed class UnsentAmbushObjective : BaseObjective
	{
		public override void WriteToGump(Gump g, ref int y)
		{
			g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, BaseQuestGump.ResolveQuestCatalogString(g,
				"quest-unsent-letter-objective-ambush-001"));
			y += 16;
		}

		public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
		{
			return new UnsentAmbushObjectiveInstance(this, instance);
		}
	}

	public sealed class UnsentAmbushObjectiveInstance : BaseObjectiveInstance, IDeserializable
	{
		public const int Version = 1;

		private readonly UnsentAmbushObjective m_Objective;

		public int Phase { get; set; }
		public int Remaining { get; set; }

		public UnsentAmbushObjectiveInstance(UnsentAmbushObjective def, MLQuestInstance instance)
			: base(instance, def)
		{
			m_Objective = def;
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			m_Objective.WriteToGump(g, ref y);
			base.WriteToGump(g, ref y);
		}

		public override bool IsCompleted()
		{
			return Phase >= 3;
		}

		public void BeginAmbush(Server.Mobiles.UnsentGreyCloakBrigand.SpawnerBridge bridge)
		{
			if (Phase != 0)
				return;

			Phase = 1;
			int c0 = UnsentLetterQuestPackRuntime.AmbushWaveCount(0);
			Remaining = c0;
			bridge.SpawnWave(c0, Phase);
			UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "ambush", string.Format("BeginAmbush phase=1 remaining={0} (wave spawn)", c0));
		}

		public void OnBrigandKilled(PlayerMobile pm)
		{
			if (Phase == 0 || Phase >= 3)
				return;

			Remaining--;
			UnsentLetterDevLog.Write(pm, "ambush_combat", string.Format("brigand_killed phase={0} remaining={1}", Phase, Remaining));

			if (Remaining > 0)
				return;

			if (Phase == 1)
			{
				Phase = 2;
				int c1 = UnsentLetterQuestPackRuntime.AmbushWaveCount(1);
				Remaining = c1;
				UnsentGreyCloakBrigand.SpawnWaveFor(pm, c1, Phase);
				UnsentLetterDevLog.Write(pm, "ambush", string.Format("wave2 started phase=2 remaining={0}", c1));
			}
			else if (Phase == 2)
			{
				DropEvidence(pm);
				Phase = 3;
				UnsentLetterDevLog.Write(pm, "ambush", "evidence_given phase=3");
				CheckComplete();
			}
		}

		private static void DropEvidence(PlayerMobile pm)
		{
			var i1 = new UnsentAdrianBadge();
			var i2 = new UnsentTornLetterPage();
			UnsentLetterQuestPackRuntime.ApplyQuestItem(i1, UnsentLetterQuestPackRuntime.ItemConfigForBadge());
			UnsentLetterQuestPackRuntime.ApplyQuestItem(i2, UnsentLetterQuestPackRuntime.ItemConfigForTornPage());
			pm.PlaySound(0x5B4);
			pm.AddToBackpack(i1);
			pm.AddToBackpack(i2);
			UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-ambush-loot-001");
			UnsentLetterDevLog.Write(pm, "ambush_evidence", "Dropped badge+torn_page into backpack");
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Version);
			writer.Write(Phase);
			writer.Write(Remaining);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.ReadInt();
			Phase = reader.ReadInt();
			Remaining = reader.ReadInt();
		}
	}

	#endregion

	#region Evidence — backpack has both items, then tell Lina

	public sealed class UnsentEvidenceObjective : BaseObjective
	{
		public override void WriteToGump(Gump g, ref int y)
		{
			g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, BaseQuestGump.ResolveQuestCatalogString(g,
				"quest-unsent-letter-objective-evidence-001"));
			y += 16;
		}

		public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
		{
			return new UnsentEvidenceObjectiveInstance(this, instance);
		}
	}

	public sealed class UnsentEvidenceObjectiveInstance : BaseObjectiveInstance, IDeserializable
	{
		public const int Version = 1;

		private readonly UnsentEvidenceObjective m_Objective;

		public bool LinaTold { get; set; }

		public UnsentEvidenceObjectiveInstance(UnsentEvidenceObjective def, MLQuestInstance instance)
			: base(instance, def)
		{
			m_Objective = def;
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			m_Objective.WriteToGump(g, ref y);
			base.WriteToGump(g, ref y);
		}

		private static bool ContainerHasItem(Container c, Type t)
		{
			if (c == null)
				return false;

			foreach (Item it in c.Items)
			{
				if (t.IsAssignableFrom(it.GetType()))
					return true;

				if (it is Container sub && ContainerHasItem(sub, t))
					return true;
			}

			return false;
		}

		private static bool HasItems(PlayerMobile pm)
		{
			Container pack = pm.Backpack;
			return pack != null
				&& ContainerHasItem(pack, typeof(UnsentAdrianBadge))
				&& ContainerHasItem(pack, typeof(UnsentTornLetterPage));
		}

		public override bool IsCompleted()
		{
			return HasItems(Instance.Player) && LinaTold;
		}

		public void MarkLina()
		{
			LinaTold = true;
			UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "evidence",
				string.Format("LinaTold=true has_items={0}", HasItems(Instance.Player)));

			CheckComplete();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Version);
			writer.Write(LinaTold);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.ReadInt();
			LinaTold = reader.ReadBool();
		}
	}

	#endregion

	#region Renika clerk — speech then hirelings

	public sealed class UnsentClerkObjective : BaseObjective
	{
		public override void WriteToGump(Gump g, ref int y)
		{
			g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, BaseQuestGump.ResolveQuestCatalogString(g,
				"quest-unsent-letter-objective-clerk-001"));
			y += 16;
		}

		public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
		{
			return new UnsentClerkObjectiveInstance(this, instance);
		}
	}

	public sealed class UnsentClerkObjectiveInstance : BaseObjectiveInstance, IDeserializable
	{
		public const int Version = 1;

		private readonly UnsentClerkObjective m_Objective;

		public int HirelingsLeft { get; set; }
		public bool FightStarted { get; set; }

		public UnsentClerkObjectiveInstance(UnsentClerkObjective def, MLQuestInstance instance)
			: base(instance, def)
		{
			m_Objective = def;
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			m_Objective.WriteToGump(g, ref y);
			base.WriteToGump(g, ref y);
		}

		public override bool IsCompleted()
		{
			return FightStarted && HirelingsLeft <= 0;
		}

		public void StartFight(PlayerMobile pm, Point3D loc, Map map)
		{
			if (FightStarted)
				return;

			FightStarted = true;
			int n = UnsentLetterQuestPackRuntime.ClerkHirelingCount();
			HirelingsLeft = n;

			for (int i = 0; i < n; i++)
			{
				Mobile h = UnsentLetterQuestPackRuntime.CreateHirelingForClerk(pm);

				if (h == null)
					continue;

				Point3D off = UnsentLetterQuestPackRuntime.ClerkOffset(i);
				h.MoveToWorld(new Point3D(loc.X + off.X, loc.Y + off.Y, loc.Z), map);
				h.Combatant = pm;
				h.FocusMob = pm;
			}

			UnsentLetterDevLog.Write(pm, "clerk_fight",
				string.Format("StartFight hires={0} at {1},{2},{3}", n, loc.X, loc.Y, map));
		}

		public void OnHirelingDeath(PlayerMobile pm)
		{
			if (HirelingsLeft <= 0)
				return;

			HirelingsLeft--;
			UnsentLetterDevLog.Write(pm, "clerk_fight", string.Format("hireling_down left={0}", HirelingsLeft));

			if (HirelingsLeft == 0)
			{
				var letter = new UnsentFullLetter();
				UnsentLetterQuestPackRuntime.ApplyQuestItem(letter, UnsentLetterQuestPackRuntime.ItemConfigForFullLetter());
				pm.AddToBackpack(letter);
				UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-clerk-letter-001");
				UnsentLetterDevLog.Write(pm, "clerk_fight", "all hirelings defeated; full letter awarded");
				CheckComplete();
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Version);
			writer.Write(HirelingsLeft);
			writer.Write(FightStarted);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.ReadInt();
			HirelingsLeft = reader.ReadInt();
			FightStarted = reader.ReadBool();
		}
	}

	#endregion

	#region Return to Britain — two family talks + ending keywords

	public sealed class UnsentFamilyEndingObjective : BaseObjective
	{
		public override void WriteToGump(Gump g, ref int y)
		{
			g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, BaseQuestGump.ResolveQuestCatalogString(g,
				"quest-unsent-letter-objective-ending-001"));
			y += 16;
			g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, BaseQuestGump.ResolveQuestCatalogString(g,
				"quest-unsent-letter-objective-ending-002"));
			y += 16;
		}

		public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
		{
			return new UnsentFamilyEndingObjectiveInstance(this, instance);
		}
	}

	public sealed class UnsentFamilyEndingObjectiveInstance : BaseObjectiveInstance, IDeserializable
	{
		public const int Version = 1;

		private readonly UnsentFamilyEndingObjective m_Objective;

		public int FamilyTalks { get; set; }
		public int EndingChoice { get; set; }

		public UnsentFamilyEndingObjectiveInstance(UnsentFamilyEndingObjective def, MLQuestInstance instance)
			: base(instance, def)
		{
			m_Objective = def;
		}

		public override void WriteToGump(Gump g, ref int y)
		{
			m_Objective.WriteToGump(g, ref y);
			base.WriteToGump(g, ref y);
		}

		public override bool IsCompleted()
		{
			if (!HasFullLetter(Instance.Player))
				return false;

			return FamilyTalks >= 2 && EndingChoice != 0;
		}

		private static bool ContainerHasFullLetterRecursive(Container c, Type t)
		{
			if (c == null)
				return false;

			foreach (Item it in c.Items)
			{
				if (t.IsAssignableFrom(it.GetType()))
					return true;

				if (it is Container sub && ContainerHasFullLetterRecursive(sub, t))
					return true;
			}

			return false;
		}

		private static bool HasFullLetter(PlayerMobile pm)
		{
			var pack = pm.Backpack;

			return pack != null && ContainerHasFullLetterRecursive(pack, typeof(UnsentFullLetter));
		}

		public void AddFamilyTalk()
		{
			if (FamilyTalks < 3)
				FamilyTalks++;

			UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "ending",
				string.Format("AddFamilyTalk count={0} endingChoice={1} hasLetter={2}", FamilyTalks, EndingChoice, HasFullLetter(Instance.Player)));

			CheckComplete();
		}

		public void SetEnding(int choice)
		{
			EndingChoice = choice;
			UnsentLetterDevLog.Write(Instance.Player as PlayerMobile, "ending", string.Format("SetEnding choice={0} talks={1}", choice, FamilyTalks));
			CheckComplete();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Version);
			writer.Write(FamilyTalks);
			writer.Write(EndingChoice);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.ReadInt();
			FamilyTalks = reader.ReadInt();
			EndingChoice = reader.ReadInt();
		}
	}

	#endregion
}

namespace Server.Mobiles
{
	public class UnsentGreyCloakBrigand : Brigand
	{
		public PlayerMobile QuestPlayer { get; set; }

		[Constructable]
		public UnsentGreyCloakBrigand()
		{
			Title = UnsentLetterI18n.ResolveDefaultLang("quest-unsent-letter-npc-brigand-title-001");
		}

		public UnsentGreyCloakBrigand(Serial serial)
			: base(serial)
		{
		}

		public override void OnDeath(Container c)
		{
			if (QuestPlayer != null)
			{
				var amb = Server.Engines.MLQuests.Definitions.UnsentLetterQuestHelper.FindObjective<Server.Engines.MLQuests.Objectives.UnsentAmbushObjectiveInstance>(QuestPlayer);
				amb?.OnBrigandKilled(QuestPlayer);
			}

			base.OnDeath(c);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(QuestPlayer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
			QuestPlayer = reader.ReadMobile() as PlayerMobile;
		}

		public static void SpawnWaveFor(PlayerMobile pm, int count, int wavePhase)
		{
			Point3D o = pm.Location;
			Map map = pm.Map;
			if (map == null || map == Map.Internal)
				return;

			for (int i = 0; i < count; i++)
			{
				Mobile m = UnsentLetterQuestPackRuntime.CreateBrigandForAmbush(pm, wavePhase);

				if (m == null)
					continue;

				UnsentLetterQuestPackRuntime.SpawnAmbushMobile(pm, m, wavePhase);
			}

			Server.Engines.MLQuests.UnsentLetterDevLog.Write(pm, "ambush_spawn", string.Format("count={0} wavePhase={1} at {2} {3}", count, wavePhase, o, map));
		}

		public sealed class SpawnerBridge
		{
			private readonly PlayerMobile m_Pm;

			public SpawnerBridge(PlayerMobile pm)
			{
				m_Pm = pm;
			}

			public void SpawnWave(int count, int phase)
			{
				SpawnWaveFor(m_Pm, count, phase);
			}
		}
	}

	public class UnsentHireling : Brigand
	{
		public PlayerMobile QuestPlayer { get; set; }

		[Constructable]
		public UnsentHireling()
		{
			Title = UnsentLetterI18n.ResolveDefaultLang("quest-unsent-letter-npc-hireling-title-001");
			SetHits(HitsMax + 40);
		}

		public UnsentHireling(Serial serial)
			: base(serial)
		{
		}

		public override void OnDeath(Container c)
		{
			if (QuestPlayer != null)
			{
				var clerk =
					Server.Engines.MLQuests.Definitions.UnsentLetterQuestHelper.FindObjective<Server.Engines.MLQuests.Objectives.UnsentClerkObjectiveInstance>(QuestPlayer);
				clerk?.OnHirelingDeath(QuestPlayer);
			}

			base.OnDeath(c);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(QuestPlayer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
			QuestPlayer = reader.ReadMobile() as PlayerMobile;
		}
	}
}

namespace Server.Items
{
	public sealed class UnsentFullLetterReadGump : Gump
	{
		public UnsentFullLetterReadGump(PlayerMobile pm)
			: base(50, 50)
		{
			if (pm != null)
				pm.PlaySound(0x55);

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage(0);

			AddImage(0, 0, 7005, 0x455);
			AddImage(0, 0, 7006);
			AddImage(0, 0, 7024, 2789);

			const string color = "#C8B090";
			string title =
				pm == null
					? string.Empty
					: Server.Engines.MLQuests.Definitions.UnsentLetterI18n.Resolve(pm, "quest-unsent-letter-item-full-letter-001");
			string body =
				pm == null
					? string.Empty
					: Server.Engines.MLQuests.Definitions.UnsentLetterI18n.Resolve(pm, "quest-unsent-letter-fullletter-text-001");

			AddHtml(
				48,
				42,
				510,
				54,
				string.Format("<BASEFONT COLOR={0}><CENTER><B>{1}</B></CENTER></BASEFONT>", color, HtmlEscape(title)),
				false,
				false);
			AddHtml(56, 92, 485, 312, string.Format("<BASEFONT COLOR={0}>{1}</BASEFONT>", color, body), false, true);
		}

		private static string HtmlEscape(string s)
		{
			if (string.IsNullOrEmpty(s))
				return string.Empty;

			return s
				.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
				.Replace("\"", "&quot;");
		}
	}

	public class UnsentAdrianBadge : Item
	{
		public override string DefaultName => "quest-unsent-letter-item-badge-001";

		[Constructable]
		public UnsentAdrianBadge()
			: base(0x14ED)
		{
			Hue = 2213;
			Weight = 1.0;
		}

		public UnsentAdrianBadge(Serial serial)
			: base(serial)
		{
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			const string key = "quest-unsent-letter-item-badge-001";
			PlayerMobile pm = RootParent as PlayerMobile;
			string resolved = key;

			if (pm != null && pm.Account != null)
			{
				string lang = AccountLang.GetLanguageCode(pm.Account);
				resolved = StringCatalog.TryResolveLogicalOrHash(lang, key) ?? key;

				if (AccountLang.IsChinese(lang))
					resolved = QuestCompositeResolver.ResolveComposite(pm, resolved);
			}

			list.Add(resolved);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	public class UnsentTornLetterPage : Item
	{
		public override string DefaultName => "quest-unsent-letter-item-torn-page-001";

		[Constructable]
		public UnsentTornLetterPage()
			: base(0x14ED)
		{
			Hue = 1150;
			Weight = 1.0;
		}

		public UnsentTornLetterPage(Serial serial)
			: base(serial)
		{
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			const string key = "quest-unsent-letter-item-torn-page-001";
			PlayerMobile pm = RootParent as PlayerMobile;
			string resolved = key;

			if (pm != null && pm.Account != null)
			{
				string lang = AccountLang.GetLanguageCode(pm.Account);
				resolved = StringCatalog.TryResolveLogicalOrHash(lang, key) ?? key;

				if (AccountLang.IsChinese(lang))
					resolved = QuestCompositeResolver.ResolveComposite(pm, resolved);
			}

			list.Add(resolved);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	public class UnsentFullLetter : Item
	{
		public override string DefaultName => "quest-unsent-letter-item-full-letter-001";

		[Constructable]
		public UnsentFullLetter()
			: base(0x14ED)
		{
			Hue = 1160;
			Weight = 1.0;
		}

		public UnsentFullLetter(Serial serial)
			: base(serial)
		{
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			const string key = "quest-unsent-letter-item-full-letter-001";
			PlayerMobile pm = RootParent as PlayerMobile;
			string resolved = key;

			if (pm != null && pm.Account != null)
			{
				string lang = AccountLang.GetLanguageCode(pm.Account);
				resolved = StringCatalog.TryResolveLogicalOrHash(lang, key) ?? key;

				if (AccountLang.IsChinese(lang))
					resolved = QuestCompositeResolver.ResolveComposite(pm, resolved);
			}

			list.Add(resolved);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!from.InRange(GetWorldLocation(), 4))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045);
				return;
			}

			PlayerMobile pm = from as PlayerMobile;

			if (pm == null)
				return;

			pm.SendGump(new UnsentFullLetterReadGump(pm));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	/// <summary>
	/// Quest reward: equippable middle-torso sash; hue and +3 Str/Dex/Int depend on Mara ending (1 public, 2 private, 3 quiet).
	/// </summary>
	[Flipable(0x1541, 0x1542)]
	public class UnsentLetterCounselSash : BaseMiddleTorso
	{
		private int m_Ending;

		public static string ResolveNameKey(int ending)
		{
			switch (NormalizeEnding(ending))
			{
				case 1:
					return "quest-unsent-letter-reward-memento-public-001";
				case 2:
					return "quest-unsent-letter-reward-memento-private-001";
				case 3:
					return "quest-unsent-letter-reward-memento-quiet-001";
				default:
					return "quest-unsent-letter-reward-memento-public-001";
			}
		}

		private static int NormalizeEnding(int ending)
		{
			if (ending < 1 || ending > 3)
				return 1;
			return ending;
		}

		private static int ResolveRareHue(int ending)
		{
			switch (NormalizeEnding(ending))
			{
				case 1:
					return 1366;
				case 2:
					return 1267;
				case 3:
					return 1175;
				default:
					return 1366;
			}
		}

		private void ApplyStatsAndHue()
		{
			int e = NormalizeEnding(m_Ending);
			Hue = ResolveRareHue(e);
			Attributes.BonusStr = 0;
			Attributes.BonusDex = 0;
			Attributes.BonusInt = 0;

			switch (e)
			{
				case 1:
					Attributes.BonusStr = 3;
					break;
				case 2:
					Attributes.BonusDex = 3;
					break;
				case 3:
					Attributes.BonusInt = 3;
					break;
			}
		}

		[Constructable]
		public UnsentLetterCounselSash()
			: this(1)
		{
		}

		[Constructable]
		public UnsentLetterCounselSash(int ending)
			: base(0x1541, 0)
		{
			m_Ending = NormalizeEnding(ending);
			Weight = 1.0;
			LootType = LootType.Blessed;
			ApplyStatsAndHue();
		}

		public UnsentLetterCounselSash(Serial serial)
			: base(serial)
		{
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			string key = ResolveNameKey(m_Ending);
			PlayerMobile pm = RootParent as PlayerMobile;
			string resolved = key;

			if (pm != null && pm.Account != null)
			{
				string lang = AccountLang.GetLanguageCode(pm.Account);
				resolved = StringCatalog.TryResolveLogicalOrHash(lang, key) ?? key;

				if (AccountLang.IsChinese(lang))
					resolved = QuestCompositeResolver.ResolveComposite(pm, resolved);
			}

			list.Add(resolved);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(1);
			writer.Write(m_Ending);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
			m_Ending = reader.ReadInt();

			if (m_Ending < 1 || m_Ending > 3)
				m_Ending = 1;

			ApplyStatsAndHue();
		}
	}
}

namespace Server.Engines.MLQuests.Definitions
{
	/// <summary>Context menu Talk (cliloc 6146) opens Unsent Letter RPG dialogue.</summary>
	public sealed class UnsentLetterTalkEntry : ContextMenuEntry
	{
		private readonly PlayerMobile m_From;
		private readonly Mobile m_Npc;

		public UnsentLetterTalkEntry(PlayerMobile from, Mobile npc)
			: base(6146, 12)
		{
			m_From = from;
			m_Npc = npc;
		}

		public override void OnClick()
		{
			UnsentLetterRpgDialogue.TryOpenFromTalk(m_From, m_Npc);
		}
	}

	[QuesterName("Mara")]
	public class UnsentLetterMara : BasePerson
	{
		[Constructable]
		public UnsentLetterMara()
			: base()
		{
			Name = "Mara";
			Female = true;
			Body = 0x191;
			AI = AIType.AI_Citizen;
			FightMode = FightMode.None;
			Blessed = true;
			CantWalk = false;
			Title = UnsentLetterI18n.ResolveDefaultLang("quest-unsent-letter-npc-mara-title-001");
		}

		public UnsentLetterMara(Serial serial)
			: base(serial)
		{
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.AddCustomContextEntries(from, list);
			if (from is PlayerMobile pm)
				list.Add(new UnsentLetterTalkEntry(pm, this));
		}

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	public class UnsentLetterLina : BasePerson
	{
		[Constructable]
		public UnsentLetterLina()
			: base()
		{
			Name = "Lina";
			Female = true;
			Body = 0x191;
			AI = AIType.AI_Citizen;
			FightMode = FightMode.None;
			Blessed = true;
			Title = null;
		}

		public UnsentLetterLina(Serial serial)
			: base(serial)
		{
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.AddCustomContextEntries(from, list);
			if (from is PlayerMobile pm)
				list.Add(new UnsentLetterTalkEntry(pm, this));
		}

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	public class UnsentLetterThomas : BasePerson
	{
		[Constructable]
		public UnsentLetterThomas()
			: base()
		{
			Name = "Thomas";
			Female = false;
			Body = 0x190;
			AI = AIType.AI_Citizen;
			FightMode = FightMode.None;
			Blessed = true;
			Title = UnsentLetterI18n.ResolveDefaultLang("quest-unsent-letter-npc-thomas-title-001");
		}

		public UnsentLetterThomas(Serial serial)
			: base(serial)
		{
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.AddCustomContextEntries(from, list);
			if (from is PlayerMobile pm)
				list.Add(new UnsentLetterTalkEntry(pm, this));
		}

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	public class UnsentLetterMiner : BasePerson
	{
		[Constructable]
		public UnsentLetterMiner()
			: base()
		{
			Name = "Henrick";
			Female = false;
			Body = 0x190;
			AI = AIType.AI_Citizen;
			FightMode = FightMode.None;
			Blessed = true;
			Title = UnsentLetterI18n.ResolveDefaultLang("quest-unsent-letter-npc-miner-title-001");
		}

		public UnsentLetterMiner(Serial serial)
			: base(serial)
		{
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.AddCustomContextEntries(from, list);
			if (from is PlayerMobile pm)
				list.Add(new UnsentLetterTalkEntry(pm, this));
		}

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	public class UnsentLetterClerk : BasePerson
	{
		[Constructable]
		public UnsentLetterClerk()
			: base()
		{
			Name = "Garron";
			Female = false;
			Body = 0x190;
			AI = AIType.AI_Citizen;
			FightMode = FightMode.None;
			Blessed = true;
			Title = UnsentLetterI18n.ResolveDefaultLang("quest-unsent-letter-npc-clerk-title-001");
		}

		public UnsentLetterClerk(Serial serial)
			: base(serial)
		{
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.AddCustomContextEntries(from, list);
			if (from is PlayerMobile pm)
				list.Add(new UnsentLetterTalkEntry(pm, this));
		}

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	public static class UnsentLetterQuestHandlers
	{
		public static void ApplyMaraFamilyTruth(PlayerMobile pm)
		{
			var famAny = UnsentLetterQuestHelper.FindObjectiveAny<UnsentSpeakFamilyObjectiveInstance>(pm);
			if (famAny == null || famAny.IsCompleted())
				return;

			famAny.MarkHeard(pm, 0);
			UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-mara-truth-001");
		}

		public static void ApplyMaraPuzzleStart(PlayerMobile pm)
		{
			var puzzle = UnsentLetterQuestHelper.FindObjective<UnsentPuzzleObjectiveInstance>(pm);
			if (puzzle == null || puzzle.Started)
				return;

			puzzle.TryStart();
		}

		public static void ApplyMaraPuzzleAnswer(PlayerMobile pm, string answerLine)
		{
			var puzzle = UnsentLetterQuestHelper.FindObjective<UnsentPuzzleObjectiveInstance>(pm);
			if (puzzle == null || !puzzle.Started || puzzle.Step < 1 || puzzle.Step > 3)
				return;

			int before = puzzle.Step;
			puzzle.TryAnswer(answerLine);

			if (puzzle.IsCompleted())
				return;

			if (before == 1 && puzzle.Step == 2)
				UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-puzzle-q2-001");
			else if (before == 2 && puzzle.Step == 3)
				UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-puzzle-q3-001");
		}

		public static void ApplyMaraEndingLetterTalk(PlayerMobile pm)
		{
			var end = UnsentLetterQuestHelper.FindObjective<UnsentFamilyEndingObjectiveInstance>(pm);
			if (end == null)
				return;

			end.AddFamilyTalk();
		}

		public static void ApplyMaraEndingChoice(PlayerMobile pm, int choice)
		{
			var end = UnsentLetterQuestHelper.FindObjective<UnsentFamilyEndingObjectiveInstance>(pm);
			if (end == null)
				return;

			end.SetEnding(choice);
			ScheduleUnsentLetterRewardChain(pm);
		}

		/// <summary>
		/// After Mara confirms the family's choice in <B>Talk</B>, open the same RPG reward sheet as the quest log turn-in (no extra Quests click).
		/// </summary>
		private static void ScheduleUnsentLetterRewardChain(PlayerMobile pm)
		{
			if (pm == null)
				return;

			PlayerMobile pmRef = pm;
			Server.Timer.DelayCall(TimeSpan.FromMilliseconds(250), () => TryBeginUnsentLetterRewardOffer(pmRef));
		}

		private static void TryBeginUnsentLetterRewardOffer(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
				return;

			MLQuestInstance inst = UnsentLetterQuestHelper.FindAnyUnsentLetterInstance(pm);

			if (inst != null && inst.Quest is UnsentLetterQuest && inst.IsCompleted() && !inst.ClaimReward && !inst.Removed)
				inst.ContinueReportBack(true);
		}

		public static void ApplyLinaFather(PlayerMobile pm)
		{
			var fam = UnsentLetterQuestHelper.FindObjective<UnsentSpeakFamilyObjectiveInstance>(pm);
			if (fam == null)
				return;

			fam.MarkHeard(pm, 1);
			UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-lina-father-001");
		}

		public static void ApplyLinaEvidence(PlayerMobile pm)
		{
			var ev = UnsentLetterQuestHelper.FindObjective<UnsentEvidenceObjectiveInstance>(pm);
			if (ev == null)
				return;

			ev.MarkLina();
			UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-lina-evidence-001");
		}

		public static void ApplyLinaEndingLetterTalk(PlayerMobile pm)
		{
			var end = UnsentLetterQuestHelper.FindObjective<UnsentFamilyEndingObjectiveInstance>(pm);
			if (end == null)
				return;

			end.AddFamilyTalk();
		}

		public static void ApplyThomasFamily(PlayerMobile pm)
		{
			var fam = UnsentLetterQuestHelper.FindObjective<UnsentSpeakFamilyObjectiveInstance>(pm);
			if (fam == null)
				return;

			fam.MarkHeard(pm, 2);
			UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-thomas-family-001");
		}

		public static void ApplyThomasEndingLetterTalk(PlayerMobile pm)
		{
			var end = UnsentLetterQuestHelper.FindObjective<UnsentFamilyEndingObjectiveInstance>(pm);
			if (end == null)
				return;

			end.AddFamilyTalk();
		}

		public static void BeginMinerEscort(PlayerMobile pm, UnsentLetterMiner miner)
		{
			var amb = UnsentLetterQuestHelper.FindObjective<UnsentAmbushObjectiveInstance>(pm);
			if (amb == null || amb.Phase != 0)
				return;

			amb.BeginAmbush(new UnsentGreyCloakBrigand.SpawnerBridge(pm));
			UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-miner-ambush-001");
		}

		public static void BeginClerkFight(PlayerMobile pm, UnsentLetterClerk clerk)
		{
			var c = UnsentLetterQuestHelper.FindObjective<UnsentClerkObjectiveInstance>(pm);
			if (c == null || c.IsCompleted())
				return;

			UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-handler-clerk-fight-001");
			c.StartFight(pm, clerk.Location, clerk.Map);
		}
	}

	public class UnsentLetterQuest : MLQuest
	{
		public UnsentLetterQuest()
		{
			Activated = true;
			HasRestartDelay = true;
			Title = "quest-unsent-letter-meta-title-001";

			Description = "quest-unsent-letter-meta-description-001";

			RefusalMessage = "quest-unsent-letter-meta-refusal-001";
			InProgressMessage = "quest-unsent-letter-meta-inprogress-001";
			CompletionMessage = "quest-unsent-letter-meta-completion-001";

			Objectives.Add(new UnsentSpeakFamilyObjective());
			Objectives.Add(new UnsentPuzzleObjective());
			Objectives.Add(new UnsentAmbushObjective());
			Objectives.Add(new UnsentEvidenceObjective());
			Objectives.Add(new UnsentClerkObjective());
			Objectives.Add(new UnsentFamilyEndingObjective());

			Rewards.Add(new DummyReward("quest-unsent-letter-reward-thanks-001"));
			Rewards.Add(new UnsentLetterEndingMementoReward());
			Rewards.Add(new ItemReward("quest-unsent-letter-reward-gold-caption-001", typeof(Gold), 500));

			ObjectiveType = ObjectiveType.All;

			CompletionNotice = CompletionNoticeShortReturn;
		}

		public override IEnumerable<Type> GetQuestGivers()
		{
			yield return typeof(UnsentLetterMara);
		}

		public override void SendOffer(IQuestGiver quester, PlayerMobile pm)
		{
			UnsentLetterRpgDialogue.SendQuestOffer(this, quester, pm);
		}

		public override void OnRefuse(IQuestGiver quester, PlayerMobile pm)
		{
			UnsentLetterRpgDialogue.SendQuestRefusal(this, quester, pm);
		}

		public override void GetRewards(MLQuestInstance instance)
		{
			if (NextQuest != null)
				base.GetRewards(instance);
			else
				UnsentLetterRpgDialogue.SendQuestRewardOffer(instance);
		}

		public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
		{
			return base.CanOffer(quester, pm, context, message);
		}

		public override TimeSpan GetRestartDelay()
		{
			return TimeSpan.FromHours(22);
		}

		public override void OnAccepted(IQuestGiver quester, MLQuestInstance instance)
		{
			base.OnAccepted(quester, instance);
			UnsentLetterDevLog.Write(instance?.Player as PlayerMobile, "quest_accept", "accepted");
		}

		public override void OnRewardClaimed(MLQuestInstance instance)
		{
			base.OnRewardClaimed(instance);
			UnsentLetterDevLog.Write(instance?.Player as PlayerMobile, "quest_reward", "reward claimed");
		}

		public override void OnCancel(MLQuestInstance instance)
		{
			base.OnCancel(instance);
			UnsentLetterDevLog.Write(instance?.Player as PlayerMobile, "quest_cancel", "cancelled");
		}

		public override void OnPlayerDeath(MLQuestInstance instance)
		{
			base.OnPlayerDeath(instance);
			UnsentLetterDevLog.Write(instance?.Player as PlayerMobile, "quest_player_death", "player died");
		}

		public override void Generate()
		{
			base.Generate();

			UnsentLetterQuestPackLoader.EnsureLoaded();
			List<UnsentLetterPackSpawnerEntry> list = UnsentLetterQuestPackLoader.Root?.spawners;

			if (list == null || list.Count == 0)
			{
				Console.WriteLine("UnsentLetterQuest: pack has no spawners; skipping world generation.");
				return;
			}

			foreach (UnsentLetterPackSpawnerEntry e in list)
			{
				if (String.IsNullOrWhiteSpace(e.typeName))
					continue;

				Map map = null;

				try
				{
					map = Map.Parse(e.map);
				}
				catch (Exception ex)
				{
					Console.WriteLine("UnsentLetterQuestPack: spawner map '{0}' invalid: {1}", e.map, ex.Message);
					continue;
				}

				if (map == null || map == Map.Internal)
					continue;

				Type st = ScriptCompiler.FindTypeByName(e.typeName.Trim());

				if (st == null)
				{
					Console.WriteLine("UnsentLetterQuestPack: spawner type '{0}' unknown; skipped.", e.typeName);
					continue;
				}

				int amt = e.amount > 0 ? e.amount : 1;
				int minD = e.minDelayMinutes > 0 ? e.minDelayMinutes : 5;
				int maxD = e.maxDelayMinutes >= minD ? e.maxDelayMinutes : Math.Max(minD, 10);
				Spawner s = new Spawner(amt, minD, maxD, e.team, e.homeRange, e.typeName.Trim());

				if (e.walkingRange >= 0)
					s.WalkingRange = e.walkingRange;

				PutSpawner(s, new Point3D(e.x, e.y, e.z), map);
			}
		}
	}
}
