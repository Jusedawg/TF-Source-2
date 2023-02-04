using Sandbox.UI;
using Amper.FPS;
using System;

namespace TFS2.UI;

public partial class ItemPicker : Panel
{
	public PlayerClass PlayerClass { get; set; }
	public TFWeaponSlot Slot { get; set; }
	public event Action OnClicked;
	Panel Image { get; set; }
	Label Name { get; set; }
	public WeaponData Data { get; set; }

	protected override void OnClick( MousePanelEvent e )
	{
		OnClicked?.Invoke();
		//MenuOverlay.Open( new ItemSelection( PlayerClass, Slot ) );
		//Loadout.LocalLoadout.SetLoadoutItem( PlayerClass, Slot, Data );
	}
}
