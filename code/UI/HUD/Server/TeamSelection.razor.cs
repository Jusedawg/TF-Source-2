using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;
using TFS2.Menu;

namespace TFS2.UI;

/// <summary>
/// MotD chalkboard when player joins a server.
/// </summary>
public partial class TeamSelection : HudOverlay
{
	TeamSelectionBackground BackgroundScene { get; set; }

	public TeamSelection()
	{
		if ( Sandbox.Game.LocalPawn is not TFPlayer player ) return;

		BackgroundScene = AddChild<TeamSelectionBackground>();
		BackgroundScene.Camera.EnablePostProcessing = false;

		var footer = Add.Panel( "footer menu" );
		footer.Add.Label( "SELECT A TEAM", "title" );
		footer.Add.ButtonWithIcon( "Cancel", "highlight_off", $"button-dark {(player.Team == TFTeam.Unassigned ? "hidden" : "")}", HandleCancelClick );
	}

	void HandleCancelClick()
	{
		Sound.FromScreen( "ui.button.click" );
		Close();
	}

	[ConCmd.Client( "tf_open_menu_team" )]
	public static void Command_OpenTeamMenu()
	{
		TFGameRules.Current.ShowTeamSelectionMenu();
	}
}

public class TeamSelectionBackground : ScenePanel
{
	Label RedTeamCount { get; set; }
	Label BlueTeamCount { get; set; }

	public bool ShowTeamDoors()
	{
		// show them, unless we're playing arena.
		return TFGameRules.Current.IsTeamSelectionAllowed();
	}

	public bool ShowSpectatorTV() { return true; }
	public bool ShowRandomTeamDoor() { return true; }

	public string GetBackgroundModelPath()
	{
		if ( !ShowTeamDoors() )
			return "models/vgui/ui_arena01.vmdl";
		else
			return "models/vgui/ui_team01.vmdl";
	}

	public string GetTeamButtonModelPath( TFTeam team )
	{
		switch ( team )
		{
			// random
			case TFTeam.Unassigned:
				if ( !ShowTeamDoors() )
					return "models/vgui/ui_arenadoor01.vmdl";
				else
					return "models/vgui/ui_team01_random.vmdl";

			case TFTeam.Spectator:
				return "models/vgui/ui_team01_spectate.vmdl";

			case TFTeam.Red:
				return "models/vgui/ui_team01_red.vmdl";

			case TFTeam.Blue:
				return "models/vgui/ui_team01_blue.vmdl";
		}

		return "";
	}

	public string GetTeamLabel( TFTeam team )
	{
		switch ( team )
		{
			case TFTeam.Unassigned:

				if ( !ShowTeamDoors() )
					return "FIGHT!";
				else
					return "RANDOM";

			case TFTeam.Spectator:
				return "SPECTATE";

			default:
				return "";
		}
	}

