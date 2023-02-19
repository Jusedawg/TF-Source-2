using Sandbox;
using Editor;
using Amper.FPS;
using System;
using System.Linq;

namespace TFS2;

[Library( "tf_logic_arena" )]
[Title( "Arena" )]
[Category("Gamemode")]
[Icon("no_meeting_room")]
[EditorSprite( "materials/editor/tf_logic_arena.vmat" )]
[HammerEntity]

public partial class Arena : GamemodeEntity
{
	[Property, FGDType( "target_destination" )]
	public string PointName { get; set; }
	[Property] public float ControlPointEnableTime { get; set; } = 60;
	ControlPoint Point { get; set; }

	public override bool HasWon( out TFTeam winner, out TFWinReason reason )
	{
		winner = TFTeam.Unassigned;
		reason = TFWinReason.OpponentsDead;

		// don't call All.OfType for every team, call it once and then use it for each team.
		var allPlayers = All.OfType<TFPlayer>();

		// check if any team has no alive players.
		foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
		{
			if ( !team.IsPlayable() )
				continue;

			// If there are no alive players in this team.
			if ( !allPlayers.Where( x => x.Team == team && x.IsAlive ).Any() )
			{
				// get the opposite team
				winner = team == TFTeam.Red ? TFTeam.Blue : TFTeam.Red;

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
	}

	public override void Reset()
	{
		// lock the control point
		if ( Point != null )
		{
			Point.StartLocked = true;
			Point.Lock();
		}
	}

	public override void Tick()
	{
		if ( !TFGameRules.Current.AreObjectivesActive() )
			return;

		if ( CanUnlockPoint() ) 
			Point.Unlock( 5 );
	}

	public bool CanUnlockPoint()
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
		OnArenaRoundActive.Fire( this );
	}

	public override void RoundRestart( RoundRestartEvent args )
	{
		base.RoundRestart( args );
		OnArenaRoundStart.Fire( this );
	}

	protected Output OnArenaRoundStart { get; set; }
	protected Output OnArenaRoundActive { get; set; }
}
