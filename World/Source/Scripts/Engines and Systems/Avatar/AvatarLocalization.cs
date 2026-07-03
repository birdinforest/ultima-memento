using Server.Localization;
using Server.Mobiles;
using Server.Items;

namespace Server.Engines.Avatar
{
	public static class AvatarLocalization
	{
		public static string Key(Mobile viewer, string logicalKey, string englishIfMissing)
		{
			string lang = AccountLang.GetLanguageCode(viewer != null ? viewer.Account : null);
			string s = StringCatalog.TryResolveByKey(lang, logicalKey);
			return !string.IsNullOrEmpty(s) ? s : englishIfMissing;
		}

		public static string KeyFormat(Mobile viewer, string logicalKey, string englishIfMissing, params object[] args)
		{
			string fmt = Key(viewer, logicalKey, englishIfMissing);

			if (args == null || args.Length == 0)
				return fmt;

			try
			{
				return string.Format(fmt, args);
			}
			catch
			{
				return englishIfMissing;
			}
		}

		public static void Send(Mobile to, string logicalKey, string englishIfMissing)
		{
			to.SendMessage(Key(to, logicalKey, englishIfMissing));
		}

		public static void SendFormat(Mobile to, string logicalKey, string englishIfMissing, params object[] args)
		{
			to.SendMessage(KeyFormat(to, logicalKey, englishIfMissing, args));
		}

		public static string ResolveEnglish(Mobile viewer, string english)
		{
			if (viewer == null || string.IsNullOrEmpty(english))
				return english;

			return StringCatalog.Resolve(viewer.Account, english);
		}

		public static string CategoryName(Mobile viewer, Categories category)
		{
			switch (category)
			{
				case Categories.PrimaryBoosts:
					return Key(viewer, "avatar.category.primary_skills", "Primary Skills");
				case Categories.SecondaryBoosts:
					return Key(viewer, "avatar.category.secondary_skills", "Secondary Skills");
				case Categories.FullSkillArchive:
					return Key(viewer, "avatar.category.skill_archive", "Skill Archive");
				case Categories.Information:
					return Key(viewer, "avatar.category.information", "Information");
				case Categories.Ascensions:
					return Key(viewer, "avatar.category.ascensions", "Ascensions");
				case Categories.Templates:
					return Key(viewer, "avatar.category.templates", "Templates");
				case Categories.Items:
					return Key(viewer, "avatar.category.items", "Items");
				default:
					return category.ToString();
			}
		}

		public static string RivalFactionName(Mobile viewer, SlayerName slayer)
		{
			switch (slayer)
			{
				case SlayerName.None:
					return Key(viewer, "avatar.rival.none", "None");
				case SlayerName.Silver:
					return Key(viewer, "avatar.rival.returned", "The Returned");
				case SlayerName.Repond:
					return Key(viewer, "avatar.rival.oathbreakers", "The Oathbreakers");
				case SlayerName.ReptilianDeath:
					return Key(viewer, "avatar.rival.scaled_ones", "The Scaled Ones");
				case SlayerName.Exorcism:
					return Key(viewer, "avatar.rival.dreadwings", "The Dreadwings");
				case SlayerName.ArachnidDoom:
					return Key(viewer, "avatar.rival.doom_weavers", "The Doom Weavers");
				case SlayerName.ElementalBan:
					return Key(viewer, "avatar.rival.riftborn", "The Riftborn");
				case SlayerName.WizardSlayer:
					return Key(viewer, "avatar.rival.spellreavers", "The Spellreavers");
				case SlayerName.AvianHunter:
					return Key(viewer, "avatar.rival.skycleave_talons", "The Skycleave Talons");
				case SlayerName.SlimyScourge:
					return Key(viewer, "avatar.rival.oozen_swarm", "The Oozen Swarm");
				case SlayerName.AnimalHunter:
					return Key(viewer, "avatar.rival.pack", "The Pack");
				case SlayerName.GiantKiller:
					return Key(viewer, "avatar.rival.colossal", "The Colossal");
				case SlayerName.GolemDestruction:
					return Key(viewer, "avatar.rival.construct", "The Construct");
				case SlayerName.WeedRuin:
					return Key(viewer, "avatar.rival.briarblight", "The Briarblight");
				case SlayerName.NeptunesBane:
					return Key(viewer, "avatar.rival.tidebreakers", "The Tidebreakers");
				case SlayerName.Fey:
					return Key(viewer, "avatar.rival.faeborn_circle", "The Faeborn Circle");
				default:
					return Key(viewer, "avatar.rival.unknown", "Unknown Rival Race");
			}
		}

		public static string StarterTemplateName(Mobile viewer, AvatarStarterTemplates template)
		{
			switch (template)
			{
				case AvatarStarterTemplates.Ninja:
					return Key(viewer, "avatar.template.ninja", "Ninja");
				case AvatarStarterTemplates.Bard:
					return Key(viewer, "avatar.template.bard", "Bard");
				case AvatarStarterTemplates.Druid:
					return Key(viewer, "avatar.template.druid", "Druid");
				case AvatarStarterTemplates.Knight:
					return Key(viewer, "avatar.template.knight", "Knight");
				case AvatarStarterTemplates.Warrior:
					return Key(viewer, "avatar.template.warrior", "Warrior");
				case AvatarStarterTemplates.Mage:
					return Key(viewer, "avatar.template.mage", "Mage");
				case AvatarStarterTemplates.Archer:
					return Key(viewer, "avatar.template.archer", "Archer");
				case AvatarStarterTemplates.Brute:
					return Key(viewer, "avatar.template.brute", "The Brute");
				case AvatarStarterTemplates.Acrobat:
					return Key(viewer, "avatar.template.acrobat", "The Acrobat");
				case AvatarStarterTemplates.Scholar:
					return Key(viewer, "avatar.template.scholar", "The Scholar");
				default:
					return template.ToString();
			}
		}
	}
}
