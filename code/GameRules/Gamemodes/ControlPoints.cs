using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	public class ControlPoints : IGamemode
	{
		public string Title => "Control Points";

		public string Icon => "/ui/hud/scoreboard/icon_mode_control.png";

		public ControlPoints()
		{
			//EventDispatcher.Subscribe<>
		}

		public bool HasWon( out TFTeam team, out TFWinReason reason )
		{
			throw new NotImplementedException();
		}

		public bool IsActive()
		{
			throw new NotImplementedException();
		}
	}
}
