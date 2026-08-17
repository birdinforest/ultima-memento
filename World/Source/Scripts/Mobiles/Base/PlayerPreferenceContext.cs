using Server.Gumps;

namespace Server.Mobiles
{
	[PropertyObject]
	public class PlayerPreferenceContext
	{
		public PlayerPreferenceContext()
		{
			CharacterWepAbNames = true;
			ColorlessFabricBreakdown = true;
			ModernLockpickingEnabled = true;
			ModernRemoveTrapEnabled = true;
			ModernTrackingEnabled = true;
			VendorContainerSellCompactItemsPerPage = VendorContainerSellConfigGump.DefaultVendorContainerSellCompactItemsPerPage;
			VendorContainerSellLargeItemsPerPage = VendorContainerSellConfigGump.DefaultVendorContainerSellLargeItemsPerPage;
			VendorContainerSellSelectionBehavior = VendorContainerSellSelectionBehavior.AsManyAsPossible;
		}

		/// <summary>
		/// The version code (ShardInfo.VersionCode) of the MOTD the player last saw.
		/// Used to detect upgrades: if the server version has increased, the MOTD
		/// will re-appear even if the player had previously disabled "Show at Login".
		/// A value of 0 means the player has never seen a versioned MOTD.
		/// </summary>
		public int MotdLastSeenVersion { get; set; }

