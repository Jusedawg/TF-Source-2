using Sandbox;
using Sandbox.Localization;
using Sandbox.Menu;
using Sandbox.UI;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TFS2.UI;

namespace TFS2.Menu;

public partial class MainPage : Panel
{
	const string LATEST_BLOG_ENDPOINT = "https://tfsource2.com/api/news/getlatest";

	const string UNKNOWN_BLOG_TITLE = "Official Blogposts";
	const string UNKNOWN_BLOG_DESCRIPTION = "Follow the latest Team Fortress: Source 2 news & updates on our blog!";
	const string UNKNOWN_BLOG_URL = "https://tfsource2.com/news";
	const string UNKNOWN_BLOG_IMAGE = "https://imgur.com/GSuUbHG.png";
	BlogInfo latestBlog;
	JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

	ServerList List;
	public MainPage()
	{
		BindClass( "ingame", () => Game.InGame );

		_ = GetLatestBlog();
	}

	private async Task GetLatestBlog()
	{
		var response = await Http.RequestStringAsync( LATEST_BLOG_ENDPOINT );
		latestBlog = JsonSerializer.Deserialize<BlogInfo>( response, options );
		StateHasChanged();
	}

	public void OnClickResumeGame()
	{
		Game.Menu.HideMenu();
	}
	public async Task OnClickCreateGame()
	{
		var maxPlayers = Game.Menu.Package.GetMeta<int>( "MaxPlayers", 1 ); ;
		await Game.Menu.CreateLobbyAsync( maxPlayers);
	}
	public void OnClickViewLobby()
	{
		this.Navigate( "/lobby" );
	}
	public void OnClickLoadout()
	{
		this.Navigate( "/loadout" );
	}
	public void OnClickJoinGame()
	{
		List.SetClass( "visible", !List.HasClass( "visible" ) );
	}
	public void OnClickSettings()
	{
		this.Navigate( "/settings" );
	}

	public void OnClickQuit()
	{
		if(Game.InGame)
		{
			MenuOverlay.Open<QuitDialog>();
		}
		else
		{
			Game.Menu.Close();
		}
	}

	public void OnClickClassSelection()
	{
		if ( !Game.InGame ) return;

		HudOverlay.Open<ClassSelection>();
	}

	public void OnClickTeamSelection()
	{
		if ( !Game.InGame ) return;

		HudOverlay.Open<TeamSelection>();
	}

	public void OnClickBlog()
	{
		var blog = MenuOverlay.Open<BlogView>();
		blog.Url = GetBlogURL();
	}

	public string GetBlogTitle() => latestBlog?.Title ?? UNKNOWN_BLOG_TITLE;
	public string GetBlogDescription() => latestBlog?.Description ?? UNKNOWN_BLOG_DESCRIPTION;
	public string GetBlogURL() => latestBlog?.URL ?? UNKNOWN_BLOG_URL;
	public string GetBlogThumbnail() => latestBlog?.Thumbnail ?? UNKNOWN_BLOG_IMAGE;

	public void OnClickCredits()
	{
		MenuOverlay.Open<CreditsView>();
	}
}

public class BlogInfo
{
	public string Name { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public string URL { get; set; }
	[JsonPropertyName( "thumb" )] public string Thumbnail { get; set; }
}
