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
	Texture ColorTexture;

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

	public void SetupFreeze( Entity target, float time, Vector3 position, Rotation rotation, float fov )
	{
		var size = new Vector2( Screen.Width, Screen.Height );

		ColorTexture?.Dispose();

		ColorTexture = Texture.CreateRenderTarget()
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

		Graphics.RenderToTexture( FreezeCam, ColorTexture );

		TimeSinceFrozen = 0;
		FreezeTime = time;
		
		Style.SetBackgroundImage( ColorTexture );
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
