using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Network;
using Server.Gumps;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Misc;
using Server.Localization;
using Server.Gumps;

namespace Server.Mobiles
{
	/// <summary>
	/// A special NPC spawned by a GM to return a player's lost belongings.
	/// Only the designated target player can interact with it; all others
	/// receive a deflection message. Automatically removes itself after 24 hours
	/// or immediately after successful item delivery.
	///
	/// All player-facing strings are resolved via logical keys in
	/// <c>Data/Localization/*/charrestore.json</c>.
	/// </summary>
	public class LostItemsRestorerNPC : BasePerson
	{
		private string m_TargetName;
		private Bag m_RestorationBag;
		private string m_PersonalNote;
		private DateTime m_CreatedTime;
		private Timer m_DeleteTimer;
		private string m_LogPath;   // Absolute path to disk log for this session
		private bool m_ItemsDelivered;
		private CharRestoreTheme m_Theme = CharRestoreTheme.Ocean;

		[CommandProperty( AccessLevel.GameMaster )]
		public CharRestoreTheme RestoreTheme
		{
			get { return m_Theme; }
			set { m_Theme = value; ApplyTheme(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public string TargetName
		{
			get { return m_TargetName; }
			set { m_TargetName = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public string PersonalNote
		{
			get { return m_PersonalNote; }
			set { m_PersonalNote = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Bag RestorationBag
		{
			get { return m_RestorationBag; }
			set { m_RestorationBag = value; }
		}

		/// <summary>
		/// Absolute path to the restore session log file.
		/// Set by <c>CharacterRestoreGump.SpawnRestorerNPC</c>; persisted across restarts.
		/// </summary>
		[CommandProperty( AccessLevel.GameMaster )]
		public string LogPath
		{
			get { return m_LogPath; }
			set { m_LogPath = value; }
		}

		[Constructable]
		public LostItemsRestorerNPC() : base()
		{
			SpeechHue      = Utility.RandomTalkHue();
			NameHue        = 0xB5C;
			Hue            = Utility.RandomSkinColor();
			AI             = AIType.AI_Citizen;
			FightMode      = FightMode.None;
			m_CreatedTime  = DateTime.Now;
			ApplyTheme();

			if ( this.Female = Utility.RandomBool() )
			{
				Body = 0x191;
				Name = NameList.RandomName( "female" );
				AddItem( new Skirt( Utility.RandomNeutralHue() ) );
				Utility.AssignRandomHair( this );
				HairHue = Utility.RandomHairHue();
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName( "male" );
				AddItem( new ShortPants( Utility.RandomNeutralHue() ) );
				Utility.AssignRandomHair( this );
				int hairColor = Utility.RandomHairHue();
				FacialHairItemID = Utility.RandomList( 0, 8254, 8255, 8256, 8257 );
				HairHue          = hairColor;
				FacialHairHue    = hairColor;
			}

			AddItem( new Boots( Utility.RandomNeutralHue() ) );
			AddItem( new FancyShirt( Utility.RandomNeutralHue() ) );

			switch ( Utility.Random( 4 ) )
			{
				case 0: AddItem( new FloppyHat( Utility.RandomNeutralHue() ) ); break;
				case 1: AddItem( new WideBrimHat( Utility.RandomNeutralHue() ) ); break;
				case 2: AddItem( new StrawHat( Utility.RandomNeutralHue() ) ); break;
				case 3: AddItem( new TallStrawHat( Utility.RandomNeutralHue() ) ); break;
			}

			SetStr( 100 ); SetDex( 100 ); SetInt( 100 );
			SetDamage( 5, 10 );
			SetDamageType( ResistanceType.Physical, 100 );
			SetResistance( ResistanceType.Physical, 30, 40 );
			VirtualArmor = 20;

			StartDeleteTimer();
		}

		public LostItemsRestorerNPC( Serial serial ) : base( serial ) {}

		public string GetThemeKey( string suffix )
		{
			return CharRestoreThemes.ThemeKey( m_Theme, suffix );
		}

		public void ApplyTheme()
		{
			string title = StringCatalog.TryResolveByKey( LangConfig.DefaultLanguage, GetThemeKey( "npc.title" ) );

			if ( string.IsNullOrEmpty( title ) )
			{
				switch ( m_Theme )
				{
					case CharRestoreTheme.Wilderness: title = "the wilderness guide"; break;
					case CharRestoreTheme.Dungeon:    title = "the dungeon delver"; break;
					default:                          title = "the sea salvager"; break;
				}
			}

			Title = title;
		}

		public void SayTheme( string suffix, string fallback )
		{
			CitizenLocalization.SayLocalizedByKey( this, GetThemeKey( suffix ), fallback );
		}

		// ------------------------------------------------------------------
		// Interaction
		// ------------------------------------------------------------------

		public override bool IsEnemy( Mobile m ) { return false; }
		public override bool IsInvulnerable { get { return true; } }
		public override bool OnBeforeDeath() { return false; }

		public override bool CanPaperdollBeOpenedBy( Mobile from )
		{
			return from != null && from.AccessLevel >= AccessLevel.GameMaster;
		}

		public override bool CheckLift( Mobile from, Item item, ref LRReason reject )
		{
			if ( m_RestorationBag != null && !m_RestorationBag.Deleted &&
				( item == m_RestorationBag || item.IsChildOf( m_RestorationBag ) ) )
			{
				if ( from == null || from.AccessLevel < AccessLevel.GameMaster )
				{
					reject = LRReason.Inspecific;
					return false;
				}
			}

			return base.CheckLift( from, item, ref reject );
		}

		/// <summary>
		/// NPCs need a backpack before restoration bundles can be stored; vendors create one in ctor.
		/// </summary>
		public void EnsureBackpack()
		{
			if ( Backpack != null )
				return;

			Container pack = new Backpack();
			pack.Movable = false;
			AddItem( pack );
		}

		/// <summary>
		/// Keeps the restoration bundle inside this NPC's backpack (never on the ground at their feet).
		/// </summary>
		public bool TryStoreRestorationBag()
		{
			if ( m_RestorationBag == null || m_RestorationBag.Deleted )
				return false;

			EnsureBackpack();

			Container pack = Backpack;

			if ( pack == null )
				return false;

			m_RestorationBag.Movable = false;

			if ( m_RestorationBag.Parent == pack )
				return true;

			return pack.TryDropItem( this, m_RestorationBag, false );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from == null || !from.Alive )
				return;

			if ( m_TargetName != null && !m_TargetName.Equals( from.Name, StringComparison.OrdinalIgnoreCase ) )
			{
				SayTheme( "npc.deflect", "I am waiting for someone. Move along, traveler." );
				return;
			}

			if ( m_RestorationBag == null || m_RestorationBag.Deleted )
			{
				SayTheme( "npc.lost_parcel",
					"I seem to have lost the parcel I was carrying. Speak with the authorities." );
				return;
			}

			if ( !from.HasGump( typeof( LostItemsDialogGump ) ) )
				from.SendGump( new LostItemsDialogGump( from, this, 0 ) );
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
		{
			base.GetContextMenuEntries( from, list );

			bool isTarget = m_TargetName == null ||
				m_TargetName.Equals( from.Name, StringComparison.OrdinalIgnoreCase );

			if ( from.Alive && isTarget &&
				 m_RestorationBag != null && !m_RestorationBag.Deleted )
			{
				list.Add( new SalvagerTalkEntry( from, this ) );
			}
		}

		// ------------------------------------------------------------------
		// Item delivery
		// ------------------------------------------------------------------

		public void DeliverItems( Mobile to )
		{
			if ( m_ItemsDelivered )
				return;

			// ── Guard: validate all prerequisites before touching any items ──────
			if ( to == null )
			{
				Console.WriteLine( "[CharRestore] DeliverItems called with null target." );
				return;
			}

			if ( to.Deleted )
			{
				Console.WriteLine( $"[CharRestore] DeliverItems: target '{to.Name}' is deleted." );
				return;
			}

			if ( !to.Alive )
			{
				// Don't deliver to a dead player — they can't interact anyway.
				to.SendMessage( 0x20, StringCatalog.ResolveByKey( to.Account, "mob.you_must_be_alive_to_receive_this_dot" ) );
				return;
			}

			if ( to.Backpack == null )
			{
				to.SendMessage( 0x20, StringCatalog.ResolveByKey( to.Account, "mob.you_have_no_backpack_to_receive_these_items_dot" ) );
				Console.WriteLine( $"[CharRestore] DeliverItems: '{to.Name}' has no backpack." );
				return;
			}

			if ( m_RestorationBag == null || m_RestorationBag.Deleted )
			{
				to.SendMessage( 0x20, StringCatalog.ResolveByKey( to.Account, "mob.the_restoration_package_could_not_be_found_dot_please_contact_a_gm_dot" ) );
				Console.WriteLine( $"[CharRestore] DeliverItems: restoration bag is null or deleted for NPC 0x{Serial.Value:X8}." );
				return;
			}

			if ( m_RestorationBag.Items.Count == 0 )
			{
				to.SendMessage( 0x20, StringCatalog.ResolveByKey( to.Account, "mob.the_restoration_package_is_empty_dot_please_contact_a_gm_dot" ) );
				Console.WriteLine( $"[CharRestore] DeliverItems: restoration bag is empty for NPC 0x{Serial.Value:X8}." );
				return;
			}

			// ── Begin delivery: hand off the NPC's restoration bag (no second bag) ──
			Server.Gumps.CharRestoreLogger.LogDeliveryBegin( m_LogPath, this, to );

			TryStoreRestorationBag();

			string bagName = StringCatalog.TryResolveByKey(
				AccountLang.GetLanguageCode( to.Account ),
				GetThemeKey( "npc.bag_name" ) ) ?? "Salvaged Belongings";

			Bag deliveryBag = m_RestorationBag;

			int delivered = 0;
			List<Item> toLog = new List<Item>( deliveryBag.Items );

			foreach ( Item item in toLog )
			{
				if ( item == null || item.Deleted )
					continue;

				Server.Gumps.CharRestoreLogger.LogDeliveredItem( m_LogPath, item );
				delivered++;
			}

			if ( delivered == 0 )
			{
				to.SendMessage( 0x20, StringCatalog.ResolveByKey( to.Account, "mob.no_items_could_be_moved_dot_please_contact_a_gm_dot" ) );
				Server.Gumps.CharRestoreLogger.LogError( m_LogPath, "DeliverItems",
					new InvalidOperationException( "Restoration bag has no items — delivery aborted." ) );
				return;
			}

			m_RestorationBag = null;
			m_ItemsDelivered = true;

			deliveryBag.Name = bagName;
			deliveryBag.Hue  = 0x84C;

			if ( deliveryBag is CharRestoreBag securedBag )
				securedBag.ReleaseToPlayer();
			else
				deliveryBag.Movable = true;

			// ── Hand the restoration bag to the player (never duplicate into a new bag) ──
			if ( !to.PlaceInBackpack( deliveryBag ) )
			{
				Console.WriteLine( $"[CharRestore] DeliverItems: PlaceInBackpack failed for '{to.Name}'." );
				Server.Gumps.CharRestoreLogger.LogError( m_LogPath, "DeliverItems PlaceInBackpack",
					new InvalidOperationException( "Player backpack full or unavailable." ) );
				try { deliveryBag.MoveToWorld( to.Location, to.Map ); }
				catch { }
			}

			try { to.PlaySound( 0x249 ); } catch { }

		// ── Place personal note in player's backpack as a physical item ───────
		if ( !string.IsNullOrWhiteSpace( m_PersonalNote ) )
		{
			try
			{
				string noteName = StringCatalog.TryResolveByKey(
					AccountLang.GetLanguageCode( to.Account ),
					"charrestore.npc.note_name" );
				if ( string.IsNullOrEmpty( noteName ) )
					noteName = "Personal Note";

				Item noteItem    = new Item( 0x14F0 ); // blank scroll / parchment graphic
				noteItem.Name    = noteName;
				noteItem.InfoData = m_PersonalNote.Trim();
				to.AddToBackpack( noteItem );

				Server.Gumps.CharRestoreLogger.LogDeliveredItem( m_LogPath, noteItem );
			}
			catch ( Exception ex )
			{
				Console.WriteLine( $"[CharRestore] Personal note creation failed: {ex.Message}" );
				Server.Gumps.CharRestoreLogger.LogError( m_LogPath, "PersonalNote creation", ex );
			}
		}

		Server.Gumps.CharRestoreLogger.LogDeliveryEnd( m_LogPath, delivered, to );

		// NPC farewell
		try
		{
			SayTheme( "npc.farewell",
				"Safe harbors to you. Learn these waters before you venture out again." );
		}
		catch { }

			ScheduleDeparture();
		}

		// ------------------------------------------------------------------
		// Lifecycle
		// ------------------------------------------------------------------

		private void StartDeleteTimer()
		{
			if ( m_DeleteTimer != null )
				m_DeleteTimer.Stop();
			m_DeleteTimer = new DeleteTimer( this, TimeSpan.FromHours( 24 ) );
			m_DeleteTimer.Start();
		}

		public void ScheduleDeparture()
		{
			if ( m_DeleteTimer != null )
				m_DeleteTimer.Stop();
			m_DeleteTimer = new DeleteTimer( this, TimeSpan.FromSeconds( 8 ) );
			m_DeleteTimer.Start();
		}

		public override void OnDelete()
		{
			DestroyRestorationBag();
			base.OnDelete();
		}

		private void DestroyRestorationBag()
		{
			if ( m_RestorationBag == null || m_RestorationBag.Deleted )
			{
				m_RestorationBag = null;
				return;
			}

			try { m_RestorationBag.Delete(); }
			catch ( Exception ex )
			{
				Console.WriteLine( $"[CharRestore] DestroyRestorationBag failed: {ex.Message}" );
			}

			m_RestorationBag = null;
		}

		private class DeleteTimer : Timer
		{
			private LostItemsRestorerNPC m_NPC;
			public DeleteTimer( LostItemsRestorerNPC npc, TimeSpan delay )
				: base( delay ) { m_NPC = npc; Priority = TimerPriority.OneSecond; }

			protected override void OnTick()
			{
				if ( m_NPC != null && !m_NPC.Deleted )
				{
					Effects.SendLocationParticles(
						EffectItem.Create( m_NPC.Location, m_NPC.Map, EffectItem.DefaultDuration ),
						0x3728, 10, 10, 2023 );
					m_NPC.PlaySound( 0x1FE );
					m_NPC.Delete();
				}
			}
		}

		// ------------------------------------------------------------------
		// Serialization
		// ------------------------------------------------------------------

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 3 ); // version 3 adds m_Theme

			writer.Write( m_TargetName );
			writer.Write( m_PersonalNote );
			writer.Write( m_RestorationBag );
			writer.Write( m_CreatedTime );
			writer.Write( m_LogPath ?? "" );
			writer.Write( m_ItemsDelivered );
			writer.Write( (int)m_Theme );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			m_TargetName     = reader.ReadString();
			m_PersonalNote   = reader.ReadString();
			m_RestorationBag = reader.ReadItem() as Bag;
			m_CreatedTime    = reader.ReadDateTime();

			if ( version >= 1 )
				m_LogPath = reader.ReadString();

			if ( version >= 2 )
				m_ItemsDelivered = reader.ReadBool();

			if ( version >= 3 )
				m_Theme = CharRestoreThemes.Parse( reader.ReadInt() );
			else
				m_Theme = CharRestoreTheme.Ocean;

			ApplyTheme();

			Timer.DelayCall( TimeSpan.Zero, () =>
			{
				if ( !Deleted )
					TryStoreRestorationBag();
			} );

			// Clamp an obviously invalid timestamp to prevent negative or absurd timers.
			if ( m_CreatedTime > DateTime.Now || m_CreatedTime < DateTime.Now - TimeSpan.FromDays( 30 ) )
				m_CreatedTime = DateTime.Now - TimeSpan.FromHours( 23 );

			TimeSpan elapsed   = DateTime.Now - m_CreatedTime;
			TimeSpan remaining = TimeSpan.FromHours( 24 ) - elapsed;

			if ( remaining <= TimeSpan.Zero )
				Timer.DelayCall( TimeSpan.FromSeconds( 1 ), Delete );
			else
			{
				if ( m_DeleteTimer != null )
					m_DeleteTimer.Stop();
				m_DeleteTimer = new DeleteTimer( this, remaining );
				m_DeleteTimer.Start();
			}
		}

		// ------------------------------------------------------------------
		// Context menu entry
		// ------------------------------------------------------------------

		private class SalvagerTalkEntry : ContextMenuEntry
		{
			private Mobile m_From;
			private LostItemsRestorerNPC m_NPC;

			public SalvagerTalkEntry( Mobile from, LostItemsRestorerNPC npc )
				: base( 6146, 3 )
			{
				m_From = from;
				m_NPC  = npc;
			}

			public override void OnClick()
			{
				if ( m_From == null || !m_From.Alive )
					return;
				if ( !m_From.HasGump( typeof( LostItemsDialogGump ) ) )
					m_From.SendGump( new LostItemsDialogGump( m_From, m_NPC, 0 ) );
			}
		}
	}

	// ========================================================================
	// Dialog gump — three-stage conversation, all text from charrestore.json
	// ========================================================================

	/// <summary>
	/// Multi-stage dialog gump for <see cref="LostItemsRestorerNPC"/>.
	/// Stage 0: identity check.  Stage 1: backstory.  Stage 2: item handoff.
	/// All user-visible strings resolved via <c>charrestore.dialog.*</c> logical keys.
	/// </summary>
	public class LostItemsDialogGump : Gump
	{
		private Mobile m_From;
		private LostItemsRestorerNPC m_NPC;
		private int m_Stage;

		private static readonly string[] TitleSuffixes = new string[]
		{
			"dialog.title.greeting",
			"dialog.title.story",
			"dialog.title.handoff",
		};

		private static readonly string[][] TitleFallbacks = new string[][]
		{
			new string[] { "A Weathered Trail Guide", "A Weathered Salvager", "A Grim Delver" },
			new string[] { "Tales from the Wild", "A Salvager's Tale", "Echoes from Below" },
			new string[] { "Your Recovered Gear", "Your Belongings", "What the Depths Returned" },
		};

		private string ThemeFallback( int stage )
		{
			int themeIdx = (int)m_NPC.RestoreTheme;
			if ( themeIdx < 0 || themeIdx > 2 )
				themeIdx = 1;

			if ( stage >= 0 && stage < TitleFallbacks.Length )
				return TitleFallbacks[stage][themeIdx];

			return TitleFallbacks[0][themeIdx];
		}

		/// <summary>Theme-scoped key, e.g. <c>dialog.body.greeting</c> → <c>charrestore.theme.ocean.dialog.body.greeting</c>.</summary>
		private string KTheme( Mobile viewer, string suffix, string fallback )
		{
			if ( viewer == null || m_NPC == null )
				return fallback ?? suffix;

			string lang = AccountLang.GetLanguageCode( viewer.Account );
			string s    = StringCatalog.TryResolveByKey( lang, m_NPC.GetThemeKey( suffix ) );
			return ( s != null && s.Length > 0 ) ? s : ( fallback ?? suffix );
		}

		/// <summary>Shared <c>charrestore.*</c> key (not prefixed with theme id).</summary>
		private static string KShared( Mobile viewer, string key, string fallback )
		{
			if ( viewer == null )
				return fallback ?? key;

			string lang = AccountLang.GetLanguageCode( viewer.Account );
			string s    = StringCatalog.TryResolveByKey( lang, key );
			return ( s != null && s.Length > 0 ) ? s : ( fallback ?? key );
		}

		public LostItemsDialogGump( Mobile from, LostItemsRestorerNPC npc, int stage )
			: base( 50, 50 )
		{
			m_From  = from;
			m_NPC   = npc;
			m_Stage = stage;

			Closable   = true;
			Disposable = true;
			Dragable   = true;
			Resizable  = false;

			const string textColor = "#d5c8a2";

			AddPage( 0 );
			AddImage( 0, 0, 9543, PlayerSettings.GetGumpHue( from ) );

			string title = ( stage >= 0 && stage < TitleSuffixes.Length )
				? KTheme( from, TitleSuffixes[stage], ThemeFallback( stage ) )
				: KTheme( from, TitleSuffixes[0], ThemeFallback( 0 ) );

			AddHtml( 12, 15, 400, 20,
				"<BODY><BASEFONT Color=" + textColor + ">" + title + "</BASEFONT></BODY>",
				false, false );

			AddButton( 420, 12, 4017, 4017, 0, GumpButtonType.Reply, 0 );

			switch ( stage )
			{
				case 0: BuildStage0( from, textColor ); break;
				case 1: BuildStage1( from, textColor ); break;
				case 2: BuildStage2( from, npc, textColor ); break;
			}
		}

		private void BuildStage0( Mobile from, string c )
		{
			string body = KTheme( from, "dialog.body.greeting",
				"Hold there, traveler. Are you the one I was sent to find? " +
				"I have been carrying something that belongs to you. " +
				"Confirm who you are before I hand it over." );

			AddHtml( 12, 50, 420, 200,
				"<BODY><BASEFONT Color=" + c + ">" + body + "</BASEFONT></BODY>",
				false, true );

			AddButton( 12, 265, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtml( 50, 263, 180, 20,
				"<BODY><BASEFONT Color=" + c + ">" +
				KShared( from, "charrestore.dialog.btn.yes_me", "Yes, that is me." ) +
				"</BASEFONT></BODY>", false, false );

			AddButton( 230, 265, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddHtml( 268, 263, 180, 20,
				"<BODY><BASEFONT Color=" + c + ">" +
				KShared( from, "charrestore.dialog.btn.not_me", "No, you have the wrong person." ) +
				"</BASEFONT></BODY>", false, false );
		}

		private void BuildStage1( Mobile from, string c )
		{
			string body = KTheme( from, "dialog.body.story",
				"I thought as much. I gathered what I could before it was lost for good. " +
				"Not every soul would bother.<BR><BR>" +
				"I have it bundled here. Ready to receive it?" );

			AddHtml( 12, 50, 420, 200,
				"<BODY><BASEFONT Color=" + c + ">" + body + "</BASEFONT></BODY>",
				false, true );

			AddButton( 12, 265, 4005, 4007, 3, GumpButtonType.Reply, 0 );
			AddHtml( 50, 263, 380, 20,
				"<BODY><BASEFONT Color=" + c + ">" +
				KShared( from, "charrestore.dialog.btn.accept",
					"Yes, please. I am ready to receive my belongings." ) +
				"</BASEFONT></BODY>", false, false );
		}

		private void BuildStage2( Mobile from, LostItemsRestorerNPC npc, string c )
		{
			string body = KTheme( from, "dialog.body.handoff",
				"Here you are — your belongings. Take it all.<BR><BR>" +
				"Learn the road before you walk it. Fortune rarely forgives twice." );

			AddHtml( 12, 50, 420, 200,
				"<BODY><BASEFONT Color=" + c + ">" + body + "</BASEFONT></BODY>",
				false, true );

			AddButton( 12, 265, 4005, 4007, 4, GumpButtonType.Reply, 0 );
			AddHtml( 50, 263, 380, 20,
				"<BODY><BASEFONT Color=" + c + ">" +
				KTheme( from, "dialog.btn.thanks", "Thank you. I will remember this." ) +
				"</BASEFONT></BODY>", false, false );
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			Mobile from = sender.Mobile;
			if ( from == null || m_NPC == null || m_NPC.Deleted )
				return;

			from.PlaySound( 0x4A );

			switch ( info.ButtonID )
			{
				case 0: break; // close

				case 1: // confirmed identity
					m_NPC.SayTheme( "npc.confirmed",
						"Good. I knew it was you. Let me tell you how I came to have your things." );
					from.SendGump( new LostItemsDialogGump( from, m_NPC, 1 ) );
					break;

				case 2: // wrong person
					m_NPC.SayTheme( "npc.wrong_person",
						"My apologies for the confusion. I will keep looking." );
					break;

				case 3: // ready to receive
					from.SendGump( new LostItemsDialogGump( from, m_NPC, 2 ) );
					break;

				case 4: // accept items
					m_NPC.DeliverItems( from );
					break;
			}
		}
	}
}
