using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

public partial class FreezeCameraPanel : Panel
{
	public static FreezeCameraPanel Instance { get; set; }
	public float FreezeTime { get; set; }
	public TimeSince TimeSinceFrozen { get; set; }
	public bool IsFrozen => TimeSinceFrozen < FreezeTime;
	public bool WillFreeze { get; set; }

	SceneCamera FreezeCam;
	Texture BackgroundTexture;

	Panel KillerPanel;
	HealthCross KillerHealth;
	Label KillerHeader;
	Image KillerAvatar;
	Label KillerName;

	public FreezeCameraPanel()
	{
		Instance = this;
	}

	public override void Tick()
	{
		SetClass( "visible", ShouldDraw() );
	}

	public static void Freeze( Entity target, float time, Vector3 position, Rotation rotation, float fov )
	{
		Instance?.SetupFreeze( target, time, position, rotation, fov );
	}

	const string KILLER_HEADER = "#FreezeCamera.KillerHeader";
	const string KILLER_HEADER_DEAD = "#FreezeCamera.KillerHeaderDead";
	const string KILLER_OTHER_NAME = "#FreezeCamera.NonPlayerName";
	public void SetupFreeze( Entity target, float time, Vector3 position, Rotation rotation, float fov )
	{
		var size = new Vector2( Screen.Width, Screen.Height );

		// Display killer data on screen
		if(target is TFPlayer ply)
		{
			KillerHealth.HealthAmount = ply.Health;
			KillerHealth.MaxHealthAmount = ply.MaxHealth;
			if ( !ply.IsAlive )
				KillerHeader.Text = KILLER_HEADER_DEAD;
			else
				KillerHeader.Text = KILLER_HEADER;
			KillerAvatar.SetTexture( $"avatar:{ply.Client.SteamId}" );
			KillerName.Text = ply.Client.Name;

			KillerHealth.SetClass( "visible", true );
			KillerAvatar.SetClass( "visible", true );

			KillerPanel.SetClass( "blu", ply.Team == TFTeam.Blue );
			KillerPanel.SetClass( "red", ply.Team == TFTeam.Red );
			KillerPanel.SetClass( "other", !ply.Team.IsPlayable() );
		}
		else
		{
			KillerHeader.Text = KILLER_HEADER;
			KillerName.Text = KILLER_OTHER_NAME;

			KillerHealth.SetClass( "visible", false );
			KillerAvatar.SetClass( "visible", false );

			KillerPanel.SetClass( "blu", false );
			KillerPanel.SetClass( "red", false );
			KillerPanel.SetClass( "other", true );
		}

		BackgroundTexture?.Dispose();
		BackgroundTexture = Texture.CreateRenderTarget()
			.WithSize( size )
			.WithFormat( ImageFormat.RGBA32323232F )
			.WithScreenMultiSample()
			.Create();

		FreezeCam = new SceneCamera( "FreezeCam" );
		FreezeCam.World = Game.SceneWorld;
		FreezeCam.Position = position;
		FreezeCam.Rotation = rotation;
		FreezeCam.FieldOfView = fov;

		// Settings from regular camera
		var currentCam = Camera.Current ?? Camera.Main;
		FreezeCam.ZFar = currentCam.ZFar;
		FreezeCam.EnablePostProcessing = true;
		// TODO: Copy important render attributes from main camera

		Graphics.RenderToTexture( FreezeCam, BackgroundTexture );

		TimeSinceFrozen = 0;
		FreezeTime = time;
		
		Style.SetBackgroundImage( BackgroundTexture );
		WillFreeze = true;
	}

	public bool ShouldDraw()
	{
		var currentCam = Camera.Current ?? Camera.Main;
		if(currentCam.FirstPersonViewer != null)
		{
			// If we are already alive, stop drawing
			return false;
		}

		return IsFrozen;
	}
}
