using Sandbox;
using Amper.FPS;

namespace TFS2;

public partial class Rocket : TFProjectile
{
	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/weapons/w_models/w_rocket.vmdl" );

		MoveType = ProjectileMoveType.Fly;
		DamageInfo.WithTag(DamageTags.Blast);
		EnableShadowCasting = false;
	}

	public override void OnTraceTouch( Entity other, TraceResult trace )
	{
		if ( other == null )
			return;

		// Rocket explodes whenever it touches anything.
		Explode( other, trace );
	}

	public override string TrailParticleName => "particles/rockettrail/rockettrail.vpcf";
	public override string CriticalTrailParticleName => $"particles/rockettrail/critical_rocket_{Team.GetName()}.vpcf";
}
