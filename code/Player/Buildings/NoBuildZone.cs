using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

[Library("func_nobuild")]
[Title("No Building Zone")]
[Category("Gameplay")]
[AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[Solid, VisGroup( VisGroup.Trigger ), HideProperty( "enable_shadows" )]
[HammerEntity]
public partial class NoBuildZone : ModelEntity
{
	public override void Spawn()
	{
		SetupPhysicsFromModel( PhysicsMotionType.Static );
		EnableAllCollisions = false;

		Transmit = TransmitType.Always; 
	}
	public static bool InNoBuild(Vector3 pos)
	{
		foreach(var zone in Entity.All.OfType<NoBuildZone>())
		{
			if ( zone.WorldSpaceBounds.Contains( pos ) )
				return true;
		}

		return false;
	}
}