	public TeamSelectionBackground()
	{
		World = new SceneWorld();
		Camera.FieldOfView = 20;
		Classes = "background";

		Camera.Position = Vector3.Zero;
		Camera.Rotation = Rotation.Identity;

		World = new SceneWorld();

		var position = new Vector3( 390, 0, -39 );
		var rotation = Rotation.From( 0, 180, 0 );
		var transform = new Transform( position, rotation );

		new SceneModel( World, GetBackgroundModelPath(), transform );

		if ( ShowRandomTeamDoor() )
		{
			new TeamSelectionDoor
			{
				Team = TFTeam.Unassigned,
				Prop = new SceneModel( World, Model.Load( GetTeamButtonModelPath( TFTeam.Unassigned ) ), transform ),
				Classes = "team door random",
				Text = "1",
				Shortcut = "Slot1",
				Parent = this
			};

			Add.Label( GetTeamLabel( TFTeam.Unassigned ), "label_random" );
		}

		if ( ShowSpectatorTV() )
		{
			new TeamSelectionTV
			{
				Team = TFTeam.Spectator,
				Prop = new SceneModel( World, Model.Load( GetTeamButtonModelPath( TFTeam.Spectator ) ), transform ),
				Classes = "team tv spectator",
				Text = "2",
				Shortcut = "Slot2",
				Parent = this
			};

			Add.Label( GetTeamLabel( TFTeam.Spectator ), "label_spectate" );
		}

		if ( ShowTeamDoors() )
		{
			new TeamSelectionDoor
			{
				Team = TFTeam.Blue,
				Prop = new SceneModel( World, Model.Load( GetTeamButtonModelPath( TFTeam.Blue ) ), transform ),
				Classes = "team door blue",
				Text = "3",
				Shortcut = "Slot3",
				Parent = this
			};

			new TeamSelectionDoor
			{
				Team = TFTeam.Red,
				Prop = new SceneModel( World, Model.Load( GetTeamButtonModelPath( TFTeam.Red ) ), transform ),
				Classes = "team door red",
				Text = "4",
				Shortcut = "Slot4",
				Parent = this
			};

			var players = Entity.All.OfType<TFPlayer>();
			int redPlayersCount = players.Where( x => x.Team == TFTeam.Red ).Count();
			int bluePlayersCount = players.Where( x => x.Team == TFTeam.Blue ).Count();

			RedTeamCount = Add.Label( redPlayersCount.ToString(), "teamcounter red" );
			BlueTeamCount = Add.Label( bluePlayersCount.ToString(), "teamcounter blue" );
		}
	}

	public override void Tick()
	{
		base.Tick();

		var players = Entity.All.OfType<TFPlayer>();
		int redPlayersCount = players.Where( x => x.Team == TFTeam.Red ).Count();
		int bluePlayersCount = players.Where( x => x.Team == TFTeam.Blue ).Count();
		RedTeamCount.Text = $"{redPlayersCount}";
		BlueTeamCount.Text = $"{bluePlayersCount}";
	}
}

public class TeamSelectionButton : Label
{
	public SceneModel Prop { get; set; }
	public TFTeam Team { get; set; }
	public string Shortcut { get; set; }

	public TeamSelectionButton()
	{
		AddEventListener( "onclick", HandleClick );
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		Prop?.SetAnimParameter( "b_hovered", true );
	}

	protected override void OnMouseOut( MousePanelEvent e )
	{
		Prop?.SetAnimParameter( "b_hovered", false );
	}

	public override void Tick()
	{
		base.Tick();
		Prop?.Update( RealTime.Delta );

		SetClass( "blocked", !CanUse());
	}

	private bool CanUse()
	{
		if ( !Team.IsPlayable() ) return true;
		if ( Game.LocalPawn is TFPlayer ply )
			return TFGameRules.Current.CanPlayerChangeTeamTo( ply, (int)Team );

		return false;
	}

	public void HandleClick()
	{
		if ( !CanUse() )
		{
			Sound.FromScreen( "weapon_medigun.notarget" );
			return;
		}
		else
		{
			Sound.FromScreen( "ui.button.click" );
		}

		ConsoleSystem.Run( $"tf_join_team", Team );
		HudOverlay.CloseActive();
	}

	[GameEvent.Client.BuildInput]
	public void ProcessClientInput()
	{
		if ( Sandbox.Game.LocalPawn is not TFPlayer player )
			return;

		if ( Input.Pressed( Shortcut ) )
			HandleClick();
	}
}

public class TeamSelectionDoor : TeamSelectionButton
{
	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );
		Sound.FromScreen( "ui.team_door.open" );
	}

	protected override void OnMouseOut( MousePanelEvent e )
	{
		base.OnMouseOut( e );
		Sound.FromScreen( "ui.team_door.close" );
	}
}

public class TeamSelectionTV : TeamSelectionButton
{
	TimeSince TimeSinceSound { get; set; }

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );

		if ( TimeSinceSound > 5f )
		{
			Sound.FromScreen( "ui.spectator_tv.tune" );
			TimeSinceSound = 0;
		}
	}
}
