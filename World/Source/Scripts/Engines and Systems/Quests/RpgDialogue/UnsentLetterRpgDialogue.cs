using System;
using System.Collections.Generic;
using System.Text;
using Server;
using Server.Accounting;
using Server.Engines.MLQuests;
using Server.Engines.MLQuests.Gumps;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Engines.RpgDialogue;
using Server.Localization;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
	/// <summary>
	/// RPG dialogue for The Unsent Letter. Opened from the NPC context menu Talk entry only.
	/// </summary>
	public static class UnsentLetterRpgDialogue
	{
		private static string ResolveCatalogBody(PlayerMobile pm, string english)
		{
			if (english == null || english.Length == 0)
				return "";

			if (pm?.Account == null)
				return english;

			string lang = AccountLang.GetLanguageCode(pm.Account);
			string r = StringCatalog.TryResolveLogicalOrHash(lang, english) ?? english;

			if (AccountLang.IsChinese(lang))
				r = QuestCompositeResolver.ResolveComposite(pm, r);

			return r;
		}

		public static void SendQuestOffer(UnsentLetterQuest quest, IQuestGiver quester, PlayerMobile pm)
		{
			Mobile m = quester as Mobile;

			if (quest == null || m == null || m.Deleted || pm == null || pm.Deleted)
				return;

			string desc = ResolveTextDefString(pm, quest.Description);
			string pitch = "quest-unsent-letter-offer-pitch-001";
			string offerFooter = "quest-unsent-letter-offer-footer-001";
			string body = ResolveCatalogBody(pm, pitch) + "<BR><BR>" + desc + ResolveCatalogBody(pm, offerFooter);

			pm.SendGump(new DynamicRpgDialogueGump(m, pm, body,
				new[]
				{
					new DynamicRpgDialogueOption("quest-unsent-letter-ui-accept-offer-001",
						(p, mob) => quest.OnAccept(quester, p)),
					new DynamicRpgDialogueOption("quest-unsent-letter-ui-decline-offer-001",
						(p, mob) => quest.OnRefuse(quester, p))
				}));
		}

		public static void SendQuestRewardOffer(MLQuestInstance instance)
		{
			if (instance == null || instance.Player == null || instance.Player.Deleted)
				return;

			UnsentLetterQuest quest = instance.Quest as UnsentLetterQuest;

			if (quest == null)
			{
				instance.SendRewardGump();
				return;
			}

			PlayerMobile pm = instance.Player;
			BaseQuestGump.CloseOtherGumps(pm);

			Mobile npc = instance.Quester as Mobile;

			if (npc == null || npc.Deleted)
			{
				pm.SendGump(new QuestRewardGump(instance));
				return;
			}

			StringBuilder sb = new StringBuilder();
			string completion = ResolveTextDefString(pm, quest.CompletionMessage);

			if (!string.IsNullOrEmpty(completion))
			{
				sb.Append(completion);
				sb.Append("<BR><BR>");
			}

			sb.Append(ResolveCatalogBody(pm, "quest-unsent-letter-reward-section-001"));

			foreach (BaseReward reward in quest.Rewards)
			{
				string line = ResolveTextDefString(pm, reward.Name);

				if (string.IsNullOrEmpty(line))
					continue;

				sb.Append("\u2022 ");
				sb.Append(line);
				sb.Append("<BR>");
			}

			string body = sb.ToString();
			MLQuestInstance instRef = instance;

			pm.SendGump(new DynamicRpgDialogueGump(npc, pm, body,
				new[]
				{
					new DynamicRpgDialogueOption("quest-unsent-letter-ui-accept-reward-001",
						(p, m) => instRef.ClaimRewards())
				}));
		}

		public static void SendQuestRefusal(UnsentLetterQuest quest, IQuestGiver quester, PlayerMobile pm)
		{
			Mobile m = quester as Mobile;

			if (quest == null || m == null || m.Deleted || pm == null || pm.Deleted)
				return;

			TextDefinition rm = quest.RefusalMessage;

			if (TextDefinition.IsNullOrEmpty(rm))
				return;

			if (rm.Number > 0)
			{
				TextDefinition.SendMessageTo(pm, rm);
				return;
			}

			string body = ResolveTextDefString(pm, rm);

			if (string.IsNullOrEmpty(body))
				return;

			pm.SendGump(new DynamicRpgDialogueGump(m, pm, body, Array.Empty<DynamicRpgDialogueOption>()));
		}

		private static string ResolveTextDefString(PlayerMobile pm, TextDefinition def)
		{
			if (def == null)
				return "";

			if (def.Number > 0)
				return string.Format("#{0}", def.Number);

			if (string.IsNullOrEmpty(def.String))
				return "";

			if (pm?.Account == null)
				return def.String;

			string lang = AccountLang.GetLanguageCode(pm.Account);
			string r = StringCatalog.TryResolveLogicalOrHash(lang, def.String) ?? def.String;

			if (AccountLang.IsChinese(lang))
				r = QuestCompositeResolver.ResolveComposite(pm, r);

			return r;
		}

		public static bool TryOpenMaraQuestOffer(PlayerMobile pm, UnsentLetterMara mara)
		{
			if (pm == null || mara == null || mara.Deleted || !pm.Alive)
				return false;

			if (UnsentLetterQuestHelper.FindAnyUnsentLetterInstance(pm) != null)
				return false;

			if (!CheckTalkRange(pm, mara, 5))
				return false;

			UnsentLetterQuest quest = MLQuestSystem.FindQuest(typeof(UnsentLetterQuest)) as UnsentLetterQuest;

			if (quest == null)
				return false;

			if (!quest.CanOffer(mara, pm, true))
				return false;

			SendQuestOffer(quest, mara, pm);
			return true;
		}

		private static bool CheckTalkRange(PlayerMobile pm, Mobile npc, int range)
		{
			if (pm == null || npc == null || npc.Deleted || !pm.Alive)
				return false;

			if (!pm.InRange(npc.Location, range) || !pm.CanSee(npc))
			{
				UnsentLetterI18n.SendMessageLocalized(pm, "quest-unsent-letter-msg-toofar-001");
				return false;
			}

			return true;
		}

		private static bool CoreEligible(PlayerMobile pm, Mobile npc, int range)
		{
			if (!CheckTalkRange(pm, npc, range))
				return false;

			if (UnsentLetterQuestHelper.FindInstance(pm) == null)
				return false;

			return true;
		}

		public static void TryOpenFromTalk(PlayerMobile pm, Mobile npc)
		{
			if (pm == null || npc == null || npc.Deleted || !pm.Alive)
				return;

			bool opened = false;

			if (npc is UnsentLetterMara mara)
			{
				MLQuestInstance any = UnsentLetterQuestHelper.FindAnyUnsentLetterInstance(pm);

				if (any != null && !any.Removed && any.ClaimReward && any.Quest is UnsentLetterQuest)
				{
					if (CheckTalkRange(pm, mara, 5))
					{
						SendQuestRewardOffer(any);
						opened = true;
					}
				}
				else if (any == null)
				{
					if (TryOpenMaraQuestOffer(pm, mara))
						opened = true;
					else if (CheckTalkRange(pm, mara, 5))
					{
						pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
							"quest-unsent-letter-rpg-fallback-mara-notready-001",
							Array.Empty<DynamicRpgDialogueOption>()));
						opened = true;
					}
				}
				else
					opened = TryOpenQuestDialogue(pm, npc);
			}
			else if (UnsentLetterQuestHelper.FindAnyUnsentLetterInstance(pm) != null)
			{
				MLQuestInstance pend = UnsentLetterQuestHelper.FindAnyUnsentLetterInstance(pm);
				if (pend != null && pend.ClaimReward && !(npc is UnsentLetterMara))
					opened = TryOpenClaimRewardWrongNpc(pm, npc);
				else
					opened = TryOpenQuestDialogue(pm, npc);
			}
			else
				opened = TryOpenNoQuestFallback(pm, npc);

			if (opened)
				MLQuestSystem.TurnToFace(npc, pm);
		}

		private static bool TryOpenClaimRewardWrongNpc(PlayerMobile pm, Mobile npc)
		{
			int range = 5;
			if (npc is UnsentLetterMiner)
				range = 8;
			else if (npc is UnsentLetterClerk)
				range = 6;

			if (!CheckTalkRange(pm, npc, range))
				return false;

			pm.SendGump(new DynamicRpgDialogueGump(npc, pm,
				"quest-unsent-letter-rpg-fallback-claim-turnin-mara-001",
				Array.Empty<DynamicRpgDialogueOption>()));
			return true;
		}

		private static bool TryOpenNoQuestFallback(PlayerMobile pm, Mobile npc)
		{
			string key;

			if (npc is UnsentLetterLina)
				key = "quest-unsent-letter-rpg-fallback-noquest-lina-001";
			else if (npc is UnsentLetterThomas)
				key = "quest-unsent-letter-rpg-fallback-noquest-thomas-001";
			else if (npc is UnsentLetterMiner)
				key = "quest-unsent-letter-rpg-fallback-noquest-miner-001";
			else if (npc is UnsentLetterClerk)
				key = "quest-unsent-letter-rpg-fallback-noquest-clerk-001";
			else
				key = "quest-unsent-letter-rpg-fallback-noquest-generic-001";

			if (!CheckTalkRange(pm, npc, npc is UnsentLetterMiner ? 8 : npc is UnsentLetterClerk ? 6 : 5))
				return false;

			pm.SendGump(new DynamicRpgDialogueGump(npc, pm, key, Array.Empty<DynamicRpgDialogueOption>()));
			return true;
		}

		private static bool TryOpenQuestDialogue(PlayerMobile pm, Mobile npc)
		{
			if (npc is UnsentLetterMara mara)
				return TryOpenMara(pm, mara);
			if (npc is UnsentLetterLina lina)
				return TryOpenLina(pm, lina);
			if (npc is UnsentLetterThomas thomas)
				return TryOpenThomas(pm, thomas);
			if (npc is UnsentLetterMiner miner)
				return TryOpenMiner(pm, miner);
			if (npc is UnsentLetterClerk clerk)
				return TryOpenClerk(pm, clerk);

			return false;
		}

		private static bool TryOpenMara(PlayerMobile pm, UnsentLetterMara mara)
		{
			if (!CoreEligible(pm, mara, 5))
				return false;

			UnsentLetterQuestPhase phase = UnsentLetterQuestHelper.GetCurrentPhase(pm);

			var famAny = UnsentLetterQuestHelper.FindObjectiveAny<UnsentSpeakFamilyObjectiveInstance>(pm);

			if (phase == UnsentLetterQuestPhase.SpeakFamily && famAny != null && !famAny.IsCompleted())
			{
				pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
					"quest-unsent-letter-rpg-mara-family-body-001",
					new[]
					{
						new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-family-opt-truth-001",
							(p, m) => UnsentLetterQuestHandlers.ApplyMaraFamilyTruth(p)),
						new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-family-opt-adrian-001",
							(p, m) => UnsentLetterQuestHandlers.ApplyMaraFamilyTruth(p))
					}));
				return true;
			}

			var puzzle = UnsentLetterQuestHelper.FindObjective<UnsentPuzzleObjectiveInstance>(pm);

			if (phase == UnsentLetterQuestPhase.Puzzle && puzzle != null)
			{
				if (!puzzle.Started)
				{
					UnsentLetterQuestHandlers.ApplyMaraPuzzleStart(pm);
					MaraShowPuzzleQuestion(pm, mara);
					return true;
				}

				if (puzzle.Started && puzzle.Step >= 1 && puzzle.Step <= 3)
				{
					MaraShowPuzzleQuestion(pm, mara);
					return true;
				}
			}

			var end = UnsentLetterQuestHelper.FindObjective<UnsentFamilyEndingObjectiveInstance>(pm);

			if (phase == UnsentLetterQuestPhase.Ending && end != null)
			{
				var opts = new List<DynamicRpgDialogueOption>();

				opts.Add(new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-ending-opt-letter-001",
					(p, m) => UnsentLetterQuestHandlers.ApplyMaraEndingLetterTalk(p)));

				if (end.FamilyTalks >= 2 && end.EndingChoice == 0)
				{
					opts.Add(new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-ending-opt-public-001",
						(p, m) => UnsentLetterQuestHandlers.ApplyMaraEndingChoice(p, 1)));
					opts.Add(new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-ending-opt-private-001",
						(p, m) => UnsentLetterQuestHandlers.ApplyMaraEndingChoice(p, 2)));
					opts.Add(new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-ending-opt-quiet-001",
						(p, m) => UnsentLetterQuestHandlers.ApplyMaraEndingChoice(p, 3)));
				}

				pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
					"quest-unsent-letter-rpg-mara-ending-body-001",
					opts.ToArray()));
				return true;
			}

			if (phase == UnsentLetterQuestPhase.Ambush)
			{
				pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
					"quest-unsent-letter-rpg-mara-hint-ambush-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return true;
			}

			if (phase == UnsentLetterQuestPhase.Evidence)
			{
				pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
					"quest-unsent-letter-rpg-mara-hint-evidence-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return true;
			}

			if (phase == UnsentLetterQuestPhase.Clerk)
			{
				pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
					"quest-unsent-letter-rpg-mara-hint-clerk-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return true;
			}

			pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
				"quest-unsent-letter-rpg-mara-hint-generic-001",
				Array.Empty<DynamicRpgDialogueOption>()));
			return true;
		}

		private static void MaraShowPuzzleQuestion(PlayerMobile pm, Mobile mara)
		{
			var puzzle = UnsentLetterQuestHelper.FindObjective<UnsentPuzzleObjectiveInstance>(pm);

			if (puzzle == null)
			{
				pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
					"quest-unsent-letter-rpg-mara-puzzle-null-body-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return;
			}

			switch (puzzle.Step)
			{
				case 1:
					pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
						"quest-unsent-letter-rpg-mara-puzzle-q1-body-001",
						new[]
						{
							new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-puzzle-q1-opt-a-001",
								(p, m) => MaraPuzzleAnswer(p, m, "montor road")),
							new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-puzzle-q1-opt-b-001",
								(p, m) => MaraPuzzleAnswer(p, m, "eastreach"))
						}));
					break;
				case 2:
					pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
						"quest-unsent-letter-rpg-mara-puzzle-q2-body-001",
						new[]
						{
							new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-puzzle-q2-opt-a-001",
								(p, m) => MaraPuzzleAnswer(p, m, "winter")),
							new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-puzzle-q2-opt-b-001",
								(p, m) => MaraPuzzleAnswer(p, m, "summer"))
						}));
					break;
				case 3:
					pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
						"quest-unsent-letter-rpg-mara-puzzle-q3-body-001",
						new[]
						{
							new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-puzzle-q3-opt-a-001",
								(p, m) => MaraPuzzleAnswer(p, m, "silence")),
							new DynamicRpgDialogueOption("quest-unsent-letter-rpg-mara-puzzle-q3-opt-b-001",
								(p, m) => MaraPuzzleAnswer(p, m, "pride"))
						}));
					break;
				default:
					pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
						"quest-unsent-letter-rpg-mara-puzzle-done-001",
						Array.Empty<DynamicRpgDialogueOption>()));
					break;
			}
		}

		private static void MaraPuzzleAnswer(PlayerMobile pm, Mobile mara, string answerSpeech)
		{
			UnsentLetterQuestHandlers.ApplyMaraPuzzleAnswer(pm, answerSpeech);
			var puzzle = UnsentLetterQuestHelper.FindObjective<UnsentPuzzleObjectiveInstance>(pm);

			if (puzzle == null)
			{
				pm.SendGump(new DynamicRpgDialogueGump(mara, pm,
					"quest-unsent-letter-rpg-mara-puzzle-finish-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return;
			}

			MaraShowPuzzleQuestion(pm, mara);
		}

		private static bool TryOpenLina(PlayerMobile pm, UnsentLetterLina lina)
		{
			if (!CoreEligible(pm, lina, 5))
				return false;

			UnsentLetterQuestPhase phase = UnsentLetterQuestHelper.GetCurrentPhase(pm);

			if (phase == UnsentLetterQuestPhase.SpeakFamily
				&& UnsentLetterQuestHelper.FindObjective<UnsentSpeakFamilyObjectiveInstance>(pm) != null)
			{
				pm.SendGump(new DynamicRpgDialogueGump(lina, pm,
					"quest-unsent-letter-rpg-lina-family-body-001",
					new[]
					{
						new DynamicRpgDialogueOption("quest-unsent-letter-rpg-lina-family-opt-001",
							(p, m) => UnsentLetterQuestHandlers.ApplyLinaFather(p))
					}));
				return true;
			}

			if (phase == UnsentLetterQuestPhase.Evidence)
			{
				pm.SendGump(new DynamicRpgDialogueGump(lina, pm,
					"quest-unsent-letter-rpg-lina-evidence-body-001",
					new[]
					{
						new DynamicRpgDialogueOption("quest-unsent-letter-rpg-lina-evidence-opt-001",
							(p, m) => UnsentLetterQuestHandlers.ApplyLinaEvidence(p))
					}));
				return true;
			}

			if (phase == UnsentLetterQuestPhase.Ending)
			{
				pm.SendGump(new DynamicRpgDialogueGump(lina, pm,
					"quest-unsent-letter-rpg-lina-ending-body-001",
					new[]
					{
						new DynamicRpgDialogueOption("quest-unsent-letter-rpg-lina-ending-opt-001",
							(p, m) => UnsentLetterQuestHandlers.ApplyLinaEndingLetterTalk(p))
					}));
				return true;
			}

			pm.SendGump(new DynamicRpgDialogueGump(lina, pm,
				"quest-unsent-letter-rpg-lina-hint-wrongphase-001",
				Array.Empty<DynamicRpgDialogueOption>()));
			return true;
		}

		private static bool TryOpenThomas(PlayerMobile pm, UnsentLetterThomas thomas)
		{
			if (!CoreEligible(pm, thomas, 5))
				return false;

			UnsentLetterQuestPhase phase = UnsentLetterQuestHelper.GetCurrentPhase(pm);

			if (phase == UnsentLetterQuestPhase.SpeakFamily
				&& UnsentLetterQuestHelper.FindObjective<UnsentSpeakFamilyObjectiveInstance>(pm) != null)
			{
				pm.SendGump(new DynamicRpgDialogueGump(thomas, pm,
					"quest-unsent-letter-rpg-thomas-family-body-001",
					new[]
					{
						new DynamicRpgDialogueOption("quest-unsent-letter-rpg-thomas-family-opt-001",
							(p, m) => UnsentLetterQuestHandlers.ApplyThomasFamily(p))
					}));
				return true;
			}

			if (phase == UnsentLetterQuestPhase.Ending)
			{
				pm.SendGump(new DynamicRpgDialogueGump(thomas, pm,
					"quest-unsent-letter-rpg-thomas-ending-body-001",
					new[]
					{
						new DynamicRpgDialogueOption("quest-unsent-letter-rpg-thomas-ending-opt-001",
							(p, m) => UnsentLetterQuestHandlers.ApplyThomasEndingLetterTalk(p))
					}));
				return true;
			}

			pm.SendGump(new DynamicRpgDialogueGump(thomas, pm,
				"quest-unsent-letter-rpg-thomas-hint-wrongphase-001",
				Array.Empty<DynamicRpgDialogueOption>()));
			return true;
		}

		private static bool TryOpenMiner(PlayerMobile pm, UnsentLetterMiner miner)
		{
			if (!CheckTalkRange(pm, miner, 8) || UnsentLetterQuestHelper.FindInstance(pm) == null)
				return false;

			UnsentLetterQuestPhase phase = UnsentLetterQuestHelper.GetCurrentPhase(pm);

			if (phase != UnsentLetterQuestPhase.Ambush)
			{
				pm.SendGump(new DynamicRpgDialogueGump(miner, pm,
					"quest-unsent-letter-rpg-miner-hint-wrongphase-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return true;
			}

			var amb = UnsentLetterQuestHelper.FindObjective<UnsentAmbushObjectiveInstance>(pm);

			if (amb == null)
				return false;

			if (amb.Phase == 0)
			{
				pm.SendGump(new DynamicRpgDialogueGump(miner, pm,
					"quest-unsent-letter-rpg-miner-body-001",
					new[]
					{
						new DynamicRpgDialogueOption("quest-unsent-letter-rpg-miner-opt-001",
							(p, m) => UnsentLetterQuestHandlers.BeginMinerEscort(p, miner))
					}));
				return true;
			}

			if (amb.Phase >= 1 && amb.Phase < 3)
			{
				pm.SendGump(new DynamicRpgDialogueGump(miner, pm,
					"quest-unsent-letter-rpg-miner-hint-combat-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return true;
			}

			pm.SendGump(new DynamicRpgDialogueGump(miner, pm,
				"quest-unsent-letter-rpg-miner-hint-done-001",
				Array.Empty<DynamicRpgDialogueOption>()));
			return true;
		}

		private static bool TryOpenClerk(PlayerMobile pm, UnsentLetterClerk clerk)
		{
			if (!CheckTalkRange(pm, clerk, 6) || UnsentLetterQuestHelper.FindInstance(pm) == null)
				return false;

			UnsentLetterQuestPhase phase = UnsentLetterQuestHelper.GetCurrentPhase(pm);

			if (phase != UnsentLetterQuestPhase.Clerk)
			{
				pm.SendGump(new DynamicRpgDialogueGump(clerk, pm,
					"quest-unsent-letter-rpg-clerk-hint-wrongphase-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return true;
			}

			var c = UnsentLetterQuestHelper.FindObjective<UnsentClerkObjectiveInstance>(pm);

			if (c == null || c.IsCompleted())
			{
				pm.SendGump(new DynamicRpgDialogueGump(clerk, pm,
					"quest-unsent-letter-rpg-clerk-hint-done-001",
					Array.Empty<DynamicRpgDialogueOption>()));
				return true;
			}

			pm.SendGump(new DynamicRpgDialogueGump(clerk, pm,
				"quest-unsent-letter-rpg-clerk-body-001",
				new[]
				{
					new DynamicRpgDialogueOption("quest-unsent-letter-rpg-clerk-opt-001",
						(p, m) => UnsentLetterQuestHandlers.BeginClerkFight(p, clerk))
				}));
			return true;
		}
	}
}
