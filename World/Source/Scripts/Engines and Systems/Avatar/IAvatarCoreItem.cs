using Server.Mobiles;

namespace Server.Engines.Avatar
{
	/// <summary>
	/// Account-scoped class core items that survive Avatar permadeath via migration + optional dormancy.
	/// </summary>
	public interface IAvatarCoreItem
	{
		bool IsDormant { get; set; }

		void SnapshotToContext( PlayerContext ctx );

		void RestoreFromContext( PlayerContext ctx );

		void ApplyResourceDecay();

		void RebindOwner( PlayerMobile newOwner );

		void ActivateResonance( PlayerMobile player );
	}

	public enum ResonancePath
	{
		Search,
		Registrar,
		Sage
	}
}
