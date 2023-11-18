using Sandbox;
using Sandbox.UI;
using Amper.FPS;
using System;
using System.Linq;

namespace TFS2.UI;

public partial class Health : Panel
{
	private Panel PlayerClassIcon { get; set; }

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

		var healthUpdates = ChildrenOfType<HealthUpdateLabel>().ToList();
		foreach ( var healthUpdate in healthUpdates )
		{
			healthUpdate.Delete();
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

	public void OnHealthKitPickUp( PlayerHealthKitPickUpEvent ev )
	{
		var health = (int)Math.Floor( ev.Health );
		var label = new HealthUpdateLabel( health );

		AddChild( label );
	}
}

public class HealthUpdateLabel : Label
{
	private const float HEALTH_UPDATE_DURATION = 1.5f;

	public HealthUpdateLabel( int health )
	{
		Text = $"+{health}";
		SetClass( "health_update", true );

		Style.AnimationDuration = HEALTH_UPDATE_DURATION;
		DeleteAsync();
	}

	public async void DeleteAsync()
	{
		await GameTask.DelaySeconds( HEALTH_UPDATE_DURATION );
		Delete();
	}
}
