using Sandbox.UI;

namespace TFS2.UI;

public class MenuOverlay : Panel
{
	/// <summary>
	/// Current active menu overlay.
	/// </summary>
	public static MenuOverlay CurrentMenu { get; set; }

	public static bool IsActive => CurrentMenu != null;

	public static MenuOverlay Open<T>() where T : MenuOverlay, new()
	{
		var overlay = new T();
		return Open( overlay );
	}

	public static MenuOverlay Open( MenuOverlay overlay )
	{
		CloseActive();

		TFHud.Instance.RootPanel.AddChild( overlay );
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
