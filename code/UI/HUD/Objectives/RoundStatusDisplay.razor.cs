using Sandbox;
using Sandbox.UI;
using Amper.FPS;
using System.Collections.Generic;
using System.Linq;
using System;
using Sandbox.UI.Construct;

namespace TFS2.UI;

public partial class RoundStatusDisplay : Panel
{
	private const float TIME_UPDATE_DURATION = 2.5f;
	private readonly Queue<(TimeSince time, Label panel)> timeUpdates = new();
	Dictionary<RoundTimer, RoundStatusTimerEntry> Timers { get; set; } = new();
	Label GameStateLabel { get; set; }
	Panel TimersContainer { get; set; }
	public RoundStatusDisplay()
	{
		EventDispatcher.Subscribe<TimeAddedEvent>( OnTimeAdded, this );
	}

	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );
		if ( !IsVisible )
			return;

		UpdateGameStateLabel();
		UpdateTimers();
	}

	public bool ShouldDraw()
	{
		return !Input.Down( "Score" );
	}

	const string WAITING_FOR_PLAYERS_TEXT = "#GameState.WaitingForPlayers";
	const string SETUP_TEXT = "#GameState.Setup";
	const string OVERTIME_TEXT = "Overtime";
	public void UpdateGameStateLabel()
	{
		string value = "";

		// Show a message for waiting for players, unless we play arena (it has it's own message)
		if ( SDKGame.Current.IsWaitingForPlayers && !TFGameRules.Current.IsPlaying<Arena>() )
			value = WAITING_FOR_PLAYERS_TEXT;
		else if ( TFGameRules.Current.IsInSetup )
			value = SETUP_TEXT;
		else if ( RoundTimer.AnyInOvertime )
			value = OVERTIME_TEXT;

		GameStateLabel.Text = value;
	}

	public void UpdateTimers()
	{
		var timers = RoundTimer.All.Where( x => x.IsVisibleOnHUD );
		var keys = Timers.Keys;

		foreach ( var entry in timers.Except( keys ) ) AddTimer( entry );
		foreach ( var entry in keys.Except( timers ) ) RemoveTimer( entry );
		
		while ( timeUpdates.TryPeek( out var update ) && update.time >= TIME_UPDATE_DURATION )
		{
			update.panel.Delete();
			timeUpdates.Dequeue();
		}
	}

	public void AddTimer( RoundTimer timer )
	{
		Timers[timer] = new RoundStatusTimerEntry
		{
			Timer = timer,
			Parent = TimersContainer
		};
		ReorderTimers();
	}

	public void RemoveTimer( RoundTimer timer )
	{
		if ( Timers.TryGetValue( timer, out var entry ) )
		{
			entry?.Delete();
			Timers.Remove( timer );
			ReorderTimers();
		}
	}
	public void ReorderTimers()
	{
		TimersContainer.SortChildren( ( x, y ) =>
		{
			if ( x is not RoundStatusTimerEntry entry1 || y is not RoundStatusTimerEntry entry2 )
				return 0;

			var timer1 = entry1.Timer;
			var timer2 = entry2.Timer;
			if ( timer1 == null || timer2 == null )
				return 0;

			// Sort in DESCENDING order.
			return timer1.OwnerTeam < timer2.OwnerTeam ? 1 : -1;
		} );
	}
	private void OnTimeAdded( TimeAddedEvent ev )
	{
		var label = TimersContainer.Add.Label( $"+{RoundTimer.GetTimeString( ev.TimeAdded )}" );

		label.SetClass( "time_update", true );
		label.Style.AnimationIterationCount = 1;
		label.Style.AnimationDuration = TIME_UPDATE_DURATION;

		timeUpdates.Enqueue( ((TimeSince)0, label) );
	}
}

class RoundStatusTimerEntry : Label
{
	public RoundTimer Timer { get; set; }
	public override void Tick()
	{
		if ( Timer == null )
			return;

		Text = Timer.GetTimeString();
		SetClass( "active", !Timer.Paused );
		if(SDKGame.Current.IsWaitingForPlayers)
			SetClass( "visible", Timer.Name == TFGameRules.WAITING_FOR_PLAYERS_TIMER_NAME ); // too bad!
		else
			SetClass( "visible", TFGameRules.Current.State >= GameState.PreRound );
	}
}
