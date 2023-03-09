using Amper.FPS;
using Sandbox;
using System;

namespace TFS2;

public partial class TFGameMovement : GameMovement
{
	protected new TFPlayer Player;

	public override void Simulate( SDKPlayer player )
	{
		Player = (TFPlayer)player;
		base.Simulate( player );
	}

	public override bool CanJump()
	{
		// Prevent the player from jumping while doing looping taunts (e.g. conga)
		// The taunt system will make the player stop taunting upon jumping.
		if ( Player.InCondition( TFCondition.Taunting ) ) return false;

		return base.CanJump();
	}

	public override Trace SetupBBoxTrace( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs )
	{
		var tr = base.SetupBBoxTrace( start, end, mins, maxs );
		var playerTeam = Player.Team;

		//
		// Team Collision
		//

		if ( !tf_collide_with_teammates )
			tr = tr.WithoutTags( playerTeam.GetTag() );

		//
		// Blockers
		//

		// If round is not over yet
		if ( !TFGameRules.Current.AreRespawnRoomsOpen() )
		{
			// if visualizers are active, we ignore only our team's own barriers.
			tr = tr.WithoutTags( $"team_barrier_{playerTeam.GetName()}" );
		}
		else
		{
			// otherwise ignore all barriers.
			tr = tr.WithoutTags( $"team_barrier" );
		}


		return tr;
	}

	public override void AirDash()
	{
		base.AirDash();
		AirDashEffects();
	}

	public string AirDashEffect => "particles/rocketjumptrail/doublejump_puff.vpcf";

	public void AirDashEffects()
	{
		Particles.Create( AirDashEffect, Player, "doublejumpfx" );

		if ( Player.AirDashCount > 1 )
		{
			var pitch = ((float)Player.AirDashCount).Remap( 1, 5, 1, 1.2f );
			Sound.FromEntity( "general.banana_slip", Player ).SetRandomPitch( pitch, pitch );
		}
	}
}
