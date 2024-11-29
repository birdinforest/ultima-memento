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
			builder.Append("- Craft - Resist bonus for exceptional hats now uses Arms Lore skill level and has been reduced to 7, down from 15<br>");
			builder.Append("- Craft - Items can only be enhanced if they are basic resources<br>");
			builder.Append("- Craft - Add ability to break down all items in a container<br>");
			builder.Append("- Craft - Cut/Break down no longer requires 50 skill<br>");
			builder.Append("- Craft - Bulk crafting no longer resets Stat Gain cooldown<br>");
			builder.Append("- Craft - Merchant crates (disabled) now have 24 hour timer and cap at 100k gold<br>");
			builder.Append("- Craft - Large BODs can no longer generate the required Small BODs<br>");
			builder.Append("- Enchantment - Lower Mana Cost (LMC) is now capped at 8%<br>");
			builder.Append("- Enchantment - Lower Reagent Cost (LRC) is now capped at 20%<br>");
			builder.Append("- Gather - Add 'Glistening ore vein' to give Mining a more active playstyle<br>");
			builder.Append("- Gather - Dwarven ore can now only be acquired via 'Glistening ore vein'<br>");
			builder.Append("- Gather - Increased Nepturite spawn rate<br>");
			builder.Append("- Gather - Logs now weigh 1 stone, up from 0.5<br>");
			builder.Append("- Gather - Logs can now be used while on a pack animal<br>");
			builder.Append("- Gather - Sawing logs now acts as if it was done individually<br>");
			builder.Append("- Gather - Ore now weighs 2 stone, up from 0.5<br>");
			builder.Append("- Gather - Ore now only comes in the large size<br>");
			builder.Append("- Gather - Ore can now be used while on a pack animal<br>");
			builder.Append("- Gather - Smelting ore now acts as if it was done individually<br>");
			builder.Append("- Gather - Ore veins have been reduced and are now 5 to 17, down from 10 to 34<br>");
			builder.Append("- Gather - Crops within 1 tile are automatically harvested<br>");
			builder.Append("- Gather - Tool uses are reduced by the quantity of resources harvested<br>");
			builder.Append("- Gump - Redesigned crafting tool gump<br>");
			builder.Append("- Item - Increased the cost of all 115/120/125 Powerscrolls<br>");
			builder.Append("- Item - Powerscrolls now have to be used in order<br>");
			builder.Append("- Item - Powerscrolls purchases can now use Checks in your Bank<br>");
			builder.Append("- Item - Powerscrolls purchases can now use Gold in your Inventory<br>");
			builder.Append("- Item - The luck bonus for Lucky horse shoe is now 100 per item<br>");
			builder.Append("- Item - Artifacts no longer have enchantment points<br>");
			builder.Append("- Item - Dungeon Chests now allow for multiple Stealing attempts<br>");
			builder.Append("- Item - Oil cloth are now stackable<br>");
			builder.Append("- Item - Quiver Damage Modifier has been reduced to 10%<br>");
			builder.Append("- Item - Books are deleted after memorizing them<br>");
			builder.Append("- Item - Scepter/Stave now have Spell Channeling by default<br>");
			builder.Append("- Item - Bank vaults are now sold by Bankers<br>");
			builder.Append("- Misc - Reduce packhorse max weight to 2400, down from 65000<br>");
			builder.Append("- Misc - Reduce max properties from randomly generated items from 16 to 8<br>");
			builder.Append("- Quest - Sage Artifact quest has been ported to ML Quest System<br>");
			builder.Append("- Quest - Sage Artifact quest now involves townsfolk<br>");
			builder.Append("- Settings - Added a setting to require eating Powerscrolls in order<br>");
			builder.Append("- Settings - Lower Mana Cost (LMC) is now capped at 40%<br>");
			builder.Append("- Settings - Lower Reagent Cost (LRC) is now capped at 100%<br>");
			builder.Append("- Skill - Skills gains are accelerated to 70 but reduced at 85/95/105/110/115<br>");
			builder.Append("- Skill - Healing is now an activatable* skill that can remove poison/bleed or heal you<br>"); // TODO: Update documentation, Make usable in client files
			builder.Append("- Skill - Hiding cooldown is increased to 4 seconds, up from 1s and 2s<br>");
			builder.Append("- Spell - Players must be friend or higher to use any spell in a house<br>");
			builder.Append("- Stats - Mana Regen cap reduced to 18, down from 36<br>");

			builder.Append("<br>");
			builder.Append("Fixes<br>");
			builder.Append("- Craft - Candelabras now cap at 95 skill rather than 195<br>");
			builder.Append("- Gather - More tiles are now mineable<br>");
			builder.Append("- Gather - Added system message when digging up Dwarven ore/granite<br>");
			builder.Append("- Gather - The 'Resources' server setting is now limited by the amount of resources in the harvest bank<br>");
			builder.Append("- Gather - Drop harvested items to ground when backpack is full<br>");
			builder.Append("- Gather - Fix InvalidCastException when Fishing<br>");
			builder.Append("- Gump - Alien Players who use Tithe to pay for a res from another player are no longer double penalized<br>");
			builder.Append("- Gump - Sending buttons that don't exist will now disconnect the Player<br>");
			builder.Append("- Gump - Remove redundant quiver damage qualifier<br>");
			builder.Append("- Item - Added missing Powerscrolls (Begging, Camping, Forensics, Mercantile, Tasting)<br>");
			builder.Append("- Item - Lucky horse shoe now work on Instruments and Quivers<br>");
			builder.Append("- Item - Unidentified items with deleted contents are now properly deleted<br>");
			builder.Append("- Item - Fix InvalidCastException when monsters rummage an ElementalSpellbook<br>");
			builder.Append("- Item - Fix monsters stamping their name on an un-owned MysticSpellbook<br>");
			builder.Append("- Item - Fix issue with Relic Tablets where they would rotate instead of open when in a Player's backpack in a house<br>");
			builder.Append("- Item - Horse Armor only gives bonus once<br>");
			builder.Append("- Item - Horse Armor can no longer be infinitely farmed<br>");
			builder.Append("- Item - Repair and Durability potions now work on clothing<br>");
			builder.Append("- Item - Stone face trap is now choppable<br>");
			builder.Append("- Misc - Set Map when [scan players<br>");
			builder.Append("- Misc - Fix typo in RangeCheck()<br>");
			builder.Append("- Misc - Buffs/Debuffs now end on the Server before the Client timer elapses<br>");
			builder.Append("- Misc - Stop deleting an item when it's stacked with itself<br>");
			builder.Append("- Misc - Monster races now get the configured starting gold<br>");
			builder.Append("- Spell - BloodOath could linger up to 1s too long on the Server<br>");
			builder.Append("- Spell - Bard songs weren't blocked when no instrument was assigned<br>");
			builder.Append("- Spell - Bard songs weren't blocked when below required skill level<br>");
			builder.Append("- Spell - Mirror image now has the correct buff name<br>");
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