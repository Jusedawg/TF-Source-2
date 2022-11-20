using Sandbox;
using System;

namespace TFS2;

public interface IChargeable
{
	public float ChargeMaxTime { get; }
	public bool IsCharging { get; set; }
	public TimeSince TimeSinceStartCharge { get; set; }

	public virtual float GetCurrentCharge()
	{
		if ( !IsCharging ) return 0;
		if ( ChargeMaxTime == 0 ) return 0;
		return Math.Clamp( TimeSinceStartCharge / ChargeMaxTime, 0, 1 );
	}

	public virtual bool IsCharged => IsCharging && GetCurrentCharge() >= 1;

	public virtual void StopCharging()
	{
		IsCharging = false;
		OnStopCharge();
	}

	public virtual void StartCharging()
	{
		IsCharging = true;
		TimeSinceStartCharge = 0;

		OnStartCharge();
	}

	public void OnStartCharge();
	public void OnStopCharge();
}
