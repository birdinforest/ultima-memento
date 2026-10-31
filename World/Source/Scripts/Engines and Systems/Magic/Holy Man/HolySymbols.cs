using Server.Spells.HolyMan;
using Server.Localization;

namespace Server.Items
{
	public class HolyManSymbol770 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol770() : base( BanishEvilSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Patriarch Morden";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol770( Serial serial ) : base( serial )
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
	public class HolyManSymbol771 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol771() : base( DampenSpiritSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Archbishop Halyrn";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol771( Serial serial ) : base( serial )
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
	public class HolyManSymbol772 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol772() : base( EnchantSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Bishop Leantre";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol772( Serial serial ) : base( serial )
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
	public class HolyManSymbol773 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol773() : base( HammerOfFaithSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Deacon Wilems";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol773( Serial serial ) : base( serial )
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
	public class HolyManSymbol774 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol774() : base( HeavenlyLightSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Drumat the Apostle";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol774( Serial serial ) : base( serial )
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
	public class HolyManSymbol775 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol775() : base( NourishSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Vincent the Priest";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol775( Serial serial ) : base( serial )
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
	public class HolyManSymbol776 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol776() : base( PurgeSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Abigayl the Preacher";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol776( Serial serial ) : base( serial )
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
	public class HolyManSymbol777 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol777() : base( RebirthSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Cardinal Greggs";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol777( Serial serial ) : base( serial )
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
	public class HolyManSymbol778 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol778() : base( SacredBoonSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Father Michal";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol778( Serial serial ) : base( serial )
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
	public class HolyManSymbol779 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol779() : base( SanctifySpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Sister Tiana";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol779( Serial serial ) : base( serial )
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
	public class HolyManSymbol780 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol780() : base( SeanceSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Brother Kurklan";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol780( Serial serial ) : base( serial )
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
	public class HolyManSymbol781 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol781() : base( SmiteSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Edwin the Pope";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol781( Serial serial ) : base( serial )
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
	public class HolyManSymbol782 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol782() : base( TouchOfLifeSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Xephyn the Monk";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol782( Serial serial ) : base( serial )
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
	public class HolyManSymbol783 : SpellScroll
	{
		public override string DefaultDescription{ get{ return HolyManSpell.SpellDescription( SpellID ); } }

		[Constructable]
		public HolyManSymbol783() : base( TrialByFireSpell.SpellInfo.SpellDefinition.SpellID, 0xE5B )
		{
			Hue = 0xB89;
			Name = "holy symbol";

			ColorText4 = "Chancellor Davis";
			ColorHue4 = "4FE9E4";
			ColorText5 = HolyManSpellProvider.GetDefinition(SpellID).Name;
			ColorHue5 = "E5EC79";
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendMessage( StringCatalog.Resolve( from.Account, "This symbol once belonged to a great holy man." ) );
		}

		public HolyManSymbol783( Serial serial ) : base( serial )
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