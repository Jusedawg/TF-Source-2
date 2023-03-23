using System;

namespace TFS2
{
	public struct GamemodeProperties
	{
		public bool DisablePlayerRespawn { get; init; }
		/// <summary>
		/// Do you need players in both teams to start the round?
		/// </summary>
		public bool RequireBothTeams { get; init; }
		public bool ShouldAnnounceFirstBlood { get; init; }
		public bool DisableGameStartSong { get; init; }
		public bool IsAttackDefense { get; init; }
		public bool RequireReadyUp { get; init; }
		/// <summary>
		/// If false, shows the arena selection screen.
		/// </summary>
		public bool DisableTeamSelection { get; init; }
		public Func<TFPlayer, TFTeam> AutoTeamOverride;
		public GamemodeProperties()
		{
			DisablePlayerRespawn = false;
			RequireBothTeams = false;
			ShouldAnnounceFirstBlood = false;
			DisableGameStartSong = false;
			IsAttackDefense = false;
			RequireReadyUp = false;
			DisableTeamSelection = false;
		}
	}
}
