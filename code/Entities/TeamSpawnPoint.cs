using Amper.FPS;
using Sandbox;
using Editor;
using System.Linq;

namespace TFS2;

/// <summary>
/// Spawn point marker for Team Fortress: Source 2 gamemode. 
/// </summary>
[Library( "tf_player_teamspawn" )]
[Title("Spawn Point")]
[Category("Gameplay")]
[Icon("meeting_room")]
[EditorModel( "models/editor/team_player_start.vmdl" )]
[DrawAngles]
[HammerEntity]
public partial class TeamSpawnPoint : SDKSpawnPoint, IResettable
{
	[Property("Enabled", Title = "Enabled"), Net]
	public bool StartsEnabled { get; set; } = true;
	/// <summary>
	/// The default team option for this spawn room. If "Associated Respawn Room" is set, it will 
	/// stay in sync with the value of that respawn room.
	/// </summary>
	[Property( "Team", Title = "Default Team" )]
	public HammerTFTeamOption DefaultTeamOption { get; set; }

	/// <summary>
	/// Respawn room this spawn point belongs to. The team value of the respawn room will be used for this spawn point.
	/// </summary>
	[Property, FGDType( "target_destination" )]
	public string AssociatedRespawnRoom { get; set; }
	/// <summary>
	/// Only spawn here when this control point is owned by the players team
	/// </summary>
	[Property, FGDType( "target_destination" )]
	public string AssociatedControlPoint { get; set; }
	/// <summary>
	/// Change team according to the team of the associated respawn room.
	/// If set to false, this spawn just gets disabled when the respawn room changes team.
	/// </summary>
	[Property]
	public bool ChangeTeamAutomatically { get; set; } = true;
	public RespawnRoom Room { get; set; }
	public ControlPoint Point { get; set; }
	public HammerTFTeamOption TeamOption { get; set; }
	public bool Enabled { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Reset();
	}

	public void Reset(bool fullRoundReset = true)
	{
		TeamOption = DefaultTeamOption;
		Enabled = StartsEnabled;
	}

	[GameEvent.Entity.PostSpawn]
	public void PostLevelSetup()
	{
		Room = FindByName( AssociatedRespawnRoom ) as RespawnRoom;

		if ( Room != null )
		{
			Room.AddSpawnPoint( this );
			TeamOption = Room.TeamOption;
		}

		Point = FindByName( AssociatedControlPoint ) as ControlPoint;
	}

	public override bool CanSpawn( SDKPlayer player )
	{
		if ( !IsEnabled() ) return false;

		var playerTeam = (TFTeam)player.TeamNumber;
		if ( Room != null )  
		{
			if ( ChangeTeamAutomatically )
				TeamOption = Room.TeamOption;

			var point = Point ?? Room.ControlPoint;
			var farthestPoints = TFGameRules.Current.GetFarthestOwnedControlPointsWithRespawnRoom( playerTeam );
			if ( farthestPoints != null && !farthestPoints.Contains(point) )
				return false;
		}

		if ( !TeamOption.Is( playerTeam ) )
			return false;

		return true;
	}
	public bool IsEnabled() => Enabled && (Point == null || ITeam.IsSame( Point, this ));
	[Input] public void Enable() => Enabled = true;
	[Input] public void Disable() => Enabled = false;
	[Input] public void Toggle() => Enabled = !Enabled;

	[GameEvent.Tick.Server]
	public void Tick()
	{
		if ( tf_debug_spawnpoints )
			Debug();
	}

	protected virtual void Debug()
	{
		DebugOverlay.Sphere( Position, 10, CanSpawn( All.OfType<TFPlayer>().First() ) ? Color.Green : Color.Red, 0, false );
		DebugOverlay.Line( Position, Position + Vector3.Up * 64, Color.Yellow );
		DebugOverlay.Line( Position, Position - Vector3.Up * 64, Color.Yellow );
		DebugOverlay.Text(
			$"Room Name: {AssociatedRespawnRoom}\n" +
			$"Room: {Room}\n" +
			$"TeamOption: {TeamOption}\n" +
			$"ControlPoint: {AssociatedControlPoint ?? "none"}\n" + 
			$"Enabled: {IsEnabled()}",
			Position + Vector3.Up * 50
		);
	}

	[ConVar.Replicated] public static bool tf_debug_spawnpoints { get; set; }
}
