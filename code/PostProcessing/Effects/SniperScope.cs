using Sandbox;

namespace TFS2.PostProcessing;

public class SniperScope : RenderHook
{
	public override void OnStage( SceneCamera target, Stage renderStage )
	{
		if ( renderStage != Stage.AfterPostProcess )
			return;

		//Log.Info( $"SniperScope OnStage {Enabled}" );
		Graphics.Blit(Material.Load( "materials/screen_fx/scope_post.vmat" ));
	}
}
