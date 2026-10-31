using Server.Spells;
using Server.Spells.Song;
using Server.Commands;
using System.Collections.Generic;

namespace Server.Scripts.Commands
{
	public class CastSongSpells
	{
		private static List<Docs.DocCommandEntry> m_HelpEntries;

		public static void Initialize()
		{
			m_HelpEntries = CastSpellCommand.RegisterAll(
				"Bard",
				BardSongProvider.SpellDefinitions,
				BardSongProvider.Cast,
				definition =>
				{
					var spellNameNormalized = definition.Name.String.Replace(" ", "").Replace("'", "");
					return new string[] { spellNameNormalized };
				}
			);

			CommandSystem.Register("Spells_Bard", AccessLevel.Player, new CommandEventHandler(SpellsBard_OnCommand));
		}

		[Usage("Spells_Bard")]
		[Description("Lists Bard spell cast commands.")]
		private static void SpellsBard_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendGump(new HelpInfo.CommandListGump(0, e.Mobile, m_HelpEntries));
		}
	}
}
