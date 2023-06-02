using Amper.FPS;
using Sandbox;
using Sandbox.Utility;
using System;
using TFS2.UI;

namespace TFS2;

partial class TFPlayer
{
	bool WasFirstPerson { get; set; }

	[Net, Predicted]
	public bool IsThirdperson { get; set; } //Cannot override base Thirdperson
	bool StayThirdperson { get; set; }

	/// <summary>
	/// Camera Checks, called in TFPlayer.Simulate
	/// </summary>
	public void SimulateCameraLogic()
	{
		if ( InCondition( TFCondition.Taunting ) ) return;

		else if ( Input.Pressed( "Inspect" ) )
		{
			ThirdpersonSet( false );
		}
	}
	public override void CalculatePlayerView()
	{
		Camera.Rotation = ViewAngles.ToRotation();

		if ( IsThirdperson )
		{
			Camera.FirstPersonViewer = null;

			Vector3 targetPos;
			var center = Position + Vector3.Up * 64;
			// DebugOverlay.Axis( center, Rotation );

			var pos = center;
			var rot = ViewAngles.ToRotation();

			float distance = cl_thirdperson_distance * Scale;
			//targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 32) * Scale);
			targetPos = pos;
			targetPos += rot.Forward * -distance;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.WorldOnly()
				.Radius( 8 )
				.Run();

			Camera.Position = tr.EndPosition;
		}
		else
		{
			Camera.Position = this.GetEyePosition();
			Camera.FirstPersonViewer = this;

			SmoothViewOnStairs();

			var punch = ViewPunchAngle;
			Camera.Rotation *= Rotation.From( punch.x, punch.y, punch.z );
			SmoothViewOnStairs();
		}
	}

	public override void CalculateDeathCamView()
	{
		if ( TimeSinceDeath > DeathAnimationTime )
		{
			CalculateFreezeCamView();
			return;
		}

		base.CalculateDeathCamView();
	}

	public void CalculateFreezeCamView()
	{
		Camera.FirstPersonViewer = null;

		var killer = LastAttacker;

		if ( killer == null )
			return;

		// get time for death animation
		var deathAnimTime = DeathAnimationTime;
		// get time for freeze cam to move to the player
		var travelTime = sv_spectator_freeze_traveltime;

		// time that has passed while we are in freeze cam
		var timeInFreezeCam = TimeSinceDeath - deathAnimTime;
		timeInFreezeCam = MathF.Max( 0, timeInFreezeCam );

		// get lerp of the travel
		var travelLerp = Math.Clamp( timeInFreezeCam / travelTime, 0, 1 );

		// getting origin position and killer eye position
		var originPos = LastDeathCamPosition;
		var killerPos = killer.GetEyePosition();

		// direction to target from us.
		var toTarget = killerPos - originPos;
		toTarget = toTarget.Normal;

		// getting distance from that we need to keep from killer's eyes.
		var distFromTarget = FreezeCamDistanceMin;

		// final position, this is where the freezecam will end.
		var targetPos = killerPos - toTarget * distFromTarget;

		//
		// making sure there are no walls in between us
		//

		var tr = Trace.Ray( killerPos, targetPos )
			.WithAnyTags( CollisionTags.Solid )
			.Ignore( killer )
			.Run();

		targetPos = tr.EndPosition;
		if ( tr.Hit ) targetPos += toTarget * MathF.Min( 5, tr.Distance );
		var targetRot = Rotation.LookAt( toTarget );

		Camera.Position = originPos.LerpTo( targetPos, travelLerp * Easing.EaseIn( travelLerp ) );
		Camera.Rotation = targetRot;

		//
		// Playing freezecam sound .3s before we reach destination.
		//

		var freezeSoundLength = .3f;
		var freezeSoundStartTime = travelTime - freezeSoundLength;

		if ( WillPlayFreezeCamSound && timeInFreezeCam > freezeSoundStartTime )
		{
			WillPlayFreezeCamSound = false;
			PlayFreezeCamSound();
		}

		//
		// Freezing screen when we reach lerp 1.
		//

		float fov = Camera.FieldOfView;
		if ( fov <= 0 ) fov = DesiredFieldOfView;

		if ( WillFreezeGameScene && travelLerp >= 1 )
		{
			WillFreezeGameScene = false;
			FreezeCameraPanel.Freeze( killer, sv_spectator_freeze_time, targetPos, targetRot, fov );
		}
	}
	/// <summary>
	/// Changes camera from firstperson to thirdperson and vice-versa
	/// </summary>
	public void SwapCamera() => IsThirdperson = !IsThirdperson;

	/// <summary>
	/// Forces camera to thirdperson if true, firstperson if false
	/// </summary>
	/// <param name="enabled"></param>
	public void ThirdpersonSet( bool enabled ) => IsThirdperson = enabled;
}
