﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;

namespace TFS2.UI;

public partial class ClassSelection : MenuOverlay
{
	ClassSelectionBackgroundScene BackgroundScene { get; set; }
	ClassSelectionPlayerModel PlayerScene { get; set; }
	Sound BackgroundMusic { get; set; }
	Sound NoteSound { get; set; }
	PlayerClass SelectedClass { get; set; }

	public ClassSelection()
	{
		if ( Sandbox.Game.LocalPawn is not TFPlayer player ) return;

		// TODO: Convert to HTML
		StyleSheet.Load( "/UI/HUD/Server/ClassSelection.scss" );
		BackgroundScene = AddChild<ClassSelectionBackgroundScene>();
		BackgroundMusic = Sound.FromScreen( "music.class_menu.background" );

		PlayerScene = AddChild<ClassSelectionPlayerModel>();
		PreviewClass( player.PlayerClass ?? PlayerClass.Get( TFPlayerClass.Heavy ) );
		BackgroundScene.Camera.EnablePostProcessing = false;
		PlayerScene.Camera.EnablePostProcessing = false;

		var footer = Add.Panel( "footer menu" );
		footer.Add.Label( "SELECT A CLASS", "title" );
		footer.Add.ButtonWithIcon( "Edit Loadout", "inventory", "button-dark", OnClickLoadout );

		// If player has class specified, show Cancel button. Otherwise show the text.
		if ( SelectedClass != null )
			footer.Add.ButtonWithIcon( "Cancel", "highlight_off", "button-dark", OnClickCancel );
	}

	public override void OnDeleted()
	{
		BackgroundMusic.Stop();
		base.OnDeleted();
	}

	public void PlayNote( int note )
	{
		NoteSound.Stop();
		NoteSound = Sound.FromScreen( $"music.class_menu.note_{note}" );
	}

	public void OnSelectedPlayerClass( PlayerClass playerClass )
	{
		if ( SelectedClass == playerClass )
			return;

		if ( playerClass == null )
		{
			NoteSound.Stop();
			Sound.FromScreen( "ui.button.click" );
		}
		else
			PlayNote( (int)playerClass.Entry + 1 );

		PreviewClass( playerClass );
		SelectedClass = playerClass;
	}

	public void PreviewClass( PlayerClass playerClass )
	{
		SelectedClass = playerClass;
		BackgroundScene.PreviewClass( playerClass );
		PlayerScene.PreviewClass( playerClass );
	}

	[ConCmd.Client( "tf_open_menu_class" )]
	public static void Command_OpenClassMenu()
	{
		TFGameRules.Current.ShowClassSelectionMenu();
	}

	public void OnClickLoadout()
	{
		Sound.FromScreen( "ui.button.click" );
		Open( new ClassLoadout( SelectedClass ) );
	}

	private void OnClickCancel()
	{
		Sound.FromScreen( "ui.button.click" );
		Close();
	}
}

public class ClassSelectionBackgroundScene : ScenePanel
{
	public ClassSelectionBackgroundScene()
	{
		World = new SceneWorld();
		Camera.FieldOfView = 20;
		Classes = "scene background";

		Camera.Position = Vector3.Zero;
		Camera.Rotation = Rotation.Identity;

		World = new SceneWorld();

		var position = new Vector3( 390, 0, -39 );
		var rotation = Rotation.From( 0, 180, 0 );
		var transform = new Transform( position, rotation );
		new SceneModel( World, "models/vgui/ui_class01.vmdl", transform );

		//
		// All playable classes.
		//

		if ( Sandbox.Game.LocalPawn is TFPlayer player )
		{
			var team = player.Team;
			foreach ( TFPlayerClass value in Enum.GetValues( typeof( TFPlayerClass ) ) )
			{
				// Undefined class is random class. Always add it in the end.
				if ( value == TFPlayerClass.Undefined ) continue;

				var playerClass = PlayerClass.Get( value );
				if ( playerClass == null ) continue;

				new ClassSelectionButton
				{
					PlayerClass = playerClass,
					Classes = $"class {playerClass.ResourceName} {team}",
					Parent = this
				};
			}

			//
			// Random class.
			//
			new ClassSelectionButton
			{
				Classes = $"class random {team}",
				Parent = this
			};
		}
	}

	public void PreviewClass( PlayerClass pclass )
	{
		foreach ( var button in Children.OfType<ClassSelectionButton>() )
		{
			button.SetClass( "active", button.PlayerClass == pclass );
		}
	}
}

public class ClassSelectionPlayerModel : ScenePanel
{
	public Vector3 Offset => new( 280, 65, -50 );
	public SceneModel PlayerModel { get; set; }
	public SceneModel WeaponModel { get; set; }
	public PlayerClass SelectedClass { get; set; }

