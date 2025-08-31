using Server.Engines.Craft;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.GlobalShoppe
{
	[Flipable(0x3D03, 0x3D04)]
	public class TinkerShoppe : CustomerOrderShoppe<TinkerOrderContext>
	{
		[Constructable]
		public TinkerShoppe() : base(0x3D03)
		{
			Name = "Tinker Work Shoppe";
		}

		public TinkerShoppe(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem { get { return DefTinkering.CraftSystem; } }

		public override NpcGuild Guild { get { return NpcGuild.TinkersGuild; } }

		protected override SkillName PrimarySkill { get { return SkillName.Tinkering; } }

		protected override ShoppeType ShoppeType { get { return ShoppeType.Tinker; } }

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is TinkerTools)
				return AddTools(from, dropped);
			if (dropped.ItemID >= 0x1BE3 && dropped.ItemID <= 0x1BFA)
				return AddResource(from, dropped);

			return base.OnDragDrop(from, dropped);
		}

		public override void PrepareOrders(TradeSkillContext context)
		{
			context.Orders.ForEach(untypedOrder =>
			{
				var order = untypedOrder as TinkerOrderContext;
				if (order == null)
				{
					Console.WriteLine("Failed to set Tinker rewards for order ({0})", untypedOrder.GetType().Name);
					return;
				}

				if (order.IsInitialized) return;

				var rewards = TinkerRewardCalculator.Instance;
				rewards.SetRewards(context, order);

				var item = ShoppeItemCache.GetOrCreate(order.Type);
				order.GraphicId = item.ItemID;
				order.ItemName = item.Name;
				order.Person = CreatePersonName();

				order.IsInitialized = true;
			});
		}

		protected override IEnumerable<TinkerOrderContext> CreateOrders(CraftSystem craftSystem, Mobile from, TradeSkillContext context, int count)
		{
			if (count < 1) yield break;

			// Build item list
			var items = GetCraftItems(from, craftSystem)
				.Where(i => TypeUtilities.IsAnyTypeOrDerived(
					i.ItemType,
					typeof(JewelryBracelet),
					typeof(JewelryCirclet),
					typeof(JewelryEarrings),
					typeof(JewelryNecklace),
					typeof(JewelryRing)
				))
				.ToList();
			if (items.Count < 1) yield break;

			// Build resource list
			var resources = new List<CraftResource>();
			foreach (var o in craftSystem.CraftSubRes)
			{
				var res = o as CraftSubRes;
				if (res == null || from.Skills[craftSystem.MainSkill].Value < res.RequiredSkill) continue;
				if (res.ItemType == typeof(IronIngot)) continue; // Always an option

				CraftResource resource;
				if (res.ItemType == typeof(DullCopperIngot)) resource = CraftResource.DullCopper;
				else if (res.ItemType == typeof(ShadowIronIngot)) resource = CraftResource.ShadowIron;
				else if (res.ItemType == typeof(CopperIngot)) resource = CraftResource.Copper;
				else if (res.ItemType == typeof(BronzeIngot)) resource = CraftResource.Bronze;
				else if (res.ItemType == typeof(GoldIngot)) resource = CraftResource.Gold;
				else if (res.ItemType == typeof(AgapiteIngot)) resource = CraftResource.Agapite;
				else if (res.ItemType == typeof(VeriteIngot)) resource = CraftResource.Verite;
				else if (res.ItemType == typeof(ValoriteIngot)) resource = CraftResource.Valorite;
				else if (res.ItemType == typeof(NepturiteIngot)) resource = CraftResource.Nepturite;
				else if (res.ItemType == typeof(ObsidianIngot)) resource = CraftResource.Obsidian;
				else if (res.ItemType == typeof(SteelIngot)) resource = CraftResource.Steel;
				else if (res.ItemType == typeof(BrassIngot)) resource = CraftResource.Brass;
				else if (res.ItemType == typeof(MithrilIngot)) resource = CraftResource.Mithril;
				else if (res.ItemType == typeof(XormiteIngot)) resource = CraftResource.Xormite;
				else if (res.ItemType == typeof(DwarvenIngot)) resource = CraftResource.Dwarven;
				else continue;

				resources.Add(resource);
			}

			// Add quantity bonus for every 5 points over 100
			var amountBonus = (int)(Math.Max(0, from.Skills[craftSystem.MainSkill].Value - 100) / 5);

			// Stop with Basic Resources once GM
			var disallowBasic = 0 < resources.Count && 100 <= from.Skills[craftSystem.MainSkill].Value;

			for (int i = 0; i < count; i++)
			{
				var item = Utility.Random(items);
				if (item == null) yield break;

				var gemType = BaseTrinket.GetGemType(item);
				if (gemType == GemType.Pearl && Utility.RandomBool()) gemType = GemType.None; // Reduce likelihood of Pearls

				var amount = gemType == GemType.Pearl
					? 1 // Pearls are very rare
					: amountBonus + Utility.RandomMinMax(13, 20);
				var resource = disallowBasic || (0 < resources.Count && Utility.RandomDouble() < 0.5) ? Utility.Random(resources) : CraftResource.None;
				if (resource == CraftResource.None && gemType == GemType.None) amount += 10; // Pump value by increasing count

				var order = new TinkerOrderContext(item.ItemType)
				{
					GemType = gemType,
					MaxAmount = amount,
					CurrentAmount = 0,
					Resource = resource,
				};

				yield return order;
			}
		}

		protected override string CreateTask(TradeSkillContext context)
		{
			string task = null;

			switch (Utility.RandomMinMax(1, 6))
			{
				case 1: task = "Repair"; break;
				case 2: task = "Fix"; break;
				case 3: task = "Enhance"; break;
				case 4: task = "Modify"; break;
				case 5: task = "Resize"; break;
				case 6: task = "Alter"; break;
			}

			string forr = "their";
			switch (Utility.RandomMinMax(0, 15))
			{
				case 0: forr = "their"; break;
				case 1: forr = "their friend's"; break;
				case 2: forr = "this"; break;
				case 3: forr = "their father's"; break;
				case 4: forr = "their mother's"; break;
				case 5: forr = "their brother's"; break;
				case 6: forr = "their sister's"; break;
				case 7: forr = "their uncle's"; break;
				case 8: forr = "their aunt's"; break;
				case 9: forr = "their cousin's"; break;
				case 10: forr = "their grandparent's"; break;
			}

			if (Utility.RandomMinMax(1, 3) == 1)
			{
				Item item = null;

				switch (Utility.RandomMinMax(1, 7))
				{
					case 1: item = new TrinketLantern(); break;
					case 2: item = new TrinketCandle(); break;
					case 3: item = new JewelryRing(); break;
					case 4: item = new JewelryNecklace(); break;
					case 5: item = new JewelryEarrings(); break;
					case 6: item = new JewelryBracelet(); break;
					case 7: item = new JewelryCirclet(); break;
				}

				bool evil = false;
				bool orient = false;

				switch (Utility.RandomMinMax(1, 8))
				{
					case 1: evil = true; break;
					case 2: orient = true; break;
				}

				string sAdjective = "unusual";
				string eAdjective = "might";

				sAdjective = RandomThings.MagicItemAdj("start", orient, evil, item.ItemID);
				eAdjective = RandomThings.MagicItemAdj("end", orient, evil, item.ItemID);

				string name = "item";
				string xName = ContainerFunctions.GetOwner("property");

				if (item.Name != null && item.Name != "") { name = item.Name.ToLower(); }
				if (name == "item") { item.SyncName(); name = (item.Name).ToLower(); }

				switch (Utility.RandomMinMax(0, 5))
				{
					case 0: name = sAdjective + " " + name + " of " + xName; break;
					case 1: name = name + " of " + xName; break;
					case 2: name = sAdjective + " " + name; break;
					case 3: name = sAdjective + " " + name + " of " + xName; break;
					case 4: name = name + " of " + xName; break;
					case 5: name = sAdjective + " " + name; break;
				}

				item.Delete();

				task = task + " " + forr + " " + name;
			}
			else
			{
				string[] sTinks = new string[] { " jointing plane ", " moulding plane ", " smoothing plane ", " clock ", " axle ", " rolling pin ", " scissors ", " mortar pestle ", " scorp ", " draw knife ", " sewing kit ", " druid's cauldron ", " saw ", " dovetail saw ", " froe ", " shovel ", " hammer ", " tongs ", " inshave ", " pickaxe ", " lockpick ", " skillet ", " flour sifter ", " bowcrafting tools ", " mapmakers pen ", " scribes pen ", " skinning knife ", " witch's cauldron ", " waxing pot ", " sextant ", " spoon ", " plate ", " forkleft ", " cleaver ", " knife ", " goblet ", " mug ", " candle ", " scales ", " key ", " key ring ", " globe ", " spyglass ", " lantern ", " heating stand ", " amulet ", " bracelet ", " ring ", " earrings ", " potion keg " };
				string sTink = sTinks[Utility.RandomMinMax(0, (sTinks.Length - 1))];

				task = task + " " + forr + sTink;
			}

			return task;
		}

		protected override string GetDescription(TinkerOrderContext order)
		{
			var description = string.Format("Craft {0}", order.MaxAmount);
			if (order.RequireExceptional) description += " exceptional";
			if (order.Resource != CraftResource.None) description = string.Format("{0} {1}", description, CraftResources.GetResourceName(order.Resource));

			string gemName = null;
			switch (order.GemType)
			{
				case GemType.StarSapphire:
					gemName = "star sapphire";
					break;

				case GemType.Emerald:
				case GemType.Sapphire:
				case GemType.Ruby:
				case GemType.Citrine:
				case GemType.Amethyst:
				case GemType.Tourmaline:
				case GemType.Amber:
				case GemType.Diamond:
				case GemType.Pearl:
					gemName = order.GemType.ToString().ToLower();
					break;

				case GemType.None:
				default:
					break;
			}

			if (gemName != null) description = string.Format("{0} {1}", description, gemName);

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
				"TINKER WORK SHOPPE",
				"Tinker Tools",
				"Ingots"
			);
		}

		protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
		{
			base.OnJobFailed(from, context, customer);

			from.SendSound(0x542); // Tinker
		}

		protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
		{
			base.OnJobSuccess(from, context, customer);

			from.SendSound(0x542); // Tinker
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