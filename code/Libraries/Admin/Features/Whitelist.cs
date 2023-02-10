using Breaker;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	[Category("Whitelist")]
	public static class Whitelist
	{
		#region Persistent Whitelist
		private const string WHITELIST_FILE = "whitelist.json";
		
		private static List<long> whitelistedPlayers = new();
		private static BaseFileSystem fs => Config.Instance.GetFileSystem();
		[BRKEvent.ConfigLoaded]
		public static void Load()
		{
			Debug.Log( $"Loading whitelist from {fs.GetFullPath(WHITELIST_FILE)}..." );
			if ( !fs.FileExists( WHITELIST_FILE ) )
				fs.WriteAllText( WHITELIST_FILE, "[]" );

			whitelistedPlayers = fs.ReadJsonOrDefault<List<long>>( WHITELIST_FILE );
			Debug.Log( $"Loaded {whitelistedPlayers.Count} whitelisted players." );
		}

		public static void Save()
		{
			Debug.Log( $"Saving whitelist to {fs.GetFullPath( WHITELIST_FILE )}..." );
			fs.WriteJson( WHITELIST_FILE, whitelistedPlayers );
			Debug.Log( $"Saved {whitelistedPlayers.Count} whitelisted players." );
		}
		#endregion

		[Command("whitelist", "wl"), Permission("breaker.whitelist.manage")]
		private static void Manage([Title("add/remove")] string action, IEnumerable<IClient> clients)
		{
			switch ( action.ToLower() )
			{
				case "add":
					foreach ( IClient cl in clients )
						Add( cl );
					return;
				case "remove":
					foreach ( IClient cl in clients )
						Remove( cl );
					return;
				default:
					Logging.Error( $"Invalid action '{action}'! Valid actions are 'add' and 'remove'." );
					return;
			}
		}
		[Command( "whitelistoffline", "wloffline" ), Permission( "breaker.whitelist.manage" )]
		private static void ManageOffline( [Title( "add/remove" )] string action, long user )
		{
			switch ( action.ToLower() )
			{
				case "add":
					Add( user );
					return;
				case "remove":
					Remove( user );
					return;
				default:
					Logging.Error( $"Invalid action '{action}'! Valid actions are 'add' and 'remove'." );
					return;
			}
		}
		public static void Add(long id)
		{
			if ( !whitelistedPlayers.Contains( id ) )
			{
				Log.Info( $"Added {id} to whitelist" );
				whitelistedPlayers.Add( id );
				Save();
			}
		}
		public static void Add(IClient cl)
		{
			Add( cl.SteamId );
		}
		public static void Remove( long id )
		{
			if ( whitelistedPlayers.Contains( id ) )
			{
				Log.Info( $"Removing {id} from whitelist" );
				whitelistedPlayers.Remove( id );
				Save();
			}
		}
		public static void Remove( IClient cl )
		{
			Remove( cl.SteamId );
		}
		private const string WHITELIST_PERMISSION = "breaker.whitelist.bypass";
		[Permission( WHITELIST_PERMISSION )]
		public static bool IsWhitelisted( long id )
		{
			if ( !User.Exists( id ) )
				return false;

			User user = User.All[id];
			if ( user != null && Permission.Has( user, WHITELIST_PERMISSION ) )
				return true;
			return whitelistedPlayers.Contains( id );
		}
		public static bool IsWhitelisted( IClient cl)
		{
			return cl.IsListenServerHost || IsWhitelisted( cl.SteamId );
		}

		[BRKEvent.PlayerNumberChanged]
		private static void CheckPlayers()
		{
			if(!Console.Vars.breaker_whitelist_enabled)
				return;

			foreach (var client in Game.Clients)
			{
				if ( !IsWhitelisted( client ) )
				{
					Debug.Log( $"User {client.Name} is not whitelisted, Kicking..." );
					client.Kick();
				}
			}
		}
	}
}
