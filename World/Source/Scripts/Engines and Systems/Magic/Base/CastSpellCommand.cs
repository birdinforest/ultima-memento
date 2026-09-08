using Server.Commands;
using System;
using System.Collections.Generic;

namespace Server.Spells
{
	public delegate void CastSpellCommandDelegate(Mobile from, int spellId);

	public static class CastSpellCommand
	{
		public static Docs.DocCommandEntry Register(int spellId, string className, string spellName, CastSpellCommandDelegate castHandler, params string[] aliases)
		{
			var spellNameNormalized = spellName.Replace(" ", "");
			var classNameNormalized = className.Replace(" ", "");
			var primary = string.Format("Cast_{0}_{1}", classNameNormalized, spellNameNormalized);
			var description = string.Format("Casts the {0} spell: {1}", className, spellName);

			CommandEventHandler handler = e => castHandler(e.Mobile, spellId);
			CommandSystem.Register(primary, AccessLevel.Player, handler);

			if (aliases != null)
			{
				for (var i = 0; i < aliases.Length; i++)
					CommandSystem.Register(aliases[i], AccessLevel.Player, handler);
			}

			return new Docs.DocCommandEntry(AccessLevel.Player, primary, aliases, primary, description);
		}

		public static List<Docs.DocCommandEntry> RegisterAll(string className, IEnumerable<SpellDefinition> definitions, CastSpellCommandDelegate castHandler, Func<SpellDefinition, string[]> aliasFactory)
		{
			var entries = new List<Docs.DocCommandEntry>();

			foreach (var definition in definitions)
			{
				var aliases = aliasFactory != null ? aliasFactory(definition) : null;
				entries.Add(Register(definition.SpellID, className, definition.Name.String, castHandler, aliases));
			}

			return entries;
		}
	}
}
