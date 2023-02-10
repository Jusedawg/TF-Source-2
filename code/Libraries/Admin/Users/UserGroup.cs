using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breaker
{
    public class UserGroup
    {
		#region Persistent User Groups
		const string GROUP_FILE = "groups.json";
		private static BaseFileSystem fs => Config.Instance.GetFileSystem();
		private static Dictionary<string, UserGroup> groups = new();
		public static IReadOnlyDictionary<string,UserGroup> All => groups.AsReadOnly();
		private static readonly Dictionary<string, UserGroup> defaultGroups = new()
		{
			{ "user", new() {Id = "user", Weight = 0 } },
			{ "admin", new() {Id = "admin", Weight = 100} }
		};
		
		[BRKEvent.ConfigLoaded]
		public static void Load()
		{
			Debug.Log( $"Reading groups from {fs.GetFullPath( GROUP_FILE )}..." );
			groups = fs.ReadJsonOrDefault<Dictionary<string,UserGroup>>( GROUP_FILE );
			if(groups == null)
			{
				groups = defaultGroups;
				Save();
			}
			
			Debug.Log( $"Loaded data of {groups.Count} groups." );
		}

		public static void Save()
		{
			if ( groups.Count == 0 )
			{
				Debug.Log( $"No groups exist, creating default groups..." );
				groups = defaultGroups;
			}

			fs.WriteJson( GROUP_FILE, groups );
			Debug.Log( $"Saved {groups.Count} groups to {fs.GetFullPath( GROUP_FILE )}" );
		}

		public static void Add(UserGroup group)
		{
			if(groups.ContainsKey(group.Id))
			{
				Logging.Error( $"Tried to register group with duplicate id {group.Id}!" );
				return;
			}

			groups.Add( group.Id, group );

			Save();
		}

		public static void Remove(string id)
		{
			if ( !groups.ContainsKey( id ) )
			{
				Logging.Error( $"Tried to remove group with id {id} which doesnt exist!" );
				return;
			}

			groups.Remove( id );

			Save();
		}

		public static void Remove( UserGroup group ) => Remove( group.Id );
		public static void Update(UserGroup group)
		{
			if ( !groups.ContainsKey( group.Id ) )
			{
				Logging.Error( $"Tried to update group with id {group.Id} which doesnt exist!" );
				return;
			}

			groups[group.Id] = group;

			Save();
		}
		public static bool Exists( string id ) => groups.ContainsKey( id );
		#endregion
		public static UserGroup GetDefault()
		{
			if ( !string.IsNullOrEmpty( Config.Instance?.DefaultUserGroup ) )
			{
				if ( UserGroup.All.TryGetValue( Config.Instance.DefaultUserGroup, out var group ) )
					return group;
			}

			return UserGroup.All.Select( kv => kv.Value ).OrderBy( g => g.Weight ).First();
		}

		public string Id { get; set; }
		
		#region Settings
		public string Name { get; set; }
		public Color NameColor { get; set; }
		public string Prefix { get; set; }
		public Color PrefixColor { get; set; }
		/// <summary>
		/// The icon to use for this group. This is a URL or Path to an image.
		/// </summary>
		public string Icon { get; set; }
		#endregion Settings

		/// <summary>
		/// Decides what users may be targeted by members of this group.
		/// Users can always target other Users with a weight lower then their own.
		/// </summary>
		public int Weight { get; set; }
		public List<string> Permissions { get; set; } = new();
		public UserGroup()
		{
			// Required for JSON
		}

		public UserGroup( string id, int weight, List<string> permissions )
		{
			Id = id;
			Weight = weight;
			Permissions = permissions;
		}

		public bool CanTarget( UserGroup target, string command = "" )
		{
			return this == target || target.Weight < Weight;
		}
	}
}
