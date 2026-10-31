using System;

namespace Server.Spells
{
	public class SpellInfo
	{
		public SpellInfo( SpellDefinition spellDefinition, params Type[] regs ) : this( spellDefinition.Name, spellDefinition.PowerWords, 16, 0, 0, true, regs )
		{
			SpellDefinition = spellDefinition;
		}

		public SpellInfo( SpellDefinition spellDefinition, bool allowTown, params Type[] regs ) : this( spellDefinition.Name, spellDefinition.PowerWords, 16, 0, 0, allowTown, regs )
		{
			SpellDefinition = spellDefinition;
		}

		public SpellInfo( SpellDefinition spellDefinition, int action, params Type[] regs ) : this( spellDefinition.Name, spellDefinition.PowerWords, action, 0, 0, true, regs )
		{
			SpellDefinition = spellDefinition;
		}

		public SpellInfo( SpellDefinition spellDefinition, int action, bool allowTown, params Type[] regs ) : this( spellDefinition.Name, spellDefinition.PowerWords, action, 0, 0, allowTown, regs )
		{
			SpellDefinition = spellDefinition;
		}

		public SpellInfo( SpellDefinition spellDefinition, int action, int handEffect, params Type[] regs ) : this( spellDefinition.Name, spellDefinition.PowerWords, action, handEffect, handEffect, true, regs )
		{
			SpellDefinition = spellDefinition;
		}

		public SpellInfo( SpellDefinition spellDefinition, int action, int handEffect, bool allowTown, params Type[] regs ) : this( spellDefinition.Name, spellDefinition.PowerWords, action, handEffect, handEffect, allowTown, regs )
		{
			SpellDefinition = spellDefinition;
		}
		
		public SpellInfo( string name, string mantra, params Type[] regs ) : this( name, mantra, 16, 0, 0, true, regs )
		{
		}

		public SpellInfo( string name, string mantra, bool allowTown, params Type[] regs ) : this( name, mantra, 16, 0, 0, allowTown, regs )
		{
		}

		public SpellInfo( string name, string mantra, int action, params Type[] regs ) : this( name, mantra, action, 0, 0, true, regs )
		{
		}

		public SpellInfo( string name, string mantra, int action, bool allowTown, params Type[] regs ) : this( name, mantra, action, 0, 0, allowTown, regs )
		{
		}

		public SpellInfo( string name, string mantra, int action, int handEffect, params Type[] regs ) : this( name, mantra, action, handEffect, handEffect, true, regs )
		{
		}

		public SpellInfo( string name, string mantra, int action, int handEffect, bool allowTown, params Type[] regs ) : this( name, mantra, action, handEffect, handEffect, allowTown, regs )
		{
		}

		public SpellInfo( string name, string mantra, int action, int leftHandEffect, int rightHandEffect, bool allowTown, params Type[] regs )
		{
			Name = name;
			Mantra = mantra;
			Action = action;
			Reagents = regs;
			AllowTown = allowTown;
			LeftHandEffect = leftHandEffect;
			RightHandEffect = rightHandEffect;
			Amounts = new int[regs.Length];
			for ( int i = 0; i < regs.Length; ++i )
				Amounts[i] = 1;
		}

		public int Action { get; set; }
		public bool AllowTown { get; set; }
		public int[] Amounts { get; set; }
		public string Mantra { get; set; }
		public string Name { get; set; }
		public Type[] Reagents { get; set; }
		public int LeftHandEffect { get; set; }
		public int RightHandEffect { get; set; }
		public SpellDefinition SpellDefinition { get; private set; }
	}
}