		public PlayerPreferenceContext(GenericReader reader)
		{
			int version = reader.ReadInt();

			DoubleClickID = reader.ReadBool();
			SuppressVendorTooltip = reader.ReadBool();
			SingleAttemptID = reader.ReadBool();
			ColorlessFabricBreakdown = reader.ReadBool();
			CharacterSheath = reader.ReadBool();
			CharacterWepAbNames = reader.ReadBool();
			CharMusical = reader.ReadString();
			WeaponBarOpen = reader.ReadBool();
			IgnoreVendorGoldSafeguard = reader.ReadBool();
			ClassicPoisoning = reader.ReadBool();

			if (1 < version)
			{
				CharacterBarbaric = reader.ReadInt();
				CharacterEvil = reader.ReadBool();
				CharacterLoot = reader.ReadString();
				CharacterOriental = reader.ReadBool();
				ShowGumpImages = version < 8 ? reader.ReadInt() == 1 : reader.ReadBool();
				MagerySpellHue = reader.ReadInt();
				MessageOfTheDay = reader.ReadBool();
				MusicPlaylist = reader.ReadString();
				MyChat = reader.ReadString();
				MyLibrary = reader.ReadString();
				QuickBar = reader.ReadString();
				RegBar = reader.ReadString();
				UsingAncientBook = reader.ReadBool();
			}

			DefaultRunebookSpellType = 2 < version ? (RunebookGump.SpellType)reader.ReadInt() : RunebookGump.SpellType.None;

			if (3 < version)
				MotdLastSeenVersion = reader.ReadInt();
			// v5 (local bump): Jascen's DoubleClickToTalk rides version 4 upstream, but our
			// v4 saves already carry MotdLastSeenVersion — gating on 4 avoids over-reading
			// a byte from existing saves.
			DoubleClickToTalk = 4 < version ? reader.ReadBool() : false;

			// v6 (local bump): Jascen's VendorContainerSell fields ride version 5 upstream,
			// but our v5 already carries DoubleClickToTalk — gating on 5 instead.
			if ( 5 < version )
			{
				VendorContainerSellEnabled = reader.ReadBool();
				VendorContainerSellShowItemImages = reader.ReadBool();
				VendorContainerSellCompactItemsPerPage = reader.ReadInt();
				VendorContainerSellLargeItemsPerPage = reader.ReadInt();
				VendorContainerSellSelectionBehavior = (VendorContainerSellSelectionBehavior)reader.ReadInt();
			}
			else
			{
				VendorContainerSellCompactItemsPerPage = VendorContainerSellConfigGump.DefaultVendorContainerSellCompactItemsPerPage;
				VendorContainerSellLargeItemsPerPage = VendorContainerSellConfigGump.DefaultVendorContainerSellLargeItemsPerPage;
				VendorContainerSellSelectionBehavior = VendorContainerSellSelectionBehavior.AsManyAsPossible;
			}

			// v7 (local bump): Jascen's LegacyCarve rides version 5 upstream, but our v6 saves
			// already carry the VendorContainerSell block — gating on 6 avoids over-reading
			// a bool from existing v6 saves (which shifted the whole PlayerMobile stream and
			// crashed world load with EndOfStreamException).
			LegacyCarve = 6 < version ? reader.ReadBool() : false;

			// v8 (local bump): Jascen's Modern lockpicking/removetrap gump preferences ride
			// version 6 upstream — gating on 7 keeps existing v7 saves reading correctly.
			if ( 7 < version )
			{
				ModernLockpickingEnabled = reader.ReadBool();
				ModernRemoveTrapEnabled = reader.ReadBool();
				ModernLockpickingAutoRetryEnabled = reader.ReadBool();
				ModernRemoveTrapsAutoRetryEnabled = reader.ReadBool();
			}
			else
			{
				ModernLockpickingEnabled = true;
				ModernRemoveTrapEnabled = true;
			}

			SuppressSystemMessages = 8 < version ? reader.ReadBool() : false;
			ModernTrackingEnabled = 9 < version ? reader.ReadBool() : true;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int CharacterBarbaric { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CharacterEvil { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string CharacterLoot { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CharacterOriental { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CharacterSheath { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CharacterWepAbNames { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string CharMusical { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ClassicPoisoning { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ColorlessFabricBreakdown { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public RunebookGump.SpellType DefaultRunebookSpellType { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DoubleClickID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DoubleClickToTalk { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IgnoreVendorGoldSafeguard { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool LegacyCarve { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MagerySpellHue { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool MessageOfTheDay { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ModernLockpickingAutoRetryEnabled { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ModernLockpickingEnabled { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ModernRemoveTrapEnabled { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ModernRemoveTrapsAutoRetryEnabled { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ModernTrackingEnabled { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string MusicPlaylist { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string MyChat { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string MyLibrary { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string QuickBar { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string RegBar { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowGumpImages { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SingleAttemptID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SuppressSystemMessages { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SuppressVendorTooltip { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UsingAncientBook { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VendorContainerSellCompactItemsPerPage { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool VendorContainerSellEnabled { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VendorContainerSellLargeItemsPerPage { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public VendorContainerSellSelectionBehavior VendorContainerSellSelectionBehavior { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool VendorContainerSellShowItemImages { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool WeaponBarOpen { get; set; }

		public void Serialize(GenericWriter writer)
		{
			writer.Write(10);

			writer.Write(DoubleClickID);
			writer.Write(SuppressVendorTooltip);
			writer.Write(SingleAttemptID);
			writer.Write(ColorlessFabricBreakdown);
			writer.Write(CharacterSheath);
			writer.Write(CharacterWepAbNames);
			writer.Write(CharMusical);
			writer.Write(WeaponBarOpen);
			writer.Write(IgnoreVendorGoldSafeguard);
			writer.Write(ClassicPoisoning);

			writer.Write(CharacterBarbaric);
			writer.Write(CharacterEvil);
			writer.Write(CharacterLoot);
			writer.Write(CharacterOriental);
			writer.Write(ShowGumpImages);
			writer.Write(MagerySpellHue);
			writer.Write(MessageOfTheDay);
			writer.Write(MusicPlaylist);
			writer.Write(MyChat);
			writer.Write(MyLibrary);
			writer.Write(QuickBar);
			writer.Write(RegBar);
			writer.Write(UsingAncientBook);
			writer.Write((int)DefaultRunebookSpellType);

			writer.Write(MotdLastSeenVersion);
			writer.Write(DoubleClickToTalk);

			writer.Write(VendorContainerSellEnabled);
			writer.Write(VendorContainerSellShowItemImages);
			writer.Write(VendorContainerSellCompactItemsPerPage);
			writer.Write(VendorContainerSellLargeItemsPerPage);
			writer.Write((int)VendorContainerSellSelectionBehavior);

			writer.Write(LegacyCarve);

			writer.Write(ModernLockpickingEnabled);
			writer.Write(ModernRemoveTrapEnabled);
			writer.Write(ModernLockpickingAutoRetryEnabled);
			writer.Write(ModernRemoveTrapsAutoRetryEnabled);

			writer.Write(SuppressSystemMessages);
			writer.Write(ModernTrackingEnabled);
		}
	}
}
