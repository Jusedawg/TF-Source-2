using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Amper.FPS;

namespace TFS2;

public partial class SentryGun : IFalloffProvider
{
	protected virtual List<float> LevelFireCooldown => new() { 0.225f, 0.135f, 0.135f };
	protected virtual float BulletDamage => 16f;
	protected virtual float RocketDamage => 100;
	protected virtual float RocketFireCooldown => 3f;
	protected virtual float RocketSpeed => 1100;
	public virtual bool HasRockets => Level == MaxLevel;
	bool IFalloffProvider.UseFalloff => true;
	bool IFalloffProvider.UseRampup => true;
	protected virtual float GetFireCooldown() => LevelFireCooldown.ElementAtOrDefault( Level - 1 );
	protected TimeSince timeSinceFireBullet;
	protected TimeSince timeSinceFireRockets;
	protected virtual void TickFire()
	{
		if ( !CanHitTarget(out var tr) )
			return;

		if ( CanFire() )
			Fire(tr);
		else
			StopFireEffects();

		if ( CanFireRockets() )
			FireRockets();
		else
			StopFireRocketEffects();
	}

	protected const float BULLET_RANGE_INCREASE = 100f;
	protected virtual bool CanHitTarget(out TraceResult tr)
	{
		tr = default;
		if ( Target == null ) return false;

		tr = Trace.Ray( AimRay, Range + BULLET_RANGE_INCREASE )
						.Ignore( this )
						.UseHitboxes()
						.WorldAndEntities()
						.WithTag( CollisionTags.Solid )
						.WithoutTags( Team.GetTag() )
						.Run();

		if ( tr.Entity == null ) return false;
		return tr.Entity == Target;
	}
	protected virtual void Fire(TraceResult tr)
	{
		timeSinceFireBullet = 0;
		if(!HasPrimaryAmmo)
		{
			EmptyFireEffects();
			return;
		}

		var info = ExtendedDamageInfo.Create( BulletDamage )
							.UsingTraceResult( tr )
							.WithTag( DamageTags.Bullet )
							.WithAttacker( Owner )
							.WithInflictor( this );

		tr.Entity.TakeDamage( info );
		PrimaryAmmo--;
		FireEffects();
	}

	protected virtual void FireEffects()
	{
		Sound.FromEntity( $"building_sentry.fire{Level}", this );
		SetAnimParameter( "b_fire", false ); // TODO: For some reason b_fire true/false seem to be flipped, investigate this.
	}
	protected virtual void StopFireEffects()
	{
		SetAnimParameter( "b_fire", true );
		SetAnimParameter( "b_empty", false );
	}

	protected virtual bool CanFire()
	{
		return timeSinceFireBullet >= GetFireCooldown();
	}

	protected virtual void FireRockets()
	{
		timeSinceFireRockets = 0;
		if ( !HasSecondaryAmmo )
		{
			EmptyFireEffects();
			return;
		}

		CreateProjectile();
		SecondaryAmmo--;
		FireRocketEffects();
	}

	protected virtual TFProjectile CreateProjectile()
	{
		return Projectile.Create<SentryRockets>( AimRay.Position, AimRay.Forward * RocketSpeed, Owner, this, RocketDamage );
	}

	protected virtual void FireRocketEffects()
	{
		Sound.FromEntity( "building_sentry.fire.rockets", this );
		SetAnimParameter( "b_fire_rockets", true );
	}

	protected virtual void StopFireRocketEffects()
	{
		SetAnimParameter( "b_fire_rockets", false );
	}

	protected virtual bool CanFireRockets()
	{
		return HasRockets && timeSinceFireRockets >= RocketFireCooldown;
	}
	protected void EmptyFireEffects()
	{
		Sound.FromEntity( $"building_sentry.fire.empty", this );
		SetAnimParameter( "b_fire", false );
		SetAnimParameter( "b_empty", true );
	}
}
