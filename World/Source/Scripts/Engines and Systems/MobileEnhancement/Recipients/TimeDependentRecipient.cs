using System;

namespace Server.Engines.MobileEnhancement
{
	/// <summary>
	/// Base for a single recipient of an enhancement. Tracks the Duration
	/// and removes when time is up.
	/// </summary>
	public abstract class TimeDependentRecipient<TEnhancement> : Recipient<TEnhancement>
		where TEnhancement : class
	{
		protected readonly TimeSpan Duration;
		protected DateTime AppliedAt;

		protected TimeDependentRecipient(Mobile targetMobile, TimeSpan duration) : base(targetMobile)
		{
			Duration = duration;
		}

		/// <summary>Default: valid if current time is before AppliedAt + Duration.</summary>
		public override bool GetIsValid(DateTime now)
		{
			if (!base.GetIsValid(now)) return false;

			return now < AppliedAt + Duration;
		}

		public override DateTime? GetNextValidationAt(DateTime appliedAt, DateTime? lastValidated)
		{
			return AppliedAt + Duration;
		}
	}
}