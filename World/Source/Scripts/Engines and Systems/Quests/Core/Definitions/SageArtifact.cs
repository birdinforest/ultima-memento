using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.MLQuests.Gumps;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Localization;
using Server.Utilities;
using Server.Accounting;

namespace Server.Engines.MLQuests.Definitions
{
    #region Quests

    public class ArtifactRumor : MLQuest
    {
        public override Type NextQuest { get { return typeof(ArtifactRumorTownsfolk); } }
        public override bool MustQuitQuestChain { get { return true; } }

        public ArtifactRumor()
        {
            Activated = true;
            Title = "An arduous journey";
            Description = "Greetings, Adventurer! I've heard some information an artefact of great power. Would you like to hear it?";
            RefusalMessage = "The Sage shrugs. Don't say I didn't give you the chance.";
            InProgressMessage = "The Sage casually rub their thumb and fingers together, a small, almost teasing gesture, as if to silently signal that something is owed. Their eyes narrow slightly and a knowing smile tugs at the corner of their mouth.";
            CompletionNotice = CompletionNoticeShortReturn;

            Objectives.Add(new DummyObjective("Bribe the Sage"));
            Objectives.Add(new CollectObjective(10000, typeof(Gold), "Gold coins"));

            Rewards.Add(new DummyReward("Information about a powerful artefact"));
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(Sage);
        }

        public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
        {
            if (!base.CanOffer(quester, pm, context, message)) return false;

            if (GetArtifactRumorObjectiveInstance.HasRewardItem(pm))
            {
                MLQuestSystem.Tell(quester, pm, "It looks like you're already searching for an artefact. Come back after you're done.");
                return false;
            }

            return true;
        }
    }

