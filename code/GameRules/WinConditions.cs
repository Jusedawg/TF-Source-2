using Sandbox;
using Amper.FPS;
using System;
using System.Linq;


namespace TFS2;

partial class TFGameRules
{
	public void CheckWinConditions()
	{
		if(GameMode.HasWon(out var team, out var reason))
		{
			DeclareWinner(team, reason);
		}
	}

	public bool ControlPointsRoundEndCheck()
	{
		if ( !MapHasControlPoints )
			return false;

		if ( !TeamOwnsAllControlPointsCausesRoundEnd() )
			return false;

		foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
		{
			if ( !team.IsPlayable() ) 
				continue;

			if ( TeamOwnsAllControlPoints( team ) )
			{
				DeclareWinner( team, TFWinReason.AllPointsCaptured );
				return true;
			}
		}

		return false;
	}

	public bool TeamWipeRoundEndCheck()
	{
		if ( !TeamWipeCausesRoundEnd() )
			return false;

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
				var winner = team == TFTeam.Red ? TFTeam.Blue : TFTeam.Red;
				DeclareWinner( winner, TFWinReason.OpponentsDead );

				return true;
			}
		}

		return false;
	}
}
