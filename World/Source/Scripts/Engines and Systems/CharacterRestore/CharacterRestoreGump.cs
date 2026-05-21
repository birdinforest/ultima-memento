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

	/// <summary>
	/// Descriptor for one item found in a backup save.
	/// <see cref="SerializeBlob"/> is the raw <c>Items.bin</c> record used for full
	/// <see cref="Item.Deserialize"/> (weapons, armor, spellbooks, etc.).
	/// <see cref="FullProps"/> is a summary for the GM gump display only.
	/// </summary>
	public class BackupItemInfo
	{
		public string TypeFull   { get; set; }
		public string TypeShort  { get; set; }
		public bool   IsEquipped { get; set; }
		public bool   Selected   { get; set; }

		// Short fields mirrored from FullProps for quick gump display
		public int    Hue    { get; set; }
		public int    Amount { get; set; }
		public string Name   { get; set; }
		public string Layer  { get; set; }

		/// <summary>Exact bytes from <c>Items.bin</c> for this item's save record.</summary>
		public byte[] SerializeBlob { get; set; }

		/// <summary>
		/// Base-class properties parsed for gump display (optional).
		/// </summary>
		public BackupSaveAnalyzer.ParsedItemProps? FullProps { get; set; }

		public string DisplayLabel
		{
			get
			{
				var sb = new StringBuilder( TypeShort ?? "?" );
				if ( !string.IsNullOrEmpty( Name ) )
					sb.Append( " \"" ).Append( Name ).Append( '"' );
				if ( Amount > 1 )
					sb.Append( " x" ).Append( Amount );
				if ( FullProps.HasValue )
				{
					var p = FullProps.Value;
					if ( p.Resource != 0 )
						sb.Append( " [" ).Append( (CraftResource)p.Resource ).Append( "]" );
					if ( p.EnchantedSpell != 0 )
						sb.Append( " {" ).Append( (MagicSpell)p.EnchantedSpell ).Append( "}" );
					if ( p.ArtifactLevelVal != 0 )
						sb.Append( " <" ).Append( (ArtifactLevel)p.ArtifactLevelVal ).Append( ">" );
				}
				if ( IsEquipped )
					sb.Append( " [" ).Append( Layer ).Append( "]" );
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

			// 1. Find character serial — scan Mobiles.bin to match characterName
			List<int> charSerials = GetAccountCharSerials( accountsXml, accountName );
			int charSerial = MatchCharacterSerialByName( backupPath, charSerials, characterName, errors );

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

				var addedSerials = new HashSet<int>();

				// 5. Collect equipped items
				foreach ( int s in directItems )
				{
					if ( s == backpackSerial ) continue;
					AddItem( s, true, serialMap, propMap, binData, items, addedSerials );
				}

				// 6. Collect backpack contents
				if ( backpackSerial != -1 )
					CollectContainerContents( backpackSerial, childrenMap, serialMap, propMap, binData, items, addedSerials );
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
			byte[] binData,
			List<BackupItemInfo> items,
			HashSet<int> addedSerials )
		{
			if ( !serialMap.TryGetValue( serial, out var entry ) )
				return;
			string tf = entry.TypeFull ?? "";
			if ( tf.IndexOf( "BankBox", StringComparison.OrdinalIgnoreCase ) >= 0 )
				return;
			if ( !addedSerials.Add( serial ) )
				return;

			var info = new BackupItemInfo
			{
				TypeFull   = entry.TypeFull,
				TypeShort  = ShortTypeName( entry.TypeFull ),
				IsEquipped = isEquipped,
				Selected   = true,
				Amount     = 1,
			};

			if ( binData != null && entry.Length > 0 && entry.Pos >= 0 &&
			     entry.Pos + entry.Length <= binData.Length )
			{
				var blob = new byte[entry.Length];
				Buffer.BlockCopy( binData, (int)entry.Pos, blob, 0, entry.Length );
				info.SerializeBlob = blob;
			}

			if ( propMap.TryGetValue( serial, out var props ) )
			{
				info.Hue       = props.Hue;
				info.Amount    = props.Amount > 0 ? props.Amount : 1;
				info.Name      = props.Name;
				info.Layer     = props.Layer != 0 && LayerNames.ContainsKey( props.Layer )
					? LayerNames[props.Layer] : $"Layer_{props.Layer}";
				info.FullProps = props;  // attach complete parsed properties
			}

			items.Add( info );
		}

		private static void CollectContainerContents( int containerSerial,
			Dictionary<int,List<int>> childrenMap,
			Dictionary<int,(string TypeFull, long Pos, int Length)> serialMap,
			Dictionary<int,ParsedItemProps> propMap,
			byte[] binData,
			List<BackupItemInfo> items,
			HashSet<int> addedSerials,
			int depth = 0 )
		{
			if ( depth > 5 || !childrenMap.ContainsKey( containerSerial ) )
				return;

			foreach ( int s in childrenMap[containerSerial] )
			{
				AddItem( s, false, serialMap, propMap, binData, items, addedSerials );

				string tn = serialMap.TryGetValue( s, out var e ) ? e.TypeFull : "";
				if ( IsContainerType( tn ) )
					CollectContainerContents( s, childrenMap, serialMap, propMap, binData, items, addedSerials, depth + 1 );
			}
		}

		/// <summary>
		/// Reconstructs an item via <see cref="Item.Deserialize"/> from its backup blob,
		/// matching world load (weapons, armor, spellbook content, etc.).
		/// </summary>
		public static Item DeserializeItemFromBlob( string logPath, BackupItemInfo info, Type itemType )
		{
			if ( info == null || itemType == null || info.SerializeBlob == null || info.SerializeBlob.Length == 0 )
				return null;

			Item item;

			try
			{
				item = (Item)Activator.CreateInstance( itemType );
			}
			catch
			{
				return null;
			}

			if ( item == null )
				return null;

			World.BeginCharRestoreItemLoad( info.TypeFull );

			try
			{
				using ( var ms = new MemoryStream( info.SerializeBlob, false ) )
				using ( var binReader = new BinaryReader( ms, Encoding.UTF8 ) )
				{
					var reader = new BinaryFileReader( binReader );
					item.Deserialize( reader );

					if ( reader.Position != info.SerializeBlob.Length )
					{
						CharRestoreLogger.LogItemFail( logPath, info,
							$"Deserialize length mismatch: read {reader.Position}, expected {info.SerializeBlob.Length}" );
					}
				}
			}
			catch ( Exception ex )
			{
				CharRestoreLogger.LogItemFail( logPath, info, $"Deserialize failed: {ex.Message}" );
				try { if ( !item.Deleted ) item.Delete(); } catch { }
				return null;
			}
			finally
			{
				World.EndCharRestoreItemLoad();
			}

			if ( item.Deleted )
				return null;

			item.Parent = null;
			item.Internalize();

			return item;
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

		/// <summary>
		/// Returns all character serials listed for the account in accounts.xml.
		/// </summary>
		private static List<int> GetAccountCharSerials( string xmlPath, string accountName )
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

				var serials = new List<int>();
				foreach ( XmlElement charEl in charsEl.GetElementsByTagName( "char" ) )
				{
					if ( int.TryParse( charEl.InnerText.Trim(), out int serial ) && serial != 0 )
						serials.Add( serial );
				}

				if ( serials.Count == 0 )
					throw new InvalidOperationException( $"Account '{accountName}' has no valid character serials." );

				return serials;
			}

			throw new InvalidOperationException( $"Account '{accountName}' not found in accounts.xml." );
		}

		/// <summary>
		/// Scans Mobiles.bin to find which of <paramref name="charSerials"/> has a stored name
		/// matching <paramref name="characterName"/>.  Falls back to the first serial if the
		/// Mobiles files are missing or no name matches (and emits a warning entry in errors).
		/// </summary>
		private static int MatchCharacterSerialByName( string backupPath, List<int> charSerials,
			string characterName, List<string> errors )
		{
			if ( charSerials.Count == 1 || string.IsNullOrWhiteSpace( characterName ) )
				return charSerials[0];

			string mobIdx = Path.Combine( backupPath, "Mobiles", "Mobiles.idx" );
			string mobBin = Path.Combine( backupPath, "Mobiles", "Mobiles.bin" );

			if ( !File.Exists( mobIdx ) || !File.Exists( mobBin ) )
			{
				errors.Add( $"Mobiles.idx/Mobiles.bin not found — using first character on account (serial {charSerials[0]})." );
				return charSerials[0];
			}

			// Build serial → (pos, length) from Mobiles.idx (same format as Items.idx)
			var serialToEntry = new Dictionary<int,(long Pos, int Length)>();
			using ( var fs = new FileStream( mobIdx, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			using ( var br = new BinaryReader( fs ) )
			{
				int count = br.ReadInt32();
				var charSet = new HashSet<int>( charSerials );
				for ( int i = 0; i < count; i++ )
				{
					br.ReadInt32();          // TypeID — not needed
					int serial  = br.ReadInt32();
					long pos    = br.ReadInt64();
					int  length = br.ReadInt32();
					if ( charSet.Contains( serial ) )
						serialToEntry[serial] = (pos, length);
				}
			}

			byte[] binData    = File.ReadAllBytes( mobBin );
			byte[] nameBytes  = EncodeNetString( characterName );

			foreach ( int serial in charSerials )
			{
				if ( !serialToEntry.TryGetValue( serial, out var entry ) ) continue;
				long pos   = entry.Pos;
				int  len   = entry.Length;
				if ( pos < 0 || pos + len > binData.Length ) continue;

				if ( BytePatternExists( binData, (int)pos, len, nameBytes ) )
					return serial;
			}

			errors.Add( $"Character name '{characterName}' not found in Mobiles.bin — using first character on account (serial {charSerials[0]}). Verify the name is spelled exactly as it appears in-game." );
			return charSerials[0];
		}

		/// <summary>Encode a string as .NET BinaryWriter format (7-bit-encoded length + UTF-8 bytes).</summary>
		private static byte[] EncodeNetString( string s )
		{
			byte[] utf8   = Encoding.UTF8.GetBytes( s );
			int    length = utf8.Length;
			var header = new List<byte>( 5 );
			while ( length >= 0x80 )
			{
				header.Add( (byte)((length & 0x7F) | 0x80) );
				length >>= 7;
			}
			header.Add( (byte)length );
			var result = new byte[header.Count + utf8.Length];
			for ( int i = 0; i < header.Count; i++ ) result[i] = header[i];
			Buffer.BlockCopy( utf8, 0, result, header.Count, utf8.Length );
			return result;
		}

		/// <summary>Returns true if <paramref name="needle"/> appears anywhere in the
		/// <paramref name="length"/>-byte slice of <paramref name="haystack"/> starting
		/// at <paramref name="offset"/>.</summary>
		private static bool BytePatternExists( byte[] haystack, int offset, int length, byte[] needle )
		{
			int end = offset + length - needle.Length;
			for ( int i = offset; i <= end; i++ )
			{
				bool found = true;
				for ( int j = 0; j < needle.Length; j++ )
				{
					if ( haystack[i + j] != needle[j] ) { found = false; break; }
				}
				if ( found ) return true;
			}
			return false;
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
				if ( count < 0 || count > 100000 )
					throw new InvalidDataException( $"TDB type count {count} is implausible" );
				var names = new string[count];
				for ( int i = 0; i < count; i++ )
					names[i] = br.ReadString(); // type names are ASCII, no guard needed
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

		// ---- Property caps — game-validated maximum values ----

		// Hue: UO 16-bit palette index (max used value ~3000)
		public const int Cap_Hue           = 3000;
		// Stack count upper limit for stackable items
		public const int Cap_Amount        = 60000;
		// UO item graphic IDs are 16-bit (0x0000–0xFFFF)
		public const int Cap_ItemID        = 0xFFFF;
		// Coin price; no item should cost more than 10 million gold
		public const int Cap_CoinPrice     = 10_000_000;
		// EnchantMod modifier; keep well below any exploitable threshold
		public const int Cap_EnchantMod    = 100;
		// Identification attempt counter
		public const int Cap_NotIDAttempts = 100;
		// Wand / enchanted-item charge counts
		public const int Cap_EnchantUses   = 500;
		// Usage-limit counters (e.g. limited-use bags)
		public const int Cap_Limits        = 50_000;
		// Custom graphic ID (same range as ItemID)
		public const int Cap_GraphicID     = 0xFFFF;
		// String lengths
		public const int Cap_StrName       = 80;    // item name
		public const int Cap_StrSubName    = 80;    // resource name override
		public const int Cap_StrLimits     = 80;    // limits label
		public const int Cap_StrInfo       = 500;   // InfoData / InfoText*
		public const int Cap_StrColor      = 20;    // ColorHue / ColorText display tags

		// ---- Item binary record parser ----

		/// <summary>
		/// Base-class properties parseable from <c>Item.Serialize v14</c> for gump display.
		/// Restoration uses <see cref="BackupItemInfo.SerializeBlob"/> + full Deserialize.
		/// </summary>
		public struct ParsedItemProps
		{
			// ── Item.Serialize v14 block ────────────────────────────────────────
			public int    EnchantMod;
			public string ColorHue1,  ColorText1;
			public string ColorHue2,  ColorText2;
			public string ColorHue3,  ColorText3;
			public string ColorHue4,  ColorText4;
			public string ColorHue5,  ColorText5;
			public int    WorldItemID;      // custom gump graphic
			public bool   Technology;       // sci-fi/tech item flag
			public bool   VirtualContainer; // virtual container flag
			public bool   NotIdentified;    // item has hidden properties
			public int    NotIDAttempts;    // failed identification attempts
			public int    NotIDSource;      // Identity enum value
			public int    NotIDSkill;       // IDSkill enum value
			public int    Catalog;          // Catalogs enum value
			public int    CoinPrice;        // vendor buy/sell price hint
			public int    Resource;         // CraftResource enum (primary material)
			public int    SubResource;      // CraftResource enum (secondary material)
			public string SubName;          // material name override
			public int    ArtifactLevelVal; // ArtifactLevel enum value
			public bool   NotModAble;       // cannot be modified/altered
			public bool   NeedsBothHands;   // two-hand requirement override
			public string InfoData;         // extended description
			public string InfoText1, InfoText2, InfoText3, InfoText4, InfoText5;
			public int    Limits;           // current usage count
			public int    LimitsMax;        // maximum usage count
			public string LimitsName;       // label for limit type
			public bool   LimitsDelete;     // auto-delete on limit expiry
			public bool   Built;            // player-crafted flag
			// ── Item.Serialize v11 block ────────────────────────────────────────
			public int    EnchantedSpell;   // MagicSpell enum
			public int    EnchantUses;      // current charges
			public int    EnchantUsesMax;   // max charges
			// ── Item.Serialize v10 block ────────────────────────────────────────
			public int    GraphicID;        // custom graphic override
			public int    GraphicHue;       // custom graphic hue
			// ── Flags-based section ─────────────────────────────────────────────
			public int    Hue;
			public int    Amount;
			public byte   Layer;
			public string Name;
			public int?   Parent;
			public List<int> ChildrenSerials;
			// ── Meta ────────────────────────────────────────────────────────────
			public int ParsedVersion;  // which Item.Serialize version was found
		}

		// Maximum plausible string length guard: prevents malformed data from
		// allocating multi-MB strings that could OOM or stall the main thread.
		private const int MaxStringBytes = 4096;

		/// <summary>
		/// Safe string reader: validates the length prefix before allocating.
		/// Throws <see cref="InvalidDataException"/> when the encoded length exceeds
		/// <see cref="MaxStringBytes"/> or is negative, which would indicate corrupt data.
		/// </summary>
		private static string SafeReadString( BinaryReader br )
		{
			// BinaryReader.ReadString uses 7-bit encoded length prefix.
			// We replicate that here so we can validate before allocation.
			int length = 0, shift = 0;
			for ( int i = 0; i < 5; i++ )
			{
				byte b = br.ReadByte();
				length |= (b & 0x7F) << shift;
				if ( (b & 0x80) == 0 ) break;
				shift += 7;
			}
			if ( length < 0 || length > MaxStringBytes )
				throw new InvalidDataException( $"String length {length} exceeds guard of {MaxStringBytes}" );
			byte[] bytes = br.ReadBytes( length );
			if ( bytes.Length != length )
				throw new EndOfStreamException( $"Expected {length} bytes, got {bytes.Length}" );
			return Encoding.UTF8.GetString( bytes );
		}

		private static ParsedItemProps ParseItemRecord( BinaryReader br )
		{
			int version = br.ReadInt32();
			// Valid versions: 6–14. Item.Serialize falls through: 14 → 11 → 10 → 6.
			if ( version < 6 || version > 14 )
				throw new InvalidDataException( $"Item version {version} outside supported range 6-14" );

			var props = new ParsedItemProps
			{
				Amount          = 1,
				ChildrenSerials = new List<int>(),
				ParsedVersion   = version,
			};

			if ( version >= 14 )
			{
				br.ReadBoolean();                     // Purchased — not restored (vendor flag)
				props.EnchantMod    = br.ReadInt32();
				props.ColorHue1     = SafeReadShardBlobString( br );
				props.ColorText1    = SafeReadShardBlobString( br );
				props.ColorHue2     = SafeReadShardBlobString( br );
				props.ColorText2    = SafeReadShardBlobString( br );
				props.ColorHue3     = SafeReadShardBlobString( br );
				props.ColorText3    = SafeReadShardBlobString( br );
				props.ColorHue4     = SafeReadShardBlobString( br );
				props.ColorText4    = SafeReadShardBlobString( br );
				props.ColorHue5     = SafeReadShardBlobString( br );
				props.ColorText5    = SafeReadShardBlobString( br );
				props.WorldItemID   = br.ReadInt32();
				props.Technology    = br.ReadBoolean();
				props.VirtualContainer = br.ReadBoolean();
				props.NotIdentified = br.ReadBoolean();
				props.NotIDAttempts = br.ReadInt32();
				props.NotIDSource   = ReadEncodedInt( br );
				props.NotIDSkill    = ReadEncodedInt( br );
				props.Catalog       = ReadEncodedInt( br );
				props.CoinPrice     = br.ReadInt32();
				props.Resource      = ReadEncodedInt( br );
				props.SubResource   = ReadEncodedInt( br );
				props.SubName       = SafeReadShardBlobString( br );
				props.ArtifactLevelVal = br.ReadInt32();
				props.NotModAble    = br.ReadBoolean();
				props.NeedsBothHands = br.ReadBoolean();
				props.InfoData      = SafeReadShardBlobString( br );
				props.InfoText1     = SafeReadShardBlobString( br );
				props.InfoText2     = SafeReadShardBlobString( br );
				props.InfoText3     = SafeReadShardBlobString( br );
				props.InfoText4     = SafeReadShardBlobString( br );
				props.InfoText5     = SafeReadShardBlobString( br );
				props.Limits        = br.ReadInt32();
				props.LimitsMax     = br.ReadInt32();
				props.LimitsName    = SafeReadShardBlobString( br );
				props.LimitsDelete  = br.ReadBoolean();
				br.ReadInt32();                       // BuiltBy mobile ref — not restored
				props.Built         = br.ReadBoolean();
			}

			if ( version >= 11 )
			{
				props.EnchantedSpell = ReadEncodedInt( br );
				props.EnchantUses    = br.ReadInt32();
				props.EnchantUsesMax = br.ReadInt32();
			}

			if ( version >= 10 )
			{
				props.GraphicID    = br.ReadInt32();
				props.GraphicHue   = br.ReadInt32();
				br.ReadInt32();                       // LastMobile ref — not restored
				SafeReadShardBlobString( br );                 // LastMobileName — not restored
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
				props.Name = SafeReadShardBlobString( br );

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

		private static string SafeReadShardBlobString( BinaryReader br )
		{
			byte sentinel = br.ReadByte();
			if ( sentinel == 0 )
				return null;
			return SafeReadString( br );
		}

		private static int ReadEncodedInt( BinaryReader br )
		{
			// A 32-bit 7-bit-encoded integer takes at most 5 bytes.
			int result = 0, shift = 0;
			for ( int i = 0; i < 5; i++ )
			{
				byte b = br.ReadByte();
				result |= (b & 0x7F) << shift;
				if ( (b & 0x80) == 0 )
					break;
				shift += 7;
			}
			if ( (uint)result > 0x7FFFFFFFU )
				result = (int)( (long)(uint)result - 0x100000000L );
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
		private CharRestoreTheme m_Theme  = CharRestoreTheme.Ocean;

		public CharRestoreTheme Theme => m_Theme;

		public CharacterRestoreGump( Mobile from )
			: this( from, PageSetup, 0, "Saves", "", "", "", new List<BackupItemInfo>(), "", "", (int)CharRestoreTheme.Ocean )
		{}

		public CharacterRestoreGump( Mobile from, int page, int itemPage,
			string backupPath, string accountName, string charName,
			string statusText, List<BackupItemInfo> items,
			string targetPlayerName, string personalNote, int theme )
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
			m_Theme            = CharRestoreThemes.Parse( theme );

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

		y += 44;
		AddLabel( 20, y, 0x5A, L( "charrestore.gump.lbl.theme", "Adventure Theme:" ) );
		y += 22;
		DrawThemeOption( 20, y, 32, CharRestoreTheme.Wilderness,
			"charrestore.gump.theme.wilderness", "Wilderness" );
		DrawThemeOption( 185, y, 33, CharRestoreTheme.Ocean,
			"charrestore.gump.theme.ocean", "Ocean" );
		DrawThemeOption( 350, y, 34, CharRestoreTheme.Dungeon,
			"charrestore.gump.theme.dungeon", "Dungeon" );

		y += 32;
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

		private void DrawThemeOption( int x, int y, int buttonId, CharRestoreTheme theme, string labelKey, string labelFallback )
		{
			bool selected = m_Theme == theme;
			AddButton( x, y, selected ? 4006 : 4005, 4007, buttonId, GumpButtonType.Reply, 0 );
			AddLabel( x + 32, y + 2, selected ? 0x35 : 0x5A, L( labelKey, labelFallback ) );
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

		// Update item selections from checkboxes — ONLY for items visible on the current page.
		// Items on other pages are not rendered as checkboxes, so IsSwitched() would return
		// false for them, wrongly overwriting any selection made on a different page.
		if ( m_Page == PageItems )
		{
			int pageStart = m_ItemPage * ItemsPerPage;
			int pageEnd   = Math.Min( pageStart + ItemsPerPage, m_Items.Count );
			for ( int i = pageStart; i < pageEnd; i++ )
				m_Items[i].Selected = info.IsSwitched( 200 + i );
		}

			switch ( info.ButtonID )
			{
				case 0: // Close
					return;

				case 32:
					Reopen( from, PageNPCConfig, m_ItemPage, backupPath, accountName, charName,
						m_StatusText, m_Items, targetName, personalNote, CharRestoreTheme.Wilderness );
					return;
				case 33:
					Reopen( from, PageNPCConfig, m_ItemPage, backupPath, accountName, charName,
						m_StatusText, m_Items, targetName, personalNote, CharRestoreTheme.Ocean );
					return;
				case 34:
					Reopen( from, PageNPCConfig, m_ItemPage, backupPath, accountName, charName,
						m_StatusText, m_Items, targetName, personalNote, CharRestoreTheme.Dungeon );
					return;

			// Tab navigation
			case 101: // "Next >" from Setup, "< Back" from NPC Config, or Setup tab from Items
				if ( m_Page == PageItems )
					// Clicked the "Setup" tab while on the Items page → go back to Setup
					Reopen( from, PageSetup, 0, backupPath, accountName, charName,
						m_StatusText, m_Items, targetName, personalNote );
				else
					// "Next >" (Setup page) or "< Back" (NPC Config page) → go to Items
					Reopen( from, PageItems, m_ItemPage, backupPath, accountName, charName,
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
				// Navigate to the Items page automatically when analysis finds items;
				// stay on Setup page on failure so the error message is visible.
				int nextPage = ( items != null && items.Count > 0 ) ? PageItems : PageSetup;
				Reopen( from, nextPage, 0, backupPath, accountName, charName,
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
			Reopen( from, page, itemPage, backupPath, accountName, charName,
				status, items, targetName, personalNote, m_Theme );
		}

		private void Reopen( Mobile from, int page, int itemPage,
			string backupPath, string accountName, string charName,
			string status, List<BackupItemInfo> items,
			string targetName, string personalNote, CharRestoreTheme theme )
		{
			from.CloseGump( typeof( CharacterRestoreGump ) );
			from.SendGump( new CharacterRestoreGump( from, page, itemPage,
				backupPath, accountName, charName, status,
				items ?? m_Items, targetName, personalNote, (int)theme ) );
		}

	// ----------------------------------------------------------------
	// NPC spawning — defensive, fully logged
	// ----------------------------------------------------------------

	/// <summary>Maximum items allowed per restore session to prevent memory exhaustion.</summary>
	private const int MaxRestoreItems = 500;

	private void SpawnRestorerNPC( Mobile from, string targetName, string personalNote,
		string backupPath, string accountName, string charName )
	{
		// Guard: caller must be alive and have a valid map
		if ( from == null || !from.Alive || from.Map == null || from.Map == Map.Internal )
		{
			from?.SendMessage( 0x20, StringCatalog.ResolveByKey( from?.Account, "eng.you_must_be_in_the_world_to_spawn_the_npc_dot" ) );
			return;
		}

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

		if ( selectedItems.Count > MaxRestoreItems )
		{
			from.SendMessage( 0x20, $"Too many items selected ({selectedItems.Count}). Maximum is {MaxRestoreItems}." );
			return;
		}

		// Begin disk log
		string logPath = CharRestoreLogger.BeginSession(
			from, backupPath, accountName, charName,
			string.IsNullOrWhiteSpace( targetName ) ? "(not set)" : targetName,
			selectedItems.Count );

		// Build the restoration bag
		string bundleName = StringCatalog.TryResolveByKey(
			AccountLang.GetLanguageCode( from.Account ),
			CharRestoreThemes.ThemeKey( m_Theme, "npc.bundle_name" ) ) ?? "Restoration Bundle";

		CharRestoreBag bag = null;
		try { bag = new CharRestoreBag(); }
		catch ( Exception ex )
		{
			CharRestoreLogger.LogError( logPath, "Bag creation", ex );
			from.SendMessage( 0x20, StringCatalog.ResolveByKey( from.Account, "eng.internal_error_could_not_create_restoration_bag_dot" ) );
			return;
		}
		bag.Name = bundleName;
		bag.TargetName = string.IsNullOrWhiteSpace( targetName ) ? null : targetName.Trim();

		int created = 0, failed = 0;

		foreach ( BackupItemInfo itemInfo in selectedItems )
		{
			if ( itemInfo == null )
			{
				failed++;
				CharRestoreLogger.LogItemFail( logPath, new BackupItemInfo { TypeShort = "(null entry)" }, "Null BackupItemInfo" );
				continue;
			}

			Item newItem = TryCreateItem( logPath, itemInfo, ref failed );
			if ( newItem == null )
				continue;

			try
			{
				bag.DropItem( newItem );
				CharRestoreLogger.LogItemCreate( logPath, itemInfo, newItem );
				created++;
			}
			catch ( Exception ex )
			{
				CharRestoreLogger.LogItemFail( logPath, itemInfo, $"DropItem failed: {ex.Message}" );
				// Safe cleanup: delete the orphaned item so it doesn't persist in World
				try { if ( !newItem.Deleted ) newItem.Delete(); } catch { }
				failed++;
			}
		}

		if ( created == 0 )
		{
			CharRestoreLogger.LogError( logPath, "SpawnRestorerNPC",
				new InvalidOperationException( $"Zero items created ({failed} failures)." ) );
			try { bag.Delete(); } catch { }
			string msg = $"Could not create any items ({failed} failed). Check type names in the backup manifest.";
			from.SendMessage( 0x20, msg );
			Reopen( from, m_Page, m_ItemPage, backupPath, accountName, charName,
				msg, m_Items, targetName, personalNote );
			return;
		}

		// Spawn NPC
		LostItemsRestorerNPC npc;
		try
		{
			npc = new LostItemsRestorerNPC();
		}
		catch ( Exception ex )
		{
			CharRestoreLogger.LogError( logPath, "NPC constructor", ex );
			try { bag.Delete(); } catch { }
			from.SendMessage( 0x20, StringCatalog.ResolveByKey( from.Account, "eng.internal_error_could_not_create_restorer_npc_dot" ) );
			return;
		}

		npc.TargetName     = string.IsNullOrWhiteSpace( targetName ) ? null : targetName.Trim();
		npc.PersonalNote   = string.IsNullOrWhiteSpace( personalNote ) ? null : personalNote.Trim();
		npc.RestoreTheme   = m_Theme;
		npc.RestorationBag = bag;
		npc.LogPath        = logPath;

		try
		{
			npc.MoveToWorld( from.Location, from.Map );
		}
		catch ( Exception ex )
		{
			CharRestoreLogger.LogError( logPath, "NPC MoveToWorld", ex );
			try { npc.Delete(); bag.Delete(); } catch { }
			from.SendMessage( 0x20, StringCatalog.ResolveByKey( from.Account, "eng.internal_error_could_not_place_npc_in_world_dot" ) );
			return;
		}

		// Place the bag in the NPC's backpack (PlaceInBackpack — never fall back to world drop)
		npc.EnsureBackpack();

		if ( !npc.TryStoreRestorationBag() )
		{
			CharRestoreLogger.LogError( logPath, "NPC TryStoreRestorationBag",
				new InvalidOperationException( "Could not place restoration bag in NPC backpack." ) );
			try { npc.Delete(); bag.Delete(); } catch { }
			from.SendMessage( 0x20, StringCatalog.ResolveByKey( from.Account, "eng.internal_error_could_not_place_restoration_bag_on_npc_dot" ) );
			return;
		}

		// Visual feedback
		try
		{
			Effects.SendLocationParticles(
				EffectItem.Create( npc.Location, npc.Map, EffectItem.DefaultDuration ),
				0x3728, 10, 10, 2023 );
			npc.PlaySound( 0x1FE );
		}
		catch { /* Non-critical visual; never crash for this */ }

		CharRestoreLogger.LogSessionSummary( logPath, created, failed, bag, npc );

		string result = $"NPC spawned at your location with {created} item(s)" +
			( failed > 0 ? $" ({failed} type(s) could not be created — see log)" : "" ) +
			( !string.IsNullOrEmpty( npc.TargetName ) ? $". Waiting for '{npc.TargetName}'." : "." );

		from.SendMessage( 0x35, result );

		CommandLogging.WriteLine( from,
			$"[CharRestore] Spawned LostItemsRestorerNPC 0x{npc.Serial.Value:X8} " +
			$"with {created} items for '{targetName}' at {from.Location} ({from.Map})." );

		Reopen( from, PageNPCConfig, m_ItemPage, backupPath, accountName, charName,
			result, m_Items, targetName, personalNote );
	}

	/// <summary>
	/// Attempts to create a single game item from a <see cref="BackupItemInfo"/>
	/// using full save deserialization when <see cref="BackupItemInfo.SerializeBlob"/>
	/// is available.
	/// </summary>
	private static Item TryCreateItem( string logPath, BackupItemInfo info, ref int failed )
	{
		// ── 1. Locate the type ──────────────────────────────────────────
		Type t = null;
		try
		{
			if ( !string.IsNullOrEmpty( info.TypeFull ) )
				t = ScriptCompiler.FindTypeByFullName( info.TypeFull );
			if ( t == null && !string.IsNullOrEmpty( info.TypeShort ) )
				t = ScriptCompiler.FindTypeByName( info.TypeShort );
		}
		catch ( Exception ex )
		{
			CharRestoreLogger.LogItemFail( logPath, info, $"Type lookup exception: {ex.Message}" );
			failed++; return null;
		}

		if ( t == null )
		{
			CharRestoreLogger.LogItemFail( logPath, info, "Type not found in script assembly" );
			failed++; return null;
		}

		if ( !typeof( Item ).IsAssignableFrom( t ) || t.IsAbstract )
		{
			CharRestoreLogger.LogItemFail( logPath, info, "Type is not a concrete Item subclass" );
			failed++; return null;
		}

		if ( t.GetConstructor( Type.EmptyTypes ) == null )
		{
			CharRestoreLogger.LogItemFail( logPath, info, "No no-arg constructor — cannot safely instantiate" );
			failed++; return null;
		}

		// ── 2. Full deserialize from backup blob (subclass stats, spellbook content, …) ──
		Item newItem = BackupSaveAnalyzer.DeserializeItemFromBlob( logPath, info, t );

		if ( newItem != null && !newItem.Deleted )
			return newItem;

		// ── 3. Fallback: base fields only when blob missing or deserialize failed ──
		try { newItem = (Item)Activator.CreateInstance( t ); }
		catch ( Exception ex )
		{
			CharRestoreLogger.LogItemFail( logPath, info,
				$"Constructor threw: {ex.InnerException?.Message ?? ex.Message}" );
			failed++; return null;
		}

		if ( newItem == null || newItem.Deleted )
		{
			CharRestoreLogger.LogItemFail( logPath, info, "Item null or deleted after construction" );
			failed++; return null;
		}

		CharRestoreLogger.LogItemFail( logPath, info,
			"Full deserialize unavailable — restored base Item fields only (stats/spells may differ)." );

		if ( info.FullProps.HasValue )
			ApplyBackupProperties( newItem, info.FullProps.Value, logPath, info );
		else
			ApplyBasicFields( newItem, info.Hue, info.Amount, info.Name, logPath, info );

		return newItem;
	}

	/// <summary>
	/// Applies all base-class properties from a <see cref="BackupSaveAnalyzer.ParsedItemProps"/>
	/// to <paramref name="item"/>.  Each numeric field is clamped to a game-validated maximum;
	/// enum fields are validated with <c>Enum.IsDefined</c> before assignment; string fields
	/// are sanitized and length-capped.  Nothing outside these base-class fields is touched.
	/// </summary>
	private static void ApplyBackupProperties( Item item,
		BackupSaveAnalyzer.ParsedItemProps p, string logPath, BackupItemInfo info )
	{
		// ── Hue (0–3000) ─────────────────────────────────────────────────
		if ( p.Hue > 0 )
		{
			int h = Clamp( p.Hue, 0, BackupSaveAnalyzer.Cap_Hue );
			if ( h != p.Hue )
				CharRestoreLogger.LogItemFail( logPath, info, $"Hue {p.Hue} clamped to {h}" );
			item.Hue = h;
		}

		// ── Amount (stackable items only) ────────────────────────────────
		if ( p.Amount > 1 )
		{
			if ( item.Stackable )
			{
				int a = Clamp( p.Amount, 1, BackupSaveAnalyzer.Cap_Amount );
				if ( a != p.Amount )
					CharRestoreLogger.LogItemFail( logPath, info, $"Amount {p.Amount} clamped to {a}" );
				item.Amount = a;
			}
			// Non-stackable items always have Amount = 1; silently skip
		}

		// ── Name ────────────────────────────────────────────────────────
		string name = SanitizeName( p.Name, BackupSaveAnalyzer.Cap_StrName );
		if ( !string.IsNullOrEmpty( name ) )
			item.Name = name;

		// ── EnchantMod ──────────────────────────────────────────────────
		if ( p.EnchantMod != 0 )
			item.EnchantMod = Clamp( p.EnchantMod, 0, BackupSaveAnalyzer.Cap_EnchantMod );

		// ── Color / text labels (display only, no gameplay effect) ───────
		item.ColorHue1  = SanitizeName( p.ColorHue1,  BackupSaveAnalyzer.Cap_StrColor );
		item.ColorText1 = SanitizeName( p.ColorText1, BackupSaveAnalyzer.Cap_StrColor );
		item.ColorHue2  = SanitizeName( p.ColorHue2,  BackupSaveAnalyzer.Cap_StrColor );
		item.ColorText2 = SanitizeName( p.ColorText2, BackupSaveAnalyzer.Cap_StrColor );
		item.ColorHue3  = SanitizeName( p.ColorHue3,  BackupSaveAnalyzer.Cap_StrColor );
		item.ColorText3 = SanitizeName( p.ColorText3, BackupSaveAnalyzer.Cap_StrColor );
		item.ColorHue4  = SanitizeName( p.ColorHue4,  BackupSaveAnalyzer.Cap_StrColor );
		item.ColorText4 = SanitizeName( p.ColorText4, BackupSaveAnalyzer.Cap_StrColor );
		item.ColorHue5  = SanitizeName( p.ColorHue5,  BackupSaveAnalyzer.Cap_StrColor );
		item.ColorText5 = SanitizeName( p.ColorText5, BackupSaveAnalyzer.Cap_StrColor );

		// ── WorldItemID (custom graphic) ─────────────────────────────────
		if ( p.WorldItemID > 0 && p.WorldItemID <= BackupSaveAnalyzer.Cap_ItemID )
			item.WorldItemID = p.WorldItemID;
		else if ( p.WorldItemID != 0 )
			CharRestoreLogger.LogItemFail( logPath, info,
				$"WorldItemID {p.WorldItemID} out of range; not applied" );

		// ── Boolean flags ────────────────────────────────────────────────
		item.Technology      = p.Technology;
		item.VirtualContainer = p.VirtualContainer;
		item.NotIdentified   = p.NotIdentified;
		item.NotModAble      = p.NotModAble;
		item.NeedsBothHands  = p.NeedsBothHands;
		item.LimitsDelete    = p.LimitsDelete;
		item.Built           = p.Built;

		// ── NotIDAttempts ────────────────────────────────────────────────
		item.NotIDAttempts = Clamp( p.NotIDAttempts, 0, BackupSaveAnalyzer.Cap_NotIDAttempts );

		// ── Enum fields ──────────────────────────────────────────────────
		if ( Enum.IsDefined( typeof( Identity ), p.NotIDSource ) )
			item.NotIDSource = (Identity)p.NotIDSource;
		if ( Enum.IsDefined( typeof( IDSkill ), p.NotIDSkill ) )
			item.NotIDSkill = (IDSkill)p.NotIDSkill;
		if ( Enum.IsDefined( typeof( Catalogs ), p.Catalog ) )
			item.Catalog = (Catalogs)p.Catalog;
		if ( Enum.IsDefined( typeof( CraftResource ), p.Resource ) )
			item.Resource = (CraftResource)p.Resource;
		if ( Enum.IsDefined( typeof( CraftResource ), p.SubResource ) )
			item.SubResource = (CraftResource)p.SubResource;
		if ( Enum.IsDefined( typeof( ArtifactLevel ), p.ArtifactLevelVal ) )
			item.ArtifactLevel = (ArtifactLevel)p.ArtifactLevelVal;
		if ( Enum.IsDefined( typeof( MagicSpell ), p.EnchantedSpell ) )
			item.Enchanted = (MagicSpell)p.EnchantedSpell;

		// ── CoinPrice ────────────────────────────────────────────────────
		if ( p.CoinPrice > 0 )
			item.CoinPrice = Clamp( p.CoinPrice, 0, BackupSaveAnalyzer.Cap_CoinPrice );

		// ── SubName (material name override) ─────────────────────────────
		item.SubName = SanitizeName( p.SubName, BackupSaveAnalyzer.Cap_StrSubName );

		// ── Info strings ─────────────────────────────────────────────────
		item.InfoData  = SanitizeName( p.InfoData,  BackupSaveAnalyzer.Cap_StrInfo );
		item.InfoText1 = SanitizeName( p.InfoText1, BackupSaveAnalyzer.Cap_StrInfo );
		item.InfoText2 = SanitizeName( p.InfoText2, BackupSaveAnalyzer.Cap_StrInfo );
		item.InfoText3 = SanitizeName( p.InfoText3, BackupSaveAnalyzer.Cap_StrInfo );
		item.InfoText4 = SanitizeName( p.InfoText4, BackupSaveAnalyzer.Cap_StrInfo );
		item.InfoText5 = SanitizeName( p.InfoText5, BackupSaveAnalyzer.Cap_StrInfo );

		// ── Enchantment charges ───────────────────────────────────────────
		// Set max first so current never exceeds max.
		if ( p.EnchantUsesMax > 0 )
			item.EnchantUsesMax = Clamp( p.EnchantUsesMax, 0, BackupSaveAnalyzer.Cap_EnchantUses );
		if ( p.EnchantUses > 0 )
			item.EnchantUses = Clamp( p.EnchantUses, 0,
				Math.Min( item.EnchantUsesMax, BackupSaveAnalyzer.Cap_EnchantUses ) );

		// ── Usage limits ─────────────────────────────────────────────────
		// Set max first, then current (current ≤ max).
		if ( p.LimitsMax > 0 )
		{
			item.LimitsMax = Clamp( p.LimitsMax, 0, BackupSaveAnalyzer.Cap_Limits );
			if ( p.Limits > 0 )
				item.Limits = Clamp( p.Limits, 0, item.LimitsMax );
		}
		item.LimitsName = SanitizeName( p.LimitsName, BackupSaveAnalyzer.Cap_StrLimits );

		// ── Custom graphic ────────────────────────────────────────────────
		if ( p.GraphicID > 0 && p.GraphicID <= BackupSaveAnalyzer.Cap_GraphicID )
			item.GraphicID = p.GraphicID;
		if ( p.GraphicHue > 0 )
			item.GraphicHue = Clamp( p.GraphicHue, 0, BackupSaveAnalyzer.Cap_Hue );
	}

	/// Fallback when FullProps is null — apply only the three flat fields.
	private static void ApplyBasicFields( Item item,
		int hue, int amount, string name, string logPath, BackupItemInfo info )
	{
		if ( hue > 0 && hue <= BackupSaveAnalyzer.Cap_Hue )
			item.Hue = hue;
		else if ( hue != 0 )
			CharRestoreLogger.LogItemFail( logPath, info, $"Hue {hue} out of range; not applied" );

		if ( amount > 1 && item.Stackable && amount <= BackupSaveAnalyzer.Cap_Amount )
			item.Amount = amount;

		string n = SanitizeName( name, BackupSaveAnalyzer.Cap_StrName );
		if ( !string.IsNullOrEmpty( n ) )
			item.Name = n;
	}

	private static int Clamp( int value, int min, int max )
		=> value < min ? min : value > max ? max : value;

	private static string SanitizeName( string raw, int maxLen )
	{
		if ( string.IsNullOrEmpty( raw ) ) return null;
		var sb = new StringBuilder( Math.Min( raw.Length, maxLen ) );
		foreach ( char c in raw )
		{
			if ( sb.Length >= maxLen ) break;
			if ( c >= 0x20 && c != 0x7F ) sb.Append( c );
		}
		return sb.ToString().Trim();
	}

	private static string SanitizeName( string raw )
		=> SanitizeName( raw, BackupSaveAnalyzer.Cap_StrName );

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
					m_BackupPath, m_AccountName, m_CharName, m_StatusText, m_Items, name, m_PersonalNote,
					(int)m_Gump.Theme ) );
			}

			protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
			{
				from.CloseGump( typeof( CharacterRestoreGump ) );
				from.SendGump( new CharacterRestoreGump( from, PageNPCConfig, 0,
					m_BackupPath, m_AccountName, m_CharName, m_StatusText, m_Items, "", m_PersonalNote,
					(int)m_Gump.Theme ) );
			}
		}
	}
}
