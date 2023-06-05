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

	public override float GetDuckSpeedModifier( float fraction )
	{
		if ( Player.InCondition(TFCondition.Humiliated) )
		{
			return 1 - fraction;
		}
		return base.GetDuckSpeedModifier( fraction );
	}
}
