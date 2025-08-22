using Server.Engines.CannedEvil;
using Server.Items;
using Server.Network;
using System.Linq;

namespace Server.Gumps
{
	public class ChampionSkullGump : Gump
	{
		private enum ActionButtonType
		{
			Fight = 1,
			SpawnSizeBase = 100,
			MonsterDifficultyBase = 200,
		}

		private readonly ChampionSkull _championSkull;
		private readonly Difficulty _monsterDifficulty;
		private readonly int _spawnSize;

		public ChampionSkullGump(Mobile from, ChampionSkull championSkull, int spawnSize = 1, Difficulty monsterDifficulty = Difficulty.Easy) : base(50, 50)
		{
			_championSkull = championSkull;
			_spawnSize = spawnSize;
			_monsterDifficulty = monsterDifficulty;

			from.CloseGump(typeof(ChampionSkullGump));

			AddImage(0, 0, 9582, Server.Misc.PlayerSettings.GetGumpHue(from));

			int x = 10;
			int y = 10;
			TextDefinition.AddHtmlText(this, x, y, 550, 20, string.Format("Champion Spawn - {0} - {1}", ChampionSpawnInfo.GetName(championSkull.Type), ChampionSpawnInfo.GetChampionName(championSkull.Type)), HtmlColors.COOL_BLUE);
			x += 10;
			y += 20;

			var slayers = ChampionSpawnInfo.GetRelevantSlayers(championSkull.Type).Select(slayer => SlayerGroup.GetName(slayer));
			if (slayers != null && slayers.Any())
			{
				TextDefinition.AddHtmlText(this, x, y, 300, 20, "Relevant Slayers", HtmlColors.COOL_BLUE);
				y += 20;
				TextDefinition.AddHtmlText(this, x + 10, y, 550, 40, string.Join(", ", slayers), HtmlColors.COOL_BLUE);
				y += 20;
				y += 20;
			}

			AddRewards(x, y);
			y += 102;

			AddSlider(x, y, "Spawn Size", ActionButtonType.SpawnSizeBase, ChampionSpawn.MAX_SPAWN_SIZE_MOD, spawnSize - 1);
			y += 75;

			AddSlider(x, y, "Monster Difficulty", ActionButtonType.MonsterDifficultyBase, (int)Difficulty.Deadly + 1, (int)monsterDifficulty);
			y += 75;

			// Final button
			x = 511;
			y = 350;
			AddButton(x, y, 4005, 4007, (int)ActionButtonType.Fight, GumpButtonType.Reply, 0); // Next button
			TextDefinition.AddHtmlText(this, x + 33, y + 3, 50, 20, "FIGHT!", HtmlColors.COOL_BLUE);
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info.ButtonID == 0) return;

			Mobile from = state.Mobile;
			if (from == null) return;

			if (info.ButtonID == (int)ActionButtonType.Fight)
			{
				from.SendMessage("Target the champion idol you would like to challenge.");
				from.Target = new ChampionIdolTarget(_championSkull, _spawnSize, _monsterDifficulty);
			}
			else if ((int)ActionButtonType.MonsterDifficultyBase <= info.ButtonID)
			{
				from.SendGump(new ChampionSkullGump(state.Mobile, _championSkull, _spawnSize, (Difficulty)(info.ButtonID - (int)ActionButtonType.MonsterDifficultyBase)));
			}
			else if ((int)ActionButtonType.SpawnSizeBase <= info.ButtonID)
			{
				from.SendGump(new ChampionSkullGump(state.Mobile, _championSkull, info.ButtonID - (int)ActionButtonType.SpawnSizeBase + 1, _monsterDifficulty)); // +1 -- This gump uses a 0-based index for Spawn Size
			}
		}

