using Sandbox;
using System;

namespace TFS2;

partial class TFGameMovement
{
	public override bool IsDuckingEnabled()
	{
		// Disable ducking underwater.
		if ( Player.IsUnderwater || (Player.InWater && !Player.IsGrounded) ) 
			return false;

		return base.IsDuckingEnabled();
	}
}
