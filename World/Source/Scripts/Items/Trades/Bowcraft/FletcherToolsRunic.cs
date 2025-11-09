using Server.Engines.Craft;

namespace Server.Items
{
	public class FletcherToolsRunicI : BaseTool, IRunicTool
	{
		[Constructable]
		public FletcherToolsRunicI() : this(50)
		{
		}

		[Constructable]
		public FletcherToolsRunicI(int uses) : base(uses, 0x66F9)
		{
			Name = "runic bowcrafting tools I";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public FletcherToolsRunicI(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBowFletching.CraftSystem; } }

		public override Catalogs DefaultCatalog
		{ get { return Catalogs.None; } }

		public override string DefaultDescription
		{ get { return "This tool can be used to create slightly magical items."; } }

		public int RunicMinAttributes { get { return 1; } }
		public int RunicMaxAttributes { get { return 1; } }
		public int RunicMinIntensity { get { return 40; } }
		public int RunicMaxIntensity { get { return 40; } }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}

	public class FletcherToolsRunicII : BaseTool, IRunicTool
	{
		[Constructable]
		public FletcherToolsRunicII() : this(50)
		{
		}

		[Constructable]
		public FletcherToolsRunicII(int uses) : base(uses, 0x66F9)
		{
			Name = "runic bowcrafting tools II";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public FletcherToolsRunicII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBowFletching.CraftSystem; } }

		public override Catalogs DefaultCatalog
		{ get { return Catalogs.None; } }

		public override string DefaultDescription
		{ get { return "This tool can be used to create moderately magical items."; } }

		public int RunicMinAttributes { get { return 2; } }
		public int RunicMaxAttributes { get { return 2; } }
		public int RunicMinIntensity { get { return 40; } }
		public int RunicMaxIntensity { get { return 50; } }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}

	public class FletcherToolsRunicIII : BaseTool, IRunicTool
	{
		[Constructable]
		public FletcherToolsRunicIII() : this(50)
		{
		}

		[Constructable]
		public FletcherToolsRunicIII(int uses) : base(uses, 0x66F9)
		{
			Name = "runic bowcrafting tools III";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public FletcherToolsRunicIII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBowFletching.CraftSystem; } }

		public override Catalogs DefaultCatalog
		{ get { return Catalogs.None; } }

		public override string DefaultDescription
		{ get { return "This tool can be used to create very magical items."; } }

		public int RunicMinAttributes { get { return 3; } }
		public int RunicMaxAttributes { get { return 3; } }
		public int RunicMinIntensity { get { return 70; } }
		public int RunicMaxIntensity { get { return 70; } }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0); // version
		}
	}
}