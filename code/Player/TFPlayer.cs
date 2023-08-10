using Sandbox;
using System;
using System.Linq;
using Amper.FPS;

namespace TFS2;

/// <summary>
/// Team Fortress Source 2 Player
/// </summary>
public partial class TFPlayer : SDKPlayer
{
	public new static TFPlayer LocalPlayer => Game.LocalPawn as TFPlayer;
	public override float DeathAnimationTime => 2;

	public TFPlayer()
	{
		SubscribeToConditionEvents();
	}

	public override void Spawn()
	{
		base.Spawn();
		Animator = new TFPlayerAnimator();
		ResponseController = new( this );
	}

	public override void Respawn()
	{
		// We wish to change our class to something else.
		if ( DesiredPlayerClass.IsValid() )
		{
			PlayerClass = DesiredPlayerClass;
			DesiredPlayerClass = null;
			DestroyBuildings();
		}

		base.Respawn();

		// We need to stop taunting to prevent lingering variables
		if ( InCondition( TFCondition.Taunting ) )
		{
			StopTaunt();
		}

		RemoveAllConditions();
		ResponseController.Reset();

		// We are respawning and we have a class selected.
		if ( PlayerClass.IsValid() )
		{
			Tags.Add( PlayerClass.GetTag() );

			SetupPlayerClass();
			Regenerate( true );
		}
	}

	public override bool IsReadyToPlay()
	{
		if ( !PlayerClass.IsValid() )
			return false;

		return base.IsReadyToPlay();
	}

	/// <summary>
	/// Time since last regeneration, used by regeneration locker.
	/// </summary>
	public TimeSince TimeSinceRenegeration { get; set; }

	public bool CanRegenerate()
	{
		if ( IsDead )
			return false;

		if ( !IsReadyToPlay() )
			return false;

		return true;
	}

	/// <summary>
	/// Initialize and reset all player properties like health. This function will be called every time a player touches
	/// a resupply locker or respawns.
	/// </summary>
	public void Regenerate( bool full = false )
	{
		if ( !Game.IsServer )
			return;

		using var _ = Prediction.Off();

		IsThirdpersonTF = false;

		// If we are not alive or we don't have
		if ( !CanRegenerate() )
			return;

		// The player wanted to change class but wasnt able to, respawn them now.
		if ( DesiredPlayerClass.IsValid() )
		{
			Respawn();
			return;
		}

		if ( full )
		{
			// If we do full regeneration, delete our entire inventory.
			DeleteAllWeapons();
		}

		// Reset health to max health.
		if ( Health < GetMaxHealth() )
			Health = GetMaxHealth();

		RegenerateWeaponsForClass( PlayerClass );

		//
		// Abilities + Misc stuff
		//

		if ( UsesMetal )
			Metal = MaxMetal;

		RemoveCondition( TFCondition.Burning );

		// Let SDKGame know about this.
		TFGameRules.Current.PlayerRegenerate( this, full );

		CreateTauntList();
	}

	public void RegenerateWeaponsForClass( PlayerClass pclass )
	{
		// getting loadout for this client
		var loadout = Loadout.ForClient( Client );

		foreach ( TFWeaponSlot slot in Enum.GetValues( typeof( TFWeaponSlot ) ) )
		{
			// get the weapons in our loadout for this slot.
			var data = loadout.GetLoadoutItem( pclass, slot );

			// No weapon here
			if ( !data.IsValid() )
				continue;

			var currentWeapon = GetWeaponInSlot( slot );
			if ( currentWeapon.IsValid() )
			{
				// Same weapon.
				if ( data == currentWeapon.Data )
					continue;

				// remove whatever we have equipped.
				currentWeapon.Delete();
			}

			var weapon = data.CreateInstance();
			EquipWeapon( weapon );
		}

		RegenerateAllWeapons();

		if ( !ActiveWeapon.IsValid() )
			SwitchToNextBestWeapon();
	}