    public class ArtifactRumorTownsfolk : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }
        public override bool MustQuitQuestChain { get { return true; } }

        public ArtifactRumorTownsfolk()
        {
            Activated = true;
            HasRestartDelay = true; // Set after total quest completion
            Title = "An arduous journey (part 2)";
            Description = "The Sage paused for a moment, their expression shifting as if something faint had reached their ears, leaving them unsure whether it was real or just their imagination playing tricks. Y'know, you might want to speak to the townsfolk to confirm my thoughts...";
            RefusalMessage = "Very well, I understand your reluctance. Come back to me when you are ready.";
            InProgressMessage = "There is nothing I can do for you until you speak to the townsfolk and fully refine the rumor.";
            CompletionMessage = "So it is real?! I thought I might be imagining it, but now I'm sure I wasn't just making it up. Good luck on your journey!";
            CompletionNotice = CompletionNoticeShortReturn;

            Objectives.Add(new GetArtifactRumorObjective()); // Awards a Search Page

            Rewards.Add(new DummyReward("The location of a powerful artefact"));
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(Sage);
            yield return typeof(Citizens);
        }

        public override TimeSpan GetRestartDelay()
        {
            return TimeSpan.FromDays(3);
        }

        public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
        {
            if (!base.CanOffer(quester, pm, context, message)) return false;
            if ((quester is Sage) == false) return false;

            if (GetArtifactRumorObjectiveInstance.HasRewardItem(pm))
            {
                MLQuestSystem.Tell(quester, pm, "It looks like you're already searching for an artefact. Come back after you're done.");
                return false;
            }

            return true;
        }
    }

    #endregion

    #region Objectives
    public class GetArtifactRumorObjective : BaseObjective
    {
        public virtual bool ShowDetailed { get { return true; } }

        public GetArtifactRumorObjective()
        {
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, BaseQuestGump.ResolveQuestCatalogString(g, "Speak to townsfolk until you verify the Sage's rumor"));
        }

        public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
        {
            return new GetArtifactRumorObjectiveInstance(this, instance);
        }
    }

    public class GetArtifactRumorObjectiveInstance : BaseObjectiveInstance, IDeserializable
    {
        private const int CITIZEN_PITY_AMOUNT = 50;

        private enum RumorType
        {
            Land = 0,
            Dungeon = 1,
            Item = 2
        }

        public Land Land { get; set; }
        public string Dungeon { get; set; }
        public int RelicNumber { get; set; }
        public int RumorAttempts { get; set; }

        protected bool HasLand { get { return Land != Land.None; } }
        protected bool HasDungeon { get { return !string.IsNullOrWhiteSpace(Dungeon); } }
        protected bool HasRelicNumber { get { return 0 < RelicNumber; } }

        public GetArtifactRumorObjective Objective { get; protected set; }

        public GetArtifactRumorObjectiveInstance(GetArtifactRumorObjective objective, MLQuestInstance instance)
            : base(instance, objective)
        {
            Objective = objective;
        }

        public override bool IsCompleted()
        {
            return HasLand && HasDungeon && HasRelicNumber;
        }

        public override bool OnBeforeClaimReward()
        {
            if (HasRewardItem(Instance.Player)) return false;

            var searchBase = GetSearchBase();
            return searchBase != null;
        }

        public override void OnAfterClaimReward()
        {
            Container pack = Instance.Player.Backpack;
            if (pack == null) return;

            var searchBase = GetSearchBase();
            if (searchBase == null) return;

            var questItem = new SearchPage(Instance.Player, searchBase, RelicNumber);
            Instance.Player.AddToBackpack(questItem);

            OnQuestCancelled(); // same thing, clear other quest items
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            Objective.WriteToGump(g, ref y);

            if (Objective.ShowDetailed)
            {
                base.WriteToGump(g, ref y);

                y += 16;
                g.AddLabel(103, y, BaseQuestGump.COLOR_LABEL, HasLand ? GetOrAddHint(RumorType.Land) : BaseQuestGump.ResolveQuestCatalogString(g, "You must narrow down the location."));

                if (HasLand)
                {
                    y += 16;
                    g.AddLabel(103, y, BaseQuestGump.COLOR_LABEL, HasDungeon ? GetOrAddHint(RumorType.Dungeon) : BaseQuestGump.ResolveQuestCatalogString(g, "You must narrow down the location."));
                }

                y += 16;
                g.AddLabel(103, y, BaseQuestGump.COLOR_LABEL, HasRelicNumber ? GetOrAddHint(RumorType.Item) : BaseQuestGump.ResolveQuestCatalogString(g, "You wonder which artefact everyone is talking about."));

                if (IsCompleted())
                {
                    y += 16;
                    y += 16;
                    string rumorsLine = Instance.Player != null && Instance.Player.Account != null
                        ? StringCatalog.ResolveFormat(Instance.Player.Account, "You heard {0} rumors, no wonder you're exhausted!", RumorAttempts)
                        : string.Format("You heard {0} rumors, no wonder you're exhausted!", RumorAttempts);
                    g.AddLabel(98, y, BaseQuestGump.COLOR_LABEL, rumorsLine);
                    
                    y += 16;
                    string dest = QuesterNameAttribute.GetQuesterNameFor(Instance.QuesterType);
                    string returnLine = Instance.Player != null && Instance.Player.Account != null
                        ? StringCatalog.ResolveFormat(Instance.Player.Account, "Return to {0}.", dest)
                        : string.Format("Return to {0}.", dest);
                    g.AddLabel(103, y, BaseQuestGump.COLOR_LABEL, returnLine);
                }
            }
        }

        public static bool HasRewardItem(PlayerMobile playerMobile)
        {
            return World.Items.Values.Any(item => item is SearchPage && ((SearchPage)item).Owner == playerMobile);
        }

        public virtual bool TryGetRumor(IQuestGiver quester)
        {
            var citizen = quester as Citizens;
            if (citizen == null) return false;
            if (!citizen.CanTellRumor()) return false;
            if (IsCompleted()) return true;

            citizen.MarkToldRumor(); // Always flag the Citizen as talked to
            if (++RumorAttempts < CITIZEN_PITY_AMOUNT && 1 < Utility.RandomMinMax(1, 10)) return false; // Small chance the Citizen can help

            var hintType = !HasLand ? RumorType.Land
                : !HasDungeon ? RumorType.Dungeon
                : RumorType.Item;
            var hint = GetOrAddHint(hintType, true);

            MLQuestSystem.Tell(quester, Instance.Player, hint);

            return false;
        }

        private string FormatHint(string shotKey, string englishArg, bool annotatePlace)
        {
            PlayerMobile pm = Instance.Player;

            if (pm == null || pm.Account == null)
                return StringCatalog.ResolveFormatByKey(null, shotKey, englishArg);

            string arg0 = annotatePlace
                ? QuestCompositeResolver.FormatAnnotatedPlaceForContract(pm, englishArg)
                : englishArg;

            return StringCatalog.ResolveFormatByKey(pm.Account, shotKey, arg0);
        }

        private string GetOrAddHint(RumorType rumorType, bool forCitizen = false)
        {
            switch (rumorType)
            {
                case RumorType.Land:
                    {
                        if (!HasLand)
                        {
                            var options = new List<Land>
                            {
                                Land.Sosaria,
                                Land.Sosaria,
                                Land.Sosaria,
                                Land.Lodoria,
                                Land.Lodoria,
                                Land.Lodoria,
                                Land.Serpent,
                                Land.Serpent,
                                Land.Serpent,
                                Land.IslesDread,
                                Land.Savaged,
                                Land.Savaged,
                                Land.UmberVeil,
                                Land.Kuldar,
                                Land.Underworld,
                                Land.Ambrosia,
                            };
                            Land = Utility.Random(options); // Intentionally anywhere
                        }

                        var name = Lands.LandName(Land);

                        return forCitizen
                            ? FormatHint("quest.sageart.hint.land.citizen", name, true)
                            : FormatHint("quest.sageart.hint.land.progress", name, true);
                    }

                case RumorType.Dungeon:
                    {
                        if (!HasDungeon)
                        {
                            List<SearchBase> candidates = GetCandidates(Instance.Player, Land);
                            SearchBase target = candidates[Utility.RandomMinMax(0, candidates.Count - 1)];
                            Dungeon = Worlds.GetRegionName(target.Map, target.Location);
                        }

                        return forCitizen
                            ? FormatHint("quest.sageart.hint.dungeon.citizen", Dungeon, true)
                            : FormatHint("quest.sageart.hint.dungeon.progress", Dungeon, true);
                    }

                case RumorType.Item:
                    {
                        if (!HasRelicNumber)
                        {
                            RelicNumber = Utility.RandomMinMax(1, ArtifactQuestList.MaxNumber);
                        }

                        var itemName = ArtifactQuestList.GetArtifact(RelicNumber, 1);

                        return forCitizen
                            ? FormatHint("quest.sageart.hint.item.citizen", itemName, false)
                            : FormatHint("quest.sageart.hint.item.progress", itemName, false);
                    }

                default:
                    Console.WriteLine("Failed to generate hint for Rumor '{0}'", rumorType);
                    return string.Format("Error: Failed to generate hint for Rumor '{0}'", rumorType);
            }
        }

        private static List<SearchBase> GetCandidates(PlayerMobile playerMobile, Land land, Func<SearchBase, bool> predicate = null)
        {
            return SearchPage.GetCandidates(playerMobile, false, item =>
                item is SearchBase && Lands.GetLand(item.Map, item.Location, item.X, item.Y) == land
                && (predicate == null || predicate(item))
            ).ToList();
        }

        private SearchBase GetSearchBase()
        {
            var destination = GetCandidates(Instance.Player, Land, item => Worlds.GetRegionName(item.Map, item.Location) == Dungeon).FirstOrDefault();
            if (destination != null) return destination;

            return null;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(2); // Version
            writer.Write((int)Land);
            writer.Write(Dungeon);
            writer.Write(RelicNumber);
            writer.Write(RumorAttempts);
        }

        public void Deserialize(GenericReader reader)
        {
            // Base deserialize was already handled

            int version = reader.ReadInt();
            Land = (Land)reader.ReadInt();
            Dungeon = reader.ReadString();
            RelicNumber = reader.ReadInt();
            if (1 < version)
                RumorAttempts = reader.ReadInt();
        }
    }
    #endregion
}

