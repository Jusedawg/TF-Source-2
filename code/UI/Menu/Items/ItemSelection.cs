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

	private WeaponData GetEquipped() => GetEquippedAsync().Result;
	private async Task<WeaponData> GetEquippedAsync()
	{
		var loadout = Loadout.LocalLoadout;
		await loadout.Load();

		// Add the currently equipped weapon.
		var equipped = await loadout.GetLoadoutItem( PlayerClass, Slot );
		return equipped;
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
