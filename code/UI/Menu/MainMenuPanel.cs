using Sandbox;
using Sandbox.Menu;
using Sandbox.UI;
using TFS2.UI;

namespace TFS2.Menu;

public partial class MainMenuPanel : Panel, IGameMenuPanel
{
	Label PlayerName { get; set; }
	Image PlayerAvatar { get; set; }

	public override void Tick()
	{
		if ( !IsVisible )
			return;

		if ( Game.InGame )
			TickGame();
		else
			TickMenu();
	}

	private void TickGame()
	{
		PlayerName.Text = Sandbox.Game.LocalClient.Name;
		PlayerAvatar.SetTexture( $"avatarbig:{Sandbox.Game.LocalClient.SteamId}" );
	}

	private void TickMenu()
	{

	}

	public void OnClickResumeGame()
	{
		// implement
	}
	public void OnClickJoinGame()
	{
		MenuOverlay.Open<JoinGameDialog>();
	}

	public void OnClickSettings()
	{
		MenuOverlay.Open<SettingsMenu>();
	}

	public void OnClickLoadout()
	{
		MenuOverlay.Open<ClassLoadout>();
	}

	public void OnClickQuit()
	{
		MenuOverlay.Open<QuitDialog>();
	}

	public void OnClickClassSelection()
	{
		MenuOverlay.Open<ClassSelection>();
	}

	public void OnClickTeamSelection()
	{
		MenuOverlay.Open<TeamSelection>();
	}

	public void OnClickBlog()
	{
		MenuOverlay.Open<BlogView>();
	}
}
