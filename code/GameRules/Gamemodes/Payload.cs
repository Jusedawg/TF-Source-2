using Sandbox;
using System.Linq;

namespace TFS2
{
	public class Payload : ControlPoints
	{
		public override string Title => "Payload";
		public override GamemodeProperties Properties => new() { IsAttackDefense = true};
		public override bool IsActive() => Entity.All.OfType<Cart>().Any();
        public override bool ShouldSwapTeams(TFTeam winner, TFWinReason winReason)
        {
            return true;
        }
    }
}
