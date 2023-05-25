using Sandbox;
using Amper.FPS;
using Sandbox.UI;
using System;

namespace TFS2.UI;

public partial class HealthCross : Panel
{
	const float DangerHealthFractionThreshold = 0.5f;
	public IHasMaxHealth DesiredTarget { get; set; }
	public float FlashScale { get; set; }
	public bool ShowMaxHealth { get; set; }
	IHasMaxHealth Target => DesiredTarget ?? TFPlayer.LocalPlayer;
	public HealthCross() : this( null ) { }

	public HealthCross( IHasMaxHealth target = null, float flashScale = 1, bool showMaxHealth = true )
	{
		DesiredTarget = target;
		FlashScale = flashScale;
		ShowMaxHealth = showMaxHealth;
	}

	public override void Tick()
	{
		if ( !Target.IsValid() )
			return;

		var health = Target.Health;
		var maxHealth = Target.MaxHealth;
		var fraction = Math.Clamp( health / maxHealth, 0, 1 );

		ProgressBar.Style.Height = Length.Fraction( fraction );
		HealthLabel.Text = MathF.Ceiling( health ).ToString();
		MaxHealthLabel.Text = MathF.Ceiling( maxHealth ).ToString();

		SetClass( "show_maxhealth", fraction < .95f && ShowMaxHealth );
		SetClass( "is_danger", fraction < DangerHealthFractionThreshold );

		SimulateDangerFlash();
		SimulateOverhealFlash();
	}

	const float DangerFlashTime = .25f;
	const float OverhealFlashTime = .50f;
	TimeSince TimeSinceDangerFlash { get; set; }
	TimeSince TimeSinceOverhealFlash { get; set; }

	public void SimulateDangerFlash()
	{
		//
		// Opacity
		//
		if ( TimeSinceDangerFlash > DangerFlashTime )
			TimeSinceDangerFlash = 0;

		DangerFlash.Style.Opacity = MathF.Sin( TimeSinceDangerFlash / DangerFlashTime * MathF.PI ) * 0.6f;

		//
		// Scale
		//
		var health = Target.Health;
		var maxHealth = Target.MaxHealth;
		var scale = 0f;
		var maxDangerHealth = maxHealth * DangerHealthFractionThreshold;

		if ( health <= maxDangerHealth )
		{
			scale = health.RemapClamped( maxDangerHealth, 0 );
			scale *= FlashScale;
			scale += 1;
		}

		DangerFlash.Style.Set( "transform", $"scale({scale})" );
	}

	public void SimulateOverhealFlash()
	{
		//
		// Opacity
		//
		if ( TimeSinceOverhealFlash > OverhealFlashTime )
			TimeSinceOverhealFlash = 0;

		OverhealFlash.Style.Opacity = 0.3f + MathF.Sin( TimeSinceOverhealFlash / OverhealFlashTime * MathF.PI ) * 0.3f;

		//
		// Scale
		//
		var health = Target.Health;
		var maxHealth = Target.MaxHealth;
		var maxOverheal = maxHealth * TFPlayer.tf_max_overheal;

		var scale = 0f;
		if ( health > maxHealth )
		{
			scale = health.RemapClamped( maxHealth, maxOverheal );
			scale *= FlashScale;
			scale += 1;
		}

		OverhealFlash.Style.Set( "transform", $"scale({scale})" );
	}
}
