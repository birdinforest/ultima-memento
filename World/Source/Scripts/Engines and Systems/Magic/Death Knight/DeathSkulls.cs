using Server.Spells.DeathKnight;

namespace Server.Items
{
	public class DeathKnightSkull750 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull750() : base( BanishSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Saint Kargoth";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull750( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull751 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull751() : base( DemonicTouchSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Lord Monduiz Dephaar";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull751( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull752 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull752() : base( DevilPactSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Lady Kath of Naelex";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull752( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull753 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull753() : base( GrimReaperSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Prince Myrhal of Rax";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull753( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull754 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull754() : base( HagHandSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Sir Maeril of Naelax";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull754( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull755 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull755() : base( HellfireSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Sir Farian of Lirtham";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull755( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull756 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull756() : base( LucifersBoltSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Lord Androma of Gara";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull756( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull757 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull757() : base( OrbOfOrcusSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Sir Oslan Knarren";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull757( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull758 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull758() : base( ShieldOfHateSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Sir Rezinar of Haxx";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull758( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull759 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull759() : base( SoulReaperSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Lord Thyrian of Naelax";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull759( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull760 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull760() : base( StrengthOfSteelSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Sir Minar of Darmen";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull760( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull761 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull761() : base( StrikeSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Duke Urkar of Torquann";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull761( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull762 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull762() : base( SuccubusSkinSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Sir Luren the Boar";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull762( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	public class DeathKnightSkull763 : SpellScroll
	{
		[Constructable]
		public DeathKnightSkull763() : base( WrathSpell.SpellInfo.SpellDefinition.SpellID, 0x1AE0 )
		{
			ItemID = Utility.RandomList( 0x1AE0, 0x1AE1, 0x1AE2, 0x1AE3 );
			Hue = 0xB9A;
			Name = "Death Knight Skull";

			ColorText4 = "Lord Khayven of Rax";
			ColorHue4 = "CC1313";
			ColorText5 = DeathKnightSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "D03DD9";
		}

		public override string DefaultDescription{ get{ return DeathKnightSpell.SpellDescription( SpellID ); } }

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( "This skull is from a long dead death knight." );
		}

		public DeathKnightSkull763( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}