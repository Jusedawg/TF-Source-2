using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public partial class Cart
{
	bool wasMoving = false;
	protected virtual void DoMovement()
	{
		if ( !CanMove() )
			return;

		bool isRolling = false;
		if ( CanPush() )
		{
			float maxSpeed = 0;
			switch ( GetCapRate() )
			{
				case 0:
					break;
				case 1:
					maxSpeed = Level1Speed;
					break;
				case 2:
					maxSpeed = Level2Speed;
					break;
				default:
					maxSpeed = Level3Speed;
					break;
			}

			CurrentSpeed += Acceleration * Time.Delta;
			CurrentSpeed = MathF.Min( maxSpeed, CurrentSpeed );

			TimeSincePush = 0;
		}
		else if ( CanRollforward() )
		{
			isRolling = true;
			float maxSpeed = Level3Speed;

			CurrentSpeed += Acceleration * Time.Delta;
			CurrentSpeed = MathF.Min( maxSpeed, CurrentSpeed );
		}
		else if ( CanRollback() )
		{
			isRolling = true;
			float minSpeed = -BackwardsSpeed;
			CurrentSpeed -= Acceleration * Time.Delta;
			CurrentSpeed = MathF.Max( minSpeed, CurrentSpeed );
		}
		else
		{
			float minSpeed = 0f;
			CurrentSpeed -= Acceleration * Time.Delta;
			CurrentSpeed = MathF.Max( minSpeed, CurrentSpeed );
		}

		if ( CurrentSpeed < 0 && CurrentIndex == 0 && CurrentFraction <= 0 )
		{
			CurrentSpeed = 0;
			return;
		}

		if ( CurrentSpeed == 0 )
		{
			if ( wasMoving )
			{
				StopMoveSounds();
				wasMoving = false;
			}

			return;
		}

		bool isMovingReverse = CurrentSpeed < 0;

		if ( isMovingReverse )
		{
			float remainingDistance = MathF.Abs( CurrentSpeed ) * Time.Delta;
			while ( remainingDistance > 0.01f )
			{
				remainingDistance = DoMoveBackwards( remainingDistance );
			}
		}
		else
		{
			float remainingDistance = CurrentSpeed * Time.Delta;
			while ( remainingDistance > 0.01f )
			{
				remainingDistance = DoMoveForwards( remainingDistance );
			}
		}
		var newpos = GetPathPosition();

		Vector3 dir = isMovingReverse ? newpos - Position : Position - newpos;
		Rotation = Rotation.LookAt( dir );

		Position = newpos;

		if ( isRolling )
			RollingSounds();

		if ( !wasMoving )
		{
			StartMoveSounds();
			wasMoving = true;
		}
		else
			MoveSounds();
	}

	protected virtual float DoMoveForwards( float distance )
	{
		float usedDistance = MathF.Min( distance * Time.Delta, distance );
		CurrentFraction += GetSpeedFraction( usedDistance );

		if ( CurrentFraction >= 1 )
		{
			CurrentIndex++;
			CurrentFraction -= 1;
			OnNodeChanged( CurrentNode );

			if ( NextNode == null )
			{
				var nextPath = CurrentNode.GetNextPath();
				if ( nextPath != null )
				{
					Path = nextPath;
					ResetPath();
					return -2;
				}

				IsAtEnd = true;
				StopMoveSounds();
				OnReachEnd.Fire( this );
				Log.Info( $"Reached end at: {CurrentIndex + 1}" );
				return -1;
			}
		}

		return distance - usedDistance;
	}

	protected virtual float DoMoveBackwards( float distance )
	{
		distance = MathF.Abs( distance );
		float usedDistance = MathF.Min( distance * Time.Delta, distance );
		CurrentFraction -= MathF.Abs( GetSpeedFraction( usedDistance ) );

		if ( CurrentFraction < 0 )
		{
			if ( PreviousNode == null )
			{
				StopMoveSounds();
				return 0;
			}

			CurrentIndex--;
			CurrentFraction += 1;
			OnNodeChanged( CurrentNode );
		}

		return distance - usedDistance;
	}
}
