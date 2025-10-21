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
	[Flipable(0x3CFB, 0x3CFC)]
	public class CarpentryShoppe : BasicCustomerOrderShoppe
	{
		[Constructable]
		public CarpentryShoppe() : base(0x3CFB)
		{
			Name = "Carpentry Work Shoppe";
		}

		public CarpentryShoppe(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem { get { return DefCarpentry.CraftSystem; } }
		public override NpcGuild Guild { get { return NpcGuild.CarpentersGuild; } }
		public override ShoppeType ShoppeType { get { return ShoppeType.Carpentry; } }

		protected override SkillName PrimarySkill { get { return SkillName.Carpentry; } }

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is CarpenterTools)
				return AddTools(from, dropped);
			if (dropped is BaseWoodBoard)
				return AddResource(from, dropped);

			return base.OnDragDrop(from, dropped);
		}

		public override void PrepareOrders(TradeSkillContext context)
		{
			context.Orders.ForEach(untypedOrder =>
			{
				var order = untypedOrder as EquipmentOrderContext;
				if (order == null)
				{
					Console.WriteLine("Failed to set Carpentry rewards for order ({0})", untypedOrder.GetType().Name);
					return;
				}

				if (order.IsInitialized) return;

				var rewards = CarpenterRewardCalculator.Instance;
				rewards.SetRewards(context, order);

				var item = ShoppeItemCache.GetOrCreate(order.Type);
				order.GraphicId = item.ItemID;
				order.ItemName = item.Name;
				order.Person = CreatePersonName();

				order.IsInitialized = true;
			});
		}

		protected override IEnumerable<EquipmentOrderContext> CreateOrders(CraftSystem craftSystem, Mobile from, TradeSkillContext context, int count)
		{
			if (count < 1) yield break;

			// Build item list
			var items = GetCraftItems(from, craftSystem)
				.Where(i => TypeUtilities.IsExceptionalEquipmentType(i.ItemType))
				.ToList();
			if (items.Count < 1) yield break;

			// Build resource list
			var resources = new List<CraftResource>();
			foreach (var o in craftSystem.CraftSubRes)
			{
				var res = o as CraftSubRes;
				if (res == null || from.Skills[craftSystem.MainSkill].Value < res.RequiredSkill) continue;
				if (res.ItemType == typeof(Board)) continue; // Always an option

				CraftResource resource;
				if (res.ItemType == typeof(AshBoard)) resource = CraftResource.AshTree;
				else if (res.ItemType == typeof(CherryBoard)) resource = CraftResource.CherryTree;
				else if (res.ItemType == typeof(EbonyBoard)) resource = CraftResource.EbonyTree;
				else if (res.ItemType == typeof(GoldenOakBoard)) resource = CraftResource.GoldenOakTree;
				else if (res.ItemType == typeof(HickoryBoard)) resource = CraftResource.HickoryTree;
				else if (res.ItemType == typeof(MahoganyBoard)) resource = CraftResource.MahoganyTree;
				else if (res.ItemType == typeof(OakBoard)) resource = CraftResource.OakTree;
				else if (res.ItemType == typeof(PineBoard)) resource = CraftResource.PineTree;
				else if (res.ItemType == typeof(GhostBoard)) resource = CraftResource.GhostTree;
				else if (res.ItemType == typeof(RosewoodBoard)) resource = CraftResource.RosewoodTree;
				else if (res.ItemType == typeof(WalnutBoard)) resource = CraftResource.WalnutTree;
				else if (res.ItemType == typeof(PetrifiedBoard)) resource = CraftResource.PetrifiedTree;
				else if (res.ItemType == typeof(DriftwoodBoard)) resource = CraftResource.DriftwoodTree;
				else if (res.ItemType == typeof(ElvenBoard)) resource = CraftResource.ElvenTree;
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

				var resource = disallowBasic || (0 < resources.Count && Utility.RandomDouble() < 0.5) ? Utility.Random(resources) : CraftResource.None;
				var amount = amountBonus + Utility.RandomMinMax(7, 15);
				if (resource == CraftResource.None) amount += 10; // Pump value by increasing count

				var order = new EquipmentOrderContext(item.ItemType)
				{
					RequireExceptional = Utility.RandomDouble() < 0.25,
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

			switch (Utility.RandomMinMax(1, 10))
			{
				case 1: task = "Repair"; break;
				case 2: task = "Fix"; break;
				case 3: task = "Sand"; break;
				case 4: task = "Modify"; break;
				case 5: task = "Polish"; break;
				case 6: task = "Engrave"; break;
				case 7: task = "Adjust"; break;
				case 8: task = "Improve"; break;
				case 9: task = "Oil"; break;
				case 10: task = "Refinish"; break;
			}

			if (Utility.RandomMinMax(1, 5) == 1)
			{
				Item item = null;

				switch (Utility.RandomMinMax(1, 14))
				{
					case 1: item = new WildStaff(); break;
					case 2: item = new ShepherdsCrook(); break;
					case 3: item = new QuarterStaff(); break;
					case 4: item = new GnarledStaff(); break;
					case 5: item = new WoodenShield(); break;
					case 6: item = new Bokuto(); break;
					case 7: item = new Fukiya(); break;
					case 8: item = new Tetsubo(); break;
					case 9: item = new WoodenPlateArms(); break;
					case 10: item = new WoodenPlateHelm(); break;
					case 11: item = new WoodenPlateGloves(); break;
					case 12: item = new WoodenPlateGorget(); break;
					case 13: item = new WoodenPlateLegs(); break;
					case 14: item = new WoodenPlateChest(); break;
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

				task = task + " their " + name;
			}
			else
			{
				string[] sWoods = new string[] { " ", " ash wood ", " cherry wood ", " ebony wood ", " golden oak ", " hickory ", " mahogany ", " oak ", " pine ", " ghost wood ", " rosewood ", " walnut wood ", " petrified wood ", " diftwood ", " elven wood " };
				string sWood = sWoods[Utility.RandomMinMax(0, (sWoods.Length - 1))];

				task = task + " their" + sWood;

				switch (Utility.RandomMinMax(1, 20))
				{
					case 1: task += "foot stool"; break;
					case 2: task += "stool"; break;
					case 3: task += "chair"; break;
					case 4: task += "bench"; break;
					case 5: task += "throne"; break;
					case 6: task += "nightstand"; break;
					case 7: task += "writing table"; break;
					case 8: task += "table"; break;
					case 9: task += "box"; break;
					case 10: task += "crate"; break;
					case 11: task += "chest"; break;
					case 12: task += "armoire"; break;
					case 13: task += "bookcase"; break;
					case 14: task += "shelf"; break;
					case 15: task += "drawers"; break;
					case 16: task += "foot locker"; break;
					case 17: task += "cabinet"; break;
					case 18: task += "barrel"; break;
					case 19: task += "tub"; break;
					case 20: task += "bed"; break;
				}
			}

			return task;
		}

		protected override ShoppeGump GetGump(PlayerMobile from)
		{
			var context = GetOrCreateContext(from);
			PrepareOrders(context);

			return new ShoppeGump(
				from,
				this,
				context,
				"CARPENTRY WORK SHOPPE",
				"Carpenter Tools",
				"Boards"
			);
		}

		protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
		{
			base.OnJobFailed(from, context, customer);

			from.SendSound(0x23D); // Carpentry
		}

		protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
		{
			base.OnJobSuccess(from, context, customer);

			from.SendSound(0x23D); // Carpentry
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