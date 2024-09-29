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
			builder.Append("- Enchantment - Lower Mana Cost (LMC) is now capped at 8%<br>");
			builder.Append("- Enchantment - Lower Reagent Cost (LRC) is now capped at 20%<br>");
			builder.Append("- Item - Increased the cost of all 115/120/125 Powerscrolls<br>");
			builder.Append("- Item - Powerscrolls now have to be used in order<br>");
			builder.Append("- Item - Powerscrolls purchases can now use Checks in your Bank<br>");
			builder.Append("- Item - Powerscrolls purchases can now use Gold in your Inventory<br>");
			builder.Append("- Settings - Added a setting to require eating Powerscrolls in order<br>");
			builder.Append("- Settings - Lower Mana Cost (LMC) is now capped at 40%<br>");
			builder.Append("- Settings - Lower Reagent Cost (LRC) is now capped at 100%<br>");

			builder.Append("<br>");
			builder.Append("Fixes<br>");
			builder.Append("- Item - Added missing Powerscrolls (Begging, Camping, Forensics, Mercantile, Tasting)<br>");
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