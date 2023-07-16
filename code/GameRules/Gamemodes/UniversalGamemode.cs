using Sandbox;

namespace TFS2
{
	public abstract class UniversalGamemode : BaseNetworkable, IGamemode
	{
		public virtual string Title => ToString();
		public virtual string Icon => IGamemode.DEFAULT_ICON;

		public virtual GamemodeProperties Properties => default;
		public virtual int Priority => 0;

		public abstract bool HasWon(out TFTeam team, out TFWinReason reason);

		public abstract bool IsActive();

		public virtual bool ShouldSwapTeams(TFTeam winner, TFWinReason winReason)
		{
			return Properties.SwapTeamsOnRoundRestart;
		}
	}
}
