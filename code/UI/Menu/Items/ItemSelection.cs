using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

public partial class ItemSelection : MenuOverlay
{
	PlayerClass PlayerClass { get; set; }
	Label ClassName { get; set; }
	TFWeaponSlot Slot { get; set; }
	Label SlotName { get; set; }
	Panel ItemsContainer { get; set; }
	Label PlayerName { get; set; }
	Image PlayerAvatar { get; set; }

	public ItemSelection( PlayerClass pclass, TFWeaponSlot slot )
	{
		PlayerClass = pclass;
		Slot = slot;
	}

	public override void Tick()
	{
		if ( !IsVisible ) return;

		PlayerName.Text = Sandbox.Game.LocalClient.Name;
		PlayerAvatar.SetTexture( $"avatarbig:{Sandbox.Game.LocalClient.SteamId}" );
	}

	public void OnClickBack()
	{
		Open( new ClassLoadout() );
	}
}
