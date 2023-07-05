using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;
[Library( "tf_building_dispenser" )]
[Title( "Dispenser" )]
[Category("Gameplay")]
public partial class Dispenser : TFBuilding
{
	protected virtual List<float> LevelHealing => new() { 10f, 15f, 20f };
	protected virtual List<float> LevelAmmo => new() { 0.2f, 0.3f, 0.4f };
	protected virtual List<int> LevelMetal => new() { 40, 50, 60 };
 	protected virtual Vector3 TriggerMins => new( -70, -70, 0 );
	protected virtual Vector3 TriggerMaxs => new( 70, 70, 50 );
	protected virtual int StartingMetal => 25;
	[Net] public DispenserZone Trigger { get; set; }
	public override void Spawn()
	{
		base.Spawn();

		Trigger = new();
		Trigger.SetParent(this);
		Trigger.StoredMetal = StartingMetal;
		Trigger.SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, TriggerMins, TriggerMaxs );
		Trigger.Disable();
	}
	public override void SetOwner( TFPlayer owner )
	{
		if ( Game.IsClient ) return;

		base.SetOwner( owner );
		Trigger.Team = owner.Team;
	}
	public override void Tick()
	{
		if( Trigger.Enabled && (IsCarried || IsUpgrading || IsConstructing))
		{
			Trigger.Disable();
		}

		base.Tick();
	}
	public override void TickActive()
	{
		if ( !Trigger.Enabled )
			Trigger.Enable();
	}

	public override void SetLevel( int level )
	{
		base.SetLevel( level );

		Trigger.HealingPerSecond = LevelHealing.ElementAtOrDefault( level-1 );
		Trigger.AmmoPercentagePerSecond = LevelAmmo.ElementAtOrDefault( level-1 );
		Trigger.MetalPerInterval = LevelMetal.ElementAtOrDefault( level-1 );
	}

	protected virtual string HealOriginAttachment => "heal_origin";
	public override void InitializeModel( string name )
	{
		base.InitializeModel( name );
		Trigger.HealOrigin = GetAttachment( HealOriginAttachment, false )?.Position ?? Vector3.Zero;
	}
	protected BuildingInfoLine StoredMetalLine;
	protected override void InitializeUI( BuildingData data )
	{
		base.InitializeUI( data );
		StoredMetalLine = new( 0, 0, Trigger.MaxStoredMetal, "UI/Hud/Buildings/hud_obj_status_ammo_64.png" );
	}
	protected Particles damageParticles;
	protected override void OnDamageLevelChanged( TFBuildingDamageLevel from, TFBuildingDamageLevel to )
	{
		const string DAMAGE_LIGHT_FX = "particles/buildingdamage/dispenserdamage_1.vpcf";
		const string DAMAGE_MEDIUM_FX = "particles/buildingdamage/dispenserdamage_2.vpcf";
		const string DAMAGE_HEAVY_FX = "particles/buildingdamage/dispenserdamage_3.vpcf";
		const string DAMAGE_CRITICAL_FX = "particles/buildingdamage/dispenserdamage_4.vpcf";

		base.OnDamageLevelChanged( from, to );

		if ( damageParticles != default )
		{
			damageParticles.Destroy( true );
			damageParticles = null;
		}

		string damageParticleName = to switch
		{
			TFBuildingDamageLevel.Light => DAMAGE_LIGHT_FX,
			TFBuildingDamageLevel.Medium => DAMAGE_MEDIUM_FX,
			TFBuildingDamageLevel.Heavy => DAMAGE_HEAVY_FX,
			TFBuildingDamageLevel.Critical => DAMAGE_CRITICAL_FX,
			_ => ""
		};

		if ( !string.IsNullOrEmpty( damageParticleName ) )
		{
			damageParticles = Particles.Create( damageParticleName, this );
		}
	}
	public override void TickUI()
	{
		if (!IsInitialized) return;
		base.TickUI();

		StoredMetalLine.Value = Trigger.StoredMetal;
	}

	public override IEnumerable<BuildingInfoLine> GetUILines()
	{
		yield return StoredMetalLine;
		yield return UpgradeMetalLine;
	}

	protected override void Debug()
	{
		base.Debug();
		DebugOverlay.Box( Trigger, Color.Yellow.Darken(0.2f) );

		Vector3 pos = Position + Vector3.Up * CollisionBounds.Maxs.z;
		DebugOverlay.Text( $"[DISPENSER]", pos, 13, Color.White );
		DebugOverlay.Text( $"= Trigger: {Trigger}", pos, 14, Color.Yellow );
	}

}
