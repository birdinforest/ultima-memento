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
			Title          = "the sea salvager";
			m_CreatedTime  = DateTime.Now;

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

		// ------------------------------------------------------------------
		// Interaction
		// ------------------------------------------------------------------

		public override bool IsEnemy( Mobile m ) { return false; }
		public override bool IsInvulnerable { get { return true; } }
		public override bool OnBeforeDeath() { return false; }

		public override void OnDoubleClick( Mobile from )
		{
			if ( from == null || !from.Alive )
				return;

			if ( m_TargetName != null && !m_TargetName.Equals( from.Name, StringComparison.OrdinalIgnoreCase ) )
			{
				CitizenLocalization.SayLocalizedByKey( this,
					"charrestore.npc.deflect",
					"I am waiting for someone. Move along, traveler." );
				return;
			}

			if ( m_RestorationBag == null || m_RestorationBag.Deleted )
			{
				CitizenLocalization.SayLocalizedByKey( this,
					"charrestore.npc.lost_parcel",
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
				to.SendMessage( 0x20, "You must be alive to receive this." );
				return;
			}

			if ( to.Backpack == null )
			{
				to.SendMessage( 0x20, "You have no backpack to receive these items." );
				Console.WriteLine( $"[CharRestore] DeliverItems: '{to.Name}' has no backpack." );
				return;
			}

			if ( m_RestorationBag == null || m_RestorationBag.Deleted )
			{
				to.SendMessage( 0x20, "The restoration package could not be found. Please contact a GM." );
				Console.WriteLine( $"[CharRestore] DeliverItems: restoration bag is null or deleted for NPC 0x{Serial.Value:X8}." );
				return;
			}

			if ( m_RestorationBag.Items.Count == 0 )
			{
				to.SendMessage( 0x20, "The restoration package is empty. Please contact a GM." );
				Console.WriteLine( $"[CharRestore] DeliverItems: restoration bag is empty for NPC 0x{Serial.Value:X8}." );
				return;
			}

			// ── Begin delivery ────────────────────────────────────────────────────
			Server.Gumps.CharRestoreLogger.LogDeliveryBegin( m_LogPath, this, to );

			string bagName = StringCatalog.TryResolveByKey(
				AccountLang.GetLanguageCode( to.Account ),
				"charrestore.npc.bag_name" ) ?? "Salvaged Belongings";

			Bag deliveryBag;
			try
			{
				deliveryBag = new Bag();
				deliveryBag.Name = bagName;
				deliveryBag.Hue  = 0x84C;
			}
			catch ( Exception ex )
			{
				Console.WriteLine( $"[CharRestore] DeliverItems: delivery bag creation failed: {ex.Message}" );
				Server.Gumps.CharRestoreLogger.LogError( m_LogPath, "DeliveryBag creation", ex );
				return;
			}

			// ── Move items: copy list first to avoid modification-during-enumeration ──
			int delivered = 0;
			List<Item> toMove = new List<Item>( m_RestorationBag.Items );

			foreach ( Item item in toMove )
			{
				if ( item == null || item.Deleted )
				{
					Server.Gumps.CharRestoreLogger.LogError( m_LogPath, "DeliverItems",
						new InvalidOperationException( "Null or deleted item in restoration bag — skipped." ) );
					continue;
				}

				try
				{
					Server.Gumps.CharRestoreLogger.LogDeliveredItem( m_LogPath, item );
					deliveryBag.DropItem( item );
					delivered++;
				}
				catch ( Exception ex )
				{
					// Item move failed: log and leave item in original bag rather than
					// creating an orphan or crashing.
					Console.WriteLine( $"[CharRestore] DeliverItems: DropItem failed for {item.GetType().Name}: {ex.Message}" );
					Server.Gumps.CharRestoreLogger.LogError( m_LogPath, "DeliverItems DropItem", ex );
				}
			}

			if ( delivered == 0 )
			{
				try { deliveryBag.Delete(); } catch { }
				to.SendMessage( 0x20, "No items could be moved. Please contact a GM." );
				Server.Gumps.CharRestoreLogger.LogError( m_LogPath, "DeliverItems",
					new InvalidOperationException( "Zero items moved — delivery aborted." ) );
				return;
			}

			// ── Hand delivery bag to player ───────────────────────────────────────
			try
			{
				to.AddToBackpack( deliveryBag );
			}
			catch ( Exception ex )
			{
				Console.WriteLine( $"[CharRestore] DeliverItems: AddToBackpack failed: {ex.Message}" );
				Server.Gumps.CharRestoreLogger.LogError( m_LogPath, "AddToBackpack", ex );
				// Attempt direct world-drop as last resort
				try { deliveryBag.MoveToWorld( to.Location, to.Map ); }
				catch { }
			}

			try { to.PlaySound( 0x249 ); } catch { }

			Server.Gumps.CharRestoreLogger.LogDeliveryEnd( m_LogPath, delivered, to );

			// NPC farewell
			try
			{
				CitizenLocalization.SayLocalizedByKey( this,
					"charrestore.npc.farewell",
					"Safe harbors to you. Learn these waters before you venture out again." );
			}
			catch { }

			if ( !string.IsNullOrEmpty( m_PersonalNote ) )
			{
				try { to.SendMessage( 0x59, m_PersonalNote ); } catch { }
			}

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
			writer.Write( (int) 1 ); // version (1 adds m_LogPath)

			writer.Write( m_TargetName );
			writer.Write( m_PersonalNote );
			writer.Write( m_RestorationBag );
			writer.Write( m_CreatedTime );
			writer.Write( m_LogPath ?? "" );
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

		private static readonly string[] TitleKeys = new string[]
		{
			"charrestore.dialog.title.greeting",
			"charrestore.dialog.title.story",
			"charrestore.dialog.title.handoff",
		};

		private static readonly string[] TitleFallbacks = new string[]
		{
			"A Weathered Salvager",
			"A Salvager's Tale",
			"Your Belongings",
		};

		/// <summary>
		/// Resolves a <c>charrestore.*</c> key for the given mobile's language.
		/// Falls back to <paramref name="fallback"/> when missing.
		/// </summary>
		private static string K( Mobile viewer, string key, string fallback )
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

			string title = ( stage >= 0 && stage < TitleKeys.Length )
				? K( from, TitleKeys[stage], TitleFallbacks[stage] )
				: K( from, TitleKeys[0], TitleFallbacks[0] );

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
			string body = K( from,
				"charrestore.dialog.body.greeting",
				"Hold there, traveler. Are you the one who was stranded by the sea's whim? " +
				"I have been carrying something that belongs to you — or so I have been told. " +
				"It would help me to know I have found the right person before I hand it over." );

			AddHtml( 12, 50, 420, 200,
				"<BODY><BASEFONT Color=" + c + ">" + body + "</BASEFONT></BODY>",
				false, true );

			AddButton( 12, 265, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtml( 50, 263, 180, 20,
				"<BODY><BASEFONT Color=" + c + ">" +
				K( from, "charrestore.dialog.btn.yes_me", "Yes, that is me." ) +
				"</BASEFONT></BODY>", false, false );

			AddButton( 230, 265, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddHtml( 268, 263, 180, 20,
				"<BODY><BASEFONT Color=" + c + ">" +
				K( from, "charrestore.dialog.btn.not_me", "No, you have the wrong person." ) +
				"</BASEFONT></BODY>", false, false );
		}

		private void BuildStage1( Mobile from, string c )
		{
			string body = K( from,
				"charrestore.dialog.body.story",
				"I thought as much. The sea gives and the sea takes — but sometimes a keen eye " +
				"and quick hands can recover what the tides would claim. " +
				"I pulled what I could from the wreckage before the current swept it away. " +
				"Not every soul would bother, but I have seen too many good people lose everything " +
				"to the waters. It is not right.<BR><BR>" +
				"I have it all bundled here. Ready to receive it?" );

			AddHtml( 12, 50, 420, 200,
				"<BODY><BASEFONT Color=" + c + ">" + body + "</BASEFONT></BODY>",
				false, true );

			AddButton( 12, 265, 4005, 4007, 3, GumpButtonType.Reply, 0 );
			AddHtml( 50, 263, 380, 20,
				"<BODY><BASEFONT Color=" + c + ">" +
				K( from, "charrestore.dialog.btn.accept",
					"Yes, please. I am ready to receive my belongings." ) +
				"</BASEFONT></BODY>", false, false );
		}

		private void BuildStage2( Mobile from, LostItemsRestorerNPC npc, string c )
		{
			string body = K( from,
				"charrestore.dialog.body.handoff",
				"Here you are — your salvaged belongings. Take it all. " +
				"It belongs to you, and the sea owes you at least this much.<BR><BR>" +
				"A piece of advice from an old salvager: learn the coastlines before you sail them. " +
				"The sea does not forgive twice." );

			AddHtml( 12, 50, 420, 200,
				"<BODY><BASEFONT Color=" + c + ">" + body + "</BASEFONT></BODY>",
				false, true );

			AddButton( 12, 265, 4005, 4007, 4, GumpButtonType.Reply, 0 );
			AddHtml( 50, 263, 380, 20,
				"<BODY><BASEFONT Color=" + c + ">" +
				K( from, "charrestore.dialog.btn.thanks",
					"Thank you, salvager. I will remember this." ) +
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
					CitizenLocalization.SayLocalizedByKey( m_NPC,
						"charrestore.npc.confirmed",
						"Good. I knew it was you. Let me tell you how I came to have your things." );
					from.SendGump( new LostItemsDialogGump( from, m_NPC, 1 ) );
					break;

				case 2: // wrong person
					CitizenLocalization.SayLocalizedByKey( m_NPC,
						"charrestore.npc.wrong_person",
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
