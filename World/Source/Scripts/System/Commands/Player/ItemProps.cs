using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Commands;
using Server.Localization;

namespace Server.Gumps
{
    public class ItemPropsGump : Gump
    {
		public int m_Origin;

		private static string ResolveText( Mobile from, string text )
		{
			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

        public ItemPropsGump( Mobile from, int origin ) : base( 50, 50 )
        {
			m_Origin = origin;
			string color = "#ddbc4b";
			from.SendSound( 0x4A ); 

			this.Closable=true;
			this.Disposable=true;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);

			AddImage(0, 0, 9546, Server.Misc.PlayerSettings.GetGumpHue( from ));
			AddHtml( 14, 14, 400, 20, @"<BODY><BASEFONT Color=" + color + ">" + ResolveText( from, "ITEM PROPERTIES" ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
			AddButton(867, 10, 4017, 4017, 0, GumpButtonType.Reply, 0);

			string lowmana = "<BR><BR>" + ResolveText( from, "Lower Mana Cost - Lowers the amount of mana needed to cast a spell or use a special move." );
				if ( MyServerSettings.LowerMana() < 1 )
					lowmana = "";

			string lowreg = "<BR><BR>" + ResolveText( from, "Lower Reagent Cost - Lowers the amount of reagents needed to cast spells like magery and necromancy. 100% negates the need to carry reagents at all. Tithing points, though unused, are required to be available to cast Chivalry spells. Elementalism reduces the amount of stamina loss for casting spells." ) + "<BR><BR>" + ResolveText( from, "Lower Requirements - Lowers any stat requirements the item has by a percentage. If an item has 100% Lower Requirements, it will have no stat requirements." );
				if ( MyServerSettings.LowerReg() < 1 )
					lowreg = "";

			string body =
				ResolveText( from, "Many equipment items you find maybe have magical attributes or special properties with them. Below are the brief descriptions of the various characteristics these items may have:" ) +
				"<BR><BR>" + ResolveText( from, "Damage Increase - Increases the base damage you inflict with your weapon." ) +
				"<BR><BR>" + ResolveText( from, "Damage Modifier - Increases the final damage dealt by the bow it's used with." ) +
				"<BR><BR>" + ResolveText( from, "Defense Chance Increase - Increases your chance to dodge blows." ) +
				"<BR><BR>" + ResolveText( from, "Density - An item density represents the strength of the material from which the item is created. It has categories of weak, regular, great, greater, superior, and ultimate. The better the density, the less chance it will be reduced in durability. If there is a chance the item can benefit from self repair, the amount repaired can be greater. The ability to enhance items, and not have then break, benefit from a good material density. When you stumble upon a trap, that can have a devestating affect on an item, having a good density can avoid such effects. Similar effects, caused by enemies, will be equally avoided with a good material density." ) +
				"<BR><BR>" + ResolveText( from, "Dexterity Bonus - Increases your Dexterity Stat by the number of points on the item." ) +
				"<BR><BR>" + ResolveText( from, "Durability Bonus - Durability bonuses are applied to an object once. A more durable object takes longer to wear down and break." ) +
				"<BR><BR>" + ResolveText( from, "Enhance Potions - Increases the effects of potions when they are used. Poison and nightsight potions are excluded." ) +
				"<BR><BR>" + ResolveText( from, "Faster Cast Recovery - Shortens waiting time between casting spells." ) +
				"<BR><BR>" + ResolveText( from, "Faster Casting - Decreases the time required to cast spells by 0.25 seconds per point." ) +
				"<BR><BR>" + ResolveText( from, "Hit Area Damage - May be physical, fire, cold, poison or energy type. Provides a percentage chance on each hit to deal additional area damage based on half of the weapon damage inflicted to the primary target. The area damage is not inflicted to the original target, but is inflicted to attackable targets within a 5 tile radius of the original target." ) +
				"<BR><BR>" + ResolveText( from, "Hit Chance Increase - Increases your chance to hit your opponents." ) +
				"<BR><BR>" + ResolveText( from, "Hit Dispel - Has a percentage chance on each hit, based on the wielder's Tactics skill, to cast the magery spell dispel on any summoned creature." ) +
				"<BR><BR>" + ResolveText( from, "Hit Fireball - Has a percentage chance on each hit to cast the magery spell fireball on the target." ) +
				"<BR><BR>" + ResolveText( from, "Hit Harm - Has a percentage chance on each hit to cast the magery spell harm on the target." ) +
				"<BR><BR>" + ResolveText( from, "Hit Life Leech - On every successful hit, converts a percentage of the damage inflicted by the attack into hit points for the wielder." ) +
				"<BR><BR>" + ResolveText( from, "Hit Lightning - Has a percentage chance on each hit to cast the magery spell lightning on the target." ) +
				"<BR><BR>" + ResolveText( from, "Hit Lower Attack - Has a percentage chance on each hit to lower the hit chance of the target." ) +
				"<BR><BR>" + ResolveText( from, "Hit Lower Defense - Has a percentage chance on each hit to lower the defensive capabilities of the target." ) +
				"<BR><BR>" + ResolveText( from, "Hit Magic Arrow - Has a percentage chance on each hit to cast the magery spell magic arrow on the target." ) +
				"<BR><BR>" + ResolveText( from, "Hit Mana Drain - Reduces the target's mana by a percentage of the damage dealt by the attack that triggers the affect." ) +
				"<BR><BR>" + ResolveText( from, "Hit Mana Leech - On every successful hit, converts a percentage of the damage inflicted by the attack into mana points for the wielder." ) +
				"<BR><BR>" + ResolveText( from, "Hit Point Increase - Increases your maximum hit points by the number of points on the item." ) +
				"<BR><BR>" + ResolveText( from, "Hit Point Regeneration - Increases the rate at which you regain hit points." ) +
				"<BR><BR>" + ResolveText( from, "Hit Stamina Leech - Has a percentage chance on each hit to convert 100% of the damage inflicted on the target into stamina for the wielder." ) +
				"<BR><BR>" + ResolveText( from, "Intelligence Bonus - Increases your Intelligence Stat by the number of points on the item." ) +
				"<BR><BR>" + ResolveText( from, "Lower Ammo Cost - Reduces the number of arrows/bolts used by a percentage." ) +
				lowmana + lowreg +
				"<BR><BR>" + ResolveText( from, "Luck - Increases the character's luck, which aids in events such as finding better treasure or avoiding traps." ) +
				"<BR><BR>" + ResolveText( from, "Mage Armor - Negates impediments to both active and passive meditation from armor types that would normally block it. Also negates impediment to stealth skill." ) +
				"<BR><BR>" + ResolveText( from, "Mage Weapon - Allows magery skill to substitute for the normal combat skill of the weapon. Special moves cannot be used via this substitution. Magery skill is reduced while a mage weapon is equipped." ) +
				"<BR><BR>" + ResolveText( from, "Mana Increase - Increases your maximum mana by the number of points on the item." ) +
				"<BR><BR>" + ResolveText( from, "Mana Regeneration - Increases the rate at which you regain mana, subject to diminishing returns." ) +
				"<BR><BR>" + ResolveText( from, "Night Sight - Helps you see in darkness, but also helps you in stumbling upon hidden dungeon treasure. The more night sight items you have equipped, the greater the chance to find such hidden treasure." ) +
				"<BR><BR>" + ResolveText( from, "Reflect Physical Damage - Reflect Physical Damage will reflect a percentage of any kinetic physical damage that is inflicted on you back onto the one who inflicted it." ) +
				"<BR><BR>" + ResolveText( from, "Resist - Resist types are: physical/fire/cold/poison/energy. Resist allows you to resist a percentage of all described damage." ) +
				"<BR><BR>" + ResolveText( from, "Self Repair - Has a chance of regaining a durability, when hit during combat. The better the item density, the more this will repair." ) +
				"<BR><BR>" + ResolveText( from, "Skill Bonus - Increases your skillpoints in a particular skill." ) +
				"<BR><BR>" + ResolveText( from, "Slayer - Weapons and spellbooks will do increased damage against all creatures within a certain group, while musical instruments will be more effective." ) +
				"<BR><BR>" + ResolveText( from, "Spell Channeling - Allows the casting of spells while a weapon or shield is equipped." ) +
				"<BR><BR>" + ResolveText( from, "Spell Damage Increase - Increases the amount of damage spells inflict." ) +
				"<BR><BR>" + ResolveText( from, "Stamina Increase - Increases your maximum stamina by the number of points on the item." ) +
				"<BR><BR>" + ResolveText( from, "Stamina Regeneration - Increases the rate at which you regain stamina." ) +
				"<BR><BR>" + ResolveText( from, "Strength Bonus - Increases your Strength Stat by the number of points on the item." ) +
				"<BR><BR>" + ResolveText( from, "Swing Speed Increase - Increases the base speed at which you swing your weapon." ) +
				"<BR><BR>" + ResolveText( from, "Use Best Weapon Skill - Substitutes the character's trained weapon skill for that normally required for the weapon type, but for melee weapons only. Archery and fist fighting are not included." ) +
				"<BR><BR>" + ResolveText( from, "Weight Reduction - Reduces the weight of the ammunition contained within a quiver." ) +
				"<BR><BR>";

			AddHtml( 17, 49, 875, 726, @"<BODY><BASEFONT Color=" + color + ">" + body + "</BASEFONT></BODY>", (bool)false, (bool)true);
        }

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;
			from.SendSound( 0x4A ); 
			if ( m_Origin > 0 ){ from.SendGump( new Server.Engines.Help.HelpGump( from, 1 ) ); }
		}
    }
}