using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2;

public abstract class EngineerPDA : TFWeaponBase
{
	public override bool NeedsAmmo() => false;
	public override bool CanAttack() => false;
	public override bool CanSecondaryAttack() => false;
	public override bool ShouldDrawCrosshair() => false;
	public override bool ShowAmmoOnHud() => false;
	public override void BuildInput()
	{
		if ( Input.Pressed( "Menu" ) )
			Cancel();
		else if ( Input.Pressed( "Slot1" ) )
			OnInput( 0 );
		else if ( Input.Pressed( "Slot2" ) )
			OnInput( 1 );
		else if ( Input.Pressed( "Slot3" ) )
			OnInput( 2 );
		else if ( Input.Pressed( "Slot4" ) )
			OnInput( 3 );
	}
	public virtual void OnInput( int slot )
	{
		Input.ReleaseAction( $"Slot{slot + 1}" );
	}

	public virtual void Cancel()
	{
		TFOwner.RequestedActiveWeapon = TFOwner.GetWeaponInSlot( TFWeaponSlot.Primary ); // TODO: Switch to last weapon
		Input.ReleaseAction( "Menu" );
	}
	public string GetBuilding( int slot )
	{
		var buildings = TFOwner.GetAvailableBuildings();
		if ( buildings != default )
			return buildings.ElementAtOrDefault( slot );

		return "";
	}
}

[Library( "tf_weapon_construction_pda" )]
public class ConstructionPDA : EngineerPDA
{
	public override void OnInput( int slot )
	{
		string building = GetBuilding( slot );
		if ( string.IsNullOrEmpty( building ) ) return;
		if ( !TFOwner.CanBuild( building ) ) return;

		TFPlayer.StartBuilding( building );
		base.OnInput( slot );
	}
}

[Library( "tf_weapon_destruction_pda" )]
public class DestructionPDA : EngineerPDA
{
	public override void OnInput( int slot )
	{
		string building = GetBuilding( slot );
		if ( string.IsNullOrEmpty( building ) ) return;
		var existingBuilding = TFOwner.Buildings.FirstOrDefault( b => b.Data.ResourceName == building );
		if ( existingBuilding == null ) return;

		TFPlayer.DestroyBuilding( building );
		if ( !existingBuilding.IsValid() )
			base.OnInput( slot );
	}
}