	const float characterLightBrightness = 6f;
	public ClassSelectionPlayerModel()
	{
		World = new SceneWorld();
		Camera.FieldOfView = 50;
		Classes = "scene player";

		Camera.Position = Vector3.Zero;
		Camera.Rotation = Rotation.Identity;

		World = new SceneWorld();

		new SceneLight( World, Offset + new Vector3( -100, 100, 100 ), 200, Color.White * characterLightBrightness * 1.6f )
		{
			Rotation = Rotation.LookAt( Offset )
		};

		new SceneLight( World, Offset + new Vector3( -100, -100, 100 ), 200, Color.White * characterLightBrightness )
		{
			Rotation = Rotation.LookAt( Offset )
		};
	}

	public TFWeaponSlot GetPreviewWeaponSlot()
	{
		switch ( SelectedClass.Entry )
		{
			case TFPlayerClass.Engineer:
			case TFPlayerClass.Spy:
				return TFWeaponSlot.Melee;

			case TFPlayerClass.Medic:
				return TFWeaponSlot.Secondary;

			default:
				return TFWeaponSlot.Primary;
		}
	}

	public async void PreviewClass( PlayerClass pclass )
	{
		if ( Sandbox.Game.LocalPawn is not TFPlayer player ) return;

		SelectedClass = pclass;

		PlayerModel?.Delete();
		WeaponModel?.Delete();

		if ( SelectedClass == null ) return;

		//
		// PLAYER MODEL
		//
		string playerModel = pclass.Model;
		if ( string.IsNullOrEmpty( playerModel ) ) return;

		Rotation rotation = Rotation.From( 0, 190, 0 );
		Transform transform = new( Offset, rotation );

		PlayerModel = new SceneModel( World, Model.Load( playerModel ), transform );

		//
		// WEAPON MODEL
		//

		// Fetch the player's inventory for the weapon preview...
		Loadout loadout = Loadout.ForClient( Sandbox.Game.LocalClient );
		// and get the weapon for the preview slot.
		WeaponData previewWeapon = await loadout.GetLoadoutItemAsync( pclass, GetPreviewWeaponSlot() );
		if ( previewWeapon == null ) return;

		// Ensure it has a valid world model.
		string weaponModel = previewWeapon.WorldModel;
		if ( string.IsNullOrEmpty( weaponModel ) ) return;

		WeaponModel = new SceneModel( World, Model.Load( weaponModel ), PlayerModel.Transform );
		PlayerModel.AddChild( "weapon", WeaponModel );

		int skin = Math.Clamp( player.TeamNumber - 2, 0, 1 );
		PlayerModel.SetMaterialGroup( $"{skin}" );
		WeaponModel.SetMaterialGroup( $"{skin}" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( PlayerModel != null && PlayerModel.IsValid() )
		{
			PlayerModel?.Update( RealTime.Delta );
			PlayerModel?.SetAnimParameter( "b_selection", true );
		}

		if ( WeaponModel != null && WeaponModel.IsValid() )
		{
			WeaponModel?.Update( RealTime.Delta );
		}
	}
}

public class ClassSelectionButton : Label
{
	public PlayerClass PlayerClass { get; set; }

	public ClassSelectionButton()
	{
		AddEventListener( "onclick", HandleClick );
	}

	public override void Tick()
	{
		base.Tick();

		if ( PlayerClass != null )
		{
			Text = $"{(int)PlayerClass.Entry + 1}";
		}
	}

	string GetShortcutButton()
	{
		if ( PlayerClass == null ) return "Slot10";

		return PlayerClass.Entry switch
		{
			TFPlayerClass.Scout => "Slot1",
			TFPlayerClass.Soldier => "Slot2",
			TFPlayerClass.Pyro => "Slot3",
			TFPlayerClass.Demoman => "Slot4",
			TFPlayerClass.Heavy => "Slot5",
			TFPlayerClass.Engineer => "Slot6",
			TFPlayerClass.Medic => "Slot7",
			TFPlayerClass.Sniper => "Slot8",
			TFPlayerClass.Spy => "Slot9",
			_ => "Slot10"
		};
	}

	[Event.Client.BuildInput]
	public void ProcessClientInput()
	{
		if ( Input.Pressed( GetShortcutButton() ) )
			HandleClick();
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );

		if ( Parent.Parent is not ClassSelection panel )
			return;

		panel.OnSelectedPlayerClass( PlayerClass );
	}

	public void HandleClick()
	{
		Sound.FromScreen( "ui.button.click" );

		if ( PlayerClass == null )
			// Pick a random class
			ConsoleSystem.Run( "tf_join_class", Sandbox.Game.Random.FromList( PlayerClass.All.Select( kv => kv.Key ).ToList() ) );
		else
			ConsoleSystem.Run( "tf_join_class", PlayerClass.ResourceName );

		MenuOverlay.CloseActive();
	}
}
