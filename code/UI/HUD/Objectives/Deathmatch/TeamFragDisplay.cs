using Sandbox;
using Sandbox.UI;
using System;

namespace TFS2.UI;

public partial class TeamFragDisplay : Panel
{
	Label LimitLabel { get; set; }
	Panel RedBar { get; set; }
	Label RedScore { get; set; }
	Panel BlueBar { get; set; }
	Label BlueScore { get; set; }
	public bool ShouldDraw()
	{
		return TFGameRules.Current.GameType == TFGameType.TeamDeathmatch;
	}

	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );
		if ( !IsVisible ) return;

		var tdmLogic = TFGameRules.Current.TeamDeathmatchLogic;
		if ( tdmLogic == null )
			return;

		var limit = tdmLogic.FragLimit;
		LimitLabel.Text = $"{limit}";

		float redCount = tdmLogic.GetTeamFragCount( TFTeam.Red );
		float blueCount = tdmLogic.GetTeamFragCount( TFTeam.Blue );

		var redFraction = 0f;
		var blueFraction = 0f;

		if ( limit > 0 )
		{
			redFraction = Math.Clamp( redCount / limit, 0, 1 );
			blueFraction = Math.Clamp( blueCount / limit, 0, 1 );
		}

		RedBar.Style.Height = Length.Fraction( redFraction );
		RedScore.Text = $"{redCount}";

		BlueBar.Style.Height = Length.Fraction( blueFraction );
		BlueScore.Text = $"{blueCount}";

		if ( tdmLogic.HasReachedFragLimit() )
		{
			int seconds = tdmLogic.GetTimeUntilRoundEnd().FloorToInt();
			if ( lastBeepTime != seconds )
			{
				float pitch = 1f + (4 - seconds) * 0.2f;
				if ( seconds == 0 )
					Sound.FromScreen( "tdm.finale_beep.last" );
				else
					Sound.FromScreen( "tdm.finale_beep" ).SetRandomPitch( pitch, pitch );

				Alert.Show( $"{tdmLogic.FirstScorer.GetTitle()} wins in {seconds}s", "/ui/icons/ico_flag_moving.png", 2 );
				lastBeepTime = seconds;
			}
		}
	}

	int lastBeepTime = 0;
}
