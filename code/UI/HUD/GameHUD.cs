using Sandbox.UI;

namespace TFS2;

[UseTemplate]
public class GameHUD : Panel
{
	public static GameHUD Instance { get; set; }

	public static bool Enabled
	{
		set
		{
			Instance.Style.Set( "display", value ? "flex" : "none" );
		}
	}

	public GameHUD()
	{
		Instance = this;
	}
}
