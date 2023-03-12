using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;

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

		else if ( Input.Pressed( InputButton.Grenade ) )
		{
			SwapCamera();
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
