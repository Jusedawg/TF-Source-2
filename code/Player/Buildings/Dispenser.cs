using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;
[Library( "tf_building_dispenser" )]
[Title( "Dispenser" )]
public class Dispenser : TFBuilding
{
	public override void TickActive()
	{
		DebugOverlay.ScreenText( "Dispenser Active" );
	}
}
