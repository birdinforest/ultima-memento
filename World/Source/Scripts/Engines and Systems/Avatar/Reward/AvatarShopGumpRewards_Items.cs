using Server.Items;
using Server.Mobiles;
using Server.Multis;
using System.Collections.Generic;

namespace Server.Engines.Avatar
{
	public partial class RewardFactory
	{
		public static List<IReward> CreateItemRewards(PlayerMobile from, PlayerContext context)
		{
			return new List<IReward>
			{
				// Currency
				ItemReward.Create(
					ONE_THOUSAND_GOLD,
					true,
					() => { return new Gold(500); },
					amount: 500,
					graphicOverride: AvatarShopGump.GOLD_STACK_ITEM_ID
				),
				ItemReward.Create(
					5 * ONE_THOUSAND_GOLD,
					true,
					() => { return new Gold(5000); },
					amount: 5000,
					graphicOverride: AvatarShopGump.GOLD_STACK_ITEM_ID
				),

				// Resources
				ItemReward.Create(
					2 * ONE_HUNDRED_GOLD,
					true,
					() => { return new IronIngot(50); },
					50
				).WithDescription("A handful of ingots to get you started."),
				ItemReward.Create(
					ONE_HUNDRED_GOLD,
					true,
					() => { return new Fabric(50); },
					50
				).WithDescription("A handful of fabric to get you started."),
				ItemReward.Create(
					5 * TEN_GOLD,
					true,
					() => { return new Bottle(); },
					10
				).WithDescription("A handful of bottles to get you started."),

				// Tools
				ItemReward.Create(
					5 * ONE_HUNDRED_GOLD,
					true,
					() => { return new TinkerTools(); }
				),
				ItemReward.Create(
					5 * TEN_GOLD,
					true,
					() => { return new SmithHammer(); }
				),
				ItemReward.Create(
					5 * TEN_GOLD,
					true,
					() => { return new CarpenterTools(); }
				),
				ItemReward.Create(
					5 * TEN_GOLD,
					true,
					() => { return new SewingKit(); }
				),
				ItemReward.Create(
					5 * TEN_GOLD,
					true,
					() => { return new Hatchet(); }
				),
				ItemReward.Create(
					5 * TEN_GOLD,
					true,
					() => { return new Spade(); }
				),
				ItemReward.Create(
					5 * TEN_GOLD,
					true,
					() => { return new FishingPole(); }
				),
				ItemReward.Create(
					5 * TEN_GOLD,
					true,
					() => { return new Scissors(); }
				),

				// Equipment
				ItemReward.Create(
					ONE_THOUSAND_GOLD,
					true,
					() => { return new HikingBoots(); }
				),
				ItemReward.Create(
					5 * ONE_HUNDRED_GOLD,
					true,
					() => { return new BookOfChivalry(); }
				),
				ItemReward.Create(
					5 * ONE_HUNDRED_GOLD,
					true,
					() => { return new NecromancerSpellbook(); }
				).WithName("Necromancer Spellbook (Empty)"),
				ItemReward.Create(
					5 * ONE_HUNDRED_GOLD,
					true,
					() => { return new Spellbook(); }
				).WithName("Mage's Spellbook (Empty)"),
				ItemReward.Create(
					5 * ONE_HUNDRED_GOLD,
					true,
					() => { return new ElementalSpellbook(); }
				).WithName("Elementalist Spellbook (Empty)"),

				// Utility
				ItemReward.Create(
					5 * ONE_HUNDRED_GOLD,
					true,
					() => {
						var bag = new Bag();
						bag.AddItem(new Scissors());
						bag.AddItem(new Fabric(50));

						return bag;
						}
				).WithName("Healer's Kit").WithDescription("Contains scissors and fabric."),
				ItemReward.Create(
					5 * ONE_HUNDRED_GOLD,
					true,
					() => {
						var bag = new Bag();
						bag.AddItem(new CurePotion() { Amount = 10 });
						bag.AddItem(new HealPotion() { Amount = 10 });
						bag.AddItem(new RefreshPotion() { Amount = 10 });

						return bag;
						}
				).WithName("Warrior's Potion Bag").WithDescription("Contains healing, cure, and refresh potions."),
				ItemReward.Create(
					ONE_THOUSAND_GOLD,
					true,
					() => { return new BagOfReagents(); }
				).WithName("Bag of Reagents").WithDescription("Contains magery reagents for spells."),
				ItemReward.Create(
					5 * ONE_HUNDRED_GOLD,
					true,
					() => { return new BagOfNecroReagents(); }
				).WithName("Bag of Necro Reagents").WithDescription("Contains necromancy reagents for spells."),
				ItemReward.Create(
					10 * ONE_THOUSAND_GOLD,
					true,
					() => { return new SmallBoatDeed(); }
				).WithDescription("Hit the seas sailing!"),
				ItemReward.Create(
					30 * ONE_THOUSAND_GOLD,
					true,
					() => { return new MagicCarpetADeed(); }
				).WithDescription("I can show you the world!"),

				// Tames
				ItemReward.Create(
					ONE_HUNDRED_GOLD,
					true,
					() => { return new CagedHorse(); }
				).WithDescription("A horse is a great way to get around."),
				ItemReward.Create(
					ONE_THOUSAND_GOLD,
					true,
					() => { return new CagedPackHorse(); }
				).WithDescription("For when you're too weak to carry things yourself."),
			};
		}
	}
}