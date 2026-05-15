using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Localization;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Gumps
{
	// ========================================================================
	// Command registration
	// ========================================================================

	public static class CharacterRestoreCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register( "CharRestore", AccessLevel.GameMaster,
				new CommandEventHandler( OnCommand ) );
		}

		[Usage( "CharRestore" )]
		[Description( "Opens the Character Item Restore GM tool." )]
		private static void OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			from.CloseGump( typeof( CharacterRestoreGump ) );
			from.SendGump( new CharacterRestoreGump( from ) );
		}
	}

	// ========================================================================
	// Backup item descriptor
	// ========================================================================

	public class BackupItemInfo
	{
		public string TypeFull   { get; set; }
		public string TypeShort  { get; set; }
		public int    Hue        { get; set; }
		public int    Amount     { get; set; }
		public string Name       { get; set; }
		public string Layer      { get; set; }
		public bool   IsEquipped { get; set; }
		public bool   Selected   { get; set; }

		public string DisplayLabel
		{
			get
			{
				var sb = new StringBuilder( TypeShort );
				if ( !string.IsNullOrEmpty( Name ) )
					sb.Append( $" \"{Name}\"" );
				if ( Amount > 1 )
					sb.Append( $" x{Amount}" );
				if ( IsEquipped )
					sb.Append( $" [{Layer}]" );
				return sb.ToString();
			}
		}
	}

	// ========================================================================
	// C# in-game backup save analyzer
	// ========================================================================

	public static class BackupSaveAnalyzer
	{
		// ----- SaveFlag constants (mirrors Item.cs) -----
		private const int SF_Direction       = 0x00000001;
		private const int SF_Bounce          = 0x00000002;
		private const int SF_LootType        = 0x00000004;
		private const int SF_LocationFull    = 0x00000008;
		private const int SF_ItemID          = 0x00000010;
		private const int SF_Hue             = 0x00000020;
		private const int SF_Amount          = 0x00000040;
		private const int SF_Layer           = 0x00000080;
		private const int SF_Name            = 0x00000100;
		private const int SF_Parent          = 0x00000200;
		private const int SF_Items           = 0x00000400;
		private const int SF_WeightNot1or0   = 0x00000800;
		private const int SF_Map             = 0x00001000;
		private const int SF_LocationSByteZ  = 0x00020000;
		private const int SF_LocationShortXY = 0x00040000;
		private const int SF_LocationByteXY  = 0x00080000;
		private const int SF_IntWeight       = 0x01000000;

		private static readonly HashSet<byte> EquippedLayers = new HashSet<byte> {
			0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,
			0x0A,0x0C,0x0D,0x0E,0x0F,0x11,0x12,0x13,0x14,
			0x16,0x17,0x18
		};

		private static readonly Dictionary<byte,string> LayerNames = new Dictionary<byte,string> {
			{0x00,"Invalid"},{0x01,"OneHanded"},{0x02,"TwoHanded"},{0x03,"Shoes"},
			{0x04,"Pants"},{0x05,"Shirt"},{0x06,"Helm"},{0x07,"Gloves"},
			{0x08,"Ring"},{0x09,"Trinket"},{0x0A,"Neck"},{0x0B,"Hair"},
			{0x0C,"Waist"},{0x0D,"InnerTorso"},{0x0E,"Bracelet"},{0x0F,"Special"},
			{0x10,"FacialHair"},{0x11,"MiddleTorso"},{0x12,"Earrings"},{0x13,"Arms"},
			{0x14,"Cloak"},{0x15,"Backpack"},{0x16,"OuterTorso"},{0x17,"OuterLegs"},
			{0x18,"InnerLegs"},{0x19,"Mount"},{0x1A,"ShopBuy"},{0x1B,"ShopResale"},
			{0x1C,"ShopSell"},{0x1D,"Bank"}
		};

		public static List<BackupItemInfo> Analyze( string backupPath, string accountName,
			string characterName, out string statusMessage )
		{
			var items = new List<BackupItemInfo>();
			var errors = new List<string>();

			try
			{
				string accountsXml = Path.Combine( backupPath, "Accounts", "accounts.xml" );
				string itemsTdb    = Path.Combine( backupPath, "Items", "Items.tdb" );
				string itemsIdx    = Path.Combine( backupPath, "Items", "Items.idx" );
				string itemsBin    = Path.Combine( backupPath, "Items", "Items.bin" );

				foreach ( var p in new[] { accountsXml, itemsTdb, itemsIdx, itemsBin } )
					if ( !File.Exists( p ) )
						throw new FileNotFoundException( $"Required save file not found: {p}" );

				// 1. Find character serial
				int charSerial = FindCharacterSerial( accountsXml, accountName );

				// 2. Build type lookup
				string[] typeNames = ReadTdb( itemsTdb );
				var idxEntries = ReadIdx( itemsIdx );

				// serial → (typeName, pos, length)
				var serialMap = new Dictionary<int, (string TypeFull, long Pos, int Length)>();
				foreach ( var e in idxEntries )
				{
					string tn = (e.TypeID >= 0 && e.TypeID < typeNames.Length)
						? typeNames[e.TypeID] : "Unknown";
					serialMap[e.Serial] = (tn, e.Pos, e.Length);
				}

				// 3. Parse Items.bin for parent relationships
				byte[] binData = File.ReadAllBytes( itemsBin );
				var parentMap   = new Dictionary<int,int>();   // child → parent
				var childrenMap = new Dictionary<int,List<int>>(); // parent → [children]
				var propMap     = new Dictionary<int,ParsedItemProps>();

				int parseOk = 0, parseFail = 0;
				foreach ( var kv in serialMap )
				{
					int serial = kv.Key;
					long pos   = kv.Value.Pos;
					int length = kv.Value.Length;

					if ( pos < 0 || pos + length > binData.Length )
						continue;

					try
					{
						using ( var ms = new MemoryStream( binData, (int)pos, length ) )
						using ( var br = new BinaryReader( ms, Encoding.UTF8 ) )
						{
							var props = ParseItemRecord( br );
							propMap[serial] = props;

							if ( props.Parent.HasValue )
							{
								int p = props.Parent.Value;
								parentMap[serial] = p;
								if ( !childrenMap.ContainsKey( p ) )
									childrenMap[p] = new List<int>();
								childrenMap[p].Add( serial );
							}

							foreach ( int childSerial in props.ChildrenSerials )
							{
								parentMap[childSerial] = serial;
								if ( !childrenMap.ContainsKey( serial ) )
									childrenMap[serial] = new List<int>();
								childrenMap[serial].Add( childSerial );
							}

							parseOk++;
						}
					}
					catch
					{
						parseFail++;
					}
				}

				// 4. Find direct items of the character
				var directItems = new List<int>();
				if ( childrenMap.ContainsKey( charSerial ) )
					directItems.AddRange( childrenMap[charSerial] );

				// Find backpack serial
				int backpackSerial = -1;
				foreach ( int s in directItems )
				{
					if ( propMap.TryGetValue( s, out var p ) && p.Layer == 0x15 )
					{
						backpackSerial = s;
						break;
					}
					if ( serialMap.TryGetValue( s, out var entry ) &&
					     entry.TypeFull.IndexOf( "Backpack", StringComparison.OrdinalIgnoreCase ) >= 0 )
					{
						backpackSerial = s;
						break;
					}
				}

				// 5. Collect equipped items
				foreach ( int s in directItems )
				{
					if ( s == backpackSerial ) continue;
					AddItem( s, true, serialMap, propMap, items );
				}

				// 6. Collect backpack contents
				if ( backpackSerial != -1 )
					CollectContainerContents( backpackSerial, childrenMap, serialMap, propMap, items );
				else
					errors.Add( "Backpack not found — only equipped items listed." );

				int total = items.Count;
				statusMessage = $"Found {total} items (parsed {parseOk}/{parseOk+parseFail})" +
					( errors.Count > 0 ? $"; {errors.Count} warning(s)" : "" ) +
					". Select items to restore and configure the NPC below.";

				return items;
			}
			catch ( Exception ex )
			{
				statusMessage = $"Analysis failed: {ex.Message}";
				return items;
			}
		}

		private static void AddItem( int serial, bool isEquipped,
			Dictionary<int,(string TypeFull, long Pos, int Length)> serialMap,
			Dictionary<int,ParsedItemProps> propMap,
			List<BackupItemInfo> items )
		{
			if ( !serialMap.TryGetValue( serial, out var entry ) )
				return;

			var info = new BackupItemInfo
			{
				TypeFull   = entry.TypeFull,
				TypeShort  = ShortTypeName( entry.TypeFull ),
				IsEquipped = isEquipped,
				Selected   = true,
			};

			if ( propMap.TryGetValue( serial, out var props ) )
			{
				info.Hue    = props.Hue;
				info.Amount = props.Amount > 0 ? props.Amount : 1;
				info.Name   = props.Name;
				info.Layer  = props.Layer != 0 && LayerNames.ContainsKey( props.Layer )
					? LayerNames[props.Layer] : $"Layer_{props.Layer}";
			}
			else
			{
				info.Amount = 1;
			}

			items.Add( info );
		}

		private static void CollectContainerContents( int containerSerial,
			Dictionary<int,List<int>> childrenMap,
			Dictionary<int,(string TypeFull, long Pos, int Length)> serialMap,
			Dictionary<int,ParsedItemProps> propMap,
			List<BackupItemInfo> items,
			int depth = 0 )
		{
			if ( depth > 5 || !childrenMap.ContainsKey( containerSerial ) )
				return;

			foreach ( int s in childrenMap[containerSerial] )
			{
				AddItem( s, false, serialMap, propMap, items );

				string tn = serialMap.TryGetValue( s, out var e ) ? e.TypeFull : "";
				if ( IsContainerType( tn ) )
					CollectContainerContents( s, childrenMap, serialMap, propMap, items, depth + 1 );
			}
		}

		private static bool IsContainerType( string typeFull )
		{
			string s = typeFull ?? "";
			return s.IndexOf( "Bag", StringComparison.OrdinalIgnoreCase ) >= 0
				|| s.IndexOf( "Backpack", StringComparison.OrdinalIgnoreCase ) >= 0
				|| s.IndexOf( "Pouch", StringComparison.OrdinalIgnoreCase ) >= 0
				|| s.IndexOf( "Container", StringComparison.OrdinalIgnoreCase ) >= 0
				|| s.IndexOf( "Chest", StringComparison.OrdinalIgnoreCase ) >= 0
				|| s.IndexOf( "Box", StringComparison.OrdinalIgnoreCase ) >= 0
				|| s.IndexOf( "Sack", StringComparison.OrdinalIgnoreCase ) >= 0;
		}

		private static string ShortTypeName( string fullName )
		{
			if ( string.IsNullOrEmpty( fullName ) ) return "Unknown";
			int dot = fullName.LastIndexOf( '.' );
			return dot >= 0 ? fullName.Substring( dot + 1 ) : fullName;
		}

		// ---- Accounts XML ----

		private static int FindCharacterSerial( string xmlPath, string accountName )
		{
			var doc = new XmlDocument();
			doc.Load( xmlPath );

			foreach ( XmlElement acctEl in doc.DocumentElement.GetElementsByTagName( "account" ) )
			{
				string username = GetXmlAttrOrText( acctEl, "username" );
				if ( string.IsNullOrEmpty( username ) )
				{
					var un = acctEl["username"];
					username = un?.InnerText?.Trim() ?? "";
				}

				if ( !username.Equals( accountName, StringComparison.OrdinalIgnoreCase ) )
					continue;

				var charsEl = acctEl["chars"];
				if ( charsEl == null )
					throw new InvalidOperationException( $"Account '{accountName}' has no characters." );

				foreach ( XmlElement charEl in charsEl.GetElementsByTagName( "char" ) )
				{
					if ( int.TryParse( charEl.InnerText.Trim(), out int serial ) && serial != 0 )
						return serial;
				}

				throw new InvalidOperationException( $"Account '{accountName}' has no valid character serials." );
			}

			throw new InvalidOperationException( $"Account '{accountName}' not found in accounts.xml." );
		}

		private static string GetXmlAttrOrText( XmlElement el, string attrName )
		{
			return el.GetAttribute( attrName )?.Trim() ?? "";
		}

		// ---- TDB / IDX readers ----

		private static string[] ReadTdb( string path )
		{
			using ( var fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			using ( var br = new BinaryReader( fs, Encoding.UTF8 ) )
			{
				int count = br.ReadInt32();
				var names = new string[count];
				for ( int i = 0; i < count; i++ )
					names[i] = br.ReadString();
				return names;
			}
		}

		private struct IndexEntry
		{
			public int TypeID;
			public int Serial;
			public long Pos;
			public int Length;
		}

		private static List<IndexEntry> ReadIdx( string path )
		{
			using ( var fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			using ( var br = new BinaryReader( fs ) )
			{
				int count = br.ReadInt32();
				var list = new List<IndexEntry>( count );
				for ( int i = 0; i < count; i++ )
					list.Add( new IndexEntry {
						TypeID = br.ReadInt32(),
						Serial = br.ReadInt32(),
						Pos    = br.ReadInt64(),
						Length = br.ReadInt32(),
					} );
				return list;
			}
		}

		// ---- Item binary record parser ----

		private struct ParsedItemProps
		{
			public int    Hue;
			public int    Amount;
			public string Name;
			public byte   Layer;
			public int?   Parent;
			public List<int> ChildrenSerials;
		}

		private static ParsedItemProps ParseItemRecord( BinaryReader br )
		{
			int version = br.ReadInt32();
			if ( version < 6 || version > 14 )
				throw new InvalidDataException( $"Item version {version} not supported" );

			var props = new ParsedItemProps { Amount = 1, ChildrenSerials = new List<int>() };

			if ( version >= 14 )
			{
				br.ReadBoolean();   // Purchased
				br.ReadInt32();     // EnchantMod
				// ColorHue1..5 and ColorText1..5 (all strings in this codebase)
				for ( int i = 0; i < 10; i++ ) br.ReadString();
				br.ReadInt32();     // WorldItemID
				br.ReadBoolean();   // Technology
				br.ReadBoolean();   // VirtualContainer
				br.ReadBoolean();   // NotIdentified
				br.ReadInt32();     // NotIDAttempts
				ReadEncodedInt( br );   // NotIDSource
				ReadEncodedInt( br );   // NotIDSkill
				ReadEncodedInt( br );   // Catalog
				br.ReadInt32();     // CoinPrice
				ReadEncodedInt( br );   // Resource
				ReadEncodedInt( br );   // SubResource
				br.ReadString();    // SubName
				br.ReadInt32();     // ArtifactLevel
				br.ReadBoolean();   // NotModAble
				br.ReadBoolean();   // NeedsBothHands
				for ( int i = 0; i < 6; i++ ) br.ReadString(); // InfoData + InfoText1..5
				br.ReadInt32();     // Limits
				br.ReadInt32();     // LimitsMax
				br.ReadString();    // LimitsName
				br.ReadBoolean();   // LimitsDelete
				br.ReadInt32();     // BuiltBy (mobile ref)
				br.ReadBoolean();   // Built
			}

			if ( version >= 11 )
			{
				ReadEncodedInt( br );   // Enchanted
				br.ReadInt32();     // EnchantUses
				br.ReadInt32();     // EnchantUsesMax
			}

			if ( version >= 10 )
			{
				br.ReadInt32();     // GraphicID
				br.ReadInt32();     // GraphicHue
				br.ReadInt32();     // LastMobile (mobile ref)
				br.ReadString();    // LastMobileName
			}

			// ---- case 6 ----
			int flags = br.ReadInt32();

			if ( version >= 7 )
				ReadEncodedInt( br );   // minutes since last moved
			else
				br.ReadInt64();         // DeltaTime (ticks)

			if ( (flags & SF_Direction) != 0 )
				br.ReadByte();

			if ( (flags & SF_Bounce) != 0 )
				SkipBounceInfo( br );

			if ( (flags & SF_LootType) != 0 )
				br.ReadByte();

			if ( (flags & SF_LocationFull) != 0 )
			{
				ReadEncodedInt( br ); ReadEncodedInt( br ); ReadEncodedInt( br );
			}
			else
			{
				if ( (flags & SF_LocationByteXY) != 0 ) { br.ReadByte(); br.ReadByte(); }
				else if ( (flags & SF_LocationShortXY) != 0 ) { br.ReadInt16(); br.ReadInt16(); }
				if ( (flags & SF_LocationSByteZ) != 0 ) br.ReadSByte();
			}

			if ( (flags & SF_ItemID) != 0 )
				ReadEncodedInt( br );

			if ( (flags & SF_Hue) != 0 )
				props.Hue = ReadEncodedInt( br );

			if ( (flags & SF_Amount) != 0 )
				props.Amount = ReadEncodedInt( br );

			if ( (flags & SF_Layer) != 0 )
				props.Layer = br.ReadByte();

			if ( (flags & SF_Name) != 0 )
				props.Name = br.ReadString();

			if ( (flags & SF_Parent) != 0 )
				props.Parent = br.ReadInt32();

			if ( (flags & SF_Items) != 0 )
			{
				int count = br.ReadInt32();
				for ( int i = 0; i < count; i++ )
					props.ChildrenSerials.Add( br.ReadInt32() );
			}

			return props;
		}

		private static void SkipBounceInfo( BinaryReader br )
		{
			bool present = br.ReadBoolean();
			if ( present )
			{
				br.ReadByte();      // Map (1 byte)
				br.ReadInt32();     // Location.X
				br.ReadInt32();     // Location.Y
				br.ReadInt32();     // Location.Z
				br.ReadInt32();     // WorldLoc.X
				br.ReadInt32();     // WorldLoc.Y
				br.ReadInt32();     // WorldLoc.Z
				br.ReadInt32();     // parent serial
			}
		}

		private static int ReadEncodedInt( BinaryReader br )
		{
			int result = 0, shift = 0;
			while ( true )
			{
				byte b = br.ReadByte();
				result |= (b & 0x7F) << shift;
				if ( (b & 0x80) == 0 ) break;
				shift += 7;
			}
			if ( result >= 0x80000000 ) result = (int)(result - 0x100000000L);
			return result;
		}
	}

	// ========================================================================
	// GM Gump — multi-step wizard
	// ========================================================================

	public class CharacterRestoreGump : Gump
	{
		private const int PageSetup    = 0;
		private const int PageItems    = 1;
		private const int PageNPCConfig = 2;

		private const int GumpW = 520;
		private const int GumpH = 460;
		private const string TitleColor  = "#f0e6c0";
		private const string LabelColor  = "#d5c8a2";
		private const string ErrorColor  = "#ff8080";
		private const string GreenColor  = "#80ff80";
		private const int ItemsPerPage   = 12;

		private Mobile m_From;
		private int    m_Page;
		private int    m_ItemPage;

		// Setup page fields (stored in gump state via text entries)
		private string m_BackupPath   = "Saves";
		private string m_AccountName  = "";
		private string m_CharName     = "";
		private string m_StatusText   = "Enter backup path, account, and character name, then click Analyze.";

		// Item list (populated after analysis)
		private List<BackupItemInfo> m_Items = new List<BackupItemInfo>();

		// NPC config
		private string m_TargetPlayerName = "";
		private string m_PersonalNote     = "";

		public CharacterRestoreGump( Mobile from )
			: this( from, PageSetup, 0, "Saves", "", "", "", new List<BackupItemInfo>(), "", "" )
		{}

		public CharacterRestoreGump( Mobile from, int page, int itemPage,
			string backupPath, string accountName, string charName,
			string statusText, List<BackupItemInfo> items,
			string targetPlayerName, string personalNote )
			: base( 60, 40 )
		{
			m_From             = from;
			m_Page             = page;
			m_ItemPage         = itemPage;
			m_BackupPath       = backupPath;
			m_AccountName      = accountName;
			m_CharName         = charName;
			m_StatusText       = statusText ?? "";
			m_Items            = items ?? new List<BackupItemInfo>();
			m_TargetPlayerName = targetPlayerName;
			m_PersonalNote     = personalNote;

		Closable   = true;
		Disposable = true;
		Dragable   = true;
		Resizable  = false;

		AddPage( 0 );
		AddBackground( 0, 0, GumpW, GumpH, 9250 );
		AddAlphaRegion( 10, 10, GumpW - 20, GumpH - 20 );

		// Title bar
		AddHtml( 10, 12, GumpW - 20, 22,
			"<BODY><BASEFONT Color=" + TitleColor + "><CENTER>" +
			L( "charrestore.gump.title", "Character Item Restore — GM Tool" ) +
			"</CENTER></BASEFONT></BODY>",
			false, false );

		// Tab row
		DrawTab( 80,  40, 1, L( "charrestore.gump.tab.setup", "1. Setup" ),      page == PageSetup );
		DrawTab( 210, 40, 2, L( "charrestore.gump.tab.items", "2. Items" ),       page == PageItems );
		DrawTab( 340, 40, 3, L( "charrestore.gump.tab.npc",   "3. NPC & Spawn" ), page == PageNPCConfig );

			switch ( page )
			{
				case PageSetup:     DrawSetupPage();    break;
				case PageItems:     DrawItemsPage();    break;
				case PageNPCConfig: DrawNPCConfigPage(); break;
			}
		}

	/// <summary>Resolves a charrestore.gump.* key for the current GM's language.</summary>
	private string L( string key, string fallback )
	{
		string lang = AccountLang.GetLanguageCode( m_From != null ? m_From.Account : null );
		string s    = StringCatalog.TryResolveByKey( lang, key );
		return ( s != null && s.Length > 0 ) ? s : fallback;
	}

	private void DrawTab( int x, int y, int btnId, string label, bool active )
	{
		AddButton( x, y, active ? 4006 : 4005, 4007, 100 + btnId, GumpButtonType.Reply, 0 );
		string color = active ? TitleColor : LabelColor;
		AddHtml( x + 35, y, 120, 20,
			"<BODY><BASEFONT Color=" + color + ">" + label + "</BASEFONT></BODY>",
			false, false );
	}

		// ----------------------------------------------------------------
		// Page 0 — Setup
		// ----------------------------------------------------------------

	private void DrawSetupPage()
	{
		int y = 75;
		AddLabel( 20, y, 0x5A, L( "charrestore.gump.lbl.backup_path", "Backup Saves Path:" ) );
		AddBackground( 170, y - 2, 320, 22, 9350 );
		AddTextEntry( 172, y, 316, 20, 0, 10, m_BackupPath );

		y += 30;
		AddLabel( 20, y, 0x5A, L( "charrestore.gump.lbl.account", "Account Name:" ) );
		AddBackground( 170, y - 2, 320, 22, 9350 );
		AddTextEntry( 172, y, 316, 20, 0, 11, m_AccountName );

		y += 30;
		AddLabel( 20, y, 0x5A, L( "charrestore.gump.lbl.character", "Character Name:" ) );
		AddBackground( 170, y - 2, 320, 22, 9350 );
		AddTextEntry( 172, y, 316, 20, 0, 12, m_CharName );

		y += 40;
		AddButton( 20, y, 4005, 4007, 10, GumpButtonType.Reply, 0 );
		AddLabel( 58, y + 2, 0x35, L( "charrestore.gump.btn.analyze", "Analyze Backup" ) );

		y += 35;
		string statusDisplay = m_StatusText;
		if ( string.IsNullOrEmpty( statusDisplay ) )
			statusDisplay = L( "charrestore.gump.msg.setup_hint",
				"Enter backup path, account, and character name, then click Analyze." );

		bool isError = statusDisplay.StartsWith( "Analysis failed" ) ||
		               statusDisplay.StartsWith( "Error" );
		string statusColor = isError ? ErrorColor :
			( m_Items.Count > 0 ? GreenColor : LabelColor );
		string safeStatus = statusDisplay.Replace( "&", "&amp;" ).Replace( "<", "&lt;" )
			.Replace( ">", "&gt;" ).Replace( "\n", "<BR>" );
		AddHtml( 20, y, GumpW - 40, 120,
			"<BODY><BASEFONT Color=" + statusColor + ">" + safeStatus + "</BASEFONT></BODY>",
			false, true );

		y = GumpH - 45;
		if ( m_Items.Count > 0 )
		{
			AddButton( GumpW - 110, y, 4005, 4007, 101, GumpButtonType.Reply, 0 );
			AddLabel( GumpW - 72, y + 2, 0x35, L( "charrestore.gump.btn.next", "Next >" ) );
		}
	}

		// ----------------------------------------------------------------
		// Page 1 — Item list
		// ----------------------------------------------------------------

		private void DrawItemsPage()
		{
			int start    = m_ItemPage * ItemsPerPage;
			int end      = Math.Min( start + ItemsPerPage, m_Items.Count );
			int totalPages = (int)Math.Ceiling( (double)m_Items.Count / ItemsPerPage );

		string itemsHint = string.Format(
			L( "charrestore.gump.msg.items_hint",
				"Items from backup ({0} total). Check items to include in the restoration." ),
			m_Items.Count );
		AddHtml( 20, 72, GumpW - 40, 20,
			"<BODY><BASEFONT Color=" + LabelColor + ">" + itemsHint + "</BASEFONT></BODY>",
			false, false );

		// Header row
		AddLabel( 20,  92, 0x64, L( "charrestore.gump.lbl.include", "Include" ) );
		AddLabel( 80,  92, 0x64, "Type" );
		AddLabel( 330, 92, 0x64, L( "charrestore.gump.lbl.hue",    "Hue" ) );
		AddLabel( 380, 92, 0x64, L( "charrestore.gump.lbl.amount", "Amt" ) );
		AddLabel( 430, 92, 0x64, L( "charrestore.gump.lbl.layer",  "Layer" ) );

			// Item rows
			int y = 110;
			for ( int i = start; i < end; i++ )
			{
				BackupItemInfo item = m_Items[i];
				int checkId = 200 + i;

				AddCheck( 22, y, 0xD2, 0xD3, item.Selected, checkId );
				AddLabel( 50, y, item.IsEquipped ? 0x59 : 0x5A, item.TypeShort );

				if ( !string.IsNullOrEmpty( item.Name ) )
					AddLabel( 220, y, 0x480, $"({item.Name})" );

				AddLabel( 330, y, 0x5A, item.Hue == 0 ? "—" : $"#{item.Hue:X}" );
				AddLabel( 385, y, 0x5A, item.Amount.ToString() );
				AddLabel( 430, y, item.IsEquipped ? 0x59 : 0x480, item.Layer ?? "" );

				y += 22;
			}

		// Pagination
		int paginateY = GumpH - 70;
		if ( m_ItemPage > 0 )
		{
			AddButton( 20, paginateY, 0x15E3, 0x15E7, 20, GumpButtonType.Reply, 0 );
			AddLabel( 40, paginateY + 2, 0x5A, L( "charrestore.gump.btn.prev", "< Prev" ) );
		}
		if ( m_ItemPage < totalPages - 1 )
		{
			AddButton( 120, paginateY, 0x15E1, 0x15E5, 21, GumpButtonType.Reply, 0 );
			AddLabel( 100, paginateY + 2, 0x5A, L( "charrestore.gump.btn.next", "Next >" ) );
		}
		AddLabel( 200, paginateY + 2, 0x5A,
			$"Page {m_ItemPage + 1}/{Math.Max(1, totalPages)}" );

		// Select all / Clear all
		AddButton( 300, paginateY, 4005, 4007, 22, GumpButtonType.Reply, 0 );
		AddLabel( 338, paginateY + 2, 0x35, L( "charrestore.gump.btn.select_all", "Select All" ) );
		AddButton( 400, paginateY, 4005, 4007, 23, GumpButtonType.Reply, 0 );
		AddLabel( 438, paginateY + 2, 0x20, L( "charrestore.gump.btn.clear_all",  "Clear All" ) );

		// Nav buttons
		int navY = GumpH - 45;
		AddButton( 20, navY, 4005, 4007, 100, GumpButtonType.Reply, 0 );
		AddLabel( 58, navY + 2, 0x5A, L( "charrestore.gump.btn.back", "< Back" ) );
		AddButton( GumpW - 110, navY, 4005, 4007, 102, GumpButtonType.Reply, 0 );
		AddLabel( GumpW - 72, navY + 2, 0x35, L( "charrestore.gump.btn.next", "Next >" ) );
		}

		// ----------------------------------------------------------------
		// Page 2 — NPC Config + Spawn
		// ----------------------------------------------------------------

		private void DrawNPCConfigPage()
		{
			int selectedCount = 0;
			foreach ( var item in m_Items )
				if ( item.Selected ) selectedCount++;

		int y = 72;
		string npcHint = string.Format(
			L( "charrestore.gump.msg.npc_hint",
				"Configure the restoration NPC. It will spawn at your current location " +
				"and await the target player. {0} item(s) will be placed in the restoration bag." ),
			selectedCount );
		AddHtml( 20, y, GumpW - 40, 40,
			"<BODY><BASEFONT Color=" + LabelColor + ">" + npcHint + "</BASEFONT></BODY>",
			false, false );

		y += 50;
		AddLabel( 20, y, 0x5A, L( "charrestore.gump.lbl.target", "Target Player Name:" ) );
		AddBackground( 190, y - 2, 290, 22, 9350 );
		AddTextEntry( 192, y, 286, 20, 0, 30, m_TargetPlayerName );

		y += 10;
		AddButton( 20, y + 20, 4005, 4007, 30, GumpButtonType.Reply, 0 );
		AddLabel( 58, y + 22, 0x5A,
			L( "charrestore.gump.btn.target", "Click to target player ingame" ) );

		y += 55;
		AddLabel( 20, y, 0x5A, L( "charrestore.gump.lbl.note", "Personal Note (optional):" ) );
		y += 20;
		AddBackground( 20, y, GumpW - 40, 60, 9350 );
		AddTextEntry( 22, y + 2, GumpW - 44, 56, 0, 31, m_PersonalNote );

		y += 80;
		AddHtml( 20, y, GumpW - 40, 40,
			"<BODY><BASEFONT Color=" + LabelColor + ">" +
			L( "charrestore.gump.msg.npc_lifecycle",
				"The NPC will introduce itself as a sea salvager who recovered the player's lost items. " +
				"It auto-deletes after 24 hours or immediately upon item delivery." ) +
			"</BASEFONT></BODY>", false, false );

		// Spawn button
		int navY = GumpH - 45;
		AddButton( 20, navY, 4005, 4007, 101, GumpButtonType.Reply, 0 );
		AddLabel( 58, navY + 2, 0x5A, L( "charrestore.gump.btn.back", "< Back" ) );

		if ( selectedCount > 0 )
		{
			AddButton( GumpW - 150, navY, 4005, 4007, 40, GumpButtonType.Reply, 0 );
			AddLabel( GumpW - 112, navY + 2, 0x35, L( "charrestore.gump.btn.spawn", "Spawn NPC" ) );
		}
		else
		{
			AddLabel( GumpW - 200, navY + 2, 0x20,
				L( "charrestore.gump.msg.no_items_btn", "(No items selected)" ) );
		}
		}

		// ----------------------------------------------------------------
		// Response handler
		// ----------------------------------------------------------------

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;
			if ( from == null || !from.Alive )
				return;

			// Read current text entries regardless of button
			string backupPath  = GetText( info, 10, m_BackupPath );
			string accountName = GetText( info, 11, m_AccountName );
			string charName    = GetText( info, 12, m_CharName );
			string targetName  = GetText( info, 30, m_TargetPlayerName );
			string personalNote = GetText( info, 31, m_PersonalNote );

			// Update item selections from checkboxes (only when on items page)
			if ( m_Page == PageItems )
			{
				for ( int i = 0; i < m_Items.Count; i++ )
					m_Items[i].Selected = info.IsSwitched( 200 + i );
			}

			switch ( info.ButtonID )
			{
				case 0: // Close
					return;

				// Tab navigation
				case 101: // Page Setup → Items (or back from NPC page)
					if ( m_Page == PageNPCConfig )
						Reopen( from, PageItems, m_ItemPage, backupPath, accountName, charName,
							m_StatusText, m_Items, targetName, personalNote );
					else
						Reopen( from, PageSetup, 0, backupPath, accountName, charName,
							m_StatusText, m_Items, targetName, personalNote );
					return;
				case 102: // Items → NPC Config
					Reopen( from, PageNPCConfig, m_ItemPage, backupPath, accountName, charName,
						m_StatusText, m_Items, targetName, personalNote );
					return;
				case 100: // Tab: Setup
					Reopen( from, PageSetup, 0, backupPath, accountName, charName,
						m_StatusText, m_Items, targetName, personalNote );
					return;
				case 103: // Tab: Items
					Reopen( from, PageItems, m_ItemPage, backupPath, accountName, charName,
						m_StatusText, m_Items, targetName, personalNote );
					return;

				// Analyze backup
				case 10:
				{
				if ( string.IsNullOrWhiteSpace( backupPath ) ||
				     string.IsNullOrWhiteSpace( accountName ) ||
				     string.IsNullOrWhiteSpace( charName ) )
				{
					string hint = StringCatalog.TryResolveByKey(
						AccountLang.GetLanguageCode( from.Account ),
						"charrestore.gump.msg.setup_hint" )
						?? "Enter backup path, account, and character name, then click Analyze.";
					Reopen( from, PageSetup, 0, backupPath, accountName, charName,
						hint, null, targetName, personalNote );
					return;
				}

					string status;
					var items = BackupSaveAnalyzer.Analyze( backupPath, accountName, charName, out status );
					Reopen( from, PageSetup, 0, backupPath, accountName, charName,
						status, items, targetName, personalNote );
					return;
				}

				// Item list pagination
				case 20:
					Reopen( from, PageItems, Math.Max( 0, m_ItemPage - 1 ),
						backupPath, accountName, charName, m_StatusText, m_Items, targetName, personalNote );
					return;
				case 21:
				{
					int maxPage = (int)Math.Ceiling( (double)m_Items.Count / ItemsPerPage ) - 1;
					Reopen( from, PageItems, Math.Min( maxPage, m_ItemPage + 1 ),
						backupPath, accountName, charName, m_StatusText, m_Items, targetName, personalNote );
					return;
				}
				case 22: // Select all
					foreach ( var item in m_Items ) item.Selected = true;
					Reopen( from, PageItems, m_ItemPage,
						backupPath, accountName, charName, m_StatusText, m_Items, targetName, personalNote );
					return;
				case 23: // Clear all
					foreach ( var item in m_Items ) item.Selected = false;
					Reopen( from, PageItems, m_ItemPage,
						backupPath, accountName, charName, m_StatusText, m_Items, targetName, personalNote );
					return;

				// Target player button
				case 30:
			from.CloseGump( typeof( CharacterRestoreGump ) );
			from.Target = new TargetPlayerTarget( this, backupPath, accountName, charName,
				m_StatusText, m_Items, personalNote );
			from.SendMessage( StringCatalog.TryResolveByKey(
				AccountLang.GetLanguageCode( from.Account ),
				"charrestore.gump.msg.target_player" )
				?? "Target the player to restore items to." );
					return;

				// Spawn NPC
				case 40:
					SpawnRestorerNPC( from, targetName, personalNote, backupPath, accountName, charName );
					return;
			}
		}

		private static string GetText( RelayInfo info, int id, string fallback )
		{
			TextRelay relay = info.GetTextEntry( id );
			return relay?.Text ?? fallback ?? "";
		}

		private void Reopen( Mobile from, int page, int itemPage,
			string backupPath, string accountName, string charName,
			string status, List<BackupItemInfo> items,
			string targetName, string personalNote )
		{
			from.CloseGump( typeof( CharacterRestoreGump ) );
			from.SendGump( new CharacterRestoreGump( from, page, itemPage,
				backupPath, accountName, charName, status,
				items ?? m_Items, targetName, personalNote ) );
		}

		// ----------------------------------------------------------------
		// NPC spawning
		// ----------------------------------------------------------------

		private void SpawnRestorerNPC( Mobile from, string targetName, string personalNote,
			string backupPath, string accountName, string charName )
		{
			var selectedItems = new List<BackupItemInfo>();
			foreach ( var item in m_Items )
				if ( item.Selected )
					selectedItems.Add( item );

		if ( selectedItems.Count == 0 )
		{
			string noItemsMsg = StringCatalog.TryResolveByKey(
				AccountLang.GetLanguageCode( from.Account ),
				"charrestore.gump.msg.select_first" )
				?? "No items selected. Check at least one item to restore.";
			from.SendMessage( 0x20, noItemsMsg );
			Reopen( from, m_Page, m_ItemPage, backupPath, accountName, charName,
				m_StatusText, m_Items, targetName, personalNote );
			return;
		}

		// Build the restoration bag
		string bundleName = StringCatalog.TryResolveByKey(
			AccountLang.GetLanguageCode( from.Account ),
			"charrestore.npc.bundle_name" ) ?? "Restoration Bundle";
		Bag bag = new Bag();
		bag.Name = bundleName;
			int created = 0, failed = 0;

			foreach ( BackupItemInfo itemInfo in selectedItems )
			{
				try
				{
					Type t = ScriptCompiler.FindTypeByFullName( itemInfo.TypeFull );
					if ( t == null )
					{
						t = ScriptCompiler.FindTypeByName( itemInfo.TypeShort );
					}
					if ( t == null || !t.IsSubclassOf( typeof( Item ) ) )
					{
						failed++;
						continue;
					}

					Item newItem = (Item)Activator.CreateInstance( t );
					if ( newItem == null ) { failed++; continue; }

					if ( itemInfo.Hue > 0 )
						newItem.Hue = itemInfo.Hue;
					if ( itemInfo.Amount > 1 && newItem.Stackable )
						newItem.Amount = itemInfo.Amount;
					if ( !string.IsNullOrEmpty( itemInfo.Name ) )
						newItem.Name = itemInfo.Name;

					bag.DropItem( newItem );
					created++;
				}
				catch
				{
					failed++;
				}
			}

			if ( created == 0 )
			{
				bag.Delete();
				string msg = $"Could not create any items ({failed} failed). Check type names in the backup manifest.";
				from.SendMessage( 0x20, msg );
				Reopen( from, m_Page, m_ItemPage, backupPath, accountName, charName,
					msg, m_Items, targetName, personalNote );
				return;
			}

			// Spawn the NPC
			LostItemsRestorerNPC npc = new LostItemsRestorerNPC();
			npc.TargetName      = string.IsNullOrWhiteSpace( targetName ) ? null : targetName.Trim();
			npc.PersonalNote    = string.IsNullOrWhiteSpace( personalNote ) ? null : personalNote.Trim();
			npc.RestorationBag  = bag;
			npc.MoveToWorld( from.Location, from.Map );

			// Put the bag in the NPC's backpack (not equipped)
			npc.AddToBackpack( bag );

			// Visual effect
			Effects.SendLocationParticles(
				EffectItem.Create( npc.Location, npc.Map, EffectItem.DefaultDuration ),
				0x3728, 10, 10, 2023 );
			npc.PlaySound( 0x1FE );

			string result = $"NPC spawned at your location with {created} items" +
				( failed > 0 ? $" ({failed} item types could not be created)" : "" ) +
				( !string.IsNullOrEmpty( npc.TargetName ) ? $". Waiting for '{npc.TargetName}'." : "." );

			from.SendMessage( 0x35, result );

			CommandLogging.WriteLine( from,
				$"[CharRestore] Spawned LostItemsRestorerNPC with {created} items for '{targetName}' " +
				$"at {from.Location} ({from.Map})" );

			Reopen( from, PageNPCConfig, m_ItemPage, backupPath, accountName, charName,
				result, m_Items, targetName, personalNote );
		}

		// ----------------------------------------------------------------
		// Player targeting helper
		// ----------------------------------------------------------------

		private class TargetPlayerTarget : Target
		{
			private CharacterRestoreGump m_Gump;
			private string m_BackupPath, m_AccountName, m_CharName, m_StatusText, m_PersonalNote;
			private List<BackupItemInfo> m_Items;

			public TargetPlayerTarget( CharacterRestoreGump gump,
				string backupPath, string accountName, string charName,
				string statusText, List<BackupItemInfo> items, string personalNote )
				: base( 12, false, TargetFlags.None )
			{
				m_Gump         = gump;
				m_BackupPath   = backupPath;
				m_AccountName  = accountName;
				m_CharName     = charName;
				m_StatusText   = statusText;
				m_Items        = items;
				m_PersonalNote = personalNote;
			}

		protected override void OnTarget( Mobile from, object targeted )
		{
			string name = "";
			if ( targeted is Mobile m )
				name = m.Name ?? "";
			else
				from.SendMessage( StringCatalog.TryResolveByKey(
					AccountLang.GetLanguageCode( from.Account ),
					"charrestore.gump.msg.not_player" )
					?? "That is not a player." );

				from.CloseGump( typeof( CharacterRestoreGump ) );
				from.SendGump( new CharacterRestoreGump( from, PageNPCConfig, 0,
					m_BackupPath, m_AccountName, m_CharName, m_StatusText, m_Items, name, m_PersonalNote ) );
			}

			protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
			{
				from.CloseGump( typeof( CharacterRestoreGump ) );
				from.SendGump( new CharacterRestoreGump( from, PageNPCConfig, 0,
					m_BackupPath, m_AccountName, m_CharName, m_StatusText, m_Items, "", m_PersonalNote ) );
			}
		}
	}
}
