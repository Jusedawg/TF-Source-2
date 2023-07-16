namespace TFS2;

/// <summary>
/// If you are adding custom gamemode, inherit from <see cref="MapGamemode"/> or <see cref="UniversalGamemode"/> instead!
/// </summary>
public interface IGamemode
{
	public const string DEFAULT_ICON = "ui/icons/empty.png";

	public string Title { get; }
	public string Icon { get; }
	public GamemodeProperties Properties { get; }
	/// <summary>
	/// When the IsActive check for this gamemode should happen relative to other gamemodes
	/// </summary>
	public int Priority { get; }
	public bool IsActive();
	public bool HasWon( out TFTeam team, out TFWinReason reason );
	public bool ShouldSwapTeams(TFTeam winner, TFWinReason winReason);
}
