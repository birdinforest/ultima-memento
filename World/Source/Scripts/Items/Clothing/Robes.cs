using System;

namespace Server.Items
{
	// [Flipable( 0x2B6B, 0x3162 )]
	public class JokerRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.jokerrobe";
		[Constructable]
		public JokerRobe() : this( 0 )
		{
		}

		[Constructable]
		public JokerRobe( int hue ) : base( 0x2B6B, hue )
		{
			Name = "jester coat";
			Weight = 3.0;
		}

		public JokerRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x2B69, 0x3160 )]
	public class AssassinRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.assassinrobe";
		[Constructable]
		public AssassinRobe() : this( 0 )
		{
		}

		[Constructable]
		public AssassinRobe( int hue ) : base( 0x2B69, hue )
		{
			Name = "assassin robe";
			Weight = 3.0;
		}

		public AssassinRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x201D, 0x201E )]
	public class VampireRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.vampirerobe";
		[Constructable]
		public VampireRobe() : this( 0 )
		{
		}

		[Constructable]
		public VampireRobe( int hue ) : base( 0x201D, hue )
		{
			Name = "vampire robe";
			Weight = 3.0;
		}

		public VampireRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x201B, 0x201C )]
	public class DragonRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.dragonrobe";
		[Constructable]
		public DragonRobe() : this( 0 )
		{
		}

		[Constructable]
		public DragonRobe( int hue ) : base( 0x201B, hue )
		{
			Name = "dragon robe";
			Weight = 3.0;
		}

		public DragonRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x201F, 0x2020 )]
	public class ChaosRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.chaosrobe";
		[Constructable]
		public ChaosRobe() : this( 0 )
		{
		}

		[Constructable]
		public ChaosRobe( int hue ) : base( 0x201F, hue )
		{
			Name = "chaos robe";
			Weight = 3.0;
		}

		public ChaosRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x2B6A, 0x3161 )]
	public class FancyRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.fancyrobe";
		[Constructable]
		public FancyRobe() : this( 0 )
		{
		}

		[Constructable]
		public FancyRobe( int hue ) : base( 0x2B6A, hue )
		{
			Name = "fancy robe";
			Weight = 3.0;
		}

		public FancyRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x2B6C, 0x3163 )]
	public class GildedRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.gildedrobe";
		[Constructable]
		public GildedRobe() : this( 0 )
		{
		}

		[Constructable]
		public GildedRobe( int hue ) : base( 0x2B6C, hue )
		{
			Name = "gilded robe";
			Weight = 3.0;
		}

		public GildedRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x2B6E, 0x3165 )]
	public class OrnateRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.ornaterobe";
		[Constructable]
		public OrnateRobe() : this( 0 )
		{
		}

		[Constructable]
		public OrnateRobe( int hue ) : base( 0x2B6E, hue )
		{
			Name = "ornate robe";
			Weight = 3.0;
		}

		public OrnateRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x2B70, 0x3167 )]
	public class MagistrateRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.magistraterobe";
		[Constructable]
		public MagistrateRobe() : this( 0 )
		{
		}

		[Constructable]
		public MagistrateRobe( int hue ) : base( 0x2B70, hue )
		{
			Name = "magistrate robe";
			Weight = 3.0;
		}

		public MagistrateRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x2B73, 0x316A )]
	public class RoyalRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.royalrobe";
		[Constructable]
		public RoyalRobe() : this( 0 )
		{
		}

		[Constructable]
		public RoyalRobe( int hue ) : base( 0x2B73, hue )
		{
			Name = "royal robe";
			Weight = 3.0;
		}

		public RoyalRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x3175, 0x3178 )]
	public class SorcererRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.sorcererrobe";
		[Constructable]
		public SorcererRobe() : this( 0 )
		{
		}

		[Constructable]
		public SorcererRobe( int hue ) : base( 0x3175, hue )
		{
			Name = "sorcerer robe";
			Weight = 3.0;
		}

		public SorcererRobe( Serial serial ) : base( serial )
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

	public class ScholarRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.scholarrobe";
		[Constructable]
		public ScholarRobe() : this( 0 )
		{
		}

		[Constructable]
		public ScholarRobe( int hue ) : base( 0x2652, hue )
		{
			Name = "scholar robe";
			Weight = 3.0;
		}

		public ScholarRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x2FBA, 0x3174 )]
	public class NecromancerRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.necromancerrobe";
		[Constructable]
		public NecromancerRobe() : this( 0 )
		{
		}

		[Constructable]
		public NecromancerRobe( int hue ) : base( 0x2FBA, hue )
		{
			Name = "necromancer robe";
			Weight = 3.0;
		}

		public NecromancerRobe( Serial serial ) : base( serial )
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

	// [Flipable( 0x2FC6, 0x2FC7 )]
	public class SpiderRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.spiderrobe";
		[Constructable]
		public SpiderRobe() : this( 0 )
		{
		}

		[Constructable]
		public SpiderRobe( int hue ) : base( 0x2FC6, hue )
		{
			Name = "spider robe";
			Weight = 3.0;
		}

		public SpiderRobe( Serial serial ) : base( serial )
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

	public class VagabondRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.vagabondrobe";
		[Constructable]
		public VagabondRobe() : this( 0 )
		{
		}

		[Constructable]
		public VagabondRobe( int hue ) : base( 0x567D, hue )
		{
			Name = "vagabond robe";
			Weight = 3.0;
		}

		public VagabondRobe( Serial serial ) : base( serial )
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

	public class PirateCoat : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.piratecoat";
		[Constructable]
		public PirateCoat() : this( 0 )
		{
		}

		[Constructable]
		public PirateCoat( int hue ) : base( 0x567E, hue )
		{
			Name = "pirate coat";
			Weight = 3.0;
		}

		public PirateCoat( Serial serial ) : base( serial )
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

	public class JesterGarb : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.jestergarb";
		[Constructable]
		public JesterGarb() : this( 0 )
		{
		}

		[Constructable]
		public JesterGarb( int hue ) : base( 0x4C16, hue )
		{
			Name = "jester garb";
			Weight = 3.0;
		}

		public JesterGarb( Serial serial ) : base( serial )
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

	public class FoolsCoat : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.foolscoat";
		[Constructable]
		public FoolsCoat() : this( 0 )
		{
		}

		[Constructable]
		public FoolsCoat( int hue ) : base( 0x4C17, hue )
		{
			Name = "fool's coat";
			Weight = 3.0;
		}

		public FoolsCoat( Serial serial ) : base( serial )
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


	public class ExquisiteRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.exquisiterobe";
		[Constructable]
		public ExquisiteRobe() : this( 0 )
		{
		}

		[Constructable]
		public ExquisiteRobe( int hue ) : base( 0x283, hue )
		{
			Name = "exquisite robe";
			Weight = 3.0;
		}

		public ExquisiteRobe( Serial serial ) : base( serial )
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
	public class ProphetRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.prophetrobe";
		[Constructable]
		public ProphetRobe() : this( 0 )
		{
		}

		[Constructable]
		public ProphetRobe( int hue ) : base( 0x284, hue )
		{
			Name = "prophet robe";
			Weight = 3.0;
		}

		public ProphetRobe( Serial serial ) : base( serial )
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
	public class ElegantRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.elegantrobe";
		[Constructable]
		public ElegantRobe() : this( 0 )
		{
		}

		[Constructable]
		public ElegantRobe( int hue ) : base( 0x285, hue )
		{
			Name = "elegant robe";
			Weight = 3.0;
		}

		public ElegantRobe( Serial serial ) : base( serial )
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
	public class FormalRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.formalrobe";
		[Constructable]
		public FormalRobe() : this( 0 )
		{
		}

		[Constructable]
		public FormalRobe( int hue ) : base( 0x286, hue )
		{
			Name = "formal robe";
			Weight = 3.0;
		}

		public FormalRobe( Serial serial ) : base( serial )
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
	public class ArchmageRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.archmagerobe";
		[Constructable]
		public ArchmageRobe() : this( 0 )
		{
		}

		[Constructable]
		public ArchmageRobe( int hue ) : base( 0x287, hue )
		{
			Name = "archmage robe";
			Weight = 3.0;
		}

		public ArchmageRobe( Serial serial ) : base( serial )
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
	public class PriestRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.priestrobe";
		[Constructable]
		public PriestRobe() : this( 0 )
		{
		}

		[Constructable]
		public PriestRobe( int hue ) : base( 0x288, hue )
		{
			Name = "priest robe";
			Weight = 3.0;
		}

		public PriestRobe( Serial serial ) : base( serial )
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
	public class CultistRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.cultistrobe";
		[Constructable]
		public CultistRobe() : this( 0 )
		{
		}

		[Constructable]
		public CultistRobe( int hue ) : base( 0x289, hue )
		{
			Name = "cultist robe";
			Weight = 3.0;
		}

		public CultistRobe( Serial serial ) : base( serial )
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
	public class GildedDarkRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.gildeddarkrobe";
		[Constructable]
		public GildedDarkRobe() : this( 0 )
		{
		}

		[Constructable]
		public GildedDarkRobe( int hue ) : base( 0x28A, hue )
		{
			Name = "gilded dark robe";
			Weight = 3.0;
		}

		public GildedDarkRobe( Serial serial ) : base( serial )
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
	public class GildedLightRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.gildedlightrobe";
		[Constructable]
		public GildedLightRobe() : this( 0 )
		{
		}

		[Constructable]
		public GildedLightRobe( int hue ) : base( 0x301, hue )
		{
			Name = "gilded light robe";
			Weight = 3.0;
		}

		public GildedLightRobe( Serial serial ) : base( serial )
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
	public class SageRobe : BaseOuterTorso
	{
		public override string DisplayNameLocalizationKey => "item.equip.clothing.sagerobe";
		[Constructable]
		public SageRobe() : this( 0 )
		{
		}

		[Constructable]
		public SageRobe( int hue ) : base( 0x302, hue )
		{
			Name = "sage robe";
			Weight = 3.0;
		}

		public SageRobe( Serial serial ) : base( serial )
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