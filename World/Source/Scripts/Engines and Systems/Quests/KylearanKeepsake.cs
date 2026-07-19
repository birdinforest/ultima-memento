using System;
using System.Text;
using Server.Gumps;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	public class KylearanKeepsake : Item
	{
		private int m_ProgressStage;

		public override string DisplayNameLocalizationKey => "item.special.kylearan_letter";
		public override bool IsContentLocalized => true;
		public override int QuestItemHue => 0x455;

		[CommandProperty( AccessLevel.GameMaster )]
		public int ProgressStage
		{
			get { return m_ProgressStage; }
			set { m_ProgressStage = Math.Max( 0, Math.Min( 5, value ) ); InvalidateProperties(); }
		}

		[Constructable]
		public KylearanKeepsake() : this( 0 )
		{
		}

		public KylearanKeepsake( int stage ) : base( 0x14EE )
		{
			Weight = 1.0;
			Hue = 0x455;
			LootType = LootType.Blessed;
			QuestItem = true;
			m_ProgressStage = Math.Max( 0, Math.Min( 5, stage ) );
		}

		public static void GrantOrRefresh( Mobile from )
		{
			if ( from == null || from.Backpack == null )
				return;

			KylearanKeepsake keepsake = from.Backpack.FindItemByType( typeof( KylearanKeepsake ) ) as KylearanKeepsake;

			int stage = PlayerSettings.GetBardsTaleProgressStage( from );

			if ( keepsake != null )
			{
				if ( stage > keepsake.ProgressStage )
					keepsake.ProgressStage = stage;
			}
			else
			{
				from.Backpack.DropItem( new KylearanKeepsake( stage ) );
			}
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( BuildingPropertyListLocale != null )
				AddLocalizedProperty( list, "quest.bards_tale.letter.read_hint" );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "quest.bards_tale.letter.must_be_in_pack" ) );
				return;
			}

			from.CloseGump( typeof( KylearanLetterGump ) );
			from.SendGump( new KylearanLetterGump( from, this ) );
			from.PlaySound( 0x249 );
		}

		private class KylearanLetterGump : Gump
		{
			public KylearanLetterGump( Mobile from, KylearanKeepsake letter ) : base( 100, 100 )
			{
				string body = BuildLetterBody( from, letter.ProgressStage );

				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;

				AddPage( 0 );
				AddImage( 0, 0, 10901, 2786 );
				AddImage( 0, 0, 10899, 2117 );
				AddHtml( 45, 78, 386, 218, TradesBookLocalization.BodyRaw( "#d9c781", body ), false, true );
			}

			private static string BuildLetterBody( Mobile from, int stage )
			{
				var account = from.Account;
				var sb = new StringBuilder();
				var pm = from as PlayerMobile;

				sb.AppendLine( StringCatalog.ResolveByKey( account, "quest.bards_tale.letter.body.intro" ) );
				sb.AppendLine();
				sb.AppendLine( StringCatalog.ResolveByKey( account, "quest.bards_tale.keepsake.stage." + stage.ToString() ) );
				sb.AppendLine();
				sb.AppendLine( StringCatalog.ResolveByKey( account, "quest.bards_tale.letter.body.buff" ) );
				sb.AppendLine();

				if ( pm != null && pm.SkaraBraeKylearanTitleAwarded )
				{
					sb.AppendLine( StringCatalog.ResolveByKey( account, "quest.bards_tale.letter.body.contract_fulfilled" ) );
				}
				else if ( pm != null && PlayerSettings.IsSkaraBraeKylearanContractActive( pm ) )
				{
					int daysLeft = PlayerSettings.GetSkaraBraeKylearanContractDaysRemaining( pm );
					sb.AppendLine( StringCatalog.ResolveFormatByKey( account, "quest.bards_tale.letter.body.contract_active", daysLeft.ToString() ) );
				}
				else
				{
					sb.AppendLine( StringCatalog.ResolveByKey( account, "quest.bards_tale.letter.body.title_promise" ) );
				}

				return sb.ToString();
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				Mobile from = state.Mobile;

				if ( from != null )
					from.PlaySound( 0x249 );
			}
		}

		public KylearanKeepsake( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)0 );
			writer.Write( m_ProgressStage );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_ProgressStage = reader.ReadInt();
		}
	}
}
