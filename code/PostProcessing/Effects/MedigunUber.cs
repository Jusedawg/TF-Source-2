using Sandbox;
using TFS2;

namespace TFS2.PostProcessing;

public class MedigunUber : RenderHook
{
	private static readonly Vector3 RedTint = new Vector3( 1f, 115f / 255f, 15f / 255f );
	private static readonly Vector3 BluTint = new Vector3( 55f / 255f, 155f / 255f, 1f );

	private TFTeam lastTeam = TFTeam.Unassigned;
    RenderAttributes attribs = new();
	Material material = Material.Load("materials/screen_fx/uber_post.vmat");

    public override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage == Stage.BeforePostProcess )
		{
            var currentTeam = TFPlayer.LocalPlayer.Team;

            if (currentTeam != lastTeam)
            {
                attribs.Set("colorTint", currentTeam == TFTeam.Red ? RedTint : BluTint);
                lastTeam = currentTeam;
            }
            Graphics.GrabFrameTexture("ColorBuffer", attribs);
            Graphics.Blit(material, attribs);
        }
	}
}
