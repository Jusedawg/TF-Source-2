using Sandbox;
using Sandbox.UI;
using Amper.FPS;
using System.Collections.Generic;
using System;

namespace TFS2.UI;

public partial class Health : Panel
{
	private Panel PlayerClassIcon { get; set; }

	private const float HealthUpdateDuration = 1.5f;
	private readonly Queue<(TimeSince time, Label panel)> _healthUpdates = new();

	public Health()
	{
		BindClass( "red", () => TFPlayer.LocalPlayer.Team == TFTeam.Red );
		BindClass( "blue", () => TFPlayer.LocalPlayer.Team == TFTeam.Blue );

		EventDispatcher.Subscribe<PlayerRegenerateEvent>( OnRegenerate, this );
		EventDispatcher.Subscribe<PlayerHealthKitPickUpEvent>( OnHealthKitPickUp, this );

		SetupClassPreview();
	}

	public override void Tick()
	{
		if ( !TFPlayer.LocalPlayer.IsValid() )
			return;

		SetClass( "hidden", !ShouldDraw() );

		while (_healthUpdates.TryPeek( out var update ) && update.time >= HealthUpdateDuration )
		{
			update.panel.Delete();
			_healthUpdates.Dequeue();
		}
	}

	public bool ShouldDraw()
	{
		var player = TFPlayer.LocalPlayer;
		if ( !player.IsValid() )
			return false;

		if ( Input.Down( "score" ) )
			return false;

		return player.IsAlive;
	}

	public void OnRegenerate( PlayerRegenerateEvent args )
	{
		if ( args.Client != Game.LocalClient )
			return;

		while ( _healthUpdates.TryDequeue( out var update) )
		{
			update.panel.Delete();
		}

		SetupClassPreview();
	}

	public void SetupClassPreview()
	{
		var player = TFPlayer.LocalPlayer;
		if ( player == null )
			return;

		var playerClass = player.PlayerClass;
		if ( playerClass.IsValid() )
		{
			var classIcon = player.Team == TFTeam.Blue
				? playerClass.IconBlue
				: playerClass.IconRed;

			PlayerClassIcon?.Style.SetBackgroundImage( Util.JPGToPNG( classIcon ) );
		}
	}

	public void OnHealthKitPickUp( PlayerHealthKitPickUpEvent @event )
	{
		var health = (int)Math.Floor( @event.Health );
		var label = new Label { Text = $"+{health}" };

		label.SetClass( "health_update", true );
		label.Style.AnimationIterationCount = 1;
		label.Style.AnimationDuration = HealthUpdateDuration;

		AddChild( label );

		_healthUpdates.Enqueue( ((TimeSince)0, label) );
	}
}
