using System;
using Amper.FPS;

namespace TFS2;

partial class TFPlayerAnimator : PlayerAnimator
{
	new TFPlayer Player => (TFPlayer)base.Player;

	public override void UpdateMovement()
	{
		if ( Player.InCondition( TFCondition.Taunting ) )
		{
			UpdateTauntMovement();
			return;
		}

		var velocity = Player.Velocity;
		var speed = velocity.Length;
		var forward = Player.Rotation.Forward.Dot( velocity );
		var sideward = Player.Rotation.Right.Dot( velocity );

		SetAnimParameter( "wishspeed", speed );

		// Yes I know, magic numbers bad, bla bla bla, but this is the easiest workaround atm.
		// When moving diagonally, we only get 0.7 for both x and y, so we just multiply that
		// so it is always above 1, even when moving diagonally, and clamp it
		SetAnimParameter( "move_y", Math.Clamp( 1.5f * forward / speed, -1f, 1f ) );
		SetAnimParameter( "move_x", Math.Clamp( 1.5f * sideward / speed, -1f, 1f ) );
	}

	public void UpdateTauntMovement()
	{

	}
}
