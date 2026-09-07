using Server.Targeting;

namespace Server.Spells
{
	public class SpellDefinition
	{
		public TextDefinition Description { get; set; }
		public ushort IconGraphic { get; set; }
		public int ManaCost { get; set; }
		public int MinSkill { get; set; }
		public TextDefinition Name { get; set; }
		public TextDefinition PowerWords { get; set; }
		public ushort SpellID { get; set; }
		public TargetFlags TargetType { get; set; }
		public int TithingCost { get; set; }
	}
}