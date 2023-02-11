using Sandbox;
using Amper.FPS;
using System.Collections.Generic;
using System.Linq;

namespace TFS2;

partial class TFGameRules
{
	[Net] public IDictionary<TFTeam, TFTeamRole> TeamRole { get; set; }
	[Net] public IDictionary<TFTeam, string> TeamGoal { get; set; }

	public override void DeclareGameTeams()
	{
		base.DeclareGameTeams();

		TeamManager.DeclareTeam( (int)TFTeam.Red, "red", "RED", new Color( 0xB8383B ) );
		TeamManager.DeclareTeam( (int)TFTeam.Blue, "blue", "BLU", new Color( 0x5885A2 ) );
	}
	public override void OnTeamLose( int team )
	{
		base.OnTeamLose( team );

		TFTeam loser = (TFTeam)team;
		foreach(var ply in loser.GetPlayers())
		{
			ply.AddCondition( TFCondition.Humiliated );
		}
	}

	public override void PlayTeamWinSong( int team )
	{
		PlaySoundToTeam( (TFTeam)team, "announcer.your_team.won", SoundBroadcastChannel.Announcer );
	}

	public override void PlayTeamLoseSong( int team )
	{
		PlaySoundToTeam( (TFTeam)team, "announcer.your_team.lost", SoundBroadcastChannel.Announcer );
	}
}

public enum TFTeam 
{
	Unassigned,
	Spectator,

	Red,
	Blue
}

public enum HammerTFTeamOption
{
	Any,
	Red,
	Blue
}

public static class TFTeamExtensions
{
	public static TeamManager.TeamProperties GetProperties( this TFTeam team ) => TeamManager.GetProperties( (int)team );
	public static string GetTag( this TFTeam team ) => TeamManager.GetTag( (int)team );
	public static string GetName( this TFTeam team ) => TeamManager.GetName( (int)team );
	public static string GetTitle( this TFTeam team ) => TeamManager.GetTitle( (int)team );
	public static bool IsJoinable( this TFTeam team ) => TeamManager.IsJoinable( (int)team );
	public static bool IsPlayable( this TFTeam team ) => TeamManager.IsPlayable( (int)team );
	public static Color GetColor( this TFTeam team ) => TeamManager.GetColor( (int)team );
	public static IEnumerable<TFPlayer> GetPlayers( this TFTeam team ) => Entity.All.OfType<TFPlayer>().Where( x => x.Team == team );

	public static TFTeam GetEnemy( this TFTeam team )
	{
		switch( team )
		{
			case TFTeam.Red: return TFTeam.Blue;
			case TFTeam.Blue: return TFTeam.Red;
			default: return team;
		}
	}

	public static bool Is( this HammerTFTeamOption option, TFTeam team )
	{
		var teamFromOption = option.ToTFTeam();
		if ( teamFromOption == TFTeam.Unassigned )
			return true;

		return team == teamFromOption;
	}

	public static TFTeam ToTFTeam( this HammerTFTeamOption option )
	{
		return option switch
		{
			HammerTFTeamOption.Red => TFTeam.Red,
			HammerTFTeamOption.Blue => TFTeam.Blue,
			_ => TFTeam.Unassigned
		};
	}
}
