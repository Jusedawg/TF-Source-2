using Sandbox;
using Amper.FPS;

namespace TFS2;

public partial class TFProjectile : Projectile
{
	public TFTeam Team => (TFTeam)TeamNumber;

	public override void OnInitialized()
	{
		base.OnInitialized();
		SetMaterialGroup( Team == TFTeam.Blue ? 1 : 0 );
		
		if(Launcher is IFalloffProvider provider)
		{
			if ( provider.UseFalloff )
				DamageInfo.WithTag( TFDamageTags.UseFalloff );

			if ( provider.UseRampup )
				DamageInfo.WithTag( TFDamageTags.UseRampup );
		}
	}

	Particles CriticalTrail { get; set; }

	public override void CreateTrails()
	{
		base.CreateTrails();
		
		if ( DamageInfo.HasTag( TFDamageTags.Critical ) )
		{
			if ( !string.IsNullOrEmpty( CriticalTrailParticleName ) )
				CriticalTrail = Particles.Create( CriticalTrailParticleName, this, "criticaltrail" );
		}
	}

	public override void DeleteTrails( bool immediate = false )
	{
		base.DeleteTrails( immediate );
		CriticalTrail?.Destroy( immediate );
	}

	public virtual bool CanBeDeflected => true;
	public virtual bool ShouldChangeTeamOnDeflect => true;
	public virtual bool ShouldApplyBoostOnDeflect => true;

	public const string DeflectedSound = "weapon_flamethrower.reflect_projectile";
	public const string DeflectedEffect = "particles/rocketjumptrail/deflect_fx.vpcf";
	Sound DeflectSound;

	public virtual void Deflected( TFWeaponBase weapon, TFPlayer who )
	{
		if ( !Game.IsServer )
			return;

		Owner = who;
		Launcher = weapon;

		DamageInfo = DamageInfo.WithAttacker( Owner );

		if ( ShouldChangeTeamOnDeflect )
		{
			TeamNumber = who.TeamNumber;
			SetMaterialGroup( Team == TFTeam.Blue ? 1 : 0 );
		}

		if ( ShouldApplyBoostOnDeflect )
		{
			// Reflects make projectiles inflict minicritical damage.
			if ( ShouldMiniCritOnDeflection() )
				DamageInfo = DamageInfo.WithTag( TFDamageTags.MiniCritical );

			// Pyro was critboosted, projectiles retain the boost.
			if ( who.IsCritBoosted )
				DamageInfo = DamageInfo.WithTag( TFDamageTags.Critical );
		}

		CreateTrails();
		DeflectedEffects();
		DeflectSound = PlaySound( DeflectedSound );
	}

	public virtual bool ShouldMiniCritOnDeflection() => true;

	protected override void OnDestroy()
	{
		// HACK: killing reflected projectile plays the sound at world origin
		// stop the sound manually for now.

		DeflectSound.Stop();
	}

	[ClientRpc]
	public virtual void DeflectedEffects()
	{
		Particles.Create( DeflectedEffect, this );
	}

	public virtual string CriticalTrailParticleName => "";
	public override string ExplosionParticleName => "particles/explosion/explosioncore_wall.vpcf";
	public override string ExplosionSoundEffect => "weapon.explosion";
}
