using Sandbox;
using Editor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TFS2;

[Library( "tf_func_respawnroom" )]
[Title("Respawn Room")]
[Category("Gameplay")]
[Icon("Home")]
[HammerEntity]
public partial class RespawnRoom : BaseTrigger, IResettable
{
	[Property( "Team", Title = "Default Team" )] 
	public HammerTFTeamOption DefaultTeamOption { get; set; }

	/// <summary>
	/// The tf_control_point associated with this respawn room. Ownership of control points will control this spawn point's team or enabled state.
	/// </summary>
	[Property, FGDType( "target_destination" )]
	public string AssociatedControlPoint { get; set; }

	[Property]
	public bool SwitchTeamAutomatically { get; set; } = true;

	List<RespawnRoomVisualizer> Visualizers { get; set; } = new();
	List<TeamSpawnPoint> SpawnPoints { get; set; } = new();

	public ControlPoint ControlPoint { get; set; }
	public HammerTFTeamOption TeamOption { get; set; }
	public bool IsInside( TFPlayer ply ) => TouchingEntities.Contains( ply ) && Enabled;
	bool StartsEnabled = true;
	public override void Spawn()
	{
		base.Spawn();
		StartsEnabled = Enabled;
		Transmit = TransmitType.Always;

		Reset();
	}

	public void Reset( bool fullRoundReset = true )
	{
		TeamOption = DefaultTeamOption;
		Enabled = StartsEnabled;
	}

	[GameEvent.Entity.PostSpawn]
	public void PostLevelSetup()
	{
		ControlPoint = FindByName( AssociatedControlPoint ) as ControlPoint;
	}

	public override bool PassesTriggerFilters( Entity other )
	{
		if ( other is TFPlayer ply )
			return TeamOption.Is( ply.Team );

		return false;
	}

	public void AddVisualizer( RespawnRoomVisualizer visualizer )
	{
		Visualizers.Add( visualizer );
	}

	public void AddSpawnPoint( TeamSpawnPoint point )
	{
		SpawnPoints.Add( point );
	}

	/// <summary>
	/// If player is inside their team's respawn room.
	/// </summary>
	public static bool IsInsideTeamRoom( TFPlayer player )
	{
		foreach ( var room in All.OfType<RespawnRoom>() )
		{
			if ( room.IsInside( player ) )
				return true;

		}
		return false;
	}

	public static bool IsInsideRoom(Vector3 pos)
	{
		foreach(var room in All.OfType<RespawnRoom>())
		{
			if ( !room.Enabled ) continue;

			if ( room.WorldSpaceBounds.Contains( pos ) )
				return true;
		}

		return false;
	}

	[GameEvent.Tick.Server]
	public void Tick()
	{
		if(ControlPoint != null)
		{
			if ( SwitchTeamAutomatically )
				TeamOption = ControlPoint.OwnerTeam.ToOption();
			else
				Enabled = TeamOption.Is( ControlPoint.OwnerTeam );
		}

		if ( !tf_debug_spawnrooms )
			return;

		DebugOverlay.Text(
			$"Enabled: {Enabled}\n" +
			$"TeamOption: {TeamOption}\n" +
			$"ControlPoint: {ControlPoint}\n" +
			$"Visualizers: {Visualizers.Count}\n" +
			$"Spawn Points: {SpawnPoints.Count}",
			WorldSpaceBounds.Center );

	}

	[ConVar.Replicated] public static bool tf_debug_spawnrooms { get; set; }
}
