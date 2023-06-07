using Sandbox;
using Sandbox.Localization;
using Sandbox.Menu;
using Sandbox.UI;
using System.Threading.Tasks;
using TFS2.UI;

namespace TFS2.Menu;

public partial class MainPage : Panel
{
	public async Task OnClickCreateGame()
	{
		await Game.Menu.CreateLobbyAsync();
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
