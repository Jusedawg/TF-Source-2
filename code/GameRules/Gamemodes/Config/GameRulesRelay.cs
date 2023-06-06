using Amper.FPS;
using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

[Library("tf_gamerules")]
[Title("Gamerules")]
[Description("Allows modifiying gamerules and listening for events inside of a map.")]
[Icon("gavel")]
[Category( "Configuration" )]
[HammerEntity]
public partial class TFGameRulesRelay : Entity
{
	public static TFGameRulesRelay Instance { get; set; }
	[Property] public bool SwitchTeams { get; set; } = false;
	public TFGameRulesRelay()
	{
		if ( Instance != null ) return;
		Instance = this;

		EventDispatcher.Subscribe<RoundActiveEvent>( OnRoundStartEvent, this );
		EventDispatcher.Subscribe<RoundRestartEvent>( OnRoundRestartEvent, this );
	}

	private void OnRoundStartEvent( RoundActiveEvent ev )
	{
		OnRoundStart.Fire(this);
	}

	private void OnRoundRestartEvent( RoundRestartEvent ev )
	{
		OnRoundRestart.Fire(this);
	}

	public Output OnRoundStart { get; set; }
	public Output OnRoundRestart { get; set; }
}
