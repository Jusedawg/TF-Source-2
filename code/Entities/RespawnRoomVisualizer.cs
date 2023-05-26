using Sandbox;
using Editor;
using Amper.FPS;
using System;

namespace TFS2;

[Library( "tf_func_respawnroomvisualizer" )]
[Title("Respawn Room Visualizer")]
[Category("Gameplay")]
[Icon( "fence" )]
[AutoApplyMaterial( "materials/overlays/no_entry.vmat" )]
[Solid]
[HammerEntity]
public partial class RespawnRoomVisualizer : ModelEntity, IResettable
{
	[Property(Title = "Enabled")]
	public bool StartsEnabled { get; set; }
	[Property, FGDType( "target_destination" )]
	public string AssociatedRespawnRoom { get; set; }
	public RespawnRoom Room { get; set; }
	[Net] bool Enabled { get; set; }
	[Net] HammerTFTeamOption TeamOption { get; set; }

	[GameEvent.Entity.PostSpawn]
	public void PostLevelSetup()
	{
		Room = FindByName( AssociatedRespawnRoom ) as RespawnRoom;

		if ( Room != null )
		{
			Room.AddVisualizer( this );
			SetTeamOption( Room.TeamOption );
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		UsePhysicsCollision = true;
		Tags.Add( CollisionTags.PlayerClip );
		Tags.Add( TFCollisionTags.TeamBarrier );
	}

	public void Reset( bool fullRoundReset = true )
	{
		SetEnabled( StartsEnabled );
	}


	public void SetTeamOption( HammerTFTeamOption option )
	{
		TeamOption = option;

		foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
		{
			if ( !team.IsPlayable() )
				continue;

			Tags.Set( $"team_barrier_{team.GetName()}", TeamOption.Is( team ) );
			// Log.Info( $"Visualizer (room \"{AssociatedRespawnRoom}\") accepts team \"{team}\": {TeamOption.Is( team )}" );
		}
	}
	public void SetEnabled( bool enabled )
	{
		if ( enabled ) Enable();
		else Disable();
	}

	[Input]
	public void Enable()
	{
		Enabled = true;
		EnableAllCollisions = true;
	}
	[Input]
	public void Disable()
	{
		Enabled = false;
		EnableAllCollisions = false;
	}

	[GameEvent.Client.Frame]
	public void Frame()
	{
		EnableDrawing = IsVisibleForLocalPlayer();
	}

	const float FADE_END_DISTANCE = 800f;
	const float FADE_END_DISTANCE_SQR = FADE_END_DISTANCE * FADE_END_DISTANCE;
	public bool IsVisibleForLocalPlayer()
	{
		if ( TFGameRules.Current.AreRespawnRoomsOpen() || !Enabled ) 
			return false;

		var player = TFPlayer.LocalPlayer;
		if ( player == null )
			return false;

		var distSqr = player.Position.DistanceSquared( Position );
		if ( distSqr > FADE_END_DISTANCE_SQR ) return false;

		return !TeamOption.Is( player.Team );
	}
}
