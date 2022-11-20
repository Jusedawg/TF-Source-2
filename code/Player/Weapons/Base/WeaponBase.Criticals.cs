using Sandbox;
using Amper.FPS;

namespace TFS2;

partial class TFWeaponBase
{
	[Net, Predicted] public bool IsCurrentAttackCritical { get; set; }
	int LastCriticalCheckTick { get; set; }

	/// <summary>
	/// Calculates whether the current attack is a critical hit and updates the value of <c>IsCurrentAttackCritical</c>.
	/// </summary>
	public void CalculateIsAttackCritical()
	{
		if ( TFOwner == null )
			return;

		if ( LastCriticalCheckTick == Time.Tick )
			return;

		LastCriticalCheckTick = Time.Tick;
		IsCurrentAttackCritical = false;

		// If we're in round end, and we are on the winning team
		if ( TFGameRules.Current.State == GameState.RoundEnd && TFGameRules.Current.Winner == TFOwner.TeamNumber ) 
		{
			IsCurrentAttackCritical = true;
			return;
		}

		IsCurrentAttackCritical = CalculateIsAttackCriticalHelper();
	}

	public virtual bool CanFireCriticalShots() => true;
	public virtual bool CanFireRandomCriticalShots() => false;

	/// <summary>
	/// A helper to decide if current attack should be a critical hit.
	/// </summary>
	/// <returns></returns>
	public virtual bool CalculateIsAttackCriticalHelper()
	{
		if ( TFOwner == null )
			return false;

		if ( !CanFireCriticalShots() )
			return false;

		if ( TFOwner.IsCritBoosted )
			return true;

		if ( CalculateIsAttackRandomCriticalHelper() )
			return true;

		return false;

	}

	public virtual bool CalculateIsAttackRandomCriticalHelper()
	{
		if ( !CanFireRandomCriticalShots() )
			return false;

		// Disable random crits for now, figure them out later.
		return false;
	}
}
