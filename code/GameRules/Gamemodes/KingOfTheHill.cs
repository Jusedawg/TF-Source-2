using Sandbox;
using Editor;
using Amper.FPS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TFS2;

[Library( "tf_logic_koth" )]
[Title( "King of the Hill" )]
[Category( "Gamemode" )]
[Icon( "terrain" )]
[EditorSprite( "materials/editor/tf_logic_koth.vmat" )]
[HammerEntity]

public partial class KingOfTheHill : MapGamemode
{
	public override string Title => "King of the Hill";
	public override string Icon => "ui/hud/scoreboard/icon_mode_koth.png";
	[Property] public float TimerLength { get; set; } = 180;
	[Property] public float ControlPointEnableTime { get; set; } = 30;
	[Property, FGDType( "target_destination" )] public string PointName { get; set; }
	protected ControlPoint Point { get; set; }

	Dictionary<TFTeam, RoundTimer> Timers { get; set; } = new();

	public KingOfTheHill()
	{
		EventDispatcher.Subscribe<ControlPointCapturedEvent>( OnPointCapture, this );
	}

	public override bool HasWon( out TFTeam team, out TFWinReason reason )
	{
		team = TFTeam.Unassigned;
		reason = TFWinReason.AllPointsCaptured;

		foreach ( var (key, timer) in Timers )
		{
			team = key;

			if ( timer.GetRemainingTime() == 0 )
			{
				return true;
			}
		}

		return false;
	}

	public override void PostLevelSetup()
	{
		// try to find our previous points
		Point = FindByName( PointName ) as ControlPoint;
		Reset();
		Point.AddOutputEvent( "OnUnlocked", OnPointUnlocked );
	}

	const string POINT_UNLOCK_SOUND = "announcer.point.enabled";
	private ValueTask OnPointUnlocked( Entity activator, float delay )
	{
		if(TFGameRules.Current.AreObjectivesActive())
		{
			SDKGame.PlaySoundToAll( POINT_UNLOCK_SOUND, SoundBroadcastChannel.Announcer );
		}

		return ValueTask.CompletedTask;
	}

	public override void Reset()
	{
		// lock the control point
		if ( Point != null )
		{
			Point.StartLocked = true;
			Point.Lock();
		}

		//
		// Timers
		//

		foreach ( var timer in Timers.Values )
			timer.Delete();

		Timers.Clear();
	}

	public override void Tick()
	{
		if ( CanUnlockPoint() )
			Point.Unlock( 5 );
	}

	public virtual bool CanUnlockPoint()
	{
		if ( Point == null )
			return false;

		// Point is already unlocked
		if ( !Point.Locked )
			return false;

		// Point is already being unlocked.
		if ( Point.IsBeingUnlocked )
			return false;

		// objectives are disabled.
		if ( !TFGameRules.Current.AreObjectivesActive() )
			return false;

		// there should be at least 1 second delay after round is active.
		float actualEnableTime = MathF.Max( 1, ControlPointEnableTime - 5 );
		return TFGameRules.Current.TimeSinceStateChange > actualEnableTime;
	}

	public override void RoundActivate( RoundActiveEvent args )
	{
		base.RoundActivate( args );

		if ( !Game.IsServer )
			return;

		foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
		{
			if ( !team.IsPlayable() )
				continue;

			Timers[team] = new()
			{
				Name = $"@{team.GetName()}_koth_timer",
				AbsoluteTime = TimerLength,
				Paused = true,
				OwnerTeam = team
			};
		}
	}

	public virtual void OnPointCapture( ControlPointCapturedEvent args )
	{
		var point = args.Point;
		if ( Point != point )
			return;

		SetTeamTimerActive( args.NewTeam );
	}

	public virtual void SetTeamTimerActive( TFTeam team )
	{
		foreach ( var ( timerTeam, timer ) in Timers )
		{
			if ( timerTeam == team ) timer.Start();
			else timer.Pause();
		}
	}
}
