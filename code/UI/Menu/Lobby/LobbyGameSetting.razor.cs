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

		if(firstTime)
		{
			ResetValue();
		}

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

	public void ResetValue()
	{
		if(Editor is SliderControl NumberSlider)
		{
			NumberSlider.Value = float.Parse( Value );
		}
		else if (Editor is Checkbox BoolCheckbox)
		{
			BoolCheckbox.Checked = bool.Parse( Value );
		}
		else if(Editor is DropDown ChoiceDropdown)
		{
			ChoiceDropdown.Value = Value;
		}
		else if (Editor is TextEntry StringEntry)
		{
			StringEntry.Value = Value;
		}
	}

	void OnChecked( bool c )
	{
		Set( c ? "true" : "false" );
	}
}
