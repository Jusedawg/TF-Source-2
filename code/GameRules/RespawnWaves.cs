using Sandbox;
using Amper.FPS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TFS2;

partial class TFGameRules
{
	[Net] IDictionary<TFTeam, float> RespawnWaveTimes { get; set; }
	[Net] IDictionary<TFTeam, float> NextRespawnWave { get; set; }

	public void TickRespawnWaves()
	{
		// dont even bother calculating respawn waves if respawns are disabled.
		if ( !AreRespawnsAllowed() )
			return;

		foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
		{
			if ( !team.IsPlayable() ) 
				continue;

			if ( !CanTeamRespawn( team ) )
				continue;

			float nextTime;
			NextRespawnWave.TryGetValue( team, out nextTime );
			if ( nextTime > Time.Now )
				continue;

			RespawnTeam( team );

			float nextRespawnLength = GetRespawnWaveMaxLength( team );
			if ( nextRespawnLength > 0 )
			{
				NextRespawnWave[team] = Time.Now + nextRespawnLength;
			} else
			{
				NextRespawnWave[team] = 0;
			}
		}
	}

	public void RespawnTeam( TFTeam team, bool force = false )
	{
		RespawnPlayers( force, true, (int)team );
	}

	public override bool AreRespawnsAllowed()
	{
		return base.AreRespawnsAllowed() && (!HasGamemode() || GetGamemode()?.Properties.DisablePlayerRespawn == false);
	}
	public override bool AreRespawnConditionsMet( SDKPlayer player )
	{
		var ply = player as TFPlayer;
		if ( ply == null ) return false;

		// can't respawn if they're alraedy alive.
		if ( player.IsAlive ) 
			return false;

		// never respawn if game doesn't allow us to respawn.
		if ( !CanPlayerRespawn( player ) )
			return false;

		// check if we have waited enough time to respawn.
		return Time.Now >= GetMinPlayerRespawnWaitTime( ply );
	}

	/// <summary>
	/// Get minimum time that user needs to wait to be eligible to respawn.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public float GetMinPlayerRespawnWaitTime( TFPlayer player )
	{
		float time = player.DeathAnimationTime + TFPlayer.sv_spectator_freeze_traveltime + TFPlayer.sv_spectator_freeze_time;
		time += GetRespawnWaveMaxLength( player.Team, false );

		return Time.Now + time - player.TimeSinceDeath;
	}

	/// <summary>
	/// Gets the baseline respawn time value for each team.
	/// </summary>
	/// <param name="team"></param>
	/// <returns></returns>
	public float GetRespawnWaveTeamTimeValue( TFTeam team )
	{
		float value;
		RespawnWaveTimes.TryGetValue( team, out value );
		if ( value <= 0 ) value = mp_respawnwavetime;

		return value;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="team"></param>
	/// <param name="scaleWithPlayers"></param>
	/// <returns></returns>
	public float GetRespawnWaveMaxLength( TFTeam team, bool scaleWithPlayers = true )
	{
		if ( State != GameState.Gameplay ) 
			return 0;

		if ( mp_disable_respawn_times ) 
			return 0;

		float time = GetRespawnWaveTeamTimeValue( team );

		// For long respawn times, scale the time as the number of players drops
		if ( scaleWithPlayers && time > 5 )
		{
			time = MathF.Max( 5, time * GetRespawnTimeScalar( team ) );
		}

		return time;
	}

	public bool ShouldRespawnQuickly( TFPlayer player )
	{
		return false;
	}

	public float GetRespawnTimeScalar( TFTeam team )
	{
		int optimalPlayers = 8;
		int numPlayers = team.GetPlayers().Count();

		return ((float)numPlayers).Remap( 1, optimalPlayers, 0.25f, 1 );
	}

	public float GetNextPlayerRespawnWaveTime( TFPlayer player )
	{
		// the next scheduled respawn wave time
		var nextRespawnTime = GetNextTeamRespawnWaveTime( player.Team );

		// The soonest this player may spawn
		float minSpawnTime = GetMinPlayerRespawnWaitTime( player );
		if ( ShouldRespawnQuickly( player ) )
			return minSpawnTime;

		// the length of one respawn wave. We'll check in increments of this
		float flRespawnWaveMaxLen = GetRespawnWaveMaxLength( player.Team );

		if ( flRespawnWaveMaxLen <= 0 )
			return nextRespawnTime;
		
		while ( nextRespawnTime < minSpawnTime )
		{
			nextRespawnTime += flRespawnWaveMaxLen;
		}

		return nextRespawnTime;
	}

	public float GetNextTeamRespawnWaveTime( TFTeam team )
	{
		float nextRespawnWave;
		NextRespawnWave.TryGetValue( team, out nextRespawnWave );
		return nextRespawnWave;
	}

	[ConVar.Replicated] public static bool mp_disable_respawn_times { get; set; }
	[ConVar.Replicated] public static float mp_respawnwavetime { get; set; } = 10;
}
