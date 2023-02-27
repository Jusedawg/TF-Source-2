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
		public bool ShouldPlayGameStartSong { get; init; }
		public bool IsAttackDefense { get; init; }
		/// <summary>
		/// If false, shows the arena selection screen.
		/// </summary>
		public bool AllowTeamSelection { get; init; }
		public Func<TFPlayer, TFTeam> AutoTeamOverride;
		public GamemodeProperties()
		{
			DisablePlayerRespawn = false;
			RequireBothTeams = false;
			ShouldAnnounceFirstBlood = false;
			ShouldPlayGameStartSong = true;
			IsAttackDefense = false;
			AllowTeamSelection = true;
		}
	}
}
