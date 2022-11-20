using Sandbox;
using Amper.FPS;
using System;

namespace TFS2;

partial class TFWeaponBase : IInteractableTargetID
{
	string ITargetID.Name => Data.Title;
	string ITargetID.Avatar => Data.InventoryIcon;

	bool IInteractableTargetID.CanInteract( TFPlayer user ) => IsUsable( user );
	string IInteractableTargetID.InteractText => "Pickup";
	InputButton IInteractableTargetID.InteractButton => InputButton.Use;
}
