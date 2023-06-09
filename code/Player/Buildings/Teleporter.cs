using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace TFS2;

[Library( "tf_building_teleporter" )]
public partial class Teleporter : TFBuilding
{
	[Net] public bool IsReady { get; set; }
	[Net] public Teleporter LinkedTeleporter { get; protected set; }
	[Net] public bool IsPaired { get; protected set; }
	protected virtual float InitialActivationTime => 0.2f;
	/// <summary>
	/// How long the animation takes to wind down after the teleporter is unlinked
	/// </summary>
	protected virtual float UnpairAnimationTime => 2f;
	[Net] protected float ReadyProgress { get; set; }
	[Net] protected float ReadyTime { get; set; }
	protected TimeSince timeSinceLinkedInactive;
	protected Particles LevelParticle;
	public override void TickActive()
	{
		if(IsPaired)
		{
			if(!LinkedTeleporter.IsValid())
			{
				UnLink();
				return;
			}

			if(LinkedTeleporter.IsConstructing)
			{
				return;
			}

			if ( !IsReady )
			{
				SetAnimParameter( "f_spin_speed", ReadyProgress );
				ReadyProgress += Time.Delta / ReadyTime;

				if ( ReadyProgress >= 1 )
				{
					Ready();
				}
			}
			else
			{
				TickReady();
			}
		}
		else
		{
			if ( timeSinceLinkedInactive < UnpairAnimationTime )
			{
				SetAnimParameter( "f_spin_speed", 1 - timeSinceLinkedInactive / UnpairAnimationTime );
			}
			else
				SetAnimParameter( "f_spin_speed", 0f );
		}
	}
	public virtual void TickReady()
	{
		TickReadyEffects();
	}

	protected virtual void TickReadyEffects()
	{
		
	}
	/// <summary>
	/// Links another teleporter to this one.
	/// </summary>
	/// <param name="tp">The teleporter to link to.</param>
	/// <param name="linkOther">Should we call this function on the teleporter passed? Should only be called once per TP/TP link</param>
	public virtual void Link(Teleporter tp, bool linkOther = true)
	{
		if ( LinkedTeleporter.IsValid() ) return;
		LinkedTeleporter = tp;
		IsPaired = true;
		ReadyProgress = 0;
		ReadyTime = InitialActivationTime;

		if ( linkOther )
			tp.Link( this, false );
	}
	public virtual void UnLink()
	{
		LinkedTeleporter = null;
		IsPaired = false;
		timeSinceLinkedInactive = 0;

		// Reset level
		RequestedLevel = 1;
		SetLevel( 1 );
	}
	public virtual void SyncState()
	{
		if ( LinkedTeleporter == null ) return;

		if ( IsReady )
			LinkedTeleporter.Ready( false );
		else
			LinkedTeleporter.UnReady( ReadyTime, false );

		//LinkedTeleporter.RequestedLevel = RequestedLevel;
		LinkedTeleporter.AppliedMetal = AppliedMetal;
		LinkedTeleporter.ReadyProgress = ReadyProgress;
		LinkedTeleporter.ReadyTime = ReadyTime;
		LinkedTeleporter.IsPaired = IsPaired;
	}
	public override void SetOwner( TFPlayer owner )
	{
		base.SetOwner( owner );
		var tp = owner.Buildings.OfType<Teleporter>()?.FirstOrDefault();
		if ( tp != null)
		{
			Link( tp );
			tp.SyncState();
		}
	}
	public virtual void Ready(bool sync = true )
	{
		IsReady = true;
		ReadyEffects();

		if(sync)
			SyncState();
	}

	public virtual void ReadyEffects()
	{
		if(LevelParticle != default)
			LevelParticle.EnableDrawing = true;
		SetBodyGroup( "blur", 1 );
		SetAnimParameter( "f_spin_speed", 1 );
	}

	public virtual void UnReady( float time, bool sync = true )
	{
		IsReady = false;
		ReadyProgress = 0;
		ReadyTime = time;
		UnReadyEffects();

		if(sync)
			SyncState();
	}

	public virtual void UnReadyEffects()
	{
		SetAnimParameter( "f_spin_speed", 0 );
	}

	const string PARTICLE_ATTACHMENT = "centre_attach2";
	public override void SetLevel( int level )
	{
		base.SetLevel( level );

		if(LevelParticle != default)
			LevelParticle.Destroy();
		LevelParticle = Particles.Create( GetLevelParticle(), this, PARTICLE_ATTACHMENT );
		LevelParticle.EnableDrawing = IsReady && IsPaired && !IsConstructing && !IsUpgrading;
	}
	protected virtual string GetLevelParticle()
	{
		if ( Team == TFTeam.Blue )
		{
			return Level switch
			{
				1 => "particles/teleport_status/teleporter_blue_exit_level1.vpcf",
				2 => "particles/teleport_status/teleporter_blue_exit_level2.vpcf",
				_ => "particles/teleport_status/teleporter_blue_exit_level3.vpcf"
			};
		}
		else
		{
			return Level switch
			{
				1 => "particles/teleport_status/teleporter_red_exit_level1.vpcf",
				2 => "particles/teleport_status/teleporter_red_exit_level2.vpcf",
				_ => "particles/teleport_status/teleporter_red_exit_level3.vpcf"
			};
		}
	}
	public override void FinishConstruction()
	{
		base.FinishConstruction();
		LevelParticle.EnableDrawing = IsReady && IsPaired;
	}

	public override void StartUpgrade( int level, float time = 0, bool setRequested = false )
	{
		base.StartUpgrade( level, time, setRequested );
		UnReady( time );

		if ( LevelParticle != default )
			LevelParticle.EnableDrawing = false;
		SetBodyGroup( "teleporter_blur", 0 );
	}
	public override void FinishUpgrade()
	{
		base.FinishUpgrade();
		Ready();
		LevelParticle.EnableDrawing = IsReady && IsPaired;
	}
	public override void StartCarrying()
	{
		base.StartCarrying();

		IsPaired = false;
		timeSinceLinkedInactive = 0;
		UnReady( 0 );

		if ( LevelParticle != default )
			LevelParticle.EnableDrawing = false;
		SetBodyGroup( "teleporter_blur", 0 );
	}

	public override void StopCarrying( Transform deployTransform )
	{
		base.StopCarrying( deployTransform );
		IsPaired = LinkedTeleporter.IsValid();
		SyncState();
	}

	public override int ApplyRepairMetal( int amount, float metalToRepair = 3, float repairPower = 1 )
	{
		int result = base.ApplyRepairMetal( amount, metalToRepair, repairPower );
		SyncState();
		return result;
	}

	public override int ApplyUpgradeMetal( int amount )
	{
		int result = base.ApplyUpgradeMetal( amount );
		SyncState();
		return result;
	}

	protected override void Debug()
	{
		base.Debug();
		Vector3 pos = Position + Vector3.Up * CollisionBounds.Maxs.z;
		DebugOverlay.Text( $"[TELEPORTER]", pos, 13, Color.White );
		DebugOverlay.Text( $"= LinkedTeleporter: {LinkedTeleporter}", pos, 14, Color.Yellow );
		DebugOverlay.Text( $"= IsPaired: {IsPaired}", pos, 15, Color.Yellow );
		DebugOverlay.Text( $"= IsReady: {IsReady}", pos, 16, Color.Yellow );
		DebugOverlay.Text( $"= Ready Progress: {ReadyProgress}/{ReadyTime}", pos, 17, Color.Yellow );
	}
}
