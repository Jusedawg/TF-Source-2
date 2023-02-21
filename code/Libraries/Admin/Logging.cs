using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	public static partial class Logging
	{
		private static List<ILogger> _serverLoggers = new();
		private static List<ILogger> _clientLoggers = new();

		public interface ILogger
		{
			public bool Server { get; }
			public bool Client { get; }
			public void Log( string message, MessageType type = MessageType.Info );
		}
		public static void RegisterLogger(ILogger logger)
		{
			if ( logger == null ) return;

			if ( logger.Server )
			{
				_serverLoggers.Add( logger );
			}
			if ( logger.Client )
			{
				_clientLoggers.Add( logger );
			}
		}
		[Event.Hotload]
		public static void ResetLoggers()
		{
			_serverLoggers.Clear();
			_clientLoggers.Clear();
		}
		public static void TellAll(string message, MessageType type = MessageType.Info )
		{
			Debug.Log( $"TellAll {message} ({type})" );
			Print( message, type );

			TellClientRPC( message, type );
			foreach ( var logger in _serverLoggers )
			{
				logger.Log( message, type );
			}
		}
		public static void TellClient( IClient client, string message, MessageType type = MessageType.Info )
		{
			Debug.Log($"TellClient {client} {message} ({type})");
			if ( client == null )
			{
				if ( Game.IsDedicatedServer )
					Print( message, type );
				else
					return;
			}

			TellClientRPC( To.Single( client ), message, type );
			foreach(var logger in _serverLoggers)
			{
				logger.Log( message, type );
			}
		}
		public static void TellCaller(string message, MessageType type = MessageType.Info )
		{
			TellClient( Command.Caller, message, type );
		}
		
		[ClientRpc]
		internal static void TellClientRPC( string message, MessageType type )
		{
			Print( message, type );

			foreach(var logger in _clientLoggers)
			{
				logger.Log( message, type );
			}
		}
		public static void Print(object message, MessageType type = MessageType.Info)
		{
			switch ( type )
			{
				case MessageType.Info:
					Info( message );
					break;
				case MessageType.Error:
					Error( message );
					break;
				case MessageType.Announcement:
					// TODO: Make this fancier
					Info( $"[ANNOUNCEMENT] {message}" );
					break;
			}
		}
		public static void Info(object message)
		{
			Log.Info( $"{AddonInformation.PREFIX} {message}" );
		}

		public static void Error(object message)
		{
			Log.Error( $"{AddonInformation.PREFIX} {message}" );
		}

		public static string FormatClients(IEnumerable<IClient> clients)
		{
			int count = clients.Count();
			if ( count == 0 )
				return "Nobody";
			else if ( count == 1 )
				return clients.First().Name;
			else if ( count <= 5 )
				return string.Join( ", ", clients.Select( cl => cl.Name ) );
			else if ( count == Game.Clients.Count() )
				return "Everyone";
			else
				return string.Join( ", ", clients.Take( 5 ).Select( cl => cl.Name ) ) + $" and {count - 5} others";
		}
	}
	public enum MessageType
	{
		Info,
		Error,
		Announcement
	}
}
