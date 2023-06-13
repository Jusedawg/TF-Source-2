using Amper.FPS;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFS2.UI;

namespace TFS2;

[Library( "tf_building_sentry" )]
[Title( "Sentry Gun" )]
[Category( "Gameplay" )]
public partial class SentryGun : TFBuilding, IKillfeedIcon
{
	public override Ray AimRay => new( Position + GetAimOffset(), AimRotation.Forward);
	[Net] public int PrimaryAmmo { get; set; }
	[Net] public int SecondaryAmmo { get; set; }
	[Net] public int KillAmount { get; set; }
	[Net] public int AssistAmount { get; set; }
	public bool HasPrimaryAmmo => PrimaryAmmo > 0;
	public bool HasSecondaryAmmo => SecondaryAmmo > 0;
	protected virtual List<int> LevelMaxPrimaryAmmo => new() { 150, 200, 200 };
	protected virtual List<int> LevelMaxSecondaryAmmo => new() { 0, 0, 20 };
	protected virtual List<Vector3> LevelAimOffsets => new() { new( 0, 0, 32 ), new( 0, 0, 40 ), new( 0, 0, 46 ) };
	protected virtual int GetMaxPrimaryAmmo() => LevelMaxPrimaryAmmo.ElementAtOrDefault( Level - 1 );
	protected virtual int GetMaxSecondaryAmmo() => LevelMaxSecondaryAmmo.ElementAtOrDefault( Level - 1 );
	protected virtual Vector3 GetAimOffset() => LevelAimOffsets.ElementAtOrDefault( Level - 1 );
	protected override float RedeploySpeedMultiplier => 3f;
	public SentryGun()
	{
		EventDispatcher.Subscribe<PlayerDeathEvent>( OnPlayerDeath, this );
	}

	public override void Initialize( BuildingData data )
	{
		base.Initialize( data );

		PrimaryAmmo = GetMaxPrimaryAmmo();
		SecondaryAmmo = GetMaxSecondaryAmmo();
	}
	public override void StopCarrying( Transform deployTransform )
	{
		base.StopCarrying( deployTransform );

		AimRotation = Rotation;
		AimRotationTarget = Rotation;

		var currentAng = Rotation.Angles().Clamped();
		LeftIdleYaw = currentAng.yaw + 50;
		RightIdleYaw = currentAng.yaw - 50;

		currentTurnRate = TurnRate;
	}
	public override void TickActive()
	{
		FindTarget();
		RotateToTarget();

		if(HasTarget)
		{
			TickFire();
		}

		var pitchOffset = Rotation.Pitch() - AimRotation.Pitch();
		var yawOffset = Rotation.Yaw() - AimRotation.Yaw();
		if ( yawOffset > 180 ) yawOffset -= 360;
		else if ( yawOffset < -180 ) yawOffset += 360;

		SetAnimParameter( "aim_pitch", pitchOffset );
		SetAnimParameter( "aim_yaw", yawOffset );
	}
	protected virtual int MetalPerPrimaryAmmo => 1;
	protected virtual int PrimaryAmmoAdded => 40;
	protected virtual int MetalPerSecondaryAmmo => 2;
	protected virtual int SecondaryAmmoAdded => 8;
	public override int ApplyMetal( int metalCount, float metalToRepair = 3, float repairPower = 1 )
	{
		int repairMetal = ApplyRepairMetal( metalCount, metalToRepair, repairPower );
		if ( repairMetal != 0 )
			return repairMetal;

		int requestedUpgradeMetal = 25;
		if ( TFGameRules.Current.IsInSetup )
			requestedUpgradeMetal *= 2;

		requestedUpgradeMetal = (int)MathF.Min( metalCount, requestedUpgradeMetal );
		int upgradeMetal = ApplyUpgradeMetal( requestedUpgradeMetal );
		int ammoMetal = 0;

		int maxPrimary = GetMaxPrimaryAmmo();
		// If we have metal, try to add primary ammo
		if( PrimaryAmmo < maxPrimary && metalCount - upgradeMetal > 0)
		{
			int primaryAmmoGain = metalCount / MetalPerPrimaryAmmo; // Maximum possible gain
			primaryAmmoGain = (int)MathF.Min( primaryAmmoGain, PrimaryAmmoAdded ); // Limit to PrimaryAmmoAdded
			primaryAmmoGain = (int)MathF.Min( primaryAmmoGain, maxPrimary - PrimaryAmmo );

			PrimaryAmmo += primaryAmmoGain;
			ammoMetal += primaryAmmoGain * MetalPerPrimaryAmmo;
		}

		int maxSecondary = GetMaxSecondaryAmmo();
		// If we still have metal, try to add secondary ammo
		if ( SecondaryAmmo < maxSecondary && metalCount - upgradeMetal > 0 )
		{
			int secondaryAmmoGain = metalCount / MetalPerSecondaryAmmo; // Maximum possible gain
			secondaryAmmoGain = (int)MathF.Min( secondaryAmmoGain, SecondaryAmmoAdded ); // Limit to SecondaryAmmoAdded
			secondaryAmmoGain = (int)MathF.Min( secondaryAmmoGain, maxSecondary - SecondaryAmmo );


			SecondaryAmmo += secondaryAmmoGain;
			ammoMetal += secondaryAmmoGain * MetalPerSecondaryAmmo;
		}

		return upgradeMetal + ammoMetal;
	}
	public override void SetLevel( int level )
	{
		if ( Game.IsClient ) return;

		int maxPrimary = GetMaxPrimaryAmmo();
		int maxSecondary = GetMaxSecondaryAmmo();

		base.SetLevel( level );

		if(PrimaryAmmo == maxPrimary)
		{
			PrimaryAmmo = GetMaxPrimaryAmmo();
		}
		
		if(SecondaryAmmo == maxSecondary)
		{
			SecondaryAmmo = GetMaxSecondaryAmmo();
		}
	}
	public override void StartConstruction( float time = 0 )
	{
		base.StartConstruction( time );

		SetAnimParameter( "b_build", true );
	}
	private void OnPlayerDeath( PlayerDeathEvent ev )
	{
		if ( ev.Inflictor == this )
			KillAmount++;
	}
	protected BuildingInfoLine KillAssistLine;
	protected BuildingInfoLine PrimaryAmmoLine;
	protected BuildingInfoLine SecondaryAmmoLine;
	protected override void InitializeUI( BuildingData data )
	{
		base.InitializeUI( data );
		KillAssistLine = new( $"0 (0)", "/UI/Hud/Buildings/hud_obj_status_kill_64.png" );
		PrimaryAmmoLine = new( PrimaryAmmo, 0, GetMaxPrimaryAmmo(), "UI/Hud/Buildings/hud_obj_status_ammo_64.png" );
		SecondaryAmmoLine = new( SecondaryAmmo, 0, GetMaxSecondaryAmmo(), "UI/Hud/Buildings/hud_obj_status_rockets_64.png" );
	}
	public override void TickUI()
	{
		base.TickUI();

		KillAssistLine.Text = $"{KillAmount} ({AssistAmount})";

		PrimaryAmmoLine.Value = PrimaryAmmo;
		PrimaryAmmoLine.MaxValue = GetMaxPrimaryAmmo();

		SecondaryAmmoLine.Value = SecondaryAmmo;
		SecondaryAmmoLine.MaxValue = GetMaxSecondaryAmmo();
		SecondaryAmmoLine.Visible = Level == MaxLevel;
	}

