using Server.Items;
using Server.Network;
using Server.Prompts;
using Server.Spells.Chivalry;
using Server.Spells.Elementalism;
using Server.Spells.Fourth;
using Server.Spells.Herbalist;
using Server.Spells.Magical;
using Server.Spells.Mystic;
using Server.Spells.Research;
using Server.Spells.Seventh;
using Server.Spells.Undead;
using System;

namespace Server.Gumps
{
	public class RunebookGump : Gump
	{
		private const int GUMP_INSET = 93;
		private const int GUMP_WIDTH = 1050;
		private const int HALF_BODY_WIDTH = PAGE_BODY_WIDTH / 2;
		private const int LEFT_PAGE_START = GUMP_INSET + PAGE_PADDING;
		private const int PAGE_BODY_WIDTH = PAGE_WIDTH - GUMP_INSET - (2 * PAGE_PADDING);
		private const int PAGE_PADDING = 20;
		private const int PAGE_WIDTH = GUMP_WIDTH / 2;
		private const int RIGHT_PAGE_START = GUMP_WIDTH - PAGE_WIDTH + PAGE_PADDING + PAGE_PADDING;
		private const int TOP_BODY_START = TOP_START + TOP_PADDING;
		private const int TOP_PADDING = 40;
		private const int TOP_START = 64;

		private readonly PageType m_PageType;
		private readonly RunebookEntry m_SelectedEntry;
		private readonly int m_SelectedEntryIndex;
		private Runebook m_Book;

		public RunebookGump(Mobile from, Runebook book, PageType page, int selectedEntryIndex) : base(50, 50)
		{
			m_Book = book;
			m_SelectedEntryIndex = selectedEntryIndex;
			m_PageType = page;
			m_SelectedEntry = book.TryGetRune(selectedEntryIndex);

			from.SendSound(0x55);

			AddBackground(book);
			AddNavigationButtons(page);

			var x = LEFT_PAGE_START;
			var y = TOP_BODY_START;

			if (page == PageType.Help)
			{
				AddInstructionsPage();
			}
			else
			{
				AddRuneIndexSection(ref x, ref y, selectedEntryIndex);

				if (m_SelectedEntry != null)
				{
					y = TOP_BODY_START;

					x = RIGHT_PAGE_START;
					AddSelectedRuneSection(from, ref x, ref y, m_SelectedEntry);
					y += 30;

					x = RIGHT_PAGE_START;
					AddSpellSection(from, ref x, ref y);
					y += 30;
				}
			}
		}

		public Runebook Book
		{ get { return m_Book; } }

		public static bool HasSpell(Mobile from, int spellID)
		{
			Spellbook book = Spellbook.Find(from, spellID);

			return (book != null && book.HasSpell(spellID));
		}

		public int GetEntryHue(Map map)
		{
			if (map == Map.Sosaria) return HtmlColors.GRAY;
			else if (map == Map.Lodor) return HtmlColors.PINK;
			else if (map == Map.Underworld) return HtmlColors.GRAY_BLUE;
			else if (map == Map.SerpentIsland) return HtmlColors.PALE_RED;
			else if (map == Map.IslesDread) return HtmlColors.ORANGE;
			else if (map == Map.SavagedEmpire) return HtmlColors.MINT_GREEN;

			return HtmlColors.LIGHT_GOLD;
		}

		public string GetName(string name)
		{
			if (name == null || (name = name.Trim()).Length <= 0)
				return "Marked Location";

			return name;
		}

		private void AddBackground(Runebook book)
		{
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			var color = book.Hue - 1;

			if (book.ItemID == 0x22C5 && book.Hue == 0) { color = 2847; }
			else if (book.ItemID == 0x0F3D && book.Hue == 0) { color = 2881; }
			else if (book.ItemID == 0x4F50 && book.Hue == 0) { color = 2847; }
			else if (book.ItemID == 0x4F51 && book.Hue == 0) { color = 2847; }
			else if (book.ItemID == 0x5463 && book.Hue == 0) { color = 2847; }
			else if (book.ItemID == 0x5464 && book.Hue == 0) { color = 2847; }

			AddPage(0);

			// Background image
			AddImage(0, 0, 7010, color);
			AddImage(0, 0, 7011);
			AddImage(0, 0, 7025, 2736);
		}

