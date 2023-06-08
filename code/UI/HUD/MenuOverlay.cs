using Sandbox;
using Sandbox.UI;

namespace TFS2.UI;

public class HudOverlay : Panel
{
	/// <summary>
	/// Current active menu overlay.
	/// </summary>
	public static HudOverlay CurrentMenu { get; set; }

	public static bool IsActive => CurrentMenu != null;

	public static HudOverlay Open<T>() where T : HudOverlay, new()
	{
		var overlay = new T();
		return Open( overlay );
	}

	public static HudOverlay Open( HudOverlay overlay )
	{
		CloseActive();

		TFHud.Instance?.RootPanel?.AddChild( overlay );
		CurrentMenu = overlay;
		return CurrentMenu;
	}

	public static void CloseActive()
	{
		CurrentMenu?.Close();
	}

	public void Close()
	{
		if ( CurrentMenu == this )
			CurrentMenu = null;
		
		Delete();
	}
}
