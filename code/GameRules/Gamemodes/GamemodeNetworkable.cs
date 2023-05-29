using Sandbox;

namespace TFS2
{
	public abstract class GamemodeNetworkable : BaseNetworkable, IGamemode
	{
		public virtual string Title => ToString();
		public virtual string Icon => IGamemode.DEFAULT_ICON;

		public virtual GamemodeProperties Properties => default;

		public abstract bool HasWon(out TFTeam team, out TFWinReason reason);

		public abstract bool IsActive();

		public virtual bool ShouldSwapTeams(int winner, int winReason)
		{
			return Properties.SwapTeamsOnRoundRestart;
		}
	}
}
