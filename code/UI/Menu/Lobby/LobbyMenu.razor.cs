using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace TFS2.Menu;

public partial class LobbyMenu
{
	enum LobbyPage
	{
		Players,
		Maps,
		Settings,
		Addons
	}

	LobbyPage Mode;

	public void StartGame()
	{
		_ = Game.Menu.EnterServerAsync();
		this.Navigate( "/" );
	}

	protected override int BuildHash()
	{
		var lobby = Game.Menu.Lobby;
		
		return HashCode.Combine( lobby?.Title, lobby?.Owner.Id, lobby?.MemberCount, lobby?.MaxMembers, Game.Menu.Package?.PackageSettings?.Count() );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		if ( Game.Menu.Lobby == null )
		{
			this.Navigate( "/" );
		}
	}

	void OnClickLobby()
	{
		Mode = LobbyPage.Players;
	}

	void OnClickMap()
	{
		Mode = LobbyPage.Maps;
	}

	void OnClickSettings()
	{
		Mode = LobbyPage.Settings;
	}

	void OnClickAddons()
	{
		Mode = LobbyPage.Addons;
	}

	bool CanStartLobby()
	{
		return !string.IsNullOrWhiteSpace( Game.Menu.Lobby.Map );
	}
}
