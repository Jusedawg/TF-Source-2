using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker.Commands
{
	[Category("User Management")]
	public static class UserManagement
	{
		[Command("getusers", "printusers")]
		public static void PrintUsers()
		{
			Logging.TellCaller( "Groups:" );
			foreach (var cl in Game.Clients)
			{
				var user = User.Get( cl );
				Logging.TellCaller($"{cl.Name} | Groups: ({string.Join(",", user.UserGroups)})" );
			}
		}

		[Command("usergroup", "ugroup"),Permission("breaker.user.group")]
		public static void ManageGroup([Title("add/remove")] string action, IClient target, string group)
		{
			var user = User.Get( target );
			switch(action)
			{
				case "add":
					AddGroup( user, group );
					Logging.TellCaller( $"Added client {target.Name} to group {group}" );
					Logging.TellClient( target, $"You were added to group {group}!" );
					break;
				case "remove":
					RemoveGroup( user, group );
					Logging.TellCaller( $"Removed client {target.Name} from group {group}" );
					Logging.TellClient( target, $"You were removed from group {group}!" );
					break;
				default:
					Logging.TellCaller( $"Invalid action {action}!", MessageType.Error );
					break;
			}
		}

		private static void AddGroup(User user, string group)
		{
			if ( UserGroup.Exists( group ) )
			{
				if ( user.UserGroups.Contains( group ) )
				{
					Logging.TellCaller($"Client is already in group {group}!", MessageType.Error);
					return;
				}
				user.UserGroups.Add( group );
				User.Update( user );
			}
			else
			{
				Logging.TellCaller( $"Group {group} does not exist!", MessageType.Error );
			}
		}

		private static void RemoveGroup(User user, string group)
		{
			if ( user.UserGroups.Contains( group ) )
			{
				user.UserGroups.Remove( group );
				User.Update( user );
			}
			else
			{
				Logging.TellCaller($"Client is not in group {group}!", MessageType.Error);
			}
		}
	}
}
