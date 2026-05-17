using Server;
using System;
using System.Collections;
using Server.Localization;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Misc;
using Server.Mobiles;
using Server.Spells;

namespace Server.Items
{
	public class CrystallineJar : Item
	{
		public override bool IsContentLocalized => true;

		public override string DefaultDescription{ get{ return "These jars are made of a special, crystalline glass. They are mainly used to collecting liquid from strange puddles on the ground. Such puddles are often created from creatures that produce such liquid. They can also collect holy water."; } }
		public override string InfoDataLocalizationKey { get { return "prop.trade.itemdesc.crystallinejar"; } }

		public override Catalogs DefaultCatalog{ get{ return Catalogs.Potion; } }

		[Constructable]
		public CrystallineJar( ) : base( 0x2828 )
		{
			Weight = 1.0;
			Hue = 0;
			Name = "crystalline flask";
			Built = true;
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			if ( BuildingPropertyListLocale != null )
			{
				if ( Name == "crystalline flask" )
				{
					AddLocalizedProperty( list, "item.trade.crystalline.flask" );
					return;
				}

				if ( Name == "flask of holy water" )
				{
					AddLocalizedProperty( list, "item.trade.name.flask.holy.water" );
					return;
				}

				const string flaskPrefix = "flask of ";

				if ( Name != null && Name.StartsWith( flaskPrefix, StringComparison.Ordinal ) && Name.Length > flaskPrefix.Length )
				{
					string sub = Name.Substring( flaskPrefix.Length );
					string subLoc = StringCatalog.TryResolve( BuildingPropertyListLocale, sub ) ?? sub;
					string line = string.Format( ResolvePropertyText( "item.trade.name.flask.of.fmt" ), subLoc );

					list.Add( line );
					return;
				}
			}

			base.AddNameProperty( list );
		}

		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( this.Name == "crystalline flask" )
			{
				if ( BuildingPropertyListLocale != null )
					AddLocalizedProperty( list, "prop.trade.crystallinejar.holds" );
				else
					list.Add( 1070722, "Holds Odd Substances" );
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			Target t;

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1060640 ); // The item must be in your backpack to use it.
			}
			else if ( this.Name == "crystalline flask" )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.scoop" ) );
				t = new ScoopTarget( this );
				from.Target = t;
			}
			else if ( !from.Region.AllowHarmful( from, from ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.bad.idea" ) ); 
				return;
			}
			else if ( Server.Items.MonsterSplatter.TooMuchSplatter( from ) )
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.too.much.liquid" ) ); 
				return;
			}
			else
			{
				from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.dump.where" ) );
				ThrowTarget targ = from.Target as ThrowTarget;

				if ( targ != null && targ.Potion == this )
					return;

				from.RevealingAction();
				from.Target = new ThrowTarget( this );
			}
		}

		private class ScoopTarget : Target
		{
			private CrystallineJar m_Jar;

			public ScoopTarget( CrystallineJar jar ) : base( 1, false, TargetFlags.None )
			{
				m_Jar = jar;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Item )
				{
					Item iJar = targeted as Item;

					if ( from.GetDistanceToSqrt( new Point3D( iJar.X, iJar.Y, iJar.Z ) ) > 2 )
					{
						from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.too.far" ) );
					}
					else if ( (from.Paralyzed || from.Blessed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)) )
					{
						from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.cannot.yet" ) );
					}
					else if ( iJar is MonsterSplatter )
					{
						MonsterSplatter mJar = (MonsterSplatter)iJar;
						if ( mJar.owner is PlayerMobile )
						{
							from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.too.diluted" ) );
						}
						else
						{
							from.RevealingAction();
							from.PlaySound( 0x23F );
							m_Jar.Name = "flask of " + iJar.Name;
							m_Jar.Hue = iJar.Hue;
							m_Jar.Weight = iJar.Weight;
						}
					}
					else if ( iJar is HolyWater )
					{
						from.RevealingAction();
						from.PlaySound( 0x23F );
						m_Jar.Name = "flask of holy water";
						m_Jar.Hue = 0x539;
						m_Jar.Weight = 2.0;
					}
					else
					{
						from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.wrong.substance" ) );
					}
				}
				else
				{
					from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.wrong.substance" ) );
				}
			}
		}

		private class ThrowTarget : Target
		{
			private CrystallineJar m_Potion;

			public CrystallineJar Potion
			{
				get{ return m_Potion; }
			}

			public ThrowTarget( CrystallineJar potion ) : base( 12, true, TargetFlags.None )
			{
				m_Potion = potion;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Potion.Deleted || m_Potion.Map == Map.Internal )
					return;
					
				IPoint3D p = targeted as IPoint3D;
				Point3D d = new Point3D( p );

				if ( p == null || from.Map == null )
					return;

				SpellHelper.GetSurfaceTop( ref p );

				int nThrown = 1;

				if ( from.GetDistanceToSqrt( d ) > 8 )
				{
					nThrown = 0;
					from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.too.far" ) );
				}
				else if ( !from.CanSee( d ) )
				{
					nThrown = 0;
					from.SendLocalizedMessage( 500237 ); // Target can not be seen.
				}
				else if ( (from.Paralyzed || from.Blessed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)) )
				{
					nThrown = 0;
					from.SendMessage( StringCatalog.ResolveByKey( from.Account, "prop.trade.crystallinejar.msg.cannot.yet" ) );
				}
				else
				{
					string jar = m_Potion.Name;
					if ( jar.Contains("flask of ") ){ jar = jar.Replace("flask of ", ""); }
					int glow = 0; if ( m_Potion.Weight == 2.0 ){ glow = 1; }
					MonsterSplatter.AddSplatter( p.X, p.Y, p.Z, from.Map, d, from, jar, m_Potion.Hue, glow );
				}

				if ( nThrown > 0 )
				{
					from.RevealingAction();
					m_Potion.Name = "crystalline flask";
					m_Potion.Hue = 0;
					m_Potion.Weight = 1.0;
				}
			}
		}

		public CrystallineJar( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( ( int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			if ( Hue == 0 ){ Name = "crystalline flask"; }
			ItemID = 0x2828;
			Built = true;
		}
	}
}