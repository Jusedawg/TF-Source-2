using Sandbox;
using System;
using System.Linq;

namespace TFS2;

partial class TFGameRules
{
	[Net] public IGamemode GameMode { get; set; }

	public bool IsPlaying<T>() where T : IGamemode => GameMode is T;

	public override void ResetObjectives()
	{
		// reset all objectives ents
		foreach ( var flag in All.OfType<Flag>() ) flag.Reset();
		foreach ( var point in ControlPoint.All ) point.Reset();
		foreach ( var cart in All.OfType<Cart>() ) cart.Reset();
	}

	public override void CalculateObjectives()
	{
		// This function helps define what game type are we currently playing.

		GameMode = FindGamemode();

		if( GameMode == null )
		{
			Log.Info( "No gamemode found for this map, running without gamemode..." );
			return;
		}

		Log.Info( $"We're playing: {GameMode.Title}" );
	}

	/// <summary>
	/// Checks all gamemodes if they would be playable on the current map.
	/// </summary>
	/// <returns></returns>
	public virtual IGamemode FindGamemode()
	{
		// Check entities first
		foreach ( var mode in Entity.All.OfType<IGamemode>() )
		{
			if ( mode.IsActive() )
			{
				return mode;
			}
		}

		// Check non-entity gamemodes after
		var gamemodes = TypeLibrary.GetTypes<IGamemode>();
		foreach ( var mode in gamemodes.Select( g => TypeLibrary.Create<IGamemode>( g.TargetType ) ) )
		{
			if ( mode.IsActive() )
			{
				return mode;
			}
		}

		return null;
	}

	public bool AreObjectivesActive()
	{
		// If round is not active, objectives can't be interacted with.
		if ( !IsRoundActive )
			return false;

		// if we're waiting for players, we can't cap.
		if ( IsWaitingForPlayers )
			return false;

		return true;
	}

	public override void SimulateGameplay()
	{
		base.SimulateGameplay();

		if ( !Game.IsServer )
			return;

		CheckWinConditions();
	}

	public void DeclareWinner( TFTeam team, TFWinReason reason )
	{
		DeclareWinner( (int)team, (int)reason );
	}

}
