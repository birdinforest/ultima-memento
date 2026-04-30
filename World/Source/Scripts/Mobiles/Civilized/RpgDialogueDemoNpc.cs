using Server.Engines.MLQuests;
using Server.Engines.RpgDialogue;
using Server.Items;

namespace Server.Mobiles
{
	/// <summary>
	/// Demo quest-giver for <see cref="RpgDialogueGump"/> (RPG-style branching dialogue, not ML Quest).
	/// Spawn in-game as [add RpgDialogueDemoNpc or [tile RpgDialogueDemoNpc.
	/// </summary>
	[CorpseName("Eldrin's corpse")]
	public class RpgDialogueDemoNpc : BaseCreature
	{
		[Constructable]
		public RpgDialogueDemoNpc()
			: base(AIType.AI_Vendor, FightMode.None, 0, 0, 0.0, 0.0)
		{
			SpeechHue = 0x3B2;

			Name = "Eldrin";
			Title = "the Dialogue Herald";
			Female = true;
			Body = 0x191;
			Hue = Utility.RandomSkinHue();

			InitStats(100, 25, 10);
			SetSkill(SkillName.MagicResist, 40.0, 55.0);

			AddItem(new PlainDress { Hue = Utility.RandomNeutralHue() });
			AddItem(new Sandals());
		}

		public RpgDialogueDemoNpc(Serial serial)
			: base(serial)
		{
		}

		public override bool ClickTitle
		{
			get { return true; }
		}

		public override void OnDoubleClick(Mobile from)
		{
			PlayerMobile pm = from as PlayerMobile;

			if (pm != null && pm.Alive && !Deleted && from.InRange(Location, 3) && from.CanSee(this))
			{
				MLQuestSystem.TurnToFace(this, pm);
				pm.SendGump(new RpgDialogueGump(this, pm, RpgDialogueScripts.Demo, "start"));
				return;
			}

			if (pm != null && !Deleted && !from.InRange(Location, 3))
				pm.SendMessage("You are too far away to speak with Eldrin.");

			base.OnDoubleClick(from);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
