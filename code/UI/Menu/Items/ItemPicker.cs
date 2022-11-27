using Sandbox.UI;
using Amper.FPS;
using System;

namespace TFS2.UI;

[UseTemplate]
partial class ItemPicker : Panel
{
	public PlayerClass PlayerClass { get; set; }
	public TFWeaponSlot Slot { get; set; }

	public event Action OnClicked;

	Panel Image { get; set; }
	Label Name { get; set; }
	WeaponData Data { get; set; }

	public void SetWeaponData( WeaponData data )
	{
		Image.Style.SetBackgroundImage( "/ui/icons/unbound.png" );
		Name.Text = $"{Slot}";
		Classes = "";
		Data = data;

		if ( data == null )
			return;

		Image.Style.SetBackgroundImage( Util.JPGToPNG( data.InventoryIcon ) );
		Name.Text = data.Title;

		if ( !PlayerClass.IsDefaultWeapon( data ) )
			AddClass( "q_unique" );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		OnClicked?.Invoke();
		//MenuOverlay.Open( new ItemSelection( PlayerClass, Slot ) );
		//Loadout.LocalLoadout.SetLoadoutItem( PlayerClass, Slot, Data );
	}
}
