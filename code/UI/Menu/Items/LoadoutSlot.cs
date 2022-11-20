using Sandbox.UI;
using Amper.FPS;

namespace TFS2.UI;

[UseTemplate]
partial class LoadoutSlot : Panel
{
	public PlayerClass PlayerClass { get; set; }
	public TFWeaponSlot Slot { get; set; }

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
		base.OnClick( e );
		MenuOverlay.Open( new ItemSelection( PlayerClass, Slot ) );
	}
}
