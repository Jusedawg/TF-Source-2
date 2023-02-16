using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TFS2.UI;

public class DevMenu : Panel
{
	public DevMenu()
	{
		StyleSheet.Load( "/ui/DevMenu.scss" );
		Add.Label( "Developer Menu", "header" );

		var panel = Add.Panel( "button-list" );
		//
		// General
		//
		panel.Add.Label( "General", "section" );
		panel.Add.Button( "Change Team", "button", () => ConsoleSystem.Run( "tf_open_menu_team" ) );
		panel.Add.Button( "Change Class", "button", () => ConsoleSystem.Run( "tf_open_menu_class" ) );
		panel.Add.Button( "Suicide", "button", () => ConsoleSystem.Run( "kill" ) );
		panel.Add.Button( "Regenerate", "button", () => ConsoleSystem.Run( "tf_regenerate" ) );
		panel.Add.Button( "Respawn", "button", () => ConsoleSystem.Run( "respawn" ) );

		//
		// Teamplay
		//
		panel.Add.Label( "Teamplay", "section" );
		panel.Add.Button( "Restart Round", "button", () => ConsoleSystem.Run( "mp_restartround" ) );
		panel.Add.Button( "Restart Game", "button", () => ConsoleSystem.Run( "mp_restartgame" ) );

		//
		// Entities
		//
		panel.Add.Label( "Entities", "section" );
		panel.Add.Button( "Spawn a Mimic Bot", "button", () => ConsoleSystem.Run( "bot_add" ) );
		panel.Add.Button( "Spawn a Dummy Bot", "button", () => ConsoleSystem.Run( "tf_bot_add" ) );
	}

	public override void Tick()
	{
		if ( Input.Released( InputButton.Slot0 ) )
			Mouse.Position = Screen.Size * .5f;
		SetClass( "open", Input.Down( InputButton.Slot0 ) );
	}
}
