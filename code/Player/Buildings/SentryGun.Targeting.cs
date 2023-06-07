using System;
using System.Collections.Generic;
using System.Linq;
using Amper.FPS;
using Sandbox;

namespace TFS2;

public partial class SentryGun
{
	public virtual float TurnRate => 6f;
	public virtual float Range => 1100f;

	public ModelEntity Target { get; set; }
	public bool HasTarget => Target != null;

	public Rotation AimRotationTarget { get; set; }
	public Rotation AimRotation { get; set; }
	/// <summary>
	/// If we are idle, switch turn direction at this yaw value
	/// </summary>
	public float LeftIdleYaw { get; set; }
	/// <summary>
	/// If we are idle, switch turn direction at this yaw value
	/// </summary>
	public float RightIdleYaw { get; set; }
	
	private bool idleTurningRight = true;
	public virtual void RotateToTarget()
	{
		if ( HasTarget )
		{
			// Aim at the player
			AimRotationTarget = Rotation.LookAt( GetTargetAimPosition() - AimRay.Position );
		}
		else
		{
			if ( idleTurningRight )
				AimRotationTarget = Rotation.FromYaw( RightIdleYaw );
			else
				AimRotationTarget = Rotation.FromYaw( LeftIdleYaw );
		}

		var moved = TurnSentry();

		if ( !moved && !HasTarget )
		{
			idleTurningRight = !idleTurningRight;
			Sound.FromEntity( $"building_sentry.beep{Level}", this );
		}
	}

	protected virtual Vector3 GetTargetAimPosition()
	{
		return Target.CollisionWorldSpaceCenter;
	}

	private float currentTurnRate;
	public virtual bool TurnSentry()
	{
		var travelDist = Rotation.Difference( AimRotationTarget, AimRotation );
		var goalAim = AimRotationTarget.Angles();
		var currentAim = AimRotation.Angles();
		int turndir;
		var moved = false;

		//Handles Pitch Movement
		if ( !currentAim.pitch.AlmostEqual( goalAim.pitch, 0.002f ) )
		{
			moved = true;

			turndir = travelDist.Pitch() > 0 ? 1 : -1;
			currentAim.pitch += Time.Delta * (TurnRate * 5) * turndir;

			if ( turndir == 1 )
			{
				if ( currentAim.pitch > goalAim.pitch )
					currentAim.pitch = goalAim.pitch;
			}
			else
			{
				if ( currentAim.pitch < goalAim.pitch )
					currentAim.pitch = goalAim.pitch;
			}
		}

		//Handles Yaw Movement
		if ( !currentAim.yaw.AlmostEqual( goalAim.yaw, 0.002f ) )
		{
			moved = true;

			var yawdist = Math.Abs( goalAim.yaw - currentAim.yaw );
			turndir = goalAim.yaw > currentAim.yaw ? 1 : -1;
			if ( yawdist > 180 )
			{
				yawdist = 360 - yawdist;
				turndir = -turndir;
			}

			if ( !HasTarget )
			{
				if ( currentTurnRate < TurnRate * 10 )
					currentTurnRate += TurnRate;
				else
					currentTurnRate = TurnRate * 10;
			}
			else
			{
				// We are faster while tracking enemies
				if ( yawdist > 30 )
				{
					if ( currentTurnRate < TurnRate * 30 )
						currentTurnRate += TurnRate * 5;
				}
			}

			var movedyaw = Time.Delta * currentTurnRate;

			if ( Math.Abs( goalAim.yaw - currentAim.yaw ) <= movedyaw )
				currentAim.yaw = goalAim.yaw;
			else
			{
				currentAim.yaw += movedyaw * turndir;
			}
		}

		//We should NEVER have roll
		currentAim.roll = 0;

		AimRotation = Rotation.From( currentAim );

		if ( !moved || currentTurnRate <= 0 )
			currentTurnRate = TurnRate * 5;

		return moved;
	}
	public virtual void FindTarget()
	{
		var lastTarget = Target;
		//If our target leaves range or dies, remove them as our target
		if ( HasTarget )
		{
			if( Position.Distance( Target.Position ) > Range || Target.LifeState != LifeState.Alive )
				Target = null;

			var tr = Trace.Ray( AimRay.Position, Target.Position )
						.Ignore( this )
						.WorldAndEntities()
						.WithTag( CollisionTags.Solid )
						.WithoutTags(CollisionTags.Player)
						.Run();
			if ( tr.Hit )
				Target = null;
		}

		if ( Target != null )
			return;

		//This can likely be optimized
		foreach ( var ent in FindInSphere( AimRay.Position, Range )?.OrderBy(ent => ent.Position.DistanceSquared(Position)) )
		{
			if ( ent is TFPlayer player )
			{
				//Can't target our team, cloaked players, or dead players
				if ( player.Team == Team || player.InCondition( TFCondition.Cloaked ) || player.LifeState != LifeState.Alive ) continue;

				//If we have a target, is the queried player closer?
				if ( Target != null && Position.Distance( player.Position ) > Position.Distance( Target.Position ) ) continue;

				var targetCenter = player.CollisionWorldSpaceCenter;

				// Is line of sight to target obstructed?
				var tr = Trace.Ray( AimRay.Position, targetCenter )
					.Ignore( this )
					.Run();

				if ( tr.Hit )
				{
					if ( tr.Entity is TFPlayer || tr.Entity is TFBuilding ) { }
					else continue;
				}

				Target = player;
			}

			if ( Target != null && Target != lastTarget )
			{
				Sound.FromEntity( $"building_sentry.spot", this );
				break;
			}
		}
	}
}
