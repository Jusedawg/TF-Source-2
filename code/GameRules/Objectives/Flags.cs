using Sandbox;
using System;
using System.Collections.Generic;
using Amper.FPS;


namespace TFS2;

partial class TFGameRules
{
	[Net] public IDictionary<TFTeam, int> FlagCaptures { get; set; }
	[Net] public bool MapHasFlags { get; set; }

	public bool FlagsCanBePickedUp()
	{
		return AreObjectivesActive();
	}

	public bool FlagsCanBeCapped()
	{
		return AreObjectivesActive();
	}

	public bool CanFlagBeCaptured( Flag flag )
	{
		return true;
	}

	public void FlagReturned( Flag flag )
	{
		SendHUDAlertToTeam( flag.Team, "Your INTELLIGENCE was RETURNED!", "/ui/icons/ico_flag_home.png", 5, flag.Team );
		PlaySoundToTeam( flag.Team, "announcer.intel.teamreturned", SoundBroadcastChannel.Announcer );

		foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
		{
			if ( !team.IsPlayable() ) continue;
			if ( team == flag.Team ) continue;

			SendHUDAlertToTeam( team, "The ENEMY INTELLIGENCE was RETURNED!", "/ui/icons/ico_flag_home.png", 5, flag.Team );
			PlaySoundToTeam( team, "announcer.intel.enemyreturned", SoundBroadcastChannel.Announcer );
		}
	}

	public void FlagPickedUp( Flag flag, TFPlayer picker )
	{
		SendHUDAlertToTeam( picker.Team, "Your team has PICKED the enemy INTELLIGENCE!", "/ui/icons/ico_flag_moving.png", 5, flag.Team );
		PlaySoundToTeam( picker.Team, "announcer.intel.teamstolen", SoundBroadcastChannel.Announcer );

		SendHUDAlertToTeam( flag.Team, "Your INTELLIGENCE has been PICKED UP!", "/ui/icons/ico_flag_moving.png", 5, flag.Team );
		PlaySoundToTeam( flag.Team, "announcer.intel.enemystolen", SoundBroadcastChannel.Announcer );
	}

	public void FlagDropped( Flag flag, TFPlayer dropper )
	{
		SendHUDAlertToTeam( dropper.Team, "Your team DROPPED the enemy INTELLIGENCE!", "/ui/icons/ico_flag_dropped.png", 5, flag.Team );
		PlaySoundToTeam( dropper.Team, "announcer.intel.teamdropped", SoundBroadcastChannel.Announcer );

		SendHUDAlertToTeam( flag.Team, "Your INTELLIGENCE has been DROPPED!", "/ui/icons/ico_flag_dropped.png", 5, flag.Team );
		PlaySoundToTeam( flag.Team, "announcer.intel.enemydropped", SoundBroadcastChannel.Announcer );
	}

	public void FlagCaptured( Flag flag, TFPlayer capper, FlagCaptureZone zone )
	{
		var team = capper.Team;

		SendHUDAlertToTeam( capper.Team, "Your team CAPTURED the ENEMY INTELLIGENCE!", "/ui/icons/ico_flag_home.png", 5, flag.Team );
		PlaySoundToTeam( capper.Team, "announcer.intel.teamcaptured", SoundBroadcastChannel.Announcer );

		SendHUDAlertToTeam( flag.Team, "Your INTELLIGENCE was CAPTURED!", "/ui/icons/ico_flag_home.png", 5, flag.Team );
		PlaySoundToTeam( flag.Team, "announcer.intel.enemycaptured", SoundBroadcastChannel.Announcer );

		if ( !FlagCaptures.ContainsKey( team ) ) FlagCaptures[team] = 0;
		FlagCaptures[team]++;
	}

	[ConVar.Replicated] public static int tf_flag_caps_per_round { get; set; } = 3;
}
