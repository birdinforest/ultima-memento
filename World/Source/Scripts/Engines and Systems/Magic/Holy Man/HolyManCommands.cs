using Server.Commands;
using Server.Spells;
using Server.Spells.HolyMan;
using System.Collections.Generic;

namespace Server.Scripts.Commands
{
	public class HolyManCommands
	{
		private static List<Docs.DocCommandEntry> m_HelpEntries;

		public static void Initialize()
		{
			m_HelpEntries = CastSpellCommand.RegisterAll(
				"Holy Man",
				HolyManSpellProvider.SpellDefinitions,
				HolyManSpellProvider.Cast,
				definition =>
				{
					var spellNameNormalized = definition.Name.String.Replace(" ", "");
					return new string[] { string.Format("HM{0}", spellNameNormalized) };
				}
			);

			CommandSystem.Register("Spells_HolyMan", AccessLevel.Player, new CommandEventHandler(SpellsHolyMan_OnCommand));
		}

		[Usage("Spells_HolyMan")]
		[Description("Lists Holy Man spell cast commands.")]
		private static void SpellsHolyMan_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendGump(new HelpInfo.CommandListGump(0, e.Mobile, m_HelpEntries));
		}
	}
}
