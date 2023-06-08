using Sandbox;
using Sandbox.UI;
using System;

namespace TFS2.Menu;

public partial class LoadoutSelection : Panel
{
	public void OnClickBack()
	{
		this.Navigate( "/" );
	}

	public void OnClickClassButton( string className )
	{
		this.Navigate( $"/loadout/class/{className}" );
	}
}
