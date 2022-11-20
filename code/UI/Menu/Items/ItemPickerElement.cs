using Sandbox.UI;
using System;
using Amper.FPS;

namespace TFS2.UI.Menu.Items;

[UseTemplate]
partial class ItemPickerElement : Panel
{
	public event Action OnClicked;

	Panel Image { get; set; }
	Label Name { get; set; }
	bool IsEquipped { get; set; }
	PlayerClass PlayerClass { get; set; }
	WeaponData Data { get; set; }

	public void SetWeaponData( WeaponData data )
	{
		Image.Style.SetBackgroundImage( "/ui/unbound.png" );
		Name.Text = $"";
		Classes = "";
		Data = data;

		if ( data == null )
			return;

		Image.Style.SetBackgroundImage( Util.JPGToPNG( data.InventoryIcon ) );
		Name.Text = data.Title;

		if ( PlayerClass != null )
		{
			if ( !PlayerClass.IsDefaultWeapon( data ) )
				AddClass( "q_unique" );
		}

		if ( IsEquipped )
			AddClass( "has_equipped_label" );
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		if ( Data != null )
			SetWeaponData( Data );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );
		OnClicked?.Invoke();
	}
}

