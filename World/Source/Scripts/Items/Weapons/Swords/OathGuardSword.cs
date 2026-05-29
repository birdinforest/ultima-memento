using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Server;
using Server.Localization;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	public class OathGuardSword : VikingSword
	{
		private const int PlayerModeMaxHits = 5;

		private bool m_PlayerAcquired;
		private Serial m_SourceGuardSerial;
		private DateTime m_DropTime;
		private DateTime? m_FirstUsedAt;
		private int m_HitsUsed;
		private PlayerMobile m_TransferFromPlayer;

		[CommandProperty( AccessLevel.GameMaster )]
		public bool PlayerAcquired
		{
			get { return m_PlayerAcquired; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Serial SourceGuardSerial
		{
			get { return m_SourceGuardSerial; }
		}

		[Constructable]
		public OathGuardSword() : base()
		{
			InitializeGuardMode();
		}

		public OathGuardSword( Mobile guard ) : this()
		{
			if ( guard != null )
				m_SourceGuardSerial = guard.Serial;
		}

		private void InitializeGuardMode()
		{
			m_PlayerAcquired = false;
			Movable = false;
			MaxHitPoints = 0;
			HitPoints = 0;
			Name = "guard's oath sword";
		}

		private void UpdatePlayerName()
		{
			Name = String.Format(
				CultureInfo.InvariantCulture,
				"oath-spent guard's sword [{0}/{1}]",
				Math.Max( 0, HitPoints ),
				PlayerModeMaxHits );
		}

		public override bool OnEquip( Mobile from )
		{
			if ( !m_PlayerAcquired )
				return base.OnEquip( from );

			PlayerMobile pm = from as PlayerMobile;
			if ( pm == null )
				return false;

			if ( pm.OathCooldownActive )
			{
				pm.SendMessage( StringCatalog.ResolveByKey( pm.Account, "guard.oathbreak.cooldown.blocked" ) );
				return false;
			}

			pm.OathWeaponSerial = Serial;
			return base.OnEquip( from );
		}

		public override void OnAdded( object parent )
		{
			base.OnAdded( parent );

			PlayerMobile newHolder = ResolveRootPlayer( parent );

			if ( !m_PlayerAcquired && newHolder != null )
			{
				TransitionToPlayerMode( newHolder );
				return;
			}

			if ( m_PlayerAcquired && m_TransferFromPlayer != null && newHolder != null && m_TransferFromPlayer != newHolder )
			{
				TrackWeaponTransferred( m_TransferFromPlayer, newHolder );
				m_TransferFromPlayer = null;
			}
		}

		public override void OnRemoved( object parent )
		{
			PlayerMobile oldHolder = ResolveRootPlayer( parent );

			if ( m_PlayerAcquired && oldHolder != null )
				m_TransferFromPlayer = oldHolder;

			base.OnRemoved( parent );

			if ( !m_PlayerAcquired )
				return;

			if ( oldHolder == null )
				return;

			if ( !oldHolder.OathCooldownActive )
				oldHolder.SetOathCooldown( DateTime.UtcNow + TimeSpan.FromMinutes( 20 ) );
		}

		public override void OnHit( Mobile attacker, Mobile defender, double damageBonus )
		{
			int beforeHits = defender != null ? defender.Hits : 0;
			base.OnHit( attacker, defender, damageBonus );

			// Track actual damage dealt after any PvP cap adjustment we apply below.
			int damageDealtAfterCap = Math.Max( 0, beforeHits - ( defender != null ? defender.Hits : beforeHits ) );

			if ( attacker is PlayerMobile && defender is PlayerMobile && defender != null )
			{
				int dealt = beforeHits - defender.Hits;
				if ( dealt > 100 )
					defender.Hits += ( dealt - 100 );

				damageDealtAfterCap = Math.Max( 0, beforeHits - defender.Hits );
			}

			if ( !m_PlayerAcquired || !(attacker is PlayerMobile) )
				return;

			if ( m_FirstUsedAt == null )
				m_FirstUsedAt = DateTime.UtcNow;

			m_HitsUsed++;
			HitPoints = Math.Max( 0, PlayerModeMaxHits - m_HitsUsed );
			UpdatePlayerName();

			TrackWeaponStrike( (PlayerMobile)attacker, defender, damageDealtAfterCap );

			if ( m_HitsUsed >= PlayerModeMaxHits || HitPoints <= 0 )
			{
				PlayerMobile holder = RootParent as PlayerMobile;
				if ( holder != null )
				{
					holder.SetOathCooldown( DateTime.UtcNow + TimeSpan.FromMinutes( 20 ) );
					holder.SendMessage( StringCatalog.ResolveByKey( holder.Account, "guard.oathbreak.cooldown.applied" ) );
				}

				if ( Map != null && Map != Map.Internal )
				{
					Effects.PlaySound( GetWorldLocation(), Map, 0x202 );
					Effects.SendLocationParticles( EffectItem.Create( GetWorldLocation(), Map, EffectItem.DefaultDuration ), 0x376A, 9, 32, 0, 0, 5030, 0 );
				}

				TrackWeaponDestroyed( holder ?? attacker as PlayerMobile );
				Delete();
			}
		}

		private void TransitionToPlayerMode( PlayerMobile firstHolder )
		{
			m_PlayerAcquired = true;
			m_DropTime = DateTime.UtcNow;
			m_HitsUsed = 0;
			m_FirstUsedAt = null;

			Movable = true;
			MinDamage = 500;
			MaxDamage = 900;
			MaxHitPoints = PlayerModeMaxHits;
			HitPoints = PlayerModeMaxHits;
			UpdatePlayerName();

			firstHolder.OathWeaponSerial = Serial;

			var extra = BaseWeaponEventFields( firstHolder );
			extra["weapon_graphic_id"] = ItemID.ToString( CultureInfo.InvariantCulture );
			extra["weapon_hp_current"] = HitPoints.ToString( CultureInfo.InvariantCulture );
			extra["was_assailant"] = "true";
			TrackEvent( firstHolder, "weapon_acquired", extra );
		}

		private void TrackWeaponTransferred( PlayerMobile oldHolder, PlayerMobile newHolder )
		{
			var extra = BaseWeaponEventFields( newHolder );
			extra["old_holder_hash"] = HashCharacter( oldHolder.Account as Server.Accounting.Account, oldHolder.Serial.Value );
			extra["weapon_hp_current"] = HitPoints.ToString( CultureInfo.InvariantCulture );
			TrackEvent( newHolder, "weapon_transferred", extra );
		}

		private void TrackWeaponDestroyed( PlayerMobile holder )
		{
			var extra = BaseWeaponEventFields( holder );
			extra["weapon_hp_current"] = HitPoints.ToString( CultureInfo.InvariantCulture );
			extra["hits_used"] = m_HitsUsed.ToString( CultureInfo.InvariantCulture );

			double toFirstUse = 0;
			double combatDuration = 0;

			if ( m_FirstUsedAt != null )
			{
				toFirstUse = ( m_FirstUsedAt.Value - m_DropTime ).TotalMinutes;
				combatDuration = ( DateTime.UtcNow - m_FirstUsedAt.Value ).TotalSeconds;
			}

			extra["time_to_first_use_minutes"] = toFirstUse.ToString( "F2", CultureInfo.InvariantCulture );
			extra["combat_use_duration_seconds"] = combatDuration.ToString( "F2", CultureInfo.InvariantCulture );

			TrackEvent( holder, "weapon_destroyed", extra );
		}

		private void TrackWeaponStrike( PlayerMobile attacker, Mobile defender, int damageDealt )
		{
			var extra = BaseWeaponEventFields( attacker );
			extra["weapon_hp_current"] = HitPoints.ToString( CultureInfo.InvariantCulture );
			extra["hp_remaining"] = HitPoints.ToString( CultureInfo.InvariantCulture );
			extra["damage_dealt"] = damageDealt.ToString( CultureInfo.InvariantCulture );
			extra["target_serial"] = defender != null ? defender.Serial.ToString() : Serial.Zero.ToString();

			TrackEvent( attacker, "weapon_strike", extra );
		}

		private Dictionary<string, string> BaseWeaponEventFields( PlayerMobile holder )
		{
			var extra = new Dictionary<string, string>();
			extra["weapon_serial"] = Serial.ToString();
			extra["guard_serial"] = m_SourceGuardSerial.ToString();
			extra["new_holder_hash"] = holder != null
				? HashCharacter( holder.Account as Server.Accounting.Account, holder.Serial.Value )
				: "";
			return extra;
		}

		private void TrackEvent( PlayerMobile actor, string eventVariant, Dictionary<string, string> extra )
		{
			AnalyticsLogger.LogCustomEvent( actor, "guard_oathbreak_" + eventVariant, "guard_oathbreak", eventVariant, extra );
		}

		private static PlayerMobile ResolveRootPlayer( object entity )
		{
			if ( entity is PlayerMobile )
				return (PlayerMobile) entity;

			Item item = entity as Item;
			if ( item != null )
				return item.RootParent as PlayerMobile;

			return null;
		}

		public override Density Density
		{
			get { return Density.None; } // Ensure deterministic 5-hit shatter behavior.
		}

		private static string HashCharacter( Server.Accounting.Account acc, int serial )
		{
			if ( acc == null || string.IsNullOrWhiteSpace( acc.Username ) )
				return "";

			// Match AnalyticsLogger's salt+SHA256 scheme (char-level hash includes serial).
			string salt = MySettings.S_AnalyticsAccountSalt;
			if ( string.IsNullOrWhiteSpace( salt ) || salt == "CHANGE_ME_SET_A_LONG_RANDOM_SECRET" )
				return "disabled_missing_salt";

			string username = acc.Username.ToLowerInvariant();
			string key = salt + username + ":" + serial.ToString( CultureInfo.InvariantCulture );

			using ( var sha = SHA256.Create() )
			{
				byte[] bytes = sha.ComputeHash( Encoding.UTF8.GetBytes( key ) );
				var sb = new StringBuilder( bytes.Length * 2 );
				for ( int i = 0; i < bytes.Length; i++ )
					sb.Append( bytes[i].ToString( "x2" ) );
				return sb.ToString();
			}
		}

		public OathGuardSword( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)0 );
			writer.Write( m_PlayerAcquired );
			writer.Write( m_SourceGuardSerial );
			writer.Write( m_DropTime );
			writer.Write( m_HitsUsed );
			writer.Write( m_FirstUsedAt.HasValue );
			if ( m_FirstUsedAt.HasValue )
				writer.Write( m_FirstUsedAt.Value );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			m_PlayerAcquired = reader.ReadBool();
			m_SourceGuardSerial = reader.ReadInt();
			m_DropTime = reader.ReadDateTime();
			m_HitsUsed = reader.ReadInt();
			bool hasFirstUsedAt = reader.ReadBool();
			if ( hasFirstUsedAt )
				m_FirstUsedAt = reader.ReadDateTime();

			if ( !m_PlayerAcquired )
				InitializeGuardMode();
			else
			{
				Movable = true;
				MinDamage = 500;
				MaxDamage = 900;
				MaxHitPoints = PlayerModeMaxHits;
				HitPoints = Math.Max( 0, PlayerModeMaxHits - m_HitsUsed );
				UpdatePlayerName();
			}
		}
	}
}
