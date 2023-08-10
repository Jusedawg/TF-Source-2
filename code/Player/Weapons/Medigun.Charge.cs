using Sandbox;
using System;
using System.Collections.Generic;
using Amper.FPS;

namespace TFS2;

public partial class Medigun 
{
	[Net] public float ChargeLevel { get; set; }
	[Net] public bool IsReleasingCharge { get; set; }
	Dictionary<TFPlayer, float> DetachTimes { get; set; } = new();
	public bool IsCharged => ChargeLevel >= 1;

	[ConVar.Replicated] public static float tf_medigun_ubercharge_build_time { get; set; } = 40;
	[ConVar.Replicated] public static float tf_medigun_ubercharge_drain_time { get; set; } = 8;

	float NextDenySecondaryTime { get; set; }
	public override void SecondaryAttack()
	{
		var player = TFOwner;
		if ( !player.IsValid() )
			return;

		if ( !CanReleaseCharge() )
		{
			if ( IsLocalPawn && Time.Now >= NextDenySecondaryTime )
			{
				PlaySound( ReleaseFailSoundEffect );
				NextDenySecondaryTime = Time.Now + .5f;
			}

			return;
		}

		IsReleasingCharge = true;
		OnReleasedCharge();
	}

	public override void SimulateSecondaryAttack()
	{
		base.SimulateSecondaryAttack();

		if ( IsReleasingCharge )
			SimulateCharge();
	}

	public bool CanReleaseCharge()
	{
		if ( IsReleasingCharge )
			return false;

		if ( !IsCharged )
			return false;

		return true;
	}

	public float GetMinChargeAmount() => 1;

	public void SimulateCharge()
	{
		//Moved to Player UberCharge code so it ticks while weapon isn't active;
		//DrainCharge();
	}

	public void BuildCharge()
	{
		if ( !Patient.IsValid() )
			return;

		// build up ubercharge while we're healing.
		float chargeAmount = Time.Delta / tf_medigun_ubercharge_build_time;
		if ( chargeAmount <= 0 )
			return;

		var boostMax = MathF.Floor( Patient.GetMaxOverheal() * 0.95f );
		var chargeModifier = 1f;
		// Reduce healing for overhealed guys
		if ( Patient.Health >= boostMax )
			chargeModifier *= .5f;

		// If entity has multiple healers. Share uber between all of them.
		if ( Patient.Healers != null && Patient.Healers.Count > 1 )
			chargeModifier /= Patient.Healers.Count;

		// Check if we want to force fast buildup regardless of previous conditions.
		// We can go faster than normal, but we can't go slower.
		bool doPreRoundBoost = SDKGame.Current.State == GameState.PreRound || TFGameRules.Current.IsInSetup;
		if ( doPreRoundBoost )
		{
			const float SETUP_ÜBER_GAIN_MULTIPLIER = 3;
			chargeModifier *= SETUP_ÜBER_GAIN_MULTIPLIER;
		}

		chargeAmount *= chargeModifier;

		var newCharge = Math.Min( ChargeLevel + chargeAmount, 1 );
		var minChargeAmount = GetMinChargeAmount();

		var builtUberThisTick = newCharge >= minChargeAmount && ChargeLevel < minChargeAmount;
		ChargeLevel = newCharge;

		if ( builtUberThisTick )
			OnBuiltCharge();
	}

	public void DrainCharge()
	{
		if ( !IsReleasingCharge )
			return;

		float drainTime = tf_medigun_ubercharge_drain_time;
		float fDrain = Time.Delta / drainTime;
		var flExtraPlayerCost = fDrain * .5f;

		var i = 0;
		foreach ( var pair in DetachTimes )
		{
			var player = pair.Key;
			var time = pair.Value;

			if ( !IsValidTarget( player ) )
				continue;

			if ( time < (Time.Now - TFPlayer.tf_invulnerability_wearoff_time) )
				continue;

			i++;
			fDrain += flExtraPlayerCost;
		}

		SubtractChargeAndUpdateDeployState( fDrain, false );
	}

	public void SubtractChargeAndUpdateDeployState( float subtractAmount, bool forceDrain )
	{
		var player = TFOwner;
		if ( !player.IsValid() )
			return;

		var newCharge = MathF.Max( ChargeLevel - subtractAmount, 0 );
		ChargeLevel = newCharge;

		if ( ChargeLevel == 0 ) 
			OnDrainedCharge();
	}

	public void OnBuiltCharge()
	{
	}

	public void OnDrainedCharge()
	{
		IsReleasingCharge = false;
		DetachTimes.Clear();
	}

	public void OnReleasedCharge() 
	{
		TFOwner.Invulns++;
	}

	public virtual TFCondition GetChargeType() => TFCondition.Invulnerable;

#if DEBUG
	[ConCmd.Admin( "tf_medigun_set_charge" )]
	public static void Command_SetCharge( float value )
	{
		var player = ConsoleSystem.Caller.Pawn as TFPlayer;
		if ( !player.IsValid() )
			return;

		var medigun = player.GetWeaponOfType<Medigun>();
		if ( !medigun.IsValid() )
			return;

		medigun.ChargeLevel = Math.Clamp( value, 0, 1 );
	}
#endif
}
