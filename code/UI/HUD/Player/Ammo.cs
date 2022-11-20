using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

[UseTemplate]
partial class Ammo : Panel
{
	public Label ClipSize { get; set; }
	public Label MaxAmmo { get; set; }
	public Panel Charge { get; set; }
	public Panel ChargeBar { get; set; }

	public Ammo()
	{
		BindClass( "red", () => TFPlayer.LocalPlayer.Team == TFTeam.Red );
		BindClass( "blue", () => TFPlayer.LocalPlayer.Team == TFTeam.Blue );
	}

	public bool ShouldDraw()
	{
		if ( Input.Down( InputButton.Score ) )
			return false;

		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() )
			return false;

		if ( !player.IsAlive )
			return false;

		var weapon = player.ActiveWeapon as TFWeaponBase;
		if ( !weapon.IsValid() )
			return false;

		return weapon.ShowAmmoOnHud();
	}

	public override void Tick()
	{
		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() ) return;

		SetClass( "hidden", !ShouldDraw() );
		if ( !IsVisible ) return;

		var weapon = player.ActiveWeapon as TFWeaponBase;
		if ( !weapon.IsValid() ) return;

		ClipSize.Text = weapon.Clip.ToString();
		MaxAmmo.Text = weapon.Reserve.ToString();

		SetClass( "has_reserve", weapon.MaxReserve > 0 );

		//
		// Charge Meter
		//

		var hasCharge = false;
		if ( weapon is IChargeable charge )
		{
			hasCharge = true;
			ChargeBar.Style.Width = Length.Fraction( charge.GetCurrentCharge() );
		}

		SetClass( "has_charge", hasCharge );
	}
}
