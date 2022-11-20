using Sandbox;
using Amper.FPS;

namespace TFS2;

public partial class TFHoldWeaponBase : TFWeaponBase
{
	[Net, Predicted] public bool IsHolding { get; set; }

	public virtual bool WishHold() => WishPrimaryAttack();
	public virtual bool CanHold() => CanPrimaryAttack();

	/// <summary>
	/// This handles the attachment feature of the medigun. Player
	/// can hold the attack key to change their healing target.
	/// </summary>
	public override void SimulateAttack()
	{
		// The user is pressing the attack key and also is allowed to 
		// use their weapons right now.
		if ( WishHold() && CanHold() )
		{
			// Start holding.
			if ( !IsHolding )
			{
				IsHolding = true;
				OnHoldStart();
			}

			// called every tick while player is holding attack.
			OnHolding();
		}
		else if ( CanStopHolding() ) 
		{
			if ( IsHolding )
			{
				IsHolding = false;
				OnHoldStop();
			}

			// Called every frame while player is holding attack.
			OnIdling();
		}

		SimulateSecondaryAttack();
	}

	/// <summary>
	/// Do something while user is holding weapon.
	/// </summary>
	public virtual void OnHolding() 
	{
		// Do primary attacks while we're holding button.
		SimulatePrimaryAttack();
	}

	/// <summary>
	/// Do something when user does not hold the weapon.
	/// </summary>
	public virtual void OnIdling() { }

	/// <summary>
	/// Do something when the user starts holding this weapon.
	/// </summary>
	public virtual void OnHoldStart() { }
	/// <summary>
	/// Do something when the users stops holding the attack button.
	/// </summary>
	public virtual void OnHoldStop() { }
	public virtual bool CanStopHolding() => true;

	public override void OnHolster( SDKPlayer owner )
	{
		if ( IsHolding )
		{
			IsHolding = false;
			OnHoldStop();
		}

		base.OnHolster( owner );
	}
}
