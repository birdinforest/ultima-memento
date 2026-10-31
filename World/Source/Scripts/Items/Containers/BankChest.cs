using System.Collections.Generic;
using Server.ContextMenus;
using Server.Gumps;
using Server.Multis;

namespace Server.Items
{
	[Flipable(0x436, 0x437)]
	public class BankChest : Item, ISecurable
	{
		public SecureLevel m_Level;

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level
		{ get { return m_Level; } set { m_Level = value; } }

		[Constructable]
		public BankChest() : base(0x436)
		{
			Name = "Home Bank Vault";
			Weight = 50.0;
			m_Level = SecureLevel.Anyone;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Movable)
			{
				from.SendMessage("This must be secured down in a home to use.");
			}
			else if (!from.InRange(GetWorldLocation(), 2) || !from.CanSee(this) || !from.InLOS(this))
			{
				from.SendMessage("You will have to get closer to use that.");
			}
			else if (!CheckAccess(from))
			{
				from.SendMessage("You cannot use this safe.");
			}
			else
			{
				BankBox box = from.BankBox;
				if (box != null)
				{
					box.Open();
				}
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public bool CheckAccess(Mobile m)
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (house != null && (house.Public ? house.IsBanned(m) : !house.HasAccess(m)))
				return false;

			return (house != null && house.HasSecureAccess(m, m_Level));
		}

		public BankChest(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)1); // version
			writer.Write((int)m_Level);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			if (version < 1 && Name == "Bank Vault") Name = "Home Bank Vault";
			m_Level = version < 1 ? SecureLevel.Anyone : (SecureLevel)reader.ReadInt();
		}
	}
}