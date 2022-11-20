using Sandbox;
using System;
using System.Collections.Generic;
using Amper.FPS;

namespace TFS2;

/// <summary>
/// Timer entity, used to count down the time for the purposes of a gamemode.
/// </summary>
public partial class TFTimer : Timer
{
	[Net] public TFTeam OwnerTeam { get; set; }

	int lastSecond = -1;

	[Event.Tick.Server]
	void Tick()
	{
		if ( Paused )
			return;

		float timeLeft = GetRemainingTime();
		int second = timeLeft.FloorToInt();

		if ( second != lastSecond )
		{
			//
			// Outputs
			//
			switch ( second )
			{
				case 300: On5MinRemain.Fire( this ); break;
				case 240: On4MinRemain.Fire( this ); break;
				case 180: On3MinRemain.Fire( this ); break;
				case 120: On2MinRemain.Fire( this ); break;
				case 60: On1MinRemain.Fire( this ); break;
				case 30: On30SecRemain.Fire( this ); break;
				case 10: On10SecRemain.Fire( this ); break;
				case 5: On5SecRemain.Fire( this ); break;
				case 4: On4SecRemain.Fire( this ); break;
				case 3: On3SecRemain.Fire( this ); break;
				case 2: On2SecRemain.Fire( this ); break;
				case 1: On1SecRemain.Fire( this ); break;
			}

			lastSecond = second;
		}

		if ( timeLeft == 0 )
		{
			Pause();
			OnFinished.Fire( this );
		}
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
