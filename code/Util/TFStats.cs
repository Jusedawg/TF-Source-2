using Amper.FPS;
using Sandbox;
using Sandbox.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public class TFStats
{
	public static TFStats Current { get; set; }
	private TimeSince timeSinceLocalClassChange;
	public TFStats()
	{
		if(Current != null)
		{
			throw new InvalidOperationException( "There can only be one TFStats instance!" );
		}
		Current = this;

		EventDispatcher.Subscribe<PlayerChangeClassEvent>( OnPlayerChangeClass, this );
		EventDispatcher.Subscribe<PlayerDeathEvent>( OnPlayerDeath, this );
		EventDispatcher.Subscribe<RoundEndEvent>( OnRoundEnded, this );
	}

	private void OnPlayerDeath( PlayerDeathEvent ev )
	{
		if ( Game.IsServer ) return;
		var pawn = Game.LocalPawn;

		if(ev.Attacker == pawn )
		{
			Stats.Increment( "kills", 1.0 );
		}
		else if(ev.Victim == pawn )
		{
			Stats.Increment( "deaths", 1.0 );
		}
		else if (ev.Assister == pawn )
		{
			Stats.Increment( "assists", 1.0 );
		}

		Stats.Flush();
	}

	private void OnRoundEnded( RoundEndEvent ev )
	{
		if ( Game.IsClient ) return;

		/*
		var team = (TFTeam)ev.WinningTeam;
		if ( team == TFTeam.Blue )
			Stats.Increment( "wins_blu", 1.0 );
		else if ( team == TFTeam.Red )
			Stats.Increment( "wins_red", 1.0 );
		*/

		foreach(var cl in Game.Clients)
		{
			Stats.Increment( cl, "points_average", 1.0 );
		}

		Stats.Flush();
	}
	private void OnPlayerChangeClass( PlayerChangeClassEvent ev )
	{
		if ( Game.IsServer || ev.Client != Game.LocalClient ) return;

		string stat = GetClassTimeStat( ev.PreviousClass );
		if(!string.IsNullOrEmpty(stat))
		{
			Stats.Increment( stat, timeSinceLocalClassChange );
			Stats.Flush();
		}
		timeSinceLocalClassChange = 0;
	}

	private static string GetClassTimeStat(PlayerClass playerClass)
	{
		if ( playerClass == default ||playerClass.Entry == TFPlayerClass.Undefined ) return "";
		return $"playtime_{playerClass.ResourceName.ToLower()}";
	}
}
