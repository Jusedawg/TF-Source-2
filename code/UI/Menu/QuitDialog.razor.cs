using Sandbox;
using Sandbox.UI;

namespace TFS2.Menu;

/// <summary>
/// Popup dialog box asking the player if they want to quit i.e. MessagePanel.cs
/// </summary>
public partial class QuitDialog : MenuOverlay
{
	public void OnClickCancel()
	{
		Close();
	}

	public void OnClickQuit()
	{
		Game.Menu.LeaveServer( "Disconnect" );
		Close();
	}
}
