using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breaker;
using Breaker.Commands;
using Sandbox;

namespace TFS2
{
	public static class TFCommandSelectors
	{
		[BRKEvent.ConfigLoaded]
		static void OnInit()
		{
			ClientParser.RegisterMultiSelector( "blu", SelectBlue );
			ClientParser.RegisterMultiSelector( "blue", SelectBlue );
			ClientParser.RegisterMultiSelector( "red", SelectRed);
		}

		static IEnumerable<IClient> SelectBlue(IClient caller, string input )
		{
			return TFTeam.Blue.GetPlayers().Select(ply => ply.Client);
		}

		static IEnumerable<IClient> SelectRed(IClient caller, string input)
		{
			return  TFTeam.Red.GetPlayers().Select( ply => ply.Client );
		}
	}
}
