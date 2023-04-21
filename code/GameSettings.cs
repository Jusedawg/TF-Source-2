using TFS2.UI;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TFS2;

internal class ClientSettings
{
	private static List<string> GroupOrder = new List<string>()
	{
		ChatGroup,
		OtherGroup
	};

    public const string ChatGroup = "Chat";
	public const string OtherGroup = "Other";

    [Display( Name = "#GameSettings.ShowTextChat", Description = "#GameSettings.ShowTextChat.Description", GroupName = ChatGroup )]
	public bool ShowTextChat { get; set; } = true;

    [Display(Name = "#GameSettings.FooBar", Description = "#GameSettings.FooBar.Desc", GroupName = OtherGroup)]
    public bool FooBar { get; set; } = true;

    private static ClientSettings current;
	public static ClientSettings Current
	{
		get
		{
			if( current == null ) 
				current = Cookie.Get<ClientSettings>( "tfs2.clientsettings", new() );
			return current;
		}
	}

	public static int GetGroupOrder(string group)
	{
		int index = GroupOrder.FindIndex(x => x == group);
		if (index < 0) index = int.MaxValue;
		return index;
	}

    public void Save() => Cookie.Set("tfs2.clientsettings", this);

    public static void Reset()
	{
		current = new();
		current.Save();
	}

}
