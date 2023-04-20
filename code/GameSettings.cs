using TFS2.UI;

namespace TFS2;

internal class ClientSettings
{

	[Display( Name = "#GameSettings.ShowChat", Description = "#GameSettings.ShowChat.Description" )]
	public bool ShowChat { get; set; } = true;


	public void Save() => Cookie.Set( "tfs2.clientsettings", this );

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

	public static void Reset()
	{
		current = new();
		current.Save();
	}

}
