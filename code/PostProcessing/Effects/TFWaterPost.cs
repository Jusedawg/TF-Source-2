using Sandbox;

namespace TFS2.PostProcessing;

public class TFWaterPost : RenderHook
{
    RenderAttributes attributes = new RenderAttributes();
    Material scopeMaterial = Material.Load("materials/screen_fx/tfwater_post.vmat");

    public override void OnStage(SceneCamera target, Stage renderStage)
    {
        if (Enabled && renderStage == Stage.BeforePostProcess)
        {
            Graphics.GrabFrameTexture("ColorBuffer", attributes);

            Graphics.Blit(scopeMaterial, attributes);
        }
    }
}