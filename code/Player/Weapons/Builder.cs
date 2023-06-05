using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

[Library( "tf_weapon_builder" )]
public partial class Builder : TFWeaponBase
{
	[ConVar.Replicated( "tf_obj_build_rotation_speed" )]
	public static float RotationSpeed { get; set; } = 10f;
	[Net] public BuildingData PlacementData { get; set; }
	[Net] public TFBuilding CarriedBuilding { get; private set; }
	public BuildingData BuildingData => CarriedBuilding?.Data ?? PlacementData;
	/// <summary>
	/// Are we carrying an existing building?
	/// </summary>
	public bool IsCarryingBuilding => CarriedBuilding != null;
	/// <summary>
	/// Clientside blueprint
	/// </summary>
	public AnimatedEntity Blueprint { get; set; }
	bool hasBuilt = false;
	//
	// Rotation
	//
	[Net] public float TargetBuildRotation { get; set; }
	[Net] public float CurrentBuildRotation { get; set; }
	public override bool NeedsAmmo() => false;
	public override void Attack()
	{
		Build();
	}

	public virtual void Build()
	{
		if ( BuildingData == null )
		{
			Log.Error( $"Tried to build with no building data!" );
			return;
		}

		var placementResult = CalculateBuildingPlacement();
		if ( placementResult.Status != BuildingDeployResponse.CanBuild )
		{
			Log.Info( $"Tried to build even though we cant, ignoring" );
			return;
		}

		if(!CanBuildAt(BuildingData, new(placementResult.Origin, placementResult.Rotation)))
		{
			Log.Info( "Tried to build at invalid position, ignoring" );
			return;
		}

		Transform buildingTransform = new( placementResult.Origin, placementResult.Rotation );
		if ( IsCarryingBuilding )
		{
			CarriedBuilding.StopCarrying( buildingTransform );
		}
		else
		{
			TFOwner.Build( PlacementData, buildingTransform );
		}
		PlayDeployVO();

		hasBuilt = true;
		StopPlacement();
	}

	const float ROTATE_COOLDOWN = 0.5f;
	public override void SecondaryAttack()
	{
		// Rotate build angles
		TargetBuildRotation += 90;
		TargetBuildRotation %= 360;
		NextSecondaryAttackTime = Time.Now + ROTATE_COOLDOWN;
	}

	public void StartPlacement()
	{
		if( BuildingData == null)
		{
			Log.Error( "Tried to start placement without building data!" );
			return;
		}

		if ( Game.IsClient )
		{
			// TODO: Add check if player is carying an object.
			Blueprint = new AnimatedEntity
			{
				Owner = Owner
			};

			Blueprint.SetModel( BuildingData.BlueprintModel);
			Blueprint.UseAnimGraph = true;
		}

		hasBuilt = false;
		ResetForcedWeapon();
	}

	private void ResetForcedWeapon()
	{
		if( TFOwner.ForcedActiveWeapon == this)
			TFOwner.ForcedActiveWeapon = null;
	}

	/// <summary>
	/// Stop placing an object. This removes the blueprint.
	/// </summary>
	public void StopPlacement()
	{
		if ( Game.IsClient )
		{
			Blueprint?.Delete();
			Blueprint = null;
		}
		else
		{
			CarriedBuilding = null;
			ResetForcedWeapon();
			TFOwner.SwitchToLastWeapon(this);
		}
	}

	public void CarryBuilding(TFBuilding building)
	{
		building.StartCarrying();
		CarriedBuilding = building;
	}
	protected virtual void PlayDeployVO()
	{
		if(!IsCarryingBuilding)
			TFOwner.PlayResponse( BuildingData.BuiltVO );

		// TODO: Redeploy VO
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		var rotation = Rotation.From( 0, CurrentBuildRotation, 0 );
		var target = Rotation.From( 0, TargetBuildRotation, 0 );

		var lerped = Rotation.Slerp( rotation, target, Time.Delta * RotationSpeed );
		CurrentBuildRotation = lerped.Yaw();
		
		if(Game.IsClient)
		{
			if ( Blueprint == null ) return;

			var result = CalculateBuildingPlacement();
			Blueprint.Position = result.Origin;
			Blueprint.Rotation = result.Rotation;
			bool success = result.Status == BuildingDeployResponse.CanBuild && CanBuildAt( BuildingData, new( result.Origin, result.Rotation ) );
			Blueprint.SetAnimParameter( "reject", !success );
		}
	}

	/// <summary>
	/// Can this building be placed at a specific location?
	/// </summary>
	/// <param name="data"></param>
	/// <param name="location">Location it should be placed at</param>
	/// <returns></returns>
	public static bool CanBuildAt( BuildingData data, Transform location )
	{
		if(NoBuildZone.InNoBuild(location.Position))
			return false;

		if ( RespawnRoom.IsInsideRoom( location.Position ) )
			return false;

		if ( CheckStuck( data, location ) )
			return false;

		return true;
	}

