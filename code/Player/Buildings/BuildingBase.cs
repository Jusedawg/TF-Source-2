using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public abstract partial class TFBuilding : AnimatedEntity, IHasMaxHealth, ITargetID, ITeam, IInteractableTargetID
{
	[ConVar.Replicated] public static bool tf_debug_buildings { get; set; }
	[ConCmd.Admin("tf_spawn_building")]
	public static void DebugSpawnBuilding(string name)
	{
		if ( ConsoleSystem.Caller.Pawn is not TFPlayer ply ) return;

		const float SPAWN_BUILDING_DISTANCE = 2048f;
		var pos = ply.EyePosition + ply.EyeRotation.Forward * SPAWN_BUILDING_DISTANCE;
		var tr = Trace.Ray( ply.EyePosition, pos )
						.WorldAndEntities()
						.Ignore(ply)
						.WithTag( CollisionTags.Solid )
						.Run();

		ply.Build( name, new( tr.EndPosition, ply.EyeRotation.Angles().WithPitch(0).WithRoll(0).ToRotation() ) );
	}

	[Net] public bool IsInitialized { get; protected set; }
	/// <summary>
	/// Are we being carried in a toolbox right now?
	/// </summary>
	[Net] public bool IsCarried { get; protected set; }
	[Net] public new TFPlayer Owner { get; protected set; }
	[Net] public BuildingData Data { get; protected set; }
	[Net] public TFTeam Team { get; protected set; }
	[Net] public float MaxHealth { get; set; }
	/// <summary>
	/// The level this building should be at
	/// </summary>
	[Net] public int RequestedLevel { get; protected set; }
	/// <summary>
	/// The level this building is currently at, can differ from <see cref="RequestedLevel"/> while constructing.
	/// </summary>
	[Net] public int Level { get; protected set; }
	public BuildingLevelData GetRequestedLevelData() => Data.Levels.ElementAtOrDefault(RequestedLevel-1);
	public BuildingLevelData GetLevelData() => Data.Levels.ElementAtOrDefault( Level-1 );
	protected int maxLevel;
	public override void Spawn()
	{
		base.Spawn();
		Health = 1;
		UseAnimGraph = false;
	}
	public virtual void Initialize(BuildingData data)
	{
		Data = data;

		maxLevel = data.LevelCount;
		SetLevel( 1 );
		RequestedLevel = 1;
		
		IsInitialized = true;
		StartCarrying();
	}
	public virtual void InitializeModel(string name)
	{
		SetModel(name);
		SetMaterialGroup( Team.GetName() );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Data.Mins, Data.Maxs );
	}
	public virtual void SetOwner(TFPlayer owner)
	{
		Owner = owner;
		Team = owner.Team;
	}
	public virtual void SetLevel(int level)
	{
		if ( level > maxLevel )
			level = maxLevel;
		else if ( level <= 0 )
			level = 1;

		Level = level;

		var levelData = GetLevelData();
		MaxHealth = levelData.MaxHealth;
	}

	[GameEvent.Tick.Server]
	public virtual void Tick()
	{
		if ( IsCarried ) return;

		CheckState();
		if ( IsConstructing )
			TickConstruction();
		else
			TickActive();

		Debug();
	}
	/// <summary>
	/// Called when the building is fully active (No construction/sapping going on)
	/// </summary>
	public abstract void TickActive();

	/// <summary>
	/// Check if we should transition into a new state
	/// </summary>
	protected virtual void CheckState()
	{
		if ( !HasConstructed && !IsConstructing )
			StartConstruction();
	}

	public virtual void StartCarrying()
	{
		IsCarried = true;
		EnableAllCollisions = false;
		EnableDrawing = false;
		HasConstructed = false;

		Parent = Owner;
	}

	public virtual void StopCarrying(Transform deployTransform)
	{
		IsCarried = false;
		EnableAllCollisions = true;
		EnableDrawing = true;
		Parent = null;
		SetLevel( 1 );

		Transform = deployTransform;
	}

	protected virtual void Debug()
	{
		DebugOverlay.Box( this, Color.White.Darken( 0.2f ) );

		Vector3 pos = Position + Vector3.Up * CollisionBounds.Maxs.z;
		DebugOverlay.Text( $"Building - {ClassName}", pos, 0, Color.Cyan );

		DebugOverlay.Text( $"[DATA]", pos, 2, Color.Orange );
		DebugOverlay.Text( $"= Data: {Data}", pos, 3, Color.Yellow );
		DebugOverlay.Text( $"= Level: {Level}/{maxLevel}", pos, 4, Color.Yellow );
		DebugOverlay.Text( $"= RequestedLevel: {RequestedLevel}", pos, 5, Color.Yellow );

		if( IsConstructing )
		{
			DebugOverlay.Text( $"[CONSTRUCTION]", pos, 7, Color.Red );
			DebugOverlay.Text( $"= ConstructionProgress: {ConstructionProgress}", pos, 8, Color.Yellow );
			DebugOverlay.Text( $"= ConstructionTime: {ConstructionTime}", pos, 9, Color.Yellow );
			DebugOverlay.Text( $"= healthToGain: {healthToGain}", pos, 10, Color.Yellow );
		}
	}

	#region ITeam
	public int TeamNumber => (int)Team;
	#endregion

	#region UI
	public string Avatar => "";
	public string InteractText => "Pick Up";
	public string InteractButton => "attack2";
	public bool CanInteract( TFPlayer user ) => user == Owner;
	#endregion
}
