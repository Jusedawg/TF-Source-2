using Sandbox;
using Sandbox.UI;
using System;
using TFS2;

namespace TFS2.Menu;

public partial class LobbyGameSetting : Panel
{
	public Action<string, string> OnChange { get; set; }

	public string Value { get; set; }
	public GameSetting Setting { get; set; }

	Panel Editor;

	void Set( string newvalue )
	{
		OnChange?.Invoke( Setting.ConVarName, newvalue );
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( Editor is DropDown ChoiceDropdown )
		{
			ChoiceDropdown.Options.Clear();

			foreach ( var choice in Setting.Choices )
			{
				ChoiceDropdown.Options.Add( new( choice.Name, choice.Value ) );
			}

			ChoiceDropdown.Value = Value;
		}
		else if ( Editor is Checkbox BoolCheckbox )
		{
			// For some reason this cant be set in razor so we do it here instead
			BoolCheckbox.ValueChanged = OnChecked;
		}
	}

	void OnChecked( bool c )
	{
		Set( c ? "true" : "false" );
	}
}
