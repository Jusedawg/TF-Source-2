using Sandbox;
using Sandbox.UI;
using System;

namespace TFS2.UI;

public partial class ItemsPage : MenuOverlay
{
	public void OnClickBack()
	{
		Close();
	}

	public void OnClickClassButton( string className )
	{
		if ( !Enum.TryParse<TFPlayerClass>( className, out var item ) )
			return;

		var playerClass = PlayerClass.Get( item );
		if ( playerClass == null )
			return;

		Open( new ClassLoadout( playerClass ) );
	}
}