#region Items

namespace Server.Items
{
    public class SearchPage : Item
    {
        private Mobile m_Owner;
        private string m_SearchDungeon;
        private string m_SearchItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public Map GetMap { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GetX { get; private set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GetY { get; private set; }

        [CommandProperty(AccessLevel.Owner)]
        public int LegendPercent { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get { return m_Owner; } set { m_Owner = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.Owner)]
        public string SearchMessage { get; set; }

        [CommandProperty(AccessLevel.Owner)]
        public string SearchDungeon { get { return m_SearchDungeon; } set { m_SearchDungeon = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.Owner)]
        public Map DungeonMap { get; set; }

        [CommandProperty(AccessLevel.Owner)]
        public string SearchWorld { get; set; }

        [CommandProperty(AccessLevel.Owner)]
        public string SearchType { get; set; }

        [CommandProperty(AccessLevel.Owner)]
        public string SearchItem { get { return m_SearchItem; } set { m_SearchItem = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.Owner)]
        public int LegendReal { get; set; }

        public SearchPage(Mobile from, SearchBase searchBase, int relicNumber) : base(0x2159)
        {
            LegendPercent = 70;
            m_Owner = from;
            Weight = 1.0;
            Hue = 0x995;
            Name = from.Account != null
                ? StringCatalog.ResolveFormatByKey(from.Account, "quest.sageart.legend.name", from.Name)
                : "highly reliable legend for " + from.Name;

            // CHECK TO SEE IF THE NOTE IS FALSE OR TRUE
            if (LegendPercent >= Utility.RandomMinMax(1, 100)) { LegendReal = 1; }

            SearchItem = ArtifactQuestList.GetArtifact(relicNumber, 1);
            SearchType = ArtifactQuestList.GetArtifact(relicNumber, 2);

            UseSearchLocation(this, searchBase);
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add(1070722, SearchItem);
            list.Add(1049644, m_SearchDungeon);
            if (BuildingPropertyListLocale != null)
                AddLocalizedProperty(list, "prop.quest.sageart.discard");
            else
                list.Add(1049644, "Discard at any time to abandon quest");
        }

        public class SearchGump : Gump
        {
            private SearchPage m_Book;
            private Map m_Map;
            private int m_X;
            private int m_Y;

            public SearchGump(Mobile from, Item parchment) : base(100, 100)
            {
                SearchPage scroll = (SearchPage)parchment;
                string sText = scroll.SearchMessage;
                from.PlaySound(0x249);

                m_Book = scroll;
                m_Map = scroll.GetMap;
                m_X = scroll.GetX;
                m_Y = scroll.GetY;

                this.Closable = true;
                this.Disposable = true;
                this.Dragable = true;
                this.Resizable = false;

                AddPage(0);

                AddImage(0, 0, 10901, 2786);
                AddImage(0, 0, 10899, 2117);
                AddHtml(45, 78, 386, 218, @"<BODY><BASEFONT Color=#d9c781>" + sText + "</BASEFONT></BODY>", (bool)false, (bool)true);

                if (Sextants.HasSextant(from))
                    AddButton(377, 325, 10461, 10461, 1, GumpButtonType.Reply, 0);
            }

            public override void OnResponse(NetState state, RelayInfo info)
            {
                Mobile from = state.Mobile;

                if (info.ButtonID > 0)
                {
                    from.CloseGump(typeof(Sextants.MapGump));
                    from.SendGump(new SearchGump(from, m_Book));
                    from.SendGump(new Sextants.MapGump(from, m_Map, m_X, m_Y, null));
                }
                else
                {
                    from.PlaySound(0x249);
                    from.CloseGump(typeof(Sextants.MapGump));
                }
            }
        }

        public override void OnDoubleClick(Mobile e)
        {
            if (!IsChildOf(e.Backpack))
            {
                e.SendMessage(StringCatalog.ResolveByKey(e.Account, "quest.courier.mail.backpack"));
            }
            else
            {
                e.CloseGump(typeof(SearchGump));
                e.SendGump(new SearchGump(e, this));
            }
        }

        public static List<SearchBase> GetCandidates(PlayerMobile from, bool restrictDifficulty, Func<SearchBase, bool> predicate = null)
        {
            return World.Items.Values
                .Where(item => item is SearchBase)
                .Cast<SearchBase>()
                .Where(item => predicate == null || predicate(item))
                .Where(target => !restrictDifficulty || Server.Difficult.GetDifficulty(target.Location, target.Map) <= GetPlayerInfo.GetPlayerDifficulty(from))
                .ToList();
        }

        public static void UseRandomSearchLocation(SearchPage scroll, string DungeonNow, PlayerMobile from)
        {
            // Default
            string thisWorld = "the Land of Sosaria";
            string thisPlace = "Dungeon Doom";
            Map realMap = Map.Sosaria;
            Map thisMap = Map.Sosaria;

            List<SearchBase> candidates = GetCandidates(from, true);
            if (0 < candidates.Count)
            {
                SearchBase finding = candidates[Utility.RandomMinMax(0, candidates.Count - 1)];
                thisMap = Server.Misc.Worlds.GetMyDefaultMap(finding.Land);
                realMap = finding.Map;
                thisPlace = Server.Misc.Worlds.GetRegionName(finding.Map, finding.Location);
                thisWorld = Lands.LandName(finding.Land);
            }

            SetSearchLocation(scroll, thisPlace, thisWorld, thisMap, realMap);
        }

        private static void UseSearchLocation(SearchPage scroll, SearchBase item)
        {
            string itemLandName = Lands.LandName(item.Land);
            string thisPlace = Worlds.GetRegionName(item.Map, item.Location);
            Map itemMap = item.Map;
            Map baseMap = Worlds.GetMyDefaultMap(item.Land);

            SetSearchLocation(scroll, thisPlace, itemLandName, baseMap, itemMap);
        }

        private static string LocalizePlace(Mobile owner, string english)
        {
            if (owner == null || string.IsNullOrEmpty(english))
                return english;

            return QuestCompositeResolver.FormatAnnotatedPlaceForContract(owner, english);
        }

        private static string ResolveLegendWord(IAccount account, int roll, int wordSet)
        {
            string key;

            switch (wordSet)
            {
                default:
                case 1:
                    switch (roll)
                    {
                        default:
                        case 1: key = "quest.sageart.legend.word1.rumors"; break;
                        case 2: key = "quest.sageart.legend.word1.myths"; break;
                        case 3: key = "quest.sageart.legend.word1.tales"; break;
                        case 4: key = "quest.sageart.legend.word1.stories"; break;
                    }
                    break;
                case 2:
                    switch (roll)
                    {
                        case 1: key = "quest.sageart.legend.word2.kept"; break;
                        case 2: key = "quest.sageart.legend.word2.seen"; break;
                        case 3: key = "quest.sageart.legend.word2.taken"; break;
                        case 4: key = "quest.sageart.legend.word2.hidden"; break;
                        default: key = "quest.sageart.legend.word2.lost"; break;
                    }
                    break;
                case 3:
                    switch (roll)
                    {
                        case 1: key = "quest.sageart.legend.word3.within"; break;
                        case 2: key = "quest.sageart.legend.word3.somewhere_in"; break;
                        case 3: key = "quest.sageart.legend.word3.somehow_in"; break;
                        case 4: key = "quest.sageart.legend.word3.far_in"; break;
                        default: key = "quest.sageart.legend.word3.deep_in"; break;
                    }
                    break;
                case 4:
                    switch (roll)
                    {
                        case 1: key = "quest.sageart.legend.word4.thousands_of_years_ago"; break;
                        case 2: key = "quest.sageart.legend.word4.decades_ago"; break;
                        case 3: key = "quest.sageart.legend.word4.millions_of_years_ago"; break;
                        case 4: key = "quest.sageart.legend.word4.many_years_ago"; break;
                        default: key = "quest.sageart.legend.word4.centuries_ago"; break;
                    }
                    break;
            }

            return StringCatalog.ResolveByKey(account, key);
        }

        private static void SetSearchLocation(SearchPage scroll, string thisPlace, string thisWorld, Map thisMap, Map realMap)
        {
            IAccount account = scroll.Owner != null ? scroll.Owner.Account : null;
            int roll1 = Utility.RandomMinMax(1, 4);
            int roll2 = Utility.RandomMinMax(1, 4);
            int roll3 = Utility.RandomMinMax(1, 4);
            int roll4 = Utility.RandomMinMax(1, 4);
            string word1 = ResolveLegendWord(account, roll1, 1);
            string word2 = ResolveLegendWord(account, roll2, 2);
            string word3 = ResolveLegendWord(account, roll3, 3);
            string word4 = ResolveLegendWord(account, roll4, 4);

            scroll.m_SearchDungeon = thisPlace;
            scroll.SearchWorld = thisWorld;
            scroll.DungeonMap = thisMap;

            Map placer;
            int xc;
            int yc;
            string EntranceLocation = Worlds.GetAreaEntrance(0, scroll.m_SearchDungeon, realMap, out placer, out xc, out yc);

            scroll.GetMap = placer;
            scroll.GetX = xc;
            scroll.GetY = yc;

            string OldMessage = "<br><br><br><br><br><br>" + scroll.SearchMessage;

            string dungeonDisplay = LocalizePlace(scroll.Owner, scroll.m_SearchDungeon);
            string worldDisplay = LocalizePlace(scroll.Owner, scroll.SearchWorld);
            string giver = QuestCharacters.QuestGiver();

            scroll.SearchMessage = account != null
                ? StringCatalog.ResolveFormatByKey(account, "quest.sageart.legend.body", scroll.SearchItem, word1, scroll.SearchItem, word2, word3, dungeonDisplay, word4, giver, worldDisplay, EntranceLocation, OldMessage)
                : scroll.SearchItem + "<br><br>" + word1 + " tell of the " + scroll.SearchItem + " being " + word2 + " " + word3 + " " + scroll.m_SearchDungeon + " " + word4 + " by " + giver + ". in " + scroll.SearchWorld + " at the below sextant coordinates.<br><br>" + EntranceLocation + OldMessage;

            scroll.InvalidateProperties();
        }
        public SearchPage(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)2);

            writer.Write(GetMap);
            writer.Write(GetX);
            writer.Write(GetY);

            writer.Write((Mobile)m_Owner);
            writer.Write(SearchMessage);
            writer.Write(m_SearchDungeon);
            writer.Write(DungeonMap);
            writer.Write(SearchWorld);
            writer.Write(SearchType);
            writer.Write(SearchItem);
            writer.Write(LegendReal);
            writer.Write(LegendPercent);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        GetMap = reader.ReadMap();
                        GetX = reader.ReadInt();
                        GetY = reader.ReadInt();
                        break;
                    }
            }

