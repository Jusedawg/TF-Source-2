using TFS2.UI;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TFS2;

internal class ClientSettings
{
	private static List<string> GroupOrder = new List<string>()
	{
		SocialGroup,
		CombatGroup,
		OtherGroup
	};

	public const string SocialGroup = "#GameSettings.Social.Group";
	public const string CombatGroup = "#GameSettings.Combat.Group";
	public const string OtherGroup = "#GameSettings.Other.Group";

	[Display( Name = "#GameSettings.ShowTextChat", Description = "#GameSettings.ShowTextChat.Description", GroupName = SocialGroup )]
	public bool ShowTextChat { get; set; } = true;

	[Display( Name = "#GameSettings.AutoZoomIn", Description = "#GameSettings.AutoZoomIn.Desc", GroupName = CombatGroup )]
	public bool AutoZoomIn { get; set; } = true;

	[Display( Name = "#GameSettings.AutoReload", Description = "#GameSettings.AutoReload.Desc", GroupName = CombatGroup )]
	public bool AutoReload { get; set; } = true;

	private static ClientSettings current;
	public static ClientSettings Current
	{
		get
		{
			if ( current == null )
				current = Cookie.Get<ClientSettings>( "tfs2.clientsettings", new() );
			return current;
		}
	}

	public static int GetGroupOrder( string group )
	{
		int index = GroupOrder.FindIndex( x => x == group );
		if ( index < 0 ) index = int.MaxValue;
		return index;
	}

	public void Save() => Cookie.Set( "tfs2.clientsettings", this );

	public static void Reset()
	{
		current = new();
		current.Save();
	}

}
