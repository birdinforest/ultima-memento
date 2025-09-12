namespace Server.Temptation
{
	public class PlayerContext
	{
		public static readonly PlayerContext Default = new PlayerContext();

		public PlayerContext()
		{
		}

		public PlayerContext(GenericReader reader)
		{
			int version = reader.ReadInt();

			Flags = (TemptationFlags)reader.ReadInt();
		}

		public TemptationFlags Flags { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasPermanentDeath
		{
			get { return GetFlag(TemptationFlags.Deathwish); }
			set { SetFlag(TemptationFlags.Deathwish, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsBerserk
		{
			get { return GetFlag(TemptationFlags.I_can_take_it); }
			set { SetFlag(TemptationFlags.I_can_take_it, value); }
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write((int)Flags);
		}

		private bool GetFlag(TemptationFlags flag)
		{
			return (Flags & flag) != 0;
		}

		private void SetFlag(TemptationFlags flag, bool value)
		{
			if (value)
				Flags |= flag;
			else
				Flags &= ~flag;
		}
	}
}