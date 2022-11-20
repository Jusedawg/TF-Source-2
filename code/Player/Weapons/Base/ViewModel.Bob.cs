using Sandbox;
using System;
using Amper.FPS;

namespace TFS2;

partial class TFViewModel
{
	[ConVar.Client] public static float cl_bobcycle { get; set; } = 0.8f;
	[ConVar.Client] public static float cl_bobup { get; set; } = 0.5f;

	float LastBobTime;
	float LastSpeed;
	float BobTime;
	float VerticalBob;
	float LateralBob;

	public void CalculateViewBob( CameraSetup camSetup )
	{
		float cycle;

		var bobUp = cl_bobup;
		var bobCycle = cl_bobcycle;

		// Don't allow zeros, because we divide by them.
		if ( bobUp <= 0 ) bobUp = 0.01f;
		if ( bobCycle <= 0 ) bobCycle = 0.01f;

		var player = Player;
		if ( !player.IsValid() )
			return;

		//Find the speed of the player
		float speed = player.Velocity.WithZ( 0 ).Length;
		float flmaxSpeedDelta = MathF.Max( 0, (Time.Now - LastBobTime) * 320.0f );

		// don't allow too big speed changes
		speed = Math.Clamp( speed, LastSpeed - flmaxSpeedDelta, LastSpeed + flmaxSpeedDelta );
		speed = Math.Clamp( speed, -320, 320 );

		LastSpeed = speed;

		float bob_offset = speed.RemapVal( 0, 320, 0.0f, 1.0f );

		BobTime += (Time.Now - LastBobTime) * bob_offset;
		LastBobTime = Time.Now;

		//Calculate the vertical bob
		cycle = BobTime - (int)(BobTime / bobCycle) * bobCycle;
		cycle /= bobCycle;

		if ( cycle < bobUp )
		{
			cycle = MathF.PI * cycle / bobUp;
		}
		else
		{
			cycle = MathF.PI + MathF.PI * (cycle - bobUp) / (1 - bobUp);
		}

		VerticalBob = speed * 0.005f;
		VerticalBob = VerticalBob * 0.3f + VerticalBob * 0.7f * MathF.Sin( cycle );

		VerticalBob = Math.Clamp( VerticalBob, -7f, 4f );

		//Calculate the lateral bob
		cycle = BobTime - (int)(BobTime / bobCycle * 2) * bobCycle * 2;
		cycle /= bobCycle * 2;

		if ( cycle < bobUp )
		{
			cycle = MathF.PI * cycle / bobUp;
		}
		else
		{
			cycle = MathF.PI + MathF.PI * (cycle - bobUp) / (1 - bobUp);
		}

		LateralBob = speed * 0.005f;
		LateralBob = LateralBob * 0.3f + LateralBob * 0.7f * MathF.Sin( cycle );
		LateralBob = Math.Clamp( LateralBob, -7.0f, 4.0f );

	}

	public void AddViewModelBobHelper()
	{
		var angles = (QAngle)Rotation;
		var origin = Position;

		var forward = Rotation.Forward;
		var right = Rotation.Right;

		origin += VerticalBob * 0.4f * forward;
		origin.z += VerticalBob * 0.1f;

		angles.Roll += VerticalBob * 0.5f;
		angles.Pitch -= VerticalBob * 0.4f;
		angles.Yaw -= LateralBob * 0.3f;

		origin += LateralBob * 0.2f * right;

		Position = origin;
		Rotation = angles;
	}
}
