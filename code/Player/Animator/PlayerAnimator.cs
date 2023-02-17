using System;
using Amper.FPS;
using Sandbox;

namespace TFS2;

partial class TFPlayerAnimator : PlayerAnimator
{
	new TFPlayer Player => (TFPlayer)base.Player;

	public override void UpdateMovement()
	{
		if ( Player.InCondition( TFCondition.Taunting ) )
		{
			UpdateTauntMovement();
			return;
		}

		var velocity = Player.Velocity;
		var speed = velocity.Length;
		var forward = Player.Rotation.Forward.Dot( velocity );
		var sideward = Player.Rotation.Right.Dot( velocity );

		SetAnimParameter( "wishspeed", speed );

		// Yes I know, magic numbers bad, bla bla bla, but this is the easiest workaround atm.
		// When moving diagonally, we only get 0.7 for both x and y, so we just multiply that
		// so it is always above 1, even when moving diagonally, and clamp it

		// FIX, make it so that playermodels either use 0.7 as corner values for diagonal movement OR use code below to avoid having to adjust EVERY single move-matrix
		// Or just keep using magic numbers idk
		/*
		var movevector = new Vector2(forward/speed, sideward/speed);
		var adjustvector = AdjustToSquare( movevector );
		*/

		SetAnimParameter( "move_y", Math.Clamp( 1.5f * forward / speed, -1f, 1f ) );
		SetAnimParameter( "move_x", Math.Clamp( 1.5f * sideward / speed, -1f, 1f ) );
	}

public override void UpdateRotation()
	{
		if ( Player.InCondition( TFCondition.Taunting ) )
		{
			UpdateTauntRotation();
			return;
		}

		var idealRotation = GetIdealRotation();

		// If we're moving, rotate to our ideal rotation
		if ( Player.Velocity.Length > 10 )
		{
			Player.Rotation = Rotation.Slerp( Player.Rotation, idealRotation, Time.Delta * 5 ); 
		}
		// Clamp the foot rotation to within 90 degrees of the ideal rotation
		Player.Rotation = Player.Rotation.Clamp( idealRotation, 45 );
	}
	

	public override void UpdateLookAt()
	{
		Vector3 lookAtPos = Player.GetEyePosition() + Player.GetEyeRotation().Forward * 200;

		float pitch = -Player.GetEyeRotation().Pitch();
		float yaw = Player.GetEyeRotation().Yaw() - Player.Rotation.Yaw();
		if ( yaw > 180 )
		{
			yaw -= 360;
		}
		else if ( yaw < -180 )
		{
			yaw += 360;
		}

		SetLookAt( "aim_body", lookAtPos );
		SetAnimParameter( "body_pitch", pitch );
		SetAnimParameter( "body_yaw", yaw );
	}

	public void UpdateTauntMovement()
	{
		var LRinput = Input.AnalogMove.y;
		var currX = Player.GetAnimParameterFloat("move_x");
		var targetX = MathX.Lerp( currX, -LRinput, Time.Delta * 5 );

		SetAnimParameter( "move_x", targetX );
	}

	public void UpdateTauntRotation()
	{
		if ( Player.TauntEnableMove )
		{
			var LRinput = Input.AnalogMove.y;
			var targetRot = (QAngle)Player.Rotation;
			targetRot.y += LRinput * 10;

			Player.Rotation = Rotation.Lerp( Player.Rotation, targetRot, Time.Delta * 5 );
		}
	}

	//Helper function for move_x and move_y, solves issue of diagonal movement returning 0.7 and causing the playermodels to not animate at full speed
	//This is only useful if you want to be rid of magic numbers, but it feels bloaty so I have it disabled until further review
	/*
	public static Vector2 AdjustToSquare( Vector2 vector )
	{
		float x = vector.x;
		float y = vector.y;
		float absX = Math.Abs( x );
		float absY = Math.Abs( y );

		if ( absX > absY )
		{
			y /= absX;
			x = Math.Sign( x );
		}
		else if ( absY > absX )
		{
			x /= absY;
			y = Math.Sign( y );
		}
		else
		{
			x = Math.Sign( x );
			y = Math.Sign( y );
		}

		return new Vector2( x, y );
	}
	*/
}
