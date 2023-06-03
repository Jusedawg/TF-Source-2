using Amper.FPS;
using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

[Library("tf_building_teleporter_entrance")]
public class TeleporterEntrance : Teleporter
{
	const string READY_SOUND = "building_teleporter.ready";
	const string SEND_SOUND = "building_teleporter.send";
	const string RECEIVE_SOUND = "building_teleporter.receive";
	protected virtual List<float> LevelCooldownTimes => new() { 10f, 5f, 3f };
	public virtual float GetCooldownTime() => LevelCooldownTimes.ElementAtOrDefault( Level - 1 );

	/// <summary>
	/// Max velocity a player is allowed to have to be able to enter this teleporter.
	/// </summary>
	protected virtual float TeleportMaxVelocity => 5f;
	protected virtual float TeleporterFadeInTime => 0.25f;
	protected virtual float TeleporterFadeOutTime => 0.25f; // TODO: Implement

	protected List<TFPlayer> teleporterQueue = new();
	protected TFPlayer currentTarget;
	protected TimeUntil timeUntilTeleport;
	protected ModelEntity teleportZone;
	
	public override void Initialize(BuildingData data)
	{
		if ( Game.IsClient ) return;

		base.Initialize( data );

		CreateZone();
	}
	public override void InitializeModel( string name )
	{
		base.InitializeModel( name );
		EnableTouchPersists = true;
	}
	protected virtual void CreateZone()
	{
		var offset = Vector3.Up * Data.Maxs.z;
		teleportZone = new();
		teleportZone.Tags.Add( CollisionTags.Trigger );
		teleportZone.SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, Data.Mins + offset, Data.Maxs + offset );
		teleportZone.EnableAllCollisions = false;
		teleportZone.EnableTouch = true;
		teleportZone.EnableTouchPersists = true;
		teleportZone.SetParent( this );
		teleportZone.Transmit = TransmitType.Never;
	}
	public override void TickReady()
	{
		SetBodyGroup( "direction", IsPaired ? 1 : 0 );

		TeleportNext();
	}

	protected virtual void TeleportNext()
	{
		if(currentTarget != null && timeUntilTeleport )
		{
			DoTeleport( currentTarget );
			currentTarget = null;
		}
		else if ( teleporterQueue.Any() )
		{
			currentTarget = teleporterQueue[0];
			teleporterQueue.RemoveAt( 0 );

			timeUntilTeleport = TeleporterFadeInTime;
		}
	}

	const float PLAYER_EXTRA_CLEARANCE = 8;
	protected virtual void DoTeleport(TFPlayer ply)
	{
		ply.Position = LinkedTeleporter.Position + Vector3.Up * (LinkedTeleporter.CollisionBounds.Maxs.y + PLAYER_EXTRA_CLEARANCE);
		ply.Rotation = LinkedTeleporter.Rotation;
		UnReady( GetCooldownTime() );
		TeleportEffects( ply );
	}

	const string GENERIC_FX = "particles/teleported_fx/teleported_flash.vpcf";
	const string RED_FX = "particles/teleported_fx/teleportedin_red.vpcf";
	const string BLU_FX = "particles/teleported_fx/teleported_blue.vpcf";
	const string RED_PLAYER_FX = "particles/teleported_fx/teleportedin_red.vpcf";
	const string BLU_PLAYER_FX = "particles/teleported_fx/teleportedin_blue.vpcf";
	protected virtual void TeleportEffects(TFPlayer ply)
	{
		if(Team == TFTeam.Red)
		{
			Particles.Create( RED_FX, this );
			Particles.Create( RED_PLAYER_FX, ply );
		}
		else
		{
			Particles.Create( BLU_FX, this );
			Particles.Create( BLU_PLAYER_FX, ply );
		}

		Particles.Create( GENERIC_FX, this );

		Sound.FromEntity( SEND_SOUND, this );
		Sound.FromEntity( RECEIVE_SOUND, LinkedTeleporter );

		// TODO: Screen Fade
		// TODO: Temp FOV Increase
	}
	
	public override void Touch( Entity other )
	{
		if ( other is not TFPlayer ply ) return;
		// TODO: Check for disguise
		if ( ply.Team != Team ) return;

		if ( ply.Velocity.WithZ(0).Length > TeleportMaxVelocity )
		{
			// Dont add a player twice
			if ( teleporterQueue.Contains( ply ) )
				teleporterQueue.Remove( ply );

			return;
		}

		if ( !teleporterQueue.Contains( ply ) )
			teleporterQueue.Add( ply );
	}

	public override void EndTouch( Entity other )
	{
		if ( other is TFPlayer ply && teleporterQueue.Contains( ply ) )
			teleporterQueue.Remove( ply );
	}
	public override void Link( Teleporter tp, bool linkOther = true )
	{
		base.Link( tp, linkOther );

		// Rotation between this and the other teleporter
		var direction = Rotation.LookAt( tp.Position - Position );
		// Rotation of the teleporter direction arrow. This should be the amount of yaw rotation needed from our initial rotation.
		float arrowRotation = direction.Yaw() - Rotation.Yaw();
		if ( arrowRotation < 0 )
			arrowRotation += 360;
		SetAnimParameter( "f_direction", arrowRotation );
	}

	public override void FinishUpgrade()
	{
		base.FinishUpgrade();

		Sound.FromEntity( READY_SOUND, this );
	}

	public override void FinishConstruction()
	{
		base.FinishConstruction();

		Sound.FromEntity( READY_SOUND, this );
	}

	protected override void Debug()
	{
		base.Debug();

		DebugOverlay.Box( teleportZone, Color.Yellow.Darken( 0.2f ) );
		Vector3 pos = Position + Vector3.Up * CollisionBounds.Maxs.z;
		DebugOverlay.Text( $"[TELEPORTER ENTRANCE]", pos, 19, Color.White );
		DebugOverlay.Text( $"= Teleporter Queue: {teleporterQueue.Count}", pos, 20, Color.Yellow );
		DebugOverlay.Text( $"= Cooldown Time: {GetCooldownTime()}", pos, 21, Color.Yellow );
	}
}
