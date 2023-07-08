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
	static TFStats()
	{
		Current = new();
	}

	public TFStats()
	{
		EventDispatcher.Subscribe<PlayerDeathEvent>( OnPlayerDeath, this );
		EventDispatcher.Subscribe<RoundEndEvent>( OnRoundEnded, this );
	}

	private static void OnPlayerDeath( PlayerDeathEvent ev )
	{
		if ( Game.IsServer ) return;

		if(ev.Victim is TFPlayer vic && ev.Attacker is TFPlayer atk && vic != atk)
		{
			if(vic.AllowTracking() && atk.AllowTracking())
			{
				Stats.Increment( atk.Client, "kills", 1.0 );
				Stats.Increment( vic.Client, "deaths", 1.0 );

				if(ev.Assister is TFPlayer ast && ast.AllowTracking())
				{
					Stats.Increment( ast.Client, "assists", 1.0 );
				}
			}
		}
	}
	private static void OnRoundEnded( RoundEndEvent ev )
	{
		Log.NetInfo( "OnRoundEnded" );
		if ( Game.IsServer ) return;
		Log.Info( 1 );

		var team = (TFTeam)ev.WinningTeam;
		if ( team == TFTeam.Blue )
			Stats.Increment( "wins_blu", 1.0 );
		else if ( team == TFTeam.Red )
			Stats.Increment( "wins_red", 1.0 );
	}
}
