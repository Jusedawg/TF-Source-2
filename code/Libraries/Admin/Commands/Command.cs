using Breaker;
using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Breaker
{
	public static partial class Command
	{
		public struct ContextInfo
		{
			public IClient Caller;
			public float CallTick;
			public DateTime CallTime;

			public ContextInfo( IClient caller )
			{
				Caller = caller;
				CallTick = Time.Tick;
				CallTime = DateTime.Now;
			}
		}
		public class CommandInfo
		{
			public string Name = "";
			public string Group = "";
			public CommandAttribute Attribute;
			public MethodDescription Method;

			public CommandInfo( CommandAttribute attribute, MethodDescription method )
			{
				Attribute = attribute;
				Method = method;
			}

			public ParameterInfo[] GetParameters() => Method.Parameters;
		}
		public class CommandClientInfo
		{
			public string Key;
			public string Name;
			public string Group;
			public string[] Parameters;

			public CommandClientInfo( string key, string name, string group = "", IEnumerable<string> parameters = default )
			{
				Key = key;
				Name = name;
				Group = group;
				Parameters = parameters?.ToArray();
			}

			public static implicit operator CommandClientInfo( CommandInfo info )
			{
				return new CommandClientInfo( info.Attribute.Key, info.Name, info.Group, info.GetParameters().Select(p => p.Name) );
			}
		}
		private static Dictionary<string, CommandInfo> commands = new();
		private static Dictionary<string, CommandInfo> aliasToCommand = new();
		private static List<CommandClientInfo> clientCommands = new();
		/// <summary>
		/// All current commands. Only usable on the server.
		/// </summary>
		public static IReadOnlyDictionary<string, CommandInfo> All => commands.AsReadOnly();
		public static ContextInfo Context { get; private set; }
		public static IClient Caller => Context.Caller;
		/// <summary>
		/// Deletes the current list of commands and generates a new one.
		/// </summary>
		//[Event.Hotload]
		//[Event.Entity.PostSpawn]
		[BRKEvent.ConfigLoaded]
		public static void LoadAll()
		{
			commands.Clear();
			aliasToCommand.Clear();
			NetworkCommandClear();

			var types = TypeLibrary.GetTypes();
			foreach ( var type in types )
			{
				foreach ( var method in type.Methods )
				{
					if ( !method.IsStatic || method.IsProperty )
						continue;

					var attribute = method.GetCustomAttribute<CommandAttribute>();
					if ( attribute == null )
						continue;
					
					var name = attribute.Key;

					string group = null;
					var groupMethodAttrib = method.GetCustomAttribute<CategoryAttribute>();
					var groupTypeAttrib = type.GetAttribute<CategoryAttribute>();
					if ( groupMethodAttrib != null )
						group = groupMethodAttrib.Value;
					else if(groupTypeAttrib != null )
						group = groupTypeAttrib.Value;

					var title = name;
					var titleAttrib = method.GetCustomAttribute<TitleAttribute>();
					if (titleAttrib != null)
					{
						title = titleAttrib.Value;
					}

					CommandInfo info = new( attribute, method ) { Name = title, Group = group };
					if ( commands.ContainsKey( name ) )
					{
						Logging.Error( $"Tried to register command with duplicate name {name}!" );
						continue;
					}

					foreach(var alias in attribute.Aliases)
					{
						if(commands.ContainsKey(alias))
						{
							Logging.Error( $"Tried to register command with alias {alias} which already exists as a command!" );
						}
						else if ( aliasToCommand.ContainsKey( alias ) )
						{
							Logging.Error( $"Tried to register command alias {alias} which already exists!" );
							continue;
						}

						Debug.Log( $"Alias for {name}: {alias}" );
						aliasToCommand.Add( alias, info );
					}

					Debug.Log( $"Registering command {name}." );
					commands.Add( name, info );
					NetworkCommandInfo( name, title, group, method.Parameters.Select(p => p.Name).ToArray() );
				}
			}
		}

		#region Networking
		[BRKEvent.PlayerJoined]
		static void OnClientJoin(IClient client)
		{
			foreach(var kv in commands)
			{
				var cmd = kv.Value;
				NetworkCommandInfo( To.Single( client ), kv.Key, cmd.Name, cmd.Group, cmd.GetParameters().Select(p => p.Name).ToArray() );
			}
			
		}
		[ClientRpc]
		public static void NetworkCommandInfo(string key, string name, string group = "", string[] parameters = default )
		{
			CommandClientInfo info = new( key, name, group, parameters );
			clientCommands.Add( info );
		}
		[ClientRpc]
		public static void NetworkCommandClear()
		{
			clientCommands.Clear();
		}
		#endregion

		public static ParameterInfo[] Parameters( string name )
		{
			if(!commands.TryGetValue(name, out var cmd))
			{
				Log.Error( $"Tried to get parameters of command {name} which does not exist!" );
				return default;
			}

			return cmd.Method.Parameters;
		}
		public static PermissionAttribute[] Permissions(string name)
		{
			if ( !commands.TryGetValue( name, out var cmd ) )
			{
				Log.Error( $"Tried to get required permissions of command {name} which does not exist!" );
				return default;
			}

			return cmd.Method.Attributes.OfType<PermissionAttribute>().ToArray();
		}

		public static IEnumerable<CommandClientInfo> GetAllClient()
		{
			return clientCommands;
		}

		public static IEnumerable<IGrouping<string, CommandClientInfo>> GetAllClientGrouped()
		{
			return clientCommands.GroupBy( info => info.Group );
		}
	}
	[AttributeUsage( AttributeTargets.Method )]
	public class CommandAttribute : Attribute
	{
		public string Key { get; set; }
		public string[] Aliases { get; }
		public string Description { get; set; }
		public CommandAttribute(string key, params string[] aliases)
		{
			if ( string.IsNullOrEmpty( key ) )
				throw new ArgumentNullException( "name" );

			Key = key;
			Aliases = aliases;
		}
	}
}
