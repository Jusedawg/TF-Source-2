namespace TFS2;

/// <summary>
/// Partial for weapon attributes from data, with attributes taken into account.
/// (When attribute system is implemented.)
/// </summary>
partial class TFWeaponBase
{
	public override string GetParticleTracerEffect()
	{
		if ( IsCurrentAttackCritical )
		{
			return Team == TFTeam.Blue
				? Data.TracerBlueCritical
				: Data.TracerRedCritical;
		}

		return Team == TFTeam.Blue
			? Data.TracerBlue
			: Data.TracerRed;
	}
	public override string GetMuzzleFlashEffect() => Data.MuzzleFlash;

	public override int GetAmmoPerShot() => Data.AmmoPerShot;
	public override int GetBulletsPerShot() => Data.BulletsPerShot;
	public override float GetDamage() => Data.Damage;
	public override int GetTracerFrequency() => Data.TracerFrequency;
	public override int GetRange() => Data.Range;
	public override float GetSpread() => Data.BulletSpread;
	public override int GetClipSize() => Data.ClipSize;
	public override int GetReserveSize() => MaxReserve;
	public override bool IsReloadingEntireClip() => Data.ReloadsEntireClip;

	public override float GetAttackTime() => Data.AttackTime;
	public override float GetReloadStartTime() => Data.ReloadStartTime;
	public override float GetReloadTime() => Data.ReloadTime;
	public override float GetDeployTime() => Data.DeployTime;
}