	/// <summary>
	/// Would this building get stuck in the given location?
	/// </summary
	/// <param name="data"></param>
	/// <param name="location"></param>
	/// <returns>True if the building would get stuck, false otherwise</returns>
	private static bool CheckStuck( BuildingData data, Transform location )
	{
		var mins = location.PointToWorld( data.Mins );
		var maxs = location.PointToWorld( data.Maxs );

		var tr = Trace.Ray( mins, maxs )
						.WorldAndEntities()
						.WithTag( CollisionTags.Solid )
						.Run();

		if ( tr.Hit ) return true;

		return false;
	}

	/// <summary>
	/// Calculate the position of the building in the world.
	/// </summary>
	/// <returns></returns>
	public BuildingPlacementResult CalculateBuildingPlacement()
	{
		var response = new BuildingPlacementResult();

		if ( TFOwner == null )
		{
			// Owner is not a player (?) cannot build.
			response.Status = BuildingDeployResponse.CannotBuild;
			return response;
		}

		//
		// Rotation
		//

		var yaw = TFOwner.EyeRotation.Yaw();
		var rot = Rotation.From( 0, yaw, 0 );
		var buildDirection = rot.Forward;
		response.Rotation = Rotation.From( 0, yaw + CurrentBuildRotation, 0 );

		//
		// Origin
		//

		float buildDistance = 100;
		float ownerHeight = Owner.WorldSpaceBounds.Size.z;
		float halfOwnerHeight = ownerHeight * .5f;

		Vector3 objectSize = BuildingData.BBox.Size;
		var halfObjectSize = objectSize / 2;
		float objectHeight = objectSize.z;

		Vector3 buildAnchor = Owner.Position + buildDirection * buildDistance;

		Vector3 buildPosLow = buildAnchor - Vector3.Up * objectHeight;
		Vector3 buildPosHigh = buildAnchor + Vector3.Up * halfOwnerHeight;
		Vector3 buildFallbackPos = buildAnchor;

		//
		// Getting build position.
		//
		var tr = Trace.Ray( buildPosHigh, buildPosLow )
			.Ignore( Owner )
			.Ignore( this )
			.Size( BuildingData.BBox )
			.Run();

		bool validHit = true;
		if ( !tr.Hit ) validHit = false;
		if ( tr.StartedSolid ) validHit = false;
		if ( tr.Entity != null && !tr.Entity.IsWorld ) validHit = false;

		Vector3 buildOrigin = validHit ? tr.EndPosition : buildFallbackPos;
		response.Origin = buildOrigin;

		if ( !validHit )
		{
			response.Status = BuildingDeployResponse.InvalidPlace;
			return response;
		}

		if ( !VerifyCorner( buildOrigin, -halfObjectSize.x, -halfObjectSize.y ) ||
			!VerifyCorner( buildOrigin, halfObjectSize.x, +halfObjectSize.y ) ||
			!VerifyCorner( buildOrigin, halfObjectSize.x, -halfObjectSize.y ) ||
			!VerifyCorner( buildOrigin, -halfObjectSize.x, +halfObjectSize.y ) )
		{
			response.Origin = buildFallbackPos;
			response.Status = BuildingDeployResponse.InvalidPlace;
			return response;
		}

		return response;
	}

	private bool VerifyCorner( Vector3 bottomCenter, float offsetX, float offsetY )
	{
		float clearance = 32;
		Vector3 start = bottomCenter + Vector3.Forward * offsetX + Vector3.Right * offsetY;
		Vector3 end = start + Vector3.Down * clearance;

		var tr = Trace.Ray( start, end )
			.UseHitboxes()
			.Ignore( Owner )
			.Ignore( this )
			.Run();

		if ( tr.Fraction < 1.0f &&
			Vector3.Dot( tr.Normal, Vector3.Up ) < 0.65f ) return false;

		return !tr.StartedSolid && tr.Fraction < 1;
	}

	public override void OnDeploy( SDKPlayer owner )
	{
		if ( !IsValid ) return;
		base.OnDeploy( owner );
		StartPlacement();
	}

	public override void OnHolster( SDKPlayer owner )
	{
		if ( !IsValid ) return;
		base.OnHolster( owner );
		if ( !hasBuilt)
			StopPlacement();
	}

	public override bool CanHolster( SDKPlayer ply )
	{
		if ( !base.CanHolster( ply ) ) return false;

		return !IsCarryingBuilding;
	}
}

public struct BuildingPlacementResult
{
	public Vector3 Origin;
	public Rotation Rotation;
	public BuildingDeployResponse Status;
}

public enum BuildingDeployResponse
{
	CanBuild,
	CannotBuild,
	LimitReached,
	InvalidPlace
}
