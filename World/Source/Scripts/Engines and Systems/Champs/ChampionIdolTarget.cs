using Server.Engines.CannedEvil;
using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Gumps
{
	public class ChampionIdolTarget : Target
	{
		private readonly ChampionSkull _championSkull;
		private readonly Difficulty _monsterDifficulty;
		private readonly int _spawnSize;

		public ChampionIdolTarget(ChampionSkull championSkull, int spawnSize, Difficulty monsterDifficulty) : base(4, false, TargetFlags.None)
		{
			_championSkull = championSkull;
			_spawnSize = spawnSize;
			_monsterDifficulty = monsterDifficulty;
		}

		public static bool Activate(Mobile from, IdolOfTheChampion idol, ChampionSpawnType spawnType, int spawnSize, Difficulty monsterDifficulty)
		{
			if (idol == null || idol.Spawn == null)
			{
				from.SendMessage("This champion idol is not ready yet."); // Unsupported case
				return false;
			}

			if (idol.Spawn.Active)
			{
				from.SendMessage("A Champion has already been challenged.");
				return false;
			}

			from.Direction = from.GetDirectionTo(idol);
			from.Animate(32, 5, 1, true, false, 0); // Bow
			from.PrivateOverheadMessage(MessageType.Regular, 1153, false, "You smash the skull on the idol.", from.NetState);

			idol.Spawn.Start(from, spawnType, spawnSize, monsterDifficulty);

			return true;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IdolOfTheChampion)
			{
				if (!Activate(from, (IdolOfTheChampion)o, _championSkull.Type, _spawnSize, _monsterDifficulty)) return;

				_championSkull.Delete();
			}
			else
			{
				from.SendMessage("You must target a champion idol.");
			}
		}
	}
}