		private void AddInstructionsPage()
		{
			TextDefinition.AddHtmlText(this, 116, 110, 377, 487, "Rune Books are designed to help reduce the total number of carried runes and to assist in building rune libraries.<br><br>General Information:<br><br>- A rune book can hold a total of 16 locations.<br>  - Dragging a rune onto a book will add that location to the book.<br><br>- Rune books can have charges that will allow you to fast-travel to locations in the book.<br>  - If you are not using Charges, you are required to have reagents or potions on your person when attempting to cast the relevant spell.<br><br>- A rune book can have a 'default' location and a 'default' spell.<br>  - Press the -> button in the top right to go to the configuration page.<br><br>- Rune books can be named.<br>  - Press the -> button in the top right to go to the configuration page.<br>  - Click the sheet icon in the top left and enter a new name.<br>  <br>- Rune books can be dyed.<br><br>- The appearance of a rune book can be changed.<br>  - Drop this book on a local Scribe or Librarian.", false, true, HtmlColors.LIGHT_GOLD, HtmlColors.LIGHT_GOLD);
			TextDefinition.AddHtmlText(this, 555, 113, 377, 487, "Configuring your rune book:<br>- All runebook configuration can be managed on the Second Page of your runebook.<br>  - To get to the configuration page, click the -> button in the top right of the First Page.<br>- The column of buttons on the left allow for selecting a rune.<br>  - All operations on the right page will execute against the selected rune.<br>- You may rename runes.<br>- If you have a Sextant, you may see where in the world the rune will take you.<br>- You may set a default rune.<br>  - A default rune is the location that is automatically targeted when you use a fast-travel mechanism and target your rune book.<br>- You may reorganize (up/down/remove) runes.<br>- You may set a default Casting Option.<br>  - This fast-travel mechanism will be used when clicking the -> beside the runes on the First Page.<br><br>Recharging your rune book:<br>- Rune books may be charged so that you can use them without actually needing to use a spell or potion to fast-travel.<br>  - You can charge a rune book by dragging and dropping recall, gate, or astral travel spell scrolls on it.<br>  - You can charge a rune book by dragging and dropping nature fury, mushroom gateway, demonic fire, and black gate potions on it.<br>  - Dragging such items onto the book will add one charge (up to its maximum).", false, true, HtmlColors.LIGHT_GOLD, HtmlColors.LIGHT_GOLD);
		}

		private void AddNavigationButtons(PageType selectedPage)
		{
			const int RIGHT_ARROW = 4005;
			const int LEFT_ARROW = 4014;
			const int baseButtonId = (int)ActionButtonType.ChangePage;

			switch (selectedPage)
			{
				case PageType.Index:
					{
						AddButton(LEFT_PAGE_START, TOP_START, LEFT_ARROW, LEFT_ARROW, baseButtonId + (int)PageType.Help, GumpButtonType.Reply, 0);
						AddTooltip("Go To: Help");

						AddButton(RIGHT_PAGE_START + PAGE_BODY_WIDTH - 30 - 23, TOP_START, RIGHT_ARROW, RIGHT_ARROW, baseButtonId + (int)PageType.Configuration, GumpButtonType.Reply, 0);
						AddTooltip("Go To: Configuration");
						break;
					}

				case PageType.Configuration:
					{
						AddButton(LEFT_PAGE_START, TOP_START, LEFT_ARROW, LEFT_ARROW, baseButtonId + (int)PageType.Index, GumpButtonType.Reply, 0);
						AddTooltip("Go To: Index");

						AddButton(RIGHT_PAGE_START + PAGE_BODY_WIDTH - 30 - 23, TOP_START, RIGHT_ARROW, RIGHT_ARROW, baseButtonId + (int)PageType.Help, GumpButtonType.Reply, 0);
						AddTooltip("Go To: Help");
						break;
					}

				case PageType.Help:
					{
						AddButton(LEFT_PAGE_START, TOP_START, LEFT_ARROW, LEFT_ARROW, baseButtonId + (int)PageType.Configuration, GumpButtonType.Reply, 0);
						AddTooltip("Go To: Configuration");

						AddButton(RIGHT_PAGE_START + PAGE_BODY_WIDTH - 30 - 23, TOP_START, RIGHT_ARROW, RIGHT_ARROW, baseButtonId + (int)PageType.Index, GumpButtonType.Reply, 0);
						AddTooltip("Go To: Index");
						break;
					}

				default:
					return;
			}
		}

