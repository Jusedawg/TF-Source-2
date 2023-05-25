using Sandbox;
using System;
using System.Linq;

namespace TFS2;

partial class TFGameRules
{
	public IGamemode GetGamemode()
	{
		if(EntityGamemode != null)
		{
			return EntityGamemode;
		}

		if ( ClassGamemode != null )
		{
			return ClassGamemode;
		}

		return default;
	}
	[Net] private GamemodeEntity EntityGamemode { get; set; }
	[Net] private GamemodeNetworkable ClassGamemode { get; set; }

	public bool HasGamemode() => GetGamemode() != default;
	public bool IsPlaying<T>() where T : IGamemode => GetGamemode()?.GetType() == typeof(T); // Instead of is T to avoid subclasses triggering this, might want to reconsider this later
	public bool TryGetGamemode<T>(out T instance) where T : IGamemode
	{
		if( GetGamemode() is T mode)
		{
			instance = mode;
			return true;
		}

		instance = default;
		return false;
	}

	public override void ResetObjectives()
	{
		// reset all resettable ents
		foreach ( var ent in All.OfType<IResettable>() ) ent.Reset();
	}

	public override void CalculateObjectives()
	{
		// This function helps define what game type are we currently playing.

		FindGamemode();

		if( GetGamemode() == default )
		{
			Log.Info( "No gamemode found for this map, running without gamemode..." );
			return;
		}

		Log.Info( $"We're playing: {GetGamemode().Title}" );
	}

	/// <summary>
	/// Checks all gamemodes if they would be playable on the current map.
	/// </summary>
	/// <returns></returns>
	public virtual void FindGamemode()
	{
		// Check entities first
		foreach ( var mode in Entity.All.OfType<GamemodeEntity>() )
		{
			if ( mode.IsActive() )
			{
				EntityGamemode = mode;
				return;
			}
		}

		// Check non-entity gamemodes after
		var gamemodes = TypeLibrary.GetTypes<GamemodeNetworkable>().Where(g => !g.IsAbstract);

		// Skip gamemodes which require entities to spawn
		foreach ( var mode in gamemodes.Select( g => TypeLibrary.Create<IGamemode>( g.TargetType ) ) )
		{
			if ( mode.IsActive() )
			{
				ClassGamemode = (GamemodeNetworkable)mode;
				return;
			}
		}
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

	public void CheckWinConditions()
	{
		var mode = GetGamemode();
		if ( mode == default )
			return;

		if ( mode.HasWon( out var team, out var reason ) )
		{
			DeclareWinner( team, reason );
		}
	}

	public void DeclareWinner( TFTeam team, TFWinReason reason )
	{
		DeclareWinner( (int)team, (int)reason );
	}

}