	public bool GiveAmmo(float fraction)
	{
		bool neededAmmo = false;

		// Get all the current weapon entries
		foreach ( var weapon in Children.OfType<TFWeaponBase>() )
		{
			if ( !weapon.IsInitialized )
				continue;

			// weapon doesnt have any ammo.
			if ( !weapon.NeedsAmmo() )
				continue;

			// if this is false, weapon is not supposed to be owned by his class.
			if ( !weapon.Data.TryGetOwnerDataForPlayerClass( PlayerClass, out var ownerData ) )
				continue;

			//
			// Restocking ammo
			//

			if ( ownerData.Reserve > 0 )
			{
				weapon.Reserve = CalculateAmmo( weapon.Reserve, ownerData.Reserve );
			}
			else
			{
				weapon.Clip = CalculateAmmo( weapon.Clip, weapon.Data.ClipSize );
			}

			int CalculateAmmo( int currentAmmo, int maxAmmo )
			{
				// this is how much we need to fully restock our ammo
				var need = maxAmmo - currentAmmo;

				// this is how much we can give
				var canGive = maxAmmo * fraction;

				// seeing how much will give.
				var willGive = Math.Min( need, canGive ).FloorToInt();

				if ( willGive > 0 )
				{
					currentAmmo += willGive;
					neededAmmo = true;
				}

				return currentAmmo;
			}
		}

		if ( UsesMetal && Metal != MaxMetal )
		{
			int metalToAdd = MathX.CeilToInt( MaxMetal * fraction );
			if ( GiveMetal( metalToAdd ) > 0 )
				neededAmmo = true;
		}

		return neededAmmo;
	}

	protected override bool PreEquipWeapon( SDKWeapon weapon, bool makeActive )
	{
		// This is overriden from base because each class has weapon in a different slot.

		var tfWeapon = weapon as TFWeaponBase;
		if ( !tfWeapon.IsValid() )
			return base.PreEquipWeapon( weapon, makeActive );

		// Can't be equipped by this class.
		if ( !tfWeapon.Data.TryGetOwnerDataForPlayerClass( PlayerClass, out var ownerData ) )
			return false;

		var slotWeapon = GetWeaponInSlot( ownerData.Slot );
		if ( slotWeapon.IsValid() )
		{
			// Check if we can drop a weapon.
			if ( !CanDrop( weapon ) )
				return false;

			ThrowWeapon( slotWeapon );
		}

		return true;
	}

	[ConVar.Server] public static float tf_dropped_weapons_force { get; set; } = 80;

	public override void OnKilled()
	{
		AwardDeathPoints();
		DropPickedItem();
		CreateDeathEntities( LastDamageInfo );

		var dropforce = Vector3.Random;
		dropforce.z = 0.8f;
		dropforce *= tf_dropped_weapons_force;

		//
		// Drop active weapon
		//

		if ( ActiveWeapon.IsValid() )
		{
			if ( ActiveWeapon is Builder b && b.IsCarryingBuilding )
			{
				var building = b.CarriedBuilding;
				building.TakeDamage( LastDamageInfo );
				building.OnKilled();
				Buildings.Remove( building );
			}

			ActiveWeapon.OnHolster( this );
			DropWeapon( ActiveWeapon, WorldSpaceBounds.Center, dropforce );
		}

		DropAmmoPack( dropforce );

		if ( InCondition( TFCondition.Taunting ) )
		{
			StopTaunt();
		}

		base.OnKilled();

		RemoveAllConditions();
	}

	/// <summary>
	/// Awards points to all parties on death.
	/// </summary>
	protected void AwardDeathPoints()
	{
		var info = LastDamageInfo;

		if ( info.Attacker is TFPlayer atk && atk != this )
		{
			atk.Kills++;

			if ( info.Tags.Contains( TFDamageTags.Backstab ) )
				atk.Backstabs++;

			var medigun = Weapons.OfType<Medigun>().FirstOrDefault();
			if (medigun != null && medigun.IsCharged)
			{
				atk.Bonus += 2;
			}

			// Gives defense points to the player which killed us if this player was
			// the last man standing on a control point or had the intel
			if ( PickedItem is Flag )
				atk.Defenses++;
			if ( ControlPoint?.TouchingEntities.OfType<TFPlayer>().Any( ply => ply.Team != ControlPoint.OwnerTeam ) == true )
				atk.Defenses++;
		}

		Deaths++;
	}

	public bool AllowTracking()
	{
		return Client != default && !Client.IsBot;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( !IsAlive )
			return;

		SimulateItems();
		SimulateCameraLogic();
		SimulateTaunts();
		SimulateBuildings();
		SimulateVO();
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsAlive )
			return;

		TickConditions();
		TickUberchargeEffects();
		TickHealing();
		TickInvisibility();
		SimulateGesture();
		CheckForLaunchedEnd();

