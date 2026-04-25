using System;
using Server;
using Server.ContextMenus;
using System.Collections;
using System.Collections.Generic;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Mobiles
{
	public class TradesmanBard : Citizens
	{
		[Constructable]
		public TradesmanBard()
		{
			CitizenType = 12;
			SetupCitizen();
			Blessed = true;
			CantWalk = true;
			AI = AIType.AI_Melee;
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
		}

		public override void OnThink()
		{
			if ( DateTime.Now >= m_NextTalk )
			{
				int seconds = Utility.RandomMinMax( 10, 20 );
				BardHit music = new BardHit();
					music.Delete();

				foreach ( Item instrument in this.GetItemsInRange( 1 ) )
				{
					if ( instrument is BardHit )
					{
						if ( this.FindItemOnLayer( Layer.FirstValid ) != null ){ this.FindItemOnLayer( Layer.TwoHanded ).Delete(); }
						else if ( this.FindItemOnLayer( Layer.OneHanded ) != null ){ this.FindItemOnLayer( Layer.TwoHanded ).Delete(); }
						else if ( this.FindItemOnLayer( Layer.TwoHanded ) != null ){ this.FindItemOnLayer( Layer.TwoHanded ).Delete(); }
						music = (BardHit)instrument;
					}
				}

				if ( music.ItemID == 0x27B3 )
				{
					if ( music.X == X ){ Direction = Direction.South; } //music.Y = Y; }
					else if ( music.Y == Y ){ Direction = Direction.East; } //music.X = X; }
					Server.Items.BardHit.SetInstrument( this, music );
				}
				music.OnDoubleClick( this );
				if ( music.Name != "instrument"){ seconds = Utility.RandomMinMax( 5, 10 ); }

				m_NextTalk = (DateTime.Now + TimeSpan.FromSeconds( seconds ));
			}
		}

		public override void OnAfterSpawn()
		{
			base.OnAfterSpawn();
			Server.Misc.TavernPatrons.RemoveSomeGear( this, false );
			Server.Misc.MorphingTime.CheckNecromancer( this );
		}

		public TradesmanBard( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}

namespace Server.Items
{
	public class BardHit : Item
	{
		[Constructable]
		public BardHit() : base( 0x27B3 )
		{
			Name = "instrument";
			Movable = false;
			Weight = -2.0;
			ItemID = 0x27B3;
		}

		public BardHit( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from is TradesmanBard )
			{
				string[] song1 = new string[] {"D","L"};
				string[] song2 = new string[] {"a","e","i","o","u","ee","ah","oo"};

				int lyrics = Utility.RandomMinMax( 8, 15 );
				int chords = 0;
				string sing = "";
				bool added = true;

				while ( lyrics > 0 )
				{
					lyrics--;
					chords++;

					if ( chords > 8 && Utility.RandomBool() ){ added = false; }

					if ( added ){ sing = sing + song1[Utility.RandomMinMax( 0, (song1.Length-1) )] + song2[Utility.RandomMinMax( 0, (song2.Length-1) )] + " "; }

					added = true;
				}

				if ( this.Name != "instrument" )
				{
					if ( this.ItemID == 0x64BE || this.ItemID == 0x64BF ){ 		this.Name = "instrument";	from.PlaySound( Utility.RandomList( 0x4C, 0x403, 0x40B, 0x418 ) ); 	if ( Utility.RandomBool() ){ CitizenLocalization.SayLocalizedComposite( from, sing, null ); } }		// LUTE
					else if ( this.ItemID == 0x64C0 || this.ItemID == 0x64C1 ){ this.Name = "instrument";	from.PlaySound( 0x504 ); }																									// FLUTE
					else if ( this.ItemID == 0x64C2 || this.ItemID == 0x64C3 ){ this.Name = "instrument";	from.PlaySound( Utility.RandomList( 0x043, 0x045 ) ); 	if ( Utility.RandomBool() ){ CitizenLocalization.SayLocalizedComposite( from, sing, null ); } }					// HARP
					else if ( this.ItemID == 0x64C4 || this.ItemID == 0x64C5 ){ this.Name = "instrument";	from.PlaySound( 0x38 ); 	if ( Utility.RandomBool() ){ CitizenLocalization.SayLocalizedComposite( from, sing, null ); } }												// DRUM
					else if ( this.ItemID == 0x64C6 || this.ItemID == 0x64C7 ){ this.Name = "instrument";	from.PlaySound( 0x5B1 ); 	if ( Utility.RandomBool() ){ CitizenLocalization.SayLocalizedComposite( from, sing, null ); } }												// FIDDLE
					else if ( this.ItemID == 0x64C8 || this.ItemID == 0x64C9 ){ this.Name = "instrument";	from.PlaySound( Utility.RandomList( 0x52, 0x4B5, 0x4B6, 0x4B7 ) ); 	if ( Utility.RandomBool() ){ CitizenLocalization.SayLocalizedComposite( from, sing, null ); } }		// TAMBOURINE
					else if ( this.ItemID == 0x64CA || this.ItemID == 0x64CB ){ this.Name = "instrument";	from.PlaySound( Utility.RandomList( 0x3CF, 0x3D0 ) ); } 																	// TRUMPET
					else if ( this.ItemID == 0x64CC || this.ItemID == 0x64CD ){ this.Name = "instrument";	from.PlaySound( 0x5B8 ); }																									// PIPES
				}
				else
				{
					SetInstrument( from, this );

					string[] part1 = new string[] { "I've written this", "I learned this", "I heard this", "I was taught this", "Here is a", "This is a" };
					string[] part1Zh = new string[] { "这首是我写下的", "这首是我学来的", "这首是我听来的", "有人教过我这首", "这儿有一首", "这是一首" };
					string[] part2 = new string[] { "ballad", "song", "tune", "melody" };
					string[] part2Zh = new string[] { "谣曲", "歌", "曲调", "旋律" };
					string[] part4 = new string[] { "death", "fate", "exploits", "courage", "adventures", "journey", "demise", "victories", "legend", "conquests" };
					string[] part4Zh = new string[] { "殒殁", "命运", "功业", "勇气", "历险", "旅途", "终局", "胜绩", "传说", "征伐" };
					string[] part5 = new string[] { "battle", "rise", "destruction", "legend", "secret", "lore", "savior", "champion", "fall", "conquest" };
					string[] part5Zh = new string[] { "战局", "崛起", "毁灭", "传说", "秘密", "掌故", "救主", "勇士", "倾覆", "征服" };
					string[] part6 = new string[] { "horrors", "terror", "treasure", "riches", "creatures", "monsters", "depths", "conquest", "discovery" };
					string[] part6Zh = new string[] { "恐怖", "惊惧", "宝藏", "财富", "生灵", "魔物", "深处", "征服", "发现" };

					int iPart1 = Utility.RandomMinMax( 0, part1.Length - 1 );
					int iPart2 = Utility.RandomMinMax( 0, part2.Length - 1 );
					int iPart4 = Utility.RandomMinMax( 0, part4.Length - 1 );
					int iPart5 = Utility.RandomMinMax( 0, part5.Length - 1 );
					int iPart6 = Utility.RandomMinMax( 0, part6.Length - 1 );
					int iPart2b = Utility.RandomMinMax( 0, part2.Length - 1 );

					string ext1 = part1[iPart1] + " ";
					string ext2 = part2[iPart2] + " ";
					string ext3 = "";
					string ext3Zh = "";
					string ext4 = part4[iPart4];
					string ext5 = part5[iPart5];
					string ext6 = part6[iPart6];
					string ext7 = part2[iPart2b];

					switch ( Utility.RandomMinMax( 1, 10 ) )
					{
						case 1:
						{
							string c = RandomThings.GetRandomCity();
							if ( ext1 == "Here is a " || ext1 == "This is a " )
							{
								ext3 = "from " + c + ".";
								ext3Zh = "取自「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( c ) + "」。";
							}
							else
							{
								ext3 = "while I was in " + c + ".";
								ext3Zh = "是我在「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( c ) + "」时听来的。";
							}
						}
						break;
						case 2:
						{
							string jt = RandomThings.GetRandomJobTitle( 0 );
							string th = RandomThings.GetRandomThing( 0 );
							ext3 = "about " + jt + " and the " + th + ".";
							ext3Zh = "讲述「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( jt ) + "」与「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( th ) + "」的佚事。";
						}
						break;
						case 3:
						{
							string jt = RandomThings.GetRandomJobTitle( 0 );
							string cr = RandomThings.GetRandomCreature();
							ext3 = "about " + jt + " and the " + cr + ".";
							ext3Zh = "讲述「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( jt ) + "」与「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( cr ) + "」的传说。";
						}
						break;
						case 4:
						{
							string st = RandomThings.GetSongTitle();
							ext3 = "called " + st + ".";
							ext3Zh = "曲名唤作「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( st ) + "」。";
						}
						break;
						case 5:
						{
							string st = RandomThings.GetSongTitle();
							ext3 = "I call " + st + ".";
							ext3Zh = "我自己唤它「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( st ) + "」。";
						}
						break;
						case 6:
						{
							string kn = RandomThings.GetRandomKingdomName();
							string kk = RandomThings.GetRandomKingdom();
							if ( ext1 == "Here is a " || ext1 == "This is a " )
							{
								ext3 = "from the " + kn + " " + kk + ".";
								ext3Zh = "出自「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( kn ) + "」的「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( kk ) + "」。";
							}
							else
							{
								ext3 = "while travelling through the " + kn + " " + kk + ".";
								ext3Zh = "是我途经「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( kn ) + "」的「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( kk ) + "」时听来的。";
							}
						}
						break;
						case 7:
						{
							string fn = NameList.RandomName( "female" );
							string jf = RandomThings.GetBoyGirlJob( 1 );
							ext3 = "about the " + ext4 + " of " + fn + " the " + jf + ".";
							ext3Zh = "关乎「" + fn + "」这位" + Citizens.ResolveCitizenRumorToChineseForBroadcast( jf ) + "之" + part4Zh[iPart4] + "。";
						}
						break;
						case 8:
						{
							string mn = NameList.RandomName( "male" );
							string jm = RandomThings.GetBoyGirlJob( 0 );
							ext3 = "about the " + ext4 + " of " + mn + " the " + jm + ".";
							ext3Zh = "关乎「" + mn + "」这位" + Citizens.ResolveCitizenRumorToChineseForBroadcast( jm ) + "之" + part4Zh[iPart4] + "。";
						}
						break;
						case 9:
						{
							string c9 = RandomThings.GetRandomCity();
							ext3 = "about the " + ext5 + " of " + c9 + ".";
							ext3Zh = "关乎「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( c9 ) + "」之" + part5Zh[iPart5] + "。";
						}
						break;
						case 10:
						{
							string dun = RandomThings.MadeUpDungeon();
							ext3 = "about the " + ext6 + " of " + dun + ".";
							ext3Zh = "关乎「" + Citizens.ResolveCitizenRumorToChineseForBroadcast( dun ) + "」之" + part6Zh[iPart6] + "。";
						}
						break;
					}

					string say = ext1 + ext2 + ext3;
					string sayZh = part1Zh[iPart1] + part2Zh[iPart2] + ext3Zh;

					if ( Utility.RandomBool() )
					{
						string job = RandomThings.GetBoyGirlJob( 0 );
						if ( Utility.RandomBool() )
							job = RandomThings.GetBoyGirlJob( 1 );

						string name = RandomThings.GetRandomBoyName();
						string title = " the " + RandomThings.GetBoyGirlJob( 0 );
						if ( Utility.RandomBool() )
						{
							name = RandomThings.GetRandomGirlName();
							title = " the " + RandomThings.GetBoyGirlJob( 1 );
						}
						if ( Utility.RandomBool() )
							title = "";

						string dungeon = RandomThings.MadeUpDungeon();
						if ( Utility.RandomBool() )
							dungeon = QuestCharacters.SomePlace( null );

						string city = RandomThings.GetRandomCity();
						if ( Utility.RandomBool() )
							city = RandomThings.MadeUpCity();

						string cityZh = Citizens.ResolveCitizenRumorToChineseForBroadcast( city );
						string dungeonZh = Citizens.ResolveCitizenRumorToChineseForBroadcast( dungeon );
						string jobZh = Citizens.ResolveCitizenRumorToChineseForBroadcast( job );

						string singer = "written";
						switch ( Utility.RandomMinMax( 0, 3 ) )
						{
							case 1: singer = "created"; break;
							case 2: singer = "sung"; break;
							case 3: singer = "composed"; break;
						}

						string singerZh;
						switch ( singer )
						{
							case "created": singerZh = "编作"; break;
							case "sung": singerZh = "歌咏"; break;
							case "composed": singerZh = "谱成"; break;
							default: singerZh = "谱写"; break;
						}

						string book = "written on a scroll";
						switch ( Utility.RandomMinMax( 0, 3 ) )
						{
							case 1: book = "carved on a tablet"; break;
							case 2: book = "written in a book"; break;
							case 3: book = "scrawled on a wall"; break;
						}

						string bookZh;
						string bookLocZh;
						switch ( book )
						{
							case "carved on a tablet":
								bookZh = "石板";
								bookLocZh = "刻于石板之上";
								break;
							case "written in a book":
								bookZh = "册页";
								bookLocZh = "写在古册之中";
								break;
							case "scrawled on a wall":
								bookZh = "墙面";
								bookLocZh = "潦草涂写在墙上";
								break;
							default:
								bookZh = "卷轴";
								bookLocZh = "记于卷轴之上";
								break;
						}

						string verb = "found";
						switch ( Utility.RandomMinMax( 0, 3 ) )
						{
							case 1: verb = "discovered"; break;
							case 2: verb = "said to be"; break;
							case 3: verb = "seen"; break;
						}

						string verbZh;
						switch ( verb )
						{
							case "discovered": verbZh = "被人发现"; break;
							case "said to be": verbZh = "据传"; break;
							case "seen": verbZh = "有人目击"; break;
							default: verbZh = "被发掘"; break;
						}

						string ext2WordZh = part2Zh[iPart2];
						string ext7Zh = part2Zh[iPart2b];

						string whoEn = name + title;
						string whoZh;
						if ( title != null && title.Length > 0 )
						{
							string jobKey = title.Trim();
							if ( jobKey.StartsWith( "the ", StringComparison.OrdinalIgnoreCase ) && jobKey.Length > 4 )
								jobKey = jobKey.Substring( 4 );
							whoZh = "「" + name + "」这位" + Citizens.ResolveCitizenRumorToChineseForBroadcast( jobKey );
						}
						else
							whoZh = "「" + name + "」";

						switch ( Utility.RandomMinMax( 1, 9 ) )
						{
							case 1:
								say = "This " + ext2 + "was " + singer + " by " + whoEn + ".";
								sayZh = "这首" + ext2WordZh + "乃" + whoZh + "所" + singerZh + "。";
								break;
							case 2:
								say = "This " + ext2 + "was " + singer + " by " + name + " from " + city + ".";
								sayZh = "这首" + ext2WordZh + "由来自「" + cityZh + "」的「" + name + "」所" + singerZh + "。";
								break;
							case 3:
								say = "This " + ext2 + "was " + singer + " by a " + job + ".";
								sayZh = "这首" + ext2WordZh + "乃某位" + jobZh + "所" + singerZh + "。";
								break;
							case 4:
								say = "This " + ext2 + "was " + singer + " by a " + job + " in " + city + ".";
								sayZh = "这首" + ext2WordZh + "乃「" + cityZh + "」某位" + jobZh + "所" + singerZh + "。";
								break;
							case 5:
								say = "This " + ext2 + "was " + book + " " + verb + " in " + dungeon + ".";
								sayZh = "这首" + ext2WordZh + "以" + bookZh + "记谱，于「" + dungeonZh + "」" + verbZh + "。";
								break;
							case 6:
								say = "While exploring " + dungeon + ", this " + ext2 + "was found " + book + ".";
								sayZh = "探「" + dungeonZh + "」时，寻得" + bookLocZh + "的这首" + ext2WordZh + "。";
								break;
							case 7:
								say = name + title + " taught me this " + ext2 + "when I was in " + city + ".";
								sayZh = "我在「" + cityZh + "」时，向" + whoZh + "学了这首" + ext2WordZh + "。";
								break;
							case 8:
								say = "A " + job + " taught me this " + ext2 + "when I was in " + city + ".";
								sayZh = "我在「" + cityZh + "」时，有位" + jobZh + "教了我这首" + ext2WordZh + "。";
								break;
							case 9:
								say = "A " + job + " taught me this " + ext7 + ".";
								sayZh = "有位" + jobZh + "教了我这首" + ext7Zh + "。";
								break;
						}
					}

					CitizenLocalization.SayLocalizedComposite( from, say, sayZh );
					SetInstrument( from, this );
				}
			}
		}

		public static void SetInstrument( Mobile from, Item instrument )
		{
			string facing = "east";

			if ( from.Direction == Direction.South ){ facing = "south"; }
			instrument.Hue = 0;

			if ( facing == "south" )
			{
				switch ( Utility.RandomMinMax( 1, 8 ) )
				{
					case 1:	instrument.ItemID = 0x64BF; instrument.Name = "lute"; 		instrument.Z = from.Z + 9;	break;
					case 2:	instrument.ItemID = 0x64C1; instrument.Name = "flute"; 		instrument.Z = from.Z + 11;	break;
					case 3:	instrument.ItemID = 0x64C3; instrument.Name = "harp"; 		instrument.Z = from.Z + 8;	break;
					case 4:	instrument.ItemID = 0x64C5; instrument.Name = "drum"; 		instrument.Z = from.Z + 8;	break;
					case 5:	instrument.ItemID = 0x64C7; instrument.Name = "fiddle"; 	instrument.Z = from.Z + 10;	break;
					case 6:	instrument.ItemID = 0x64C9; instrument.Name = "tambourine"; instrument.Z = from.Z + 9;	break;
					case 7:	instrument.ItemID = 0x64CB; instrument.Name = "trumpet"; 	instrument.Z = from.Z + 9;	instrument.Hue = 0xB61;	break;
					case 8:	instrument.ItemID = 0x64CD; instrument.Name = "pipes"; 		instrument.Z = from.Z + 9;	break;
				}
			}
			else
			{
				switch ( Utility.RandomMinMax( 1, 8 ) )
				{
					case 1:	instrument.ItemID = 0x64BE; instrument.Name = "lute"; 		instrument.Z = from.Z + 9;	break;
					case 2:	instrument.ItemID = 0x64C0; instrument.Name = "flute"; 		instrument.Z = from.Z + 11;	break;
					case 3:	instrument.ItemID = 0x64C2; instrument.Name = "harp"; 		instrument.Z = from.Z + 8;	break;
					case 4:	instrument.ItemID = 0x64C4; instrument.Name = "drum"; 		instrument.Z = from.Z + 8;	break;
					case 5:	instrument.ItemID = 0x64C6; instrument.Name = "fiddle"; 	instrument.Z = from.Z + 10;	break;
					case 6:	instrument.ItemID = 0x64C8; instrument.Name = "tambourine"; instrument.Z = from.Z + 9;	break;
					case 7:	instrument.ItemID = 0x64CA; instrument.Name = "trumpet"; 	instrument.Z = from.Z + 9;	instrument.Hue = 0xB61;	break;
					case 8:	instrument.ItemID = 0x64CC; instrument.Name = "pipes"; 		instrument.Z = from.Z + 9;	break;
				}
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			Weight = -2.0;
		}
	}
}