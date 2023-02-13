using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	public static class Debug
	{
		public static bool Enabled()
		{
			return Console.Vars.breaker_debug;
		}

		public static void Log(string message)
		{
			if ( !Enabled() )
				return;

			Logging.Info($"[Debug] {message}");
		}
		public static void Log( object message )
		{
			Log( message.ToString() );
		}
	}
}
