using Server.Mobiles;
using Server.Spells.Song;

namespace Server.SpellBars
{
	public sealed class BardSpellSchool : ISpellSchool
	{
		public static readonly BardSpellSchool Instance = new BardSpellSchool();

		public int MaxSlots
		{ get { return BardSongProvider.SpellCount; } }

		public SpellBarSchool School
		{ get { return SpellBarSchool.Bard; } }

		public int GetBackgroundImage(PlayerMobile from)
		{ return 11165; }

		public int GetIcon(PlayerMobile from, int slotIndex)
		{
			return BardSongProvider.SpellDefinitions[slotIndex - 1].IconGraphic;
		}

		public string GetName(int slotIndex)
		{
			if (slotIndex < 1 || slotIndex > BardSongProvider.SpellCount)
				return string.Empty;

			return BardSongProvider.SpellDefinitions[slotIndex - 1].Name.String;
		}

		public int GetRegistrySpellId(int slotIndex)
		{ return BardSongProvider.SpellDefinitions[slotIndex - 1].SpellID; }

		public bool HasSpell(PlayerMobile from, int registrySpellId)
		{
			return BardSongProvider.HasSpell(from, registrySpellId);
		}
	}

	public sealed class SpellBarSetupGump_Bard_1 : SpellBarSetupGump
	{
		public SpellBarSetupGump_Bard_1(PlayerMobile from, int origin) : base(SpellBarId.Bard_1, from, origin)
		{
		}
	}

	public sealed class SpellBarSetupGump_Bard_2 : SpellBarSetupGump
	{
		public SpellBarSetupGump_Bard_2(PlayerMobile from, int origin) : base(SpellBarId.Bard_2, from, origin)
		{
		}
	}

	public sealed class SpellBarToolbarGump_Bard_1 : SpellBarToolbarGump
	{
		public SpellBarToolbarGump_Bard_1(PlayerMobile from) : base(SpellBarId.Bard_1, from)
		{
		}
	}

	public sealed class SpellBarToolbarGump_Bard_2 : SpellBarToolbarGump
	{
		public SpellBarToolbarGump_Bard_2(PlayerMobile from) : base(SpellBarId.Bard_2, from)
		{
		}
	}
}