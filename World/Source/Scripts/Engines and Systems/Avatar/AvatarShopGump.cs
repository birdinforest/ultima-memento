using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Server.Engines.Avatar
{
	public class AvatarShopGump : Gump
	{
		private const int BLANK_ITEM_ID = 0;
		private const int CARD_HEIGHT = 68;
		private const int CARD_WIDTH = 864 - NAVIGATION_WIDTH;
		private const int CATEGORY_WIDTH = NAVIGATION_WIDTH - 28 - 20;
		private const int FAT_BOTTLE_ITEM_ID = 0x1FD9;
		private const int GIANT_COIN_ITEM_ID = 0x4FAD;
		private const int GOLD_STACK_ITEM_ID = 0x0EEF;
		private const int NAVIGATION_WIDTH = 152 + 20 + 20;
		private const int NO_ITEM_ID = -1;

		private static readonly List<Categories> m_Categories = new List<Categories>
		{
			Categories.Information,
			Categories.Limits,
			Categories.Rates,
			Categories.Boosts,
			Categories.Items,
		};

		private readonly PlayerContext m_Context;
		private readonly PlayerMobile m_From;
		private readonly Action m_onGumpClose;
		private readonly int m_PageNumber;
		private readonly List<IReward> m_Rewards;
		private readonly Categories m_SelectedCategory;

		public AvatarShopGump(PlayerMobile from, Categories selectedCategory = Categories.Information, int pageNumber = 1, Action onGumpClose = null) : base(25, 25)
		{
			m_Context = AvatarEngine.Instance.GetOrCreateContext(from);
			m_From = from;
			m_onGumpClose = onGumpClose;
			m_PageNumber = pageNumber;
			m_SelectedCategory = selectedCategory;

			AddPage(0);

			const int BACKGROUND_WIDTH = 904;
			const int BACKGROUND_HEIGHT = 729;
			AddBackground(0, 0, BACKGROUND_WIDTH, BACKGROUND_HEIGHT, 2620);

			const int GUMP_WIDTH = BACKGROUND_WIDTH;
			const int GUMP_HEIGHT = BACKGROUND_HEIGHT;
			TextDefinition.AddHtmlText(this, 11, 11, GUMP_WIDTH, 20, "<CENTER>Ascensions</CENTER>", HtmlColors.COOL_BLUE);

			AddCategoryList(selectedCategory, 27, 48);

			var y = 48;

			if (selectedCategory == Categories.Information)
			{
				AddInformationCard(NO_ITEM_ID, "An Avatar is Born", "You have begun the Avatar's Ascent. This is a challenging journey of self-discovery and growth.", y);
				y += CARD_HEIGHT;
				y += 10;

				AddInformationCard(NO_ITEM_ID, "Everything is Temporary", "Upon death, your character will be reborn and all evidence of your former life will be destroyed.", y);
				y += CARD_HEIGHT;
				y += 10;

				AddInformationCard(NO_ITEM_ID, "Permanent Enhancements", string.Format(
					"{0} and {1} will apply to each rebirth.",
					TextDefinition.GetColorizedText("Limits", HtmlColors.ORANGE),
					TextDefinition.GetColorizedText("Rates", HtmlColors.ORANGE)
				), y);
				y += CARD_HEIGHT;
				y += 10;

				AddInformationCard(NO_ITEM_ID, "Temporary Enhancements", string.Format(
					"{0} and {1} are temporary and will be lost upon death.",
					TextDefinition.GetColorizedText("Boosts", HtmlColors.ORANGE),
					TextDefinition.GetColorizedText("Items", HtmlColors.ORANGE)
				), y);
				y += CARD_HEIGHT;
				y += 10;

				AddInformationCard(NO_ITEM_ID, "Opportunity Only Knocks Once...", "Your vessel is frail; you may not purchase enhancements once you leave the Gypsy encampment.", y);
				y += CARD_HEIGHT;
				y += 10;

				return;
			}

			var rewards = CreateRewards(selectedCategory, m_Context);
			if (rewards != null)
			{
				const int ITEMS_PER_PAGE = 8;
				var skip = (pageNumber - 1) * ITEMS_PER_PAGE;
				var itemIndex = skip;

				if (pageNumber == 1)
				{
					m_Context.PointsSaved = 123456789;
					var message = m_Context.PointsSaved > 0 ? string.Format("You've earned {0} coins.", TextDefinition.GetColorizedText(m_Context.PointsSaved.ToString("n0"), HtmlColors.ORANGE)) : "Your war chest is empty.";
					message += "<br>Coins are earned by killing monsters and completing quests.";
					AddInformationCard(GIANT_COIN_ITEM_ID, "Your War Chest", message, y, false);
					y += CARD_HEIGHT;
					y += 10;
				}

				m_Rewards = new List<IReward>();
				foreach (var reward in rewards.Skip(skip).Take(ITEMS_PER_PAGE))
				{
					m_Rewards.Add(reward);
					AddCard(m_Context.PointsSaved, reward.Graphic, reward.Name, reward.Description, reward.Cost, itemIndex, y);

					y += CARD_HEIGHT;
					y += 10;
					++itemIndex;
				}

				y = GUMP_HEIGHT - 33;

				// Prev page
				if (0 < skip)
					AddButton(NAVIGATION_WIDTH + 20, y, 4014, 4015, (int)_Actions.PageBase + (pageNumber - 1), GumpButtonType.Reply, 0);

				// Next page
				var remain = rewards.Count - skip - ITEMS_PER_PAGE;
				if (0 < remain)
					AddButton(GUMP_WIDTH - 47, y, 4005, 4007, (int)_Actions.PageBase + (pageNumber + 1), GumpButtonType.Reply, 0);
			}
		}

		public enum Categories
		{
			Information = 0,
			Unlocks,
			Limits,
			Rates,
			Boosts,
			Items,
		}

		private enum _Actions
		{
			Close = 0,
			SelectCategoryBase = 10,
			PurchaseBase = 50,
			PageBase = 500,
		}

		private interface IReward
		{
			int Cost { get; }
			string Description { get; }
			int Graphic { get; }
			string Name { get; }
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var buttonID = info.ButtonID;
			if (buttonID == 0)
			{
				if (m_onGumpClose != null)
					m_onGumpClose();
				return;
			}

			var player = sender.Mobile as PlayerMobile;
			if (player == null) return;

			if ((int)_Actions.PageBase <= buttonID) // Page is higher
			{
				var page = buttonID - (int)_Actions.PageBase;
				sender.Mobile.SendGump(new AvatarShopGump(m_From, m_SelectedCategory, page, m_onGumpClose));
				return;
			}
			else if ((int)_Actions.PurchaseBase <= buttonID) // Purchase is higher
			{
				sender.Mobile.SendGump(new AvatarShopGump(m_From, m_SelectedCategory, m_PageNumber, m_onGumpClose));
			}
			else if ((int)_Actions.SelectCategoryBase <= buttonID) // Select Category is higher
			{
				var selectedCategory = (Categories)(buttonID - (int)_Actions.SelectCategoryBase);
				sender.Mobile.SendGump(new AvatarShopGump(m_From, selectedCategory, 1, m_onGumpClose));
				return;
			}
		}

		private void AddCard(
			int points,
			int itemId,
			string name,
			string description,
			int cost,
			int index,
			int y,
			bool scrollable = false
			)
		{
			const int START_X = NAVIGATION_WIDTH + 20;
			const int GRAPHIC_SLOT_WIDTH = 73;
			const int GRAPHIC_SLOT_HEIGHT = 68;
			const int PURCHASE_WIDTH = 130;

			int x = START_X;

			AddBackground(x, y, CARD_WIDTH, CARD_HEIGHT + 5, 2620);

			// Item image
			if (itemId > BLANK_ITEM_ID)
			{
				AddBackground(x, y, GRAPHIC_SLOT_WIDTH, CARD_HEIGHT + 5, 2620);
				if (itemId > NO_ITEM_ID)
					GumpUtilities.AddCenteredItemToGump(this, itemId, x, y, GRAPHIC_SLOT_WIDTH, GRAPHIC_SLOT_HEIGHT);
				x += GRAPHIC_SLOT_WIDTH;
			}

			// Item text
			x += 10;
			y += 5; // Top padding

			// int DESCRIPTION_WIDTH = CARD_WIDTH - GRAPHIC_SLOT_WIDTH + 10 - PURCHASE_WIDTH;
			int DESCRIPTION_WIDTH = CARD_WIDTH - (x - START_X);
			if (0 < cost) DESCRIPTION_WIDTH -= PURCHASE_WIDTH;

			const int LAZY_AMOUNT = 30; // Arbitrary value to account for left padding
			TextDefinition.AddHtmlText(this, x, y, DESCRIPTION_WIDTH - LAZY_AMOUNT, 20, name, HtmlColors.ORANGE);
			TextDefinition.AddHtmlText(this, x + 10, y + 20, DESCRIPTION_WIDTH - LAZY_AMOUNT, 40, TextDefinition.GetColorizedText(description, HtmlColors.COOL_BLUE), false, scrollable);

			if (0 < cost)
			{
				x = START_X + CARD_WIDTH - PURCHASE_WIDTH;
				y += 6;

				// Purchase section
				const int GRAPHIC_WIDTH = 55;
				const int GRAPHIC_HEIGHT = 22;
				const int POINTS_ITEM = 0x0EEC;
				var canAfford = cost <= points;
				var pointsColor = canAfford ? HtmlColors.ORANGE : HtmlColors.RED;
				// GumpUtilities.AddCenteredItemToGump(this, POINTS_ITEM, x, y, GRAPHIC_WIDTH, GRAPHIC_HEIGHT, 0x44C);
				TextDefinition.AddHtmlText(this, x + GRAPHIC_WIDTH, y + 2, 50, 20, cost.ToString("n0"), pointsColor);

				if (canAfford)
				{
					y += 30;

					AddButton(x + 13, y - 1, 4023, 4023, (int)_Actions.PurchaseBase + index, GumpButtonType.Reply, 0); // OK
					TextDefinition.AddHtmlText(this, x + GRAPHIC_WIDTH, y + 2, 60, 20, "Purchase", HtmlColors.ORANGE);
				}
			}
		}

		private void AddCategoryList(Categories selectedCategory, int x, int y)
		{
			int i = 0;
			foreach (var category in m_Categories)
			{
				var isSelected = selectedCategory == category;
				const int CARD_HEIGHT = 34;
				const int TOP_PADDING = 6;
				const int HEIGHT_PER_ITEM = CARD_HEIGHT + TOP_PADDING; // Extra top padding
				const int LEFT_PADDING = 36;

				if (!isSelected)
				{
					const int HIDDEN_BUTTON_ID = 1150;
					const int HIDDEN_BUTTON_WIDTH = 28;

					int buttonId = (int)_Actions.SelectCategoryBase + (int)category;
					AddButton(x, y + TOP_PADDING + (i * HEIGHT_PER_ITEM), HIDDEN_BUTTON_ID, HIDDEN_BUTTON_ID, buttonId, GumpButtonType.Reply, 0);
					AddButton(x + HIDDEN_BUTTON_WIDTH, y + TOP_PADDING + (i * HEIGHT_PER_ITEM), HIDDEN_BUTTON_ID, HIDDEN_BUTTON_ID, buttonId, GumpButtonType.Reply, 0);
					AddButton(x + HIDDEN_BUTTON_WIDTH + HIDDEN_BUTTON_WIDTH, y + TOP_PADDING + (i * HEIGHT_PER_ITEM), HIDDEN_BUTTON_ID, HIDDEN_BUTTON_ID, buttonId, GumpButtonType.Reply, 0);
				}

				AddBackground(x, y + (i * HEIGHT_PER_ITEM), CATEGORY_WIDTH, HEIGHT_PER_ITEM - 6, 2620);

				var gemGraphic = isSelected ? 1210 : 1209;
				var gemHue = isSelected ? 1152 : 0;
				AddImage(x + 17, y + 10 + (i * HEIGHT_PER_ITEM), gemGraphic, gemHue);

				var color = isSelected ? HtmlColors.ORANGE : HtmlColors.COOL_BLUE;
				TextDefinition.AddHtmlText(this, x + LEFT_PADDING, y + 7 + (i * HEIGHT_PER_ITEM), CATEGORY_WIDTH - LEFT_PADDING, 16, category.ToString(), color);
				++i;
			}
		}

		private void AddInformationCard(int itemId, string name, string description, int y, bool scrollable = false)
		{
			AddCard(0, itemId, name, description, 0, 0, y, scrollable);
		}

		private List<IReward> CreateRewards(Categories selectedCategory, PlayerContext context)
		{
			switch (selectedCategory)
			{
				default:
				case Categories.Information:
					{
						// Never reached
						return null;
					}

				case Categories.Unlocks:
					{
						return new List<IReward>
						{
						};
					}

				case Categories.Limits:
					{
						return new List<IReward>
						{
							_Reward.Create(
								SecondOrderCost(1000, context.SkillCapLevel + 1),
								FAT_BOTTLE_ITEM_ID,
								string.Format("Skill Cap ({0} of {1})", context.SkillCapLevel, PlayerContext.SKILL_CAP_MAX_LEVEL),
								string.Format("Increases the skill cap by {0}. Current bonus: {1}", PlayerContext.SKILL_CAP_PER_LEVEL, PlayerContext.SKILL_CAP_PER_LEVEL * context.SkillCapLevel),
								context.SkillCapLevel < PlayerContext.SKILL_CAP_MAX_LEVEL,
								() => context.SkillCapLevel += 1
							),

							_Reward.Create(
								SecondOrderCost(100, context.StatCapLevel + 1),
								FAT_BOTTLE_ITEM_ID,
								string.Format("Stat Cap ({0} of {1})", context.StatCapLevel, PlayerContext.STAT_CAP_MAX_LEVEL),
								string.Format("Increases the stat cap by {0}. Current bonus: {1}", PlayerContext.STAT_CAP_PER_LEVEL, PlayerContext.STAT_CAP_PER_LEVEL * context.StatCapLevel),
								context.StatCapLevel < PlayerContext.STAT_CAP_MAX_LEVEL,
								() => context.StatCapLevel += 1
							),
						};
					}

				case Categories.Rates:
					{
						return new List<IReward>
						{
							_Reward.Create(
								SecondOrderCost(100, context.PointGainRateLevel + 1),
								NO_ITEM_ID,
								string.Format("Point Gain Rate ({0} of {1})", context.PointGainRateLevel, PlayerContext.POINT_GAIN_RATE_MAX_LEVEL),
								string.Format("Increases the point gain rate by {0}%. Current bonus: {1}%", PlayerContext.POINT_GAIN_RATE_PER_LEVEL, PlayerContext.POINT_GAIN_RATE_PER_LEVEL * context.PointGainRateLevel),
								context.PointGainRateLevel < PlayerContext.POINT_GAIN_RATE_MAX_LEVEL,
								() => context.PointGainRateLevel += 1
							),
							_Reward.Create(
								ExponentialCost(2000, context.SkillGainRateLevel + 1),
								NO_ITEM_ID,
								string.Format("Skill Gain Rate ({0} of {1})", context.SkillGainRateLevel, PlayerContext.SKILL_GAIN_RATE_MAX_LEVEL),
								string.Format("Increases the skill gain rate by {0}%. Current bonus: {1}%", PlayerContext.SKILL_GAIN_RATE_PER_LEVEL, PlayerContext.SKILL_GAIN_RATE_PER_LEVEL * context.SkillGainRateLevel),
								context.SkillGainRateLevel < PlayerContext.SKILL_GAIN_RATE_MAX_LEVEL,
								() => context.SkillGainRateLevel += 1
							)
						};
					}

				case Categories.Boosts:
					{
						return new List<IReward>
						{
						};
					}

				case Categories.Items:
					{
						return new List<IReward>
						{
						};
					}
			}
		}

		private int ExponentialCost(int baseCost, int level)
		{
			var cost = baseCost;
			if (level <= 0) return cost;

			for (int i = 0; i < level; i++)
			{
				cost *= 2;
			}

			return cost;
		}

		private int SecondOrderCost(double baseCost, int level)
		{
			return (int)(baseCost * Math.Pow(level, 2) + baseCost * level);
		}

		private class _ItemReward : IReward
		{
			private static readonly TextInfo m_TextInfo = new CultureInfo("en-US", false).TextInfo;

			private _ItemReward()
			{
			}

			public int Cost { get; private set; }
			public string Description { get; private set; }
			public int Graphic { get; private set; }
			public string Name { get; private set; }
			public Func<Item> OnSelect { get; set; }

			public static IReward Create<T>(int cost, Func<T> onSelect, int amount = 0, string name = null, string description = null, int graphicOverride = BLANK_ITEM_ID) where T : Item, new()
			{
				var itemSnapshot = ItemSnapshotCache.GetOrCreate(typeof(T));

				if (string.IsNullOrEmpty(name)) name = m_TextInfo.ToTitleCase(itemSnapshot.Name);
				if (0 < amount) name = string.Format("{0} ({1})", name, amount);

				if (string.IsNullOrEmpty(description)) description = itemSnapshot.DefaultDescription;

				return Create(cost, graphicOverride != BLANK_ITEM_ID ? graphicOverride : itemSnapshot.ItemID, name, description, onSelect);
			}

			public static IReward Create(int cost, int graphic, string name, string description, Func<Item> onSelect)
			{
				return new _ItemReward
				{
					Graphic = graphic,
					Name = name,
					Description = description,
					Cost = cost,
					OnSelect = onSelect,
				};
			}
		}

		private class _Reward : IReward
		{
			private _Reward()
			{
			}

			public bool CanSelect { get; set; }
			public int Cost { get; private set; }
			public string Description { get; private set; }
			public int Graphic { get; private set; }
			public string Name { get; private set; }
			public Action OnSelect { get; set; }

			public static IReward Create(int cost, int graphic, string name, string description, bool canSelect, Action onSelect)
			{
				return new _Reward
				{
					Graphic = graphic,
					Name = name,
					Description = description,
					Cost = cost,
					CanSelect = canSelect,
					OnSelect = onSelect,
				};
			}
		}
	}
}