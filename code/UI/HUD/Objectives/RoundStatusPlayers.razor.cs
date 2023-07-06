using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS2.UI;

public partial class RoundStatusPlayers : Panel
{
	public TFTeam Team { get; set; }
	public Dictionary<IClient, RoundStatusPlayersEntry> Players { get; set; } = new();

	public RoundStatusPlayers()
	{
		BindClass( "red", () => Team == TFTeam.Red );
		BindClass( "blue", () => Team == TFTeam.Blue );
	}

	public override void Tick()
	{
		var teamClients = Sandbox.Game.Clients.Where( x => x.GetTeam() == Team );
		var keyClients = Players.Keys;

		foreach ( var client in teamClients.Except( keyClients ) ) AddClient( client );
		foreach ( var client in keyClients.Except( teamClients ) ) RemoveClient( client );
	}

	public void AddClient( IClient client )
	{
		Players[client] = new RoundStatusPlayersEntry
		{
			Client = client,
			Parent = this
		};
	}

	public void RemoveClient( IClient client )
	{
		if ( Players.TryGetValue( client, out var entry ) )
		{
			entry?.Delete( true );
			Players.Remove( client );
		}
	}
}
