using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.CannedEvil;
using Server.Gumps;

namespace Server.Items
{
	public class ChampionSpawnInfoGump : Gump
	{
		private class Damager
		{
			public Mobile Mobile;
			public int Damage;
			public Damager(Mobile mob, int dmg)
			{
				Mobile = mob;
				Damage = dmg;
			}
		}

		private const int gBoarder = 20;
		private const int gRowHeight = 25;
		private const int gFontHue = LabelColors.OFFWHITE;
		private static readonly int[] gWidths = { 20, 160, 160, 20 };
		private static readonly int[] gTab;
		private static readonly int gWidth;

		static ChampionSpawnInfoGump()
		{
			gWidth = gWidths.Sum();
			int tab = 0;
			gTab = new int[gWidths.Length];
			for (int i = 0; i < gWidths.Length; ++i)
			{
				gTab[i] = tab;
				tab += gWidths[i];
			}
		}

		private readonly ChampionSpawn m_Spawn;

		public ChampionSpawnInfoGump(Mobile from, ChampionSpawn spawn)
			: base(40, 40)
		{
			m_Spawn = spawn;

			AddBackground(0, 0, gWidth, gBoarder * 2 + gRowHeight * (8 + spawn.DamageEntries.Count), 3600);

			int top = gBoarder;
			AddLabel(gBoarder, top, gFontHue, string.Format("Champion Spawn - {0}", ChampionSpawnInfo.GetName(spawn.Type)));
			top += gRowHeight;

			if (!spawn.Active)
			{
				AddLabel(gTab[1], top, gFontHue, "Status");
				AddLabel(gTab[2], top, LabelColors.DARK_RED, "Inactive");
				top += gRowHeight;
			}
			else
			{
				if (spawn.Owner == null)
				{
					AddLabel(gTab[1], top, gFontHue, "Status");
					AddLabel(gTab[2], top, gFontHue, "Running");
					top += gRowHeight;
				}
				else
				{
					AddLabel(gTab[1], top, gFontHue, "Owner");
					AddLabel(gTab[2], top, gFontHue, spawn.Owner.RawName);
					top += gRowHeight;
				}

				AddLabel(gTab[1], top, gFontHue, "Red Candles");
				AddLabel(gTab[2], top, gFontHue, string.Format("{0} of 16", spawn.Level));
				top += gRowHeight;

				AddLabel(gTab[1], top, gFontHue, "Kills");
				AddLabel(gTab[2], top, gFontHue, string.Format("{0} of {1} ({2:F1}%)", spawn.Kills, spawn.MaxKills, 100.0 * ((double)spawn.Kills / spawn.MaxKills)));
				top += gRowHeight;

				AddLabel(gTab[1], top, gFontHue, "Next Expiration");
				TimeSpan remaining = spawn.GetExpirationTimeRemaining();
				if (0 < remaining.TotalSeconds) AddLabel(gTab[2], top, gFontHue, remaining.ToString(@"hh\:mm\:ss"));
				top += gRowHeight;
            }

            AddImageTiled(gBoarder + 4, top, gWidth - 47, 2, 9101); // Bottom border
            top += gRowHeight / 3;

            AddLabel(gTab[1], top, gFontHue, "Player");
            AddLabel(gTab[2], top, gFontHue, "Damage");
            top += gRowHeight;

            var damagers = spawn.DamageEntries.Keys
                .Select(mob => new Damager(mob, spawn.DamageEntries[mob]))
                .OrderByDescending(x => x.Damage);

            foreach (Damager damager in damagers)
            {
                AddLabelCropped(gTab[1], top, 100, gRowHeight, gFontHue, damager.Mobile.RawName);
                AddLabelCropped(gTab[2], top, 80, gRowHeight, gFontHue, damager.Damage.ToString());
                top += gRowHeight;
            }

            var lastRowY = top + 23;

            if (spawn.CanStop(from))
            {
                AddButton(gTab[1], lastRowY, 0xFA5, 0xFA7, 100, GumpButtonType.Reply, 0);
                AddLabel(gTab[1] + 30, lastRowY + 4, gFontHue, "Abort");
            }

			if (spawn.Active)
			{
				AddButton(gTab[2], lastRowY, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
				AddLabel(gTab[2] + 30, lastRowY + 4, gFontHue, "Refresh");
			}
        }

		public override void OnResponse(Network.NetState sender, RelayInfo info)
		{
			switch (info.ButtonID)
			{
				case 1:
					m_Spawn.SendGump(sender.Mobile);
					break;

				case 100:
					if (m_Spawn.CanStop(sender.Mobile))
					{
						m_Spawn.Stop();
						m_Spawn.Cleanup();
					}
					break;
			}
		}
	}
}