using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	public class ControlPoints : GamemodeNetworkable
	{
		public override string Title => "Control Points";

		public override string Icon => "/ui/hud/scoreboard/icon_mode_control.png";

		public override GamemodeProperties Properties => default;

		public ControlPoints()
		{
			//EventDispatcher.Subscribe<>
		}

		public override bool HasWon( out TFTeam winnerTeam, out TFWinReason reason )
		{
			winnerTeam = TFTeam.Unassigned;
			reason = TFWinReason.AllPointsCaptured;
			foreach ( TFTeam team in Enum.GetValues( typeof( TFTeam ) ) )
			{
				if ( !team.IsPlayable() )
					continue;

				if ( TeamOwnsAllControlPoints( team ) )
				{
					winnerTeam = team;
					return true;
				}
			}

			return false;
		}

		public override bool IsActive() => Entity.All.OfType<ControlPoint>().Any();

		public static bool TeamOwnsAllControlPoints( TFTeam team )
		{
			var points = ControlPoint.All;

			// team can't own all points if there is none.
			if ( points.Count == 0 )
				return false;

			// points that by default dont belong to us.
			var enemyPoints = points.Where( x => x.GetDefaultTeamOwner() != team );

			// if we don't have any enemy points we own all points.
			if ( enemyPoints.Count() == 0 )
				return false;

			// team owns all the points if no other team owns all the points.
			return !enemyPoints.Where( x => x.OwnerTeam != team ).Any();
		}
	}
}
