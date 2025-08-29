using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.Craft;
using Server.Items;
using Server.Mobiles;
using Server.Utilities;

namespace Server.Engines.GlobalShoppe
{
	[Flipable(0x3CE1, 0x3CE2)]
	public class AlchemistShoppe : CustomerOrderShoppe<OrderContext>
	{
		[Constructable]
		public AlchemistShoppe() : base(0x3CE1)
		{
			Name = "Alchemist Work Shoppe";
		}

		public AlchemistShoppe(Serial serial) : base(serial)
		{
		}

		public override NpcGuild Guild { get { return NpcGuild.AlchemistsGuild; } }

		protected override SkillName PrimarySkill { get { return SkillName.Alchemy; } }
		protected override ShoppeType ShoppeType { get { return ShoppeType.Alchemist; } }

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is MortarPestle || dropped is GodBrewing)
				return AddTools(from, dropped);
			if (dropped.Catalog == Catalogs.Reagent)
				return AddResource(from, dropped);

			return base.OnDragDrop(from, dropped);
		}

		protected override string CreateTask(TradeSkillContext context)
		{
			string task;
			switch (Utility.RandomMinMax(1, 4))
			{
				default:
				case 1: task = "Brew"; break;
				case 2: task = "Create"; break;
				case 3: task = "Concoct"; break;
				case 4: task = "Boil"; break;
			}

			switch (Utility.RandomMinMax(1, 5))
			{
				case 1: task += " an elixir"; break;
				case 2: task += " a potion"; break;
				case 3: task += " a draught"; break;
				case 4: task += " a mixture"; break;
				case 5: task += " a philter"; break;
			}

			switch (Utility.RandomMinMax(1, 4))
			{
				case 1: task += " with "; break;
				case 2: task += " using "; break;
				case 3: task += " mixing "; break;
				case 4: task += " combining "; break;
			}

			string sWord;
			if (Utility.RandomMinMax(1, 4) > 1)
			{
				string[] sWord1 = new string[] { "ant", "animal", "bat", "bear", "beetle", "boar", "brownie", "bugbear", "basilisk", "bull", "froglok", "cat", "centaur", "chimera", "cow", "crocodile", "cyclops", "dark elf", "demon", "devil", "doppelganger", "dragon", "drake", "dryad", "dwarf", "elf", "ettin", "frog", "gargoyle", "ghoul", "giant", "gnoll", "gnome", "goblin", "gorilla", "gremlin", "griffin", "hag", "hobbit", "harpy", "hippogriff", "hobgoblin", "horse", "hydra", "imp", "kobold", "kraken", "leprechaun", "lizard", "lizard man", "medusa", "human", "minotaur", "mouse", "naga", "nightmare", "nixie", "ogre", "orc", "pixie", "pegasus", "phoenix", "giant lizard", "rat", "giant snake", "satyr", "scorpion", "serpent", "shark", "snake", "sphinx", "giant spider", "spider", "sylvan", "sprite", "succubus", "sylvan", "titan", "toad", "troglodite", "troll", "unicorn", "vampire", "weasel", "werebear", "wererat", "werewolf", "werecat", "wolf", "worm", "wyrm", "wyvern", "yeti", "zombie" };
				string sName1 = sWord1[Utility.RandomMinMax(0, sWord1.Length - 1)];
				string[] sWord2 = new string[] { "bile", "blood", "bone dust", "essence", "extract", "eyes", "hair/skin", "herbs", "juice", "oil", "powder", "salt", "sauce", "scent", "serum", "spice", "spit", "tears", "teeth", "urine" };
				string sName2 = sWord2[Utility.RandomMinMax(0, sWord2.Length - 1)];
				sWord = sName1 + " " + sName2;
			}
			else
			{
				string[] sWords = new string[] { "ants", "slime", "bat whiskers", "bees", "black cat hair", "black salt", "bloodworms", "cat whiskers", "centipedes", "coffin shavings", "crystal moonbeams", "cyclops eyelashes", "dragon scales", "efreet dust", "elemental dust", "eye of newt", "fairy dust", "fairy wings", "fire giant ash", "gelatinous goo", "genie smoke", "ghoul skin flakes", "graveyard dirt", "slime", "hell hound ash", "leeches", "lich dust", "love honey", "mosquitoes", "mummy spice", "mystic dust", "ochre jelly", "phoenix ash", "pixie dust", "pixie wings", "ritual powder", "sea serpent salt", "serpent scales", "snake scales", "sorcerer sand", "sprite wings", "tree leaves", "tree root", "tree sap", "vampire ash", "viper essence", "wasps", "wisp dust", "witch hazel", "worms", "zombie flesh" };
				sWord = sWords[Utility.RandomMinMax(0, sWords.Length - 1)];
			}

			string[] sTypes = new string[] { "black pearl", "bloodmoss", "garlic", "ginseng", "mandrake root", "nightshade", "spider silk", "sulfurous ash", "bat wing", "grave dust", "daemon blood", "pig iron", "nox crystal", "silver serpent venom", "dragon blood", "enchanted seaweed", "dragon teeth", "golden serpent venom", "lich dust", "demon claws", "unicorn horns", "demigod blood", "ghostly dust", "eyes of toads", "fairy eggs", "gargoyle ears", "beetle shells", "moon crystals", "pixie skulls", "red lotus", "sea salt", "silver widows", "swamp berries", "brimstone", "butterfly wings", "bitter root", "black sand", "blood rose", "dried toad", "maggot", "mummy wrap", "violet fungus", "werewolf claw", "wolfsbane" };
			string sType = sTypes[Utility.RandomMinMax(0, sTypes.Length - 1)];

			switch (Utility.RandomMinMax(0, 3))
			{
				case 0: task = task + sWord + " and " + sType + " into a vial of "; break;
				case 1: task = task + sWord + " and " + sType + " into a bottle of "; break;
				case 2: task = task + sWord + " and " + sType + " into a flask of "; break;
				case 3: task = task + sWord + " and " + sType + " into a jar of "; break;
			}

			string[] sMixs = new string[] { "Acidic", "Summoning", "Scrying", "Obscure", "Iron", "Ghoulish", "Enfeebling", "Altered", "Secret", "Obscuring", "Irresistible", "Gibbering", "Enlarged", "Confusing", "Analyzing", "Sympathetic", "Secure", "Permanent", "Keen", "Glittering", "Ethereal", "Contacting", "Animal", "Telekinetic", "Seeming", "Persistent", "Lawful", "Evil", "Continual", "Animated", "Telepathic", "Shadow", "Phantasmal", "Legendary", "Good", "Expeditious", "Control", "Antimagic", "Teleporting", "Shattering", "Phantom", "Lesser", "Grasping", "Explosive", "Crushing", "Arcane", "Temporal", "Shocking", "Phasing", "Levitating", "Greater", "Fabricated", "Cursed", "Articulated", "Tiny", "Shouting", "Planar", "Limited", "Guarding", "Faithful", "Dancing", "Binding", "Transmuting", "Shrinking", "Poisonous", "Lucubrating", "Fearful", "Dazzling", "Black", "Undead", "Silent", "Polymorphing", "Magical", "Hallucinatory", "Delayed", "Blinding", "Undetectable", "Slow", "Prismatic", "Magnificent", "Hideous", "Fire", "Demanding", "Blinking", "Unseen", "Solid", "Programmed", "Major", "Holding", "Flaming", "Dimensional", "Vampiric", "Soul", "Projected", "Mass", "Horrid", "Discern", "Burning", "Vanishing", "Spectral", "Mending", "Hypnotic", "Floating", "Disintegrating", "Cat", "Protective", "Mind", "Ice", "Flying", "Disruptive", "Chain", "Spidery", "Prying", "Minor", "Illusionary", "Force", "Dominating", "Changing", "Warding", "Stinking", "Pyrotechnic", "Mirrored", "Improved", "Forceful", "Dreaming", "Chaotic", "Water", "Stone", "Rainbow", "Misdirected", "Incendiary", "Freezing", "Elemental", "Charming", "Watery", "Misleading", "Instant", "Gaseous", "Emotional", "Chilling", "Weird", "Storming", "Resilient", "Mnemonic", "Interposing", "Gentle", "Enduring", "Whispering", "Suggestive", "Reverse", "Moving", "Invisible", "Ghostly", "Energy", "Clenched", "Climbing", "Comprehending", "Colorful", "True", "False" };
			string sMix = sMixs[Utility.RandomMinMax(0, sMixs.Length - 1)];

			string[] sEffects = new string[] { "Acid", "Tentacles", "Sigil", "Plane", "Legend", "Gravity", "Emotion", "Chest", "Alarm", "Terrain", "Simulacrum", "Poison", "Lightning", "Grease", "Endurance", "Circle", "Anchor", "Thoughts", "Skin", "Polymorph", "Lights", "Growth", "Enervation", "Clairvoyance", "Animal", "Time", "Sleep", "Prestidigitation", "Location", "Guards", "Enfeeblement", "Clone", "Antipathy", "Tongues", "Soul", "Projection", "Lock", "Hand", "Enhancer", "Cloud", "Arcana", "Touch", "Sound", "Pyrotechnics", "Lore", "Haste", "Etherealness", "Cold", "Armor", "Transformation", "Spells", "Refuge", "Lucubration", "Hat", "Evil", "Color", "Arrows", "Trap", "Sphere", "Repulsion", "Magic", "Hound", "Evocation", "Confusion", "Aura", "Trick", "Spider", "Resistance", "Mansion", "Hypnotism", "Eye", "Conjuration", "Banishment", "Turning", "Spray", "Retreat", "Mask", "Ice", "Fall", "Contagion", "Banshee", "Undead", "Stasis", "Rope", "Maze", "Image", "Fear", "Creation", "Bear", "Vanish", "Statue", "Runes", "Message", "Imprisonment", "Feather", "Curse", "Binding", "Veil", "Steed", "Scare", "Meteor", "Insanity", "Field", "Dance", "Vision", "Stone", "Screen", "Mind", "Invisibility", "Fireball", "Darkness", "Blindness", "Vocation", "Storm", "Script", "Mirage", "Invulnerability", "Flame", "Daylight", "Blink", "Wail", "Strength", "Scrying", "Misdirection", "Iron", "Flesh", "Dead", "Blur", "Walk", "Strike", "Seeing", "Missile", "Item", "Fog", "Deafness", "Body", "Wall", "Stun", "Self", "Mist", "Jar", "Force", "Death", "Bolt", "Wards", "Suggestion", "Sending", "Monster", "Jaunt", "Foresight", "Demand", "Bond", "Water", "Summons", "Servant", "Mouth", "Jump", "Form", "Disjunction", "Breathing", "Weapon", "Sunburst", "Shadow", "Mud", "Kill", "Freedom", "Disk", "Burning", "Weather", "Swarm", "Shape", "Nightmare", "Killer", "Frost", "Dismissal", "Cage", "Web", "Symbol", "Shelter", "Object", "Knock", "Gate", "Displacement", "Chain", "Wilting", "Sympathy", "Shield", "Page", "Languages", "Good", "Door", "Chaos", "Wind", "Telekinesis", "Shift", "Pattern", "Laughter", "Grace", "Drain", "Charm", "Wish", "Teleport", "Shout", "Person", "Law", "Grasp", "Dream", "Elements", "Edge", "Earth", "Dust" };
			string sEffect = sEffects[Utility.RandomMinMax(0, sEffects.Length - 1)];

			task = task + sMix + " " + sEffect;

			return task;
		}

		protected override ShoppeGump GetGump(PlayerMobile from)
		{
			var context = GetOrCreateContext(from);

			// Ensure Orders are configured
			context.Orders.ForEach(untypedOrder =>
			{
				var order = untypedOrder as OrderContext;
				if (order == null)
				{
					Console.WriteLine("Failed to set Alchemy rewards for order ({0})", untypedOrder.GetType().Name);
					return;
				}

				if (order.IsInitialized) return;

				var rewards = AlchemyRewardCalculator.Instance;
				rewards.SetRewards(context, order);

				var item = ShoppeItemCache.GetOrCreate(order.Type);
				order.GraphicId = item.ItemID;
				order.ItemName = item.Name;
				order.Person = CreatePersonName();

				order.IsInitialized = true;
			});

			return new ShoppeGump(
				from,
				this,
				context,
				"ALCHEMIST WORK SHOPPE",
				"Alchemy Sets",
				"Reagents"
			);
		}

		protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
		{
			base.OnJobFailed(from, context, customer);

			from.SendSound(0x240); // Liquid
		}

		protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
		{
			base.OnJobSuccess(from, context, customer);

			from.SendSound(0x240); // Liquid
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

		protected override IEnumerable<OrderContext> CreateOrders(Mobile from, TradeSkillContext context, int count)
		{
			if (count < 1) yield break;

			var craftSystem = DefAlchemy.CraftSystem;

			// Build item list
			var items = GetCraftItems(from, craftSystem)
				.Where(i => TypeUtilities.IsTypeOrDerived<BaseElixir>(i.ItemType))
				.ToList();
			if (items.Count < 1) yield break;

			// Add 2x quantity bonus for every 5 points over 100
			var amountBonus = 2 * (int)(Math.Max(0, from.Skills[craftSystem.MainSkill].Value - 100) / 5);

			for (int i = 0; i < count; i++)
			{
				var item = Utility.Random(items);
				if (item == null) yield break;

				var amount = amountBonus + Utility.RandomMinMax(6, 20);

				var order = new OrderContext(item.ItemType)
				{
					MaxAmount = amount,
					CurrentAmount = 0,
				};

				yield return order;
			}
		}

		protected override string GetDescription(OrderContext order)
		{
			var description = string.Format("Craft {0}", order.MaxAmount);

			description = string.Format("{0} {1}", description, order.ItemName);

			return description;
		}
	}
}