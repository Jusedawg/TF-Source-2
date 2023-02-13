using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace TFS2.UI;

public class TauntMenu : Panel
{
	public TauntMenu()
	{
		StyleSheet.Load( "/ui/TauntMenu.scss" );

		Add.Label( "Taunt Menu", "header" );

		//AddChild<TauntMenuButtons>();
	}

	public override void Tick()
	{
		if ( Game.LocalPawn is not TFPlayer player ) return; //FIX THIS, prevents Non-tf2 entities from ticking tauntmenu code

		SetClass( "open", Input.Down( InputButton.Drop ) );
		if(Input.Pressed( InputButton.Drop ) )
		{
			OnPlayerUpdated(); //FIX: Temp method to regen taunt buttons until I can figure out howw to get it to generate AFTER class generates
		}
		if ( Input.Down( InputButton.Drop ) )
		{
			Log.Info("OPEN");
		}

		if ( LastTeam != player.Team || LastPlayerClass != player.PlayerClass )
		{
			OnPlayerUpdated();
		}
	}

	TFTeam LastTeam { get; set; }
	PlayerClass LastPlayerClass { get; set; }

	//If the player changes class or team, delete our existing taunt buttons and regenerate them
	public void OnPlayerUpdated()
	{
		if ( Game.LocalPawn is not TFPlayer player ) return; //FIX THIS, prevents Non-tf2 entities from ticking tauntmenu code

		LastTeam = player.Team;
		LastPlayerClass = player.PlayerClass;

		if ( player.PlayerClass == null ) return;

		foreach ( var child in Children )
		{
			if ( child is TauntMenuButtons )
			{
				child.Delete();
			}
		}
		AddChild<TauntMenuButtons>();
	}
}
public class TauntMenuButtons : Panel
{
	public TauntMenuButtons()
	{
		//Add.Label( "General", "section" );
		
		if ( Game.LocalPawn is not TFPlayer player ) return;

		if ( player.PlayerClass == null ) return;

		foreach ( var taunt in player.TauntList ) //FIX INVESTIGATE: Tauntlist is returning 0, even though tauntlist is populated
		{
			Log.Info("WE adding shit");
			Add.Button( taunt.DisplayName, "button", () => ConsoleSystem.Run( "tf_playtaunt", taunt.StringName ) );
		}
	}
}
