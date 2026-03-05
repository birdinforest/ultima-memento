using System.Collections.Generic;

namespace Server.Engines.MobileEnhancement
{
	public abstract class BasicEnhancement<TEnhancement, TRecipient> : EnhancementSourceBase<TEnhancement, TRecipient>
		where TEnhancement : class
		where TRecipient : IEnhancementRecipient<TEnhancement>
	{
		public void Initialize(IEnumerable<Mobile> mobiles)
		{
			foreach (var mobile in mobiles)
			{
				var recipient = CreateRecipient(mobile);
				RegisterRecipient(recipient);
			}
		}
	}
}