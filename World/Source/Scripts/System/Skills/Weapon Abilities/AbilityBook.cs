using System;
using Server;
using Server.Items;
using System.Text;
using Server.Mobiles;
using Server.Gumps;
using System.Collections;
using Server.Network;
using Server.Spells;
using Server.Localization;

namespace Server.Items
{
	public class WeaponAbilityBook : Item
	{
		[Constructable]
		public WeaponAbilityBook( ) : base( 0x2254 )
		{
			Weight = 1.0;
			Name = "Weapon Abilities";
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			if (BuildingPropertyListLocale != null)
				AddLocalizedProperty(list, "item.special.wabook");
			else
				base.AddNameProperty(list);
		}

		public class AbilityBookGump : Gump
		{
			public AbilityBookGump( Mobile from ) : base( 100, 100 )
			{
				from.SendSound( 0x55 );

				Closable=true;
				Disposable=true;
				Dragable=true;
				Resizable=false;

				AddPage( 0 ); 
				AddImage( 80, 79, 11010, 0 );

				AddPage(1);
				AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, 2 );
				AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, 63 );
				AddHtml( 146, 111, 112, 75, StringCatalog.Resolve( from.Account, "<p align='center'>The Complete Book of Weapon Mastery" ), (bool)false, (bool)false);
				AddImage(166, 193, 21248);
				AddHtml( 291, 108, 139, 163, StringCatalog.Resolve( from.Account, "Warriors have the ability to tap their Mana to perform devastating maneuvers with their weapons that can have a variety of unusual side-effects. Each weapon will have a" ), (bool)false, (bool)false);

