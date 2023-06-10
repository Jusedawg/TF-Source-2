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
public partial class TeleporterEntrance : Teleporter
{
	const string READY_SOUND = "building_teleporter.ready";
	const string SEND_SOUND = "building_teleporter.send";
	const string RECEIVE_SOUND = "building_teleporter.receive";

	const string GENERIC_FX = "particles/teleported_fx/teleported_flash.vpcf";
	const string RED_FX = "particles/teleported_fx/teleportedin_red.vpcf";
	const string BLU_FX = "particles/teleported_fx/teleported_blue.vpcf";
	const string RED_PLAYER_FX = "particles/teleported_fx/teleportedin_red.vpcf";
	const string BLU_PLAYER_FX = "particles/teleported_fx/teleportedin_blue.vpcf";
	[Net] public int AmountTeleported { get; protected set; }

	protected virtual List<float> LevelCooldownTimes => new() { 10f, 5f, 3f };
	public virtual float GetCooldownTime() => LevelCooldownTimes.ElementAtOrDefault( Level - 1 );

	/// <summary>
	/// Max velocity a player is allowed to have to be able to enter this teleporter.
	/// </summary>
	protected virtual float TeleportMaxVelocity => 5f;
	protected virtual float TeleporterWaitTIme => 1f;
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
		base.TickReady();
		TeleportNext();
	}

	public override void Tick()
	{
		if ( IsCarried || !IsInitialized ) return;

		base.Tick();

		if( IsPaired && LinkedTeleporter.IsValid() )
		{
			// Rotation between this and the other teleporter
			var direction = Rotation.LookAt( LinkedTeleporter.Position - Position );
			// Rotation of the teleporter direction arrow. This should be the amount of yaw rotation needed from our initial rotation.
			float arrowRotation = MathF.Abs(direction.Yaw())- MathF.Abs(Rotation.Yaw());
			if ( arrowRotation < 0 )
				arrowRotation += 360;
			SetAnimParameter( "f_direction", arrowRotation );

			SetBodyGroup( "teleporter_direction", 1 );
		}
		else
			SetBodyGroup( "teleporter_direction", 0 );
	}

	protected virtual void TeleportNext()
	{
		if(currentTarget != null && timeUntilTeleport )
		{
			DoTeleport( currentTarget );
			currentTarget = null;
		}
		else if ( currentTarget == null && teleporterQueue.Any() )
		{
			currentTarget = teleporterQueue[0];
			teleporterQueue.RemoveAt( 0 );

			timeUntilTeleport = TeleporterWaitTIme;
		}
	}

	const float PLAYER_EXTRA_CLEARANCE = 8;
	protected virtual void DoTeleport(TFPlayer ply)
	{
		ply.Position = LinkedTeleporter.Position + Vector3.Up * (LinkedTeleporter.CollisionBounds.Maxs.y + PLAYER_EXTRA_CLEARANCE);
		ply.Rotation = LinkedTeleporter.Rotation;
		UnReady( GetCooldownTime() );
		TeleportEffects( ply );

		AmountTeleported++;
	}

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

		if ( ply.Velocity.x + ply.Velocity.y > TeleportMaxVelocity )
		{
			// Remove invalid players
			if ( teleporterQueue.Contains( ply ) )
				teleporterQueue.Remove( ply );

			return;
		}

		// Dont add players twice
		if ( !teleporterQueue.Contains( ply ) )
			teleporterQueue.Add( ply );
	}

	public override void EndTouch( Entity other )
	{
		if ( other is TFPlayer ply && teleporterQueue.Contains( ply ) )
			teleporterQueue.Remove( ply );
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
	protected override string GetLevelParticle()
	{
		if ( Team == TFTeam.Blue )
		{
			return Level switch
			{
				1 => "particles/teleport_status/teleporter_blue_entrance_level1.vpcf",
				2 => "particles/teleport_status/teleporter_blue_entrance_level2.vpcf",
				_ => "particles/teleport_status/teleporter_blue_entrance_level3.vpcf"
			};
		}
		else
		{
			return Level switch
			{
				1 => "particles/teleport_status/teleporter_red_entrance_level1.vpcf",
				2 => "particles/teleport_status/teleporter_red_entrance_level2.vpcf",
				_ => "particles/teleport_status/teleporter_red_entrance_level3.vpcf"
			};
		}
	}
	protected BuildingInfoLine TeleporterStatsProgress;
	protected override void InitializeUI( BuildingData data )
	{
		base.InitializeUI( data );
		TeleporterStatsProgress = new( "0", "/UI/HUD/Buildings/hud_obj_status_teleport_64.png" );
	}

	public override void TickUI()
	{
		if ( !IsInitialized ) return;
		base.TickUI();

		if ( IsReady || IsConstructing || !IsPaired )
		{
			TeleporterStatsProgress.Text = $"{AmountTeleported}";
			TeleporterStatsProgress.ShowText = true;
		}
		else
		{
			TeleporterStatsProgress.Value = ReadyProgress;
			TeleporterStatsProgress.MaxValue = ReadyTime;
			TeleporterStatsProgress.ShowText = false;
		}
	}

	public override IEnumerable<BuildingInfoLine> GetUILines()
	{
		yield return TeleporterStatsProgress;
		yield return UpgradeMetalLine;
	}

	protected override void Debug()
	{
		base.Debug();

		DebugOverlay.Box( teleportZone, Color.Yellow.Darken( 0.2f ) );
		Vector3 pos = Position + Vector3.Up * CollisionBounds.Maxs.z;
		DebugOverlay.Text( $"[TELEPORTER ENTRANCE]", pos, 19, Color.White );
		DebugOverlay.Text( $"= Current Target: {currentTarget}", pos, 20, Color.Yellow );
		DebugOverlay.Text( $"= Time Until Teleport: {timeUntilTeleport}", pos, 21, Color.Yellow );
		DebugOverlay.Text( $"= Teleporter Queue: {teleporterQueue.Count}", pos, 22, Color.Yellow );
		DebugOverlay.Text( $"= Cooldown Time: {GetCooldownTime()}", pos, 23, Color.Yellow );

	}
}
