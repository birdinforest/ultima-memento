using System.Text;

namespace Server.Misc
{
    class ChangeLog
    {
		public static string Version()
		{
			return "Version: Hegran (DD MMM YYYY)";
		}

		public static string Versions()
        {
			const string SEPARATOR_LINE = "<br>---------------------------------------------------------------------------------<br><br>";
			var builder = new StringBuilder();

			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			builder.Append(Version());
			builder.Append("<br>");

			builder.Append("<br>");
			builder.Append("Changes<br>");
			builder.Append("- Craft - Using non-basic resources can now yield multiple skill gains<br>");
			builder.Append("- Enchantment - Lower Mana Cost (LMC) is now capped at 8%<br>");
			builder.Append("- Enchantment - Lower Reagent Cost (LRC) is now capped at 20%<br>");
			builder.Append("- Gather - Increased Nepturite spawn rate<br>");
			builder.Append("- Gather - Logs now weigh 1 stone, up from 0.5<br>");
			builder.Append("- Gather - Logs can now be used while on a pack animal<br>");
			builder.Append("- Gather - Sawing logs now acts as if it was done individually<br>");
			builder.Append("- Gather - Ore now weighs 2 stone, up from 0.5<br>");
			builder.Append("- Gather - Ore now only comes in the large size<br>");
			builder.Append("- Gather - Ore can now be used while on a pack animal<br>");
			builder.Append("- Gather - Smelting ore now acts as if it was done individually<br>");
			builder.Append("- Gather - Crops within 1 tile are automatically harvested<br>");
			builder.Append("- Item - Increased the cost of all 115/120/125 Powerscrolls<br>");
			builder.Append("- Item - Powerscrolls now have to be used in order<br>");
			builder.Append("- Item - Powerscrolls purchases can now use Checks in your Bank<br>");
			builder.Append("- Item - Powerscrolls purchases can now use Gold in your Inventory<br>");
			builder.Append("- Item - The luck bonus for Lucky horse shoe is now 100 per item<br>");
			builder.Append("- Item - Artifacts no longer have enchantment points<br>");
			builder.Append("- Quest - Sage Artifact quest now always costs 10,000 gold<br>");
			builder.Append("- Settings - Added a setting to require eating Powerscrolls in order<br>");
			builder.Append("- Settings - Lower Mana Cost (LMC) is now capped at 40%<br>");
			builder.Append("- Settings - Lower Reagent Cost (LRC) is now capped at 100%<br>");
			builder.Append("- Skill - Skills gains are accelerated to 70 but reduced at 85/95/105/110/115<br>");
			builder.Append("- Skill - Healing is now an activatable* skill that can remove poison/bleed or heal you<br>"); // TODO: Update documentation, Make usable in client files

			builder.Append("<br>");
			builder.Append("Fixes<br>");
			builder.Append("- Gather - More tiles are now mineable<br>");
			builder.Append("- Gather - Added system message when digging up Dwarven ore/granite<br>");
			builder.Append("- Gather - The 'Resources' server setting is now limited by the amount of resources in the harvest bank<br>");
			builder.Append("- Item - Added missing Powerscrolls (Begging, Camping, Forensics, Mercantile, Tasting)<br>");
			builder.Append("- Item - Lucky horse shoe now work on Instruments and Quivers<br>");
			builder.Append("- Misc - Set Map when [scan players<br>");
			builder.Append("- Misc - Fix typo in RangeCheck()<br>");
			builder.Append("- Misc - Buffs/Debuffs now end on the Server before the Client timer elapses<br>");
			builder.Append("- Spell - BloodOath could linger up to 1s too long on the Server<br>");
			builder.Append(SEPARATOR_LINE);

			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			builder.Append("Re-baseline - September 28 2024<br>");
			builder.Append("- Reset repository to Adventurers of Akalabeth version 'Samurai - 25 September 2024'<br>");
			builder.Append(SEPARATOR_LINE);

			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			builder.Append("Inception - 4 August 2024<br>");
			builder.Append("- Ultima: Memento begins using Adventurers of Akalabeth version 'Necromancer - 26 July 2024'<br>");
			builder.Append("<br>");

			return builder.ToString();
		}
	}
}