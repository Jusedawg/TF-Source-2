using Sandbox.UI;

namespace TFS2;

public partial class VoiceMenuPage : Panel
{
	public bool Shown { get; set; }

	public void Show()
	{
		if ( Shown )
			return;

		AddClass( "visible" );
		Shown = true;
	}

	public void Hide()
	{
		if ( !Shown )
			return;

		RemoveClass( "visible" );
		Shown = false;
	}

	public void AddElement( string label )
	{
	}
}