				AddPage(2);
				AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, 3 );
				AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, 1 );
				AddHtml( 135, 108, 139, 163, StringCatalog.Resolve( from.Account, "unique combination of special moves. Warriors who have reached a 70 skill level in their weapon skill will be able to execute a weapon's primary special move. There can be five" ), (bool)false, (bool)false);
				AddHtml( 291, 108, 139, 163, StringCatalog.Resolve( from.Account, "total special abilities for weapons, achieved at 80, 90, 100, and 110 in the weapon skill. The skill level required can even be achieved through use of skill enhancing items such as rings," ), (bool)false, (bool)false);

				AddPage(3);
				AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, 4 );
				AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, 2 );
				AddHtml( 135, 108, 139, 163, StringCatalog.Resolve( from.Account, "bracelets, boots, robes, cloaks, belts, and earrings. In all cases another skill is required to perform these maneuvers. This is always tactics, however anatomy" ), (bool)false, (bool)false);
				AddHtml( 291, 108, 139, 163, StringCatalog.Resolve( from.Account, "can sometimes help. Whenever you equip a weapon, you will get a display of buttons to select to initiate these special moves if your skill allows. To activate or deactivate a" ), (bool)false, (bool)false);

				AddPage(4);
				AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, 5 );
				AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, 3 );
				AddHtml( 135, 108, 139, 163, StringCatalog.Resolve( from.Account, "special move, select the icon and the ribbon will turn red. At the next opportunity, the special move is performed, and the ribbon returns to its gray state. The Mana Cost of each special" ), (bool)false, (bool)false);
				AddHtml( 291, 108, 139, 163, StringCatalog.Resolve( from.Account, "move can be reduced if the warrior's skills are high enough. Add up the skill points for Swords, Bludgeoning, Fencing, Marksmanship, Parrying, Fist Fighting, Lumberjacking, Stealth, Poisoning, Bushido and" ), (bool)false, (bool)false);

				AddPage(5);
				AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, 6 );
				AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, 4 );
				AddHtml( 135, 108, 139, 163, StringCatalog.Resolve( from.Account, "Ninjitsu. If the total lies between 200 and 299, subtract 5 from the Mana Cost. If the total is 300 or more, subtract 10 from the Mana Cost. Some items have a property called 'lower" ), (bool)false, (bool)false);
				AddHtml( 291, 108, 139, 163, StringCatalog.Resolve( from.Account, "mana cost'. These items also reduce the Mana Cost of these Special Moves. If a special move is attempted within 3 seconds after another special move, the mana cost of that move will" ), (bool)false, (bool)false);

				AddPage(6);
				AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, 7 );
				AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, 5 );
				AddHtml( 135, 108, 139, 163, StringCatalog.Resolve( from.Account, "be doubled. The special move bar can have the names of the special moves to the right of the icons if you so choose. If you want to turn this feature on or off, simply type the" ), (bool)false, (bool)false);
				AddHtml( 291, 108, 139, 163, StringCatalog.Resolve( from.Account, "command '[abilitynames' without the quotes." ), (bool)false, (bool)false);

				int counter = 0;
				int pages = 55;
				int nPage = 6;
				int myIcon = 0;
				string myAttack = "";
				int myMana = 0;
				string sMana = "";
				string myDescribe1 = "";
				string myDescribe2 = "";

				while ( counter < pages )
				{
					counter++;
					nPage++;
					myDescribe2 = "";

					switch( counter )
					{
						case 1:  myIcon = 0x1;		myAttack = "skl.ability.achilles_strike";			myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "A strike from the weapon will greatly hurt the target's Achilles tendon." ); break;
						case 2:  myIcon = 0x2;		myAttack = "skl.ability.armor_ignore";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Ignores the Target's Resists but deals slightly lower damage than the weapon's maximum potential." ); break;
						case 3:  myIcon = 0x3;		myAttack = "skl.ability.armor_pierce";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Strike your foe with armor piercing force and inflicting greater damage." ); break;
						case 4:  myIcon = 0x4;		myAttack = "skl.ability.bladeweave";				myMana = 10; myDescribe1 = StringCatalog.Resolve( from.Account, "The warrior becomes one with their weapon, allowing it to guide their hand." ); myDescribe2 = StringCatalog.Resolve( from.Account, "The effects of this attack are unpredictable, but effective (10+? Mana)." ); break;
						case 5:  myIcon = 0x5;		myAttack = "skl.ability.bleed_attack";				myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes the target to bleed profusely, causing Direct Damage several times over" ); myDescribe2 = StringCatalog.Resolve( from.Account, "the next few seconds. The amount of Damage dealt decreases each time." ); break;
						case 6:  myIcon = 0x6;		myAttack = "skl.ability.block";						myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Raises your defenses for a short time." ); break;
						case 7:  myIcon = 0x7;		myAttack = "skl.ability.concussion_blow";			myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Does Direct Damage to the Target based on the difference between their current" ); myDescribe2 = StringCatalog.Resolve( from.Account, "Hit Points and Mana. The greater the difference, the more Damage they receive." ); break;
						case 8:  myIcon = 0x8;		myAttack = "skl.ability.consecrated_strike";		myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes the weapon to do the best amount of damage possible." ); break;
						case 9:  myIcon = 0xA;		myAttack = "skl.ability.crushing_blow";				myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Does a substantial extra amount of damage directly to the Target." ); break;
						case 10: myIcon = 0xB;		myAttack = "skl.ability.death_blow";				myMana = 50; myDescribe1 = StringCatalog.Resolve( from.Account, "Allows you to deliver a deadly blow." ); break;
						case 11: myIcon = 0x11;		myAttack = "skl.ability.defense_mastery";			myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Raises your physical resistance for a short time while lowering your ability to inflict damage." ); break;
						case 12: myIcon = 0x12;		myAttack = "skl.ability.devastating_blow";			myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Allows you to deliver a near deadly blow." ); break;
						case 13: myIcon = 0x13;		myAttack = "skl.ability.disarm";					myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Disarms Target and prevents them from rearming any weapon for a short duration." ); break;
						case 14: myIcon = 0x14;		myAttack = "skl.ability.dismount";					myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Dislodges Target from their Mount and deals a moderate amount of Direct damage to them." ); break;
						case 15: myIcon = 0x15;		myAttack = "skl.ability.disrobe";					myMana = 15; myDescribe1 = StringCatalog.Resolve( from.Account, "Forces your target to lose their outer clothing." ); break;
						case 16: myIcon = 0x16;		myAttack = "skl.ability.double_shot";				myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Send two arrows flying at your opponent if you're mounted." ); break;
						case 17: myIcon = 0x17;		myAttack = "skl.ability.double_strike";				myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "The next Target the user strikes will be hit by the weapon twice." ); break;
						case 18: myIcon = 0x18;		myAttack = "skl.ability.double_whirlwind_attack";	myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Hits all enemies in range, with extra bonus damage if there are many targets around you." ); break;
						case 19: myIcon = 0x19;		myAttack = "skl.ability.drain_dexterity";			myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Drains the target's dexterity when they are struck." ); break;
						case 20: myIcon = 0x1A;		myAttack = "skl.ability.drain_intellect";			myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Drains the target's intelligence when they are struck." ); break;
						case 21: myIcon = 0x1B;		myAttack = "skl.ability.drain_mana";				myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Drains the target's mana when they are struck." ); break;
						case 22: myIcon = 0x1C;		myAttack = "skl.ability.drain_stamina";				myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Drains the target's stamina when they are struck." ); break;
						case 23: myIcon = 0x2B;		myAttack = "skl.ability.drain_strength";			myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Drains the target's strength when they are struck." ); break;
						case 24: myIcon = 0x2C;		myAttack = "skl.ability.dual_wield";				myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Attack faster as you swing with both weapons." ); break;
						case 25: myIcon = 0x2D;		myAttack = "skl.ability.earth_strike";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes the weapon to do an extra amount of physical damage." ); break;
						case 26: myIcon = 0x2E;		myAttack = "skl.ability.elemental_strike";			myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes the weapon to do an extra amount of damage among fire, cold, energy, and poison." ); break;
						case 27: myIcon = 0x30;		myAttack = "skl.ability.feint";						myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Gain a defensive advantage over your primary opponent for a short time." ); break;
						case 28: myIcon = 0x3E9;	myAttack = "skl.ability.fire_strike";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes the weapon to do an extra amount of fire damage." ); break;
						case 29: myIcon = 0x3EA;	myAttack = "skl.ability.fists_of_fury";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Attacks with both fists with much more effectiveness." ); break;
						case 30: myIcon = 0x3EB;	myAttack = "skl.ability.force_arrow";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "The archer focuses their will into an arrow of pure force, dazing their enemy." ); myDescribe2 = StringCatalog.Resolve( from.Account, "Dazed enemies are temporarily easier to hit, and sometimes forget who they are attacking." ); break;
						case 31: myIcon = 0x3E8;	myAttack = "skl.ability.force_of_nature";			myMana = 40; myDescribe1 = StringCatalog.Resolve( from.Account, "Infuses the attacker with Nature's Fury. This power causes leafy vines to erupt" ); myDescribe2 = StringCatalog.Resolve( from.Account, "from beneath the attacker's skin, dealing physical and poison damage to them." ); break;
						case 32: myIcon = 0x3EC;	myAttack = "skl.ability.freeze_strike";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes the weapon to do an extra amount of cold damage." ); break;
						case 33: myIcon = 0x3ED;	myAttack = "skl.ability.frenzied_whirlwind";		myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "A quick attack to all enemies in range of your weapon that causes damage over time." ); break;
						case 34: myIcon = 0x3EE;	myAttack = "skl.ability.greater_magic_protection";	myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Allows you to absorb a great amount of magical energy." ); break;
						case 35: myIcon = 0x3EF;	myAttack = "skl.ability.greater_melee_protection";	myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Allows you to absorb a great amount of physical damage." ); break;
						case 36: myIcon = 0x3F0;	myAttack = "skl.ability.infectious_strike";			myMana = 15; myDescribe1 = StringCatalog.Resolve( from.Account, "Attempts to apply a poisoned weapon's poison to the Target. The higher the Poisoning skill of the" ); myDescribe2 = StringCatalog.Resolve( from.Account, "user, the higher the chance the strength of the applied Poison will be increased by one." ); break;
						case 37: myIcon = 0x3F1;	myAttack = "skl.ability.lightning_arrow";			myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "A charged arrow that arcs lightning into its target's allies." ); break;
						case 38: myIcon = 0x3F2;	myAttack = "skl.ability.lightning_strike";			myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes the weapon to do an extra amount of energy damage." ); break;
						case 39: myIcon = 0x3F3;	myAttack = "skl.ability.magic_protection";			myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Allows you to absorb a good amount of magical energy." ); break;
						case 40: myIcon = 0x3F4;	myAttack = "skl.ability.melee_protection";			myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Allows you to absorb a good amount of physical damage." ); break;
						case 41: myIcon = 0x3F5;	myAttack = "skl.ability.mortal_strike";				myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Prevents the Target from being healed by any means for a few Seconds." ); myDescribe2 = StringCatalog.Resolve( from.Account, "This effect does not stop a Target from regenerating hit points." ); break;
						case 42: myIcon = 0x3F6;	myAttack = "skl.ability.moving_shot";				myMana = 15; myDescribe1 = StringCatalog.Resolve( from.Account, "Allows archer to fire an arrow or bolt while moving." ); myDescribe2 = StringCatalog.Resolve( from.Account, "Normally an Archer must be Stationary to fire at a Target." ); break;
						case 43: myIcon = 0x3F7;	myAttack = "skl.ability.nerve_strike";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Does damage and paralyzes your opponent for a short time." ); break;
						case 44: myIcon = 0x3F8;	myAttack = "skl.ability.paralyzing_blow";			myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Paralyzes the target for a few seconds." ); break;
						case 45: myIcon = 0x3F9;	myAttack = "skl.ability.psychic_attack";			myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Temporarily enchants the attacker's weapon with deadly psychic energy," ); myDescribe2 = StringCatalog.Resolve( from.Account, "allowing it to damage the defender's mind and their ability to inflict damage with magic." ); break;
						case 46: myIcon = 0x3FA;	myAttack = "skl.ability.riding_attack";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Gives your attacks on horseback a much more deadly effect." ); break;
						case 47: myIcon = 0x3FB;	myAttack = "skl.ability.riding_swipe";				myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "If you are on foot, dismounts your opponent and damages the ethereal's rider or the live mount" ); myDescribe2 = StringCatalog.Resolve( from.Account, "(which must be healed before ridden again)." ); break;
						case 48: myIcon = 0x3FC;	myAttack = "skl.ability.serpent_arrow";				myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Fires a snake at the target, poisoning them in addition to normal damage with a successful hit." ); myDescribe2 = StringCatalog.Resolve( from.Account, "The archer must be skilled in poisoning and nimble of hand to achieve success." ); break;
						case 49: myIcon = 0x3FD;	myAttack = "skl.ability.shadow_infectious_strike";	myMana = 25; myDescribe1 = StringCatalog.Resolve( from.Account, "Attempts to apply a poisoned weapon's poison to the Target when sneaking and hidden. The higher the" ); myDescribe2 = StringCatalog.Resolve( from.Account, "Poisoning skill of the user, the higher the chance the strength of the applied Poison will be increased by one." ); break;
						case 50: myIcon = 0x3FE;	myAttack = "skl.ability.shadow_strike";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "This attack does moderate extra Damage to the Target and immediately hides the" ); myDescribe2 = StringCatalog.Resolve( from.Account, "user. In order to attempt a Shadowstrike you must have a high amount of the Stealth skill." ); break;
						case 51: myIcon = 0x3FF;	myAttack = "skl.ability.spin_attack";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes one to spin around really fast, hitting multiple times with their weapon." ); break;
						case 52: myIcon = 0x400;	myAttack = "skl.ability.stunning_strike";			myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "One hit with a weapon will be seriously stunned." ); break;
						case 53: myIcon = 0x401;	myAttack = "skl.ability.talon_strike";				myMana = 30; myDescribe1 = StringCatalog.Resolve( from.Account, "Attack with increased damage with additional damage over time." ); break;
						case 54: myIcon = 0x402;	myAttack = "skl.ability.toxic_strike";				myMana = 20; myDescribe1 = StringCatalog.Resolve( from.Account, "Causes the weapon to do an extra amount of poison damage." ); break;
						case 55: myIcon = 0x403;	myAttack = "skl.ability.whirlwind_attack";			myMana = 15; myDescribe1 = StringCatalog.Resolve( from.Account, "Attacks all valid Targets within a one tile radius of the attacker." ); break;
					}

					myAttack = StringCatalog.ResolveByKey( from.Account, myAttack );
					AddPage( nPage ); 

					AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, (nPage+1) );
					AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, (nPage-1) ); 

					int mana = CalculateMana( from, myMana );

					sMana = mana.ToString();

					AddImage(296, 95, 0x5DD0);
					AddImage(296, 95, myIcon);
					AddHtml( 137, 113, 134, 44, @"" + myAttack + "", (bool)false, (bool)false);
					AddHtml( 347, 115, 80, 19, StringCatalog.ResolveFormatByKey( from.Account, "skl.ability.mana_format", sMana ), (bool)false, (bool)false);
					AddHtml( 137, 160, 131, 99, @"" + myDescribe1 + "", (bool)false, (bool)false);
					AddHtml( 297, 145, 131, 121, @"" + myDescribe2 + "", (bool)false, (bool)false);
				}

				AddPage(62);
				AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, 63 );
				AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, 61 );
				AddHtml( 135, 108, 139, 163, StringCatalog.Resolve( from.Account, "There are some other ways to use these the command '[sad' will open the toolbar, in case you close the toolbar yourself. Of course it will reopen if" ), (bool)false, (bool)false);
				AddHtml( 291, 108, 139, 163, StringCatalog.Resolve( from.Account, "you simply un-equip and re-equip the weapon again. If you want to create macros to initiate these abilities, you will simply need to use the '[set1', '[set2'," ), (bool)false, (bool)false);

				AddPage(63);
				AddButton( 401, 87, 0x89E, 0x89E, 18, GumpButtonType.Page, 1 );
				AddButton( 129, 87, 0x89D, 0x89D, 19, GumpButtonType.Page, 62 );
				AddHtml( 135, 108, 139, 163, StringCatalog.Resolve( from.Account, "'[set3', '[set4', or '[set5' commands...depending on which ability you want to use. This book is also for reference only, and you do not need to carry it to use these" ), (bool)false, (bool)false);
				AddHtml( 291, 108, 139, 163, StringCatalog.Resolve( from.Account, "special abilities." ), (bool)false, (bool)false);
			}
		}

		public static int CalculateMana( Mobile from, int Power )
		{
			int mana = Power;

			double skillTotal = GetSkill( from, SkillName.Swords ) + GetSkill( from, SkillName.Bludgeoning )
				+ GetSkill( from, SkillName.Fencing ) + GetSkill( from, SkillName.Marksmanship ) + GetSkill( from, SkillName.Parry )
				+ GetSkill( from, SkillName.Lumberjacking ) + GetSkill( from, SkillName.Stealth )
				+ GetSkill( from, SkillName.Poisoning ) + GetSkill( from, SkillName.Bushido ) + GetSkill( from, SkillName.Ninjitsu );

			if ( skillTotal >= 300.0 )
				mana -= 10;
			else if ( skillTotal >= 200.0 )
				mana -= 5;

			double scalar = 1.0;
			if ( !Server.Spells.Necromancy.MindRotSpell.GetMindRotScalar( from, ref scalar ) )
				scalar = 1.0;

			// Lower Mana Cost = 40%
			int lmc = Math.Min( AosAttributes.GetValue( from, AosAttribute.LowerManaCost ), 40 );

			scalar -= (double)lmc / 100;
			mana = (int)(mana * scalar);

			return mana;
		}

		public static double GetSkill( Mobile from, SkillName skillName )
		{
			Skill skill = from.Skills[skillName];

			if ( skill == null )
				return 0.0;

			return skill.Value;
		}

		public override void OnDoubleClick( Mobile e )
		{
			e.SendGump( new AbilityBookGump( e ) );
		}

		public WeaponAbilityBook(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
