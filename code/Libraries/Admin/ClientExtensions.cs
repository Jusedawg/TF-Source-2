using Breaker;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
	public static class ClientExtensions
    {
		public static bool HasPermission( this IClient client, string permission )
		{
			return Permission.Has( client, permission );
		}
		public static void ExecuteCommand(this IClient client, string command, string[] args)
		{
			Command.Execute( command, client, args );
		}
		public static bool CanTarget(this IClient client, User other, string command = "")
		{
			var user = User.Get( client );
			if ( user == null )
			{
				return false;
			}
			return user.CanTarget( other, command );
		}
		public static bool CanTarget(this IClient client, UserGroup group, string command = "")
		{
			var user = User.Get( client );
			if ( user == null )
			{
				return false;
			}
			return user.CanTarget( group, command );
		}
	}
}
