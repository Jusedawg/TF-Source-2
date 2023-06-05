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
		IsPaired = false;
		timeSinceLinkedInactive = 0;

		// Reset level
		RequestedLevel = 1;
		SetLevel( 1 );
	}
	public virtual void SyncState()
	{
		if ( LinkedTeleporter == null ) return;

		LinkedTeleporter.IsReady = IsReady;
		LinkedTeleporter.RequestedLevel = RequestedLevel;
		LinkedTeleporter.AppliedMetal = AppliedMetal;
		LinkedTeleporter.ReadyProgress = ReadyProgress;
		LinkedTeleporter.ReadyTime = ReadyTime;
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
	public virtual void Ready()
	{
		IsReady = true;
		SyncState();

		ReadyEffects();
	}

	public virtual void ReadyEffects()
	{
		//SetBodyGroup( "direction", IsPaired ? 1 : 0 );
		SetBodyGroup( "blur", 1 );
		SetAnimParameter( "f_spin_speed", 1 );
	}

	public virtual void UnReady( float time )
	{
		IsReady = false;
		ReadyProgress = 0;
		ReadyTime = time;
		SyncState();

		UnReadyEffects();
	}

	public virtual void UnReadyEffects()
	{
		SetBodyGroup( "blur", 0 );
		SetAnimParameter( "f_spin_speed", 0 );
	}

	public override int ApplyRepairMetal( int amount, float metalToRepair = 3, float repairPower = 1 )
	{
		int result = base.ApplyRepairMetal( amount, metalToRepair, repairPower );
		SyncState();
		return result;
	}

	public override int ApplyUpgradeMetal( int amount )
	{
		int result =  base.ApplyUpgradeMetal( amount );
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
