using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace TFS2.Menu;

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

	public void OnClickBack()
	{
		Open( new ClassLoadout(PlayerClass) );
	}

	public void OnClickEquip(WeaponData weapon)
	{
		Loadout.LocalLoadout.SetLoadoutItem( PlayerClass, Slot, weapon );
		OnClickBack();
	}
}
