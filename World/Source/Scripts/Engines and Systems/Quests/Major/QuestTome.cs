using System;
using Server;
using Server.Network;
using Server.Multis;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Accounting;
using Server.Localization;
using System.Collections.Generic;
using System.Collections;
using Server.Regions; 
using System.Globalization;

namespace Server.Items
{
	public class QuestTome : Item
	{
		private static string ResolveText( Mobile from, string text )
		{
			string lang = AccountLang.GetLanguageCode( from.Account );
			return StringCatalog.TryResolve( lang, text ) ?? text;
		}

		public Map DeliverMap;
		public int DeliverX;
		public int DeliverY;

		public Mobile QuestTomeOwner;
		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile QuestTome_Owner { get{ return QuestTomeOwner; } set{ QuestTomeOwner = value; } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string QuestTomeStoryGood;
		[CommandProperty( AccessLevel.GameMaster )]
		public string QuestTome_StoryGood { get{ return QuestTomeStoryGood; } set{ QuestTomeStoryGood = value; } }

		public string QuestTomeLocateGood;
		[CommandProperty( AccessLevel.GameMaster )]
		public string QuestTome_LocateGood { get{ return QuestTomeLocateGood; } set{ QuestTomeLocateGood = value; } }

		public Land QuestTomeWorldGood;
		[CommandProperty( AccessLevel.GameMaster )]
		public Land QuestTome_WorldGood { get{ return QuestTomeWorldGood; } set{ QuestTomeWorldGood = value; } }

		public string QuestTomeNPCGood;
		[CommandProperty( AccessLevel.GameMaster )]
		public string QuestTome_NPCGood { get{ return QuestTomeNPCGood; } set{ QuestTomeNPCGood = value; } }

		public string QuestTomeStoryEvil;
		[CommandProperty( AccessLevel.GameMaster )]
		public string QuestTome_StoryEvil { get{ return QuestTomeStoryEvil; } set{ QuestTomeStoryEvil = value; } }

		public string QuestTomeLocateEvil;
		[CommandProperty( AccessLevel.GameMaster )]
		public string QuestTome_LocateEvil { get{ return QuestTomeLocateEvil; } set{ QuestTomeLocateEvil = value; } }

		public Land QuestTomeWorldEvil;
		[CommandProperty( AccessLevel.GameMaster )]
		public Land QuestTome_WorldEvil { get{ return QuestTomeWorldEvil; } set{ QuestTomeWorldEvil = value; } }

		public string QuestTomeNPCEvil;
		[CommandProperty( AccessLevel.GameMaster )]
		public string QuestTome_NPCEvil { get{ return QuestTomeNPCEvil; } set{ QuestTomeNPCEvil = value; } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string QuestTomeCitizen;
		[CommandProperty( AccessLevel.GameMaster )]
		public string QuestTome_Citizen { get{ return QuestTomeCitizen; } set{ QuestTomeCitizen = value; } }

		public int QuestTomeGoals;
		[CommandProperty(AccessLevel.Owner)]
		public int QuestTome_Goals { get { return QuestTomeGoals; } set { QuestTomeGoals = value; InvalidateProperties(); } }

		public string QuestTomeDungeon;
		[CommandProperty( AccessLevel.GameMaster )]
		public string QuestTome_Dungeon { get{ return QuestTomeDungeon; } set{ QuestTomeDungeon = value; } }

		public Land QuestTomeLand;
		[CommandProperty( AccessLevel.GameMaster )]
		public Land QuestTome_Land { get{ return QuestTomeLand; } set{ QuestTomeLand = value; } }

		public int QuestTomeType;
		[CommandProperty(AccessLevel.Owner)]
		public int QuestTome_Type { get { return QuestTomeType; } set { QuestTomeType = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string GoalItem1;
		[CommandProperty(AccessLevel.Owner)]
		public string Goal_Item1 { get { return GoalItem1; } set { GoalItem1 = value; InvalidateProperties(); } }

		public string GoalItem2;
		[CommandProperty(AccessLevel.Owner)]
		public string Goal_Item2 { get { return GoalItem2; } set { GoalItem2 = value; InvalidateProperties(); } }

		public string GoalItem3;
		[CommandProperty(AccessLevel.Owner)]
		public string Goal_Item3 { get { return GoalItem3; } set { GoalItem3 = value; InvalidateProperties(); } }

		public string GoalItem4;
		[CommandProperty(AccessLevel.Owner)]
		public string Goal_Item4 { get { return GoalItem4; } set { GoalItem4 = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public string VillainCategory;
		[CommandProperty(AccessLevel.Owner)]
		public string Villain_Category { get { return VillainCategory; } set { VillainCategory = value; InvalidateProperties(); } }

		public string VillainType;
		[CommandProperty(AccessLevel.Owner)]
		public string Villain_Type { get { return VillainType; } set { VillainType = value; InvalidateProperties(); } }

		public string VillainName;
		[CommandProperty(AccessLevel.Owner)]
		public string Villain_Name { get { return VillainName; } set { VillainName = value; InvalidateProperties(); } }

		public string VillainTitle;
		[CommandProperty(AccessLevel.Owner)]
		public string Villain_Title { get { return VillainTitle; } set { VillainTitle = value; InvalidateProperties(); } }

		public int VillainBody;
		[CommandProperty(AccessLevel.Owner)]
		public int Villain_Body { get { return VillainBody; } set { VillainBody = value; InvalidateProperties(); } }

		public int VillainHue;
		[CommandProperty(AccessLevel.Owner)]
		public int Villain_Hue { get { return VillainHue; } set { VillainHue = value; InvalidateProperties(); } }

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		[Constructable]
		public QuestTome() : base( 0x1A97 )
		{
			Name = "lost journal";
			Weight = 1.0;
		}

		public override bool IsContentLocalized { get { return true; } }
		public override string DisplayNameLocalizationKey { get { return "quest.tome.name.lost_journal"; } }

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( QuestTomeOwner != null )
			{
				if ( BuildingPropertyListLocale != null )
					AddLocalizedProperty( list, "quest.tome.opl.belongs_to", QuestTomeOwner.Name );
				else
					list.Add( 1049644, "Belongs to " + QuestTomeOwner.Name + "" );
			}
        }

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1060640 ); // The item must be in your backpack to use it.
			}
			else if ( QuestTomeOwner != from )
			{
				from.SendMessage( ResolveText( from, "This book does not belong and it crumbles to dust!" ) );
				bool remove = true;
				foreach ( Account a in Accounts.GetAccounts() )
				{
					if (a == null)
						break;

					int index = 0;

					for (int i = 0; i < a.Length; ++i)
					{
						Mobile m = a[i];

						if (m == null)
							continue;

						if ( m == QuestTomeOwner )
						{
							m.AddToBackpack( this );
							remove = false;
						}

						++index;
					}
				}
				if ( remove )
				{
					this.Delete();
				}
			}
			else if ( QuestTomeGoals > 2 && from.Region.Name == QuestTomeDungeon && QuestTomeCitizen != "" )
			{
				QuestTomeCitizen = "";
				QuestTomeLand = Land.None;
				QuestTomeType = 0;

				Type mobType = ScriptCompiler.FindTypeByName( VillainType );
				Mobile mob = (Mobile)Activator.CreateInstance( mobType );
				BaseCreature monster = (BaseCreature)mob;

				SummonPrison.SetDifficultyForMonster( monster );

				Map map = from.Map;

				bool validLocation = false;
				Point3D loc = from.Location;

				for ( int j = 0; !validLocation && j < 10; ++j )
				{
					int x = from.X + Utility.Random( 3 ) - 1;
					int y = from.Y + Utility.Random( 3 ) - 1;
					int z = map.GetAverageZ( x, y );

					if ( validLocation = map.CanFit( x, y, from.Z, 16, false, false ) )
						loc = new Point3D( x, y, from.Z );
					else if ( validLocation = map.CanFit( x, y, z, 16, false, false ) )
						loc = new Point3D( x, y, z );
				}

				monster.NameHue = 0x22;
				monster.Hue = VillainHue;
				if ( VillainBody > 0 ){ monster.Body = VillainBody; }
				monster.Title = VillainTitle;
				monster.Name = VillainName;
				monster.MoveToWorld( loc, map );
				monster.Combatant = from;
				monster.Fame = 0;
				monster.Karma = 0;
				Effects.SendLocationParticles( EffectItem.Create( monster.Location, monster.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
				monster.PlaySound( 0x1FE );
			}
			else
			{
				from.CloseGump( typeof( QuestTomeGump ) );
				from.SendGump( new QuestTomeGump( this, from, 0 ) );
			}
		}

		public QuestTome(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)2);

			writer.Write( DeliverMap );
			writer.Write( DeliverX );
			writer.Write( DeliverY );

			writer.Write( (Mobile)QuestTomeOwner );
			writer.Write( QuestTomeStoryGood );
			writer.Write( QuestTomeLocateGood );
			writer.Write( (int)QuestTomeWorldGood );
			writer.Write( QuestTomeNPCGood );
			writer.Write( QuestTomeStoryEvil );
			writer.Write( QuestTomeLocateEvil );
			writer.Write( (int)QuestTomeWorldEvil );
			writer.Write( QuestTomeNPCEvil );
			writer.Write( QuestTomeCitizen );
			writer.Write( QuestTomeGoals );
			writer.Write( QuestTomeDungeon );
			writer.Write( (int)QuestTomeLand );
			writer.Write( QuestTomeType );
			writer.Write( GoalItem1 );
			writer.Write( GoalItem2 );
			writer.Write( GoalItem3 );
			writer.Write( GoalItem4 );
			writer.Write( VillainCategory );
			writer.Write( VillainType );
			writer.Write( VillainName );
			writer.Write( VillainTitle );
			writer.Write( VillainBody );
			writer.Write( VillainHue );
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			switch ( version )
			{
				case 2:
				{
					DeliverMap = reader.ReadMap();
					DeliverX = reader.ReadInt();
					DeliverY = reader.ReadInt();
					break;
				}
			}

			QuestTomeOwner = reader.ReadMobile();
            QuestTomeStoryGood = reader.ReadString();
            QuestTomeLocateGood = reader.ReadString();

			if ( version < 1 )
				QuestTomeWorldGood = Server.Lands.LandRef( reader.ReadString() );
			else
				QuestTomeWorldGood = (Land)(reader.ReadInt());

            QuestTomeNPCGood = reader.ReadString();
            QuestTomeStoryEvil = reader.ReadString();
            QuestTomeLocateEvil = reader.ReadString();

			if ( version < 1 )
				QuestTomeWorldEvil = Server.Lands.LandRef( reader.ReadString() );
			else
				QuestTomeWorldEvil = (Land)(reader.ReadInt());

            QuestTomeNPCEvil = reader.ReadString();
            QuestTomeCitizen = reader.ReadString();
            QuestTomeGoals = reader.ReadInt();
            QuestTomeDungeon = reader.ReadString();

			if ( version < 1 )
				QuestTomeLand = Server.Lands.LandRef( reader.ReadString() );
			else
				QuestTomeLand = (Land)(reader.ReadInt());

            QuestTomeType = reader.ReadInt();
            GoalItem1 = reader.ReadString();
            GoalItem2 = reader.ReadString();
            GoalItem3 = reader.ReadString();
            GoalItem4 = reader.ReadString();
			VillainCategory = reader.ReadString();
			VillainType = reader.ReadString();
			VillainName = reader.ReadString();
			VillainTitle = reader.ReadString();
			VillainBody = reader.ReadInt();
			VillainHue = reader.ReadInt();
		}

		private class QuestTomeGump : Gump
		{
			private QuestTome m_Book;
			private Map m_Map;
			private int m_X;
			private int m_Y;

			public QuestTomeGump( QuestTome book, Mobile from, int page ) : base( 50, 50 )
			{
				m_Book = book;

				from.SendSound( 0x55 );

				m_Map = book.DeliverMap;
				m_X = book.DeliverX;
				m_Y = book.DeliverY;

				this.Closable=true;
				this.Disposable=true;
				this.Dragable=true;
				this.Resizable=false;

				AddPage(0);

				string gumpLocale = AccountLang.GetLanguageCode( from.Account );
				if ( !AccountLang.IsChinese( gumpLocale ) )
					gumpLocale = "en";

				string color = "#c6c67b";
				string story = m_Book.QuestTomeStoryGood;
				string locat = m_Book.QuestTomeLocateGood;
				string world = Server.Lands.LocalizedLandName( m_Book.QuestTomeWorldGood, gumpLocale );
				string names = m_Book.QuestTomeNPCGood;
						
				if ( ((PlayerMobile)from).KarmaLocked ) // THEY ARE ON AN EVIL PATH
				{
					color = "#cfa495";
					story = m_Book.QuestTomeStoryEvil;
					locat = m_Book.QuestTomeLocateEvil;
					world = Server.Lands.LocalizedLandName( m_Book.QuestTomeWorldEvil, gumpLocale );
					names = m_Book.QuestTomeNPCEvil;
				}

				AddImage(0, 0, 7032, Server.Misc.PlayerSettings.GetGumpHue( from ));
				AddHtml( 12, 12, 665, 20, @"<BODY><BASEFONT Color=" + color + ">" + m_Book.Name + "</BASEFONT></BODY>", (bool)false, (bool)false);

				string dead = m_Book.Name; if ( dead.Contains("Journal of ") ){ dead = dead.Replace("Journal of ", ""); }

				// For Chinese accounts, reconstruct story from shotkey templates
				if ( AccountLang.IsChinese( AccountLang.GetLanguageCode( from.Account ) ) )
				{
					bool isEvil = ((PlayerMobile)from).KarmaLocked;
					string storyKey = isEvil ? "quest.tome.story.evil" : "quest.tome.story.good";
					string zhLocale = "zh-Hans";
					story = StringCatalog.ResolveFormatByKey( from.Account, storyKey,
						AdventurerDisplayName( zhLocale, m_Book.Name ),        // {0} adventurer
						LocalizedEpicNpc( zhLocale, isEvil ? m_Book.QuestTomeNPCEvil : m_Book.QuestTomeNPCGood ),
						LocalizedQuestItemName( zhLocale, m_Book.GoalItem4 ),
						"",                                 // {3} takes - ignored in ZH
						LocalizedPersonName( zhLocale, m_Book.VillainName ),
						LocalizedEvilTitle( zhLocale, m_Book.VillainTitle ),
						LocalizedVillainCategory( zhLocale, m_Book.VillainCategory ),
						"",                                 // {7} heard - ignored in ZH
						"",                                 // {8} legend - ignored in ZH
						"",                                 // {9} hush - ignored in ZH
						"",                                 // {10} inn - ignored in ZH
						LocalizedQuestItemName( zhLocale, m_Book.GoalItem1 ),
						LocalizedQuestItemName( zhLocale, m_Book.GoalItem2 ),
						LocalizedQuestItemName( zhLocale, m_Book.GoalItem3 ),
						LocalizedEpicNpc( zhLocale, isEvil ? m_Book.QuestTomeNPCGood : m_Book.QuestTomeNPCEvil ),
						world,
						locat
					);
				}
				else
				{
					if ( story.Contains("DDDDD") ){ story = story.Replace("DDDDD", dead); }
				}

				if ( page > 0 )
				{
					AddButton(864, 9, 4017, 4017, 2, GumpButtonType.Reply, 0);
					string guideText;
					if ( AccountLang.IsChinese( AccountLang.GetLanguageCode( from.Account ) ) )
					{
						string zhLocale = "zh-Hans";
						guideText = StringCatalog.ResolveFormatByKey( from.Account, "quest.tome.help.guide",
							LocalizedQuestItemName( zhLocale, m_Book.GoalItem4 ),
							LocalizedEpicNpc( zhLocale, m_Book.QuestTomeNPCEvil ),
							LocalizedEpicNpc( zhLocale, m_Book.QuestTomeNPCGood ),
							LocalizedPersonName( zhLocale, m_Book.VillainName ),
							LocalizedEvilTitle( zhLocale, m_Book.VillainTitle ) );
					}
					else
					{
						guideText = StringCatalog.ResolveFormatByKey( from.Account, "quest.tome.help.guide",
							m_Book.GoalItem4,
							m_Book.QuestTomeNPCEvil,
							m_Book.QuestTomeNPCGood,
							m_Book.VillainName,
							m_Book.VillainTitle );
					}
					AddHtml( 12, 43, 878, 548, @"<BODY><BASEFONT Color=" + color + ">" + guideText + @"</BASEFONT></BODY>", (bool)false, (bool)false);
				}
				else
				{
					AddButton(864, 9, 4017, 4017, 0, GumpButtonType.Reply, 0);
					AddButton(792, 9, 3610, 3610, 1, GumpButtonType.Reply, 0);

					if ( Sextants.HasSextant( from ) )
						AddButton(756, 12, 10461, 10461, 3, GumpButtonType.Reply, 0);

					AddHtml( 12, 46, 346, 20, @"<BODY><BASEFONT Color=" + color + ">" + StringCatalog.ResolveFormatByKey(from.Account, "quest.tome.gump.title", from.Name) + @"</BASEFONT></BODY>", (bool)false, (bool)false);

					if ( m_Book.QuestTomeCitizen != "" ){ story = GetRumor( m_Book, false, from.Account ) + "<br><br>" + story; }

					AddHtml( 12, 82, 878, 358, @"<BODY><BASEFONT Color=" + color + ">" + story + "</BASEFONT></BODY>", (bool)false, (bool)false);

					TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
					bool zhGoals = AccountLang.IsChinese( AccountLang.GetLanguageCode( from.Account ) );

					if ( m_Book.QuestTomeGoals < 4 )
					{
						AddHtml( 55, 461, 346, 20, @"<BODY><BASEFONT Color=" + color + ">" + FormatGoalLabel( zhGoals, m_Book.GoalItem1, cultInfo ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
						if ( m_Book.QuestTomeGoals > 0 ){ AddItem(0, 448, 20413); }
						AddHtml( 55, 513, 346, 20, @"<BODY><BASEFONT Color=" + color + ">" + FormatGoalLabel( zhGoals, m_Book.GoalItem2, cultInfo ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
						if ( m_Book.QuestTomeGoals > 1 ){ AddItem(0, 500, 20413); }
						AddHtml( 55, 564, 346, 20, @"<BODY><BASEFONT Color=" + color + ">" + FormatGoalLabel( zhGoals, m_Book.GoalItem3, cultInfo ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
						if ( m_Book.QuestTomeGoals > 2 ){ AddItem(0, 551, 20413); }
					}
					else
					{
						AddHtml( 55, 513, 346, 20, @"<BODY><BASEFONT Color=" + color + ">" + FormatGoalLabel( zhGoals, m_Book.GoalItem4, cultInfo ) + "</BASEFONT></BODY>", (bool)false, (bool)false);
						AddItem(0, 500, 20413);
					}
				}
			}

			public override void OnResponse( NetState state, RelayInfo info ) 
			{
				Mobile from = state.Mobile; 
				from.CloseGump( typeof( Sextants.MapGump ) );

				if ( info.ButtonID == 1 ){ from.SendGump( new QuestTomeGump( m_Book, from, 1 ) ); }
				else if ( info.ButtonID == 2 ){ from.SendGump( new QuestTomeGump( m_Book, from, 0 ) ); }
				else if ( info.ButtonID == 3 )
				{
					from.SendGump( new QuestTomeGump( m_Book, from, 0 ) );
					from.SendGump( new Sextants.MapGump( from, m_Map, m_X, m_Y, null ) );
				}

				from.SendSound( 0x55 );
			}
		}

		public static string TellRumor( PlayerMobile player, Citizens citizen )
		{
			string rumor = "";

			if ( citizen.CanTellRumor() )
			{
				QuestTome book = player.Backpack.FindItemByType( typeof ( QuestTome ) ) as QuestTome;
				if ( book != null && book.QuestTomeOwner == player )
				{
					if ( Utility.RandomMinMax( 1, 10 ) > 1 ){ citizen.MarkToldRumor(); }

					if ( citizen.CanTellRumor() && book.QuestTomeCitizen == "" && book.QuestTomeGoals < 4 )
					{
						citizen.MarkToldRumor();
						SetRumor( citizen, player, book );
						rumor = GetRumor( book, true, player.Account );
					}
				}
			}

			return rumor;
		}

		public static string GetRumor( QuestTome book, bool talk, IAccount acct = null )
		{
			int goal = book.QuestTomeType;
			string locate = "held by a powerful creature";
			int locateType = 0; // 0=held, 1=lost, 2=found
			if ( goal == 2 ){ locate = "lost somewhere"; locateType = 1; }
			if ( book.QuestTomeGoals == 3 ){ locate = "found"; goal = 3; locateType = 2; }

			string world;
			string dungeon = book.QuestTomeDungeon;
			string from = book.QuestTomeCitizen;
			string item = book.GoalItem1;
				if ( book.QuestTomeGoals == 1 ){ item = book.GoalItem2; }
				else if ( book.QuestTomeGoals == 2 ){ item = book.GoalItem3; }
				else if ( book.QuestTomeGoals == 3 ){ item = book.VillainName + " " + book.VillainTitle; }

			if ( acct != null && AccountLang.IsChinese( AccountLang.GetLanguageCode( acct ) ) )
			{
				string locale = "zh-Hans";

				if ( book.QuestTomeGoals == 3 )
					item = LocalizedPersonName( locale, book.VillainName ) + "（" + LocalizedEvilTitle( locale, book.VillainTitle ) + "）";
				else
					item = LocalizedQuestItemName( locale, item );
			}

			if ( acct != null && AccountLang.IsChinese( AccountLang.GetLanguageCode( acct ) ) )
			{
				world = Server.Lands.LocalizedLandName( book.QuestTomeLand, "zh-Hans" );
				dungeon = LocalizedDungeon( "zh-Hans", dungeon );
			}
			else
			{
				world = Server.Lands.LandName( book.QuestTomeLand );
			}

			if ( talk )
			{
				if ( acct != null && AccountLang.IsChinese( AccountLang.GetLanguageCode( acct ) ) )
				{
					string locale = "zh-Hans";
					string who = "";
					switch ( Utility.RandomMinMax( 0, 5 ) )
					{
						case 0: who = StringCatalog.TryResolveByKey( locale, "quest.tome.rumor.who_heard" ) ?? "我听说"; break;
						case 1: who = StringCatalog.TryResolveByKey( locale, "quest.tome.rumor.who_learned" ) ?? "我打听到"; break;
						case 2: who = StringCatalog.TryResolveByKey( locale, "quest.tome.rumor.who_found_out" ) ?? "我发现了"; break;
						case 3: who = string.Format( StringCatalog.TryResolveByKey( locale, "quest.tome.rumor.who_job" ) ?? "{1}的{0}告诉我",
							LocalizedJob( locale ), LocalizedCity( locale ) ); break;
						case 4: who = string.Format( StringCatalog.TryResolveByKey( locale, "quest.tome.rumor.who_overheard" ) ?? "我偶然听到一个{0}说",
							LocalizedJob( locale ) ); break;
						case 5: who = StringCatalog.TryResolveByKey( locale, "quest.tome.rumor.who_friend" ) ?? "我朋友告诉我"; break;
					}
					// Use heard_* templates: {0}=who, {1}=item, {2}=dungeon, {3}=world
					string rumorKey;
					switch ( locateType )
					{
						case 1: rumorKey = "quest.tome.rumor.heard_lost"; break;
						case 2: rumorKey = "quest.tome.rumor.heard_found"; break;
						default: rumorKey = "quest.tome.rumor.heard_held"; break;
					}
					return StringCatalog.ResolveFormatByKey( acct, rumorKey, who, item, dungeon, world );
				}

				string whoEn = "I heard";
				switch ( Utility.RandomMinMax( 0, 5 ) )
				{
					case 0: whoEn = "I heard"; break;
					case 1: whoEn = "I learned"; break;
					case 2: whoEn = "I found out"; break;
					case 3: whoEn = "The " + RandomThings.GetRandomJob() + " in " + RandomThings.GetRandomCity() + " told me"; break;
					case 4: whoEn = "I overheard some " + RandomThings.GetRandomJob() + " say"; break;
					case 5: whoEn = "My friend told me"; break;
				}
				return whoEn + " that " + item + " may be " + locate + " within " + dungeon + " in " + world + ".";
			}

			if ( acct != null && AccountLang.IsChinese( AccountLang.GetLanguageCode( acct ) ) )
			{
				string rumorKey;
				switch ( locateType )
				{
					case 1: rumorKey = "quest.tome.rumor.talk_lost"; break;
					case 2: rumorKey = "quest.tome.rumor.talk_found"; break;
					default: rumorKey = "quest.tome.rumor.talk_held"; break;
				}
				if ( world != "" )
					return StringCatalog.ResolveFormatByKey( acct, rumorKey, from, item, dungeon, world );
				return "";
			}

			if ( world != "" ){ return "" + from + " has told you that " + item + " may be " + locate + " within " + dungeon + " in " + world + "."; }

			return "";
		}

		public static void SetRumor( Mobile m, PlayerMobile player, QuestTome book )
		{
			book.QuestTomeType = Utility.RandomMinMax( 1, 2 );

			if ( book.QuestTomeGoals > 2 ){ book.QuestTomeType = 3; }

			var options = new List<Land>
			{
				Land.Sosaria,
				Land.Lodoria,
				Land.Serpent,
				Land.Sosaria,
				Land.Lodoria,
				Land.Serpent,
				Land.UmberVeil,
				Land.Ambrosia,
				Land.IslesDread,
				Land.Savaged,
				Land.Kuldar,
			};
			Land searchLocation = PlayerSettings.GetRandomDiscoveredLand(player, options, null);

			string dungeon = "Dungeon Doom";

			int aCount = 0;

			ArrayList targets = new ArrayList();

			if ( book.QuestTomeType == 1 )
			{
				foreach ( Mobile target in World.Mobiles.Values )
				if ( target.Region is DungeonRegion && target.Fame >= 18000 && !( target is Exodus || target is CodexGargoyleA || target is CodexGargoyleB || target is Syth ) )
				{
					if ( target.Land == searchLocation )
					{
						targets.Add( target );
						aCount++;
					}
				}
			}
			else
			{
				foreach ( Item target in World.Items.Values )
				if ( target is SearchBase || target is StealBase )
				{
					if ( target.Land == searchLocation )
					{
						targets.Add( target );
						aCount++;
					}
				}
			}

			aCount = Utility.RandomMinMax( 1, aCount );

			int xCount = 0;
			for ( int i = 0; i < targets.Count; ++i )
			{
				xCount++;

				if ( xCount == aCount )
				{
					if ( book.QuestTomeType == 1 )
					{
						Mobile finding = ( Mobile )targets[ i ];
						dungeon = Server.Misc.Worlds.GetRegionName( finding.Map, finding.Location );
					}
					else
					{
						Item finding = ( Item )targets[ i ];
						dungeon = Server.Misc.Worlds.GetRegionName( finding.Map, finding.Location );
					}
				}
			}

			book.QuestTomeLand = searchLocation;
			book.QuestTomeDungeon = dungeon;
			book.QuestTomeCitizen = "" + m.Name + " " + m.Title + "";
		}

		public static bool FoundItem( Mobile player, int type, MajorItemOnCorpse chest )
		{
			QuestTome book = player.Backpack.FindItemByType( typeof ( QuestTome ) ) as QuestTome;
			if (book == null) return false;

			if ( type == book.QuestTomeType && book.QuestTomeDungeon == Server.Misc.Worlds.GetRegionName( player.Map, player.Location ) && book.QuestTomeOwner == player && book.QuestTomeGoals < 3 )
			{
				if ( Utility.RandomMinMax( 1, 3 ) != 1 )
				{
					string relic = book.GoalItem1;
						if ( book.QuestTomeGoals == 1 ){ relic = book.GoalItem2; }
						else if ( book.QuestTomeGoals == 2 ){ relic = book.GoalItem3; }

					if ( AccountLang.IsChinese( AccountLang.GetLanguageCode( player.Account ) ) )
						relic = LocalizedQuestItemName( "zh-Hans", relic );

					player.LocalOverheadMessage(MessageType.Emote, 1150, true, StringCatalog.ResolveFormatByKey(player.Account, "quest.tome.emote.found_relic", relic));
					player.SendSound( 0x5B4 );
					book.QuestTomeCitizen = "";
					book.QuestTomeDungeon = "";
					book.QuestTomeLand = Land.None;
					book.QuestTomeType = 0;
					book.QuestTomeGoals++;

					return true;
				}
				else
				{
					player.LocalOverheadMessage(MessageType.Emote, 1150, true, StringCatalog.ResolveFormatByKey(player.Account, "quest.n0_was_either_wrong_or_they_lied_dot", book.QuestTomeCitizen));
					player.SendSound( 0x5B3 );
					book.QuestTomeCitizen = "";
					book.QuestTomeDungeon = "";
					book.QuestTomeLand = Land.None;
					book.QuestTomeType = 0;

					return false;
				}
			}
			else if ( chest != null && book.VillainName == chest.VillainName && book.VillainTitle == chest.VillainTitle && book.QuestTomeOwner == player && book.QuestTomeGoals >= 3 )
			{
				player.AddToBackpack(new HoardMinionFamiliarItem());
				ApproachObsidian.TitanRiches( player );
				CustomEventSink.InvokeCombatQuestCompleted(player, 10000);
				player.LocalOverheadMessage(MessageType.Emote, 1150, true, StringCatalog.ResolveFormatByKey(player.Account, "quest.tome.emote.found_goal", book.GoalItem4));
				book.QuestTomeGoals++;

				return true;
			}
			return false;
		}

		public static void BossEscaped( Mobile from, string region )
		{
			if ( from.Backpack.FindItemByType( typeof ( QuestTome ) ) != null )
			{
				Item item = from.Backpack.FindItemByType( typeof ( QuestTome ) );
				QuestTome book = (QuestTome)item;

				if ( book.QuestTomeGoals > 2 && book.QuestTomeDungeon == region && book.QuestTomeOwner == from )
				{
					ArrayList targets = new ArrayList();
					foreach ( Mobile creature in World.Mobiles.Values )
					{
						if ( creature.Name == book.VillainName && creature.Title == book.VillainTitle )
						{
							targets.Add( creature );
						}
					}
					for ( int i = 0; i < targets.Count; ++i )
					{
						Mobile creature = ( Mobile )targets[ i ];

						Effects.SendLocationParticles( EffectItem.Create( creature.Location, creature.Map, EffectItem.DefaultDuration ), 0x3728, 10, 10, 2023 );
						creature.PlaySound( 0x1FE );

						creature.Delete();
					}
				}
			}
		}

		private static string Slugify( string value )
		{
			if ( string.IsNullOrEmpty( value ) )
				return value;

			return value.Trim().ToLowerInvariant().Replace( ' ', '_' ).Replace( "'", "" ).Replace( ".", "" );
		}

		private static string FormatGoalLabel( bool zhGoals, string raw, TextInfo cultInfo )
		{
			if ( zhGoals )
				return LocalizedQuestItemName( "zh-Hans", raw );
			return cultInfo.ToTitleCase( raw );
		}

		private static string TryResolveShotkey( string locale, string key )
		{
			string zh = StringCatalog.TryResolveByKey( locale, key );
			if ( zh != null && !string.IsNullOrEmpty( zh ) )
				return zh;
			return null;
		}

		private static readonly string[] s_RelicNouns = new string[]
		{
			"Crystal Ball", "Spellbook", "Warhammer", "Crossbow", "Scabbard", "Scimitar", "Halberd",
			"Amulet", "Armor", "Axe", "Bag", "Belt", "Blade", "Bones", "Book", "Boots", "Bottle", "Bow",
			"Bracelet", "Candle", "Cape", "Chalice", "Cloak", "Club", "Codex", "Crown", "Cutlass",
			"Dagger", "Drum", "Dust", "Earrings", "Elixir", "Flute", "Gem", "Gloves", "Goblet", "Hat",
			"Helm", "Horn", "Key", "Knife", "Kryss", "Lantern", "Lexicon", "Lute", "Mace", "Mirror",
			"Necklace", "Parchment", "Portrait", "Potion", "Pouch", "Ring", "Robe", "Rod", "Rope",
			"Sceptre", "Scroll", "Shackles", "Shield", "Skull", "Staff", "Stone", "Sword", "Tablet",
			"Tome", "Trident", "Veil", "Wand"
		};

		private static bool IsRelicNoun( string locale, string noun )
		{
			if ( IsKnownRelicNoun( noun ) )
				return true;

			return TryResolveRelic( locale, "relic.", noun ) != null;
		}

		private static string ResolveRelicNoun( string locale, string noun )
		{
			if ( string.IsNullOrEmpty( noun ) )
				return "";

			string resolved = TryResolveRelic( locale, "relic.", noun );
			if ( resolved != null )
				return resolved;

			return TranslateOfWord( locale, noun );
		}

		private static string ResolveRelicToken( string locale, string prefix, string english )
		{
			if ( string.IsNullOrEmpty( english ) )
				return "";

			string resolved = TryResolveShotkey( locale, "quest.tome.noun." + prefix + Slugify( english ) );
			return resolved ?? english;
		}

		private static bool IsKnownRelicNoun( string noun )
		{
			if ( string.IsNullOrEmpty( noun ) )
				return false;

			for ( int i = 0; i < s_RelicNouns.Length; ++i )
			{
				if ( noun.Equals( s_RelicNouns[i], StringComparison.OrdinalIgnoreCase ) )
					return true;
			}

			return false;
		}

		/// <summary>
		/// Split "{adj} {noun} of {source}" using the first case-insensitive " of "
		/// that follows a known relic noun (handles Title Case " Of " and "Eye Of Newt" sources).
		/// </summary>
		private static bool TryExtractOfPhrase( string locale, ref string body, out string ofPart )
		{
			ofPart = null;

			if ( string.IsNullOrEmpty( body ) )
				return false;

			int searchFrom = 0;

			while ( true )
			{
				int ofIdx = body.IndexOf( " of ", searchFrom, StringComparison.OrdinalIgnoreCase );
				if ( ofIdx <= 0 )
					return false;

				string before = body.Substring( 0, ofIdx ).Trim();
				string after = body.Substring( ofIdx + 4 ).Trim();

				string adjCheck;
				string nounCheck;

				if ( TrySplitRelicBody( before, out adjCheck, out nounCheck ) && IsRelicNoun( locale, nounCheck ) )
				{
					body = before;
					ofPart = after;
					return true;
				}

				searchFrom = ofIdx + 1;
			}
		}

		private static bool TrySplitRelicBody( string body, out string adj, out string noun )
		{
			adj = "";
			noun = body;

			if ( string.IsNullOrEmpty( body ) )
				return false;

			foreach ( string candidate in s_RelicNouns )
			{
				if ( body.Equals( candidate, StringComparison.OrdinalIgnoreCase ) )
				{
					adj = "";
					noun = candidate;
					return true;
				}

				string suffix = " " + candidate;
				if ( body.EndsWith( suffix, StringComparison.OrdinalIgnoreCase ) )
				{
					adj = body.Substring( 0, body.Length - suffix.Length ).Trim();
					noun = candidate;
					return true;
				}
			}

			int lastSpace = body.LastIndexOf( ' ' );
			if ( lastSpace > 0 )
			{
				adj = body.Substring( 0, lastSpace ).Trim();
				noun = body.Substring( lastSpace + 1 ).Trim();
				return true;
			}

			return true;
		}

		private static string StripInlineAnnotation( string zh )
		{
			if ( string.IsNullOrEmpty( zh ) )
				return zh;

			int idx = zh.IndexOf( '（' );
			if ( idx > 0 )
				return zh.Substring( 0, idx );

			idx = zh.IndexOf( '(' );
			if ( idx > 0 )
				return zh.Substring( 0, idx );

			return zh;
		}

		private static string TryResolveRelic( string locale, string prefix, string english )
		{
			if ( string.IsNullOrEmpty( english ) )
				return null;

			return TryResolveShotkey( locale, "quest.tome.noun." + prefix + Slugify( english ) );
		}

		private static bool IsLiquidContainer( string noun )
		{
			return noun.Equals( "Bottle", StringComparison.OrdinalIgnoreCase )
				|| noun.Equals( "Potion", StringComparison.OrdinalIgnoreCase )
				|| noun.Equals( "Elixir", StringComparison.OrdinalIgnoreCase );
		}

		private static string TranslateOfWord( string locale, string word )
		{
			string resolved = TryResolveRelic( locale, "relic.of.", word );
			if ( resolved != null )
				return resolved;

			resolved = TryResolveRelic( locale, "relic.creature.", word );
			if ( resolved != null )
				return resolved;

			resolved = TryResolveRelic( locale, "relic.substance.", word );
			if ( resolved != null )
				return resolved;

			resolved = TryResolveRelic( locale, "relic.adj.", word );
			if ( resolved != null )
				return resolved;

			return RandomThings.GetChineseFantasyName( word );
		}

		private static string TranslateOfPhrase( string locale, string ofPart )
		{
			if ( string.IsNullOrEmpty( ofPart ) )
				return "";

			string whole = TryResolveRelic( locale, "relic.herb.", ofPart );
			if ( whole != null )
				return whole;

			whole = TryResolveRelic( locale, "relic.of.", ofPart );
			if ( whole != null )
				return whole;

			string[] parts = ofPart.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );

			if ( parts.Length == 2 )
			{
				string creature = TryResolveRelic( locale, "relic.creature.", parts[0] );
				string substance = TryResolveRelic( locale, "relic.substance.", parts[1] );

				if ( creature != null && substance != null )
					return creature + substance;
			}

			if ( parts.Length == 1 )
				return TranslateOfWord( locale, parts[0] );

			string result = "";

			for ( int i = 0; i < parts.Length; ++i )
			{
				if ( parts[i].Equals( "of", StringComparison.OrdinalIgnoreCase ) )
					continue;

				if ( parts[i].Equals( "the", StringComparison.OrdinalIgnoreCase ) )
					continue;

				result += TranslateOfWord( locale, parts[i] );
			}

			return result;
		}

		private static string ComposeRelicAdj( string locale, string adjPhrase )
		{
			if ( string.IsNullOrEmpty( adjPhrase ) )
				return "";

			string phrase = TryResolveShotkey( locale, "quest.tome.noun.relic.adj." + Slugify( adjPhrase ) );
			if ( phrase != null )
				return phrase;

			string[] parts = adjPhrase.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
			if ( parts.Length == 1 )
				return TranslateOfWord( locale, parts[0] );

			string result = "";

			for ( int i = 0; i < parts.Length; ++i )
			{
				result += TranslateOfWord( locale, parts[i] );
			}

			return result;
		}

		public static string LocalizedPersonName( string locale, string englishName )
		{
			if ( string.IsNullOrEmpty( englishName ) )
				return englishName;

			string epic = TryResolveShotkey( locale, "quest.tome.noun.epic.name." + Slugify( englishName ) );
			if ( epic != null )
				return StripInlineAnnotation( epic );

			return RandomThings.GetChineseFantasyName( englishName );
		}

		public static string LocalizedEpicNpc( string locale, string nameAndTitle )
		{
			if ( string.IsNullOrEmpty( nameAndTitle ) )
				return nameAndTitle;

			nameAndTitle = nameAndTitle.Trim();

			int idx = nameAndTitle.IndexOf( " the " );
			if ( idx > 0 )
			{
				string name = nameAndTitle.Substring( 0, idx );
				string title = nameAndTitle.Substring( idx + 1 );

				string zhName = TryResolveShotkey( locale, "quest.tome.noun.epic.name." + Slugify( name ) );
				string zhTitle = TryResolveShotkey( locale, "quest.tome.noun.epic.title." + Slugify( title ) );

				if ( zhName != null && zhTitle != null )
					return StripInlineAnnotation( zhTitle ) + StripInlineAnnotation( zhName ) + "（" + nameAndTitle + "）";
				if ( zhName != null )
					return StripInlineAnnotation( zhName ) + "（" + nameAndTitle + "）";
				if ( zhTitle != null )
					return StripInlineAnnotation( zhTitle ) + "（" + nameAndTitle + "）";
			}
			else
			{
				string zhName = TryResolveShotkey( locale, "quest.tome.noun.epic.name." + Slugify( nameAndTitle ) );
				if ( zhName != null )
					return StripInlineAnnotation( zhName ) + "（" + nameAndTitle + "）";
			}

			return nameAndTitle;
		}

		public static string LocalizedEvilTitle( string locale, string title )
		{
			if ( string.IsNullOrEmpty( title ) || !title.StartsWith( "the " ) )
				return title;

			string rest = title.Substring( 4 );
			int ofIdx = rest.LastIndexOf( " of " );
			if ( ofIdx < 0 )
				return title;

			string theme = rest.Substring( ofIdx + 4 );
			string mid = rest.Substring( 0, ofIdx );
			int lastSpace = mid.LastIndexOf( ' ' );
			if ( lastSpace < 0 )
				return title;

			string adj = mid.Substring( 0, lastSpace );
			string subs = mid.Substring( lastSpace + 1 );

			string zhTheme = ResolveRelicToken( locale, "evil.theme.", theme );
			string zhAdj = ResolveRelicToken( locale, "evil.adj.", adj );
			string zhSubs = ResolveRelicToken( locale, "evil.subs.", subs );

			return zhTheme + "之" + zhAdj + zhSubs;
		}

		public static string LocalizedQuestItemName( string locale, string englishItem )
		{
			if ( string.IsNullOrEmpty( englishItem ) )
				return englishItem;

			string item = englishItem.Trim();
			string ownerPrefix = null;

			int poss = item.IndexOf( "'s " );
			if ( poss < 0 )
				poss = item.IndexOf( "' " );

			if ( poss > 0 )
			{
				ownerPrefix = item.Substring( 0, poss );
				item = item.Substring( poss + ( item[poss + 1] == 's' ? 3 : 2 ) );
			}

			if ( item.StartsWith( "the ", StringComparison.OrdinalIgnoreCase ) )
				item = item.Substring( 4 );

			string ofPart = null;
			TryExtractOfPhrase( locale, ref item, out ofPart );

			string adjPhrase;
			string noun;
			TrySplitRelicBody( item, out adjPhrase, out noun );

			string zhNoun = ResolveRelicNoun( locale, noun );
			string zhAdj = ComposeRelicAdj( locale, adjPhrase );
			string zhOf = TranslateOfPhrase( locale, ofPart );

			if ( ownerPrefix != null )
			{
				string zhOwner = LocalizedPersonName( locale, ownerPrefix );

				if ( !string.IsNullOrEmpty( zhOf ) )
					return zhOwner + "的" + zhAdj + zhOf + zhNoun;

				return zhOwner + "的" + zhAdj + zhNoun;
			}

			if ( IsLiquidContainer( noun ) && !string.IsNullOrEmpty( ofPart ) )
			{
				if ( !string.IsNullOrEmpty( zhAdj ) )
					return zhAdj + "的" + zhOf + zhNoun;

				return zhOf + zhNoun;
			}

			if ( !string.IsNullOrEmpty( zhOf ) )
				return zhOf + "之" + zhAdj + zhNoun;

			return zhAdj + zhNoun;
		}

		private static string AnnotatedNoun( string locale, string keyPrefix, string englishValue )
		{
			string keyPart = englishValue.ToLowerInvariant().Replace( ' ', '_' ).Replace( "'", "" );
			string shotkey = keyPrefix + keyPart;
			string zh = StringCatalog.TryResolveByKey( locale, shotkey );
			if ( zh != null && !string.IsNullOrEmpty( zh ) )
				return zh + "（" + englishValue + "）";
			return englishValue;
		}

		public static string LocalizedCity( string locale )
		{
			return AnnotatedNoun( locale, "quest.tome.noun.city.", RandomThings.GetRandomCity() );
		}

		public static string LocalizedJob( string locale )
		{
			return AnnotatedNoun( locale, "quest.tome.noun.job.", RandomThings.GetRandomJob() );
		}

		public static string LocalizedDungeon( string locale, string dungeonValue )
		{
			if ( string.IsNullOrEmpty( dungeonValue ) )
				return dungeonValue;
			return AnnotatedNoun( locale, "quest.tome.noun.dungeon.", dungeonValue );
		}

		public static string LocalizedVillainCategory( string locale, string categoryValue )
		{
			if ( string.IsNullOrEmpty( categoryValue ) )
				return categoryValue;
			return AnnotatedNoun( locale, "quest.tome.noun.villain.", categoryValue );
		}

		/// <summary>
		/// Parse "Journal of Lysander the Mage" → "莱桑德（法师）"
		/// </summary>
		public static string AdventurerDisplayName( string locale, string bookName )
		{
			if ( string.IsNullOrEmpty( bookName ) )
				return bookName;

			string name = bookName;
			if ( name.StartsWith( "Journal of " ) )
				name = name.Substring( "Journal of ".Length );

			int idx = name.LastIndexOf( " the " );
			if ( idx > 0 )
			{
				string person = name.Substring( 0, idx );
				string title = name.Substring( idx + 5 );
				string titleKey = "quest.tome.noun.title." + title.ToLowerInvariant();
				string zhTitle = StringCatalog.TryResolveByKey( locale, titleKey );
				string zhPerson = LocalizedPersonName( locale, person );
				if ( zhTitle != null && !string.IsNullOrEmpty( zhTitle ) )
					return zhPerson + "（" + zhTitle + "）";
			}

			return LocalizedPersonName( locale, name );
		}
	}
}