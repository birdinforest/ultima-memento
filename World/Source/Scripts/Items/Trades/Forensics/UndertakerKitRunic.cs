using Server.Engines.Craft;

namespace Server.Items
{
	public class UndertakerKitRunicI : BaseTool, IRunicTool
	{
		[Constructable]
		public UndertakerKitRunicI() : this(50)
		{
		}

		[Constructable]
		public UndertakerKitRunicI(int uses) : base(uses, 0x661B)
		{
			Name = "runic undertaker kit I";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public UndertakerKitRunicI(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBonecrafting.CraftSystem; } }

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

	public class UndertakerKitRunicII : BaseTool, IRunicTool
	{
		[Constructable]
		public UndertakerKitRunicII() : this(50)
		{
		}

		[Constructable]
		public UndertakerKitRunicII(int uses) : base(uses, 0x661B)
		{
			Name = "runic undertaker kit II";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public UndertakerKitRunicII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBonecrafting.CraftSystem; } }

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

	public class UndertakerKitRunicIII : BaseTool, IRunicTool
	{
		[Constructable]
		public UndertakerKitRunicIII() : this(50)
		{
		}

		[Constructable]
		public UndertakerKitRunicIII(int uses) : base(uses, 0x661B)
		{
			Name = "runic undertaker kit III";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public UndertakerKitRunicIII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefBonecrafting.CraftSystem; } }

		public override Catalogs DefaultCatalog
		{ get { return Catalogs.None; } }

		public override string DefaultDescription
		{ get { return "This tool can be used to create very magical items."; } }

		public int RunicMinAttributes { get { return 3; } }
		public int RunicMaxAttributes { get { return 3; } }
		public int RunicMinIntensity { get { return 50; } }
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
}