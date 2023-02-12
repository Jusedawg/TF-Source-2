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
		public const string CONFIG_LOADED_EVENT = "breaker.config.loaded";
		public const string PLAYER_JOINED_EVENT = "breaker.player.joined";
		public const string PLAYER_LEFT_EVENT = "breaker.player.left";
		public class ConfigLoadedAttribute : EventAttribute
		{
			public ConfigLoadedAttribute() : base( CONFIG_LOADED_EVENT ) { }
		}
		public class PlayerJoinedAttribute : EventAttribute
		{
			public PlayerJoinedAttribute() : base(PLAYER_JOINED_EVENT) {  }
		}
		public class PlayerLeftAttribute : EventAttribute
		{
			public PlayerLeftAttribute() : base(PLAYER_LEFT_EVENT) { }
		}

		static int lastPlayerCount = 0;
		static List<long> joinedPlayers = new();

		[Event.Tick.Server]
		private static void Tick()
		{
			var clients = Game.Clients;
			if ( clients == null || clients.Count() == lastPlayerCount )
				return;

			List<long> currentPlayers = new(Game.Clients.Count);
			foreach(var client in clients)
			{
				currentPlayers.Add( client.SteamId );
				if ( !joinedPlayers.Contains( client.SteamId ) )
				{
					joinedPlayers.Add( client.SteamId );
					Event.Run( PLAYER_JOINED_EVENT, client );
				}
			}

			foreach ( var player in joinedPlayers )
			{
				if ( !currentPlayers.Contains( player ) )
				{
					joinedPlayers.Remove( player );
					Event.Run( PLAYER_LEFT_EVENT, player );
				}
			}
		}
	}
}
