using Sandbox;
using Sandbox.UI;
using Amper.FPS;

namespace TFS2.UI;

[UseTemplate]
public partial class ObjectivesStatus : Panel
{
	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );
	}

	public bool ShouldDraw()
	{
		if ( SDKGame.Current.State == GameState.RoundEnd ) return false;
		if ( Input.Down( InputButton.Score ) ) return false;
		return true;
	}
}
