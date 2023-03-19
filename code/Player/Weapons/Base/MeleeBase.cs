using Sandbox;
using Amper.FPS;
using System.Linq;

namespace TFS2;

public partial class TFMeleeBase : TFWeaponBase
{
	[Net, Predicted] public float? SmackTime { get; set; }

	// Melees don't have ammo.
	public override bool ShowAmmoOnHud() => false;
	public override bool NeedsAmmo() => false;
	public override int GetRange() => 48;

	public override void OnHolster( SDKPlayer owner )
	{
		SmackTime = null;
		base.OnHolster( owner );
	}

	public override void PrimaryAttack()
	{
		Swing();
	}

	public override void SendAnimParametersOnAttack()
	{
		SendPlayerAnimParameter( "b_fire" );
		SendViewModelAnimParameter( IsCurrentAttackCritical ? "b_fire_critical" : "b_fire" );
	}

	public override void SimulateAttack()
	{
		base.SimulateAttack();

		if ( SmackTime.HasValue && Time.Now > SmackTime )
			Smack();
	}

	public virtual void Swing()
	{
		CalculateIsAttackCritical();

		// send all the appropriate weapon animations.
		SendAnimParametersOnAttack();

		CalculateNextAttackTime();

		PlayAttackSound();

		SmackTime = Time.Now + Data.SmackTime;
	}

	public virtual void Smack()
	{
		var damage = GetDamage();
		FireBullet( damage );
		SmackTime = null;
	}

	public override TraceResult TraceFireBullet( int seedOffset = 0 )
	{
		Game.SetRandomSeed( Time.Tick + seedOffset );
		Vector3 origin = GetAttackOrigin();
		Vector3 target = origin + GetAttackDirectionWithSpread( GetSpread() ) * GetRange();
		
		TraceResult tr = default;
		
		if ( SetupFireBulletTrace( origin, target ).Size( 32 ).RunAll() is {} results )
		{
			// Try to find non teammate hits first.
			tr = results.FirstOrDefault( x => x.Entity is var entity && ITeam.IsEnemy( entity, TFOwner ),
				// If we didn't hit an enemy, get the first trace.
				results.First());
			
			/*
			 * Due to the trace having a size of 32, the decal will be offset from the wall and not render.
			 */

			// See if we reach the wall.
			if ( SetupFireBulletTrace( origin, target ).Run() is var decal && decal.Hit == false ) 
				// Otherwise snap HitPosition to surface.
				decal = SetupFireBulletTrace( tr.EndPosition, tr.EndPosition - tr.Normal * GetRange() ).Run();

			tr.HitPosition = decal.HitPosition;
			tr.Surface = decal.Surface;
		}
		else
		{
			tr.StartPosition = origin;
			tr.EndPosition = target;
		}

		DrawDebugTrace( tr );
		return tr;
	}


	protected override void DrawDebugTrace( TraceResult tr, float time = 5 )
	{
		if ( sv_debug_hitscan_hits )
		{
			DebugOverlay.Line( tr.StartPosition, tr.EndPosition, Game.IsServer ? Color.Yellow : Color.Green, 5f );
			DebugOverlay.Sphere( tr.StartPosition, 2f, Color.Cyan, 5 );
			DebugOverlay.Sphere( tr.EndPosition, 2f, Color.Red, 5 );
			DebugOverlay.Text( $"{tr.Distance}", tr.EndPosition, 5f );
		}
	}

	public override void OnHitEntity( Entity entity, TraceResult tr )
	{
		base.OnHitEntity( entity, tr );
		PlayImpactSound( entity );
	}

	public virtual void PlayImpactSound( Entity entity )
	{
		if ( entity is SDKPlayer )
			PlaySound( Data.SoundHitFlesh );
		else
			PlaySound( Data.SoundHitWorld );
	}

	public override Trace SetupFireBulletTrace( Vector3 Origin, Vector3 Target )
	{
		return base.SetupFireBulletTrace( Origin, Target )
			.UseHitboxes( false );
	}
}
