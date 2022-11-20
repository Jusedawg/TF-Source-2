using Sandbox;
using Sandbox.UI;
using Amper.FPS;

namespace TFS2;

public partial class TargetIDHovered : TargetID
{
	public override ITargetID FindTarget()
	{
		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() )
			return null;

		var hoveredTarget = player.HoveredEntity as ITargetID;
		if ( !hoveredTarget.IsValid() )
			return null;

		// Don't show target in primary if they're already shown in secondary.
		if ( TargetIDHealing.Current.Target == hoveredTarget )
			return null;

		if ( !CanInspect( player, player.HoveredEntity ) )
			return null;

		return hoveredTarget;
	}

	public bool CanInspect( TFPlayer localPlayer, Entity target )
	{
		if ( target is TFWeaponBase )
			return true;

		var pClass = localPlayer.PlayerClass;
		if ( pClass.IsValid() )
		{
			if ( !pClass.Abilities.CanSeeEnemyHealth )
				return ITeam.IsSame( target, Local.Pawn );
		}

		return true;
	}

	public override bool UpdatePretext( Label label )
	{
		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() )
			return false;

		if ( Target is TFPlayer enemy )
		{
			if ( !ITeam.IsSame( player, enemy ) )
			{
				label.Text = "Enemy:";
				return true;
			}
		}

		return false;
	}

	public override bool UpdateSubtext( Label label )
	{
		if ( Target is TFWeaponBase weapon )
		{
			label.Text = $"Dropped by: {weapon.OriginalOwner.Name}";
			return true;
		}

		return false;
	}
}
