using Amper.FPS;
using Sandbox;
using System;

namespace TFS2;

partial class TFPlayer
{
	[ConVar.Server] public static float tf_spy_stealth_blink_scale { get; set; } = 0.85f;
	[ConVar.Replicated] public static float tf_spy_invis_time { get; set; } = 1.0f;
	[ConVar.Replicated] public static float tf_spy_invis_unstealth_time { get; set; } = 2.0f;
	[ConVar.Replicated] public static float tf_spy_cloak_consume_rate { get; set; } = 10.0f;
	[ConVar.Replicated] public static float tf_spy_cloak_regen_rate { get; set; } = 3.3f;
	[ConVar.Replicated] public static float tf_spy_cloak_no_attack_time { get; set; } = 2.0f;

	[Net, Predicted] public float Invisibility { get; set; } = 0.0f;
	[Net] public float InvisChangeCompleteTime { get; set; } = 0.0f;
	[Net] public float CloakNoAttackExpire { get; set; } = 0.0f;
	[Net] public float CloakNextChangeTime { get; set; } = 0.0f;
	[Net] public float LastCloakExposeTime { get; set; } = 0.0f;

	public bool CanGoInvisible()
	{
		// If you have the flag, you cannot go invisible.
		if ( PickedItem != null && PickedItem is Flag )
			return false;

		// If the round has ended and you're not a winner, you can't go invisible.
		if ( TFGameRules.Current.IsRoundEnded && TFGameRules.Current.Winner != TeamNumber )
			return false;

		// Go Invisible.
		return true;
	}

	public void FadeInvis( float InvisFadeTime )
	{
		RemoveCondition( TFCondition.Cloaked );

		if ( InvisFadeTime > 0.15f )
		{
			//Not sure what was meant to go here
		}
		else
		{
			CloakNoAttackExpire = Time.Now + tf_spy_cloak_no_attack_time;
		}

		InvisChangeCompleteTime = Time.Now + InvisFadeTime;
	}

	public void OnSpyTouchedWhileCloaked()
	{
		LastCloakExposeTime = Time.Now;
		AddCondition( TFCondition.CloakedBlink );
	}

	private void TickInvisibility()
	{
		//Skip invis tick if condition is removed + no invisbility to show
		if ( !InCondition( TFCondition.Cloaked ) && Invisibility == 0f ) return;

		float TargetInvis;
		if ( InCondition( TFCondition.CloakedBlink ) )
		{
			// We were bumped into or hit for some damage.
			TargetInvis = 0.5f;
		}
		else
		{
			// Go invisible or appear.
			if ( InvisChangeCompleteTime > Time.Now )
			{
				float timeLeft = Math.Max( 0f, InvisChangeCompleteTime - Time.Now );
				if ( InCondition( TFCondition.Cloaked ) )
				{
					TargetInvis = 1f - (timeLeft / tf_spy_invis_time);
				}
				else
				{
					TargetInvis = timeLeft / tf_spy_invis_time;
				}
				TargetInvis = MathX.Clamp( TargetInvis, 0.0f, 1.0f );
			}
			else
			{
				TargetInvis = InCondition( TFCondition.Cloaked ) ? 1f : 0f;
			}
		}

		Invisibility = TargetInvis;
		UpdateMaterialGroup();

		if ( IsClient )
		{
			//Apply visual changes on client
			float invisBlend = 1f - Invisibility;
			//If this is another player but teams match ours, prevent player from being
			if ( !IsLocalPawn && LocalPlayer.Team == Team )
			{
				invisBlend = Math.Max( 0.5f, invisBlend );
			}
			SceneObject.Attributes.Set( Invisibility_Attribute, invisBlend );

			//Apply invis attribute to view models, broken atm
			//Prevent view models from going fully invisible
			if ( IsLocalPawn )
			{
				GetViewModel().Weapon.SceneObject.Attributes.Set( Invisibility_Attribute, Math.Max( 0.5f, invisBlend ) );
			}
		}
	}
}
