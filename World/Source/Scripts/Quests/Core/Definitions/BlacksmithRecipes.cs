using System;
using System.Collections.Generic;
using System.Text;
using Server.Engines.Craft;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    #region Quests

    /*
    [go 2981 1023
    [go 3155 2600
    [go 1612 1451
    [go 2478 890
    [go 856 712
    [go 917 2097
    */

    public class HintRingArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }
        public override Type NextQuest { get { return typeof(RingArmorQuest); } }

        public HintRingArmorQuest()
        {
            Activated = true;
            Title = "Delivery: The Hammer and Anvil";
            Description = "TODO: Deliver this to the Britain Smith";
            Objectives.Add(new DeliverObjective(typeof(BlacksmithDeliveryCrate), 1, typeof(BritainGuildmasterSmithGuy)));
            Objectives.Add(new DummyObjective("- Blacksmith Crate"));
            Objectives.Add(new DummyObjective("- Location: Britain"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- Traveling the lands by foot is dangerous"));
            Objectives.Add(new DummyObjective("- Use the public moongates to minimize some time and hazard"));
            Objectives.Add(new DummyObjective("- Humans can ride horses"));
            Objectives.Add(new DummyObjective("- Non-human races can purchase 'Hiking Boots' from the Cobbler"));

            Rewards.Add(new ItemReward("Gold Coins", typeof(Gold), 300));
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield break;
        }
    }

    public class RingArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return false; } } // Hint quest is optional
        public override Type NextQuest { get { return typeof(HintChainArmorQuest); } } // Optional delivery

        public RingArmorQuest()
        {
            Activated = true;
            Title = "The Hammer's Return";
            var builder = new StringBuilder();
            builder.Append("The Smith looks busy and irritated as you approach. \"Teach you, ha! Not with MY customers at stake. Of course, I'm a Guildmaster - but do you think we just let anyone who asks into the guild?\"");
            builder.Append("<br><br>");
            builder.Append("The Smith points a finger at the display cases over yonder. \"Make me something useful first. You CAN make something useful, right?\" The Smith stares at you and waits impatiently for your stammering reply.");
            builder.Append("<br><br>");
            builder.Append("\"Look, I've no time for another apprentice. I've got some notes on the basics of ringmail armor that you can pore over, but you'll have to make it worth my while first.\" ");
            builder.Append("<br><br>");
            builder.Append("The Smith quickly scans the shelves and crates before acknowledging you again. \"I'm running low on shield inventory and empty stock makes for unhappy customers when they come knockin'.\"");
            builder.Append("<br><br>");
            builder.Append("\"Try your hand at a bunch of Tear Kite Shields and I'll see what's good enough to sell.\" With that, the Smith turns away and returns to work.");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("You do your best to flag down the busy Smith and finally succeed.");
            builder.Append("<br><br>");
            builder.Append("\"Alrighty, let's see what you've come up with.\" You present your wares proudly as the Smith dons some spectacles. He swiftly grades the shields for quality: rapping them inside and out, peering at the stud work, and studying the metal form.");
            builder.Append("<br><br>");
            builder.Append("As the sellable product is sorted into piles, the Smith looks up, slightly surprised to see you still standing there. \"Hmm? What is it now?\" As you begin to request the aforementioned ringmail documents, you are interrupted.");
            builder.Append("<br><br>");
            builder.Append("\"Ah, yes. Those silly papers.\" The Smith shakes his head and walks over to the nearby shelf.");
            builder.Append("<br><br>");
            builder.Append("\"...Here.\" You quickly snatch the bag as he tosses it at your head. The Smith ignores you as he resumes work in his shop.");
            CompletionMessage = builder.ToString();

            Objectives.Add(DummyObjective.CraftAndMarkQuestItems);
            Objectives.Add(new CraftObjective(20, typeof(WoodenKiteShield), "Kite Shield"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("* Click yourself to view your Quest Log"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- Explore the \"Help\" menu from your Paperdoll"));
            Objectives.Add(new DummyObjective("- Exceptionally crafted items are slightly magical"));
            Objectives.Add(new DummyObjective("- Arms Lore will increase the value of your exceptionally crafted items"));

            Rewards.Add(new ConstructibleItemReward("Ringmail Armor Recipes",
                player =>
                {
                    return DefBlacksmithy.CraftSystem.GetRecipeScrolls(
                        player,
                        typeof(RingmailGloves),
                        typeof(RingmailLegs),
                        typeof(RingmailArms),
                        typeof(RingmailChest),
                        typeof(RingmailSkirt)
                    );
                })
            );
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(BritainGuildmasterSmithGuy);  // Quest Giver & Recipient
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "BritainGuildmasterSmithGuy"), new Point3D(2981, 1023, 0), Map.Sosaria);
        }
    }

    public class HintChainArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }
        public override Type NextQuest { get { return typeof(ChainArmorQuest); } }

        public HintChainArmorQuest()
        {
            Activated = true;
            Title = "Delivery: Metals of Montor";
            var builder = new StringBuilder();
            builder.Append("\"You again? Still looking to hone your craft, eh? No, no, we're too busy here.\" Intentional chatter fills the air - the shop is bustling. Apprentices move about addressing customer requests, mending armor, and sharpening weapons. You briefly step to the side as someone shoulders past.");
            builder.Append("<br><br>");
            builder.Append("\"Here, I've got an idea that should satisfy us both.\" Clang! A muffled jingle of metal smacking together jars you as the Smith plops a heavy crate down on the floor in front of you.");
            builder.Append("<br><br>");
            builder.Append("\"Take this package due south to the Montor blacksmith and I'm sure someone there will be delighted to teach you something more useful.\"");
            builder.Append("<br><br>");
            builder.Append("The Smith turns away, already forgetting you and talking to himself, \"...cheaper than whatever animals they decide to use as couriers these days at Beasts of Burden...those prices are highway robbery...whetstone wheel acting up again...\"");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("\"Ay', mate! I been expectin' d'ose!\" The Smith stops what she's doing to grab the crate right out of your hands.");
            builder.Append("<br><br>");
            builder.Append("She sets the crate down [clang!], flips through a few items [ting...ting...ting...], and rummages through to find a shield and katana. She eyes each of her prizes proudly and grins.");
            builder.Append("<br><br>");
            builder.Append("\"About time!\" Swoosh! Swoosh! You quickly step back, dodging wild swings, and eye her with bewilderment. The Smith giggles to herself. \"Ma always said I was safer swingin' hammers!\"");
            builder.Append("<br><br>");
            builder.Append("\"Ye have my d'anks!\"");
            CompletionMessage = builder.ToString();

            Objectives.Add(new DeliverObjective(typeof(BlacksmithDeliveryCrate), 1, typeof(MontorSmithGirl)));
            Objectives.Add(new DummyObjective("- Blacksmith Crate"));
            Objectives.Add(new DummyObjective("- Location: Montor"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- Traveling the lands by foot is dangerous"));
            Objectives.Add(new DummyObjective("- Use the public moongates to minimize some time and hazard"));
            Objectives.Add(new DummyObjective("- Humans can ride horses"));
            Objectives.Add(new DummyObjective("- Non-human races can purchase 'Hiking Boots' from the Cobbler"));

            Rewards.Add(new ItemReward("Gold Coins", typeof(Gold), 300));
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield break;
        }
    }

    public class ChainArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return false; } } // Hint quest is optional
        public override Type NextQuest { get { return typeof(HintPlateArmorQuest); } }

        public ChainArmorQuest()
        {
            Activated = true;
            Title = "The Heart of the Forge";
            var builder = new StringBuilder();
            builder.Append("\"Oi! M'good pal! M'good buddy! M'good friend!\" The Smith sees you enter the shop and a warm smile breaks her face. She immediately approaches you.");
            builder.Append("<br><br>");
            builder.Append("\"What can'r do ye fer?\" She looks at you expectantly and listens to your requests, clapping her hands at last.");
            builder.Append("<br><br>");
            builder.Append("\"Aha! Ye've come to da right joint.\" She walks to her coffer and pulls out a stack of scrolls - waving them at you.");
            builder.Append("<br><br>");
            builder.Append("\"Sure, sure, yer good fer d'ese.\" She waggles a finger at you teasingly, with a sparkle in her eye. \"But only if'n ye prove its,\" she says, as she plops the stack of scrolls back into her coffer.");
            builder.Append("<br><br>");
            builder.Append("\"Chainmail is a might challenge more den ringmail, eh.\" With a grin, she says, \"Show me yer skill with'n da gloves and leggin's!\"");
            Description = builder.ToString();

            Objectives.Add(DummyObjective.CraftAndMarkQuestItems);
            Objectives.Add(new CraftObjective(15, typeof(RingmailGloves), "Ringmail Gloves"));
            Objectives.Add(new CraftObjective(15, typeof(RingmailLegs), "Ringmail Leggings"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- Resistances are vital to your survival"));
            Objectives.Add(new DummyObjective("- Exceptionally crafted armor usually has more total resistances"));
            Objectives.Add(new DummyObjective("- Fistfighting and other weapon skills reduce your chance to be hit"));

            builder.Clear();
            builder.Append("The smith looks up from her work in surprise. \"Done, hm? Swift, yes, yes. I be seein' ye workin'.\"");
            builder.Append("<br><br>");
            builder.Append("\"Give'r 'ere, c'mon. Lemmee see dat.\" She works methodically to drape the gloves out across a few tables and grabs the leggings to hang on a rack. With her pliers suddenly in hand, she pulls on a few (seemingly) random rings, testing for shoddy workmanship.");
            builder.Append("<br><br>");
            builder.Append("\"Fine work, yes! Mmm, tis good.\" She drops the tools, walks away and returns with a not-insignificantly-sized blade in hand. Recalling your last near-miss involving a katana, you take a step back and a knowing grin splits her face. \"I says, yer a quick study.\"");
            builder.Append("<br><br>");
            builder.Append("With a quick snap of the wrist, the blade strikes the ringmail. \"Tend the diff'rence - rings expose more skin!\" Her other hand briefly hovers above the table, her open grasp suddenly releasing a stack of scrolls to scatter chainmail patterns across your view.");
            builder.Append("<br><br>");
            builder.Append("\"Yer ready... good luck.\"");
            CompletionMessage = builder.ToString();

            Rewards.Add(new ConstructibleItemReward("Chain Armor Recipes",
                player =>
                {
                    return DefBlacksmithy.CraftSystem.GetRecipeScrolls(
                        player,
                        typeof(ChainCoif),
                        typeof(ChainLegs),
                        typeof(ChainChest),
                        typeof(ChainSkirt)
                    );
                })
            );
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(MontorSmithGirl);  // Quest Giver & Recipient
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "MontorSmithGirl"), new Point3D(3155, 2600, 5), Map.Sosaria);
        }
    }

    public class HintPlateArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }
        public override Type NextQuest { get { return typeof(PlateArmorQuest); } }

        public HintPlateArmorQuest()
        {
            Activated = true;
            Title = "Delivery: Forged Iron";

            var builder = new StringBuilder();
            builder.Append("\"Plate mail, hmmm?! If'n yer up fer rivalin' me, ye betta step it up. Yer good, but not dat good.\" The smith pokes you in the chest with a gnarly finger and raises an eyebrow at you. She turns away briefly and then spreads map out on the table.");
            builder.Append("<br><br>");
            builder.Append("\"Time fer findin' safety in da mountains - Devil Guard lies northwest o' here. Take me goods with ye.\" She stuffs a crate in your arms and smacks you on the butt.");
            builder.Append("<br><br>");
            builder.Append("\"Get yer tookus movin'!\"");
            builder.Append("<br><br>");
            Description = builder.ToString();

            Objectives.Add(new DeliverObjective(typeof(BlacksmithDeliveryCrate), 1, typeof(DevilGuardSmithGuy)));
            Objectives.Add(new DummyObjective("- Blacksmith Crate"));
            Objectives.Add(new DummyObjective("- Location: Devil Guard"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- Alt+R can open a mini map"));
            Objectives.Add(new DummyObjective("- Use the Top Menu Bar to access a 'World Map'"));
            Objectives.Add(new DummyObjective("- Unlock the world map via right-click -> 'free view' -> left mouse dragging"));
            Objectives.Add(new DummyObjective("- Use map markers to keep track of points of interest"));

            Rewards.Add(new ItemReward("Gold Coins", typeof(Gold), 300));

            builder.Clear();
            builder.Append("\"STOP!\" A stern, but even voice commands you. The blacksmith looks up, staring at you with icy blue eyes, pausing his hammer strokes just long enough to visually scold you. Softly, but with a tone used to being followed, he asserts: \"Wipe your boots at the door.\" *clang clang clang*");
            builder.Append("<br><br>");
            builder.Append("\"Leave the crate beside the scrap iron barrel, and do not disturb a thing in my shop.\" The smith doesn't bother to look up from his work this time.");
            builder.Append("<br><br>");
            builder.Append("");
            builder.Append("<br><br>");
            CompletionMessage = builder.ToString();
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield break;
        }
    }

    public class PlateArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return false; } } // Hint quest is optional
        public override Type NextQuest { get { return typeof(HintAnimalArmorQuest); } }

        public PlateArmorQuest()
        {
            Activated = true;
            Title = "A Test of Strength"; // A Humbling Truth...?

            var builder = new StringBuilder();
            builder.Append("\"What is it that you want?\" The smith softly inquires, noticing that you haven't yet left. Again, the smith doesn't bother to look up from his work. *stroke, stroke, stroke - turn - stroke, stroke, stroke - into the coals*");
            builder.Append("<br><br>");
            builder.Append("You look to his left, mesmerized by the mirror finish on various pieces of platemail hanging beside him. A sniff breaks the spell.");
            builder.Append("<br><br>");
            builder.Append("\"Huh. A fellow tradesman? Yes, I'll teach you, though plate is in less demand 'round these parts. Demonstrate your capability with a chainmail coif and tunic...or 15.\" He finally looks up, a piercing blue gaze daring you to complain.\"");
            builder.Append("<br><br>");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("\"Aye. See my chainmail over yonder?\" With a short motion, the head of his hammer briefly guides your gaze before returning back to the metal. *clang clang clang*");
            builder.Append("<br><br>");
            builder.Append("\"Go, take your wares, and compare your quality against mine. Do. Not. Touch. A. Thing. When you are ready, I wish to see every flaw in your workmanship. You will show me how you have room for improvement in every piece.\"");
            builder.Append("<br><br>");
            builder.Append("...you complete the arduous task, humbled by the methodical activity, but excited by the growth opportunity.");
            builder.Append("<br><br>");
            builder.Append("\"Very well. Take these platemail recipes and have the scribe duplicate them so you have a copy for yourself.\"");
            CompletionMessage = builder.ToString();

            Objectives.Add(DummyObjective.CraftAndMarkQuestItems);
            Objectives.Add(new CraftObjective(15, typeof(ChainCoif), "Chainmail Coif"));
            Objectives.Add(new CraftObjective(15, typeof(ChainChest), "Chainmail Tunic"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- Hit Point Regeneration can help offset damage from being poisoned"));
            Objectives.Add(new DummyObjective("- Stats (Strength, Dexterity, and Intelligence) have a hard cap of 150, regardless of bonuses"));
            Objectives.Add(new DummyObjective("- You can permanently increase Stats by raising your skills"));
            Objectives.Add(new DummyObjective("- You can temporarily increase Stats with potions, spells, and equipment"));

            Rewards.Add(new ConstructibleItemReward("Plate Armor Recipes",
                player =>
                {
                    return DefBlacksmithy.CraftSystem.GetRecipeScrolls(
                        player,
                        typeof(PlateArms),
                        typeof(PlateGloves),
                        typeof(PlateGorget),
                        typeof(PlateLegs),
                        typeof(PlateSkirt),
                        typeof(PlateChest),
                        typeof(FemalePlateChest),
                        typeof(PlateMempo),
                        typeof(PlateDo),
                        typeof(PlateHiroSode),
                        typeof(PlateSuneate),
                        typeof(PlateHaidate)
                    );
                })
            );
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(DevilGuardSmithGuy);  // Quest Giver & Recipient
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "DevilGuardSmithGuy"), new Point3D(1612, 1451, 7), Map.Sosaria);
        }
    }

    public class HintAnimalArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }
        public override Type NextQuest { get { return typeof(AnimalArmorQuest); } }

        public HintAnimalArmorQuest()
        {
            Activated = true;
            Title = "Delivery: The Iron Golem";

            var builder = new StringBuilder();
            builder.Append("\"Alas, I am not the correct mentor for the next step in your crafting journey.\" A surprising silence fills the air, an unfamiliar sound in this shop. The smith stares into the coals for a moment...and then looks up at you.");
            builder.Append("<br><br>");
            builder.Append("\"I suggest you travel northeast, to another town shielded by mountains. Seek out Yew. I am aware of a blacksmith who used to live there. He is...interesting. An excellent craftsman, but with less focus on metals and...humans. Heh.\" The blacksmith chuckles to himself before handing over a crate of blacksmith supplies.");
            builder.Append("<br><br>");
            builder.Append("\"Take this. An offering will improve your odds of success in building this relationship.\"");
            builder.Append("<br><br>");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("");
            builder.Append("<br><br>");
            CompletionMessage = builder.ToString();

            Objectives.Add(new DeliverObjective(typeof(BlacksmithDeliveryCrate), 1, typeof(YewSmithGuy)));
            Objectives.Add(new DummyObjective("- Blacksmith Crate"));
            Objectives.Add(new DummyObjective("- Location: Yew"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- "));

            Rewards.Add(new ItemReward("Gold Coins", typeof(Gold), 300));
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield break;
        }
    }

    public class AnimalArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return false; } } // Hint quest is optional
        public override Type NextQuest { get { return typeof(HintRoyalArmorQuest); } }

        public AnimalArmorQuest()
        {
            Activated = true;
            Title = "The Unbreakable Bond";

            var builder = new StringBuilder();
            builder.Append("");
            builder.Append("<br><br>");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("");
            builder.Append("<br><br>");
            CompletionMessage = builder.ToString();

            Objectives.Add(DummyObjective.CraftAndMarkQuestItems);
            Objectives.Add(new CraftObjective(10, typeof(PlateGorget), "Platemail Gorget"));
            Objectives.Add(new CraftObjective(10, typeof(PlateChest), "Platemail"));

            Rewards.Add(new ConstructibleItemReward("Animal Armor Recipes",
                player =>
                {
                    return DefBlacksmithy.CraftSystem.GetRecipeScrolls(
                        player,
                        typeof(HorseArmor),
                        typeof(DragonBardingDeed)
                    );
                })
            );
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(YewSmithGuy);  // Quest Giver & Recipient
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "YewSmithGuy"), new Point3D(2478, 890, 7), Map.Sosaria);
        }
    }

    public class HintRoyalArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }
        public override Type NextQuest { get { return typeof(RoyalArmorQuest); } }

        public HintRoyalArmorQuest()
        {
            Activated = true;
            Title = "Delivery: Smelted Moon Rocks";

            var builder = new StringBuilder();
            builder.Append("");
            builder.Append("<br><br>");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("");
            builder.Append("<br><br>");
            CompletionMessage = builder.ToString();

            Objectives.Add(new DeliverObjective(typeof(BlacksmithDeliveryCrate), 1, typeof(MoonSmithGuy)));
            Objectives.Add(new DummyObjective("- Blacksmith Crate"));
            Objectives.Add(new DummyObjective("- Location: The city of Moon"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- "));

            Rewards.Add(new ItemReward("Gold Coins", typeof(Gold), 300));
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield break;
        }
    }

    public class RoyalArmorQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return false; } } // Hint quest is optional
        public override Type NextQuest { get { return typeof(HintTridentQuest); } }

        public RoyalArmorQuest()
        {
            Activated = true;
            Title = "Shattered Steel & Broken Bonds";

            var builder = new StringBuilder();
            builder.Append("");
            builder.Append("<br><br>");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("");
            builder.Append("<br><br>");
            CompletionMessage = builder.ToString();

            Objectives.Add(DummyObjective.CraftAndMarkQuestItems);
            Objectives.Add(new CraftObjective(1, typeof(HorseArmor), "Horse Barding"));
            // Objectives.Add(new CraftObjective(1, typeof(DragonBardingDeed), "Dragon Barding"));

            Rewards.Add(new ConstructibleItemReward("Royal Armor Recipes",
                player =>
                {
                    return DefBlacksmithy.CraftSystem.GetRecipeScrolls(
                        player,
                        typeof(RoyalBoots),
                        typeof(RoyalGloves),
                        typeof(RoyalGorget),
                        typeof(RoyalHelm),
                        typeof(RoyalsLegs),
                        typeof(RoyalArms),
                        typeof(RoyalChest)
                    );
                })
            );
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(MoonSmithGuy); // Quest Giver & Recipient
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "MoonSmithGuy"), new Point3D(856, 712, 5), Map.Sosaria);
        }
    }

    public class HintTridentQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }
        public override Type NextQuest { get { return typeof(TridentQuest); } }

        public HintTridentQuest()
        {
            Activated = true;
            Title = "Delivery: The Titan's Helm";

            var builder = new StringBuilder();
            builder.Append("");
            builder.Append("<br><br>");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("");
            builder.Append("<br><br>");
            CompletionMessage = builder.ToString();

            Objectives.Add(new DeliverObjective(typeof(BlacksmithDeliveryCrate), 1, typeof(GreySmithGuy)));
            Objectives.Add(new DummyObjective("- Blacksmith Crate"));
            Objectives.Add(new DummyObjective("- Location: The city of Grey"));
            Objectives.Add(new DummyObjective(""));
            Objectives.Add(new DummyObjective("Tips:"));
            Objectives.Add(new DummyObjective("- "));

            Rewards.Add(new ItemReward("Gold Coins", typeof(Gold), 300));
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield break;
        }
    }

    public class TridentQuest : MLQuest
    {
        public override bool IsChainTriggered { get { return false; } } // Hint quest is optional

        public TridentQuest()
        {
            Activated = true;
            Title = "Forging the Legacy";

            var builder = new StringBuilder();
            builder.Append("");
            builder.Append("<br><br>");
            Description = builder.ToString();

            builder.Clear();
            builder.Append("");
            builder.Append("<br><br>");
            CompletionMessage = builder.ToString();

            Objectives.Add(DummyObjective.CraftAndMarkQuestItems);
            Objectives.Add(new CraftObjective(1, typeof(RoyalBoots), "Royal Boots"));
            Objectives.Add(new CraftObjective(1, typeof(RoyalArms), "Royal Mantle"));

            Rewards.Add(new ConstructibleItemReward("Trident Recipe",
                player =>
                {
                    return DefBlacksmithy.CraftSystem.GetRecipeScrolls(
                        player,
                        typeof(Pitchfork)
                    );
                })
            );
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(GreySmithGuy);  // Quest Giver & Recipient
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "GreySmithGuy"), new Point3D(917, 2097, 5), Map.Sosaria);
        }
    }

    #endregion

    #region Mobiles

    [QuesterName("the Smith Guildmaster in Britain")]
    public class BritainGuildmasterSmithGuy : BlacksmithGuildmaster
    {
        [Constructable]
        public BritainGuildmasterSmithGuy()
        {
        }

        public BritainGuildmasterSmithGuy(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    [QuesterName("the Smith in Montor")]
    public class MontorSmithGirl : Blacksmith
    {
        [Constructable]
        public MontorSmithGirl()
        {
            Body = 0x191;
            Female = true;
            Name = NameList.RandomName("female");
        }

        public MontorSmithGirl(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    [QuesterName("the Smith in Devil Guard")]
    public class DevilGuardSmithGuy : Blacksmith
    {
        [Constructable]
        public DevilGuardSmithGuy()
        {
            // TODO: Male
        }

        public DevilGuardSmithGuy(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    [QuesterName("the Smith in Yew")]
    public class YewSmithGuy : Blacksmith
    {
        [Constructable]
        public YewSmithGuy()
        {
        }

        public YewSmithGuy(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    [QuesterName("the Smith in Moon")]
    public class MoonSmithGuy : Blacksmith
    {
        [Constructable]
        public MoonSmithGuy()
        {
        }

        public MoonSmithGuy(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    [QuesterName("the Smith in Grey")]
    public class GreySmithGuy : Blacksmith
    {
        [Constructable]
        public GreySmithGuy()
        {
        }

        public GreySmithGuy(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    #endregion


    #region Items

    public class BlacksmithDeliveryCrate : Item
    {
        [Constructable]
        public BlacksmithDeliveryCrate() : base(0x4F8D)
        {
            Name = "blacksmith crate";
            Weight = 10.0;
            ResourceMods.DefaultItemHue(this);
        }

        public BlacksmithDeliveryCrate(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    #endregion
}