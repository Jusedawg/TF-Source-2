using Sandbox;
using Amper.FPS;
using System;
using TFS2.UI;

namespace TFS2;

partial class TFWeaponBase : IInteractableTargetID, ITargetIDSubtext, IKillfeedIcon
{
	string ITargetID.Name => Data.Title;
	string ITargetID.Avatar => Data.InventoryIcon;
	bool IInteractableTargetID.CanInteract( TFPlayer user ) => IsUsable( user );
	string IInteractableTargetID.InteractText => "Pickup";
	string IInteractableTargetID.InteractButton => "Inspect";
	string ITargetIDSubtext.Subtext => $"Dropped by: {OriginalOwner?.Name ?? "Unknown Player"}";
	string IKillfeedIcon.GetIcon( bool isCrit, string[] tags )
	{
		if ( isCrit && !string.IsNullOrEmpty( Data.KillFeedIconSpecial ) )
			return Data.KillFeedIconSpecial;

		return Data.KillFeedIcon;
	}
}
