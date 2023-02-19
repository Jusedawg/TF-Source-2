namespace TFS2
{
	public class GamemodeProperties
	{
		public bool DisablePlayerRespawn { get; init; } = false;
		/// <summary>
		/// Do you need players in both teams to start the round?
		/// </summary>
		public bool RequireBothTeams { get; init; } = false;
		public bool ShouldAnnounceFirstBlood { get; init; } = false;
		public bool ShouldPlayGameStartSong { get; init; } = true;
		public bool IsAttackDefense { get; init; } = false;
	}
}
