using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace TFS2.Menu;

public partial class ItemSelection : MenuOverlay
{
	string ClassName 
	{
		get => PlayerClass.ResourceName;
		set
		{
			PlayerClass = PlayerClass.Get( value );
		} 
	}
	PlayerClass PlayerClass { get; set; }
	TFWeaponSlot Slot { get; set; }

	private WeaponData GetEquipped()
	{
		return Loadout.LocalLoadout.GetLoadoutItem( PlayerClass, Slot );
	}

	public void OnClickBack()
	{
		this.Navigate( $"/loadout/class/{ClassName}/" );
	}

	public void OnClickEquip(WeaponData weapon)
	{
		Loadout.LocalLoadout.SetLoadoutItem( PlayerClass, Slot, weapon );
		OnClickBack();
	}
}