            m_Owner = reader.ReadMobile();
            SearchMessage = reader.ReadString();
            m_SearchDungeon = reader.ReadString();
            DungeonMap = reader.ReadMap();
            SearchWorld = reader.ReadString();
            SearchType = reader.ReadString();
            SearchItem = reader.ReadString();
            LegendReal = reader.ReadInt();
            LegendPercent = reader.ReadInt();
            ItemID = 0x2159;
            Hue = 0x995;
        }
    }

    [Flipable(0x577B, 0x577C)]
    public class SearchBoard : Item
    {
        private static string ResolveBoardText(Mobile from, string key)
        {
            return StringCatalog.ResolveByKey(from != null ? from.Account : null, key);
        }

        [Constructable]
        public SearchBoard() : base(0x577B)
        {
            Weight = 1.0;
            Name = "Sage Advice";
            Hue = 0x986;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
            if (BuildingPropertyListLocale != null)
                AddLocalizedProperty(list, "prop.quest.sageart.board.title");
            else
                list.Add("The Search for Artifacts");
        }

        public override void OnDoubleClick(Mobile e)
        {
            if (e.InRange(this.GetWorldLocation(), 4))
            {
                e.CloseGump(typeof(BoardGump));
                e.SendGump(new BoardGump(e, ResolveBoardText(e, "quest.sageart.board.gump.title"), ResolveBoardText(e, "quest.sageart.board.gump.body"), "#d3d307", true));
            }
            else
            {
                e.SendLocalizedMessage(502138); // That is too far away for you to use
            }
        }

        public SearchBoard(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}

#endregion