using Server.Engines.Craft;
using Server.Items;
using Server.Mobiles;
using Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.GlobalShoppe
{
	[Flipable(0x3CF1, 0x3CF2)]
	public class BakerShoppe : CustomerOrderShoppe<OrderContext>
	{
		[Constructable]
		public BakerShoppe() : base(0x3CF1)
		{
			Name = "Baker Work Shoppe";
		}

		public BakerShoppe(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem { get { return DefCooking.CraftSystem; } }

		public override NpcGuild Guild { get { return NpcGuild.CulinariansGuild; } }

		protected override SkillName PrimarySkill { get { return SkillName.Cooking; } }

		protected override ShoppeType ShoppeType { get { return ShoppeType.Baker; } }

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is CulinarySet)
				return AddTools(from, dropped);
			if (dropped is Dough || dropped is SweetDough)
				return AddResource(from, dropped);

			return base.OnDragDrop(from, dropped);
		}

		public override void PrepareOrders(TradeSkillContext context)
		{
			context.Orders.ForEach(untypedOrder =>
			{
				var order = untypedOrder as OrderContext;
				if (order == null)
				{
					Console.WriteLine("Failed to set Baking rewards for order ({0})", untypedOrder.GetType().Name);
					return;
				}

				if (order.IsInitialized) return;

				var rewards = CookingRewardCalculator.Instance;
				rewards.SetRewards(context, order);

				var item = ShoppeItemCache.GetOrCreate(order.Type);
				order.GraphicId = item.ItemID;
				order.ItemName = item.Name;
				order.Person = CreatePersonName();

				order.IsInitialized = true;
			});
		}

		protected override IEnumerable<OrderContext> CreateOrders(CraftSystem craftSystem, Mobile from, TradeSkillContext context, int count)
		{
			if (count < 1) yield break;

			// Build item list
			var items = GetCraftItems(from, craftSystem)
				.Where(i => !TypeUtilities.IsAnyTypeOrDerived(i.ItemType,
						// Base ingredients
						typeof(SackFlour),
						typeof(Dough),
						typeof(SweetDough),

						// Unprepared items
						typeof(CookableFood),

						// Pumpkins
						typeof(BaseLight),

						// Only purchasable
						typeof(WoodenBowlOfPeas)
					)
				)
				.ToList();
			if (items.Count < 1) yield break;

			// Add 5x quantity bonus for every 5 points over 100
			var amountBonus = 5 * (int)(Math.Max(0, from.Skills[craftSystem.MainSkill].Value - 100) / 5);

			for (int i = 0; i < count; i++)
			{
				var item = Utility.Random(items);
				if (item == null) yield break;

				var amount = amountBonus + Utility.RandomMinMax(15, 40);

				var order = new OrderContext(item.ItemType)
				{
					MaxAmount = amount,
					CurrentAmount = 0,
				};

				yield return order;
			}
		}

		protected override string CreateTask(TradeSkillContext context)
		{
			string task = null;
			switch (Utility.RandomMinMax(1, 3))
			{
				case 1: task = "Make"; break;
				case 2: task = "Cook"; break;
				case 3: task = "Bake"; break;
			}

			switch (Utility.RandomMinMax(1, 10))
			{
				case 1: task += " a batch"; break;
				case 2: task += " some"; break;
				case 3: task += " a pack"; break;
				case 4: task += " a pile"; break;
				case 5: task += " a sack"; break;
				case 6: task += " a box"; break;
				case 7: task += " a crate"; break;
				case 8: task += " a bundle"; break;
				case 9: task += " a bunch"; break;
				case 10: task += " a pan"; break;
			}

			string[] sTastes = new string[] { "aged", "bitter", "bittersweet", "bland", "burnt", "buttery", "chalky", "cheesy", "chewy", "chocolaty", "citrusy", "cool", "creamy", "crispy", "crumbly", "crunchy", "crusty", "doughy", "dry", "earthy", "eggy", "fatty", "fermented", "fiery", "fishy", "fizzy", "flakey", "flat", "flavorful", "fresh", "fried", "fruity", "garlicky", "gelatinous", "gingery", "glazed", "grainy", "greasy", "gooey", "gritty", "harsh", "hearty", "heavy", "herbal", "hot", "icy", "infused", "juicy", "lean", "light", "lemony", "malty", "mashed", "meaty", "mellow", "mild", "minty", "moist", "mushy", "nutty", "oily", "oniony", "overripe", "pasty", "peppery", "pickled", "plain", "powdery", "raw", "refreshing", "rich", "ripe", "roasted", "robust", "rubbery", "runny", "salty", "sauteed", "savory", "seared", "seasoned", "sharp", "silky", "slimy", "smokey", "smothered", "smooth", "soggy", "soupy", "sour", "spicy", "spongy", "stale", "sticky", "stale", "stringy", "strong", "sugary", "sweet", "syrupy", "tangy", "tart", "tasteless", "tender", "toasted", "tough", "unflavored", "unseasoned", "velvety", "vinegary", "watery", "whipped", "woody", "yeasty", "zesty", "zingy", "amazing", "appealing", "appetizing", "delectable", "delicious", "delightful", "divine", "enjoyable", "enticing", "excellent", "exquisite", "extraordinary", "fantastic", "heavenly", "luscious", "marvelous", "mouthwatering", "palatable", "pleasant", "pleasing", "satisfying", "scrumptious", "superb", "tantalizing", "tasty", "terrific", "wonderful", "yummy" };
			string sTaste = sTastes[Utility.RandomMinMax(0, (sTastes.Length - 1))];

			string[] sFoods = new string[] { "biscuits", "bread", "bagels", "rolls", "buns", "muffins", "brownies", "cakes", "cookies", "crackers", "custards", "pastries", "pies", "roasts", "tarts" };
			string sFood = sFoods[Utility.RandomMinMax(0, (sFoods.Length - 1))];

			string[] sWeirds = new string[] { "ant", "ape", "baboon", "badger", "basilisk", "bear", "beaver", "beetle", "beholder", "boar", "brownie", "buffalo", "bull", "camel", "centaur", "centipede", "chimera", "cockatrice", "crocodile", "deer", "demon", "devil", "dinosaur", "djinni", "dog", "dragon", "dryad", "dwarf", "eagle", "efreet", "elemental", "elephant", "elf", "ettin", "frog", "fungi", "gargoyle", "ghast", "ghost", "ghoul", "giant", "gnoll", "gnome", "goat", "goblin", "golem", "gorgon", "griffon", "hag", "halfling", "harpy", "hell hound", "hippogriff", "hippopotamus", "hobgoblin", "horse", "hydra", "hyena", "imp", "jackal", "jaguar", "ki-rin", "kobold", "leopard", "leprechaun", "lich", "lion", "lizard", "lizardman", "lycanthrope", "lynx", "mammoth", "manticore", "mastodon", "medusa", "minotaur", "mule", "mummy", "naga", "nightmare", "ogre", "orc", "owl", "pegasus", "pixie", "porcupine", "ram", "rat", "reaper", "rhinoceros", "roc", "satyr", "scorpion", "serpent", "shadow", "skeleton", "skunk", "snake", "spectre", "sphinx", "spider", "sprite", "stag", "tiger", "titan", "toad", "troglodyte", "troll", "unicorn", "vampire", "weasel", "wight", "wisp", "wolf", "wolverine", "worm", "wraith", "wyvern", "yeti", "zombie", "zorn" };
			string sWeird = sWeirds[Utility.RandomMinMax(0, (sWeirds.Length - 1))];

			if (Utility.RandomMinMax(1, 3) > 1) { sWeird = ""; } else { sWeird = " " + sWeird; }

			task = task + " of " + sTaste + sWeird + " " + sFood;

			return task;
		}

		protected override string GetDescription(OrderContext order)
		{
			var description = string.Format("Craft {0}", order.MaxAmount);

			description = string.Format("{0} {1}", description, order.ItemName);

			return description;
		}

		protected override ShoppeGump GetGump(PlayerMobile from)
		{
			var context = GetOrCreateContext(from);
			PrepareOrders(context);

			return new ShoppeGump(
				from,
				this,
				context,
				"BAKER WORK SHOPPE",
				"Culinary Sets",
				"Dough"
			);
		}

		protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
		{
			base.OnJobFailed(from, context, customer);

			from.SendSound(0x054); // Torch1
		}

		protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
		{
			base.OnJobSuccess(from, context, customer);

			from.SendSound(0x054); // Torch1
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}
	}
}