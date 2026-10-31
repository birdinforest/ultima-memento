
using Server.Spells;
using Server.Spells.DeathKnight;
using Server.Commands;
using System.Collections.Generic;

namespace Server.Scripts.Commands
{
	public class DeathKnightCommands
	{
		private static List<Docs.DocCommandEntry> m_HelpEntries;

		public static void Initialize()
		{
			m_HelpEntries = CastSpellCommand.RegisterAll(
				"Death Knight",
				DeathKnightSpellProvider.SpellDefinitions,
				DeathKnightSpellProvider.Cast,
				definition =>
				{
					var spellNameNormalized = definition.Name.String.Replace(" ", "").Replace("'", "");
					return new string[] { string.Format("DK{0}", spellNameNormalized) };
				}
			);

			CommandSystem.Register("Spells_DeathKnight", AccessLevel.Player, new CommandEventHandler(SpellsDeathKnight_OnCommand));
		}

		[Usage("Spells_DeathKnight")]
		[Description("Lists Death Knight spell cast commands.")]
		private static void SpellsDeathKnight_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendGump(new HelpInfo.CommandListGump(0, e.Mobile, m_HelpEntries));
		}
	}
}