		// Check if your weapon is completely empty.
		// If so, switch off the gun automatically.
		SwitchOffEmptyWeapon();
	}

	public virtual void OnSwitchedViewMode( bool is_first_person )
	{
		(ActiveWeapon as TFWeaponBase)?.OnSwitchedViewMode( is_first_person );
	}

	protected override void OnDestroy()
	{
		if ( !Game.IsServer )
			return;

		DropPickedItem();
		DeleteAllWeapons();
		DestroyBuildings();
	}

	/// <summary>
	/// Returns the eye height of the current class.
	/// </summary>
	/// <returns></returns>
	public float GetClassEyeHeight()
	{
		if ( PlayerClass == null )
			return base.GetPlayerViewOffset( false ).z;

		return PlayerClass.EyeHeight;
	}

	public override Vector3 GetPlayerViewOffset( bool ducked )
	{
		var vec = base.GetPlayerViewOffset( ducked );

		if ( !ducked )
			vec = vec.WithZ( GetClassEyeHeight() );

		return vec;
	}

	public override float GetMaxHealth()
	{
		if ( !PlayerClass.IsValid() )
			return base.GetMaxHealth();

		return PlayerClass.MaxHealth;
	}

	public override float CalculateMaxSpeed()
	{
		if ( !PlayerClass.IsValid() )
			return 0;

		var maxSpeed = PlayerClass.MaxSpeed;
		(ActiveWeapon as TFWeaponBase)?.ModifyOwnerMaxSpeed( ref maxSpeed );

		if ( InCondition( TFCondition.Humiliated ) )
		{
			maxSpeed *= 0.9f;
		}

		return maxSpeed;
	}

	public override ViewVectors ViewVectors => new()
	{
		ViewOffset = new( 0, 0, 72 ),

		HullMin = new( -24, -24, 0 ),
		HullMax = new( 24, 24, 82 ),

		DuckHullMin = new( -24, -24, 0 ),
		DuckHullMax = new( 24, 24, 62 ),
		DuckViewOffset = new( 0, 0, 45 ),

		ObserverHullMin = new( -10, -10, -10 ),
		ObserverHullMax = new( 10, 10, 10 ),

		DeadViewOffset = new( 0, 0, 14 )
	};

	public override SDKViewModel CreateViewModel() => new TFViewModel();
	public TFWeaponSlot GetActiveTFSlot() => (TFWeaponSlot)GetActiveSlot();
	public TFWeaponBase GetWeaponInSlot( TFWeaponSlot slot ) => GetWeaponInSlot( (int)slot ) as TFWeaponBase;

	public void UpdateMaterialGroup()
	{
		int baseSkin = Team == TFTeam.Blue
			? 1
			: 0;

		if ( Invisibility > 0f )
		{
			baseSkin += 4;
		}
		else if ( IsInvulnerable() )
		{
			baseSkin += 2;
		}

		SetMaterialGroup( baseSkin );
	}

	public void SwitchOffEmptyWeapon()
	{
		var shouldSwitch = true;

		if ( ActiveWeapon.IsValid() )
		{
			// If weapon has enough ammo, no need to switch.
			if ( ActiveWeapon.HasAmmo() )
				shouldSwitch = false;

			// If weapon has ammo in reserve
			if ( ActiveWeapon.Reserve > 0 )
				shouldSwitch = false;

			if ( ActiveWeapon.NextPrimaryAttackTime > Time.Now )
				shouldSwitch = false;
		}

		if ( shouldSwitch )
		{
			SwitchToNextBestWeapon();
		}
	}

	public override void Touch( Entity other )
	{
		TFPlayer player = other as TFPlayer;

		if ( player != null )
			CheckTouchingSpies( player );

		base.Touch( other );
	}

	public void CheckTouchingSpies( TFPlayer player )
	{
		// If he is on your team, don't reveal the spy
		if ( player.Team == Team )
			return;

		// If he isn't cloaked, don't do this.
		if ( !InCondition( TFCondition.Cloaked ) )
			return;

		// Apply spy touched effects.
		player.OnSpyTouchedWhileCloaked();
	}

	private void DropAmmoPack( Vector3 force )
	{
		var pack = new AmmoPackMedium
		{
			Respawns = false,
			PlaybackRate = 0
		};

		pack.Position = WorldSpaceBounds.Center;
		pack.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		var velocity = force + Velocity;
		pack.Rotation = Rotation.LookAt( velocity );
		pack.ApplyAbsoluteImpulse( velocity );
	}
}
