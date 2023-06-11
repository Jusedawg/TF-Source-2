using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public partial class SentryRockets : Rocket
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/buildables/sentry/lvl3/sentry3_rockets.vmdl" );
	}

	public override Trace SetupCollisionTrace( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs )
	{
		// Same as base but ignore launcher instead of owner

		var tr = Trace.Ray( start, end )

			// Collides with:
			.WithAnyTags( CollisionTags.Solid, CollisionTags.Clip, CollisionTags.ProjectileClip )

			// Except weapons and other projectiles.
			.WithoutTags( CollisionTags.Projectile, CollisionTags.Weapon )

			// Doesn't collide with debris
			.WithoutTags( CollisionTags.Debris )

			.Ignore( this )
			.Ignore( Launcher );

		if ( !SDKGame.mp_friendly_fire )
			tr = tr.WithoutTags( TeamManager.GetTag( TeamNumber ) );

		if ( mins != 0 || maxs != 0 ) tr = tr.Size( mins, maxs );
		return tr;
	}
}
