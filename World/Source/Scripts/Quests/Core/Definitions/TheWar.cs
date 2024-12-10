using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    #region Quests

    public class SupportingTheWarQuest : MLQuest
    {
        public SupportingTheWarQuest()
        {
            Activated = true;
            HasRestartDelay = true; // This quest is meant to be repeated
            Title = "I Want You!";
            Description = "Description SupportingTheWarQuest";
            RefusalMessage = "RefusalMessage SupportingTheWarQuest";
            InProgressMessage = "InProgressMessage SupportingTheWarQuest";
            CompletionMessage = "CompletionMessage SupportingTheWarQuest";

            Objectives.Add(new DummyObjective("Collect the following:"));
            Objectives.Add(new CollectObjective(5, typeof(RingmailChest), "Ringmail Tunic"));

            Rewards.Add(new ConstructibleItemReward("3 New Weapon Recipes",
                player =>
                {
                    return DefBlacksmithy.CraftSystem.GetRandomRecipeScrolls(player, 3, recipe => !player.HasRecipe(recipe) && DefBlacksmithy.CraftSystem.IsWeaponRecipe(recipe));
                })
            );
        }

        public override IEnumerable<Type> GetQuestGivers()
        {
            yield return typeof(WarEffortRecruiter);  // Quest Giver & Recipient
        }

        public override void Generate()
        {
            base.Generate();

            // TODO: Place one in all Towns
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "WarEffortRecruiter"), new Point3D(2999, 1062, 0), Map.Sosaria);
        }

        public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
        {
            if (!base.CanOffer(quester, pm, context, message)) return false;

            return DefBlacksmithy.CraftSystem.NeedsWeaponRecipe(pm);
        }

        public override TimeSpan GetRestartDelay()
        {
            return TimeSpan.FromHours(23);
        }
    }

    #endregion

    #region Mobiles

    public class WarEffortRecruiter : BasePerson
    {
        public override bool CanShout { get { return true; } }

        [Constructable]
        public WarEffortRecruiter() : base()
        {
            Name = "Local Recruiter";
            AI = AIType.AI_Citizen;
            FightMode = FightMode.None;
            Blessed = true;
            CantWalk = true;

            if (Female = Utility.RandomBool())
            {
                Body = 0x191;
                Utility.AssignRandomHair(this);
                HairHue = Utility.RandomHairHue();
            }
            else
            {
                Body = 0x190;
                Utility.AssignRandomHair(this);
                int HairColor = Utility.RandomHairHue();
                FacialHairItemID = Utility.RandomList(0, 8254, 8255, 8256, 8257, 8267, 8268, 8269);
                HairHue = HairColor;
                FacialHairHue = HairColor;
            }

            Server.Misc.MorphingTime.RemoveMyClothes(this);
            Server.Misc.IntelligentAction.DressUpFighters(this, "", false, false, true);
            Title = ""; // Clear the Title
        }

        public WarEffortRecruiter(Serial serial) : base(serial)
        {
        }

        public override void Shout(PlayerMobile pm)
        {
            string message;
            switch (Utility.RandomMinMax(1, 4))
            {
                default:
                case 1: message = "Oi! You there! Yes, you! I need hands, skilled and steady!"; break;
                case 2: message = "We're in need of armor, strong and ready! The war's not waiting, and neither are we! If you've got the craft in your blood, the forge needs you now!"; break;
                case 3: message = "Steel your nerves and your hearts! We're building for the fight of our lives—get to the forge, and do your part!"; break;
                case 4: message = "The need's great, and the pay's fair!"; break;
            }

            MLQuestSystem.Tell(this, pm, message);
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


    #endregion
}