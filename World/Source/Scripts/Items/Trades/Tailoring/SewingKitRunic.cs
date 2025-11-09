using Server.Engines.Craft;

namespace Server.Items
{
	public class SewingKitRunicI : BaseTool, IRunicTool
	{
		[Constructable]
		public SewingKitRunicI() : this(50)
		{
		}

		[Constructable]
		public SewingKitRunicI(int uses) : base(uses, 0x4C81)
		{
			Name = "runic sewing kit I";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public SewingKitRunicI(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefTailoring.CraftSystem; } }

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

	public class SewingKitRunicII : BaseTool, IRunicTool
	{
		[Constructable]
		public SewingKitRunicII() : this(50)
		{
		}

		[Constructable]
		public SewingKitRunicII(int uses) : base(uses, 0x4C81)
		{
			Name = "runic sewing kit II";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public SewingKitRunicII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefTailoring.CraftSystem; } }

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

	public class SewingKitRunicIII : BaseTool, IRunicTool
	{
		[Constructable]
		public SewingKitRunicIII() : this(50)
		{
		}

		[Constructable]
		public SewingKitRunicIII(int uses) : base(uses, 0x4C81)
		{
			Name = "runic sewing kit III";
			Weight = 1.0;
			Layer = Layer.OneHanded;
		}

		public SewingKitRunicIII(Serial serial) : base(serial)
		{
		}

		public override CraftSystem CraftSystem
		{ get { return DefTailoring.CraftSystem; } }

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