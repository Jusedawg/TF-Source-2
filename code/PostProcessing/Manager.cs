using Sandbox;
using Amper.FPS;
using TFS2.PostProcessing;

namespace TFS2;

public class TFPostProcessingManager : PostProcessingManager
{
	public override void Update()
	{
		base.Update();

		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() )
			return;

		// TODO: Reimplement this with new post processing

		//<SniperScope>( (player.ActiveWeapon as SniperRifle)?.IsZoomed ?? false );
		//SetVisible<MedigunUber>( player.InCondition( TFCondition.Invulnerable ) );
	}
}
