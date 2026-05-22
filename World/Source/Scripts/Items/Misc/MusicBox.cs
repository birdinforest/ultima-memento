using System;
using Server;
using Server.Network;
using System.Text;
using Server.Items;
using Server.Mobiles;
using Server.Localization;


namespace Server.Items
{
	public class MusicBox : Item
	{
		public int Mplay;

		[CommandProperty(AccessLevel.Owner)]
		public int M_play { get { return Mplay; } set { Mplay = value; InvalidateProperties(); } }

		[Constructable]
		public MusicBox() : base( 0x420C )
		{
Name = StringCatalog.Resolve(null, "Lute of Many Songs");
			Weight = 5;
		}

		public MusicBox( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile m )
		{
			if ( Mplay == 1){ m.Send(PlayMusic.GetInstance( MusicName.Ultima )); m.SendMessage( StringCatalog.Resolve(m.Account, "Ultima") ); Mplay = Mplay + 1; }
			else if ( Mplay == 2){ m.Send(PlayMusic.GetInstance( MusicName.Mines )); m.SendMessage( StringCatalog.Resolve(m.Account, "Mines") ); Mplay = Mplay + 1; }
			else if ( Mplay == 3){ m.Send(PlayMusic.GetInstance( MusicName.Dragon )); m.SendMessage( StringCatalog.Resolve(m.Account, "Dragon") ); Mplay = Mplay + 1; }
			else if ( Mplay == 4){ m.Send(PlayMusic.GetInstance( MusicName.Scouting )); m.SendMessage( StringCatalog.Resolve(m.Account, "Scouting") ); Mplay = Mplay + 1; }
			else if ( Mplay == 5){ m.Send(PlayMusic.GetInstance( MusicName.Wrong )); m.SendMessage( StringCatalog.Resolve(m.Account, "Wrong") ); Mplay = Mplay + 1; }
			else if ( Mplay == 6){ m.Send(PlayMusic.GetInstance( MusicName.Hunting )); m.SendMessage( StringCatalog.Resolve(m.Account, "Hunting") ); Mplay = Mplay + 1; }
			else if ( Mplay == 7){ m.Send(PlayMusic.GetInstance( MusicName.Covetous )); m.SendMessage( StringCatalog.Resolve(m.Account, "Covetous") ); Mplay = Mplay + 1; }
			else if ( Mplay == 8){ m.Send(PlayMusic.GetInstance( MusicName.Deceit )); m.SendMessage( StringCatalog.Resolve(m.Account, "Deceit") ); Mplay = Mplay + 1; }
			else if ( Mplay == 9){ m.Send(PlayMusic.GetInstance( MusicName.Odyssey )); m.SendMessage( StringCatalog.Resolve(m.Account, "Odyssey") ); Mplay = Mplay + 1; }
			else if ( Mplay == 10){ m.Send(PlayMusic.GetInstance( MusicName.Britain )); m.SendMessage( StringCatalog.Resolve(m.Account, "Britain") ); Mplay = Mplay + 1; }
			else if ( Mplay == 11){ m.Send(PlayMusic.GetInstance( MusicName.CastleBritain )); m.SendMessage( StringCatalog.Resolve(m.Account, "Castle British") ); Mplay = Mplay + 1; }
			else if ( Mplay == 12){ m.Send(PlayMusic.GetInstance( MusicName.BucsDen )); m.SendMessage( StringCatalog.Resolve(m.Account, "Bucs Den") ); Mplay = Mplay + 1; }
			else if ( Mplay == 13){ m.Send(PlayMusic.GetInstance( MusicName.DevilGuard )); m.SendMessage( StringCatalog.Resolve(m.Account, "Devil Guard") ); Mplay = Mplay + 1; }
			else if ( Mplay == 14){ m.Send(PlayMusic.GetInstance( MusicName.CastleKnowledge )); m.SendMessage( StringCatalog.Resolve(m.Account, "Castle Knowledge") ); Mplay = Mplay + 1; }
			else if ( Mplay == 15){ m.Send(PlayMusic.GetInstance( MusicName.Adventure )); m.SendMessage( StringCatalog.Resolve(m.Account, "Adventure") ); Mplay = Mplay + 1; }
			else if ( Mplay == 16){ m.Send(PlayMusic.GetInstance( MusicName.Renika )); m.SendMessage( StringCatalog.Resolve(m.Account, "Renika") ); Mplay = Mplay + 1; }
			else if ( Mplay == 17){ m.Send(PlayMusic.GetInstance( MusicName.Montor )); m.SendMessage( StringCatalog.Resolve(m.Account, "Montor") ); Mplay = Mplay + 1; }
			else if ( Mplay == 18){ m.Send(PlayMusic.GetInstance( MusicName.Grey )); m.SendMessage( StringCatalog.Resolve(m.Account, "Grey") ); Mplay = Mplay + 1; }
			else if ( Mplay == 19){ m.Send(PlayMusic.GetInstance( MusicName.Destard )); m.SendMessage( StringCatalog.Resolve(m.Account, "Destard") ); Mplay = Mplay + 1; }
			else if ( Mplay == 20){ m.Send(PlayMusic.GetInstance( MusicName.FiresHell )); m.SendMessage( StringCatalog.Resolve(m.Account, "Fires of Hell") ); Mplay = Mplay + 1; }
			else if ( Mplay == 21){ m.Send(PlayMusic.GetInstance( MusicName.SkaraBrae )); m.SendMessage( StringCatalog.Resolve(m.Account, "Skara Brae") ); Mplay = Mplay + 1; }
			else if ( Mplay == 22){ m.Send(PlayMusic.GetInstance( MusicName.Moon )); m.SendMessage( StringCatalog.Resolve(m.Account, "Moon") ); Mplay = Mplay + 1; }
			else if ( Mplay == 23){ m.Send(PlayMusic.GetInstance( MusicName.Luna )); m.SendMessage( StringCatalog.Resolve(m.Account, "Luna") ); Mplay = Mplay + 1; }
			else if ( Mplay == 24){ m.Send(PlayMusic.GetInstance( MusicName.TimeAwaits )); m.SendMessage( StringCatalog.Resolve(m.Account, "Time Awaits") ); Mplay = Mplay + 1; }
			else if ( Mplay == 25){ m.Send(PlayMusic.GetInstance( MusicName.Yew )); m.SendMessage( StringCatalog.Resolve(m.Account, "Yew") ); Mplay = Mplay + 1; }
			else if ( Mplay == 26){ m.Send(PlayMusic.GetInstance( MusicName.Doom )); m.SendMessage( StringCatalog.Resolve(m.Account, "Doom") ); Mplay = Mplay + 1; }
			else if ( Mplay == 27){ m.Send(PlayMusic.GetInstance( MusicName.Exodus )); m.SendMessage( StringCatalog.Resolve(m.Account, "Exodus") ); Mplay = Mplay + 1; }
			else if ( Mplay == 28){ m.Send(PlayMusic.GetInstance( MusicName.Traveling )); m.SendMessage( StringCatalog.Resolve(m.Account, "Traveling") ); Mplay = Mplay + 1; }
			else if ( Mplay == 29){ m.Send(PlayMusic.GetInstance( MusicName.Docks )); m.SendMessage( StringCatalog.Resolve(m.Account, "Docks") ); Mplay = Mplay + 1; }
			else if ( Mplay == 30){ m.Send(PlayMusic.GetInstance( MusicName.Explore )); m.SendMessage( StringCatalog.Resolve(m.Account, "Explore") ); Mplay = Mplay + 1; }
			else if ( Mplay == 31){ m.Send(PlayMusic.GetInstance( MusicName.Searching )); m.SendMessage( StringCatalog.Resolve(m.Account, "Searching") ); Mplay = Mplay + 1; }
			else if ( Mplay == 32){ m.Send(PlayMusic.GetInstance( MusicName.Wandering )); m.SendMessage( StringCatalog.Resolve(m.Account, "Wandering") ); Mplay = Mplay + 1; }
			else if ( Mplay == 33){ m.Send(PlayMusic.GetInstance( MusicName.Sailing )); m.SendMessage( StringCatalog.Resolve(m.Account, "Sailing") ); Mplay = Mplay + 1; }
			else if ( Mplay == 34){ m.Send(PlayMusic.GetInstance( MusicName.Expedition )); m.SendMessage( StringCatalog.Resolve(m.Account, "Expedition") ); Mplay = Mplay + 1; }
			else if ( Mplay == 35){ m.Send(PlayMusic.GetInstance( MusicName.Tavern )); m.SendMessage( StringCatalog.Resolve(m.Account, "Tavern") ); Mplay = Mplay + 1; }
			else if ( Mplay == 36){ m.Send(PlayMusic.GetInstance( MusicName.Bar )); m.SendMessage( StringCatalog.Resolve(m.Account, "Bar") ); Mplay = Mplay + 1; }
			else if ( Mplay == 37){ m.Send(PlayMusic.GetInstance( MusicName.Alehouse )); m.SendMessage( StringCatalog.Resolve(m.Account, "Alehouse") ); Mplay = Mplay + 1; }
			else if ( Mplay == 38){ m.Send(PlayMusic.GetInstance( MusicName.Inn )); m.SendMessage( StringCatalog.Resolve(m.Account, "Inn") ); Mplay = Mplay + 1; }
			else if ( Mplay == 39){ m.Send(PlayMusic.GetInstance( MusicName.Combat1 )); m.SendMessage( StringCatalog.Resolve(m.Account, "Combat 1") ); Mplay = Mplay + 1; }
			else if ( Mplay == 40){ m.Send(PlayMusic.GetInstance( MusicName.Combat2 )); m.SendMessage( StringCatalog.Resolve(m.Account, "Combat 2") ); Mplay = Mplay + 1; }
			else if ( Mplay == 41){ m.Send(PlayMusic.GetInstance( MusicName.Combat3 )); m.SendMessage( StringCatalog.Resolve(m.Account, "Combat 3") ); Mplay = Mplay + 1; }
			else if ( Mplay == 42){ m.Send(PlayMusic.GetInstance( MusicName.Catacombs )); m.SendMessage( StringCatalog.Resolve(m.Account, "Catacombs") ); Mplay = Mplay + 1; }
			else if ( Mplay == 43){ m.Send(PlayMusic.GetInstance( MusicName.Death )); m.SendMessage( StringCatalog.Resolve(m.Account, "Death") ); Mplay = Mplay + 1; }
			else if ( Mplay == 44){ m.Send(PlayMusic.GetInstance( MusicName.Roaming )); m.SendMessage( StringCatalog.Resolve(m.Account, "Roaming") ); Mplay = Mplay + 1; }
			else if ( Mplay == 45){ m.Send(PlayMusic.GetInstance( MusicName.WizardDen )); m.SendMessage( StringCatalog.Resolve(m.Account, "Wizard Den") ); Mplay = Mplay + 1; }
			else if ( Mplay == 46){ m.Send(PlayMusic.GetInstance( MusicName.Fawn )); m.SendMessage( StringCatalog.Resolve(m.Account, "Fawn") ); Mplay = Mplay + 1; }
			else if ( Mplay == 47){ m.Send(PlayMusic.GetInstance( MusicName.Clues )); m.SendMessage( StringCatalog.Resolve(m.Account, "Clues") ); Mplay = Mplay + 1; }
			else if ( Mplay == 48){ m.Send(PlayMusic.GetInstance( MusicName.DeathGulch )); m.SendMessage( StringCatalog.Resolve(m.Account, "Death Gulch") ); Mplay = Mplay + 1; }
			else if ( Mplay == 49){ m.Send(PlayMusic.GetInstance( MusicName.Elidor )); m.SendMessage( StringCatalog.Resolve(m.Account, "Elidor") ); Mplay = Mplay + 1; }
			else if ( Mplay == 50){ m.Send(PlayMusic.GetInstance( MusicName.Guild )); m.SendMessage( StringCatalog.Resolve(m.Account, "Guild") ); Mplay = Mplay + 1; }
			else if ( Mplay == 51){ m.Send(PlayMusic.GetInstance( MusicName.MinesMorinia )); m.SendMessage( StringCatalog.Resolve(m.Account, "Mines of Morinia") ); Mplay = Mplay + 1; }
			else if ( Mplay == 52){ m.Send(PlayMusic.GetInstance( MusicName.Taiko )); m.SendMessage( StringCatalog.Resolve(m.Account, "Taiko") ); Mplay = Mplay + 1; }
			else if ( Mplay == 53){ m.Send(PlayMusic.GetInstance( MusicName.DardinsPit )); m.SendMessage( StringCatalog.Resolve(m.Account, "Dardin's Pit") ); Mplay = Mplay + 1; }
			else if ( Mplay == 54){ m.Send(PlayMusic.GetInstance( MusicName.City )); m.SendMessage( StringCatalog.Resolve(m.Account, "City") ); Mplay = Mplay + 1; }
			else if ( Mplay == 55){ m.Send(PlayMusic.GetInstance( MusicName.PerinianDepths )); m.SendMessage( StringCatalog.Resolve(m.Account, "Perinian Depths") ); Mplay = Mplay + 1; }
			else if ( Mplay == 56){ m.Send(PlayMusic.GetInstance( MusicName.Hythloth )); m.SendMessage( StringCatalog.Resolve(m.Account, "Hythloth") ); Mplay = Mplay + 1; }
			else if ( Mplay == 57){ m.Send(PlayMusic.GetInstance( MusicName.Seeking )); m.SendMessage( StringCatalog.Resolve(m.Account, "Seeking") ); Mplay = Mplay + 1; }
			else if ( Mplay == 58){ m.Send(PlayMusic.GetInstance( MusicName.TimeLord )); m.SendMessage( StringCatalog.Resolve(m.Account, "Time Lord") ); Mplay = Mplay + 1; }
			else if ( Mplay == 59){ m.Send(PlayMusic.GetInstance( MusicName.Cave )); m.SendMessage( StringCatalog.Resolve(m.Account, "Cave") ); Mplay = Mplay + 1; }
			else if ( Mplay == 60){ m.Send(PlayMusic.GetInstance( MusicName.Quest )); m.SendMessage( StringCatalog.Resolve(m.Account, "Quest") ); Mplay = Mplay + 1; }
			else if ( Mplay == 61){ m.Send(PlayMusic.GetInstance( MusicName.Grotto )); m.SendMessage( StringCatalog.Resolve(m.Account, "Grotto") ); Mplay = Mplay + 1; }
			else if ( Mplay == 62){ m.Send(PlayMusic.GetInstance( MusicName.Shame )); m.SendMessage( StringCatalog.Resolve(m.Account, "Shame") ); Mplay = Mplay + 1; }
			else if ( Mplay == 63){ m.Send(PlayMusic.GetInstance( MusicName.DarkGuild )); m.SendMessage( StringCatalog.Resolve(m.Account, "Dark Guild") ); Mplay = Mplay + 1; }
			else if ( Mplay == 64){ m.Send(PlayMusic.GetInstance( MusicName.Despise )); m.SendMessage( StringCatalog.Resolve(m.Account, "Despise") ); Mplay = Mplay + 1; }
			else if ( Mplay == 65){ m.Send(PlayMusic.GetInstance( MusicName.Pub )); m.SendMessage( StringCatalog.Resolve(m.Account, "Pub") ); Mplay = Mplay + 1; }
			else if ( Mplay == 66){ m.Send(PlayMusic.GetInstance( MusicName.Combat4 )); m.SendMessage( StringCatalog.Resolve(m.Account, "Combat 4") ); Mplay = Mplay + 1; }
			else { m.Send(PlayMusic.GetInstance( MusicName.Pirates )); m.SendMessage(StringCatalog.Resolve(m.Account, "Pirates")); Mplay = 1; }
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
            writer.Write( Mplay );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            Mplay = reader.ReadInt();
		}
	}
}