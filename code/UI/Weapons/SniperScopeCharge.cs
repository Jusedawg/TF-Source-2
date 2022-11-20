using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

[UseTemplate]
partial class SniperScopeCharge : Panel
{
	Panel Lines { get; set; }
	Label Percentage { get; set; }

	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );

		if ( !IsVisible )
			return;

		if ( TFPlayer.LocalPlayer.ActiveWeapon is SniperRifle rifle )
		{
			var fraction = rifle.GetChargeFraction();
			var perc = fraction * 100;

			Lines.Style.Width = Length.Fraction( fraction );
			Percentage.Text = $"{perc.FloorToInt()}%";

			SetClass( "is_charged", fraction >= 1 );
		}
	}

	public bool ShouldDraw()
	{
		var player = TFPlayer.LocalPlayer;

		if ( !player.IsValid() )
			return false;

		return (player.ActiveWeapon as SniperRifle)?.IsZoomed ?? false;
	}
}