		private void AddRuneIndexSection(ref int x, ref int y, int selectedEntryIndex)
		{
			const int EMPTY_BOX = 3609;
			const int CHECKED_BOX = 4017;
			const int RIGHT_ARROW = 4005;
			const int PAGE_ICON = 4011;

			var isConfiguring = m_PageType == PageType.Configuration;

			var description = !string.IsNullOrWhiteSpace(m_Book.Description) ? m_Book.Description : "My Runebook";
			if (isConfiguring)
			{
				// Rename Book
				AddButton(x, y, PAGE_ICON, PAGE_ICON, (int)ActionButtonType.RenameBook, GumpButtonType.Reply, 0);
				AddTooltip("Rename Runebook");
				TextDefinition.AddHtmlText(this, x + 40, y + 3, HALF_BODY_WIDTH, 20, description, HtmlColors.WHITE);
				y += 30;
			}
			else
			{
				TextDefinition.AddHtmlText(this, x, y, HALF_BODY_WIDTH, 20, description, HtmlColors.KHAKI);
				y += 30;
			}

			for (var i = 0; i < Runebook.MAX_RECALL_RUNES; ++i)
			{
				if (m_Book.Entries.Count <= i)
				{
					AddImage(x, y, EMPTY_BOX);
				}
				else
				{
					var entry = m_Book.TryGetRune(i);
					if (entry == null) continue;

					var hue = m_Book.Default == entry ? HtmlColors.KHAKI : GetEntryHue(entry.Map);

					var graphic = i == selectedEntryIndex ? CHECKED_BOX : EMPTY_BOX;
					AddButton(x, y, graphic, graphic, (int)ActionButtonType.SelectRuneBase + i, GumpButtonType.Reply, 0);

					var newX = x + 40;
					if (!isConfiguring)
					{
						AddButton(newX, y, RIGHT_ARROW, RIGHT_ARROW, (int)ActionButtonType.ExecuteDefaultSpellAgainstTargetBase + i, GumpButtonType.Reply, 0);
						newX += 40;
					}

					TextDefinition.AddHtmlText(this, newX, y + 3, PAGE_BODY_WIDTH - 30, 20, GetName(entry.Description), hue);
				}

				y += 30;
			}
		}

