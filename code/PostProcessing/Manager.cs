using Sandbox;
using Amper.FPS;
using TFS2.PostProcessing;

namespace TFS2;

public class TFPostProcessingManager : PostProcessingManager
{
    public override void Update()
	{
        var player = TFPlayer.LocalPlayer;
        bool playerAlive = player.IsValid() && player.IsAlive;
        GetOrCreate<SniperScope>().Enabled = playerAlive && ((player.ActiveWeapon as SniperRifle)?.IsZoomed ?? false);
        GetOrCreate<MedigunUber>().Enabled = playerAlive && player.InCondition(TFCondition.Invulnerable);
    }
}
