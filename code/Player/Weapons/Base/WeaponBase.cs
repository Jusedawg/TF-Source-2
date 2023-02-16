using Sandbox;
using Amper.FPS;

namespace TFS2;

/// <summary>
/// Base Team Fortress weapon 
/// </summary>
public abstract partial class TFWeaponBase : SDKWeapon, IUse
{
	[Net] public WeaponData Data { get; set; }

	//
	// Owner Data
	// 
	[Net] public TFHoldPose HoldPose { get; set; }
	[Net] public int MaxReserve { get; set; }
	[Net] public bool AttachToHands { get; set; }
	public TFTeam Team => (TFTeam)TeamNumber;

	public bool IsInitialized => Data != null;
	public TFWeaponSlot Slot => (TFWeaponSlot)SlotNumber;
	public TFPlayer TFOwner => Owner as TFPlayer;

	public override bool CanAttack()
	{
		if ( TFOwner.InCondition( TFCondition.Cloaked ) || TFOwner.InCondition( TFCondition.Taunting ) )
			return false;

		return base.CanAttack();
	}

	public override bool CanReload()
	{
		if ( TFOwner.InCondition( TFCondition.Cloaked ) || TFOwner.InCondition( TFCondition.Taunting ) )
			return false;

		return base.CanReload();
	}

	public override bool CanDeploy( SDKPlayer player )
	{
		if ( !HasAmmo() && Reserve <= 0 )
			return false;

		return base.CanDeploy( player );
	}

	public void Initialize( WeaponData data )
	{
		Data = data;

		SetModel( Data.WorldModel );
		Clip = Data.ClipSize;
		EnableShadowInFirstPerson = false;
	}

	public override void OnEquip( SDKPlayer owner )
	{
		base.OnEquip( owner );
		DroppedAutoDestroyTime = null;

		if ( owner is not TFPlayer player )
			return;

		if ( !Data.TryGetOwnerDataForPlayerClass( player.PlayerClass, out var ownerData ) )
			return;

		OnEquippedByNewOwner( player, ownerData );
	}

	public void OnEquippedByNewOwner( TFPlayer player, WeaponOwnerData ownerData )
	{
		// Put the item in the correct slot.
		SlotNumber = (int)ownerData.Slot;
		MaxReserve = ownerData.Reserve;
		HoldPose = ownerData.HoldPose;
		AttachToHands = ownerData.AttachToHands;

		// Change the material group.
		SetMaterialGroup( player.Team == TFTeam.Red ? 0 : 1 );
	}

	public override void PlayAttackSound()
	{
		PlaySound( IsCurrentAttackCritical ? Data.SoundCrit : Data.SoundSingle );
	}

	[ConVar.Server] public static float tf_dropped_weapon_lifetime { get; set; } = 30;

	public float? DroppedAutoDestroyTime { get; set; }

	public override void OnDrop( SDKPlayer owner )
	{
		base.OnDrop( owner );
		DroppedAutoDestroyTime = Time.Now + tf_dropped_weapon_lifetime;
	}

	public override void DoRecoil()
	{
		var player = TFOwner;
		if ( !player.IsValid() )
			return;

		var punch = player.ViewPunchAngle;
		punch.x -= Data.PunchAngle;
		player.ViewPunchAngle = punch;
	}

	public override void ServerTick()
	{
		if ( Owner.IsValid() )
			return;

		if ( DroppedAutoDestroyTime.HasValue )
		{
			if ( DroppedAutoDestroyTime.Value <= Time.Now )
				Delete();
		}
	}

	public virtual void ModifyOwnerMaxSpeed( ref float speed ) { }

	public override void ApplyDamageModifications( Entity victim, ref ExtendedDamageInfo info, TraceResult trace )
	{
		//if ( IsCurrentAttackCritical )
		//	info = info.WithTag( TFDamageTags.Critical );

		//
		// Whether a weapon's damage is subject to rampup or falloff is defined in the weapon's data asset.
		//

		if ( Data.UseFalloff )
			info = info.WithTag( TFDamageTags.UseFalloff );

		if ( Data.UseRampup )
			info = info.WithTag( TFDamageTags.UseRampup );
	}

	public override void PrimaryAttack()
	{
		CalculateIsAttackCritical();
		base.PrimaryAttack();
	}

	public virtual bool ShowAmmoOnHud() => true;

	public bool OnUse( Entity user )
	{
		if ( user is TFPlayer player )
			player.EquipWeapon( this, true );

		return false;
	}

	public bool IsUsable( Entity user )
	{
		// Can't be used if we already have an owner.
		if ( Owner.IsValid() )
			return false;

		// Can't be used if it runs out of ammo.
		if ( Clip <= 0 && Reserve <= 0 && NeedsAmmo() )
			return false;

		if ( user is TFPlayer player )
		{
			var pclass = player.PlayerClass;
			return Data.CanBeOwnedByPlayerClass( pclass );
		}

		return false;
	}

	public override void OnHitEntity( Entity entity, TraceResult tr )
	{
		// HACK: Don't play surface impact effects on players.
		if ( entity is TFPlayer )
			return;

		base.OnHitEntity( entity, tr );
	}

	public override bool ShouldAutoReload()
	{
		if(Owner?.Client?.GetClientData<bool>( "cl_autoreload" ) == true)
		{
			return true;
		}

		return base.ShouldAutoReload();
	}

	#region ViewModel
	public override void SetupAnimParameters()
	{
		base.SetupAnimParameters();
		SendPlayerAnimParameter( "weapon_slot", (int)HoldPose );
	}
	public override void SendAnimParametersOnAttack()
	{
		SendAnimParameter( "b_fire" );
	}
	public override void SendAnimParametersOnReloadStart()
	{
		SendAnimParameter( "b_reload", true );
	}
	public override void SendAnimParametersOnReloadStop()
	{
		SendAnimParameter( "b_reload", false );
	}

	public virtual void OnSwitchedViewMode( bool is_first_person ) { }

	public override string GetViewModelPath()
	{
		if ( !Data.TryGetOwnerDataForPlayerClass( TFOwner.PlayerClass, out var ownerData ) )
			return "";

		return ownerData.ViewModel;
	}
	#endregion

	/*
	public override void SetupProjectile( Projectile ent, Vector3 origin, Vector3 velocity, float damage, DamageFlags flags )
	{
		base.SetupProjectile( ent, origin, velocity, damage, flags );

		if ( IsCurrentAttackCritical )
			ent.DamageFlags |= TFDamageFlags.Critical;

		if ( Data.UseFalloff )
			ent.DamageFlags |= TFDamageFlags.UseFalloff;

		if ( Data.UseRampup )
			ent.DamageFlags |= TFDamageFlags.UseRampup;
	}
	*/
}

/// <summary>
/// Slots for weapons to occupy
/// </summary>
public enum TFWeaponSlot
{
	Primary = 0,
	Secondary,
	Melee,
	PDA,
	PDA2,
	Action
}

/// <summary>
/// Weapon hold poses for animgraph.
/// </summary>
public enum TFHoldPose
{
	Primary,
	Secondary,
	Melee,
	PDA,
	PDA2,
	Action,
	AllClass,
	Item1,
	Item2,
	Item3,
	Item4,
	Item5,
	Item6,
	Item7,
	Item8,
	Item9,

	// Don't change the order of the items in this list, 
	// otherwise it will break animgraphs. Always add new elements in the end.
}
