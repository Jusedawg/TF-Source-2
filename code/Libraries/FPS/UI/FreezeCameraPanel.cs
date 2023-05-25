using Sandbox;
using Sandbox.UI;

namespace Amper.FPS;

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
		var draw = ShouldDraw();
		SetClass( "visible", draw );
		if ( !draw && FreezeCam != null )
			FreezeCam = null;
	}

	public static void Freeze( float time, Vector3 position, Rotation rotation, float fov )
	{
		Instance?.SetupFreeze( time, position, rotation, fov );
	}

	public void SetupFreeze( float time, Vector3 position, Rotation rotation, float fov )
	{
		var size = new Vector2( Screen.Width, Screen.Height );

		ColorTexture?.Dispose();

		ColorTexture = Texture.CreateRenderTarget()
			.WithSize( size )
			.WithFormat( ImageFormat.RGBA32323232F )
			.WithScreenMultiSample()
			.Create();

		var currentCam = Camera.Current ?? Camera.Main;
		FreezeCam = new SceneCamera( "FreezeCam" );
		FreezeCam.World = Game.SceneWorld;
		FreezeCam.Position = position;
		FreezeCam.Rotation = rotation;
		FreezeCam.FieldOfView = fov;
		FreezeCam.ZFar = currentCam.ZFar;

		Graphics.RenderToTexture( FreezeCam, ColorTexture );

		TimeSinceFrozen = 0;
		FreezeTime = time;
		
		Style.SetBackgroundImage( ColorTexture );
		WillFreeze = true;
	}

	public bool ShouldDraw()
	{
		return IsFrozen;
	}

	/*
	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );
		return;
		if ( WillFreeze )
		{
			// Fill the texture with background
			Rect rect = new( 0, 0, Size.x, Size.y );
			//Graphics.DrawQuad(rect, Material.UI.Basic)
			//Render.Draw.DrawScene( ColorTexture, DepthTexture, Map.Scene, Render.Attributes, , Position, Rotation, FieldOfView, 0.1f, 9999, false);
			WillFreeze = false;
		}
	}
	*/
}
