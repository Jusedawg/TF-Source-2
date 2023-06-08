using Sandbox;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amper.FPS;

namespace TFS2;

public delegate void LoadoutAvailableDelegate( bool success );

partial class Loadout
{
	public event LoadoutAvailableDelegate LoadoutAvailable;

	/// <summary>
	/// We received loadout data from the client.
	/// </summary>
	/// <param name="data"></param>
	[ConCmd.Server( "send_loadout" )]
	public static void OnClientTransmit( string data )
	{
		Game.AssertServer();

		var client = ConsoleSystem.Caller;
		if ( client == null )
			return;

		data = data.Decompress();

		var loadout = ForClient( client );

		// if what data that we recieved is not valid.
		if ( string.IsNullOrEmpty( data ) )
		{
			loadout.State = LoadoutState.Failed;
			loadout.LoadoutAvailable?.Invoke( false );
			return;
		}

		// deserialize loadout data.
		var obj = JsonSerializer.Deserialize<LoadoutData>( data );

		loadout.Data = obj;

		loadout.LoadoutAvailable?.Invoke( false );
		loadout.LoadoutAvailable = null;

		loadout.OnUpdated();
	}

	/// <summary>
	/// Request loadout information from client.
	/// </summary>
	public void RequestDataFromClient( Action<bool> callback )
	{
		Game.AssertServer();

		LoadoutAvailable += ( success ) => callback( success );
		RequestDataFromClient();
	}

	/// <summary>
	/// Request loadout information from client.
	/// </summary>
	public void RequestDataFromClient()
	{
		Game.AssertServer();
		OnServerRequest( To.Single( Game.Clients.FirstOrDefault(cl => cl.SteamId == Client) ) );
	}
}
