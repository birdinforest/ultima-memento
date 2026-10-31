using Server.Mobiles;
using Server.Gumps;
using Server.Misc.Cache;
using System.Text;
using System.Globalization;
using Server.Engines.MLQuests;
using System.Linq;
using Server.Engines.MLQuests.Definitions;

namespace Server.Items
{
	[Flipable(0x577B, 0x577C)]
	public class StealingBoard : Item
	{
		[Constructable]
		public StealingBoard( ) : base( 0x577B )
		{
			Weight = 1.0;
			Name = "Stealing the Past";
			Hue = 0xAEA;
		}

		public override void OnDoubleClick( Mobile e )
		{
			PlayerMobile pm = (PlayerMobile)e;

			if ( pm.NpcGuild != NpcGuild.ThievesGuild )
			{
				e.SendMessage( "This board seems to be written in some sort of thieves' cant." );
			}
			else if ( e.InRange( this.GetWorldLocation(), 4 ) )
			{
				e.CloseGump( typeof( BoardGump ) );
				
				var builder = new StringBuilder();
				builder.Append("There are those with great wealth that seek items that have been rumored to lie within the dungeons of the land. These nobles seek a crafty thief to sneak into these places and find these items. Speak with the Thief Guildmaster to take the job. Bring each prize back in the order listed, mark it as a quest item, and collect your fee when you deliver it.<br><br>");

				builder.Append("The Guildmaster only commissions one relic at a time. These rare items might not always be there. A better thief may have beaten you to it, and you may be forced to wait for it to reappear again. So time is of the essence if you want to profit from the eagerness of royalty.");

				var questContext = MLQuestSystem.GetContext(pm);
				if (questContext != null)
				{
					var entries = MLQuestSystem.Quests.Values
						.Where(quest => quest is StealingThePastQuest && questContext.HasDoneQuest(quest))
						.Cast<StealingThePastQuest>()
						.Select(quest => StealableArtifactsSpawner.Entries.FirstOrDefault(entry => entry.Type == quest.ItemType))
						.Where(entry => entry != null)
						.ToList();
					
					if (entries.Any())
					{
						builder.Append("<br><br>");
						builder.Append("You have stolen the following relics:");
					}

					var textInfo = new CultureInfo("en-US", false).TextInfo;
					foreach (var entry in entries)
					{
						var snapshot = ItemSnapshotCache.GetOrCreate(entry.Type);
						if (snapshot == null) continue;

						builder.Append("<br><br>");
						builder.Append(string.Format("- {0}<br>", textInfo.ToTitleCase(snapshot.NameNonNormalized)));
						var reg = Region.Find( entry.Location, entry.Map );
						builder.Append(string.Format("   {0}<br>", reg.Name)); 
						builder.Append(string.Format("   {0}", Server.Lands.LandName( Server.Lands.GetLand( entry.Map, entry.Location, entry.Location.X, entry.Location.Y ) )));
					}
				}

				e.SendGump( new BoardGump( e, "STEALING THE PAST", builder.ToString(), "#deb7e2", true ) );
			}
			else
			{
				e.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

		public StealingBoard(Serial serial) : base(serial)
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