		private void AddSelectedRuneSection(Mobile from, ref int x, ref int y, RunebookEntry entry)
		{
			const int RIGHT_ARROW = 4005;
			const int PAGE_ICON = 4011;

			var isConfiguring = m_PageType == PageType.Configuration;
			var hue = GetEntryHue(entry.Map);

			TextDefinition.AddHtmlText(this, x, y, HALF_BODY_WIDTH, 20, "Rune Information", HtmlColors.KHAKI);
			y += 30;

			if (isConfiguring)
			{
				// Rename Rune
				AddButton(x, y, PAGE_ICON, PAGE_ICON, (int)ActionButtonType.RenameRune, GumpButtonType.Reply, 0);
				AddTooltip("Rename Rune");
				TextDefinition.AddHtmlText(this, x + 40, y + 3, HALF_BODY_WIDTH, 20, GetName(entry.Description), hue);
				y += 30;
			}
			else
			{
				TextDefinition.AddHtmlText(this, x, y, HALF_BODY_WIDTH, 20, GetName(entry.Description), hue);
				y += 30;
			}

			int xLong = 0, yLat = 0;
			int xMins = 0, yMins = 0;
			bool xEast = false, ySouth = false;
			if (Sextant.Format(entry.Location, entry.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
			{
				var newX = x;
				if (Sextants.HasSextant(from))
				{
					AddButton(x, y, 10461, 10461, (int)ActionButtonType.PreviewRuneLocation, GumpButtonType.Reply, 0);
					newX += 40;
				}

				var world = Server.Lands.LandName(Server.Lands.GetLand(entry.Map, entry.Location, entry.Location.X, entry.Location.Y));
				string location = String.Format("{0}° {1}'{2}, {3}° {4}'{5} ({6})", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W", world);
				TextDefinition.AddHtmlText(this, newX, y + 6, PAGE_BODY_WIDTH, 20, location, hue);
				y += 37; // Icon is extra tall
			}

			if (isConfiguring)
			{
				var previousY = y;
				// Set as Default
				AddButton(x, y, RIGHT_ARROW, RIGHT_ARROW, (int)ActionButtonType.SetRuneAsDefault, GumpButtonType.Reply, 0);
				TextDefinition.AddHtmlText(this, x + 40, y + 3, HALF_BODY_WIDTH, 20, "Set as Default", HtmlColors.WHITE);
				y += 30;

				// Drop Rune
				AddButton(x, y, RIGHT_ARROW, RIGHT_ARROW, (int)ActionButtonType.DropRune, GumpButtonType.Reply, 0);
				TextDefinition.AddHtmlText(this, x + 40, y + 3, HALF_BODY_WIDTH, 20, "Remove Rune", HtmlColors.WHITE);
				y += 30;

				x += HALF_BODY_WIDTH;
				y = previousY;

				if (m_Book.CanMove(entry, false))
				{
					// Move up
					AddButton(x, y, RIGHT_ARROW, RIGHT_ARROW, (int)ActionButtonType.MoveRune_Up, GumpButtonType.Reply, 0);
					TextDefinition.AddHtmlText(this, x + 40, y + 3, HALF_BODY_WIDTH, 20, "Move Up", HtmlColors.WHITE);
				}

				y += 30;

				if (m_Book.CanMove(entry, true))
				{
					// Move down
					AddButton(x, y, RIGHT_ARROW, RIGHT_ARROW, (int)ActionButtonType.MoveRune_Down, GumpButtonType.Reply, 0);
					TextDefinition.AddHtmlText(this, x + 40, y + 3, HALF_BODY_WIDTH, 20, "Move Down", HtmlColors.WHITE);
				}

				y += 30;
			}
		}

		private void AddSpell(ref int x, ref int y, SpellType spellType, bool settingDefault)
		{
			const int EMPTY_BOX = 3609;
			const int CHECKED_BOX = 4017;
			const int RIGHT_ARROW = 4005;

			var isDefault = spellType == m_Book.SpellType;
			var actionBase = settingDefault ? (int)ActionButtonType.SetDefaultSpellBase : (int)ActionButtonType.CastSpellBase;
			var graphic = settingDefault
				? isDefault
					? CHECKED_BOX
					: EMPTY_BOX
				: RIGHT_ARROW;

			string spellName;
			switch (spellType)
			{
				case SpellType.Astral_Travel: spellName = "Astral Travel"; break;
				case SpellType.Black_Gate: spellName = "Black Gate"; break;
				case SpellType.Demonic_Fire: spellName = "Demonic Fire"; break;
				case SpellType.Ethereal_Travel: spellName = "Ethereal Travel"; break;
				case SpellType.Nature_Passage: spellName = "Nature Passage"; break;
				case SpellType.Recall: spellName = "Recall"; break;
				case SpellType.Sacred_Journey: spellName = "Sacred Journey"; break;
				case SpellType.Elemental_Void: spellName = "Elemental Void"; break;
				case SpellType.Elemental_Gate: spellName = "Elemental Gate"; break;
				case SpellType.Gate: spellName = "Gate"; break;
				case SpellType.Mushroom_Gateway: spellName = "Mushroom Gateway"; break;
				case SpellType.Use_Charges: spellName = string.Format("Charges ({0}/{1})", m_Book.CurCharges, m_Book.MaxCharges); break;

				default: return;
			}

			AddButton(x, y, graphic, graphic, actionBase + (int)spellType, GumpButtonType.Reply, 0);
			TextDefinition.AddHtmlText(this, x + 40, y + 3, HALF_BODY_WIDTH, 20, spellName, isDefault ? HtmlColors.KHAKI : HtmlColors.WHITE);
			y += 30;
		}

		private void AddSpellSection(Mobile from, ref int x, ref int y)
		{
			var isConfiguring = m_PageType == PageType.Configuration;
			var hasDruidism = 0 < from.Skills[SkillName.Druidism].Value;
			var hasElementalism = 0 < from.Skills[SkillName.Elementalism].Value;
			var hasKnightship = 0 < from.Skills[SkillName.Knightship].Value;
			var hasMagery = 0 < from.Skills[SkillName.Magery].Value;
			var hasNecromancy = 0 < from.Skills[SkillName.Necromancy].Value;
			var hasResearchBag = Server.Misc.ResearchSettings.ResearchMaterials(from) != null;

			TextDefinition.AddHtmlText(this, x, y, HALF_BODY_WIDTH, 20, "Casting Options", HtmlColors.KHAKI);
			y += 30;

			var initialY = y;
			AddSpell(ref x, ref y, SpellType.Use_Charges, isConfiguring);
			if (isConfiguring || Server.Misc.GetPlayerInfo.isMonk(from)) AddSpell(ref x, ref y, SpellType.Astral_Travel, isConfiguring);
			if (isConfiguring || hasNecromancy) AddSpell(ref x, ref y, SpellType.Demonic_Fire, isConfiguring);
			if (isConfiguring || hasResearchBag) AddSpell(ref x, ref y, SpellType.Ethereal_Travel, isConfiguring);
			if (isConfiguring || hasElementalism) AddSpell(ref x, ref y, SpellType.Elemental_Void, isConfiguring);
			if (isConfiguring || hasDruidism) AddSpell(ref x, ref y, SpellType.Nature_Passage, isConfiguring);
			if (isConfiguring || hasMagery) AddSpell(ref x, ref y, SpellType.Recall, isConfiguring);
			if (isConfiguring || hasKnightship) AddSpell(ref x, ref y, SpellType.Sacred_Journey, isConfiguring);

			x += HALF_BODY_WIDTH;
			var finalY = y;
			y = initialY;

			if (isConfiguring || hasNecromancy) AddSpell(ref x, ref y, SpellType.Black_Gate, isConfiguring);
			if (isConfiguring || hasElementalism) AddSpell(ref x, ref y, SpellType.Elemental_Gate, isConfiguring);
			if (isConfiguring || hasMagery) AddSpell(ref x, ref y, SpellType.Gate, isConfiguring);
			if (isConfiguring || hasDruidism) AddSpell(ref x, ref y, SpellType.Mushroom_Gateway, isConfiguring);

			// Restore the lowest position
			y = Math.Max(y, finalY);
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			Mobile from = state.Mobile;
			var removeOpener = true;

			try
			{
				from.SendSound(0x55);

				if (m_Book.Deleted || !from.InRange(m_Book.GetWorldLocation(), (Core.ML ? 3 : 1)) || !Multis.DesignContext.Check(from))
					return;

				from.CloseGump(typeof(Sextants.MapGump));

				ActionButtonType buttonId = (ActionButtonType)info.ButtonID;
				switch (buttonId)
				{
					case ActionButtonType.RenameBook:
						{
							if (!m_Book.IsLockedDown || from.AccessLevel >= AccessLevel.GameMaster)
							{
								from.SendLocalizedMessage(502414); // Please enter a title for the runebook:
								from.Prompt = new RenameRunebookPrompt(m_Book, m_PageType, m_SelectedEntryIndex);
							}
							else
							{
								from.SendLocalizedMessage(502413, null, 0x35); // That cannot be done while the book is locked down.
							}
							break;
						}

					case ActionButtonType.SetRuneAsDefault:
						{
							if (m_SelectedEntry == null) return;

							if (m_Book.CheckAccess(from))
							{
								m_Book.Default = m_SelectedEntry;

								removeOpener = false;
								from.CloseGump(typeof(RunebookGump));
								from.SendGump(new RunebookGump(from, m_Book, m_PageType, m_SelectedEntryIndex));

								from.SendLocalizedMessage(502417); // New default location set.
							}
							break;
						}

					case ActionButtonType.RenameRune:
						{
							if (m_SelectedEntry == null) return;

							if (m_Book.CheckAccess(from))
							{
								from.SendLocalizedMessage(501804); // Please enter a description for this marked object.
								from.Prompt = new RenameEntryPrompt(m_Book, m_SelectedEntry, m_PageType, m_SelectedEntryIndex);
							}
							break;
						}

					case ActionButtonType.DropRune:
						{
							if (m_SelectedEntry == null) return;

							if (!m_Book.IsLockedDown || from.AccessLevel >= AccessLevel.GameMaster)
							{
								m_Book.DropRune(from, m_SelectedEntry);

								removeOpener = false;
								from.CloseGump(typeof(RunebookGump));
								from.SendGump(new RunebookGump(from, m_Book, m_PageType, m_SelectedEntryIndex));
							}
							else
							{
								from.SendLocalizedMessage(502413, null, 0x35); // That cannot be done while the book is locked down.
							}
							break;
						}

					case ActionButtonType.PreviewRuneLocation:
						{
							if (m_SelectedEntry == null) return;

							from.SendGump(new Sextants.MapGump(from, m_SelectedEntry.Map, m_SelectedEntry.Location.X, m_SelectedEntry.Location.Y, null));
							break;
						}

					case ActionButtonType.MoveRune_Up:
						{
							if (m_Book.CheckAccess(from))
							{
								int index = m_Book.MoveRune(m_SelectedEntry, false);
								if (index < 0) return;

								removeOpener = false;
								from.CloseGump(typeof(RunebookGump));
								from.SendGump(new RunebookGump(from, m_Book, m_PageType, index));
							}
							break;
						}
					case ActionButtonType.MoveRune_Down:
						{
							if (m_Book.CheckAccess(from))
							{
								int index = m_Book.MoveRune(m_SelectedEntry, true);
								if (index < 0) return;

								removeOpener = false;
								from.CloseGump(typeof(RunebookGump));
								from.SendGump(new RunebookGump(from, m_Book, m_PageType, index));
							}
							break;
						}
					default:
						{
							// Casting spell
							if (ActionButtonType.CastSpellBase <= buttonId)
							{
								SpellType spellId = SpellType.None;
								RunebookEntry entry = null;
								if (buttonId >= ActionButtonType.ExecuteDefaultSpellAgainstTargetBase) // Use default spell on provided rune
								{
									var runeIndex = (int)buttonId - (int)ActionButtonType.ExecuteDefaultSpellAgainstTargetBase;
									entry = m_Book.TryGetRune(runeIndex);
									spellId = m_Book.SpellType;
								}
								else if (buttonId >= ActionButtonType.CastSpellBase) // Use provided spell on default rune
								{
									entry = m_SelectedEntry;
									spellId = (SpellType)(buttonId - (int)ActionButtonType.CastSpellBase);
								}

								if (entry == null) return; // Make sure we have a valid target
								if (spellId == SpellType.None) return; // Make sure we have a valid spell

								switch (spellId)
								{
									// Self only
									case SpellType.Use_Charges:
										{
											if (m_Book.CurCharges < 1)
											{
												removeOpener = false;
												from.CloseGump(typeof(RunebookGump));
												from.SendGump(new RunebookGump(from, m_Book, m_PageType, m_SelectedEntryIndex));

												from.SendLocalizedMessage(502412); // There are no charges left on that item.
												return;
											}

											new TravelSpell(from, m_Book, entry, m_Book).Cast();
											break;
										}
									case SpellType.Astral_Travel:
										{
											if (!HasSpell(from, 251))
											{
												from.SendMessage("You do not have that skill!");
												return;
											}

											new AstralTravel(from, null, entry, null).Cast();
											break;
										}
									case SpellType.Demonic_Fire:
										{
											var potion = from.Backpack.FindItemByType(typeof(HellsGateScroll));
											if (potion == null)
											{
												from.SendMessage("You do not have that potion!");
												return;
											}

											new HellsGateSpell(from, null, entry, null).Cast();
											from.SendMessage("You empty a jar in the attempt.");
											from.AddToBackpack(new Jar());
											potion.Consume();
											break;
										}
									case SpellType.Ethereal_Travel:
										{
											// Reagent check still happens
											new ResearchEtherealTravel(from, null, entry, null).Cast();
											break;
										}
									case SpellType.Nature_Passage:
										{
											var potion = from.Backpack.FindItemByType(typeof(NaturesPassagePotion));
											if (potion == null)
											{
												from.SendMessage("You do not have that potion!");
												return;
											}

											new NaturesPassageSpell(from, null, entry, null).Cast();
											from.SendMessage("You empty a jar in the attempt.");
											from.AddToBackpack(new Jar());
											potion.Consume();
											break;
										}
									case SpellType.Recall:
										{
											if (!HasSpell(from, 31))
											{
												from.SendLocalizedMessage(500015); // You do not have that spell!
												return;
											}

											new RecallSpell(from, null, entry, null).Cast();
											break;
										}
									case SpellType.Sacred_Journey:
										{
											if (!HasSpell(from, 209))
											{
												from.SendLocalizedMessage(500015); // You do not have that spell!
												return;
											}

											new SacredJourneySpell(from, null, entry, null).Cast();
											break;
										}
									case SpellType.Elemental_Void:
										{
											if (!HasSpell(from, 315))
											{
												from.SendLocalizedMessage(500015); // You do not have that spell!
												return;
											}

											new Elemental_Void_Spell(from, null, entry, null).Cast();
											break;
										}

									// Gate spells
									case SpellType.Black_Gate:
										{
											var potion = from.Backpack.FindItemByType(typeof(GraveyardGatewayScroll));
											if (potion == null)
											{
												from.SendMessage("You do not have that potion!");
												return;
											}

											new UndeadGraveyardGatewaySpell(from, null, entry).Cast();
											from.SendMessage("You empty a jar in the attempt.");
											from.AddToBackpack(new Jar());
											potion.Consume();
											break;
										}
									case SpellType.Elemental_Gate:
										{
											if (!HasSpell(from, 326))
											{
												from.SendLocalizedMessage(500015); // You do not have that spell!
												return;
											}

											new Elemental_Gate_Spell(from, null, entry).Cast();
											break;
										}
									case SpellType.Gate:
										{
											if (!HasSpell(from, 51))
											{
												from.SendLocalizedMessage(500015); // You do not have that spell!
												return;
											}

											new GateTravelSpell(from, null, entry).Cast();
											break;
										}
									case SpellType.Mushroom_Gateway:
										{
											var potion = from.Backpack.FindItemByType(typeof(MushroomGatewayPotion));
											if (potion == null)
											{
												from.SendMessage("You do not have that potion!");
												return;
											}

											new MushroomGatewaySpell(from, null, entry).Cast();
											from.SendMessage("You empty a jar in the attempt.");
											from.AddToBackpack(new Jar());
											potion.Consume();
											break;
										}

									default:
										Console.WriteLine("Invalid runebook spell selected");
										return;
								}

								int xLong = 0, yLat = 0;
								int xMins = 0, yMins = 0;
								bool xEast = false, ySouth = false;

								if (Sextant.Format(entry.Location, entry.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
								{
									string location = String.Format("{0}° {1}'{2}, {3}° {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
									from.SendMessage(location);
								}

								m_Book.OnTravel();
							}
							else if (ActionButtonType.SetDefaultSpellBase <= buttonId) // Selecting default spell
							{
								var spellId = (SpellType)(info.ButtonID - (int)ActionButtonType.SetDefaultSpellBase);
								if (!Enum.IsDefined(typeof(SpellType), spellId)) return;

								m_Book.SpellType = spellId;
								removeOpener = false;
								from.CloseGump(typeof(RunebookGump));
								from.SendGump(new RunebookGump(from, m_Book, m_PageType, m_SelectedEntryIndex));
							}
							else if (ActionButtonType.SelectRuneBase <= buttonId) // Selecting rune
							{
								var index = info.ButtonID - (int)ActionButtonType.SelectRuneBase;
								if (index < 0 || index >= m_Book.Entries.Count) return;

								removeOpener = false;
								from.CloseGump(typeof(RunebookGump));
								from.SendGump(new RunebookGump(from, m_Book, m_PageType, index));
							}
							else if (ActionButtonType.ChangePage <= buttonId) // Changing Page
							{
								var newPage = (PageType)(info.ButtonID - (int)ActionButtonType.ChangePage);

								removeOpener = false;
								from.CloseGump(typeof(RunebookGump));
								from.SendGump(new RunebookGump(from, m_Book, newPage, m_SelectedEntryIndex));
							}

							break;
						}
				}
			}
			finally
			{
				if (removeOpener)
					m_Book.Openers.Remove(from);
			}
		}

		private class RenameRunebookPrompt : Prompt
		{
			private Runebook m_Book;
			private PageType m_PageType;
			private int m_SelectedEntryIndex;

			public RenameRunebookPrompt(Runebook book, PageType pageType, int selectedEntryIndex)
			{
				m_Book = book;
				m_PageType = pageType;
				m_SelectedEntryIndex = selectedEntryIndex;
			}

			public override void OnCancel(Mobile from)
			{
				from.SendLocalizedMessage(502415); // Request cancelled.

				if (!m_Book.Deleted && from.InRange(m_Book.GetWorldLocation(), (Core.ML ? 3 : 1)))
				{
					from.CloseGump(typeof(RunebookGump));
					from.SendGump(new RunebookGump(from, m_Book, m_PageType, m_SelectedEntryIndex));
				}
			}

			public override void OnResponse(Mobile from, string text)
			{
				if (m_Book.Deleted || !from.InRange(m_Book.GetWorldLocation(), (Core.ML ? 3 : 1)))
					return;

				if (m_Book.CheckAccess(from))
				{
					m_Book.Description = Utility.FixHtml(text.Trim());

					from.CloseGump(typeof(RunebookGump));
					from.SendGump(new RunebookGump(from, m_Book, m_PageType, m_SelectedEntryIndex));

					from.SendMessage("The book's title has been changed.");
				}
				else
				{
					m_Book.Openers.Remove(from);

					from.SendLocalizedMessage(502416); // That cannot be done while the book is locked down.
				}
			}
		}

		private class RenameEntryPrompt : Prompt
		{
			private readonly Runebook m_Book;
			private readonly RunebookEntry m_Rune;
			private readonly PageType m_PageType;
			private readonly int m_SelectedEntryIndex;

			public RenameEntryPrompt(Runebook book, RunebookEntry rune, PageType pageType, int selectedEntryIndex)
			{
				m_Book = book;
				m_Rune = rune;
				m_PageType = pageType;
				m_SelectedEntryIndex = selectedEntryIndex;
			}

			public override void OnResponse(Mobile from, string text)
			{
				if (m_Book.Deleted || !from.InRange(m_Book.GetWorldLocation(), (Core.ML ? 3 : 1)))
					return;

				if (m_Book.CheckAccess(from))
				{
					m_Rune.Description = text;
					from.SendLocalizedMessage(1010474); // The etching on the rune has been changed.

					from.CloseGump(typeof(RunebookGump));
					from.SendGump(new RunebookGump(from, m_Book, m_PageType, m_SelectedEntryIndex));
				}
				else
				{
					m_Book.Openers.Remove(from);

					from.SendLocalizedMessage(502416); // That cannot be done while the book is locked down.
				}
			}
		}

		public enum PageType
		{
			Index,
			Configuration,
			Help
		}

		private enum ActionButtonType
		{
			None = 0,
			RenameBook,

			PreviewRuneLocation,
			RenameRune,
			SetRuneAsDefault,
			DropRune,
			MoveRune_Up,
			MoveRune_Down,

			ChangePage = 50,
			SelectRuneBase = 100,
			SetDefaultSpellBase = 200,
			CastSpellBase = 300,
			ExecuteDefaultSpellAgainstTargetBase = 400,
		}

		public enum SpellType
		{
			None = 0,
			Use_Charges,
			Astral_Travel,
			Black_Gate,
			Demonic_Fire,
			Elemental_Gate,
			Elemental_Void,
			Ethereal_Travel,
			Gate,
			Mushroom_Gateway,
			Nature_Passage,
			Recall,
			Sacred_Journey,
		}
	}
}