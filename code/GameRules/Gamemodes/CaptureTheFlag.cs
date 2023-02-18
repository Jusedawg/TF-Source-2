using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	public class CaptureTheFlag : IGamemode
	{
		public string Title => "Capture The Flag";

		public string Icon => IGamemode.DEFAULT_ICON;

		public CaptureTheFlag()
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
