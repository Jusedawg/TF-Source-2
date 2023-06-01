﻿using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public abstract partial class TFBuilding : AnimatedEntity, IHasMaxHealth, ITargetID, ITeam, IInteractableTargetID, ITargetIDSubtext
{
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
	[Net] public int MaxLevel { get; protected set; }
	[Net] public int AppliedMetal { get; protected set; }
	public BuildingLevelData GetRequestedLevelData() => Data.Levels.ElementAtOrDefault(RequestedLevel-1);
	public BuildingLevelData GetLevelData() => Data.Levels.ElementAtOrDefault( Level-1 );
	public override void Spawn()
	{
		base.Spawn();
		Health = 1;
		UseAnimGraph = false;
	}
	public virtual void Initialize(BuildingData data)
	{
		if ( Game.IsClient ) return;

		Data = data;

		MaxLevel = data.LevelCount;
		SetLevel( 1 );
		RequestedLevel = 1;

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Data.Mins, Data.Maxs );
		EnableAllCollisions = true;

		IsInitialized = true;
		StartCarrying();
	}
	public virtual void InitializeModel(string name)
	{
		SetModel(name);
		SetMaterialGroup( Team.GetName() );

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Data.Mins, Data.Maxs );
		EnableAllCollisions = true;
		UseAnimGraph = true;
	}
	public virtual void SetOwner(TFPlayer owner)
	{
		Owner = owner;
		Team = owner.Team;
	}
	public virtual void SetLevel(int level)
	{
		if ( Game.IsClient ) return;

		if ( level > MaxLevel )
			level = MaxLevel;
		else if ( level <= 0 )
			level = 1;

		Level = level;

		var levelData = GetLevelData();
		MaxHealth = levelData.MaxHealth;
	}

	public virtual int ApplyRepairMetal(int amount, float metalToRepair = 3f, float repairPower = 1)
	{
		if ( amount <= 0 ) return 0; // No point in trying to apply no metal
		if ( Level >= MaxLevel ) return 0;
		if ( IsConstructing || IsUpgrading ) return 0;

		return DoRepair(amount, metalToRepair, repairPower);
	}

	protected virtual int DoRepair(int amount, float metalToRepair, float repairPower)
	{
		float healAmount = 100 * repairPower;
		healAmount = MathF.Min( healAmount, MaxHealth - Health );

		var newHealth = MathF.Min( MaxHealth, Health + healAmount );
		int cost = MathX.CeilToInt((newHealth - Health) * metalToRepair);

		Health = newHealth;
		return cost;
	}

	/// <summary>
	/// Applies metal to the buildings upgrade progress.
	/// </summary>
	/// <param name="amount">The amount of metal to apply</param>
	/// <returns>How much metal has been applied</returns>
	public virtual int ApplyUpgradeMetal(int amount)
	{
		if ( amount <= 0 ) return 0; // No point in trying to apply no metal
		if ( Level >= MaxLevel ) return 0;
		if ( IsConstructing || IsUpgrading ) return 0;

		amount = (int)MathF.Min( Data.UpgradeCost - AppliedMetal, amount );
		AppliedMetal += amount;

		return amount;
	}

	[GameEvent.Tick.Server]
	public virtual void Tick()
	{
		if ( IsCarried ) return;

		CheckState();
		if ( IsConstructing )
			TickConstruction();
		else if ( IsUpgrading )
			TickUpgrade();
		else
			TickActive();

		if(tf_debug_buildings)
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
		else if ( Level != RequestedLevel && !IsUpgrading )
			StartUpgrade( Level + 1 );
		else if (AppliedMetal >= Data.UpgradeCost && !IsUpgrading )
		{
			AppliedMetal -= Data.UpgradeCost;
			RequestedLevel = Level + 1;
			StartUpgrade( RequestedLevel );
		}
	}

	public virtual void StartCarrying()
	{
		if ( Game.IsClient ) return;

		IsCarried = true;
		EnableAllCollisions = false;
		EnableDrawing = false;
		HasConstructed = false;

		Parent = Owner;
	}

	public virtual void StopCarrying(Transform deployTransform)
	{
		if ( Game.IsClient ) return;

		IsCarried = false;
		EnableAllCollisions = true;
		EnableDrawing = true;
		Parent = null;
		SetLevel( 1 );

		Transform = deployTransform;
	}

	public override void TakeDamage( DamageInfo info )
	{
		if(ITeam.IsSame(info.Attacker, this))
		{
			return;
		}

		base.TakeDamage( info );
	}

	public virtual void ManualDestroy()
	{
		var dmg = DamageInfo.Generic( Health )
								.WithTags( "manual_destroy" );
		TakeDamage( dmg );
	}

	public override void OnKilled()
	{
		PlaySound( Data.DestroyedSound );
		// TODO: Explosion Particle
		// TODO: Destroyed Voiceline
		// TODO: Building parts

		base.OnKilled();
	}

	#region Debug
	protected virtual void Debug()
	{
		DebugOverlay.Box( this, Color.White.Darken( 0.2f ) );

		Vector3 pos = Position + Vector3.Up * CollisionBounds.Maxs.z;
		DebugOverlay.Text( $"Building - {ClassName}", pos, 0, Color.Cyan );

		DebugOverlay.Text( $"[DATA]", pos, 2, Color.Orange );
		DebugOverlay.Text( $"= Data: {Data}", pos, 3, Color.Yellow );
		DebugOverlay.Text( $"= Level: {Level}/{MaxLevel}", pos, 4, Color.Yellow );
		DebugOverlay.Text( $"= RequestedLevel: {RequestedLevel}", pos, 5, Color.Yellow );
		DebugOverlay.Text( $"= Owner: {Owner}/{Team}", pos, 6, Color.Yellow );

		if ( IsConstructing )
		{
			DebugOverlay.Text( $"[CONSTRUCTION]", pos, 8, Color.Red );
			DebugOverlay.Text( $"= ConstructionProgress: {ConstructionProgress}", pos, 9, Color.Yellow );
			DebugOverlay.Text( $"= ConstructionTime: {ConstructionTime} (x{GetConstructionRate()})", pos, 10, Color.Yellow );
			DebugOverlay.Text( $"= healthToGain: {healthToGain}", pos, 11, Color.Yellow );
		}
		else if(IsUpgrading)
		{
			DebugOverlay.Text( $"[UPGRADE]", pos, 8, Color.Blue );
			DebugOverlay.Text( $"= UpgradeProgress: {UpgradeProgress}", pos, 9, Color.Yellow );
			DebugOverlay.Text( $"= UpgradeTime: {UpgradeTime}", pos, 10, Color.Yellow );
		}
		else
		{
			DebugOverlay.Text( $"[ACTIVE]", pos, 8, Color.Green );
			DebugOverlay.Text( $"= AppliedMetal: {AppliedMetal}", pos, 9, Color.Yellow );
		}
	}

	[ConVar.Replicated] public static bool tf_debug_buildings { get; set; }
	[ConCmd.Admin( "tf_spawn_building" )]
	public static void DebugSpawnBuilding( string name )
	{
		if ( ConsoleSystem.Caller.Pawn is not TFPlayer ply ) return;

		const float SPAWN_BUILDING_DISTANCE = 2048f;
		var pos = ply.EyePosition + ply.EyeRotation.Forward * SPAWN_BUILDING_DISTANCE;
		var tr = Trace.Ray( ply.EyePosition, pos )
						.WorldAndEntities()
						.Ignore( ply )
						.WithTag( CollisionTags.Solid )
						.Run();

		ply.Build( name, new( tr.EndPosition, ply.EyeRotation.Angles().WithPitch( 0 ).WithRoll( 0 ).ToRotation() ) );
	}
	#endregion

	#region ITeam
	public int TeamNumber => (int)Team;
	#endregion

	#region UI
	string ITargetID.Name => $"{Data.Title} built by {Owner.Client.Name}";
	string ITargetID.Avatar => "";
	string IInteractableTargetID.InteractText => "Pick Up";
	string IInteractableTargetID.InteractButton => "Attack2";
	string ITargetIDSubtext.Subtext
	{
		get
		{
			if(Level == MaxLevel)
			{
				return MaxLevelSubtext;
			}
			else
			{
				return NormalSubtext;
			}
		}
	}
	protected virtual string MaxLevelSubtext => $"(Level {Level})";
	protected virtual string NormalSubtext => $"(Level {Level}) Upgrade Progress: {AppliedMetal}/{Data.UpgradeCost}";
	public bool CanInteract( TFPlayer user ) => user == Owner;
	#endregion
}
