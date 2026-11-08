using System;
using System.Collections.Generic;
using System.Text;
using Server.ContextMenus;
using Server.Engines.Events.Christmas;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
	#region Quests

	/// <summary>
	/// Midwinter storyline that introduces the Cu Sidhe (fairy wolf of the Sidhe Otherworld).
	/// Available during December via Eira of the Winter Court in Britain.
	/// </summary>
	public class HowlOfTheCuSidheQuest : MLQuest
	{
		public HowlOfTheCuSidheQuest()
		{
			Activated = true;
			OneTimeOnly = false;
			HasRestartDelay = true;

			Title = "Howl of the Cu Sidhe";

			var builder = new StringBuilder();
			builder.Append("Traveler—stand a moment. Do you hear it?");
			builder.Append("<br><br>");
			builder.Append("When the longest nights settle over Sosaria, the veil between our world and the Sidhe Otherworld grows thin. Through that frost-laced gap walk the Cu Sidhe—fairy wolves of pearl and storm, heralds of the Winter Court.");
			builder.Append("<br><br>");
			builder.Append("One of them has been torn free of the path home. Madness claws at it; its spirit sheds as Winter Spirit Motes wherever blood is spilled beneath December stars. I would see that hound returned—or granted a merciful end.");
			builder.Append("<br><br>");
			builder.Append("Gather the motes as an offering, then seek the Veil-Torn Cu Sidhe in the woods near Yew. When the deed is done, return to me. Those who aid the Winter Court are not forgotten.");
			Description = builder.ToString();

			RefusalMessage = "Then keep your distance from the howling dark. The Court remembers indifference as keenly as courage.";

			builder.Clear();
			builder.Append("The offering is not yet complete.<br><br>");
			builder.Append("- Collect Winter Spirit Motes from creatures slain in midwinter<br>");
			builder.Append("- Mark them as quest items when you are ready<br>");
			builder.Append("- Slay the Veil-Torn Cu Sidhe near Yew<br>");
			builder.Append("- Return to Eira of the Winter Court");
			InProgressMessage = builder.ToString();

			builder.Clear();
			builder.Append("The howl softens. Somewhere beyond the veil, a white wolf finds its path again.");
			builder.Append("<br><br>");
			builder.Append("Take these Pads of the Cu Sidhe—boots that remember the Otherworld's snow. With them, even those not of elven blood may earn a fairy hound's trust. Wear also this robe of midwinter, and keep a few motes for the Favor Exchange.");
			builder.Append("<br><br>");
			builder.Append("When December winds rise again, listen. The Court may yet have need of you.");
			CompletionMessage = builder.ToString();

			Objectives.Add(new DummyObjective("Aid the Winter Court:"));
			Objectives.Add(new CollectObjective(20, typeof(WinterSpiritMote), "Winter Spirit Motes"));
			Objectives.Add(new KillObjective(1, new Type[] { typeof(VeilTornCuSidhe) }, "Veil-Torn Cu Sidhe"));
			Objectives.Add(new DummyObjective(""));
			Objectives.Add(new DummyObjective("* Click yourself to view your Quest Log"));

			Rewards.Add(new ItemReward("Pads of the Cu Sidhe", typeof(Artifact_PadsOfTheCuSidhe)));
			Rewards.Add(new ItemReward("Christmas Robe", typeof(ChristmasRobe)));
			Rewards.Add(new ItemReward("10 Winter Spirit Motes", typeof(WinterSpiritMote), 10));
		}

		public override IEnumerable<Type> GetQuestGivers()
		{
			yield return typeof(EiraOfTheWinterCourt);
		}

		public override void Generate()
		{
			base.Generate();

			// Britain — near the town well / midwinter gathering place
			PutSpawner(new Spawner(1, 5, 10, 0, 0, "EiraOfTheWinterCourt"), new Point3D(3005, 1055, 0), Map.Sosaria);

			// Yew woods — veil-torn fairy wolf
			PutSpawner(new Spawner(1, 5, 10, 0, 4, "VeilTornCuSidhe"), new Point3D(2485, 945, 0), Map.Sosaria);
			PutSpawner(new Spawner(1, 5, 10, 0, 4, "VeilTornCuSidhe"), new Point3D(2420, 980, 0), Map.Sosaria);
		}

		public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
		{
			if (!base.CanOffer(quester, pm, context, message))
				return false;

			if (!ChristmasEvent.IsActive)
			{
				if (message)
					MLQuestSystem.Tell(quester, pm, "The Midwinter Court walks only when December crowns the calendar. Until then, I keep only the Favor Exchange.");
				return false;
			}

			return true;
		}

		public override TimeSpan GetRestartDelay()
		{
			return TimeSpan.FromDays(7);
		}
	}

	#endregion

	#region Mobiles

	public class EiraOfTheWinterCourt : BasePerson
	{
		public override bool CanShout { get { return ChristmasEvent.IsActive; } }
		public override bool IsInvulnerable { get { return true; } }
		public override bool AlwaysAttackable { get { return false; } }

		[Constructable]
		public EiraOfTheWinterCourt()
		{
			Name = "Eira";
			Title = "of the Winter Court";
			Female = true;
			Body = 0x191;
			Hue = 0x47E;
			HairItemID = 0x203C;
			HairHue = 1150;

			AI = AIType.AI_Citizen;
			FightMode = FightMode.None;
			Blessed = true;
			CantWalk = true;

			Server.Misc.MorphingTime.RemoveMyClothes(this);

			// Avoid ChristmasRobe here — its OnEquip recolors the wearer's body hue.
			AddItem(new FancyDress() { Hue = 1153 });
			AddItem(new Cloak() { Hue = 1150 });
			AddItem(new Boots() { Hue = 1150 });
		}

		public EiraOfTheWinterCourt(Serial serial) : base(serial)
		{
		}

		public override void Shout(PlayerMobile pm)
		{
			string message;
			switch (Utility.RandomMinMax(1, 4))
			{
				default:
				case 1: message = "Do you hear the midwinter howl? The Cu Sidhe walks between worlds!"; break;
				case 2: message = "Winter Spirit Motes cling to the fallen—bring them to me!"; break;
				case 3: message = "Aid the Winter Court, and the fairy hound may yet find its way home."; break;
				case 4: message = "December's veil thins. Will you stand with the Sidhe?"; break;
			}

			MLQuestSystem.Tell(this, pm, message);
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.AddCustomContextEntries(from, list);

			if (from.Alive && from is PlayerMobile && CanSee(from) && InLOS(from))
				list.Add(new RedeemMidwinterFavorsEntry((PlayerMobile)from, this));
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			base.OnSpeech(e);

			if (e.Handled || !(e.Mobile is PlayerMobile) || !e.Mobile.Alive)
				return;

			if (!e.Mobile.InRange(this, 6) || !CanSee(e.Mobile) || !InLOS(e.Mobile))
				return;

			string speech = (e.Speech ?? string.Empty).ToLowerInvariant();

			if (speech.IndexOf("redeem") >= 0 || speech.IndexOf("favor") >= 0 || speech.IndexOf("mote") >= 0 || speech.IndexOf("exchange") >= 0 || speech.IndexOf("buy") >= 0)
			{
				e.Handled = true;
				SayTo(e.Mobile, "Offer your Winter Spirit Motes, and choose a gift of the season.");
				e.Mobile.SendGump(new YuletideRedeemGump(e.Mobile));
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);
			list.Add("Midwinter Favor Exchange");
			if (ChristmasEvent.IsActive)
				list.Add("Quest: Howl of the Cu Sidhe");
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		private class RedeemMidwinterFavorsEntry : ContextMenuEntry
		{
			private readonly PlayerMobile m_From;
			private readonly EiraOfTheWinterCourt m_Eira;

			public RedeemMidwinterFavorsEntry(PlayerMobile from, EiraOfTheWinterCourt eira) : base(6103, 6) // "Browse"
			{
				m_From = from;
				m_Eira = eira;
			}

			public override void OnClick()
			{
				if (m_From == null || m_Eira == null || m_Eira.Deleted)
					return;

				if (!m_From.InRange(m_Eira, 6))
				{
					m_From.SendLocalizedMessage(500446); // That is too far away.
					return;
				}

				m_Eira.SayTo(m_From, "Offer your Winter Spirit Motes, and choose a gift of the season.");
				m_From.SendGump(new YuletideRedeemGump(m_From));
			}
		}
	}

	/// <summary>
	/// A Cu Sidhe stranded on the wrong side of the midwinter veil—hostile, untamable, story foe.
	/// </summary>
	[CorpseName("a Veil-Torn Cu Sidhe corpse")]
	public class VeilTornCuSidhe : BaseCreature
	{
		[Constructable]
		public VeilTornCuSidhe() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a Veil-Torn Cu Sidhe";
			Body = 277;
			BaseSoundID = 0x577;
			Hue = 1150;

			SetStr(900, 1100);
			SetDex(140, 170);
			SetInt(200, 250);

			SetHits(800, 1000);

			SetDamage(18, 26);

			SetDamageType(ResistanceType.Physical, 20);
			SetDamageType(ResistanceType.Cold, 50);
			SetDamageType(ResistanceType.Energy, 30);

			SetResistance(ResistanceType.Physical, 45, 60);
			SetResistance(ResistanceType.Fire, 20, 35);
			SetResistance(ResistanceType.Cold, 70, 90);
			SetResistance(ResistanceType.Poison, 30, 45);
			SetResistance(ResistanceType.Energy, 60, 80);

			SetSkill(SkillName.FistFighting, 85.0, 100.0);
			SetSkill(SkillName.Tactics, 85.0, 100.0);
			SetSkill(SkillName.MagicResist, 70.0, 90.0);
			SetSkill(SkillName.Anatomy, 60.0, 80.0);

			Fame = 8000;
			Karma = -4000;

			Tamable = false;
			ControlSlots = 0;

			PackGold(400, 700);

			if (Utility.RandomDouble() < 0.35)
				PackItem(new WinterSpiritMote(Utility.RandomMinMax(3, 8)));
		}

		public VeilTornCuSidhe(Serial serial) : base(serial)
		{
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.MonsterFilthyRich, 3);
		}

		public override int Meat { get { return 3; } }
		public override int Hides { get { return 10; } }

		public override int GetIdleSound() { return 0x577; }
		public override int GetAttackSound() { return 0x576; }
		public override int GetAngerSound() { return 0x578; }
		public override int GetHurtSound() { return 0x576; }
		public override int GetDeathSound() { return 0x579; }

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			if (ChristmasEvent.IsActive && c != null && Utility.RandomDouble() < 0.5)
				c.DropItem(new WinterSpiritMote(Utility.RandomMinMax(5, 12)));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}

	#endregion
}
