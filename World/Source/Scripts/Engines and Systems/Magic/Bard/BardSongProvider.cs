using Server.Items;
using System.Collections.Generic;
using System.Linq;

namespace Server.Spells.Song
{
	public class BardSongProvider
	{
		public static readonly List<SpellDefinition> SpellDefinitions = new List<SpellDefinition>
		{
			ArmysPaeonSong.SpellInfo.SpellDefinition,
			EnchantingEtudeSong.SpellInfo.SpellDefinition,
			EnergyCarolSong.SpellInfo.SpellDefinition,
			EnergyThrenodySong.SpellInfo.SpellDefinition,
			FireCarolSong.SpellInfo.SpellDefinition,
			FireThrenodySong.SpellInfo.SpellDefinition,
			FoeRequiemSong.SpellInfo.SpellDefinition,
			IceCarolSong.SpellInfo.SpellDefinition,
			IceThrenodySong.SpellInfo.SpellDefinition,
			KnightsMinneSong.SpellInfo.SpellDefinition,
			MagesBalladSong.SpellInfo.SpellDefinition,
			MagicFinaleSong.SpellInfo.SpellDefinition,
			PoisonCarolSong.SpellInfo.SpellDefinition,
			PoisonThrenodySong.SpellInfo.SpellDefinition,
			SheepfoeMamboSong.SpellInfo.SpellDefinition,
			SinewyEtudeSong.SpellInfo.SpellDefinition
		};

		public static int FirstSpellId
		{ get { return SpellDefinitions[0].SpellID; } }

		public static int LastSpellId
		{ get { return SpellDefinitions[SpellDefinitions.Count - 1].SpellID; } }

		public static int SpellCount
		{ get { return SpellDefinitions.Count; } }

		public static void Cast(Mobile from, int spellID)
		{
			if (!Multis.DesignContext.Check(from)) return;

			if (HasSpell(from, spellID))
				SpellRegistry.NewSpell(spellID, from, null).Cast();
			else
				from.SendLocalizedMessage(500015);
		}

		public static SpellDefinition GetDefinition(int spellID)
		{
			var spellDefinition = SpellDefinitions.FirstOrDefault(definition => definition.SpellID == spellID);

			return spellDefinition;
		}

		public static bool HasSpell(Mobile from, int spellID)
		{
			Spellbook book = Spellbook.Find(from, spellID, SpellbookType.Song);

			return book != null && book.HasSpell(spellID);
		}
	}
}