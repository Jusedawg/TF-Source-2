using Sandbox;
using Amper.FPS;
using Sandbox.UI;
using System;
using static TFS2.TFPlayer;

namespace TFS2.UI;

public partial class HealthCross : Panel
{
	const float DangerHealthFractionThreshold = 0.5f;
	public float HealthAmount { get; set; }
	public float MaxHealthAmount { get; set; }
	public float FlashScale { get; set; }
	public IHasMaxHealth DesiredTarget { get; set; }
	public bool ShowMaxHealth { get; set; }
	public bool Local { get; set; } = false;

	Panel ProgressBar;
	Label HealthLabel;
	Label MaxHealthLabel;
	Panel DangerFlash;
	Panel OverhealFlash;
	public HealthCross() : this( null ) { }

	public HealthCross( IHasMaxHealth target = null, float flashScale = 1, bool showMaxHealth = true )
	{
		DesiredTarget = target;
		FlashScale = flashScale;
		ShowMaxHealth = showMaxHealth;
	}

	public override void Tick()
	{
		float health = GetHealth();
		float maxHealth = GetMaxHealth();
		var fraction = Math.Clamp( health / maxHealth, 0, 1 );

		ProgressBar.Style.Height = Length.Fraction( fraction );
		HealthLabel.Text = MathF.Ceiling( health ).ToString();
		MaxHealthLabel.Text = MathF.Ceiling( maxHealth ).ToString();

		SetClass( "show_maxhealth", fraction < .95f && ShowMaxHealth );
		SetClass( "is_danger", fraction < DangerHealthFractionThreshold );

		if ( FlashScale <= 0 ) return;

		SimulateDangerFlash();
		SimulateOverhealFlash();
	}

	public float GetHealth()
	{
		if ( DesiredTarget != default )
			return DesiredTarget.Health;
		else if ( Local )
			return Game.LocalPawn.Health;

		return HealthAmount;
	}
	public float GetMaxHealth()
	{
		if ( DesiredTarget != default )
			return DesiredTarget.MaxHealth;
		else if ( Local )
			return (Game.LocalPawn is IHasMaxHealth maxHealthPawn) ? maxHealthPawn.MaxHealth : Game.LocalPawn.Health;

		return MaxHealthAmount;
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
		var health = GetHealth();
		var maxHealth = GetMaxHealth();
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
		var health = GetHealth();
		var maxHealth = GetMaxHealth();
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
