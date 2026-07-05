using Server.Items;

namespace Server.Engines.Avatar
{
	public partial class PlayerContext
	{
		private Serial _researchBagSerial;
		private Serial _ancientSpellbookSerial;

		public string SnapshotRuneFound;
		public string SnapshotSpellsMagery;
		public string SnapshotSpellsNecromancy;
		public string SnapshotResearchSpells;

		public string SnapshotRuneLocation;
		public string SnapshotSpellsMageLocation;
		public string SnapshotSpellsNecroLocation;
		public string SnapshotBagInkLocation;
		public string SnapshotResearchLocation;

		public string CurrentResonanceLocation;
		public string ResonanceLocationType;
		public Serial MemoryEchoSearchBaseSerial;

		public int RebirthReportInkBefore;
		public int RebirthReportInkAfter;
		public int RebirthReportScrollsBefore;
		public int RebirthReportScrollsAfter;
		public int RebirthReportQuillsBefore;
		public int RebirthReportQuillsAfter;
		public int RebirthReportPrepBefore;
		public int RebirthReportPrepAfter;

		public bool HasPendingRebirthReport;
		public bool SuppressRebirthReport;

		public int AvatarDeathCount;

		/// <summary>Runtime guard while <see cref="PlayerMobile.BeginAvatarRebirth"/> is in flight.</summary>
		public bool RebirthInProgress;

		public Serial SnapshotAncientSpellbookOwnerSerial;
		public string SnapshotAncientSpellbookNames;
		public int SnapshotAncientSpellbookPaper;
		public int SnapshotAncientSpellbookQuill;
		public ulong SnapshotAncientSpellbookContent;
		public int SnapshotAncientSpellbookSlayer;
		public int SnapshotAncientSpellbookSlayer2;

		public bool HasResearchBag
		{ get { return _researchBagSerial != Serial.Zero && World.Items.ContainsKey( _researchBagSerial ); } }

		public bool HasAncientSpellbook
		{ get { return _ancientSpellbookSerial != Serial.Zero && World.Items.ContainsKey( _ancientSpellbookSerial ); } }

		public ResearchBag GetResearchBag()
		{
			if ( !HasResearchBag )
				return null;

			return World.Items[_researchBagSerial] as ResearchBag;
		}

		public AncientSpellbook GetAncientSpellbook()
		{
			if ( !HasAncientSpellbook )
				return null;

			return World.Items[_ancientSpellbookSerial] as AncientSpellbook;
		}

		public void SetResearchBagSerial( ResearchBag bag )
		{
			_researchBagSerial = bag != null ? bag.Serial : Serial.Zero;
		}

		public void SetAncientSpellbookSerial( AncientSpellbook book )
		{
			_ancientSpellbookSerial = book != null ? book.Serial : Serial.Zero;
		}

		internal void DeserializeResearchFields( GenericReader reader, int version )
		{
			if ( version < 11 )
				return;

			_researchBagSerial = reader.ReadInt();
			_ancientSpellbookSerial = reader.ReadInt();

			SnapshotRuneFound = reader.ReadString();
			SnapshotSpellsMagery = reader.ReadString();
			SnapshotSpellsNecromancy = reader.ReadString();
			SnapshotResearchSpells = reader.ReadString();

			SnapshotRuneLocation = reader.ReadString();
			SnapshotSpellsMageLocation = reader.ReadString();
			SnapshotSpellsNecroLocation = reader.ReadString();
			SnapshotBagInkLocation = reader.ReadString();
			SnapshotResearchLocation = reader.ReadString();

			CurrentResonanceLocation = reader.ReadString();
			ResonanceLocationType = reader.ReadString();

			if ( version >= 12 )
				MemoryEchoSearchBaseSerial = reader.ReadInt();
			else
				MemoryEchoSearchBaseSerial = Serial.Zero;

			RebirthReportInkBefore = reader.ReadInt();
			RebirthReportInkAfter = reader.ReadInt();
			RebirthReportScrollsBefore = reader.ReadInt();
			RebirthReportScrollsAfter = reader.ReadInt();
			RebirthReportQuillsBefore = reader.ReadInt();
			RebirthReportQuillsAfter = reader.ReadInt();
			RebirthReportPrepBefore = reader.ReadInt();
			RebirthReportPrepAfter = reader.ReadInt();

			HasPendingRebirthReport = reader.ReadBool();
			SuppressRebirthReport = reader.ReadBool();

			AvatarDeathCount = reader.ReadInt();

			SnapshotAncientSpellbookOwnerSerial = reader.ReadInt();
			SnapshotAncientSpellbookNames = reader.ReadString();
			SnapshotAncientSpellbookPaper = reader.ReadInt();
			SnapshotAncientSpellbookQuill = reader.ReadInt();
			SnapshotAncientSpellbookContent = reader.ReadULong();
			SnapshotAncientSpellbookSlayer = reader.ReadInt();
			SnapshotAncientSpellbookSlayer2 = reader.ReadInt();
		}

		internal void SerializeResearchFields( GenericWriter writer )
		{
			writer.Write( _researchBagSerial );
			writer.Write( _ancientSpellbookSerial );

			writer.Write( SnapshotRuneFound );
			writer.Write( SnapshotSpellsMagery );
			writer.Write( SnapshotSpellsNecromancy );
			writer.Write( SnapshotResearchSpells );

			writer.Write( SnapshotRuneLocation );
			writer.Write( SnapshotSpellsMageLocation );
			writer.Write( SnapshotSpellsNecroLocation );
			writer.Write( SnapshotBagInkLocation );
			writer.Write( SnapshotResearchLocation );

			writer.Write( CurrentResonanceLocation );
			writer.Write( ResonanceLocationType );
			writer.Write( MemoryEchoSearchBaseSerial );

			writer.Write( RebirthReportInkBefore );
			writer.Write( RebirthReportInkAfter );
			writer.Write( RebirthReportScrollsBefore );
			writer.Write( RebirthReportScrollsAfter );
			writer.Write( RebirthReportQuillsBefore );
			writer.Write( RebirthReportQuillsAfter );
			writer.Write( RebirthReportPrepBefore );
			writer.Write( RebirthReportPrepAfter );

			writer.Write( HasPendingRebirthReport );
			writer.Write( SuppressRebirthReport );

			writer.Write( AvatarDeathCount );

			writer.Write( SnapshotAncientSpellbookOwnerSerial );
			writer.Write( SnapshotAncientSpellbookNames );
			writer.Write( SnapshotAncientSpellbookPaper );
			writer.Write( SnapshotAncientSpellbookQuill );
			writer.Write( SnapshotAncientSpellbookContent );
			writer.Write( SnapshotAncientSpellbookSlayer );
			writer.Write( SnapshotAncientSpellbookSlayer2 );
		}
	}
}
