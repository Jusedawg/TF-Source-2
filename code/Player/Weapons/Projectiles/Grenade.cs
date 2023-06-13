using Sandbox;
using Amper.FPS;

namespace TFS2;

public partial class Grenade : TFProjectile
{
	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/weapons/w_models/w_grenade_grenadelauncher.vmdl" );
		SetBBox( 4 );

		MoveType = ProjectileMoveType.Physics;
		DamageInfo = DamageInfo.WithTag(TFDamageTags.Blast);
		FaceVelocity = false;
		AutoExplodeTime = GetExplodeTime();
		EnableShadowCasting = false;
	}

	public override void OnTraceTouch( Entity other, TraceResult trace )
	{
		// If we touched the floor, don't run phys checks again.
		if ( Touched )
			return;

		if ( other == null )
			return;

		if ( CanExplodeFromTouching( other ) )
		{
			Explode( other, trace );
		}
	}

	public virtual bool CanExplodeFromTouching( Entity other )
	{
		// Can't touch our teammaters.
		if ( ITeam.IsSame( this, other ) ) return false;

		// grenades can only directly impact players.
		return other is TFPlayer || other is TFBuilding;
	}

	public float GetExplodeTime()
	{
		// TODO: Attribute multiplier.
		return 2.3f;
	}

	public const float ShakeAmplitude = 10;
	public const float ShakeFrequency = 150;
	public const float ShakeDuration = 1;
	public const float ShakeRadius = 300;

	public override void Explode( TraceResult trace )
	{
		base.Explode( trace );
		ScreenShake.Shake( Position, ShakeAmplitude, ShakeFrequency, ShakeDuration, ShakeRadius, ShakeCommand.Start );
	}

	public override string TrailParticleName => $"particles/stickybomb/pipebombtrail_{Team.GetName()}.vpcf";
	public override string CriticalTrailParticleName => $"particles/stickybomb/critical_pipe_{Team.GetName()}.vpcf";

	public override float TouchedDamageMultiplier => tf_grenade_roller_damage_mult;
	[ConVar.Replicated] public static float tf_grenade_roller_damage_mult { get; set; } = 0.6f;
}
