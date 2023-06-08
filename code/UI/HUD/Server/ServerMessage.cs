using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using TFS2.Menu;

namespace TFS2.UI;

/// <summary>
/// MOTD chalkboard screen shown when the player first joins a server.
/// </summary>
public partial class ServerMessage : HudOverlay
{
	ScenePanel scene;

	public ServerMessage()
	{
		var world = new SceneWorld();
		var position = new Vector3( 390, 0, -39 );
		var rotation = Rotation.From( 0, 180, 0 );
		new SceneObject( world, "models/vgui/ui_welcome01.vmdl", new Transform( position, rotation ) );

		scene = Add.ScenePanel( world, Vector3.Zero, Rotation.Identity, 20 );

		scene.Camera.EnablePostProcessing = false;

		var board = scene.Add.Panel( "board" );
		board.Add.Label( "Welcome to Team Fortress: Source 2", "title" );
		board.Add.Label( "This gamemode is still under heavy development.\n Follow our progress at tfsource2.com", "content" );

		board.Add.Label( "\n BUGS ARE TO BE EXPECTED AND ESSENTIAL FEATURES MAY BE MISSING.", "content" );
		board.Add.Label( " Please report bugs on our issue tracker: github.com/AmperSoftware/TF-Source-2", "content" );

		board.Add.Label( "\n Join our official Discord Community: discord.gg/tfs2", "content" );

		board.Add.Label( "\n We are not affiliated with Valve Software.", "content" );

		var footer = Add.Panel( "footer menu" );
		footer.Add.ButtonWithIcon( "Back", "undo", "button-dark disabled" );
		footer.Add.ButtonWithIcon( "Watch Movie", "movie", "button-dark disabled" );
		footer.Add.ButtonWithIcon( "Continue", "redo", "button-dark", () => Open<TeamSelection>() );
	}

	[ConCmd.Client( "tf_open_menu_motd" )]
	public static void Command_OpenMotd()
	{
		Open<ServerMessage>();
	}
}
