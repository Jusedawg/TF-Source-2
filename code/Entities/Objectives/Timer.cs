using Sandbox;
using System;
using System.Collections.Generic;
using Amper.FPS;
using Editor;

namespace TFS2;

/// <summary>
/// Timer entity, used to count down the time for the purposes of a gamemode.
/// </summary>
[Library("tf_timer")]
[Title("Round Timer")]
[Description("Timer which ticks down is shown on the HUD")]
[Icon("timer")]
[HammerEntity]
public partial class TFTimer : Timer
{
	[Property] public bool PlayAnnouncerVoicelines { get; set; } = true;
	[Net] public TFTeam OwnerTeam { get; set; }
	protected override void Tick()
	{
		base.Tick();

		if ( !PlayAnnouncerVoicelines ) return;
		int secondsRemaining = GetRemainingTime().FloorToInt();
		PlayAnnouncerTimeVoiceLine( secondsRemaining );
	}

	public void PlayAnnouncerTimeVoiceLine( int second )
	{
		var time = "";
		switch ( second )
		{
			case 300: time = "5min"; break;
			case 60: time = "1min"; break;
			case 30: time = "30sec"; break;
			case 10: time = "10sec"; break;
			case 5: time = "5sec"; break;
			case 4: time = "4sec"; break;
			case 3: time = "3sec"; break;
			case 2: time = "2sec"; break;
			case 1: time = "1sec"; break;
		}

		// no time value
		if ( string.IsNullOrEmpty( time ) ) return;

		//
		// Compute announcer voice line
		//
		var begins = false;
		var sound = "announcer.";

		// beings or ends
		if ( begins ) sound += "begins.";
		else sound += "ends.";

		sound += time;

		SDKGame.PlaySoundToAll( sound, SoundBroadcastChannel.Announcer );
	}
}
