using System;

namespace Server.Misc
{
	public static class ShardInfo
	{
		/// <summary>
		/// Single source of truth for the shard/game version.
		/// Update this when releasing a new version.
		/// </summary>
		public static readonly Version CurrentVersion = new Version(3, 0, 8);

		/// <summary>
		/// Display string: "Version: X.Y.Z"
		/// </summary>
		public static string VersionString => $"Version: {CurrentVersion}";

		/// <summary>
		/// Numeric code for comparing versions (e.g. 2.3.0 → 20300).
		/// Used to detect version upgrades for forced announcements.
		/// </summary>
		public static int VersionCode => CurrentVersion.Major * 10000 + CurrentVersion.Minor * 100 + CurrentVersion.Build;
	}
}
