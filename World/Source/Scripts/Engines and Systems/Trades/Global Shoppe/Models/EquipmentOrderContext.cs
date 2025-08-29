using System;

namespace Server.Engines.GlobalShoppe
{
	[PropertyObject]
	public class EquipmentOrderContext : IOrderContext, IResourceItem, IExceptionalItem
	{
		public EquipmentOrderContext(Type type)
		{
			Type = type;
		}

		public EquipmentOrderContext(GenericReader reader)
		{
			int version = reader.ReadInt();

			var typeName = reader.ReadString();
			if (!string.IsNullOrWhiteSpace(typeName)) Type = ScriptCompiler.FindTypeByName(typeName);

			RequireExceptional = reader.ReadBool();
			Resource = (CraftResource)reader.ReadInt();
			MaxAmount = reader.ReadInt();
			CurrentAmount = reader.ReadInt();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int CurrentAmount { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int GoldReward { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int GraphicId { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsComplete
		{ get { return IsValid && MaxAmount <= CurrentAmount; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsInitialized { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsValid
		{ get { return Type != null; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public string ItemName { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxAmount { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Person { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointReward { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ReputationReward { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RequireExceptional { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Type Type { get; private set; }

		public void Serialize(GenericWriter writer)
		{
			writer.Write((int)0); // version

			var typeName = IsValid ? Type.Name : null;
			writer.Write(typeName);
			writer.Write(RequireExceptional);
			writer.Write((int)Resource);
			writer.Write(MaxAmount);
			writer.Write(CurrentAmount);
		}
	}
}