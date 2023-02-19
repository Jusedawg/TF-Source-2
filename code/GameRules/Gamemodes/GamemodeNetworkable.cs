using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	public abstract class GamemodeNetworkable : BaseNetworkable, IGamemode
	{
		public virtual string Title => ToString();
		public virtual string Icon => IGamemode.DEFAULT_ICON;

		public virtual GamemodeProperties Properties => default;

		public abstract bool HasWon( out TFTeam team, out TFWinReason reason );

		public abstract bool IsActive();

		public GamemodeNetworkable()
		{

		}
	}
}
