using Server.Commands;
using Server.Commands.Generic;
using Server.Mobiles;

namespace Server.Temptation
{
	public class TemptationsCommand : BaseCommand
	{
		public TemptationsCommand()
		{
			AccessLevel = AccessLevel.Player;
			Supports = CommandSupport.AllMobiles;
			Commands = new string[] { "Temptations" };
			ObjectTypes = ObjectTypes.Mobiles;
			Usage = "Temptations";
			Description = "Gets the Temptations for the targeted character.";
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			var from = (PlayerMobile)e.Mobile;
			var pm = obj as PlayerMobile;
			if (pm == null)
			{
				LogFailure("That is not a player.");
				return;
			}

			CommandLogging.WriteLine(from, "{0} {1} viewing Temptations of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(pm));

			var context = TemptationEngine.Instance.GetContextOrDefault(pm);
			TemptationGump.Open(pm, from, context);
		}
	}
}