using Server.Mobiles;
using Server.Spells.HolyMan;

namespace Server.SpellBars
{
	public sealed class PriestSpellSchool : ISpellSchool
	{
		public static readonly PriestSpellSchool Instance = new PriestSpellSchool();

		public int MaxSlots
		{ get { return HolyManSpellProvider.SpellCount; } }

		public SpellBarSchool School
		{ get { return SpellBarSchool.Priest; } }

		public int GetBackgroundImage(PlayerMobile from)
		{ return 11171; }

		public int GetIcon(PlayerMobile from, int slotIndex)
		{ return HolyManSpellProvider.SpellDefinitions[slotIndex - 1].IconGraphic; }

		public string GetName(int slotIndex)
		{
			if (slotIndex < 1 || slotIndex > HolyManSpellProvider.SpellDefinitions.Count)
				return string.Empty;

			return HolyManSpellProvider.SpellDefinitions[slotIndex - 1].Name.String;
		}

		public int GetRegistrySpellId(int slotIndex)
		{ return HolyManSpellProvider.SpellDefinitions[slotIndex - 1].SpellID; }

		public bool HasSpell(PlayerMobile from, int registrySpellId)
		{
			return HolyManSpellProvider.HasSpell(from, registrySpellId);
		}
	}

	public sealed class SpellBarSetupGump_Priest_1 : SpellBarSetupGump
	{
		public SpellBarSetupGump_Priest_1(PlayerMobile from, int origin) : base(SpellBarId.Priest_1, from, origin)
		{
		}
	}

	public sealed class SpellBarSetupGump_Priest_2 : SpellBarSetupGump
	{
		public SpellBarSetupGump_Priest_2(PlayerMobile from, int origin) : base(SpellBarId.Priest_2, from, origin)
		{
		}
	}

	public sealed class SpellBarToolbarGump_Priest_1 : SpellBarToolbarGump
	{
		public SpellBarToolbarGump_Priest_1(PlayerMobile from) : base(SpellBarId.Priest_1, from)
		{
		}
	}

	public sealed class SpellBarToolbarGump_Priest_2 : SpellBarToolbarGump
	{
		public SpellBarToolbarGump_Priest_2(PlayerMobile from) : base(SpellBarId.Priest_2, from)
		{
		}
	}
}