using System;
using Amper.FPS;
using Sandbox;

namespace TFS2;

partial class TFPlayerAnimator : PlayerAnimator
{
	new TFPlayer Player => (TFPlayer)base.Player;
	const float DeltaMultiplier = 5f;

	private bool IsTaunting { get; set; }
	private TimeUntil ExitTauntTime;
	const float TauntRotationBlendTime = 0.5f;

	private TimeUntil NextLookTime;
	private Entity LookTarget;

	private static readonly string[] EyeAttachments = new string[] {
		"lefteye",
		"righteye"
	};

	public override void UpdateMovement()
	{
		if ( Player.InCondition( TFCondition.Taunting ) )
		{
			UpdateTauntMovement();

			UpdateTauntRotation();
			
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
			if ( !IsTaunting )
			{
				IsTaunting = true;
			}

			UpdateTauntRotation();
			return;
		}

		var idealRotation = GetIdealRotation();

		//Handle Timer Settings when we exit a taunt
		if ( IsTaunting )
		{
			IsTaunting = false;
			ExitTauntTime = TauntRotationBlendTime;
		}

		//if we are in a specific window, rotate slowly without clamping
		if ( !ExitTauntTime )
		{
			Player.Rotation = Rotation.Lerp( Player.Rotation, idealRotation, (float)ExitTauntTime.Fraction );
		}

		else 
		{

			// If we're moving, rotate to our ideal rotation
			if ( Player.Velocity.Length > 10 )
			{
				Player.Rotation = Rotation.Slerp( Player.Rotation, idealRotation, Time.Delta * DeltaMultiplier );
			}
			// Clamp the foot rotation to within 90 degrees of the ideal rotation
			Player.Rotation = Player.Rotation.Clamp( idealRotation, 45 );
		}
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

		UpdateLookTarget();

    }

	// find a valid entity to stare intensely at
	private void UpdateLookTarget()
	{
		if ( NextLookTime || LookTarget == null || !LookTarget.IsValid )
		{
            var random = new Random();

			Entity best = null;
			float bestDist = float.MaxValue;

			foreach ( var ent in Entity.FindInSphere( Player.Position, 1024 ) )
			{
				if (ent == Player)
					continue;

				float dist = Vector3.DistanceBetweenSquared(Player.Position, ent.Position);

				// give extra weight to other players
				if (ent is TFPlayer)
					dist *= 0.5f;

				if (dist < bestDist)
				{
					var headDir = Player.GetEyeRotation().Forward;
					var targetDir = (ent.Position - Player.GetEyePosition()).Normal;

					if (Vector3.Dot(headDir, targetDir) > 0.7f)
					{
                        bestDist = dist;
                        best = ent;
                    }
				}
			}

			LookTarget = best;

			// Source SDK timing
			NextLookTime = random.Int( 1, 5 );
		}

		// now, update the eye shader params
		foreach (var att in EyeAttachments)
		{
			if (Player.GetAttachment(att) is not Transform eyeTransform)
				continue;

			Vector3 targetPos = Player.GetEyePosition() + Player.GetEyeRotation().Forward * 200;
			if (LookTarget != null && LookTarget.IsValid)
				targetPos = LookTarget.Position;

			Vector3 lookDir = (targetPos - eyeTransform.Position).Normal;

            var eyeForward = lookDir;
            var eyeRight = Vector3.Cross(eyeForward, eyeTransform.Rotation.Up).Normal;
            var eyeUp = Vector3.Cross(eyeRight, eyeForward).Normal;

			// iris scale from the .qc file
			const float irisScale = 0.6f;

			float invScale = 1.0f / irisScale;
			eyeRight *= -invScale;
			eyeUp *= -invScale;

            // offset by 0.5f to place the texture in the eye center
            Vector4 irisU = new Vector4(eyeRight, -Vector3.Dot(eyeRight, eyeTransform.Position) + 0.5f);
            Vector4 irisV = new Vector4(eyeUp, -Vector3.Dot(eyeUp, eyeTransform.Position) + 0.5f);


			// set the dynamic params
			if (Player.SceneObject != null)
			{
                Player.SceneObject.Attributes.Set($"${att}_origin", eyeTransform.Position);
                Player.SceneObject.Attributes.Set($"${att}_iris_u", irisU);
                Player.SceneObject.Attributes.Set($"${att}_iris_v", irisV);
            }
        }
    }

	public void UpdateTauntMovement()
	{
		var LRinput = Input.AnalogMove.y;

		if ( Player.ActiveTaunt?.TauntStrafing == null ? false : Player.ActiveTaunt.TauntStrafing )
		{
			return;
		}

		//If we don't allow strafing, convert LR input into smooth turn blending
		else
		{
			var currX = Player.GetAnimParameterFloat( "move_x" );
			var targetX = MathX.Lerp( currX, -LRinput, Time.Delta * DeltaMultiplier );

			Player.SetAnimParameter( "move_x", targetX );
		}
	}

	public void UpdateTauntRotation()
	{
		if ( Player.TauntEnableMove )
		{
			if ( Player.ActiveTaunt?.TauntStrafing == null ? false : Player.ActiveTaunt.TauntStrafing )
			{
				Player.Rotation = GetIdealRotation();
			}

			else
			{
				var LRinput = Input.AnalogMove.y;
				var targetRot = (QAngle)Player.Rotation;
				targetRot.y += LRinput * 10;

				Player.Rotation = Rotation.Lerp( Player.Rotation, targetRot, Time.Delta * 5 );
			}
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
