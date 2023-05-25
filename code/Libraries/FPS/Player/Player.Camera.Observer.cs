using Sandbox;
using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amper.FPS;

public partial class SDKPlayer
{
	public virtual float FreezeCamDistanceMin => 96;
	public virtual float FreezeCamDistanceMax => 200;
	public virtual float ChaseDistanceMin => 16;
	public virtual float ChaseDistanceMax => 96;
	protected Vector3 LastDeathCamPosition { get; set; }
	protected bool WillPlayFreezeCamSound { get; set; }
	protected bool WillFreezeGameScene { get; set; }

	public void CalculateChaseCamView( )
	{
		Camera.FirstPersonViewer = null;

		var target = ObserverTarget;

		if ( target == null )
			return;

		// TODO:
		// VALVE:
		// If our target isn't visible, we're at a camera point of some kind.
		// Instead of letting the player rotate around an invisible point, treat
		// the point as a fixed camera.

		var specPos = target.GetEyePosition() - Rotation.Forward * 96;

		var tr = Trace.Ray( target.GetEyePosition(), specPos )
			.Ignore( target )
			.WithAnyTags( CollisionTags.Solid )
			.Run();

		Camera.Position = tr.EndPosition;
	}

	public virtual void CalculateDeathCamView()
	{
		Camera.FirstPersonViewer = null;

		var killer = LastAttacker;

		// if we dont have a killer use chase cam
		if ( killer == null || this == killer )
			return;

		//
		// Force look at enemy
		//

		float rotLerp = TimeSinceDeath / (DeathAnimationTime / 2);
		rotLerp = Math.Clamp( rotLerp, 0, 1.0f );

		var toKiller = killer.GetEyePosition() - EyePosition;
		toKiller = toKiller.Normal;

		var rotToKiller = Rotation.LookAt( toKiller );
		Camera.Rotation = Rotation.Lerp( Rotation, rotToKiller, rotLerp );

		//
		// Zoom out from our target
		//

		float posLerp = TimeSinceDeath / DeathAnimationTime;
		posLerp = Math.Clamp( posLerp, 0, 1.0f );

		var target = EyePosition + -toKiller * posLerp * ChaseDistanceMax * Easing.QuadraticInOut( posLerp );

		var tr = Trace.Ray( EyePosition, target )
			.WithAnyTags( CollisionTags.Solid )
			.Run();

		target = tr.EndPosition;
		if ( tr.Hit ) target += toKiller * 6;

		Camera.Position = target;

		// position is going to be reset next tick, remember it to use in freezecam.
		LastDeathCamPosition = EyePosition;

		WillPlayFreezeCamSound = true;
		WillFreezeGameScene = true;
	}

	public virtual void PlayFreezeCamSound()
	{
		Sound.FromScreen( "player.freeze_cam" );
	}

	public void CalculateInEyeCamView( )
	{
		var target = ObserverTarget;

		// dont do anything, we don't have target.
		if ( target == null )
			return;

		if ( target.LifeState != LifeState.Alive )
		{
			CalculateChaseCamView( );
			return;
		}

		Camera.Position = target.GetEyePosition();
		Camera.Rotation = target.GetEyeRotation();
		Camera.FirstPersonViewer = target;
	}

	public void CalculateRoamingCamView( )
	{
		Camera.FirstPersonViewer = null;
	}
}
