﻿
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace TFS2.UI;

internal class SettingRow : Panel
{
	public Label Label { get; }
	public Panel ValueArea { get; }

	public SettingRow( object target, PropertyDescription property ) : this()
	{
		Label.Text = property.GetDisplayInfo().Name;

		var typeDesc = TypeLibrary.GetType( property.PropertyType );
		var currentValue = property.GetValue( target );

		if ( property.PropertyType == typeof( bool ) )
		{
			var button = ValueArea.Add.Button( string.Empty, "toggle" );
			button.SetClass( "active", (bool)currentValue );
			if ( button.HasClass( "active" ) ) button.SetText( "done" );
			button.AddEventListener( "onmousedown", () =>
			{
				button.SetClass( "active", !button.HasClass( "active" ) );
				button.SetText( "done" );
				property.SetValue( target, button.HasClass( "active" ) );
				CreateEvent( "save" );
				if ( !button.HasClass( "active" ) ) button.SetText( string.Empty );
			} );
		}

		if ( property.PropertyType == typeof( string ) )
		{
			var textentry = ValueArea.Add.TextEntry( (string)currentValue );
			textentry.AddEventListener( "value.changed", () =>
			{
				property.SetValue( target, textentry.Value );
				CreateEvent( "save" );
			} );
		}

		if ( property.PropertyType.IsEnum )
		{
			var dropdown = new DropDown( ValueArea );
			dropdown.SetPropertyObject( "value", currentValue );
			dropdown.AddEventListener( "value.changed", () =>
			{
				Enum.TryParse( property.PropertyType, $"{dropdown.Value}", out var newval );
				property.SetValue( target, newval );
				CreateEvent( "save" );
			} );
		}

		if ( property.PropertyType == typeof( float ) )
		{
			var minmax = property.GetCustomAttribute<MinMaxAttribute>();
			var min = minmax?.MinValue ?? 0f;
			var max = minmax?.MaxValue ?? 100f;
			var step = property.GetCustomAttribute<SliderStepAttribute>()?.Step ?? .1f;
			var slider = ValueArea.Add.SliderWithEntry( min, max, step );
			slider.Bind( "value", target, property.Name );
			slider.AddEventListener( "value.changed", () =>
			{
				CreateEvent( "save" );
			} );
		}

		if ( property.PropertyType == typeof( int ) )
		{
			var minmax = property.GetCustomAttribute<MinMaxAttribute>();
			var min = minmax?.MinValue ?? 0;
			var max = minmax?.MaxValue ?? 100;
			var step = property.GetCustomAttribute<SliderStepAttribute>()?.Step ?? 1;
			var slider = ValueArea.Add.SliderWithEntry( min, max, step );
			slider.Bind( "value", target, property.Name );
			slider.AddEventListener( "value.changed", () =>
			{
				CreateEvent( "save" );
			} );
		}
	}

	public SettingRow()
	{
		Label = Add.Label( "Label" );
		Add.Panel().Style.FlexGrow = 1;
		ValueArea = Add.Panel( "value-area" );
	}

}
