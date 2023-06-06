using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amper.FPS;
using Editor;
using Sandbox;

namespace TFS2;

[Library("tf_round_win")]
[Title("Round Win")]
[Category( "Configuration" )]
[Icon("emoji_events")]
[HammerEntity]
public class RoundWinRelay : Entity
{
	[Property] public TFWinReason Reason { get; set; } = TFWinReason.None;
	[Property] public HammerTFTeamOption Team { get; set; } = HammerTFTeamOption.Any;

	[Input("WinRound")]
	private void DoRoundWin()
	{
		TFGameRules.Current.DeclareWinner( Team.ToTFTeam(), Reason );
	}

	[Input]
	private void SetTeam(HammerTFTeamOption team)
	{
		Team = team;
	}
}