	public override IEnumerable<BuildingInfoLine> GetUILines()
	{
		yield return KillAssistLine;
		yield return PrimaryAmmoLine;
		yield return SecondaryAmmoLine;
		yield return UpgradeMetalLine;
	}

	string IKillfeedIcon.GetIcon(bool isCrit, string[] tags)
	{
		return $"ui/deathnotice/sentry{Level}.png";
	}

	protected override void Debug()
	{
		base.Debug();

		DebugOverlay.Sphere( AimRay.Position, Range, Color.Yellow.Darken( 0.2f ) );
		DebugOverlay.Line( AimRay.Position, AimRay.Position + AimRotationTarget.Forward.Normal * Range, Color.Orange );
		DebugOverlay.Line( AimRay.Position, AimRay.Position + Rotation.Forward.Normal * Range, Color.White );

		Vector3 pos = Position + Vector3.Up * CollisionBounds.Maxs.z;
		DebugOverlay.Text( $"[SENTRY]", pos, 13, Color.White );
		DebugOverlay.Text( $"= Target: {Target}", pos, 14, Color.Yellow );
		DebugOverlay.Text( $"= AimRotation: {AimRotation.Angles()}", pos, 15, Color.Yellow );
		DebugOverlay.Text( $"= AimRotationTarget: {AimRotationTarget.Angles()}", pos, 16, Color.Yellow );
		DebugOverlay.Text( $"= Ammo: {PrimaryAmmo}/{SecondaryAmmo}", pos, 17, Color.Yellow );
		DebugOverlay.Text( $"= MaxAmmo: {GetMaxPrimaryAmmo()}/{GetMaxSecondaryAmmo()}", pos, 18, Color.Yellow );

		if ( HasTarget )
		{
			DebugOverlay.Sphere( GetTargetAimPosition(), 32f, Color.Red );
			DebugOverlay.Line( AimRay.Position, GetTargetAimPosition(), Color.Red );
		}
	}
}
