using Sandbox;
using Sandbox.UI;

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
		var loadout = Loadout.LocalLoadout;
		loadout.Load().Wait();

		// Add the currently equipped weapon.
		var equipped = loadout.GetLoadoutItem( PlayerClass, Slot ).Result;
		return equipped;
	}

	public void OnClickBack()
	{
		Open( new ClassLoadout() );
	}
}
