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
	[Net, Predicted] private bool IsFiring { get; set; }

	public override void SimulateAttack()
	{
		base.SimulateAttack();

		IsFiring = WishPrimaryAttack();
	}

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

	SoundHandle? SpinSound { get; set; }
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

				SpinSound?.Stop(true);
				SpinSound = Audio.Play( sound, this );
			}
		}
		else
		{
			//Don't want to interrupt spindown sound
			if ( LastSpinSound == SpinSoundType.SpinDown )
			{
				return;
			}
			SpinSound?.Stop(true);
			SpinSound = null;
		}
	}

	public override void OnHolster( SDKPlayer owner )
	{
		base.OnHolster( owner );
		
		SpinSound?.Stop(true);
		SpinSound = null;
	}

	public override void ModifyOwnerMaxSpeed( ref float speed )
	{
		if ( IsHolding )
			speed = SpinnedMaxSpeed;
	}

	public SpinSoundType GetSpinSoundType()
	{
		//Prevents current sound from playing for a split second when switching weapons
		if ( ViewModel?.Weapon != this)
		{
			return SpinSoundType.None;
		}

		// If we are in the progress of changing our spin state.
		if ( NextSpinChangeTime >= Time.Now )
		{
			return IsHolding
				? SpinSoundType.SpinUp
				: SpinSoundType.SpinDown;
		}

		if ( IsHolding )
		{
			if ( IsFiring )
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

	//
	// Animations
	//

	float BarrelAngle { get; set; }
	float BarrelVelocity { get; set; }
	float BarrelTargetVelocity { get; set; }
	float BarrelMaxVelocity { get; set; } = 20f;

	public override void OnEquip( SDKPlayer owner )
	{
		base.OnEquip( owner );

		BarrelVelocity = 0f;
		BarrelTargetVelocity = 0f;
		BarrelAngle = 0f;
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		UpdateViewmodelAnimations();
	}

	public void UpdateViewmodelAnimations()
	{
		if ( IsHolding )
		{
			if ( IsFiring && HasEnoughAmmoToAttack() )
			{
					SendViewModelAnimParameter( "b_fire_hold", true );
			}

			else SendViewModelAnimParameter( "b_fire_hold", false );
		}

		BarrelTargetVelocity = IsHolding ? BarrelMaxVelocity : 0f;

		UpdateBarrelRotation();
	}

	public void UpdateBarrelRotation()
	{
		CalculateBarrelMovement();
		
		SendViewModelAnimParameter("f_barrel_cycle", BarrelAngle / 360);
		SetAnimParameter("f_barrel_cycle", BarrelAngle / 360 );
		
		/*
		//Procedural Animation, could not get transform/SetBone to perform right so we are using animgraph method for now
		if ( ViewModel?.Weapon == this && BarrelBoneIndex != -1 )
		{
			Transform newTransform = ViewModel.GetBoneTransform( BarrelBoneIndex, false);
			DebugOverlay.ScreenText($"{newTransform.Rotation.Angles()}", 1);
			//newTransform.Rotation = Rotation.From( 0, 0, BarrelAngle);
			//QAngle

			ViewModel.SetBone( BarrelBoneIndex, newTransform );
		}
		*/
	}

	void CalculateBarrelMovement()
	{
		if ( BarrelVelocity != BarrelTargetVelocity )
		{
			float flBarrelAcceleration = IsHolding ? 0.5f : 0.1f;

			// update barrel velocity to bring it up to speed or to rest
			BarrelVelocity = MathX.Approach( BarrelVelocity, BarrelTargetVelocity, flBarrelAcceleration );
		}

		// update the barrel rotation based on current velocity
		BarrelAngle += BarrelVelocity;

		//Reset barrel angle beyond 360, for use with f_barrel_cycle
		if ( BarrelAngle >= 360 )
		{
			BarrelAngle -= 360;
		}
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
