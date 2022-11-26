using Sandbox;
using Amper.FPS;

namespace TFS2;

[Library( "tf_weapon_minigun", Title = "Minigun" )]
public partial class Minigun : TFHoldWeaponBase
{
	public const float SpinStopTime = 0.75f;
	public const float SpinStartTime = 0.75f;
	public const float SpinnedMaxSpeed = 110;

	[Net, Predicted] public float NextSpinChangeTime { get; set; }

	public override bool WishHold()
	{
		return WishPrimaryAttack() || WishSecondaryAttack();
	}

	public override bool CanHold()
	{
		if ( NextSpinChangeTime > Time.Now )
			return false;

		return CanAttack();
	}

	public override bool CanStopHolding()
	{
		if ( NextSpinChangeTime > Time.Now )
			return false;

		return base.CanStopHolding();
	}

	public override void OnHoldStart()
	{
		SendViewModelAnimParameter( "b_spool", true );
		SendPlayerAnimParameter( "b_deployed", true );

		NextSpinChangeTime = Time.Now + SpinStartTime;
	}

	public override void OnHoldStop()
	{
		SendViewModelAnimParameter( "b_spool", false );
		SendPlayerAnimParameter( "b_deployed", false );

		NextSpinChangeTime = Time.Now + SpinStopTime;
	}

	public override bool CanOwnerJump()
	{
		if ( IsHolding )
			return false;

		return base.CanOwnerJump();
	}

	public override bool CanPrimaryAttack()
	{
		if ( NextSpinChangeTime > Time.Now )
			return false;

		return base.CanPrimaryAttack();
	}

	public override bool CanHolster( SDKPlayer player )
	{
		if ( IsHolding )
			return false;

		if ( NextSpinChangeTime > Time.Now ) 
			return false;
    
		return base.CanHolster( player );
	}

	//
	// Effects
	//

	Sound? SpinSound { get; set; }
	SpinSoundType LastSpinSound { get; set; }

	public const string SpinDrySound = "weapon_minigun.empty";
	public const string SpinLoopSound = "weapon_minigun.spin";
	public const string SpinFireSound = "weapon_minigun.shoot";
	public const string SpinFireCritSound = "weapon_minigun.shoot.crit";
	public const string SpinStartSound = "weapon_minigun.wind_up";
	public const string SpinStopSound = "weapon_minigun.wind_down";

	public override void ClientTick()
	{
		base.ClientTick();

		var soundType = GetSpinSoundType();
		if ( soundType != SpinSoundType.None )
		{
			if ( SpinSound == null || soundType != LastSpinSound )
			{
				var sound = soundType switch
				{
					SpinSoundType.Spin => SpinLoopSound,
					SpinSoundType.Fire => SpinFireSound,
					SpinSoundType.CritFire => SpinFireCritSound,
					SpinSoundType.DryFire => SpinDrySound,
					SpinSoundType.SpinUp => SpinStartSound,
					SpinSoundType.SpinDown => SpinStopSound,
					_ => ""
				};

				LastSpinSound = soundType;

				SpinSound?.Stop();
				SpinSound = PlaySound( sound );
			}
		}
		else
		{
			SpinSound?.Stop();
			SpinSound = null;
		}
	}

	public override void OnHolster( SDKPlayer owner )
	{
		base.OnHolster( owner );
		
		SpinSound?.Stop();
		SpinSound = null;
	}

	public override void ModifyOwnerMaxSpeed( ref float speed )
	{
		if ( IsHolding )
			speed = SpinnedMaxSpeed;
	}

	public SpinSoundType GetSpinSoundType()
	{
		// If we are in the progress of changing our spin state.
		if ( NextSpinChangeTime >= Time.Now )
		{
			return IsHolding
				? SpinSoundType.SpinUp
				: SpinSoundType.SpinDown;
		}

		if ( IsHolding )
		{
			if ( WishPrimaryAttack() )
			{
				if ( HasEnoughAmmoToAttack() )
				{
					return IsCurrentAttackCritical
						? SpinSoundType.CritFire
						: SpinSoundType.Fire;
				}

				return SpinSoundType.DryFire;
			}

			return SpinSoundType.Spin;
		}

		return SpinSoundType.None;
	}

	public override float CrosshairScale() => 1.5f;

	public enum SpinSoundType
	{
		None,
		SpinUp,
		SpinDown,
		Spin,
		Fire,
		CritFire,
		DryFire
	}
}
