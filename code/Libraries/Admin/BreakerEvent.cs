using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	public static class BRKEvent
	{
		public class ConfigLoadedAttribute : EventAttribute
		{
			public ConfigLoadedAttribute() : base( "breaker.config.loaded" ) { }
		}
		public class PlayerNumberChangedAttribute : EventAttribute
		{
			// TODO: This is stupid, get a way to only check for this if a client joined
			public PlayerNumberChangedAttribute() : base( "breaker.players.changed" ) { }
		}
		public class PlayerJoinedAttribute : EventAttribute
		{
			public PlayerJoinedAttribute() : base("breaker.player.joined") {  }
		}
		public class PlayerLeftAttribute : EventAttribute
		{
			public PlayerLeftAttribute() : base("breaker.player.left") { }
		}

		static int lastPlayerCount = 0;
		static List<long> joinedPlayers = new();
		[Event.Tick.Server]
		private static void Tick()
		{
			var clients = Game.Clients;
			if ( clients == null || clients.Count() == lastPlayerCount )
				return;

			List<long> currentPlayers = new();
			foreach(var client in clients)
			{
				currentPlayers.Add( client.SteamId );
				if ( !joinedPlayers.Contains( client.SteamId ) )
				{
					joinedPlayers.Add( client.SteamId );
					Event.Run( "Breaker.player.joined", client );
				}
			}

			foreach ( var player in joinedPlayers )
			{
				if ( !currentPlayers.Contains( player ) )
				{
					joinedPlayers.Remove( player );
					Event.Run( "Breaker.player.left", player );
				}
			}

			lastPlayerCount = clients.Count();
			Event.Run( "Breaker.players.changed" );
		}
	}
}
