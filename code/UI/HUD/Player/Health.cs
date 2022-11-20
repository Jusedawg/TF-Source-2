using Sandbox;
using Sandbox.UI;
using Amper.FPS;

namespace TFS2.UI;

[UseTemplate]
public class Health : Panel
{
	private Panel PlayerClassIcon { get; set; }

	public Health()
	{
		BindClass( "red", () => TFPlayer.LocalPlayer.Team == TFTeam.Red );
		BindClass( "blue", () => TFPlayer.LocalPlayer.Team == TFTeam.Blue );

		EventDispatcher.Subscribe<PlayerRegenerateEvent>( OnRegenerate, this );
		SetupClassPreview();
	}

	public override void Tick()
	{
		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() ) return;

		SetClass( "hidden", !ShouldDraw() );
	}

	public bool ShouldDraw()
	{
		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() )
			return false;

		if ( Input.Down( InputButton.Score ) )
			return false;

		return player.IsAlive;
	}

	public void OnRegenerate( PlayerRegenerateEvent args )
	{
		if ( args.Client != Local.Client ) return;

		SetupClassPreview();
	}

	public void SetupClassPreview()
	{
		var player = TFPlayer.LocalPlayer;
		if ( player == null ) return;

		var pClass = player.PlayerClass;
		if ( pClass.IsValid() )
		{
			var classIcon = player.Team == TFTeam.Blue
				? pClass.IconBlue
				: pClass.IconRed;

			PlayerClassIcon.Style.SetBackgroundImage( Util.JPGToPNG( classIcon ) );
		}
	}
}