		private void AddRewards(int x, int y)
		{
			TextDefinition.AddHtmlText(this, x, y, 300, 20, "Rewards", HtmlColors.COOL_BLUE);
			x += 10;
			y += 20;

			TextDefinition.AddHtmlText(this, x, y, 550, 20, "Improved by increasing Size and Difficulty", HtmlColors.COOL_BLUE);
			y += 25;

			// Gold
			AddImage(x, y, 10329); // Black box with gold
			AddTooltip("Gold");
			var goldPercentage = ChampionRewards.GetGoldPercent(_spawnSize, _monsterDifficulty);
			TextDefinition.AddHtmlText(this, x, y + 35, 32, 20, string.Format("<CENTER>{0}%</CENTER>", goldPercentage), HtmlColors.COOL_BLUE);

			// Powerscrolls (Count, Percentage)
			x += 50;
			AddImage(x, y, 10351); // Black box
			TextDefinition.AddHtmlText(this, x, y + 35, 32, 20, string.Format("<CENTER>{0}</CENTER>", ChampionRewards.GetPowerscrollDropCount(_spawnSize, _monsterDifficulty)), HtmlColors.COOL_BLUE);
			AddItem(x - 10, y + 1, 0x14F0, 0x481); // White Scroll
			AddTooltip("Powerscrolls");

			// Boss tier item
			x += 50;
			var bossItemChance = ChampionRewards.GetBossItemDropChance(_spawnSize, _monsterDifficulty);
			if (0 < bossItemChance)
			{
				AddImage(x, y, 10349); // Black box with question mark
				AddTooltip("A powerful item");
				TextDefinition.AddHtmlText(this, x, y + 35, 32, 20, string.Format("<CENTER>{0}%</CENTER>", bossItemChance), HtmlColors.COOL_BLUE);
			}

			// Treasure Chest
			var treasureChestChance = ChampionRewards.GetTreasureChestDropChance(_spawnSize, _monsterDifficulty);
			if (0 < treasureChestChance)
			{
				x += 50;
				AddImage(x, y, 10349); // Black box with question mark
				AddTooltip("Treasure Chest");
				TextDefinition.AddHtmlText(this, x, y + 35, 32, 20, string.Format("<CENTER>{0}%</CENTER>", treasureChestChance), HtmlColors.COOL_BLUE);
			}

			// Artifact
			var artifactChance = ChampionRewards.GetArtifactDropChance(_spawnSize, _monsterDifficulty);
			if (0 < artifactChance)
			{
				x += 50;
				AddImage(x, y, 10339); // Black box with goblet
				AddTooltip("Artifact");
				TextDefinition.AddHtmlText(this, x, y + 35, 32, 20, string.Format("<CENTER>{0}%</CENTER>", artifactChance), HtmlColors.COOL_BLUE);
			}
		}

		private void AddSlider(int x, int y, string label, ActionButtonType baseValue, int ticks, int selectedValue)
		{
			TextDefinition.AddHtmlText(this, x, y, 300, 20, label, HtmlColors.COOL_BLUE);
			AddTooltip(baseValue == ActionButtonType.SpawnSizeBase ? "Increase the amount of minions that spawn and are required per level" : "Increase the health and damage of minions");
			x += 10;
			y += 20;

			const int WIDTH = 550;
			const int WIDTH_WITHOUT_OUTER_BUTTONS = WIDTH - 14;
			var space = (int)((double)WIDTH_WITHOUT_OUTER_BUTTONS / (ticks - 1));
			AddImageTiled(x, y + 18, WIDTH, 3, 2700);

			for (int i = 0; i < ticks; i++)
			{
				var isSelected = i == selectedValue;
				const int UP_ARROW = 5600;
				const int DOWN_ARROW = 5602;

				if (isSelected)
				{
					AddImage(x + (i * space), y, DOWN_ARROW);
					AddImage(x + (i * space), y + 24, UP_ARROW);
				}
				else
				{
					AddButton(x + (i * space), y + 24, UP_ARROW, UP_ARROW, ((int)baseValue) + i, GumpButtonType.Reply, 0);
				}

				switch (baseValue)
				{
					case ActionButtonType.SpawnSizeBase:
						AddTooltip(string.Format("Size: {0}", i + 1));
						break;

					case ActionButtonType.MonsterDifficultyBase:
						AddTooltip(string.Format("Difficulty: {0}", (Difficulty)i));
						break;
				}
			}
		}
	}
}