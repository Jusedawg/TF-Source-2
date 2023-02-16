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
	TFTeam LastTeam { get; set; }
	PlayerClass LastPlayerClass { get; set; }

	public override void Tick()
	{
		if ( Game.LocalPawn is not TFPlayer player ) return;

		if ( LastTeam != player.Team || LastPlayerClass != player.PlayerClass )
		{
			OnPlayerUpdated();
		}

		if ( !player.IsAlive ) return;

		SetClass( "open", Input.Down( InputButton.Drop ) );

		if ( Input.Released( InputButton.Drop ) )
		{
			Mouse.Position = Screen.Size * .5f;
		}

	}

	//If the player changes class or team, delete our existing taunt buttons and regenerate them
	public void OnPlayerUpdated()
	{
		if ( Game.LocalPawn is not TFPlayer player ) return;

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
			Add.Button( taunt.DisplayName, "button", () => ConsoleSystem.Run( "tf_playtaunt", taunt.ResourceName ) );
		}
	}
}
