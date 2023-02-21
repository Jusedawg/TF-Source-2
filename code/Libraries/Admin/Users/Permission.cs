using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Breaker
{
	public static class Permission
	{
		public static bool Has( IClient cl, string permission )
		{
			Debug.Log( $"Checking permission {permission} for client {cl}" );

			if ( Game.IsDedicatedServer && cl == default || cl.IsListenServerHost )
			{
				Debug.Log( "Command was called by host, always allowing..." );
				return true;
			}

			Debug.Log( $"Checking after exceptions..." );
			return Has( User.Get( cl ), permission );
		}
		public static bool Has( User user, string permission )
		{
			var permissions = user.GetPermissions();

			foreach(var wildcard in user.GetPermissions().Where(p => p.EndsWith(".*")))
			{
				if ( CheckWildcard( wildcard, permission ) )
					return true;
			}

			return permissions.Contains( permission );
		}

		private static bool CheckWildcard( string wildcard, string permission )
		{
			var wildcardParts = wildcard.Split( '.' );
			var permissionParts = permission.Split( '.' );
			if ( wildcardParts.Length > permissionParts.Length )
				return false;

			// Check if the generic permission matches the permission we are looking for
			for ( int i = 0; i < wildcardParts.Length; i++ )
			{
				if ( wildcardParts[i] != permissionParts[i] )
					break;

				if ( i == wildcardParts.Length - 1 )
					return true;
			}

			return false;
		}
	}
	[AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
	public class PermissionAttribute : Attribute
	{
		public string Permission { get; private set; }
		/// <summary>
		/// Dont check for this permission when executing the command
		/// </summary>
		public bool ManualEnforcement { get; init; }
		public PermissionAttribute( string permission, bool manualEnforcement = false )
		{
			Permission = permission;
			ManualEnforcement = manualEnforcement;
		}

		public static implicit operator string( PermissionAttribute attr ) => attr.Permission;
	}
}
