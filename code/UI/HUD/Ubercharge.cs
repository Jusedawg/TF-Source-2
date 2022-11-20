using Sandbox;
using Sandbox.UI;
using System;

namespace TFS2.UI;

[UseTemplate]
partial class Ubercharge : Panel
{
	public Label UberLabel { get; set; }
	public Panel Bar { get; set; }
	TimeSince TimeSinceFlashCycle { get; set; }

	public Ubercharge()
	{
		BindClass( "red", () => TFPlayer.LocalPlayer.Team == TFTeam.Red );
		BindClass( "blue", () => TFPlayer.LocalPlayer.Team == TFTeam.Blue );
	}

	public override void Tick()
	{
		var player = TFPlayer.LocalPlayer;
		if ( player == null )
			return;

		SetClass( "visible", ShouldDraw() );
		if ( !IsVisible )
			return;

		var medigun = player.ActiveWeapon as Medigun;
		if ( medigun == null )
			return;

		UberLabel.Text = $"ÜberCharge: {MathF.Floor( medigun.ChargeLevel * 100 )}%";
		Bar.Style.Width = Length.Fraction( medigun.ChargeLevel );

		var highlighted = false;
		var charged = medigun.ChargeLevel >= 1;

		if ( charged )
		{
			if ( TimeSinceFlashCycle > 0.6f ) TimeSinceFlashCycle = 0;
			if ( TimeSinceFlashCycle <= 0.3f ) highlighted = true;
		}

		SetClass( "highlight", highlighted );
		SetClass( "charged", charged );
	}

	public bool ShouldDraw()
	{
		var player = TFPlayer.LocalPlayer;
		if ( player == null )
			return false;

		if ( Input.Down( InputButton.Score ) )
			return false;

		if ( !player.IsAlive )
			return false;

		return player.ActiveWeapon is Medigun;
	}
}
