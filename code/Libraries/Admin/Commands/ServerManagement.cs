using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	[Category("Server Management")]
	public static class ServerManagement
	{
		[Command("map", "changelevel"), Permission("breaker.map")]
		public static void ChangeMap(string map)
		{
			Logging.TellAll( $"{Command.CallerName} changed map to {map}..." );
			Game.ChangeLevel( map );
		}
	}
}
