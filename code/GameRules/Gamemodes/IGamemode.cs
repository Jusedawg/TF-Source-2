using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2
{
	/// <summary>
	/// If you are adding custom gamemode, inherit from <see cref="GamemodeEntity"/> or <see cref="GamemodeNetworkable"/> instead!
	/// </summary>
	public interface IGamemode
	{
		public const string DEFAULT_ICON = "/ui/icons/empty.png";

		public string Title { get; }
		public string Icon { get; }
		public GamemodeProperties Properties { get; }
		public bool IsActive();
		public bool HasWon( out TFTeam team, out TFWinReason reason );
	}
}
