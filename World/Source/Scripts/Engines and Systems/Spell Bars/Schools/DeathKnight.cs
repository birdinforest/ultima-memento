using Server.Mobiles;
using Server.Spells.DeathKnight;

namespace Server.SpellBars
{
	public sealed class DeathKnightSpellSchool : ISpellSchool
	{
		public static readonly DeathKnightSpellSchool Instance = new DeathKnightSpellSchool();

		public int MaxSlots
		{ get { return DeathKnightSpellProvider.SpellCount; } }

		public SpellBarSchool School
		{ get { return SpellBarSchool.DeathKnight; } }

		public int GetBackgroundImage(PlayerMobile from)
		{ return 11168; }

		public int GetIcon(PlayerMobile from, int slotIndex)
		{ return DeathKnightSpellProvider.SpellDefinitions[slotIndex - 1].IconGraphic; }

		public string GetName(int slotIndex)
		{
			if (slotIndex < 1 || slotIndex > DeathKnightSpellProvider.SpellCount)
				return string.Empty;

			return DeathKnightSpellProvider.SpellDefinitions[slotIndex - 1].Name.String;
		}

		public int GetRegistrySpellId(int slotIndex)
		{ return DeathKnightSpellProvider.SpellDefinitions[slotIndex - 1].SpellID; }

		public bool HasSpell(PlayerMobile from, int registrySpellId)
		{
			return DeathKnightSpellProvider.HasSpell(from, registrySpellId);
		}
	}

	public sealed class SpellBarSetupGump_Death_1 : SpellBarSetupGump
	{
		public SpellBarSetupGump_Death_1(PlayerMobile from, int origin) : base(SpellBarId.Death_1, from, origin)
		{
		}
	}

	public sealed class SpellBarSetupGump_Death_2 : SpellBarSetupGump
	{
		public SpellBarSetupGump_Death_2(PlayerMobile from, int origin) : base(SpellBarId.Death_2, from, origin)
		{
		}
	}

	public sealed class SpellBarToolbarGump_Death_1 : SpellBarToolbarGump
	{
		public SpellBarToolbarGump_Death_1(PlayerMobile from) : base(SpellBarId.Death_1, from)
		{
		}
	}

	public sealed class SpellBarToolbarGump_Death_2 : SpellBarToolbarGump
	{
		public SpellBarToolbarGump_Death_2(PlayerMobile from) : base(SpellBarId.Death_2, from)
		{
		}
	}
}