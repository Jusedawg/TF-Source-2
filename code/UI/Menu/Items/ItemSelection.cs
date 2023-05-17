using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace TFS2.UI;

public partial class ItemSelection : MenuOverlay
{
	PlayerClass PlayerClass { get; set; }
	TFWeaponSlot Slot { get; set; }

	public ItemSelection( PlayerClass pclass, TFWeaponSlot slot )
	{
		PlayerClass = pclass;
		Slot = slot;
	}

	private WeaponData GetEquipped()
	{
		return Loadout.LocalLoadout.GetLoadoutItem( PlayerClass, Slot );
	}
	private async Task<WeaponData> GetEquippedAsync()
	{
		return await Loadout.LocalLoadout.GetLoadoutItemAsync( PlayerClass, Slot );
	}

	public void OnClickBack()
	{
		Open( new ClassLoadout() );
	}

	public void OnClickEquip(WeaponData weapon)
	{
		Loadout.LocalLoadout.SetLoadoutItem( PlayerClass, Slot, weapon );
		OnClickBack();
	}
}
